using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Common;
using Common.Maths;


namespace Blue3DPrinter
{
    /// <summary>
    /// not the same of <see cref="PropType"/>
    /// </summary>
    public enum PropType2 : byte
    {
        Int32 = 0,
        String = 1,
        Bool = 2,
        Float = 3,
        Byte4 = 5,
    }
    public abstract class PropertyValuePair
    {
        public abstract PropType2 Type { get; }
        public string Name;
        protected object m_value;

        public static PropertyValuePair Read(BinaryReader br)
        {
            var type = (PropType2)br.ReadByte();
            var name = br.ReadString();

            switch (type)
            {
                case PropType2.Int32: return new PropertyInt32(name, br.ReadInt32());
                case PropType2.String: return new PropertyString(name, br.ReadString());
                case PropType2.Bool: return new PropertyBool(name, br.ReadBoolean());
                case PropType2.Float: return new PropertyFloat(name, br.ReadSingle());
                case PropType2.Byte4: return new PropertyByte4(name, new Vector4b(br.ReadUInt32()));
                default: throw new Exception($"Unknow PropType2 {type}, if missing please add the sealed class");
            }
        }
        public void Write(BinaryWriter bw)
        {
            bw.Write((byte)Type);
            bw.Write(Name);
            switch (Type)
            {
                case PropType2.Byte4: //i hope the cast from vector4b to int is correct
                case PropType2.Int32: bw.Write((int)m_value); break;
                case PropType2.String: bw.Write((string)m_value); break;
                case PropType2.Bool: bw.Write((bool)m_value); break;
                case PropType2.Float: bw.Write((float)m_value); break;

                default: throw new Exception($"Unknow PropType2 {Type}, if missing please add the sealed class");
            }
        }
    }
    public abstract class PropertyValuePair<T> : PropertyValuePair
    {
        public T Value { get => (T)m_value; set => m_value = value; } 
        protected PropertyValuePair(string name, T value)
        {
            Name = name;
            m_value = value;
        }
    }
    public class PropertyInt32 : PropertyValuePair<int>
    {
        public override PropType2 Type => PropType2.Int32;
        public PropertyInt32(string name, int value) : base(name, value) { }
    }
    public class PropertyString : PropertyValuePair<string>
    {
        public override PropType2 Type => PropType2.String;
        public PropertyString(string name, string value) : base(name, value) { }
    }
    public class PropertyBool : PropertyValuePair<bool>
    {
        public override PropType2 Type => PropType2.Bool;
        public PropertyBool(string name, bool value) : base(name, value) { }
    }
    public class PropertyFloat : PropertyValuePair<float>
    {
        public override PropType2 Type => PropType2.Float;
        public PropertyFloat(string name, float value) : base(name, value) { }
    }
    public class PropertyByte4 : PropertyValuePair<Vector4b>
    {
        public override PropType2 Type => PropType2.Byte4;
        public PropertyByte4(string name, byte r, byte g, byte b, byte a) : base(name, new Vector4b(r, g, b, a)) { }
        public PropertyByte4(string name, Vector4b vector) : base(name, vector) { }
    }
    public class PropertyValueCollection
    {
        public byte Version { get; private set; } = 1;
        public Dictionary<string, PropertyValuePair> Properties = new Dictionary<string, PropertyValuePair>();

        public PropertyValueCollection()
        {
        }

        public static PropertyValueCollection Read(BinaryReader br)
        {
            var collection = new PropertyValueCollection();
            if (!collection.read(br)) return null;
            return collection;
        }

        bool read(BinaryReader br)
        {
            try
            {
                Version = br.ReadByte();
                int count = br.ReadUInt16();
                for (int i = 0; i < count; i++)
                {
                    var prop = PropertyValuePair.Read(br);
                    if (Properties.ContainsKey(prop.Name)) LogMsg.Error($"The property {prop.Name} already exist, it can't be added");
                    else Properties.Add(prop.Name, prop);
                }
            }
            catch(Exception e)
            {
                LogMsg.Error(e.ToString());
                return false;
            }
            return true;
        }
        public void Write(BinaryWriter bw)
        {
            bw.Write(Version);
            bw.Write(Properties.Count);
            foreach(var pair in Properties)
                pair.Value.Write(bw);
        }
    }

}
