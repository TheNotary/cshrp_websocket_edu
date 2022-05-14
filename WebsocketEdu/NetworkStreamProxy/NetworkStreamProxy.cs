using System.Net.Sockets;
using System.Text;



namespace WebsocketEdu
{
    public class NetworkStreamProxy : AbstractNetworkStreamProxy
    {
        private readonly NetworkStream _networkStream;
        private MemoryStream _readLog;
        //private readonly MemoryStream _writeStream;

        public NetworkStreamProxy(NetworkStream networkStream)
        {
            _networkStream = networkStream;
            //_writeStream = new MemoryStream();
            _readLog = new MemoryStream();
        }

        public override bool DataAvailable => _networkStream.DataAvailable;
        public override Stream SourceStream => (Stream)_networkStream;
        public override Stream WriteStream => (Stream)_networkStream;

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

        public override string GetWritesAsString()
        {
            throw new NotImplementedException();
        }

    }
}
