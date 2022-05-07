using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WebsocketEdu;

namespace WebsocketEduTest
{
    public class MockNetworkStreamProxy : INetworkStream
    {
        private readonly Stream _stream;
        private readonly FeedableMemoryStream _feedableMemoryStream;
        private readonly MemoryStream _writeStream;

        public MockNetworkStreamProxy()
        {
            _feedableMemoryStream = new FeedableMemoryStream();
            _stream = _feedableMemoryStream;
            _writeStream = new MemoryStream();
        }

        public bool DataAvailable => throw new NotImplementedException();

        public Stream Stream => _stream;

        public byte[] GetWrites()
        {
            return _writeStream.ToArray();
        }

        public string GetWritesAsString()
        {
            return Encoding.UTF8.GetString(_writeStream.ToArray());
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            _feedableMemoryStream.Read(buffer, offset, count);
        }

        public int ReadByte()
        {
            return (int) _feedableMemoryStream.ReadByte();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _writeStream.Write(buffer, offset, count);
        }

        public void WriteByte(byte value)
        {
            _writeStream.WriteByte(value);
        }
    }
}
