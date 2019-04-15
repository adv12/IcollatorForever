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

        bool HasXorImage { get; }
        bool HasAndImage { get; }

        Image<Rgba32> XorImage { get; }
        Image<Rgba32> AndImage { get; }

        string XorDataUrl { get; }
        string AndDataUrl { get; }

        void Write(Stream s);
    }
}
