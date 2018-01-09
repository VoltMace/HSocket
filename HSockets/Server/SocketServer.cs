using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace HSocket.Server
{ 
    public class SocketServer<TConnection> where TConnection : Connection<TConnection>,new()
    {
        public SocketServer(int port) : this(IPAddress.Any, port)
        {   }

        public SocketServer(IPAddress ip, int port)
        {
            _server = new TcpListener(ip, port);
            _connections = new ConcurrentDictionary<string, TConnection>();
        }

  
        private State _state = State.Closed;         

        private bool _retryOnConnectionBusy = true;
        private TcpListener _server;
        private ConcurrentDictionary<string, TConnection> _connections;


        public State State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                onServerStateChange(_state); 
            }
        }

        public IEnumerable<TConnection> Connections
        {
            get
            { 
                return _connections.Values.AsEnumerable();
            }
        }
        /// <summary>
        /// If connection is busy attempt to resend until message is sent. Changing this setting only affects new connections.
        /// </summary>
        public bool RetryOnConnectionBusy
        {
            get { return _retryOnConnectionBusy; }
            set
            {
                _retryOnConnectionBusy = value;
            }
        }

        protected virtual void onClientConnect(TConnection client)
        { }

        protected virtual void onServerStateChange(State state)
        { }

        protected virtual void onMessageReceived(TConnection client, string message)
        { }

        protected virtual void onBinaryReceived(TConnection client, byte[] data)
        { }

        protected virtual void onClientDisconnect(TConnection client)
        {

        }

        protected virtual void onClientStateChanged(TConnection client, State state)
        {
             
        }

        protected virtual bool OnHandshake(Dictionary<string,string> headers)
        {
            return true;
        }

        public void Start()
        {
            _server.Start();
            State = State.Open;
            Task.Run(() => Listen());
        }
        public void Broadcast(string message)
        {
            foreach (var conn in Connections)
                if (conn.State == State.Open)
                    conn.Send(message);
        }
        /// <summary>
        /// Closes the server.
        /// </summary>
        /// <param name="hardquit">Set true to abandon all connections instead of performing a clean exit.</param>
        public void Close(bool hardquit)
        {
            State = State.Closing;
            foreach (var conn in Connections)
                conn.Close(hardquit);

            if (hardquit)
                Shutdown();
        }

        public TConnection Connection(string ID)
        {
            TConnection con;
            return _connections.TryGetValue(ID, out con) ? con : null;
        } 

        #region Private methods
        private void Shutdown()
        {
            _server.Stop();
            _connections.Clear();
            State = State.Closed;
        }
        private async void Listen()
        { 
            while(State == State.Open)
            {
                var conn = await _server.AcceptTcpClientAsync(); 
                Task.Run(() => HandleConnection(conn));
            } 
        }
        private void HandleConnection(TcpClient conn)
        {
            try
            {
                _handshake(conn);

                string id = Guid.NewGuid().ToString();
                while (_connections.ContainsKey(id))
                    id = Guid.NewGuid().ToString();          
                TConnection socket = new TConnection();
                socket.RetryOnConnectionBusy = RetryOnConnectionBusy;
                socket.Init(conn, id); 
                socket.Closed += Socket_Closed;
                socket.MessageReceived += onMessageReceived;
                socket.BinaryReceived += onBinaryReceived;
                socket.StateChanged += onClientStateChanged;
                _connections.TryAdd(socket.ID, socket);
                onClientConnect(socket);
            } catch (Exception) { }
        }



        private void Socket_Closed(TConnection source)
        {
            onClientDisconnect(source);
             _connections.TryRemove(source.ID,out  source);
            if (State == State.Closing && _connections.Count == 0)
                Shutdown();
        }


        private void _handshake(TcpClient conn)
        {
            var stream = conn.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (line.StartsWith("Sec-WebSocket-Key"))
                    dict.Add("Key", line.Split(':')[1].Trim());
                if (line.StartsWith("GET"))
                    dict.Add("Protocol", line.Split(' ')[1].Trim().Substring(1));
                else
                {
                    int index=line.IndexOf(':');
                    string key = line.Remove(index);
                    string value = line.Remove(0,key.Length+1)?.Trim();
                    dict.Add(key, value);
                }
            
            } 
            if (!dict.ContainsKey("Key"))
                throw new NotImplementedException("Failed to receive the key necessary to upgrade the connection.");
            string acceptKey = Convert.ToBase64String(
                                    SHA1.Create().ComputeHash(
                                        Encoding.UTF8.GetBytes(
                                            dict["Key"] + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                        )
                                    )
                                );
            byte[] bytes;
            if(OnHandshake(dict))
            {
                string response = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine;
                response += "Upgrade: websocket" + Environment.NewLine;
                response += "Connection: Upgrade" + Environment.NewLine;
                response += "Sec-WebSocket-Accept: " + acceptKey + Environment.NewLine;
                if (dict.ContainsKey("Protocol") && !string.IsNullOrEmpty(dict["Protocol"]))
                    response += "Sec-WebSocket-Protocol: " + dict["Protocol"] + Environment.NewLine;
                response += Environment.NewLine;
                bytes = Encoding.UTF8.GetBytes(response); 
            }
            else
                bytes = Encoding.UTF8.GetBytes("Error handshake");
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }
        #endregion
    }



}
