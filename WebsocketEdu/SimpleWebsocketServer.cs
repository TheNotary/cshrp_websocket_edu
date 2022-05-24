using System.Net;
using System.Net.Sockets;

namespace WebsocketEdu
{
    internal class SimpleWebsocketServer
    {
        private string localAddress;
        private int port;
        private int threadPoolSize;
        private TcpListener? server;
        private LinkedList<Thread> threads = new LinkedList<Thread>();

        public SimpleWebsocketServer(string localAddress, int port) : this(localAddress, port, 20) { }

        public SimpleWebsocketServer(string localAddress, int port, int threadPoolSize)
        {
            this.localAddress = localAddress;
            this.port = port;
            this.threadPoolSize = threadPoolSize;
        }

        public void Start()
        {
            server = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:{0}.{1}Waiting for a connection...", port, Environment.NewLine);
            ThreadManagementLoop();
            throw new NotImplementedException();
        }

        public void ThreadManagementLoop()
        {
            ChannelBridge channelBridge = new ChannelBridge();
            while (true)
            {
                if (threads.Count < threadPoolSize)
                {
                    //GC.Collect(); // For testing memory leaks
                    Thread t = new Thread(new ParameterizedThreadStart(TcpController.HandleNewClientConnectionInThread));
                    object[] threadParams = new object[] { server, channelBridge };
                    t.Start(threadParams);
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