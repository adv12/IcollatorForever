// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System;
using System.Collections;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace IcollatorForever
{
    public static class ImageExtensions
    {
        public static string ToBase64JpegString<TPixel>(this Image<TPixel> image)
            where TPixel : struct, IPixel<TPixel>
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsJpeg(stream);
                byte[] bytes = stream.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        public static string ToBase64PngString<TPixel>(this Image<TPixel> image)
            where TPixel : struct, IPixel<TPixel>
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsPng(stream);
                byte[] bytes = stream.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        public static string ToBase64GifString<TPixel>(this Image<TPixel> image)
            where TPixel : struct, IPixel<TPixel>
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsGif(stream);
                byte[] bytes = stream.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        public static IcoIconEntry ToIcoIconEntry(this Image<Rgba32> image, IconEntryDescription description)
        {
            switch (description.BitCount)
            {
                case 32:
                case 24:
                    return image.ToIcoIconEntryFullColor(description);
                default:
                    return null;
            }
        }

        public static IcoIconEntry ToIcoIconEntryFullColor(this Image<Rgba32> image, IconEntryDescription description)
        {
            byte[] xorImageBytes = image.GetXorImageBytesFullColor(description);

            byte[] andImageBytes = image.GetAndImageBytes(description);

            description.OverwriteSizeInBytes(40 + xorImageBytes.Length + andImageBytes.Length);
            byte[] header = description.ToIcoEntryHeader();
            Console.WriteLine($"header.Length = {header.Length}");
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(header, 0, header.Length);
                stream.Write(xorImageBytes, 0, xorImageBytes.Length);
                stream.Write(andImageBytes, 0, andImageBytes.Length);
                byte[] data = stream.ToArray();
                return new IcoIconEntry(description, data);
            }
        }

        public static byte[] GetXorImageBytesFullColor(this Image<Rgba32> image, IconEntryDescription description)
        {
            int bytesPerPixel = description.BitCount / 8;
            Console.WriteLine($"bytesPerPixel = {bytesPerPixel}");
            int stride = description.Width * bytesPerPixel;
            Console.WriteLine($"stride = {stride}");
            byte[] xorImageBytes = new byte[description.Height * stride];
            for (int r = 0; r < description.Height; r++)
            {
                int rowOffset = r * stride;
                for (int c = 0; c < description.Width; c++)
                {
                    int offset = rowOffset + c * bytesPerPixel;
                    Rgba32 pixel = image[c, description.Height - 1 - r];
                    xorImageBytes[offset] = pixel.B;
                    xorImageBytes[offset + 1] = pixel.G;
                    xorImageBytes[offset + 2] = pixel.R;
                    if (bytesPerPixel == 4)
                    {
                        xorImageBytes[offset + 3] = pixel.A;
                    }
                }
            }
            return xorImageBytes;
        }

        public static byte[] GetAndImageBytes(this Image<Rgba32> image, IconEntryDescription description)
        {
            int bitsInRow = image.Width;
            // rows are always a multiple of 32 bits long
            // (padded with 0's if necessary)
            int remainder = bitsInRow % 32;
            int padding = 0;
            if (remainder > 0)
            {
                padding = 32 - remainder;
                bitsInRow += padding;
            }
            int stride = bitsInRow / 8;
            byte[] andImageBytes = new byte[stride * image.Height];
            BitArray bits = new BitArray(bitsInRow * image.Height);

            for (int r = 0; r < description.Height; r++)
            {
                int rowOffset = r * bitsInRow;
                bool lastValue = true;
                for (int c = 0; c < description.Width; c++)
                {
                    Rgba32 pixel = image[c, description.Height - 1 - r];
                    int offset = rowOffset + c;
                    if (pixel.A > 0)
                    {
                        bits[offset] = lastValue = false;
                    }
                    else
                    {
                        bits[offset] = lastValue = true;
                    }
                }
                for (int i = 0; i < padding; i++)
                {
                    bits[rowOffset + description.Width + i] = lastValue;
                }
            }
            bits.CopyTo(andImageBytes, 0);
            return andImageBytes;
        }

    }
}
