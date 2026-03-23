using Common;
using System;
using System.IO;
using System.Linq;

namespace UnityTool
{
    // All these methods must be deleted, they was used only for testing
#if DEBUG
    public partial class BundleFile
    {
        public BundleFile()
        {
        }

        public static BundleFile Debug_LoadAndSaveUncompressed(string filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);

            BundleFile bundle = new BundleFile();
            MemoryStream blockstream;

            using (var file = File.OpenRead(filename))
            using (var reader = new EndianBinaryReader(file, EndianType.BigEndian))
                bundle.Debug_Read(reader, out _, out blockstream);

            Console.WriteLine("copying");
            var copy = bundle.Clone();
            //currently i'm working with this flag
            copy.header.IsBlocksAndDirectoryInfoCombined = true;
            //to read with hex editor the first structure must be decoded
            copy.header.Compression = CompressionMode.NONE;

            copy.Debug_Save(Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "_unpack.bundle", blockstream);


            return copy;
        }

        /// <summary>
        /// </summary>
        /// <param name="blockinfostream">optional : return the true uncompressed blockinfo section</param>
        void Debug_Read(EndianBinaryReader reader, out MemoryStream blockinfostream, out MemoryStream rest)
        {
            Header.Read(reader, out header);
            if (header.version >= BundleVersion.BF_LargeFilesSupport) reader.AlignStream(16);

            var compressed = ReadBlockInfoStream(reader);
            blockinfostream = Compressor.Decode(compressed, (int)header.uncompressedSize, header.Compression);
            if (header.Compression != CompressionMode.NONE) compressed.Dispose();
            ReadBlockInfo(blockinfostream);

            blockinfostream.Dispose();
            blockinfostream = null;
            if (header.version >= BundleVersion.BF_LargeFilesSupport) reader.AlignStream(16);

            foreach (var block in blocksInfo)
            {
                byte[] buffer = reader.ReadBytes((int)block.compressedSize);
                MemoryStream blockstream = new MemoryStream(buffer, 0, buffer.Length, true, true);
                block.tmp_decompressed = Compressor.Decode(blockstream, (int)block.uncompressedSize, block.Compression);
                block.Compression = CompressionMode.NONE;
                block.compressedSize = block.uncompressedSize = (uint)block.tmp_decompressed.Length;
            }


            long restsize = reader.Length - reader.Position;

            if (restsize > int.MaxValue)
                throw new OverflowException($"Sorry, c# can't use buffer greater than {Globals.MaxCSharpArraySize}");

            rest = new MemoryStream(reader.ReadBytes((int)restsize));
        }

        public byte[] Debug_ExportFileStream(int index, MemoryStream prebuildedblocks, out string path)
        {
            var node = directoryInfo[index];
            path = node.path;
            Debugg.Warning($"export file {path}");
            byte[] buffer = new byte[node.size];
            Buffer.BlockCopy(prebuildedblocks.GetBuffer(), (int)node.offset, buffer, 0, (int)node.size);
            return buffer;
        }

        void Debug_Save(string filename, MemoryStream rest)
        {
            using (var file = File.Create(filename))
            {
                using (var writer = new EndianBinaryWriter(file))
                {
                    header.Write(writer);
                    if (header.version >= BundleVersion.BF_LargeFilesSupport) writer.AlignStream(16);

                    var blockinfopart = CreateBlockInfoStream(header.Compression);
                    blockinfopart.WriteTo(file);

                    if (header.version >= BundleVersion.BF_LargeFilesSupport) writer.AlignStream(16);

                    foreach (var block in blocksInfo)
                        if (block.tmp_decompressed != null) block.tmp_decompressed.WriteTo(file);

                    rest.WriteTo(file);

                    //update header
                    header.size = writer.BaseStream.Length;
                    header.compressedSize = (uint)blockinfopart.Length;
                    header.uncompressedSize = header.compressedSize;
                    writer.BaseStream.Position = 0;
                    header.Write(writer);
                }
            }
        }

        /// <summary>
        /// Uncompress all blocks at once and build a single stream.<br/>
        /// It'll be used for <b><see cref="GetFileStream(int, MemoryStream, out string)"/></b>
        /// </summary>
        public bool Debug_CreateUncompressedBlockStream(EndianBinaryReader reader, out MemoryStream BlockStream, out long Size)
        {
            Size = blocksInfo.Sum(x => x.uncompressedSize);
            if (Size > int.MaxValue) throw new NotSupportedException($"can't manage bytes array greater than {int.MaxValue}");

            BlockStream = new MemoryStream((int)Size);

            long blockoffset = 0;
            for (int i = 0; i < blocksInfo.Length; i++)
            {
                var block = blocksInfo[i];
                Debugg.Info($"decompressed block {i}");

                reader.Position = blockBeginPosition + blockoffset;

                MemoryStream compressed = new MemoryStream(reader.ReadBytes((int)block.compressedSize), 0, (int)block.compressedSize, true, true);
                MemoryStream uncompressed = Compressor.Decode(compressed, (int)block.uncompressedSize, block.Compression);

                uncompressed.WriteTo(BlockStream);

                blockoffset += block.compressedSize;
            }
            return true;
        }

        public BundleFile Clone()
        {
            var copy = new BundleFile();
            copy.header = header.Clone();
            copy.blocksInfo = new Block[blocksInfo.Length];
            for (int i = 0; i < blocksInfo.Length; i++) copy.blocksInfo[i] = blocksInfo[i].Clone(copy);
            copy.directoryInfo = new Node[directoryInfo.Length];
            for (int i = 0; i < directoryInfo.Length; i++) copy.directoryInfo[i] = directoryInfo[i].Clone(copy);
            return copy;
        }
    }
#endif
}

