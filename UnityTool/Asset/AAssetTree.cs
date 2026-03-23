using System;
using System.Collections.Generic;
using System.IO;

using Common;

namespace UnityTool
{
    public partial class AssetFile
    {
        public class AssetTree
        {
            const uint bit32 = ((uint)1 << 31);

            public List<AssetTreeNode> Nodes;
            public byte[] StringBuffer;

            public AssetTree()
            {
                Nodes = new List<AssetTreeNode>();
            }

            public bool Read(BinaryReader reader, AssetVersion version)
            {
                if (version >= AssetVersion.kUnknown_12 || version == AssetVersion.kUnknown_10)
                {
                    if (!ReadBlobTree(reader, version)) return false;
                }
                else
                {
                    if (!ReadRecursiveTree(reader, version)) return false;
                }
                return true;
            }

            bool ReadBlobTree(BinaryReader reader, AssetVersion version)
            {
                int numberOfNodes = reader.ReadInt32();
                int stringBufferSize = reader.ReadInt32();
                
                Nodes.Clear();
                for (int i = 0; i < numberOfNodes; i++)
                {
                    var node = new AssetTreeNode();
                    if (!node.ReadBob(reader, version)) return false;
                    Nodes.Add(node);
                }

                StringBuffer = reader.ReadBytes(stringBufferSize);
                using (var stringBufferReader = new BinaryReader(new MemoryStream(StringBuffer)))
                {
                    for (int i = 0; i < numberOfNodes; i++)
                    {
                        var node = Nodes[i];
                        node.m_Type = ReadString(stringBufferReader, node.m_TypeStrOffset);
                        node.m_Name = ReadString(stringBufferReader, node.m_NameStrOffset);
                    }
                }

                return true;
            }
            bool ReadRecursiveTree(BinaryReader reader, AssetVersion version, int level = 0)
            {
                var node = new AssetTreeNode();
                node.ReadRecursive(reader, version, level);
                Nodes.Add(node);

                int childrenCount = reader.ReadInt32();
                for (int i = 0; i < childrenCount; i++)
                    if (!ReadRecursiveTree(reader, version, level + 1)) return false;
                return true;
            }

            string ReadString(BinaryReader reader, uint value)
            {
                var isOffset = (value & bit32) == 0;
                
                if (isOffset)
                {
                    reader.BaseStream.Position = value;
                    return reader.ReadStringToNull();
                }
                var offset = value & ~bit32;
                if (CommonString.StringBuffer.TryGetValue(offset, out var str)) return str;
                
                return offset.ToString();
            }

        }

        public class AssetTreeNode
        {
            public string m_Type;
            public string m_Name;
            public int m_ByteSize;
            public int m_Index;
            public int m_TypeFlags; //m_IsArray
            public int m_Version;
            public int m_MetaFlag;
            public int m_Level;
            public uint m_TypeStrOffset;
            public uint m_NameStrOffset;
            public ulong m_RefTypeHash;


            public AssetTreeNode()
            {

            }

            public AssetTreeNode(string type, string name, int level, bool align)
            {
                m_Type = type;
                m_Name = name;
                m_Level = level;
                m_MetaFlag = align ? 0x4000 : 0;
            }

            public bool ReadBob(BinaryReader reader, AssetVersion version)
            {
                m_Version = reader.ReadUInt16();
                m_Level = reader.ReadByte();
                m_TypeFlags = reader.ReadByte();
                m_TypeStrOffset = reader.ReadUInt32();
                m_NameStrOffset = reader.ReadUInt32();
                m_ByteSize = reader.ReadInt32();
                m_Index = reader.ReadInt32();
                m_MetaFlag = reader.ReadInt32();
                
                if (version >= AssetVersion.kTypeTreeNodeWithTypeFlags)
                    m_RefTypeHash = reader.ReadUInt64();

                return true;
            }

            public bool ReadRecursive(BinaryReader reader, AssetVersion version, int level)
            {
                m_Level = level;
                m_Type = reader.ReadStringToNull();
                m_Name = reader.ReadStringToNull();
                m_ByteSize = reader.ReadInt32();
                
                if (version == AssetVersion.kUnknown_2)
                    reader.ReadInt32(); //variableCount

                if (version != AssetVersion.kUnknown_3)
                    m_Index = reader.ReadInt32();
                
                m_TypeFlags = reader.ReadInt32();
                m_Version = reader.ReadInt32();
                
                if (version != AssetVersion.kUnknown_3)
                    m_MetaFlag = reader.ReadInt32();
                
                return true;
            }


        }

    }
}
