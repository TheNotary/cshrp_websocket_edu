using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static void PutByte(this MemoryStream memoryStream, int index, int bits)
        {
            byte[] buffer = memoryStream.GetBuffer();
            memoryStream.SetLength(memoryStream.Length + 1);
            buffer[index] = (byte)bits;
        }

    }
}
