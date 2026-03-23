using Common;
using Common.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blue3DPrinter
{
    public enum PropType : byte
    {
        String = 0,
        Bool = 1,
        Int = 2,
        Float = 3,
        Vector3f = 4,
        Long = 5,
    }

    public enum BlueprintPropType
    {
        AddDigoutBox = 1,
        FlattenTerrain = 2,
        GroundYOffset = 3,
        RotationToFaceNorth = 4,
        Powered = 5,
        Rotation = 6,
        GroupName = 7,
        ChangedBuild = 8,
        ChangedDate = 9,
        CreatorPlayerName = 10, // 0x0000000A
        CreatorPlayerId = 11, // 0x0000000B
        ChangedPlayerName = 12, // 0x0000000C
        ChangedPlayerId = 13, // 0x0000000D
        CameraYOffset = 14, // 0x0000000E
        CameraZOffset = 15, // 0x0000000F
        DisplayName = 16, // 0x00000010
        GroundYOffsetFloat = 17, // 0x00000011
        PivotPoint = 18, // 0x00000012
        KeepTopSoil = 19, // 0x00000013
        RotationSensitivity = 20, // 0x00000014
        Tags = 21, // 0x00000015
    }

    public class BlueprintProperties
    {
        public void GenerateDefaultProperties()
        {
            data.Clear();

            string id = "";
            foreach (var t in Encoding.Unicode.GetBytes("Blue3DPrinter0000")) { id += t.ToString("X2"); }

            id = "01234567890123456";

            data.Add(BlueprintPropType.CreatorPlayerName, new BlueprintProp(PropType.String, "Blue3DPrinter"));
            data.Add(BlueprintPropType.CreatorPlayerId, new BlueprintProp(PropType.String, id));
            data.Add(BlueprintPropType.ChangedPlayerName, new BlueprintProp(PropType.String, "Blue3DPrinter"));
            data.Add(BlueprintPropType.ChangedPlayerId, new BlueprintProp(PropType.String, id));
            data.Add(BlueprintPropType.DisplayName, new BlueprintProp(PropType.String, "Blue3DPrinter_voxelized"));
        }

        private Dictionary<BlueprintPropType, BlueprintProp> data = new Dictionary<BlueprintPropType, BlueprintProp>();

        public void Clear() => data.Clear();

        public bool Contains(BlueprintPropType key) => data.ContainsKey(key);

        public bool TryGetValue<T>(BlueprintPropType key, out T value)
        {
            BlueprintProp pair;

            if (data.TryGetValue(key, out pair) && pair.Object is T)
            {
                value = (T)pair.Object;
                return true;
            }
            else
                value = default(T);
            return false;
        }

        public void SetValue(BlueprintPropType key, PropType type, object value)
        {
            data[key] = new BlueprintProp()
            {
                Type = type,
                Object = value
            };
        }

        public void Remove(BlueprintPropType key) => data.Remove(key);
        public bool Write(BinaryWriter bw)
        {
            bw.Write((ushort)1);
            bw.Write((ushort)data.Count);

            foreach (var pair in data)
            {
                bw.Write((int)pair.Key);
                bw.Write(false);
                bw.Write(false);
                bw.Write(false);

                var obj = pair.Value.Object;
                var type = pair.Value.Type;

                bw.Write((byte)type);
                switch (type)
                {
                    case PropType.String:
                        bw.Write((string)obj);
                        break;
                    case PropType.Bool:
                        bw.Write((bool)obj);
                        bw.Write("");
                        break;
                    case PropType.Int:
                        bw.Write((int)obj);
                        bw.Write("");
                        break;
                    case PropType.Float:
                        bw.Write((float)obj);
                        bw.Write("");
                        break;
                    case PropType.Vector3f:
                        Vector3f v = (Vector3f)obj;
                        bw.Write(v.x);
                        bw.Write(v.y);
                        bw.Write(v.z);
                        bw.Write("");
                        break;
                    case PropType.Long:
                        bw.Write((long)obj);
                        bw.Write("");
                        break;
                    default:
                        LogMsg.Message("> HORROR, unknow properties : \"" + type.ToString() + "\", can't continue writing");
                        return false;
                }
            }
            bw.Write((ushort)0);
            return true;
        }
        public bool Read(BinaryReader br)
        {
            int num1 = br.ReadUInt16(); //1
            int count = br.ReadUInt16();

            for (int i = 0; i < count; ++i)
            {
                BlueprintPropType key = (BlueprintPropType)br.ReadInt32();

                if (br.ReadBoolean() || br.ReadBoolean() || br.ReadBoolean())
                    throw new Exception("error reading 3 booleans");

                PropType propType = (PropType)br.ReadByte();
                object obj = null;

                switch (propType)
                {
                    case PropType.String:
                        obj = br.ReadString();
                        break;
                    case PropType.Bool:
                        obj = br.ReadBoolean();
                        br.ReadString();
                        break;
                    case PropType.Int:
                        obj = br.ReadInt32();
                        br.ReadString();
                        break;
                    case PropType.Float:
                        obj = br.ReadSingle();
                        br.ReadString();
                        break;
                    case PropType.Vector3f:
                        obj = new Vector3f(br);
                        br.ReadString();
                        break;
                    case PropType.Long:
                        obj = br.ReadInt64();

                        //try to convert into datetime
                        if (key == BlueprintPropType.ChangedDate)
                            obj = DateTime.FromBinary((long)obj);

                        br.ReadString();
                        break;
                    default:
                        LogMsg.Message("> HORROR, unknow properties : \"" + propType.ToString() + "\", can't continue reading");
                        return false;
                }

                data[key] = new BlueprintProp()
                {
                    Type = propType,
                    Object = obj
                };
            }
            int num0 = br.ReadUInt16(); //0
            return true;
        }

        public bool Write(TextWriter tw)
        {
            tw.WriteLine("\n//+------------------------+");
            tw.WriteLine("//|       PROPERTIES       |");
            tw.WriteLine("//+------------------------+");
            
            foreach (var pair in data)
                tw.WriteLine(Utils.mytxtformat(pair.Key.ToString(), pair.Value.ObjToString()));
            
            return true;
        }

        public bool Read(TextReader tr)
        {
            throw new NotImplementedException();
        }

        struct BlueprintProp
        {
            public PropType Type;
            public object Object;
            public BlueprintProp(PropType type, object value)
            {
                Type = type;
                Object = value;
            }
            public string ObjToString()
            {
                if (Object == null) return "";
                if (Object is DateTime time) return string.Format("{0:dd/MM/yyyy hh:mm:ss}", time);
                return Object.ToString();
            }
        }

    }


}
