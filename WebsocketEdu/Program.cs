using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;

namespace WebsocketEdu
{
    /// <summary>
    /// This application is a basic example of a working websocket server implementation.  
    /// </summary>
    public class WebsocketEduApp
    {
        private static int threadPoolSize = 2;
        private static int port = 80;
        private static TcpListener? server;
        private static LinkedList<Thread> threads = new LinkedList<Thread>();

        static void Main(string[] args)
        {
            SimpleWebsocketServer simpleWebsocketServer = new SimpleWebsocketServer("0.0.0.0", 80, 2);
            simpleWebsocketServer.Start();
        }

    }

}
