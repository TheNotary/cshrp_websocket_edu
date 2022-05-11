using System.Net.Sockets;

namespace WebsocketEdu
{
    public class NetworkStreamProxy : INetworkStream
    {
        private MemoryStream readLog;
        private readonly NetworkStream _networkStream;
        public bool DataAvailable => _networkStream.DataAvailable;
        public Stream Stream => (Stream) _networkStream;
        public NetworkStreamProxy(NetworkStream networkStream)
        {
            _networkStream = networkStream;
            readLog = new MemoryStream();
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            _networkStream.Read(buffer, offset, count);
            readLog.Write(buffer, offset, count);
        }

        public int ReadByte()
        {
            int newByte = (int) _networkStream.ReadByte();
            readLog.WriteByte((byte) newByte);
            return newByte;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _networkStream.Write(buffer, offset, count);
        }

        public void WriteByte(byte value)
        {
            _networkStream.WriteByte(value);
        }

        public void PrintBytesRecieved()
        {
            //readLog.Seek(0, SeekOrigin.Begin);
            Console.WriteLine("Bytes in Frame were:");
            byte[] bytes = readLog.ToArray();
            for (int i = 0; i < bytes.Length; i++) {
                Console.Write(bytes[i].ToString() + " ");
            }
            Console.WriteLine();
        }

        public void ClearDebugBuffer()
        {
            readLog.Close();
            readLog = new MemoryStream();
        }

    }
}
