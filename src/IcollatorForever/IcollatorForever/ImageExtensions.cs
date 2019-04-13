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
                image.SaveAsJpeg<Rgba32>(stream);
                byte[] bytes = stream.GetBuffer();
                return Convert.ToBase64String(bytes);
            }
        }

        public static string ToBase64PngString(this Image<Rgba32> image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.SaveAsPng<Rgba32>(stream);
                byte[] bytes = stream.GetBuffer();
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
