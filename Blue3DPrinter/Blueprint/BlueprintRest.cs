
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

using Common.Maths;
using Common;

using Blue3DPrinter.other;
using System.Text;

namespace Blue3DPrinter
{
    public class BlueprintRest
    {
        /// <summary>
        /// Store the begin and end position of this class in the file, for compressed version.
        /// </summary>
        //long PositionBegin, PositionEnd;
        Blueprint blueprint;

        /// <summary>
        /// equal to 29 at game version 1.6.3
        /// </summary>
        uint version => (uint)blueprint.Header.Version;

        Vector3i size => blueprint.Header.Size;

        private byte[] m_Density;

        /*
        internal uint[] m_Blocks; // see NodeContext
        private ushort[] m_Blocks_param3;
        private byte[] m_Density;
        internal int[] m_Colors;
        internal long[] m_Textures; //6bit as index of texture in the palette, for 6 faces
        internal byte[] m_TexturesRot; //1bit for each 6 faces mean rotate or no-rotate
        private int[] m_Overlays;
        private int[] m_OverlaysRot;
        //private bool[] m_TerrainFillers;
        */

        int localRotationY;
        Dictionary<Vector3ui, PropertyValueCollection> Tiles;
        Dictionary<Vector3ui, int> blockLockCodes;
        List<PropertyValueCollection> signalSources;
        Dictionary<string, List<PropertyValueCollection>> signalReceivers;
        List<PropertyValueCollection> logicCircuits ;
        List<string> shortcutNames;
        List<BlueprintPart> blueprintParts;

        GroupIdMatchTable SnapPointMatchTable;

        public ColorPalette colorPalette = ColorPalette.Default;

        /// <summary>
        /// the compressed block data structure
        /// </summary>
        /// <param name="header">require header to extract file version and blueprint's block size</param>
        public BlueprintRest(Blueprint blueprint)
        {
            this.blueprint = blueprint;
            Tiles = new Dictionary<Vector3ui, PropertyValueCollection>();
            blockLockCodes = new Dictionary<Vector3ui, int>();
            signalSources = new List<PropertyValueCollection>();
            signalReceivers = new Dictionary<string, List<PropertyValueCollection>>();
            logicCircuits = new List<PropertyValueCollection>();
            shortcutNames = new List<string>();
            blueprintParts = new List<BlueprintPart>();

            if (blueprint.Header.Properties.TryGetValue(BlueprintPropType.Rotation, out Vector3f rotation))
            {
                localRotationY = 0;
            }
        }

        /// <summary>
        /// Read the compressed Metadata from file without decompressing
        /// </summary>
        /// <remarks>
        /// Metadata stream contain all the blueprint block definition, must be decompressed before reading
        /// </remarks>
        public static MemoryStream GetCompressedPart(BinaryReader br)
        {
            int lenght = br.ReadInt32();
            br.ReadByte(); //0
            br.ReadBoolean(); // true

            if (br.BaseStream.Position + lenght > br.BaseStream.Length)
                throw new EndOfStreamException("The length of compressed stream is greater than file length");

            MemoryStream compressedpart = new MemoryStream(br.ReadBytes(lenght));
            br.ReadInt32();
            br.ReadByte();//1
            br.ReadByte();//0

            //fix the misunderstanding
            if (!compressedpart.CanRead || compressedpart.Length == 0) return null;
            compressedpart.Position = 0;
            return compressedpart;
        }

