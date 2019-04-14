// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace IcollatorForever
{
    public interface IIconEntry
    {
        IconEntryDescription Description { get; }

        bool HasAndImage { get; }
        bool HasXorImage { get; }

        Image<Rgba32> AndImage { get; }
        Image<Rgba32> XorImage { get; }

        void Write(Stream s);
    }
}
