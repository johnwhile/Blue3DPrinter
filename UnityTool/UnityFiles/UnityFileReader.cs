using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace UnityTool
{
    public enum FileType
    {
        AssetsFile,
        BundleFile,
        WebFile,
        ResourceFile,
        GZipFile,
        BrotliFile
    }

    public class UnityFileReader : EndianBinaryReader
    {
        /// <summary>
        /// Get full folder
        /// </summary>
        public string FilePath;
        /// <summary>
        /// Get filename with extension
        /// </summary>
        public string FileName;
        public FileType FileType;

        public AssetFile Asset;
        public BuildTarget Target => Asset.Target;
        public BuildVersion Build => Asset.Build;
        public AssetVersion Version => Asset.Version;

        public AssetFile.ObjectInfo ObjectInfo;

        private static readonly byte[] gzipMagic = { 0x1f, 0x8b };
        private static readonly byte[] brotliMagic = { 0x62, 0x72, 0x6F, 0x74, 0x6C, 0x69 };

        public UnityFileReader(string path) : this(path, File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

        public UnityFileReader(string path, Stream stream) : base(stream, EndianType.BigEndian)
        {
            FilePath = Path.GetDirectoryName(path);
            FileName = Path.GetFileName(path);
            FileType = CheckFileType();
        }

        public bool LoadFile(out AssetFile assetfile)
        {
            if (FileType != FileType.AssetsFile) throw new Exception("Not a asset file");
            Asset = assetfile = new AssetFile();
            if (!Asset.Read(this)) return false;
            return true;
        }

        public bool LoadFile(out BundleFile file)
        {
            if (FileType != FileType.BundleFile) throw new Exception("Not a bundle file");

            file = new BundleFile();
            if (!file.Read(this)) return false;
            return true;
        }


        FileType CheckFileType()
        {
            var signature = this.ReadStringToNull(20);
            Position = 0;
            switch (signature)
            {
                case "UnityWeb":
                case "UnityRaw":
                case "UnityArchive":
                case "UnityFS":
                    return FileType.BundleFile;
                case "UnityWebData1.0":
                    return FileType.WebFile;
                default:
                    {
                        var magic = ReadBytes(2);
                        Position = 0;
                        if (gzipMagic.SequenceEqual(magic))
                            return FileType.GZipFile;
                        
                        Position = 0x20;
                        magic = ReadBytes(6);
                        Position = 0;
                        if (brotliMagic.SequenceEqual(magic)) 
                            return FileType.BrotliFile;
                        
                        return IsSerializedFile() ? 
                            FileType.AssetsFile : 
                            FileType.ResourceFile;
                        
                    }
            }
        }

        bool IsSerializedFile()
        {
            var header = new AssetFile.Header(this, out bool success);
            Position = 0;
            return success;

            /*
            var m_MetadataSize = ReadUInt32();
            long m_FileSize = ReadUInt32();
            var m_Version = ReadUInt32();
            long m_DataOffset = ReadUInt32();
            var m_Endianess = ReadByte();
            var m_Reserved = ReadBytes(3);
            
            if (m_Version >= 22)
            {
                if (fileSize < 48)
                {
                    Position = 0;
                    return false;
                }
                m_MetadataSize = ReadUInt32();
                m_FileSize = ReadInt64();
                m_DataOffset = ReadInt64();
            }
            Position = 0;

            if (m_FileSize != fileSize) return false;
            if (m_DataOffset > fileSize) return false;
            return true;
            */
        }

        /// <summary>
        /// Create array of class with constructor:
        /// <code>class(<see cref="UnityFileReader"/> reader) {}</code>
        /// </summary>
        public bool CreateInstanceArray<T>(out T[] array)
        {
            array = new T[ReadInt32()];
            for (int i = 0; i < array.Length; i++)
                array[i] = (T)Activator.CreateInstance(typeof(T), this);
            
            return true;
        }

    }
}
