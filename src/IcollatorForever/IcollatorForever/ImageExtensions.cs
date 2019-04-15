// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace IcollatorForever
{
    public static class ImageExtensions
    {
        public static string ToBase64JpegString(this Image<Rgba32> image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsJpeg(stream);
                byte[] bytes = stream.GetBuffer();
                return Convert.ToBase64String(bytes);
            }
        }

        public static string ToBase64PngString(this Image<Rgba32> image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsPng(stream);
                byte[] bytes = stream.GetBuffer();
                return Convert.ToBase64String(bytes);
            }
        }

        public static string ToBase64GifString(this Image<Rgba32> image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsGif(stream);
                byte[] bytes = stream.GetBuffer();
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
