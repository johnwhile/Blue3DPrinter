using System;
using System.IO;

using Common;

namespace UnityTool
{
    public partial class BundleFile
    {
        public class Header
        {
            public string signature;
            public BundleVersion version;
            public string unityVersion;
            public string unityRevision;
            public long size; //size of entire file
            public uint compressedSize;
            public uint uncompressedSize;
            public BundleFlags Flags { get; protected set; }

            public CompressionMode Compression
            {
                get => Flags.GetCompression();
                set => Flags = Flags.SetCompression(value);
            }

            public bool IsBlocksAndDirectoryInfoCombined
            {
                get => Flags.GetBlocksAndDirectoryInfoCombined();
                set => Flags = Flags.SetBlocksAndDirectoryInfoCombined(value);
            }
            public bool IsBlocksInfoAtTheEnd
            {
                get => Flags.GetBlocksInfoAtTheEnd();
            }

            public static bool Read(BinaryReader br, out Header header)
            {

                header = new Header();
                return header.Read(br);
            }

            public bool Read(BinaryReader br)
            {
                signature = br.ReadStringToNull();

                if (!signature.Equals("UnityFS"))
                {
                    Debugg.Error($"BundleFile Signature is UnityFS -> {signature}");
                    return false;
                }
                version = (BundleVersion)br.ReadUInt32();
                unityVersion = br.ReadStringToNull();
                unityRevision = br.ReadStringToNull();
                size = br.ReadInt64();
                compressedSize = br.ReadUInt32();
                uncompressedSize = br.ReadUInt32();
                Flags = (BundleFlags)br.ReadUInt32();

                //to do: remove it
                if (signature != "UnityFS") br.ReadByte();

                if (uncompressedSize > Globals.MaxCSharpArraySize)
                    throw new OverflowException($"Sorry, c# can't use buffer greater than {Globals.MaxCSharpArraySize}");

                return true;
            }

            public void Write(BinaryWriter bw)
            {
                bw.WriteStringToNull(signature);
                bw.Write((uint)version);
                bw.WriteStringToNull(unityVersion);
                bw.WriteStringToNull(unityRevision);
                bw.Write(size);
                bw.Write(compressedSize);
                bw.Write(uncompressedSize);
                bw.Write((uint)Flags);
            }



            public Header Clone()
            {
                Header copy = new Header();
                copy.signature = string.Copy(signature);
                copy.version = version;
                copy.unityVersion = string.Copy(unityVersion);
                copy.unityRevision = string.Copy(unityRevision);
                copy.size = size;
                copy.compressedSize = compressedSize;
                copy.uncompressedSize = uncompressedSize;
                copy.Flags = Flags;
                return copy;
            }

        }
    }
}
