using HSocket.Hub;
using HSocket.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomServer
{
    class CustomConnection : Connection<CustomConnection>
    {
        //Add you session data here

        //example login
        public string Login;
    }
}
