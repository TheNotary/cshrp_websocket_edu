using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebsocketEdu
{
    public abstract class AbstractNetworkStreamProxy : INetworkStream
    {
        
        public abstract bool DataAvailable { get; }

        public Stream Stream => throw new NotImplementedException();

        public abstract void ClearDebugBuffer();
        public abstract string GetWritesAsString();
        public abstract string PrintBytesRecieved();
        public abstract void Read(byte[] buffer, int offset, int count);
        public abstract int ReadByte();
        public abstract void Write(byte[] buffer, int offset, int count);
        public abstract void WriteByte(byte value);
    }
}
