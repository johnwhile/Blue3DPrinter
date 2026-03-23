using System;
using System.IO;

using Common;

namespace UnityTool
{
    public partial class AssetFile
    {
        /// <summary>
        /// Serialized Type
        /// </summary>
        public class AssetType
        {
            public ClassIDType classID;
            public bool isStrippedType;
            public short ScriptTypeIndex = -1;
            public AssetTree type;
            public Hash128 scriptID;
            public Hash128 oldTypeHash;
            public int[] typeDependencies;
            public string klassName;
            public string nameSpace;
            public string asmName;

            public AssetType(UnityFileReader reader, bool enableTree, bool isRefType)
            {
                var version = reader.Version;

                classID = (ClassIDType)reader.ReadInt32();

                if (version >= AssetVersion.kRefactoredClassId)
                    isStrippedType = reader.ReadBoolean();
                
                if (version >= AssetVersion.kRefactorTypeData)
                    ScriptTypeIndex = reader.ReadInt16();
                
                if (version >= AssetVersion.kHasTypeTreeHashes)
                {
                    if (isRefType && ScriptTypeIndex >= 0)
                        scriptID = new Hash128(reader);
                    else if ((version < AssetVersion.kRefactoredClassId && classID < 0) || (version >= AssetVersion.kRefactoredClassId && (int)classID == 114))
                        scriptID = new Hash128(reader);
                    oldTypeHash = new Hash128(reader);
                }

                if (enableTree)
                {
                    type = new AssetTree();
                    if (!type.Read(reader,version)) return;

                    if (version >= AssetVersion.kStoresTypeDependencies)
                    {
                        if (isRefType)
                        {
                            klassName = reader.ReadStringToNull();
                            nameSpace = reader.ReadStringToNull();
                            asmName = reader.ReadStringToNull();
                        }
                        else
                        {
                            int count = reader.ReadInt32();
                            typeDependencies = reader.ReadInt32Array(count);
                        }
                    }
                }
            }
        }
    }
}