        /// <summary>
        /// Write the compressed or uncompressed Metadata to file
        /// </summary>
        /// <param name="asUncompressed">true if you are writing my .epbx format for debug/investigation</param>
        public static bool WriteCompressed(BinaryWriter bw, byte[] metadata, bool asUncompressed = false)
        {
            if (!asUncompressed)
            {
                bw.Write(metadata.Length);
                bw.Write((byte)0);
                bw.Write(true);
                bw.Write(metadata);
                bw.Write((int)0);
                bw.Write((byte)1);
                bw.Write((byte)0);
            }
            else
                bw.Write(metadata);
            return true;
        }
        /// <summary>
        /// Return the Uncompressed stream
        /// </summary>
        public static MemoryStream Uncompressing(MemoryStream input)
        {
            MemoryStream output = new MemoryStream();
            input.Position = 0;
            using (var archive = new ZipArchive(input, ZipArchiveMode.Read, true))
            {
                //only first entry
                if (archive.Entries.Count != 1)
                    throw new Exception("The number of entries in the compressed data is not correct");
                ZipArchiveEntry entry = archive.Entries[0];
                entry.Open().CopyTo(output);
                LogMsg.Message(string.Format("> uncompress entry \"{0}\" from {1:n0} Bytes to {2:n0} Bytes", entry.Name, entry.CompressedLength, entry.Length));
            }
            //fix the misunderstanding
            if (output == null || !output.CanRead || output.Length == 0) return null;
            output.Position = 0;

            return output;
        }
        /// <summary>
        /// Return the Compressed stream
        /// </summary>
        public static MemoryStream Compressing(MemoryStream input)
        {
            MemoryStream output = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("0");
                input.CopyTo(entry.Open());
            }

            //fix the misunderstanding
            if (output == null || !output.CanRead || output.Length == 0) return null;
            output.Position = 0;

            return output;
        }

        /// <summary>
        /// Read metadata from the uncompressed stream
        /// </summary>
        public bool Read(MemoryStream uncompressed)
        {
            if (uncompressed == null || !uncompressed.CanRead || !uncompressed.CanSeek)
            {
                throw new ArgumentException("The uncompressed's MemoryStream is closed or disposed");
            }
            using (BinaryReader reader = new BinaryReader(uncompressed, Encoding.Default, true))
            {
                return Read(reader);
            }
        }
        /// <summary>
        /// <see cref="Read(MemoryStream)"/>
        /// The BinaryReader must be associated to a uncompressed stream
        /// </summary>
        public bool Read(BinaryReader br)
        {
            LogMsg.Message("> reading blocks...");
            try
            {
                bool result = ReadMetadata(br);
                return result;
            }
            catch (Exception e)
            {
                LogMsg.Error(string.Format("ERROR reading blueprint's block data at uncompressed stream position : {0}\n{1}", br.BaseStream.Position, e.Message.ToString()));
                return false;
            }
        }

