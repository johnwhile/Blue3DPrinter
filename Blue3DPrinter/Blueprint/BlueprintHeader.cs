using Blue3DPrinter.other;
using Common.Maths;
using System;
using System.IO;

namespace Blue3DPrinter
{

    /// <summary>
    /// TODO : remove all old verion variant in the reading functions, it no longer makes sense to keep them
    /// </summary>
    public enum BlueprintVersion : int
    {
        V_1_06 = 29, //1.6.3
        V_1_10 = 31
    }

    /// <summary>
    /// Uncompressed first part of the file
    /// </summary>
    public class BlueprintHeader
    {
        //Store the begin and end position of this class in the file
        long PositionBegin, PositionEnd;

        Blueprint blueprint;

        //public static int PrefabYOffset;
        //public static uint CurrentSaveVersion;

        public BlockMapping BlockMap;
        /// <summary>
        /// <i>Blue3DPrinter.Default.FileVersion</i>
        /// </summary>
        public BlueprintVersion Version = BlueprintVersion.V_1_10;
        public PrefabType prefabType;
        public Vector3i Size;

        public BlockStatistics Statistics;
        public BlueprintProperties Properties ;
        public BlockGrouping BlockGroupInfo;

        public BlueprintHeader(Blueprint blueprint)
        {
            this.blueprint = blueprint;
            Statistics = new BlockStatistics();
            Properties = new BlueprintProperties();
            BlockMap = new BlockMapping(0);
            BlockGroupInfo = new BlockGrouping();

            Version = (BlueprintVersion)AppSetting.FileVersion;
        }

        public bool Write(BinaryWriter bw)
        {
            uint version = (uint)Version;

            bw.Write(2022986309U); //blueprint file format
            bw.Write(version);
            bw.Write((byte)prefabType);

            bw.Write(Size.x);
            bw.Write(Size.y);
            bw.Write(Size.z);

            if (!Properties.Write(bw)) return false;
            
            //todo: remove old versions after 1.06 
            if (version > 3 && !Statistics.Write(bw, version)) return false;

            if (version > 27 && BlockMap!=null)
            {
                bw.Write(true);
                BlockMap.Write(bw);
            }
            else
            {
                bw.Write(false);
            }

            BlockGroupInfo.Write(bw);

            return true;
        }

        public bool Read(BinaryReader br)
        {
            PositionBegin = br.BaseStream.Position;

            if (br.ReadUInt32() != 2022986309U)
            {
                LogMsg.Message("> file type not 2022986309 ?", ConsoleColor.Red);
                return false;
            }
            uint version = br.ReadUInt32();

            Version = (BlueprintVersion)version;
            LogMsg.Message("> file Version = " + Version, ConsoleColor.Yellow);

            if (version < 29)
                LogMsg.Warning("> i tested only Version = 29");

            if (version > 1) 
                prefabType = (PrefabType) br.ReadByte();

            if (version > 2)
            {
                Size.x = br.ReadInt32();
                Size.y = br.ReadInt32();
                Size.z = br.ReadInt32();
                if (!Properties.Read(br)) return false;
            }

            Statistics.Clear();
            if (version > 3 && !Statistics.Read(br, version)) return false;

            BlockMap.Clear();
            if (version > 27 && br.ReadBoolean())
            {
                if (!BlockMap.Read(br)) return false;
            }

            //if (Header.readVersion < 6)
            //    this.convertStatisticsToNewColorFormat();

            BlockGroupInfo.Clear();
            if (version >= 9)
            {
                BlockGroupInfo.Read(br);
            }

            PositionEnd = br.BaseStream.Position;

            return true;
        }


        public bool Write(TextWriter tw)
        {
            tw.WriteLine("//+------------------------+");
            tw.WriteLine("//|        HEADER          |");
            tw.WriteLine("//+------------------------+");

            tw.WriteLine(Utils.mytxtformat("Version", Version, 12));
            tw.WriteLine(Utils.mytxtformat("prefab", prefabType, 12));
            tw.WriteLine(Utils.mytxtformat("size", Size, 12));

            Properties.Write(tw);
            Statistics.Write(tw);

            if (BlockMap != null)
                BlockMap.Write(tw);

            BlockGroupInfo.Write(tw);

            return true;
        }

        public bool Read(TextReader tr)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Copy entire section of file to another stream. The cursor are set to end
        /// </summary>
        /// <param name="br">the blueprint file stream</param>
        /// <param name="output">the output file stream</param>
        internal void CopyStreamTo(BinaryReader br, Stream output)
        {
            int length = (int)(PositionEnd - PositionBegin);
            if (length <= 0) throw new Exception("Wrond stream size");
            br.BaseStream.Position = PositionBegin;
            Utils.CopyStream(br.BaseStream, output, length);
            br.BaseStream.Position = PositionEnd;
        }


    }

    /// <summary>
    /// Define the type of blueprint show in game, it doesn't affect the file format !
    /// </summary>
    public enum PrefabType : byte
    {
        UNKNOWN = 0,
        /// <summary>
        /// asteroid voxel, never tested...
        /// </summary>
        AV = 1,
        /// <summary>
        /// Base
        /// </summary>
        BA = 2,
        /// <summary>
        /// Small vessel
        /// </summary>
        SV = 4,
        /// <summary>
        /// Capital vessel
        /// </summary>
        CV = 8,
        /// <summary>
        /// Hover vessel
        /// </summary>
        HV = 16
    }
}
