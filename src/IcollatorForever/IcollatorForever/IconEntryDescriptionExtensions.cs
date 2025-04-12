// Copyright (c) 2025 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcollatorForever
{
    public static class IconEntryDescriptionExtensions
    {
        public static byte[] ToIcoEntryHeader(this IconEntryDescription description)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (EndianBinaryWriter writer = new EndianBinaryWriter(EndianBitConverter.Little, stream))
                {
                    writer.Write(40);
                    writer.Write(description.Width);
                    writer.Write(description.Height * 2); // *2 to account for height of AND image
                    writer.Write((short)description.Planes);
                    writer.Write((short)description.BitCount);
                    writer.Write(0); // no compression
                    writer.Write(description.SizeInBytes - 40); // ImageHeader SizeImage
                    writer.Write(0); // ImageHeader XPixelsPerM
                    writer.Write(0); // ImageHeader YPixelsPerM
                    writer.Write(0); // ImageHeader ColorsUsed
                    writer.Write(0); // ImageHeader ColorsImportant
                }
                return stream.ToArray();
            }
        }
    }
}
