using System;
using System.IO;

using Common;

namespace UnityTool
{
    public partial class BundleFile : UnityFile
    {
        Header header;
        Hash128 Hash;
        Block[] blocksInfo;
        Node[] directoryInfo;

        /// <summary>
        /// The position in the file where blocks begin, relative to <see cref="tmp_reader"/>.<br\>
        /// <b>Is zero if file read fail</b>
        /// </summary>
        long blockBeginPosition;

        int BlocksCount => blocksInfo != null ? blocksInfo.Length : 0;

        public int FilesCount => directoryInfo != null ? directoryInfo.Length : 0;

        /// <summary>
        /// Get the full uncompressed size of all internal blocks structure
        /// </summary>
        public long BlockUncompressedSize { get; private set; }

        public string GetFilePath(int index) => directoryInfo?[index].path;


        public override void Dispose()
        {
            foreach (var block in blocksInfo)
                block.tmp_decompressed?.Dispose();
        }

        /// <summary>
        /// Read header and blockinfo, the bundle blocks are readed when you export the files with <see cref="ExportFileStream(int, string)"/>
        /// </summary>
        public bool Read(EndianBinaryReader reader)
        {
            Debugg.Info($"Reading bundle");

            reader.Position = 0;

            if (!Header.Read(reader, out header)) return false;
            if (header.version >= BundleVersion.BF_LargeFilesSupport) reader.AlignStream(16);
            if (header.Flags.GetBlockInfoNeedPaddingAtStart()) reader.AlignStream(16);

            MemoryStream blockinfostream;

            var compressed = ReadBlockInfoStream(reader);

            if (compressed == null) return false;

            blockinfostream = Compressor.Decode(compressed, (int)header.uncompressedSize, header.Compression);

            //if is not compressed, decode return same stream, if compressed you no longer need to keep the old one
            if (header.Compression != CompressionMode.NONE) compressed.Dispose();

            ReadBlockInfo(blockinfostream);

            //this is new ?
            if (header.version >= BundleVersion.BF_LargeFilesSupport) reader.AlignStream(16);

            //remember the position relative to the file tmp_reader
            blockBeginPosition = reader.Position;

            return true;
        }

        /// <summary>
        /// return the compressed block info part. If IsBlocksInfoAtTheEnd = true, <b>Stream position is resetted to the correct value.</b>
        /// </summary>
        MemoryStream ReadBlockInfoStream(EndianBinaryReader reader)
        {
            byte[] compressedBuffer;

            if (header.IsBlocksInfoAtTheEnd)
            {
                var position = reader.Position;
                reader.Position = reader.Length - header.compressedSize;
                compressedBuffer = reader.ReadBytes((int)header.compressedSize);
                reader.Position = position;
            }
            else if (header.IsBlocksAndDirectoryInfoCombined)
            {
                compressedBuffer = reader.ReadBytes((int)header.compressedSize);
            }
            else
            {
                throw new Exception($"Unknow header flag {header.Flags}");
            }
            return new MemoryStream(compressedBuffer, 0, compressedBuffer.Length, true, true);
        }

        /// <summary>
        /// Return the block info part.
        /// </summary>
        MemoryStream CreateBlockInfoStream(CompressionMode mode)
        {
            MemoryStream uncompressed = new MemoryStream();
            using (var writer = new EndianBinaryWriter(uncompressed, EndianType.BigEndian, true))
            {
                Hash.Write(writer);
                writer.Write(blocksInfo.Length);
                foreach (var block in blocksInfo) block.Write(writer);

                writer.Write(directoryInfo.Length);
                foreach (var dir in directoryInfo) dir.Write(writer);
            }

            return Compressor.Encode(uncompressed, mode);
        }

        /// <summary>
        /// read the block info part using uncompressed stream.
        /// </summary>
        void ReadBlockInfo(MemoryStream uncompressed)
        {
            using (var reader = new EndianBinaryReader(uncompressed))
            {
                Hash = new Hash128(reader);

                var blockCount = reader.ReadInt32();
                Debugg.Print($"reading {blockCount} blocks");
                blocksInfo = new Block[blockCount];
                for (int i = 0; i < blockCount; i++)
                {
                    blocksInfo[i] = Block.Read(this, reader);
                    BlockUncompressedSize += blocksInfo[i].uncompressedSize;
                    //Debugg.Print($"block {i} " + BlocksInfo[i].ToString());
                }
                var nodesCount = reader.ReadInt32();
                Debugg.Print($"reading {nodesCount} nodes");
                directoryInfo = new Node[nodesCount];
                for (int i = 0; i < nodesCount; i++)
                {
                    directoryInfo[i] = Node.Read(this, reader);
                    //Debugg.Print($"Directory {i} " + DirectoryInfo[i].ToString());
                }
            }
        }

