﻿// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace IcollatorForever
{
    public class IconEntry : IComparable<IconEntry>
    {
        public string SourceFileName { get; }
        public int SourceIndex { get; }
        public int Width { get; private set; }
        public int Height { get; }
        public int IhHeight { get; private set; }
        public int ColorCount { get; }
        public byte Reserved { get; private set; }
        public int Planes { get; private set; }
        public int BitCount { get; private set; }
        public int IhCompression { get; private set; }
        public int IhImageSize { get; private set; }
        public int IhXpixelsPerM { get; private set; }
        public int IhYpixelsPerM { get; private set; }
        public int IhColorsUsed { get; private set; }
        public int IhColorsImportant { get; private set; }
        public int SizeInBytes { get; private set; }
        public int FileOffset { get; }
        public int HeaderSize { get; private set; }
        public byte[] Data { get; private set; }
        public int[][] XorColors { get; private set; }
        public int[][] AndColors { get; private set; }
        public int[][] XorIndices { get; private set; }
        public int[][] AndIndices { get; private set; }
        public int[][][] XorRgba { get; private set; }
        public Image<Rgba32> AndImage { get; private set; }
        public Image<Rgba32> XorImage { get; private set; }

        public string XorPngString
        {
            get
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    XorImage.SaveAsPng<Rgba32>(stream);
                    byte[] bytes = stream.GetBuffer();
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        public string AndPngString
        {
            get
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    AndImage.SaveAsPng<Rgba32>(stream);
                    byte[] bytes = stream.GetBuffer();
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        public string XorJpegString
        {
            get
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    XorImage.SaveAsJpeg<Rgba32>(stream);
                    byte[] bytes = stream.GetBuffer();
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        public string AndJpegString
        {
            get
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    AndImage.SaveAsJpeg<Rgba32>(stream);
                    byte[] bytes = stream.GetBuffer();
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        public IconEntry(int width, int height, int colorCount,
        byte reservedByte, int planes, int bitCount,
        int sizeInBytes, int fileOffset, string sourceFileName,
        int indexInSourceFile)
        {
            Width = width;
            Height = height;
            ColorCount = colorCount;
            SizeInBytes = sizeInBytes;
            FileOffset = fileOffset;
            SourceFileName = sourceFileName;
            SourceIndex = indexInSourceFile;
        }

        public void SetData(byte[] value)
        {
            Data = value;
            ReadHeader(Data);
            XorImage = GetImage(Data, 40, Width, Height,
                BitCount, true);
            int xorColorTableEntries = 0;
            if (BitCount <= 8)
            {
                xorColorTableEntries = 1 << BitCount;
            }
            int bitsInRow = Width * BitCount;
            // rows are always a multiple of 32 bytes long
            // (padded with 0's if necessary)
            int remainder = bitsInRow % 32;
            if (remainder > 0)
            {
                bitsInRow += (32 - remainder);
            }

            int xorByteCount = Height * bitsInRow / 8;
            int startOfAndImage = 40 + xorColorTableEntries * 4 + xorByteCount;
            AndImage = GetImage(Data, startOfAndImage,
                Width, Height, 1, false);
            int bitsInAndRow = Width;
            int andRemainder = bitsInAndRow % 32;
            if (andRemainder > 0)
            {
                bitsInAndRow += 32 - andRemainder;
            }
            int andByteCount = Height * bitsInAndRow / 8;
            int calculatedSizeInBytes = startOfAndImage + andByteCount;
            if (calculatedSizeInBytes != SizeInBytes)
            {
                //System.err.println("Calculated Size in Bytes: " + calculatedSizeInBytes);
                //System.err.println("Supposed Size in Bytes: " + SizeInBytes);
            }
            SizeInBytes = calculatedSizeInBytes;
            byte[] correctedData = new byte[calculatedSizeInBytes];
            for (int i = 0; i < correctedData.Length; i++)
            {
                if (i < Data.Length)
                {
                    correctedData[i] = Data[i];
                }
            }
            Data = correctedData;
        }

        public static int[] SplitBytes(byte[] inBytes, int splitInto)
        {
            int[] outBytes = new int[splitInto * inBytes.Length];
            int bits = 8 / splitInto;
            int mask = (255 >> (8 - bits));
            for (int i = 0; i < inBytes.Length; i++)
            {
                int outIndex = i * splitInto;
                int b = inBytes[i];
                for (int j = 0; j < splitInto; j++)
                {
                    outBytes[outIndex + j] = mask & (b >> ((splitInto - 1 - j) * bits));
                }
            }
            return outBytes;
        }

        private void ReadHeader(byte[] data)
        {
            using (MemoryStream s = new MemoryStream(data))
            {
                try
                {
                    HeaderSize = IconUtils.ReadInt(s, 4);
                    Console.WriteLine("headerSize = " + HeaderSize);
                    Width = IconUtils.ReadInt(s, 4);
                    Console.WriteLine("width = " + Width);
                    IhHeight = IconUtils.ReadInt(s, 4);
                    Console.WriteLine("ihHeight = " + IhHeight);
                    if (IhHeight == Height)
                    {
                        // this is an indication that the developer of the software
                        // that wrote this file (e.g. GraphicConverter) incorrectly
                        // interpreted the "Height" field in the InfoHeader as designating
                        // the height of the icon rather than the combined height of
                        // the XOR and AND images that make up the icon.  We need to fix
                        // this in the data lest we write out equally incorrect data.
                        IhHeight = Height * 2;
                        byte[] replacementBytes = IconUtils.GetBytes(IhHeight, 4);
                        for (int i = 0; i < 4; i++)
                        {
                            data[16 + i] = replacementBytes[i];
                        }
                        Console.WriteLine("ihHeight fixed: " + IhHeight);
                    }
                    Planes = IconUtils.ReadInt(s, 2);
                    Console.WriteLine("planes = " + Planes);
                    BitCount = IconUtils.ReadInt(s, 2);
                    Console.WriteLine("bitCount = " + BitCount);
                    IhCompression = IconUtils.ReadInt(s, 4);
                    Console.WriteLine("ihCompression = " + IhCompression);
                    IhImageSize = IconUtils.ReadInt(s, 4);
                    Console.WriteLine("ihImageSize = " + IhImageSize);
                    IhXpixelsPerM = IconUtils.ReadInt(s, 4);
                    Console.WriteLine("ihXpixelsPerM = " + IhXpixelsPerM);
                    IhYpixelsPerM = IconUtils.ReadInt(s, 4);
                    Console.WriteLine("ihYpixelsPerM = " + IhYpixelsPerM);
                    IhColorsUsed = IconUtils.ReadInt(s, 4);
                    Console.WriteLine("ihColorsUsed = " + IhColorsUsed);
                    IhColorsImportant = IconUtils.ReadInt(s, 4);
                    Console.WriteLine("ihColorsImportant = " + IhColorsImportant);
                }
                catch (Exception e)
                {
                    //System.err.println("Error reading InfoHeader: ");
                    //e.printStackTrace(System.err);
                }
            }
        }

        private static Image<Rgba32> GetImage(byte[] data, int imOffset,
            int imWidth, int imHeight, int imBitCount, bool hasColorTable)
        {
            int calculatedColorCount = 1 << imBitCount;

            Image<Rgba32> im = new Image<Rgba32>(imWidth, imHeight);

            try
            {
                if (imBitCount <= 8)
                {
                    byte[] reds;
                    byte[] greens;
                    byte[] blues;

                    if (hasColorTable)
                    {
                        reds = new byte[calculatedColorCount];
                        greens = new byte[calculatedColorCount];
                        blues = new byte[calculatedColorCount];

                        for (int i = 0; i < calculatedColorCount; i++)
                        {
                            blues[i] = data[imOffset + (i * 4)];
                            greens[i] = data[imOffset + 1 + (i * 4)];
                            reds[i] = data[imOffset + 2 + (i * 4)];
                        }
                    }
                    else
                    {
                        reds = new byte[] { 0, (byte)255 };
                        greens = new byte[] { 0, (byte)255 };
                        blues = new byte[] { 0, (byte)255 };
                    }

                    MemoryStream s = new MemoryStream(data);

                    s.Seek(imOffset, SeekOrigin.Begin);
                    if (hasColorTable)
                    {
                        s.Seek(4 * calculatedColorCount, SeekOrigin.Current);
                    }

                    int bitsInRow = imWidth * imBitCount;
                    // rows are always a multiple of 32 bytes long
                    // (padded with 0's if necessary)
                    int remainder = bitsInRow % 32;
                    if (remainder > 0)
                    {
                        bitsInRow += (32 - remainder);
                    }
                    byte[] rowBytes = new byte[bitsInRow / 8];
                    if (!hasColorTable)
                    {
                        //                    Console.WriteLine("beginning and image");
                    }
                    for (int i = 0; i < imHeight; i++)
                    {
                        s.Read(rowBytes, 0, rowBytes.Length);
                        int[] pixels = SplitBytes(rowBytes, 8 / imBitCount);

                        for (int j = 0; j < imWidth; j++)
                        {
                            int pixel = pixels[j];
                            //                        Console.WriteLine("pixel " + j + "," + (imHeight - 1 - i)
                            //                            + " has index " + pixels[j]);
                            im[j, imHeight - i - 1] = new Rgba32(reds[pixel], greens[pixel], blues[pixel]);
                        }
                    }
                }
                else if (imBitCount == 24)
                {
                    for (int i = 0; i < imHeight; i++)
                    {
                        for (int j = 0; j < imWidth; j++)
                        {

                            byte blue = data[imOffset + 3 * (imWidth * i + j)];
                            byte green = data[imOffset + 3 * (imWidth * i + j) + 1];
                            byte red = data[imOffset + 3 * (imWidth * i + j) + 2];
                            //                        Console.WriteLine("Color " + i + "[" + (red & 255)
                            //                        + "," + (green & 255) + ","
                            //                        + (blue & 255) + "," + (alpha & 255) + "]" );

                            im[j, imHeight - i - 1] = new Rgba32(red, green, blue);
                        }
                    }
                }
                else if (imBitCount == 32)
                {
                    bool hasAlphaChannel = false;
                    for (int i = 0; i < imHeight; i++)
                    {
                        for (int j = 0; j < imWidth; j++)
                        {

                            byte blue = data[imOffset + 4 * (imWidth * i + j)];
                            byte green = data[imOffset + 4 * (imWidth * i + j) + 1];
                            byte red = data[imOffset + 4 * (imWidth * i + j) + 2];
                            byte alpha = data[imOffset + 4 * (imWidth * i + j) + 3];

                            if (alpha != 0)
                            {
                                hasAlphaChannel = true;
                            }
                            //                        Console.WriteLine("Color " + i + "[" + (red & 255)
                            //                        + "," + (green & 255) + ","
                            //                        + (blue & 255) + "," + (alpha & 255) + "]" );

                            im[j, imHeight - i - 1] = new Rgba32(red, green, blue, alpha);
                        }
                    }
                    if (!hasAlphaChannel)
                    {
                        for (int x = 0; x < imWidth; x++)
                        {
                            for (int y = 0; y < imHeight; y++)
                            {
                                data[imOffset + (4 * (imWidth * x + (imHeight - 1 - y))) + 3] = (byte)255;
                                Rgba32 color = im[x, y];
                                im[x, y] = new Rgba32(color.R, color.G, color.B, 255);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //e.printStackTrace(System.err);
            }
            return im;

        }

        public int CompareTo(IconEntry other)
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
