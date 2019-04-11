// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.IO;

namespace IcollatorForever
{
    public class IconUtils
    {

        public static List<IconEntry> GetList(string filename, Stream s)
        {
            int reserved = ReadInt(s, 2);
            int type = ReadInt(s, 2);
            int count = ReadInt(s, 2);
            List<IconEntry> list = new List<IconEntry>();
            for (int i = 0; i < count; i++)
            {
                int width = s.ReadByte();
                int height = s.ReadByte();
                int colorCount = s.ReadByte();
                int reserved2 = s.ReadByte();
                int planes = ReadInt(s, 2);
                int bitCount = ReadInt(s, 2);
                int sizeInBytes = ReadInt(s, 4);
                int fileOffset = ReadInt(s, 4);
                if (width == 0)
                {
                    width = 256;
                }
                if (height == 0)
                {
                    height = 256;
                }
                IconEntry entry = new IconEntry(width, height, colorCount, (byte)reserved2,
                    planes, bitCount, sizeInBytes, fileOffset, filename, i);
                list.Add(entry);
            }
            for (int i = 0; i < list.Count; i++)
            {
                IconEntry entry = list[i];
                byte[] data = new byte[entry.SizeInBytes];
                s.Read(data, 0, data.Length);
                entry.SetData(data);
            }
            return list;
        }
        public static int ReadInt(Stream s, int numBytes)
        {
            int value = 0;
            for (int i = 0; i < numBytes; i++)
            {
                value += (s.ReadByte() << (8 * i));
            }
            return value;
        }
        public static byte[] GetBytes(int value, int numBytes)
        {
            byte[] bytes = new byte[numBytes];
            for (int i = 0; i < numBytes; i++)
            {
                bytes[i] = (byte)(255 & (value >> (8 * i)));
            }
            return bytes;
        }
        public static void WriteToStream(List<IconEntry> list, Stream s)
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
                IconEntry entry = list[i];
                s.Write(GetBytes(entry.Width, 1), 0, 1);
                s.Write(GetBytes(entry.Height, 1), 0, 1);
                s.Write(GetBytes(entry.ColorCount, 1), 0, 1);
                s.Write(GetBytes(entry.Reserved, 1), 0, 1);
                s.Write(GetBytes(entry.Planes, 2), 0, 2);
                s.Write(GetBytes(entry.BitCount, 2), 0, 2);
                s.Write(GetBytes(entry.SizeInBytes, 4), 0, 4);
                s.Write(GetBytes(fileOffset, 4), 0, 4);
                fileOffset += entry.SizeInBytes;
            }
            // Now that we've written the index at the
            // start of the file, write the actual
            // data for each icon
            for (int i = 0; i < count; i++)
            {
                IconEntry entry = list[i];
                s.Write(entry.Data, 0, entry.Data.Length);
            }
        }


    }

}
