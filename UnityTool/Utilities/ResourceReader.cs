using System.IO;

using Common;

namespace UnityTool
{
    //todo : manage already opened files
    public class ResourceReader
    {
        public static byte[] GetData(string filename, long offset, long size)
        {
            if (File.Exists(filename))
            {
                using (var file = File.OpenRead(filename))
                using (var filereader = new EndianBinaryReader(file))
                {
                    filereader.Position = offset;
                    return filereader.ReadBytes((int)size);
                }
            }
            else
            {
                Debugg.Error($"the path {filename} not exist");
                return null;
            }
        }
    }
}
