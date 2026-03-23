using System.IO;

using Common;

namespace UnityTool
{
    public partial class BundleFile
    {
        private class Node
        {
            readonly BundleFile bundle;

            public long offset;
            public long size;
            public uint flags;
            public string path;

            public Node(BundleFile bundle)
            {
                this.bundle = bundle;
            }


            public static Node Read(BundleFile bundle, BinaryReader br)
            {
                var node = new Node(bundle);

                node.offset = br.ReadInt64();
                node.size = br.ReadInt64();
                node.flags = br.ReadUInt32();
                node.path = br.ReadStringToNull();
                return node;
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(offset);
                bw.Write(size);
                bw.Write(flags);
                bw.WriteStringToNull(path);
            }

            public Node Clone(BundleFile bundle)
            {
                var copy = new Node(bundle);
                copy.offset = offset;
                copy.size = size;
                copy.flags = flags;
                copy.path = string.Copy(path);
                return copy;
            }
            public override string ToString()
            {
                return $"offset {offset} size {size} path {path}";
            }
        }
    }
}
