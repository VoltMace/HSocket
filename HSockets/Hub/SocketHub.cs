using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using HSocket.Server;

namespace HSocket.Hub
{
    public class SocketHub : SocketServer<Connection>
    {
        public SocketHub(int port) : base(port)
        {   }

        public SocketHub(IPAddress ip, int port) : base(ip, port)
        {   }

        public event TextReceivedEventHandler<Connection> OnTextReceived;
        public event BinaryReceivedEventHandler<Connection> OnBinaryReceived;
        public event ConnectionClosedEventHandler<Connection> OnClose;
        public event StateChangedEventHandler<Connection> OnStateChange;
        public event OnConnectedEventHandler<Connection> OnConnected;
        public event ServerStateChangedEventHandler<Connection> OnStateChanged;
        public event OnHandshakeEventHandler onHandshake;

        protected override void onClientStateChanged(Connection client, State state)
        {
            OnStateChange?.Invoke(client, state);
        }

        protected override bool OnHandshake(Dictionary<string, string> headers)
        {
            bool? result = onHandshake?.Invoke(headers);
            return result.HasValue ? result.Value : true; 
            
        }

        protected override void onBinaryReceived(Connection client, byte[] data)
        {
            OnBinaryReceived?.Invoke(client, data);
        }
         

        protected override void onMessageReceived(Connection client, string message)
        {
            OnTextReceived?.Invoke(client, message);
        }

        protected override void onClientConnect(Connection client)
        {
            OnConnected?.Invoke(client);
        }

        protected override void onClientDisconnect(Connection client)
        {
            OnClose?.Invoke(client);
        }

        protected override void onServerStateChange(State state)
        {
            OnStateChanged?.Invoke(this, state);
        }




    }
}
