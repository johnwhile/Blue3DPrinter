using System;
using System.IO;

using Common;

namespace UnityTool
{
    public partial class AssetFile
    {
        public class ObjectInfo
        {
            //owner
            internal readonly AssetFile asset;
            internal readonly int index;


            public BuildVersion UnityBuild => asset.Build;

            /// <summary>
            /// <b>Remember to add <see cref="Header.DataOffset"/></b>
            /// </summary>
            public long byteStart;
            public uint byteSize;
            public int typeID;
            public ClassIDType classID;
            public ushort isDestroyed;
            public byte stripped;
            public long PathID;

            public AssetType SerializedType;
            public short ScriptTypeIndex = 0;

            public ObjectInfo(AssetFile asset, int index)
            {
                this.asset = asset;
                this.index = index;
            }


            public bool Read(BinaryReader reader, AssetVersion version, bool enableBigId)
            {
                if (enableBigId) 
                    PathID = reader.ReadInt64();
                else if (version < AssetVersion.kUnknown_14)
                   PathID = reader.ReadInt32();   
                else
                {
                    reader.AlignStream(4);
                    PathID = reader.ReadInt64();
                }

                if (version >= AssetVersion.kLargeFilesSupport)
                    byteStart = reader.ReadInt64();
                else
                    byteStart = reader.ReadUInt32();

                byteSize = reader.ReadUInt32();
                typeID = reader.ReadInt32();

                if (version < AssetVersion.kRefactoredClassId) 
                    classID = (ClassIDType)reader.ReadUInt16();

                if (version < AssetVersion.kHasScriptTypeIndex) 
                    isDestroyed = reader.ReadUInt16();

                if (version >= AssetVersion.kHasScriptTypeIndex && version < AssetVersion.kRefactorTypeData)
                    ScriptTypeIndex = reader.ReadInt16();

                if (version == AssetVersion.kSupportsStrippedObject || version == AssetVersion.kRefactoredClassId) 
                    stripped = reader.ReadByte();
                
                return true;
            }

            public override string ToString()
            {
                return $"{classID} pathid:{PathID}";
            }
        }
    }
}
