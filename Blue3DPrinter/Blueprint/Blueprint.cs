using System;
using System.IO;

using Common;
using System.Diagnostics;
using Common.Maths;

namespace Blue3DPrinter
{
    public class Blueprint
    {
        public string Filename { get; set; }
        /// <summary>
        /// the Header is not compressed, contain statistic, version, size (very important) and other unknow things
        /// </summary>
        public BlueprintHeader Header { get; private set; }
        /// <summary>
        /// contain the block structure, is compressed
        /// </summary>
        public BlueprintRest MetaData { get; private set; }

        public BlockCollection Blocks { get; private set; }

        /// <summary>
        /// the purpose is only to read the blueprint and not create a new instance because the writing methods will never be implemented, maybe...
        /// </summary>
        internal Blueprint() 
        {
            Header = new BlueprintHeader(this);
            MetaData = new BlueprintRest(this);
            Blocks = new BlockCollection(this);
        }
        internal Blueprint(Vector3i capacity)
        {
            Header = new BlueprintHeader(this);
            MetaData = new BlueprintRest(this);
            Blocks = new BlockCollection(this, capacity);
        }


        /// <summary>
        /// </summary>
        /// <param name="asUncompressed">for debug purpose, save as epbx</param>
        /// <returns></returns>
        public bool Save(string filename, bool asUncompressed = false)
        {
            filename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)) + (asUncompressed ? ".epbx" : ".epb");
            LogMsg.Message("> saving " + Path.GetFileName(filename), ConsoleColor.Yellow);

            using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    if (!Header.Write(writer)) return false;
                    if (MetaData == null) return false;
                    var restStream = MetaData.GetMetadataStream();
                    if (restStream == null || restStream.Length == 0) return false;

                    if (!asUncompressed) restStream = BlueprintRest.Compressing(restStream);

                    BlueprintRest.WriteCompressed(writer, restStream.ToArray(), asUncompressed);
                }
            }

            return true;
        }


        /// <summary>
        /// Load a blueprint.epb file. To generate the 3d model, the BlockConfig.ecf file must be correctly loaded.
        /// </summary>
        public static Blueprint Open(string filename)
        {
            LogMsg.Message("> opening " + Path.GetFileName(filename), ConsoleColor.Yellow);
            Blueprint blueprint = new Blueprint();
            blueprint.Filename = filename;

            if (blueprint.Load(filename))
            {
                LogMsg.Message("> done, tot blocks = " + blueprint.Blocks.Count, ConsoleColor.Green);
                return blueprint;
            }
            return null;
        }

        /// <summary>
        /// purely for investigation purposes, will be removed.
        /// </summary>
        /// <remarks>the uncompressed blueprint file are generate to work with 010Editor templates to investigate the binary data structure</remarks>
        public static bool ConvertToUncompressed(string filename)
        {
            LogMsg.Message("> converting to my uncompressed binary format " + Path.GetFileName(filename), ConsoleColor.Yellow);
            Blueprint blueprint = new Blueprint();

            string unzipfilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".epbx");

            try
            {
                using (FileStream inputStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(inputStream))
                using (Stream outputStream = File.Open(unzipfilename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    blueprint.Header = new BlueprintHeader(blueprint);

                    //header must be readed to know start and end position
                    if (!blueprint.Header.Read(reader))
                    {
                        LogMsg.Error("> error reading header at : " + reader.BaseStream.Position);
                        return false;
                    }
                    blueprint.Header.CopyStreamTo(reader, outputStream);

                    MemoryStream partStream = BlueprintRest.GetCompressedPart(reader);
                    if (partStream==null)
                    {
                        LogMsg.Message("> error reading metadata", ConsoleColor.Red);
                        return false;
                    }
                    partStream = BlueprintRest.Uncompressing(partStream);
                    if (partStream == null)
                    {
                        LogMsg.Message("> error uncompressing metadata", ConsoleColor.Red);
                        return false;
                    }
                    outputStream.Write(partStream.ToArray(), 0, (int)partStream.Length);
                }
                LogMsg.Message("> done", ConsoleColor.Green);
                return true;
            }
            catch (Exception e)
            {
                Console.Write("> error writing stream: " + e.Message.ToString());
                return false;
            }
        }
        /// <summary>
        /// purely for investigation purposes, convert a epb into text file.
        /// </summary>
        public static bool ConvertToText(string filename)
        {
            LogMsg.Message("> converting to my txt format " + Path.GetFileName(filename), ConsoleColor.Yellow);
            Blueprint blueprint = new Blueprint();

            string txtfilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".txt");

            try
            {
                using (FileStream inputStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(inputStream))
                using (Stream outputStream = File.Open(txtfilename, FileMode.Create, FileAccess.Write, FileShare.None))
                using (TextWriter tw = new StreamWriter(outputStream))
                {
                    blueprint.Header = new BlueprintHeader(blueprint);

                    //header must be readed to know start and end position
                    if (!blueprint.Header.Read(reader))
                    {
                        LogMsg.Error("> error reading header at : " + reader.BaseStream.Position);
                        return false;
                    }

                    blueprint.Header.Write(tw);

                    //blueprint.Header.CopyStreamTo(reader, outputStream);

                    tw.WriteLine("\n//+---------------------------+");
                    tw.WriteLine("//| uncompressed part of file |");
                    tw.WriteLine("//+---------------------------+");

                    MemoryStream partStream = BlueprintRest.GetCompressedPart(reader);

                    tw.WriteLine("bytes compressed :\t" + partStream.Length);

                    if (partStream == null)
                    {
                        LogMsg.Message("> error reading metadata", ConsoleColor.Red);
                        return false;
                    }
                    partStream = BlueprintRest.Uncompressing(partStream);
                    
                    tw.WriteLine("bytes ucompressed :\t" + partStream.Length);

                    if (partStream == null)
                    {
                        LogMsg.Message("> error uncompressing metadata", ConsoleColor.Red);
                        return false;
                    }



                    //outputStream.Write(partStream.ToArray(), 0, (int)partStream.Length);
                }
                LogMsg.Message("> done", ConsoleColor.Green);
                return true;
            }
            catch (Exception e)
            {
                Console.Write("> error writing stream: " + e.Message.ToString());
                return false;
            }
        }

        /// <summary>
        /// Load a valid blueprint file
        /// </summary>
        private bool Load(string filename)
        {
            bool result = false;

            try
            {
                using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(fileStream))
                    {
                        if (!Header.Read(reader))
                        {
                            LogMsg.Error("> error reading header at : " + reader.BaseStream.Position);
                            return false;
                        }
                        Blocks = new BlockCollection(this, Header.Size);

                        MemoryStream restStream = BlueprintRest.GetCompressedPart(reader);
                        restStream = BlueprintRest.Uncompressing(restStream);

                        if (!MetaData.Read(restStream))
                        {
                            LogMsg.Error("> error reading metadata at : " + reader.BaseStream.Position);
                            return false;
                        }
                        result = true;
                    }
                }
            }
            catch (Exception e)
            {
                LogMsg.Error("> error reading with error code: " + e.Message.ToString());
                result = false;
            }
            return result;
        }


    }
}
