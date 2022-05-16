using System.Net;
using System.Net.Sockets;

namespace WebsocketEdu
{
    /// <summary>
    /// This application is a basic example of a working websocket server implementation.  
    /// </summary>
    public class WebsocketEduApp
    {
        private static int threadPoolSize = 1;
        private static int port = 80;
        private static TcpListener? server;
        private static LinkedList<Thread> threads = new LinkedList<Thread>();

        static void Main(string[] args)
        {
            server = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:{0}.{1}Waiting for a connection...", port, Environment.NewLine);
            ThreadManagementLoop();
        }

        public static void ThreadManagementLoop()
        {
            while (true)
            {
                if (threads.Count < threadPoolSize)
                {
                    //GC.Collect(); // For testing memory leaks
                    Thread t = new Thread(new ParameterizedThreadStart(TcpController.HandleNewClientConnectionInThread));
                    t.Start(server);
                    threads.AddLast(t);
                    Console.WriteLine("Started new thread, threadcount at " + threads.Count);
                }
                // FIXME: that weird bug where the console doesn't update is probably because I'm using this while:true loop and not explicitly managing updates
                Thread.Sleep(500);

                // Clean up dead threads
                if (threads.First == null) continue;
                LinkedListNode<Thread> node = threads.First;
                for (int i = 0; i < threads.Count; i++)
                {
                    Thread t = node.Value;
                    if (!t.IsAlive)
                        threads.Remove(t);
                    if (node.Next == null) break;
                    node = node.Next;
                }
            }
        }

    }

}
