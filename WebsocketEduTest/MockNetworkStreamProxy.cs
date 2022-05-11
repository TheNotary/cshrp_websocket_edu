using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
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

        public MockNetworkStreamProxy(FeedableMemoryStream fms)
        {
            _feedableMemoryStream = fms;
            _stream = _feedableMemoryStream;
            _writeStream = new MemoryStream();
        }

        public MockNetworkStreamProxy(string testString)
        {
            _feedableMemoryStream = new FeedableMemoryStream();
            byte[] buffer = Encoding.ASCII.GetBytes(testString);
            _feedableMemoryStream.Write(buffer, 0, buffer.Length);
            _feedableMemoryStream.Seek(0, SeekOrigin.Begin);

            _stream = _feedableMemoryStream;
            _writeStream = new MemoryStream();
        }

        public static explicit operator Stream(MockNetworkStreamProxy v)
        {
            return v.Stream;
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

        /* No need to implement since the bytes read are already known to the parent test
         * */
        public void ClearDebugBuffer()
        {
            throw new NotImplementedException();
        }

        /* No need to implement since the bytes read are already known to the parent test
         * */
        public void PrintBytesRecieved()
        {
            throw new NotImplementedException();
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

        public void PutByte(byte value)
        {
            _feedableMemoryStream.PutByte(value);
        }

        public void PutBytes(byte[] bytes)
        {
            _feedableMemoryStream.PutBytes(bytes, 0, bytes.Length);
        }

    }
}
