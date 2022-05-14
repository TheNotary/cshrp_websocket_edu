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
        private readonly MemoryStream readLog;

        public MockNetworkStreamProxy()
        {
            _feedableMemoryStream = new FeedableMemoryStream();
            _stream = _feedableMemoryStream;
            _writeStream = new MemoryStream();
            readLog = new MemoryStream();
        }

        public MockNetworkStreamProxy(FeedableMemoryStream fms)
        {
            _feedableMemoryStream = fms;
            _stream = _feedableMemoryStream;
            _writeStream = new MemoryStream();
            readLog = new MemoryStream();
        }

        public MockNetworkStreamProxy(string testString)
        {
            _feedableMemoryStream = new FeedableMemoryStream(testString);
            _stream = _feedableMemoryStream;
            _writeStream = new MemoryStream();
            readLog = new MemoryStream();
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

        /* This message return, as a string, all the characters that were written to the _writeStream 
         * which simulates what this client would have written to the tcp stream.
         * */
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

        public string PrintBytesRecieved()
        {
            StringBuilder sb = new StringBuilder();  // TODO: Abstract class...
            byte[] bytes = readLog.ToArray();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString() + " ");
            }
            return sb.ToString();
        }

        public byte[] GetBytesRecieved()
        {
            return readLog.ToArray();
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            _feedableMemoryStream.Read(buffer, offset, count);
            readLog.Write(buffer, offset, count);
        }

        public int ReadByte()
        {
            int thisByte = _feedableMemoryStream.ReadByte();
            readLog.WriteByte((byte)thisByte);
            return thisByte;
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

        /* Converts the internal _feedableMemoryStream ToArray.  
         * */
        public byte[] ToArray()
        {
            return _feedableMemoryStream.ToArray();
        }


    }
}
