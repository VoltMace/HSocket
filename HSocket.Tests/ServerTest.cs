using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HSocket.Tests
{
    [TestClass]
    public class ServerTest
    {
        [TestMethod]
        public void BaseServerTest()
        {
            ClientWebSocket socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("SessionId", "Custom header");
            socket.Options.SetRequestHeader("ClientGuid", "1");
            socket.ConnectAsync(new Uri("ws://localhost:8953"), CancellationToken.None).Wait();

            int size = 40000;
            string str = RandomString(size);
            List<byte> ar = Encoding.UTF8.GetBytes(str).ToList();
            List<List<byte>> bytes =  ToChunks(ar, 10000).ToList();
            for(int i=0;i<3;i++) 
                socket.SendAsync(new ArraySegment<byte>(bytes[i].ToArray()), WebSocketMessageType.Text, false, CancellationToken.None).GetAwaiter().GetResult();
            socket.SendAsync(new ArraySegment<byte>(bytes[3].ToArray()), WebSocketMessageType.Text, true, CancellationToken.None).GetAwaiter().GetResult();

            MemoryStream ms = new MemoryStream(); WebSocketReceiveResult result;
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[10000]);
            do
            {
                result = socket.ReceiveAsync(buffer, CancellationToken.None).Result;
                ms.Write(buffer.Array, buffer.Offset, result.Count);

            } while (!result.EndOfMessage);
            ms.Seek(0, SeekOrigin.Begin);
            string s = Encoding.UTF8.GetString(ms.ToArray());
            int count = s.Length;
            Console.WriteLine("Received " + count);

             //socket.CloseAsync(WebSocketCloseStatus.NormalClosure,null,CancellationToken.None).Wait();
        }

        public   string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public     IEnumerable<List<T>> ToChunks<T>(  IEnumerable<T> items, int chunkSize)
        {
            List<T> chunk = new List<T>(chunkSize);
            foreach (var item in items)
            {
                chunk.Add(item);
                if (chunk.Count == chunkSize)
                {
                    yield return chunk;
                    chunk = new List<T>(chunkSize);
                }
            }
            if (chunk.Any())
                yield return chunk;
        }
    }
}
