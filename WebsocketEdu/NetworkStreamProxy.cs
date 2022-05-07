using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebsocketEdu
{
    public class NetworkStreamProxy : INetworkStream
    {
        private readonly NetworkStream _networkStream;
        public bool DataAvailable => _networkStream.DataAvailable;
        public Stream Stream => (Stream) _networkStream;

        public NetworkStreamProxy(NetworkStream networkStream)
        {
            _networkStream = networkStream;
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            _networkStream.Read(buffer, offset, count);
        }

        public int ReadByte()
        {
            return _networkStream.ReadByte();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _networkStream.Write(buffer, offset, count);
        }

        public void WriteByte(byte value)
        {
            _networkStream.WriteByte(value);
        }
    }
}