        /// <summary>
        /// Create the stream from this class
        /// </summary>
        public MemoryStream GetMetadataStream()
        {
            MemoryStream output = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(output, Encoding.Default, true))
            {
                if (!WriteMetadata(writer)) return null;
            }
            if (output == null || !output.CanRead || output.Length == 0) return null;
            output.Position = 0;
            return output;
        }

        /// <summary>
        /// Read from txt file and store into stream
        /// </summary>
        public bool Read(TextReader tr, out MemoryStream uncompressed)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write to txt file (uncompressed data anyway)
        /// </summary>
        public bool Write(TextWriter tw, MemoryStream uncompressed)
        {
            throw new NotImplementedException();
        }



        private bool WriteMetadata(BinaryWriter bw)
        {
            if (size.x > 250 || size.y > 250 || size.z > 250)
            {
                LogMsg.Message("Error : size out of limit, can't be greater than 250 blocks per side = " + size.ToString(), ConsoleColor.Red);
                return false;
            }
            if (version <= 2)
            {
                bw.Write(size.x);
                bw.Write(size.y);
                bw.Write(size.z);
            }
            int length = size.x * size.y * size.z;

            BitInstances bits = new BitInstances();
            bits.Init(length);

            long begin, end;

            bool HasUnknowParam = false;
            bool HasColor = false;
            bool HasTexture = false;
            bool HasOverlay = false;

            ////////////////////////////////// m_Blocks
            if (version <= 6)
                throw new NotSupportedException("Version 6 is deprecated");
            else
            {
                begin = bw.BaseStream.Position;
                bits.Write(bw);
                for (int x = 0; x < size.x; x++)
                    for (int y = 0; y < size.y; y++)
                        for (int z = 0; z < size.z; z++)
                        {
                            var block = blueprint.Blocks[x, y, z];
                            if (block != null && block.caption > 0)
                            {
                                bw.Write(block.caption);
                                bits.SetFlag(coordToOffset(x, y, z), true);

                                HasUnknowParam |= block.unknow > 0;
                                HasColor |= block.color > 0;
                                HasTexture |= block.texture > 0;
                                HasOverlay |= block.overlay > 0;
                            }
                        }
                end = bw.BaseStream.Position;
                bw.BaseStream.Position = begin;
                bits.Write(bw);
                bw.BaseStream.Position = end;
            }

            ////////////////////////////////// unknow
            if (version > 11)
            {
                begin = bw.BaseStream.Position;
                bits.Clear();
                bits.Write(bw);

                if (HasUnknowParam)
                {
                    for (int x = 0; x < size.x; x++)
                        for (int y = 0; y < size.y; y++)
                            for (int z = 0; z < size.z; z++)
                            {
                                var block = blueprint.Blocks[x, y, z];
                                if (block != null && block.unknow > 0)
                                {
                                    bw.Write(block.unknow);
                                    bits.SetFlag(coordToOffset(x, y, z), true);
                                }
                            }
                    end = bw.BaseStream.Position;
                    bw.BaseStream.Position = begin;
                    bits.Write(bw);
                    bw.BaseStream.Position = end;
                }
            }

            ////////////////////////////////// m_Density
            bw.Write(false);
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    for (int z = 0; z < size.z; z++)
                    {
                        var block = blueprint.Blocks[x, y, z];
                        if (block != null && block.caption > 0) bw.Write((sbyte)-1);
                        else bw.Write((byte)0);
                    }
            ////////////////////////////////// m_Colors
            begin = bw.BaseStream.Position;
            bits.Clear();
            bits.Write(bw);
            if (HasColor)
            {

            }
            ////////////////////////////////// m_Texture
            begin = bw.BaseStream.Position;
            bits.Clear();
            bits.Write(bw);
            if (HasTexture)
            {

            }
            ////////////////////////////////// m_TextureRot
            if (version > 20)
            {
                begin = bw.BaseStream.Position;
                bits.Clear();
                bits.Write(bw);
                if (HasTexture)
                {

                }
            }
            ////////////////////////////////// m_Overlay
            begin = bw.BaseStream.Position;
            bits.Clear();
            bits.Write(bw);
            if (HasOverlay)
            {

            }
            ////////////////////////////////// m_OverlayRot
            begin = bw.BaseStream.Position;
            bits.Clear();
            bits.Write(bw);
            if (HasOverlay)
            {

            }

            ////////////////////////////////// Tiles
            bw.Write((short)0);

            ////////////////////////////////// LockCodes
            if (version > 11)
                bw.Write((ushort)0);

            ////////////////////////////////// Signals
            if (version >= 14)
            {
                bw.Write((ushort)0);
                ////////////////////////////////// ReceiveSignals
                bw.Write((ushort)0);
            }
            ////////////////////////////////// Logics
            if (version >= 15)
                bw.Write((ushort)0);

            ////////////////////////////////// ShortCutName
            if (version > 15)
            {
                bw.Write((ushort)0);
                if (version < 17)
                    bw.Write((ushort)0);
            }
            ////////////////////////////////// BlueprintPart
            if (version > 18)
                bw.Write((ushort)0);

            ////////////////////////////////// ColorPalette
            if (version > 20)
            {
                bw.Write((byte)1);
                if (colorPalette == null) colorPalette = ColorPalette.Default;
                if (!colorPalette.Write(bw)) return false;
            }

            return true;
        }

        /// <summary>
        /// The most important function : see <b>blue3dprinter\NerdInside\EbpUncompressed.bt</b> to look the file format syntax
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        private bool ReadMetadata(BinaryReader br)
        {
            Vector3i Size = version <= 2U ? (Vector3i)Utils.ReadVector3ui(br) : size;

            if (Size.x > 250 || Size.y > 250 || Size.z > 250)
            {
                LogMsg.Message("Error : size out of limit, can't be greater than 250 blocks per side = " + Size.ToString(), ConsoleColor.Red);
                return false;
            }
            int length = Size.x * Size.y * Size.z;

            localRotationY = 0;


            //m_Blocks = new uint[length];
            //m_Blocks_param3 = new ushort[length];
            m_Density = new byte[length];
            //m_Colors = new int[length];
            //m_Textures = new long[length];
            //m_TexturesRot = new byte[length];
            //m_Overlays = new int[length];
            //m_OverlaysRot = new int[length];

            BitInstances bits = new BitInstances();

            //generate a precomputed table to speed up access
            Vector3b[] precalccoords = new Vector3b[length];
            for (int i = 0; i < length; i++)
            {
                offsetToCoord(i, out int x, out int y, out int z);
                precalccoords[i] = new Vector3b((byte)x, (byte)y, (byte)z);
            }

            #region BLOCKS
            if (version <= 6)
            {
                throw new NotSupportedException("Version 6 is deprecated");
                //for (int i = 0; i < length; i++) m_Blocks[i] = br.ReadUInt32();
            }
            else
            {
                if (!bits.Read(br, length)) return false;

                for (int i = 0; i < length; i++)
                    if (bits.GetFlag(i))
                    {
                        //m_Blocks[i] = br.ReadUInt32();
                        var vector = precalccoords[i];
                        var block = blueprint.Blocks.Create(vector);
                        block.caption = br.ReadUInt32();
                    }

            }

            if (version < 28)
                for (int i = 0; i < length; i++)
                    blueprint.Blocks[precalccoords[i]].caption &= 4278190079U;
            //m_Blocks[i] &= 4278190079U; //1111 1110 1111 1111 1111 1111 1111 1111

            /*
            if (this.Header.BlockIdMapping != null && this.Header.BlockIdMapping.parentQuery != null)
            {
              for (int index = 0; index < this.m_Blocks.Length; ++index)
                this.m_Blocks[index].set_BuildSolution(this.Header.BlockIdMapping.parentQuery[this.m_Blocks[index].get_BuildSolution()]);
            }*/

            //Console.WriteLine("> reading block param3");
            if (version > 11)
            {
                if (!bits.Read(br, length)) return false;

                for (int i = 0; i < length; i++)
                    if (bits.GetFlag(i))
                        blueprint.Blocks[precalccoords[i]].unknow = br.ReadUInt16();
                //m_Blocks_param3[i] = br.ReadUInt16();
            }
            #endregion

            #region DENSITY
            //Console.WriteLine("> reading density");
            if (br.ReadBoolean())
            {
                // is constant density
                sbyte num = br.ReadSByte();
                for (int i = 0; i < length; i++)
                    m_Density[i] = (byte)num;
            }
            else
            {
                m_Density = br.ReadBytes(length);
            }
            #endregion

            if (version > 6)
            {
                #region COLORS
                //Console.WriteLine("> reading Colors");

                if (!bits.Read(br, length)) return false;

                for (int i = 0; i < length; i++)
                    if (bits.GetFlag(i))
                    {
                        int color = br.ReadInt32();

                        var vector = precalccoords[i];
                        var block = blueprint.Blocks[vector];

                        //?????
                        if (block != null) block.color = color;

                        //m_Colors[i] = br.ReadInt32();
                    }
                #endregion

                #region TEXTURES (long-> is the texture asset ID or something like)

                //Console.WriteLine("> reading Textures");
                if (!bits.Read(br, length)) return false;

                for (int i = 0; i < length; i++)
                    if (bits.GetFlag(i))
                    {
                        long texture = br.ReadInt64();
                        var block = blueprint.Blocks[precalccoords[i]];

                        //?????
                        if (block != null) block.texture = texture;
                    }
                //m_Textures[i] = br.ReadInt64();
                #endregion

                #region TEXTURES ROTATION (to do)
                if (version > 20)
                {
                    //Console.WriteLine("> reading TexturesRot");

                    if (!bits.Read(br, length)) return false;

                    for (int i = 0; i < length; i++)
                        if (bits.GetFlag(i))
                        {
                            byte rot = br.ReadByte();
                            var block = blueprint.Blocks[precalccoords[i]];

                            //?????
                            if (block != null) block.texturerot = rot;
                        }
                    //m_TexturesRot[i] = br.ReadByte();
                }
                #endregion
            }

            if (version > 7)
            {
                #region OVERLAYS (to do)
                //Console.WriteLine("> reading Overlays");

                if (!bits.Read(br, length)) return false;

                for (int i = 0; i < length; i++)
                {
                    if (bits.GetFlag(i))
                    {
                        int overlay = br.ReadInt32();
                        var block = blueprint.Blocks[precalccoords[i]];
                        if (block != null) block.overlay = overlay;
                    }
                }
                //m_Overlays[i] = br.ReadInt32();
                #endregion

                #region OVERLAY ROTATIONS (to do)
                //Console.WriteLine("> reading OverlaysRot");
                if (!bits.Read(br, length)) return false;

                for (int i = 0; i < length; i++)
                {
                    if (bits.GetFlag(i))
                    {
                        if (version >= 8U)
                            blueprint.Blocks[precalccoords[i]].overlayrot = br.ReadInt32();
                        //m_OverlaysRot[i] = br.ReadInt32();
                        else
                            br.ReadInt16();
                    }
                }
                #endregion
            }

            #region TILES
            //Console.WriteLine("> reading Tiles");
            Tiles.Clear();
            if (version > 10)
            {
                var count = br.ReadInt16();
                for (int i = 0; i < count; i++)
                {
                    Vector3ui key = Utils.ReadVector3i_12_8_12(br);
                    var collection = PropertyValueCollection.Read(br);
                    if (collection==null) return false;
                    Tiles.Add(key, collection);
                }
            }
            #endregion

            #region BLOCK LOCKCODES
            //Console.WriteLine("> reading LockCode");
            blockLockCodes.Clear();
            if (version > 11)
            {
                var count = br.ReadUInt16();
                for (int i = 0; i < count; i++)
                    blockLockCodes.Add(Utils.ReadVector3i_12_8_12(br), version <= 24U ? br.ReadUInt16() : br.ReadInt32());
            }
            #endregion

            //Console.WriteLine("> reading Signals");
            signalSources.Clear();
            signalReceivers.Clear();
            if (version >= 14)
            {
                #region SIGNALS SOURCES
                var count = br.ReadUInt16();
                if (version >= 17)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var collection = PropertyValueCollection.Read(br);
                        if (collection == null) return false;
                        signalSources.Add(collection);
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        Vector3ui Vector3i = Utils.ReadVector3i_12_8_12(br);
                        //signalSources.Add(new ContextAttributeLineProvider(br.ReadString(), Vector3i, ContextAttributeLineProvider.CommandHelperLineProvider.syncObjectEnabled).RebuildBuilder(Vector3i.syncObjectEnabled));
                    }
                }
                #endregion

                #region SIGNALS RECEIVERS
                //Console.WriteLine("> reading Receivers");
                count = br.ReadUInt16();
                for (int i = 0; i < count; i++)
                {
                    string key = br.ReadString();
                    int capacity = br.ReadUInt16();
                    signalReceivers.Add(key, new List<PropertyValueCollection>(capacity));
                    for (int j = 0; j < capacity; j++)
                    {
                        var collection = PropertyValueCollection.Read(br);
                        if (collection==null) return false;
                        signalReceivers[key].Add(collection);
                    }
                }
                #endregion
            }
            //Console.WriteLine("> reading Logics");
            logicCircuits.Clear();
            if (version >= 15)
            {
                #region LOGIC CIRCUITS
                var count = br.ReadUInt16();
                for (int i = 0; i < count; i++)
                {
                    var collection = PropertyValueCollection.Read(br);
                    if (collection == null) return false;
                    logicCircuits.Add(collection);
                }
                #endregion
            }
            //Console.WriteLine("> reading ShortNames");
            shortcutNames.Clear();
            if (version > 15)
            {
                #region SHORTCUT NAMES
                var count = br.ReadUInt16();
                for (int i = 0; i < count; i++)
                    shortcutNames.Add(br.ReadString());

                if (version < 17)
                {
                    count = br.ReadUInt16();
                    for (int i = 0; i < count; i++) br.ReadString(); //nothing to save...
                }
                #endregion
            }
            //Console.WriteLine("> reading BlueprintPart");
            
            #region BLUEPRINT PARTS (TODO for game version 1.10)
            blueprintParts.Clear();
            if (version > 18)
            {
                int subversion = version > 29 ? br.ReadByte() : 0;
                var count = br.ReadUInt16();
                for (int i = 0; i < count; i++)
                {
                    var blueprintPart = BlueprintPart.Read(br, subversion);
                    if (blueprintPart == null) return false;
                    blueprintParts.Add(blueprintPart);
                }
                if (version > 30)
                {
                    SnapPointMatchTable = new GroupIdMatchTable();
                    SnapPointMatchTable.Read(br);
                }
                
            }
            #endregion

            #region COLOR PALETTE
            //Console.WriteLine("> reading ColorPalette");
            if (version > 20)
            {
                br.ReadByte(); //palette count ? but no reference
                colorPalette = new ColorPalette(br);
            }
            else
                colorPalette = new ColorPalette();

            #endregion

            /*if (version <= 5)
            {
                for (int i = 0; i < count; i++)
                    m_Blocks[i].previousCaption = NodeContext.ListBookmark(this.m_Blocks[index].previousCaption);
            }
            if (this.Header.readVersion < 6U)
                this.convertBlocksToNewColorFormat();
            if (this.Header.readVersion < 10U)
                this.convertLandingGears();
            if (this.Header.readVersion < 22U)
                this.convertParentIndex(this.Header.readVersion < 11U);
            */

            return true;
        }

        public int coordToOffset(int x, int y, int z)
        {
            int sizex = size.x;
            int sizey = size.y;
            int sizez = size.z;
            switch (localRotationY)
            {
                case 0: return x + y * sizex + z * sizex * sizey;
                case 1: return z + y * sizez + (sizex - x - 1) * sizez * sizey;
                case 2: return sizex - x - 1 + y * sizex + (sizez - z - 1) * sizex * sizey;
                case 3: return sizez - z - 1 + y * sizez + x * sizez * sizey;
                default: return x + y * sizex + z * sizex * sizey;
            }
        }
        public void offsetToCoord(int offset, out int x, out int y, out int z)
        {
            int sizex = size.x;
            int sizey = size.y;
            int sizez = size.z;

            switch (localRotationY)
            {
                case 0:
                    z = offset / (sizex * sizey);
                    offset %= sizex * sizey;
                    y = offset / sizex;
                    offset %= sizex;
                    x = offset;
                    break;
                case 1:
                    x = -(offset / (sizez * sizey) - sizex + 1);
                    offset %= sizez * sizey;
                    y = offset / sizez;
                    offset %= sizez;
                    z = offset;
                    break;
                case 2:
                    z = -(offset / (sizex * sizey) - sizez + 1);
                    offset %= sizex * sizey;
                    y = offset / sizex;
                    offset %= sizex;
                    x = -(offset - sizex + 1);
                    break;
                case 3:
                    x = offset / (sizez * sizey);
                    offset %= sizez * sizey;
                    y = offset / sizez;
                    offset %= sizez;
                    z = -(offset - sizez + 1);
                    break;
                default:
                    z = offset / (sizex * sizey);
                    offset %= sizex * sizey;
                    y = offset / sizex;
                    offset %= sizex;
                    x = offset;
                    break;
            }
        }


        /// <summary>
        /// bit array field, is a simple method to don't read empty blocks 
        /// </summary>
        class BitInstances
        {
            byte[] chunks;

            public bool Read(BinaryReader br, int size)
            {
                int count = br.ReadInt32();
                if (count < 0) throw new InvalidCastException("int32 negative");

                if (count > size) return false;
                chunks = br.ReadBytes(count);
                return count > 0;
            }

            /// <summary>
            /// write a temporary white list. You have to memorize the file position begin
            /// </summary>
            /// <param name="numofBits">the size.x*y*z</param>
            public void Init(int numofBits)
            {
                chunks = new byte[numofBits / 8 + 1];
            }

            public void Clear()
            {
                for (int i = 0; i < chunks.Length; i++) chunks[i] = 0;
            }
            public bool Write(BinaryWriter bw)
            {
                int lenght = chunks.Length;
                bw.Write(lenght);
                bw.Write(chunks);
                return true;
            }

            public bool GetFlag(int i)
            {
                if (i >= chunks.Length * 8) throw new IndexOutOfRangeException("wrong index for bitinstances method");
                return (chunks[i / 8] & (1 << (i % 8))) > 0;
            }

            public void SetFlag(int i, bool value = true)
            {
                if (i >= chunks.Length * 8) throw new IndexOutOfRangeException("wrong index for bitinstances method");
                int bit = 1 << (i % 8);
                if (value) chunks[i / 8] |= (byte)bit;
                else chunks[i / 8] &= (byte)~bit;
            }
        }
    }

}