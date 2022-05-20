using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsocketEdu
{
    public struct WebsocketFrame
    {
        public byte[] _headerBytes;

        public bool fin,
             isMasked;
        public int opcode;
        public ulong payloadLength;
        public byte[] mask;
        public byte[] encodedPayload;
        public byte[] decodedPayload;
        public int closeCode;
        public string closeCodeReason;


    }
}
