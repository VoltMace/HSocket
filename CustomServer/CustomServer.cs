using HSocket.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomServer
{
    class CustomServer : SocketServer<CustomConnection>
    {
        public CustomServer(int port) : base(port)
        {
        }

        //Create your server and set in teamplate you session or you can set default
        //and add some logic for you server

        protected override void onBinaryReceived(CustomConnection client, byte[] data)
        {
           
        }

        /// <summary>
        /// Rise when client just create socket
        /// </summary>
        /// <param name="client"></param>
        protected override void onClientConnect(CustomConnection client)
        {
            Broadcast("client with " + client.ID + " has connected");   //send to all client
        }

        protected override void onClientDisconnect(CustomConnection client)
        {
           
        }

        protected override void onClientStateChanged(CustomConnection client, State state)
        {
          
        }

        /// <summary>
        /// Validate create socket or not
        /// </summary>
        /// <param name="headers">Client headers</param>
        /// <returns></returns>
        protected override bool OnHandshake(Dictionary<string, string> headers)
        {
            return true;
        }

        protected override void onMessageReceived(CustomConnection client, string message)
        {
            
        }

        protected override void onServerStateChange(State state)
        {
            
        }
    }
}