        /// <summary>
        /// Extract data as file, that are typically very large.<br/>
        /// MemoryStream or byte[] arrat cannot be used because the size can be greater than 0x7FFFFFC7
        /// </summary>
        /// <remarks>
        /// Assuming the file is extracted only once, all decompressed blocks used only for that file
        /// (<i><see cref="Block.tmp_decompressed"/></i>) will be disposed. Instead the blocks used by multiple files will be kept
        /// and disposed only after <see cref="Close()"/>
        /// </remarks>
        /// <param name="path">folder to save</param>
        /// <returns>return the full filename</returns>
        public string ExportFileStream(EndianBinaryReader reader, int index, string path)
        {
            if (index >= FilesCount) return null;

            if (blockBeginPosition == 0)
            {
                Debugg.Error("File was not loaded or opened");
                return null;
            }
            var node = directoryInfo[index];
            string filename = Path.Combine(path, node.path);

            Debugg.Info($"Exporting bundle resource : {filename}");

            using (FileStream file = File.Create(filename))
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                int begin = 0;
                long sizesum = 0;

                while (sizesum + blocksInfo[begin].uncompressedSize <= node.offset && ++begin < blocksInfo.Length)
                    sizesum += blocksInfo[begin].uncompressedSize;

                long beginoffset = node.offset - sizesum; //byte of i-th block to read from

                int end = begin;
                while (sizesum + blocksInfo[end].uncompressedSize < (node.offset + node.size) && ++end < blocksInfo.Length)
                    sizesum += blocksInfo[begin].uncompressedSize;

                long endsize = node.offset + node.size - sizesum; //byte to read from last block

                long blockstreamoffset = 0;
                for (int i = 0; i < begin; blockstreamoffset += blocksInfo[i++].compressedSize) ;

                for (int i = begin; i <= end; i++)
                {
                    var block = blocksInfo[i];

                    //build a temporary uncompressed cache
                    if (block.tmp_decompressed == null)
                    {
                        //Debugg.Info($"prepare decompressed cache for block {i}");
                        reader.Position = blockBeginPosition + blockstreamoffset;
                        MemoryStream compressed = new MemoryStream(reader.ReadBytes((int)block.compressedSize), 0, (int)block.compressedSize, false, true);
                        if (compressed == null) return null;
                        block.tmp_decompressed = Compressor.Decode(compressed, (int)block.uncompressedSize, block.Compression);
                    }
                    blockstreamoffset += block.compressedSize;

                    byte[] source = block.tmp_decompressed.GetBuffer();

                    long sourceoffset = i == begin ? beginoffset : 0;
                    long sourcesize = block.uncompressedSize;

                    if (i == begin && i != end)
                        sourcesize = block.uncompressedSize - sourceoffset;

                    if (i == end && i != begin)
                        sourcesize = endsize;

                    if (i == begin && i == end)
                        sourcesize = endsize - sourceoffset;

                    writer.Write(source, (int)sourceoffset, (int)sourcesize);
                    writer.Flush();

                    //this block will be not used anymore for other files
                    if (sourcesize == block.uncompressedSize) block.tmp_decompressed.Dispose();
                }
            }
            Debugg.Info("done");
            reader.Position = 0;
            return filename;
        }

        /// <summary>
        /// Not implemented yet
        /// </summary>
        public void Save(string filename)
        {
            using (var file = File.Create(filename))
            {
                using (var writer = new EndianBinaryWriter(file))
                {
                    if (!header.IsBlocksAndDirectoryInfoCombined)throw new NotImplementedException("currently write blockinfo only after header");

                    header.Write(writer);
                    if (header.version >= BundleVersion.BF_LargeFilesSupport) writer.AlignStream(16);

                    var blockinfopart = CreateBlockInfoStream(header.Compression);
                    blockinfopart.WriteTo(file);

                    //this is new ?
                    if (header.version >= BundleVersion.BF_LargeFilesSupport) writer.AlignStream(16);

                    foreach (var block in blocksInfo)
                    {
                        if (block.tmp_decompressed != null) block.tmp_decompressed.WriteTo(file);
                    }

                    //update Header
                    header.size = writer.BaseStream.Length;
                    header.compressedSize = (uint)blockinfopart.Length;
                    header.uncompressedSize = (uint)blockinfopart.Length;
                    writer.BaseStream.Position = 0;
                    header.Write(writer);
                }
            }
        }

    }

}

