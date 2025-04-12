// Copyright (c) 2025 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.IO;

namespace IcollatorForever
{
    public class IconUtils
    {
        public static byte[] GetBytes(int value, int numBytes)
        {
            byte[] bytes = new byte[numBytes];
            for (int i = 0; i < numBytes; i++)
            {
                bytes[i] = (byte)(255 & (value >> (8 * i)));
            }
            return bytes;
        }

        public static void WriteToStream(List<IIconEntry> list, Stream s)
        {
            // Reserved (always 0)
            s.Write(GetBytes(0, 2), 0, 2);
            // Type (always 1 for an icon)
            s.Write(GetBytes(1, 2), 0, 2);
            // Icon Counts
            int count = list.Count;
            s.Write(GetBytes(count, 2), 0, 2);
            // fileOffset of start of first icon.
            // as we iterate through the collection,
            // keep a running total of the sizeInBytes
            // property (plus this initial offset)
            // so we know the fileOffset for each icon.
            int fileOffset = 6 + (16 * count);
            for (int i = 0; i < count; i++)
            {
                IIconEntry entry = list[i];
                s.Write(GetBytes(entry.Description.Width, 1), 0, 1);
                s.Write(GetBytes(entry.Description.Height, 1), 0, 1);
                s.Write(GetBytes(entry.Description.ColorCount, 1), 0, 1);
                s.Write(GetBytes(entry.Description.ReservedByte, 1), 0, 1);
                s.Write(GetBytes(entry.Description.Planes, 2), 0, 2);
                s.Write(GetBytes(entry.Description.BitCount, 2), 0, 2);
                s.Write(GetBytes(entry.Description.SizeInBytes, 4), 0, 4);
                s.Write(GetBytes(fileOffset, 4), 0, 4);
                fileOffset += entry.Description.SizeInBytes;
            }
            // Now that we've written the index at the
            // start of the file, write the actual
            // data for each icon
            for (int i = 0; i < count; i++)
            {
                IIconEntry entry = list[i];
                entry.Write(s);
            }
        }


    }

}
