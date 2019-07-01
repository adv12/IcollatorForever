﻿// Copyright (c) 2019 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the IcollatorForever distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace IcollatorForever
{
    public class Icon : IDisposable
    {
        private IIconEntry[] _entries;
        private Stream _stream;
        private EndianBinaryReader _reader;

        /// <summary>
        /// Gets the the list of entry descriptions read from the icon's header
        /// </summary>
        public IconEntryDescription[] EntryDescriptions { get; }

        /// <summary>
        /// Ensures that the lazily-loaded entries are all fully initialized
        /// and returns the list
        /// </summary>
        public IIconEntry[] Entries
        {
            get
            {
                for (int i = 0; i < _entries.Length; i++)
                {
                    GetEntry(i);
                }
                return _entries;
            }
        }

        /// <summary>
        /// Creates a new Icon from a stream
        /// </summary>
        public Icon(string filename, Stream s)
        {
            _stream = s;
            _reader = new EndianBinaryReader(EndianBitConverter.Little, s);

            _reader.ReadInt16(); // reserved field
            _reader.ReadInt16(); // type field
            int count = _reader.ReadInt16();
            EntryDescriptions = new IconEntryDescription[count];
            _entries = new IIconEntry[count];
            List<IIconEntry> list = new List<IIconEntry>();
            for (int i = 0; i < count; i++)
            {
                int width = s.ReadByte();
                int height = s.ReadByte();
                int colorCount = s.ReadByte();
                int reserved2 = s.ReadByte();
                int planes = _reader.ReadInt16();
                int bitCount = _reader.ReadInt16();
                int sizeInBytes = _reader.ReadInt32();
                int fileOffset = _reader.ReadInt32();
                if (width == 0)
                {
                    width = 256;
                }
                if (height == 0)
                {
                    height = 256;
                }
                IconEntryDescription description = new IconEntryDescription(
                    width, height, colorCount, (byte)reserved2, planes, bitCount,
                    sizeInBytes, fileOffset, filename, i);
                EntryDescriptions[i] = description;
            }
        }

        /// <summary>
        /// Gets a lazily-loaded, fully-initialized IconEntry for the specified index.  If the
        /// stream being used can't seek, will read all entries preceding the one requested so
        /// they are accessible for later use.  If the stream can seek, will instantiate only
        /// the entry requested for lower memory use.
        /// </summary>
        public IIconEntry GetEntry(int index)
        {
            if (_entries[index] == null)
            {
                if (_stream.CanSeek)
                {
                    _entries[index] = new IcoIconEntry(EntryDescriptions[index], _stream);
                }
                else
                {
                    for (int i = 0; i <= index; i++)
                    {
                        _entries[index] = new IcoIconEntry(EntryDescriptions[index], _stream);
                    }
                }
            }
            return _entries[index];
        }

        /// <summary>
        /// Gets an entry matching the specified description. The description should match
        /// one of the elements of EntryDescriptions.
        /// </summary>
        public IIconEntry GetEntry(IconEntryDescription description)
        {
            if (description == null)
            {
                return null;
            }
            int index = Array.IndexOf(EntryDescriptions, description);
            if (index == -1)
            {
                return null;
            }
            return GetEntry(EntryDescriptions[index]);
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _reader?.Dispose();
        }
    }
}
