using System;
using System.IO;
using System.Text;
using WebsocketEdu;

namespace WebsocketEduTest
{
    public class MockNetworkStreamProxy : AbstractNetworkStreamProxy
    {
        private readonly FeedableMemoryStream _networkStream;
        private MemoryStream _readLog;
        private readonly MemoryStream _writeStream;
        

        public MockNetworkStreamProxy(FeedableMemoryStream fms)
        {
            _networkStream = fms;
            _writeStream = new MemoryStream();
            _readLog = new MemoryStream();
        }
        public MockNetworkStreamProxy()
        {
            _networkStream = new FeedableMemoryStream();
            _writeStream = new MemoryStream();
            _readLog = new MemoryStream();
        }

        public MockNetworkStreamProxy(string testString)
        {
            _networkStream = new FeedableMemoryStream(testString);
            _writeStream = new MemoryStream();
            _readLog = new MemoryStream();
        }

        public override bool DataAvailable
        {
            get
            { // FIXME: check for off by one
                return SourceStream.Position < SourceStream.Length;
            }
        }

        public override Stream SourceStream => (Stream)_networkStream;
        public override MemoryStream WriteStream => _writeStream;
        public override MemoryStream ReadLog
        {
            get
            {
                return _readLog;
            }
            set
            {
                _readLog = value;
            }
        }

        public void PutByte(byte value)
        {
            _networkStream.PutByte(value);
        }

        public void PutBytes(byte[] bytes)
        {
            _networkStream.PutBytes(bytes, 0, bytes.Length);
        }
    }
}
