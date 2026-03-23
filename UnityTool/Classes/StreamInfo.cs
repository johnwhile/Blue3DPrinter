using System;
using System.IO;
using Common;

namespace UnityTool
{
    public class StreamingInfo
    {
        public long offset; //ulong
        public uint size;
        public string path;

        public string Filename => Path.GetFileName(path);

        public StreamingInfo(UnityFileReader reader)
        {
            offset =  reader.Build.IsGreaterEqual(2020) ?
                reader.ReadInt64() ://2020.1 and up
                reader.ReadUInt32(); 

            size = reader.ReadUInt32();
            path = reader.ReadAlignedString();
        }
    }
}
