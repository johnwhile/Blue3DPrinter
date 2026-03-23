using System;
using System.IO;

using Common;

namespace UnityTool
{
    public partial class AssetFile
    {
        public class FileIdentifier
        {
            public enum AssetType : int
            {
                kNonAssetType = 0,
                kDeprecatedCachedAssetType = 1,
                kSerializedAssetType = 2,
                kMetaAssetType = 3
            }

            public Guid guid;
            public AssetType type;
            public string pathName;

            public FileIdentifier(UnityFileReader reader)
            {
                if (reader.Version >= AssetVersion.kUnknown_6)
                    reader.ReadStringToNull(); //temp empty

                if (reader.Version >= AssetVersion.kUnknown_5)
                {
                    guid = new Guid(reader.ReadBytes(16));
                    type = (AssetType)reader.ReadInt32();
                }
                pathName = reader.ReadStringToNull();
            }
        }
    }
}
