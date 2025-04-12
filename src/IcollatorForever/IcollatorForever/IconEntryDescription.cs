// Copyright (c) 2025 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System;
using System.Text;

namespace IcollatorForever
{
    public class IconEntryDescription : IEquatable<IconEntryDescription>, IComparable<IconEntryDescription>
    {
        public int Width { get; private set; }
        public int Height { get; }
        public int ColorCount { get; }
        public byte ReservedByte { get; }
        public int Planes { get; private set; }
        public int BitCount { get; private set; }
        public int SizeInBytes { get; private set; }
        public int FileOffset { get; }
        public string? SourceFileName { get; }
        public int SourceIndex { get; }

        public IconEntryDescription(int width, int height, int colorCount,
            byte reservedByte, int planes, int bitCount,
            int sizeInBytes, int fileOffset, string? sourceFileName,
            int indexInSourceFile)
        {
            Width = width;
            Height = height;
            ColorCount = colorCount;
            ReservedByte = reservedByte;
            Planes = planes;
            BitCount = bitCount;
            SizeInBytes = sizeInBytes;
            FileOffset = fileOffset;
            SourceFileName = sourceFileName;
            SourceIndex = indexInSourceFile;
        }

        public void OverwriteSizeInBytes(int size)
        {
            SizeInBytes = size;
        }

        public void OverwriteWidth(int width)
        {
            Width = width;
        }

        public void OverwritePlanes(int planes)
        {
            Planes = planes;
        }

        public void OverwriteBitCount(int bitCount)
        {
            BitCount = bitCount;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as IconEntryDescription);
        }

        public bool Equals(IconEntryDescription? other)
        {
            return other != null &&
                   Width == other.Width &&
                   Height == other.Height &&
                   ColorCount == other.ColorCount &&
                   ReservedByte == other.ReservedByte &&
                   Planes == other.Planes &&
                   BitCount == other.BitCount &&
                   SizeInBytes == other.SizeInBytes &&
                   FileOffset == other.FileOffset &&
                   SourceFileName == other.SourceFileName &&
                   SourceIndex == other.SourceIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HashCode.Combine(Width, Height, ColorCount, ReservedByte),
            HashCode.Combine(Planes, BitCount, SizeInBytes, FileOffset, SourceFileName, SourceIndex));
        }

        public int CompareTo(IconEntryDescription? other)
        {
            if (other == null)
            {
                return 1;
            }
            if (this.Width > other.Width)
            {
                return 1;
            }
            else if (this.Width < other.Width)
            {
                return -1;
            }
            else
            {
                if (this.BitCount < other.BitCount)
                {
                    return 1;
                }
                else if (this.BitCount > other.BitCount)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string ToKey()
        {
            StringBuilder sb = new StringBuilder(SourceFileName);
            sb.Append("[");
            sb.Append(SourceIndex);
            sb.Append("]@");
            sb.Append(Width);
            sb.Append("x");
            sb.Append(Height);
            sb.Append(",");
            sb.Append(BitCount);
            sb.Append("bit,");
            sb.Append(ColorCount);
            sb.Append("colors,");
            sb.Append(SizeInBytes);
            sb.Append("bytes,");
            sb.Append("fileOffset=");
            sb.Append(FileOffset);
            return sb.ToString();
        }
    }
}
