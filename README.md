# HSocket
Create socket hub and set port and event and start server

            SocketHub server = new SocketHub(8953);
            Console.WriteLine("Server start on 8953");

            server.onHandshake += Server_onHandshake;
            server.OnConnected += Server_OnConnected;

            server.OnBinaryReceived += Server_OnBinaryReceived;
            server.OnTextReceived += Server_OnTextReceived;
            

            server.OnStateChange += Server_OnStateChange;
            server.OnClose += Server_OnClose;

            server.Start();
            
Or if u want more setting create you custum hub and connection with inheritance

    class CustomConnection : Connection<CustomConnection>
    {
          //login and any custom data
    }
    class CustomServer : SocketServer<CustomConnection>
    { 
        protected override void onMessageReceived(CustomConnection client, string message)
        {
            
        }
    }
 
 this library NOT USE win api socket and can work with old OS version
