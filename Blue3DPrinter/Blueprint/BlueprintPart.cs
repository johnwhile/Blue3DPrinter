using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using Common.Maths;

namespace Blue3DPrinter
{
    /// <summary>
    /// Introduced with game version 1.10
    /// </summary>
    public class BlueprintPart
    {
        public static int CurrentSaveVersion;
        public string Name;
        public Vector3i Pos;
        public Vector3i Size;
        public List<PartSnapPoint> SnapPoints;
        public string Path;
        public string Filename;
        private string OrigName;
        public int RotateY;


        public static BlueprintPart Read(BinaryReader br, int version  =0)
        {
            BlueprintPart part = new BlueprintPart();
            part.read(br, version);
            return part;
        }

        void read(BinaryReader br, int version = 0)
        {
            Name = br.ReadString();

            if (version == 0)
            {
                Pos = Utils.ReadVector3i_11_10_11(br);
                Size = Utils.ReadVector3i_11_10_11(br);
            }
            else
            {
                Pos = Utils.ReadVector3i_18_18_18(br);
                Size = Utils.ReadVector3i_11_10_11(br);
                if (version >4)
                {
                    RotateY = br.ReadByte();

                }
                if (br.ReadBoolean())
                {
                    Path = br.ReadString();
                    Filename = br.ReadString();
                    if (version > 2) OrigName = br.ReadString();
                }
                int capacity = br.ReadUInt16();
                SnapPoints = new List<PartSnapPoint>(capacity);

                for (int i = 0; i < capacity; i++) SnapPoints.Add(PartSnapPoint.Read(br, version));
            }
        }

        public void Write(BinaryWriter bw, int version = 0)
        {
            throw new NotImplementedException();
        }
    }

    public class GroupIdMatchTable
    {
        public List<SGroupIdMatch> entries;
        public Dictionary<int, string> groupIdToName;
        public byte version = 6;

        public GroupIdMatchTable()
        {
            entries = new List<SGroupIdMatch>();
            groupIdToName = new Dictionary<int, string>();
        }

        public void Read(BinaryReader br)
        {
            entries.Clear();
            groupIdToName.Clear();

            version = br.ReadByte();
            int count = br.ReadUInt16();

            for (int i = 0; i < count; i++)
                entries.Add(SGroupIdMatch.Read(br));

            if (version > 1)
            {
                int numkeys = br.ReadUInt16();
                for (int k = 0; k < numkeys; k++)
                {
                    var key = br.ReadInt32();
                    var name = br.ReadString();
                    groupIdToName.Add(key, name);
                }
            }
        }

        public void Write(BinaryWriter bw)
        {
            throw new NotImplementedException();
        }

        public struct SGroupIdMatch
        {
            public int GroupId1;
            public int GroupId2;
            public bool bMirror;
            public static SGroupIdMatch Read(BinaryReader br)
            {
                var group = default(SGroupIdMatch);
                group.GroupId1 = br.ReadInt32();
                group.GroupId2 = br.ReadInt32();
                group.bMirror = br.ReadBoolean();
                return group;
            }
        }

    }

    public enum SnapPointEnum : byte
    {
        _0 = 0,
        _1 = 1,
        _2 = 2,
        _3 = 3,
        _4 = 4,
        _5 = 5,
        _255 = 255, // 0xFF
    }

    public struct PartSnapPoint
    {
        public ushort Id;
        public Vector3i OffsetPos;
        public int GroupId;
        public SnapPointEnum Facing;
        public bool bOverlapping;
        public bool bCopyAir;

        public static PartSnapPoint Read(BinaryReader br, int version)
        {
            var point = default(PartSnapPoint);
            point.read(br, version);
            return point;
        }

        void read(BinaryReader br, int version)
        {
            Id = br.ReadUInt16();
            OffsetPos = new Vector3i(br.ReadSByte(), br.ReadSByte(), br.ReadSByte());
            GroupId = br.ReadInt32();
            Facing = (SnapPointEnum)br.ReadByte();
            
            bOverlapping = true;
            bCopyAir = false; //??
            if (version > 1)
            {
                byte b = br.ReadByte();
                if (version < 6)
                {
                    bOverlapping = b > 0;
                }
                else
                {
                    bOverlapping = (b & 1) > 0;
                    bCopyAir = (b & 2) > 0;
                }
            }
        }

        public void Write(BinaryWriter bw, int version)
        {
            throw new NotImplementedException();
        }
    }
}
