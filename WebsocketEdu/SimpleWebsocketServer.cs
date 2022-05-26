﻿using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace WebsocketEdu
{
    internal class SimpleWebsocketServer
    {
        const int defaultThreadPoolSize = 20;
        private string localAddress;
        private int port;
        private int threadPoolSize;
        public string adminPassword;
        private TcpListener? server;
        private LinkedList<Thread> threads = new LinkedList<Thread>();

        public SimpleWebsocketServer(string localAddress, int port) : this(localAddress, port, defaultThreadPoolSize, GenerateRandomPassword()) { }

        public SimpleWebsocketServer(string localAddress, int port, int threadPoolSize, string adminPassword)
        {
            this.localAddress = localAddress;
            this.port = port;
            this.threadPoolSize = threadPoolSize;
            this.adminPassword = adminPassword;
        }

        public void Start()
        {
            server = new TcpListener(IPAddress.Parse(localAddress), port);
            server.Start();
            Console.WriteLine("Server has started on {2}:{0}.{1}Waiting for a connection...", port, Environment.NewLine, localAddress);

            ThreadManagementLoop();
        }

        public void ThreadManagementLoop()
        {
            ChannelBridge channelBridge = new ChannelBridge(adminPassword);
            CancellationTokenSource managementCts = new CancellationTokenSource();
            channelBridge.ManagementCancelationToken = managementCts;
            var token = managementCts.Token;

            while (!token.IsCancellationRequested)
            {
                if (threads.Count < threadPoolSize)
                {
                    //GC.Collect(); // For testing memory leaks
                    Thread t = new Thread(new ParameterizedThreadStart(TcpController.HandleNewClientConnectionInThread));
                    if (server == null) throw new Exception("Server was null which isn't possible");

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

        public static string GenerateRandomPassword()
        {
            int tokenLength = 10;

            char[] charSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&()".ToCharArray();
            int byteSize = 256; //Labelling convenience
            int biasZone = byteSize - (byteSize % charSet.Length);

            byte[] rBytes = new byte[tokenLength]; //Do as much before and after lock as possible
            char[] rName = new char[tokenLength];

            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(rBytes);
            for (var i = 0; i < tokenLength; i++)
            {
                rName[i] = charSet[rBytes[i] % charSet.Length];
            }

            return new string(rName);
        }
    }
}