// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.ComponentModel;
using SixLabors.ImageSharp.Processing;

namespace IcollatorForever
{
    public class IcoIconEntry : IIconEntry, IDisposable
    {
        private int _startOfXorImage;
        private int _startOfAndImage;

        private Image<Rgba32>? _xorImage;
        private Image<Rgba32>? _andImage;

        private string? _xorDataUrl;
        private string? _andDataUrl;

        private EndianBinaryReader? _reader;

        public IconEntryDescription Description { get; }

        public int IhHeight { get; private set; }
        public int IhCompression { get; private set; }
        public int IhImageSize { get; private set; }
        public int IhXpixelsPerM { get; private set; }
        public int IhYpixelsPerM { get; private set; }
        public int IhColorsUsed { get; private set; }
        public int IhColorsImportant { get; private set; }
        public int HeaderSize { get; private set; }
        public byte[] Data { get; private set; }
        public bool IsPng { get; private set; }
        public bool HasAndImage => !IsPng;
        public bool HasXorImage => true;

        public Image<Rgba32> XorImage
        {
            get
            {
                if (_xorImage == null)
                {
                    if (IsPng)
                    {
                        _xorImage = Image.Load<Rgba32>(Data);
                    }
                    else
                    {
                        _xorImage = GetImage(Data, _startOfXorImage, Description.Width, Description.Height, Description.BitCount, true);
                    }
                }
                return _xorImage;
            }
        }

        public Image<Rgba32>? AndImage
        {
            get
            {
                if (_andImage == null)
                {
                    if (IsPng)
                    {
                        return null;
                    }
                    else
                    {
                        _andImage = GetImage(Data, _startOfAndImage, Description.Width, Description.Height, 1, false);
                    }
                }
                return _andImage;
            }
        }

        public string XorDataUrl
        {
            get
            {
                if (_xorDataUrl == null)
                {
                    _xorDataUrl = "data:image/png;base64," + XorImage.ToBase64PngString();
                }
                return _xorDataUrl;
            }
        }

        public string AndDataUrl
        {
            get
            {
                if (_andDataUrl == null)
                {
                    _andDataUrl = "data:image/png;base64," + AndImage.ToBase64PngString();
                }
                return _andDataUrl;
            }
        }

