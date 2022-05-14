using System.Net.Sockets;
using System.Text;



namespace WebsocketEdu
{
    public class NetworkStreamProxy : INetworkStream
    {
        private readonly NetworkStream _networkStream;
        private MemoryStream _readLog;
        private readonly MemoryStream _writeStream;


        public NetworkStreamProxy(NetworkStream networkStream)
        {
            _networkStream = networkStream;
            _writeStream = new MemoryStream();
            _readLog = new MemoryStream();
        }

        public bool DataAvailable => _networkStream.DataAvailable;
        public Stream SourceStream => (Stream)_networkStream;


        public void Read(byte[] buffer, int offset, int count)
        {
            SourceStream.Read(buffer, offset, count);
            _readLog.Write(buffer, offset, count);
        }

        public int ReadByte()
        {
            int newByte = (int) _networkStream.ReadByte();
            _readLog.WriteByte((byte) newByte);
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

        public string PrintBytesRecieved()
        {
            StringBuilder sb = new StringBuilder();
            byte[] bytes = _readLog.ToArray();
            for (int i = 0; i < bytes.Length; i++) {
                sb.Append(bytes[i].ToString() + " ");
            }
            return sb.ToString();
        }

        public void ClearDebugBuffer()
        {
            _readLog.Close();
            _readLog = new MemoryStream();
        }

        public string GetWritesAsString()
        {
            return Encoding.UTF8.GetString(_writeStream.ToArray());
        }

    }
}
