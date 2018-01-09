using System;
using System.Collections.Generic;
using System.Text;

namespace HSocket.Server
{ 
    public delegate void TextReceivedEventHandler<T>(T client, string data);
    public delegate void BinaryReceivedEventHandler<T>(T client, byte[] data);
    public delegate void ConnectionClosedEventHandler<T>(T client); 
    public delegate void StateChangedEventHandler<T>(T client, State state);
    public delegate void OnConnectedEventHandler<TConnection>(TConnection client);
    public delegate bool OnHandshakeEventHandler(Dictionary<string, string> headers);
    public delegate void ServerStateChangedEventHandler<TConnection>(SocketServer<TConnection> server, State state) where TConnection : Connection<TConnection>, new();
}
