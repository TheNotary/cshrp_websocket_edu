using Xunit;
using WebsocketEdu;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using Xunit.Abstractions;
using System.IO.Pipes;
using Microsoft.Win32.SafeHandles;

namespace WebsocketEduTest
{
    public class WaysOfTransmittingDataAcrossThreadsTest
    {
        private readonly ITestOutputHelper output;

        public WaysOfTransmittingDataAcrossThreadsTest(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        public void ItCanUseTcpStreamsToHandleThisDuplexIssue()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8881);
            server.Start();

            Thread t = new Thread(new ParameterizedThreadStart(ListenToClientAndLogData));
            t.Start(server);

            TcpClient client = new TcpClient("127.0.0.1", 8881);

            Socket socket = client.Client;
            socket.Send(new byte[] { 0b00000001 });
            socket.Send(new byte[] { 0b00000010 });
            socket.Send(new byte[] { 0b00000011 });
            socket.Send(new byte[] { 0b11111111 }); // this byte tells the client to close down
            client.Client.Close();

            t.Join();

            Assert.True(true);
        }

        private void ListenToClientAndLogData(object? svr)
        {
            if (svr == null) throw new ArgumentNullException(nameof(svr));
            TcpListener server = (TcpListener) svr;
            TcpClient client = server.AcceptTcpClient();

            Socket socket = client.Client;
            NetworkStream stream = client.GetStream();

            while (!client.Connected) ;
            while (client.Connected)
            {
                while (!stream.DataAvailable) ; // block here till we have data
                int myByte = stream.ReadByte();
                output.WriteLine(((int) myByte).ToString());
                if (myByte == 255) client.Close(); // 6 is our magic disconnect byte
            }
        }

        [Fact]
        public void ItCanUseFeedableMemoryStreams()
        {
            FeedableMemoryStream fms = new FeedableMemoryStream();

            Thread t = new Thread(new ParameterizedThreadStart(ListenToFeedableMemoryStream));
            t.Start(fms);

            fms.PutByte(1);
            fms.PutByte(2);
            fms.PutByte(3);
            fms.PutByte(255);

            t.Join();
        }

        private void ListenToFeedableMemoryStream(object? strm)
        {
            if (strm == null) throw new ArgumentNullException(nameof(strm));
            Stream stream = (Stream)strm;

            while (true)
            {
                if (stream.Position < stream.Length)
                {
                    int myByte = stream.ReadByte();
                    if (myByte == 255) break; // A 255 byte can signal the end of the stream
                    output.WriteLine("Data Recieved: " + myByte);
                }
            }
        }

        [Fact]
        public void ItCanUsePipedOutputStreams()
        {
            AnonymousPipeServerStream pipeServer = 
                new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            
            Stream clientPipe = new AnonymousPipeClientStream(PipeDirection.In, pipeServer.GetClientHandleAsString());
            Thread t = new Thread(new ParameterizedThreadStart(ListenToPipe));
            t.Start(clientPipe);

            pipeServer.WriteByte(1);
            pipeServer.WriteByte(2);
            pipeServer.WriteByte(3);
            pipeServer.WriteByte(255);

            t.Join();
        }

        private void ListenToPipe(object? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            //string pipeHandle = (string) obj;

            Stream pipe = (Stream) obj;

            while (true)
            {
                int myByte = pipe.ReadByte();
                if (myByte == 255) break; // A 255 byte can signal the end of the stream
                output.WriteLine("Data Recieved: " + myByte);
            }
        }

    }
}