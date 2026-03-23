using System;
using System.IO;

using Common;

using K4os.Compression.LZ4;

namespace UnityTool
{
    public static class Compressor
    {
        public static MemoryStream Decode(MemoryStream compressed, int uncompressedsize, CompressionMode mode)
        {
            //Debugg.Print($"decompression with {mode}");
            switch (mode)
            {
                default: return compressed;
                case CompressionMode.LZMA:
                    {
                        throw new NotImplementedException("For empyrion isn't necessary");
                        /*
                        var uncompressed = new MemoryStream(uncompressedsize);
                        var archive = new SevenZipExtractor.ArchiveFile(compressed, SevenZipExtractor.SevenZipFormat.Lzma);

                        Debugg.Print($"SevenZipExtractor entries = {archive.Entries.Count}");
                        if (archive.Entries.Count > 0)
                        {
                            archive.Entries[0].Extract(uncompressed);
                            uncompressed.Position = 0;
                        }
                        return uncompressed;
                        */
                    }
                case CompressionMode.LZ4:
                case CompressionMode.LZ4HC:
                    {
                        byte[] buffer = new byte[uncompressedsize];
                        int result = LZ4Codec.Decode(compressed.GetBuffer(), 0, (int)compressed.Length, buffer, 0, uncompressedsize);

                        if (result != uncompressedsize)
                        {
                            Debugg.Error("uncorrect byte writted with LZ4HC, possible corrupted");
                            return null;
                        }

                        return new MemoryStream(buffer, 0, uncompressedsize, false, true);
                    }
            }
        }

        public static MemoryStream Encode(MemoryStream uncompressed, CompressionMode mode)
        {
            Debugg.Print($"compression with {mode}");

            switch (mode)
            {
                default: return uncompressed;
                case CompressionMode.LZMA:
                case CompressionMode.LZ4:
                case CompressionMode.LZ4HC:
                    throw new NotImplementedException();
            }
        }

    }
}
