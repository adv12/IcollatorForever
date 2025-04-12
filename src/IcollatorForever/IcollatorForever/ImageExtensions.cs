// Copyright (c) 2025 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System;
using System.Collections;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.ImageSharp.Formats.Png;

namespace IcollatorForever
{
    public static class ImageExtensions
    {
        public static string ToBase64JpegString<TPixel>(this Image<TPixel> image)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsJpeg(stream);
                byte[] bytes = stream.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        public static string? ToBase64PngString(this Image? image)
        {
            if (image == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsPng(stream);
                byte[] bytes = stream.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        public static string? ToBase64GifString(this Image? image)
        {
            if (image == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsGif(stream);
                byte[] bytes = stream.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        public static IcoIconEntry ToIcoIconEntry(this Image image, IconEntryDescription description, bool asPng = false)
        {
            switch (description.BitCount)
            {
                case 32:
                case 24:
                    return image.ToIcoIconEntryFullColor(description, asPng);
                default:
                    return image.ToIcoIconEntryIndexedColor(description, asPng);
            }
        }

        public static IcoIconEntry ToIcoIconEntryFullColor(this Image image, IconEntryDescription description, bool asPng = false)
        {
            byte[] xorImageBytes = image.GetXorImageBytesFullColor(description, asPng);

            if (asPng)
            {
                description.OverwriteSizeInBytes(xorImageBytes.Length);
                return new IcoIconEntry(description, xorImageBytes);
            }
            else
            {

                byte[] andImageBytes = image.GetAndImageBytes(description);
                return GetIcoIconEntry(description, xorImageBytes, andImageBytes);
            }

        }

        public static IcoIconEntry ToIcoIconEntryIndexedColor(this Image image, IconEntryDescription description, bool asPng = false)
        {
            byte[] xorImageBytes = image.GetXorImageBytesIndexedColor(description, asPng);

            if (asPng)
            {
                description.OverwriteSizeInBytes(xorImageBytes.Length);
                return new IcoIconEntry(description, xorImageBytes);
            }
            else
            {
                byte[] andImageBytes = image.GetAndImageBytes(description);
                return GetIcoIconEntry(description, xorImageBytes, andImageBytes);
            }
        }

        private static IcoIconEntry GetIcoIconEntry(IconEntryDescription description, byte[] xorImageBytes, byte[] andImageBytes)
        {
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

        public static byte[] GetXorImageBytesFullColor(this Image image, IconEntryDescription description, bool asPng = false)
        {
            int bytesPerPixel = description.BitCount / 8;

            if (asPng)
            {
                PngEncoder encoder = new PngEncoder
                {
                    BitDepth = PngBitDepth.Bit8,
                    ColorType = description.BitCount == 32 ? PngColorType.RgbWithAlpha : PngColorType.Rgb
                };
                using (MemoryStream stream = new MemoryStream())
                {
                    image.SaveAsPng(stream, encoder);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream.ToArray();
                }
            }

            Image<Rgba32> rgbaImage = image as Image<Rgba32> ?? image.CloneAs<Rgba32>();

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
                    Rgba32 pixel = rgbaImage[c, description.Height - 1 - r];
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

        public static byte[] GetXorImageBytesIndexedColor(this Image image, IconEntryDescription description, bool asPng = false)
        {
            int bitCount = description.BitCount;
            int numColors = 1 << bitCount;
            if (bitCount == 1)
            {
                image = image.Clone(x => x.BlackWhite());
            }

            if (asPng)
            {
                PngEncoder encoder = new PngEncoder
                {
                    BitDepth = (PngBitDepth)bitCount,
                    ColorType = PngColorType.Palette
                };
                using (MemoryStream stream = new MemoryStream())
                {
                    image.SaveAsPng(stream, encoder);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream.ToArray();
                }
            }

            Image<Rgba32> rgbaImage = image as Image<Rgba32> ?? image.CloneAs<Rgba32>();

            QuantizerOptions options = new QuantizerOptions() { MaxColors = Math.Min(255, numColors) };
            WuQuantizer quantizer = new WuQuantizer(options);
            var frameQuantizer = quantizer.CreatePixelSpecificQuantizer<Rgba32>(new Configuration());
            frameQuantizer.BuildPalette(new DefaultPixelSamplingStrategy(), rgbaImage);
            var quantizedFrame = frameQuantizer.QuantizeFrame(rgbaImage.Frames[0], new Rectangle(0, 0, image.Width, image.Height));
            Console.WriteLine($"quantizedFrame.Palette.Length = {quantizedFrame.Palette.Length}");
            int bitsInRow = quantizedFrame.Width * bitCount;
            // rows are always a multiple of 32 bits long
            // (padded with 0's if necessary)
            int remainder = bitsInRow % 32;
            int padding = 0;
            if (remainder > 0)
            {
                Console.WriteLine($"remainder = {remainder}");
                padding = 32 - remainder;
                bitsInRow += padding;
            }
            Console.WriteLine($"bitsInRow = {bitsInRow}");
            int bytesInRow = bitsInRow / 8;
            Console.WriteLine($"bytesInRow = {bytesInRow}");
            using (MemoryStream stream = new MemoryStream())
            using (EndianBinaryWriter writer = new EndianBinaryWriter(EndianBitConverter.Little, stream))
            {
                foreach (Rgba32 pixel in quantizedFrame.Palette.ToArray())
                {
                    writer.Write(pixel.B);
                    writer.Write(pixel.G);
                    writer.Write(pixel.R);
                    writer.Write(pixel.A);
                }
                for (int i = quantizedFrame.Palette.Length; i < numColors; i++)
                {
                    writer.Write(0);
                }
                Console.WriteLine($"quantizedFrame.Height: {quantizedFrame.Height}");
                Console.WriteLine($"quantizedFrame.Width: {quantizedFrame.Width}");

                int valsPerByte = 8 / bitCount;
                for (int r = 0; r < quantizedFrame.Height; r++)
                {
                    var rowVals = quantizedFrame.GetWritablePixelRowSpanUnsafe(quantizedFrame.Height - 1 - r);
                    byte b = 0;
                    int position = valsPerByte - 1;
                    int bytesWritten = 0;
                    foreach (byte val in rowVals)
                    {
                        b |= (byte)(val << (position * bitCount));
                        if (--position < 0)
                        {
                            writer.Write(b);
                            bytesWritten++;
                            b = 0;
                            position = valsPerByte - 1;
                        }
                    }
                    Console.WriteLine($"bytesWritten = {bytesWritten}");
                    int extra = bytesInRow - bytesWritten;
                    Console.WriteLine($"extra = {extra}");
                    for (int i = 0; i < extra; i++)
                    {
                        writer.Write((byte)0);
                    }
                }
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
            ;
        }

        public static byte[] GetAndImageBytes(this Image image, IconEntryDescription description)
        {
            Image<Rgba32> rgbaImage = image as Image<Rgba32> ?? image.CloneAs<Rgba32>();
            int bitsInRow = rgbaImage.Width;
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
            byte[] andImageBytes = new byte[stride * rgbaImage.Height];
            BitArray bits = new BitArray(bitsInRow * rgbaImage.Height);

            for (int r = 0; r < description.Height; r++)
                {
                    int rowOffset = r * bitsInRow;
                    bool lastValue = true;
                    for (int c = 0; c < description.Width; c++)
                    {
                        Rgba32 pixel = rgbaImage[c, description.Height - 1 - r];
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

        public static bool HasTransparency(this Image image)
        {
            Image<Rgba32>? rgbaImage = image as Image<Rgba32>;
            if (rgbaImage == null)
            {
                return false;
            }
            if (image.Width == 0 || image.Height == 0)
            {
                return false;
            }

            Rgba32 pixel = rgbaImage[0, 0];
            if (pixel.A != 0 && pixel.A != 255)
            {
                return true;
            }
            for (int x = 0; x < rgbaImage.Width; x++)
            {
                for (int y = 0; y < rgbaImage.Height; y++)
                {
                    if (rgbaImage[x, y].A != pixel.A)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
