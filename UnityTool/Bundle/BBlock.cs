using System;
using System.IO;

namespace UnityTool
{
    public partial class BundleFile
    {
        private class Block
        {
            readonly BundleFile bundle;

            public uint uncompressedSize;
            public uint compressedSize;
            public ushort flags;

            public Block(BundleFile bundle)
            {
                this.bundle = bundle;
            }

            /// <summary>
            /// temporary stream
            /// </summary>
            public MemoryStream tmp_decompressed;

            public CompressionMode Compression
            {
                get => (CompressionMode)(flags & (ushort)BundleFlags.CompressionMask);
                set => flags = (ushort)(flags & ~(ushort)BundleFlags.CompressionMask | (ushort)value);
            }

            public bool WriteTo(BinaryReader reader, MemoryStream destination)
            {

                return true;
            }

            public static Block Read(BundleFile bundle, BinaryReader br)
            {
                var block = new Block(bundle);
                block.uncompressedSize = br.ReadUInt32();
                block.compressedSize = br.ReadUInt32();
                block.flags = br.ReadUInt16();

                if (block.uncompressedSize > Globals.MaxCSharpArraySize)
                    throw new OverflowException($"Sorry, c# can't use buffer greater than {Globals.MaxCSharpArraySize}");

                return block;
            }
            public void Write(BinaryWriter bw)
            {
                bw.Write(uncompressedSize);
                bw.Write(compressedSize);
                bw.Write(flags);
            }
            public Block Clone(BundleFile bundle)
            {
                var copy = new Block(bundle);
                copy.uncompressedSize = uncompressedSize;
                copy.compressedSize = compressedSize;
                copy.flags = flags;

                if (tmp_decompressed != null)
                {
                    copy.tmp_decompressed = new MemoryStream(tmp_decompressed.ToArray());
                }
                return copy;
            }

            public override string ToString()
            {
                return $"size from {compressedSize} to {uncompressedSize}";
            }
        }
    }
}
