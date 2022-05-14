using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace WebsocketEdu
{
    public abstract class AbstractNetworkStreamProxy : INetworkStream
    {
        private readonly NetworkStream? _networkStream;
        //private MemoryStream? _readLog;
        private readonly MemoryStream? _writeStream;

        public abstract bool DataAvailable { get; }
        public abstract Stream SourceStream { get; }
        public abstract Stream WriteStream { get; }
        public abstract MemoryStream ReadLog { get; set; }
        public void Read(byte[] buffer, int offset, int count)
        {
            SourceStream.Read(buffer, offset, count);
            ReadLog.Write(buffer, offset, count);
        }
        public int ReadByte()
        {
            int thisByte = SourceStream.ReadByte();
            ReadLog.WriteByte((byte)thisByte);
            return thisByte;
        }
        public void Write(byte[] buffer, int offset, int count)
        {
            WriteStream.Write(buffer, offset, count);
        }
        public void WriteByte(byte value)
        {
            WriteStream.WriteByte(value);
        }
        public void ClearDebugBuffer()
        {
            ReadLog.Close();
            ReadLog = new MemoryStream();
        }
        public abstract string GetWritesAsString();

        public abstract string PrintBytesRecieved();
    }
}
