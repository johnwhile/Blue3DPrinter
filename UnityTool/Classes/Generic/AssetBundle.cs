using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Common;

namespace UnityTool
{
    public class AssetInfo
    {
        public int preloadIndex;
        public int preloadSize;
        public PPtr<ObjectBase> assetObj;

        public AssetInfo(UnityFileReader reader)
        {
            preloadIndex = reader.ReadInt32();
            preloadSize = reader.ReadInt32();
            assetObj = new PPtr<ObjectBase>(reader);
        }
    }

    public sealed class AssetBundle : NamedObject
    {
        public PPtr<ObjectBase>[] m_PreloadTable;
        public KeyValuePair<string, AssetInfo>[] m_Container;

        public AssetBundle(UnityFileReader reader) : base(reader)
        {
            var m_PreloadTableSize = reader.ReadInt32();
            m_PreloadTable = new PPtr<ObjectBase>[m_PreloadTableSize];
            
            for (int i = 0; i < m_PreloadTableSize; i++)
                m_PreloadTable[i] = new PPtr<ObjectBase>(reader);
            

            var m_ContainerSize = reader.ReadInt32();
            m_Container = new KeyValuePair<string, AssetInfo>[m_ContainerSize];
            
            for (int i = 0; i < m_ContainerSize; i++)
                m_Container[i] = new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader));
            
        }
    }
}
