using System.Diagnostics;
using Common;
using System.IO;
using System;

namespace UnityTool
{
    [DebuggerDisplay("{DebugString}")]
    public sealed class PPtr<T> where T : ObjectBase
    {
        AssetFile asset;
        int index = -2; //-2 - Prepare, -1 - Missing
        public int FileID;
        public long PathID;

        public bool IsNull => PathID == 0 || FileID < 0;

        public PPtr(UnityFileReader reader)
        {
            FileID = reader.ReadInt32();
            PathID = reader.Version < AssetVersion.kUnknown_14 ? reader.ReadInt32() : reader.ReadInt64();
            asset = reader.Asset;
        }

        public string DebugString
        {
            get
            {
                if (asset.ObjectDictionary.TryGetValue(PathID, out int index))
                {
                    var info = asset.ObjectInfos[index];
                    return info.ToString();
                }
                return "Not-Found";
            }
        }


        public bool TryGetObjectInfo(UnityFileReader reader, out AssetFile.ObjectInfo info)
        {
            info = null;

            //find the file using FileID
            if (TryGetAssetsFile(out var sourceFile))
            {
                //find object using PathID
                if (sourceFile.ObjectDictionary.TryGetValue(PathID, out var index))
                {
                    info = sourceFile.ObjectInfos[index];
                    return true;
                }
                else
                {
                    Debugg.Error($"The file path {PathID} not found");
                }
            }
            else
            {
                Debugg.Error($"The fileid {FileID} can't be load because it's an external file");
            }
            return false;
        }


        /// <summary>
        /// Get <see cref="Component"/> derived objects
        /// </summary>
        /// <param name="reader">not implemented multi-asset reader</param>
        public bool TryGet(UnityFileReader reader, out T result)
        {
            result = null;

            if (!ClassIDTool.TryGetClassID<T>(out var requested))
                throw new NotImplementedException($"Cannot get component for type {typeof(T)} because class is not implemented");

            if (TryGetObjectInfo(reader, out var info))
            {
                if (info.classID != requested)
                {
                    Debugg.Error($"The returned class {info.classID} doesn't match with requested {requested}");
                    return false;
                }
                if (info.asset != asset) throw new NotImplementedException("Multi-Asset-reader not implemented");

                var obj = info.asset.GetObjectByIndex(reader, info.index);

                if (obj is T castobj)
                {
                    result = castobj;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// return the assetfile where data are stored. Can be found from an externals files
        /// </summary>
        bool TryGetAssetsFile(out AssetFile result)
        {
            result = null;

            if (FileID == 0)
            {
                result = asset;
                return true;
            }

            if (FileID > 0 && FileID - 1 < asset.Externals.Length)
            {
                //var assetsManager = assetsFile.assetsManager;
                //var assetsFileList = assetsManager.assetsFileList;
                //var assetsFileIndexCache = assetsManager.assetsFileIndexCache;

                if (index == -2) //prepare
                {
                    var external = asset.Externals[FileID - 1];
                    var name = Path.GetFileName(external.pathName);

                    Debugg.Warning($"require find external file {name}");

                    
                    /*
                    if (!assetsFileIndexCache.TryGetValue(name, out index))
                    {
                        index = assetsFileList.FindIndex(x => x.fileName.Equals(name, StringComparison.OrdinalIgnoreCase));
                        assetsFileIndexCache.Add(name, index);
                    }*/
                }

                if (index >= 0)
                {
                    //result = assetsFileList[index];
                    //return true;
                }
            }


            return false;
        }
        

        /*
        public bool TryGet<T2>(out T2 result) where T2 : ObjectBase
        {
            if (TryGetAssetsFile(out var sourceFile))
                if (sourceFile.ObjectDictionary.TryGetValue(m_PathID, out var obj))
                {
                    if (obj is T2 variable)
                    {
                        result = variable;
                        return true;
                    }
                }
            
            result = null;
            return false;
        }
        */

        /*
        public void Set(T m_Object)
        {
            var name = m_Object.assetsFile.fileName;
            if (string.Equals(assetsFile.fileName, name, StringComparison.OrdinalIgnoreCase))
            {
                m_FileID = 0;
            }
            else
            {
                m_FileID = assetsFile.m_Externals.FindIndex(x => string.Equals(x.fileName, name, StringComparison.OrdinalIgnoreCase));
                if (m_FileID == -1)
                {
                    assetsFile.m_Externals.Add(new FileIdentifier
                    {
                        fileName = m_Object.assetsFile.fileName
                    });
                    m_FileID = assetsFile.m_Externals.Count;
                }
                else
                {
                    m_FileID += 1;
                }
            }

            var assetsManager = assetsFile.assetsManager;
            var assetsFileList = assetsManager.assetsFileList;
            var assetsFileIndexCache = assetsManager.assetsFileIndexCache;

            if (!assetsFileIndexCache.TryGetValue(name, out index))
            {
                index = assetsFileList.FindIndex(x => x.fileName.Equals(name, StringComparison.OrdinalIgnoreCase));
                assetsFileIndexCache.Add(name, index);
            }

            m_PathID = m_Object.m_PathID;
        }
        */

    }
}
