// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System.IO;

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
