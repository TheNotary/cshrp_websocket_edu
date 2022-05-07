using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebsocketEdu
{
    public interface INetworkStream
    {
        bool DataAvailable { get; }
        Stream Stream { get; }

        int ReadByte();
        void WriteByte(byte value);
        void Read(byte[] buffer, int offset, int count);
        void Write(byte[] buffer, int offset, int count);
    }
}
