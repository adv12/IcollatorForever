using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IcollatorForever
{
    public static class StreamExtensions
    {
        public static int ReadInt(this Stream s, int numBytes)
        {
            int value = 0;
            for (int i = 0; i < numBytes; i++)
            {
                value += (s.ReadByte() << (8 * i));
            }
            return value;
        }
    }
}
