
using Common;

namespace UnityTool
{
    public partial class AssetFile
    {
        public class Header
        {
            public uint MetadataSize;
            public long FileSize;
            public AssetVersion Version;
            public long DataOffset;
            public EndianType Endianess = EndianType.LittleEndian;
            public byte[] Reserved;

            /// <summary>
            /// </summary>
            /// <param name="success">header can be used to validate the file</param>
            public Header(EndianBinaryReader reader, out bool success)
            {
                success = false;
                var fileSize = reader.Length;
                if (fileSize < 20) return;

                MetadataSize = reader.ReadUInt32();
                FileSize = reader.ReadUInt32();
                Version = (AssetVersion)reader.ReadUInt32();
                DataOffset = reader.ReadUInt32();

                if (Version >= AssetVersion.kUnknown_9)
                {
                    if (reader.ReadBoolean()) Endianess = EndianType.BigEndian;
                    Reserved = reader.ReadBytes(3);
                }
                else
                {
                    reader.Position = FileSize - MetadataSize;
                    if (reader.ReadBoolean()) Endianess = EndianType.BigEndian;
                }

                if (Version >= AssetVersion.kLargeFilesSupport)
                {
                    if (fileSize < 48) return;
                    
                    MetadataSize = reader.ReadUInt32();
                    FileSize = reader.ReadInt64();
                    DataOffset = reader.ReadInt64();
                    reader.ReadInt64(); // unknown
                }

                if (FileSize != fileSize || DataOffset > fileSize) return;

                success = true;
            }
        }
    }
}
