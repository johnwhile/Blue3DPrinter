using System;
using System.IO;

using Common;

namespace UnityTool
{
    public partial class AssetFile
    {
        public class ObjectIdentifier
        {
            public int localSerializedFileIndex;
            public long localIdentifierInFile;

            public ObjectIdentifier(UnityFileReader reader)
            {
                localSerializedFileIndex = reader.ReadInt32();
                if (reader.Version < AssetVersion.kUnknown_14)
                    localIdentifierInFile = reader.ReadInt32();
                else
                {
                    reader.AlignStream(4);
                    localIdentifierInFile = reader.ReadInt64();
                }
            }
        }
    }
}
