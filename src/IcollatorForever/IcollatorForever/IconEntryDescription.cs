using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public string SourceFileName { get; }
        public int SourceIndex { get; }

        public IconEntryDescription(int width, int height, int colorCount,
            byte reservedByte, int planes, int bitCount,
            int sizeInBytes, int fileOffset, string sourceFileName,
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

        public override bool Equals(object obj)
        {
            return Equals(obj as IconEntryDescription);
        }

        public bool Equals(IconEntryDescription other)
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
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + Width.GetHashCode();
                hash = hash * 23 + Height.GetHashCode();
                hash = hash * 23 + ColorCount.GetHashCode();
                hash = hash * 23 + ReservedByte.GetHashCode();
                hash = hash * 23 + Planes.GetHashCode();
                hash = hash * 23 + BitCount.GetHashCode();
                hash = hash * 23 + SizeInBytes.GetHashCode();
                hash = hash * 23 + FileOffset.GetHashCode();
                hash = hash * 23 + (SourceFileName == null ? 1 : SourceFileName.GetHashCode());
                hash = hash * 23 + SourceIndex.GetHashCode();
                return hash;
            }
        }

        public int CompareTo(IconEntryDescription other)
        {
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
    }
}