        public IcoIconEntry(IconEntryDescription description, Stream stream)
        {
            Description = description;
            if (stream.CanSeek)
            {
                stream.Seek(Description.FileOffset, SeekOrigin.Begin);
            }
            byte[] data = new byte[Description.SizeInBytes];
            int totalBytesRead = 0;
            int bytesRead = 0;
            while ((bytesRead = stream.Read(data, totalBytesRead, data.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;
            }
            Data = data;
            Init();
        }

        public IcoIconEntry(IconEntryDescription description, byte[] data)
        {
            Description = description;
            Data = data;
            Init();
        }

        public void Init()
        {
            IsPng = true;
            if (Data.Length > 8)
            {
                // PNG signature (first 8 bytes of all PNG files)
                byte[] sig = { 137, 80, 78, 71, 13, 10, 26, 10 };
                for (int i = 0; i < 8; i++)
                {
                    if (Data[i] != sig[i])
                    {
                        IsPng = false;
                        break;
                    }
                }
            }
            if (IsPng)
            {
                ReadPng();
            }
            else
            {
                ReadBmp();
            }
        }

        public void ReadPng()
        {
            // Do I need to do anything here?  Or is it replaced by lazy loading _xorImage?
        }

        public void ReadBmp()
        {
            ReadHeader(Data);

            _startOfXorImage = HeaderSize;

            int xorColorTableEntries = 0;
            if (Description.BitCount <= 8)
            {
                xorColorTableEntries = 1 << Description.BitCount;
            }
            int bitsInRow = Description.Width * Description.BitCount;
            // rows are always a multiple of 32 bits long
            // (padded with 0's if necessary)
            int remainder = bitsInRow % 32;
            if (remainder > 0)
            {
                bitsInRow += (32 - remainder);
            }

            int xorByteCount = Description.Height * bitsInRow / 8;
            _startOfAndImage = HeaderSize + xorColorTableEntries * 4 + xorByteCount;
            
            int bitsInAndRow = Description.Width;
            int andRemainder = bitsInAndRow % 32;
            if (andRemainder > 0)
            {
                bitsInAndRow += 32 - andRemainder;
            }
            int andByteCount = Description.Height * bitsInAndRow / 8;
            int calculatedSizeInBytes = _startOfAndImage + andByteCount;
            if (calculatedSizeInBytes != Description.SizeInBytes)
            {
                //System.err.println("Calculated Size in Bytes: " + calculatedSizeInBytes);
                //System.err.println("Supposed Size in Bytes: " + SizeInBytes);
            }
            Description.OverwriteSizeInBytes(calculatedSizeInBytes);
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
                    _reader = new EndianBinaryReader(EndianBitConverter.Little, s);
                    HeaderSize = _reader.ReadInt32();
                    //Console.WriteLine("headerSize = " + HeaderSize);
                    Description.OverwriteWidth(_reader.ReadInt32());
                    //Console.WriteLine("width = " + Description.Width);
                    IhHeight = _reader.ReadInt32();
                    //Console.WriteLine("ihHeight = " + IhHeight);
                    if (IhHeight == Description.Height)
                    {
                        // this is an indication that the developer of the software
                        // that wrote this file (e.g. GraphicConverter) incorrectly
                        // interpreted the "Height" field in the InfoHeader as designating
                        // the height of the icon rather than the combined height of
                        // the XOR and AND images that make up the icon.  We need to fix
                        // this in the data lest we write out equally incorrect data.
                        IhHeight = Description.Height * 2;
                        byte[] replacementBytes = IconUtils.GetBytes(IhHeight, 4);
                        for (int i = 0; i < 4; i++)
                        {
                            data[16 + i] = replacementBytes[i];
                        }
                        //Console.WriteLine("ihHeight fixed: " + IhHeight);
                    }
                    Description.OverwritePlanes(_reader.ReadInt16());
                    //Console.WriteLine("planes = " + Description.Planes);
                    Description.OverwriteBitCount(_reader.ReadInt16());
                    //Console.WriteLine("bitCount = " + Description.BitCount);
                    IhCompression = _reader.ReadInt32();
                    //Console.WriteLine("ihCompression = " + IhCompression);
                    IhImageSize = _reader.ReadInt32();
                    //Console.WriteLine("ihImageSize = " + IhImageSize);
                    IhXpixelsPerM = _reader.ReadInt32();
                    //Console.WriteLine("ihXpixelsPerM = " + IhXpixelsPerM);
                    IhYpixelsPerM = _reader.ReadInt32();
                    //Console.WriteLine("ihYpixelsPerM = " + IhYpixelsPerM);
                    IhColorsUsed = _reader.ReadInt32();
                    //Console.WriteLine("ihColorsUsed = " + IhColorsUsed);
                    IhColorsImportant = _reader.ReadInt32();
                    //Console.WriteLine("ihColorsImportant = " + IhColorsImportant);
                }
                catch (Exception)
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
                    // rows are always a multiple of 32 bits long
                    // (padded with 0's if necessary)
                    int remainder = bitsInRow % 32;
                    if (remainder > 0)
                    {
                        bitsInRow += (32 - remainder);
                    }
                    byte[] rowBytes = new byte[bitsInRow / 8];

                    for (int i = 0; i < imHeight; i++)
                    {
                        s.Read(rowBytes, 0, rowBytes.Length);
                        int[] pixels = SplitBytes(rowBytes, 8 / imBitCount);

                        for (int j = 0; j < imWidth; j++)
                        {
                            int pixel = pixels[j];

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
                Console.WriteLine(e);
            }
            return im;

        }

        public void Write(Stream s)
        {
            s.Write(Data, 0, Data.Length);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }
            IcoIconEntry that = (IcoIconEntry)obj;
            return this.Description.Equals(that.Description);
        }

        public override int GetHashCode()
        {
            return Description.GetHashCode();
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }

}
