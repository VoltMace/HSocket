using System;

namespace CustomServer
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomServer server = new CustomServer(8000);
            server.Start();
            Console.WriteLine("Server start on 8000");
            Console.ReadKey();
            server.Close(false);
        }
    }
}
