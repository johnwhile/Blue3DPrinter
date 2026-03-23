using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Common;

namespace UnityTool
{
    public static class ClassIDTool
    {
        static Dictionary<Type, ClassIDType> classidmap;

        static ClassIDTool()
        {
            classidmap = new Dictionary<Type, ClassIDType>();
            classidmap.Add(typeof(AssetBundle), ClassIDType.AssetBundle);
            classidmap.Add(typeof(GameObject), ClassIDType.GameObject);
            classidmap.Add(typeof(Mesh), ClassIDType.Mesh);
            classidmap.Add(typeof(Material), ClassIDType.Material);
            classidmap.Add(typeof(Texture2D), ClassIDType.Texture2D);
            classidmap.Add(typeof(Transform), ClassIDType.Transform);
            classidmap.Add(typeof(Shader), ClassIDType.Shader);
            classidmap.Add(typeof(SkinnedMeshRenderer), ClassIDType.SkinnedMeshRenderer);
        }

        /// <summary>
        /// return the knowed class id
        /// </summary>
        public static bool TryGetClassID<T>(out ClassIDType result) where T : ObjectBase
        {
            result = ClassIDType.UnknownType;
            return classidmap.TryGetValue(typeof(T), out result);
        }
    }



	public static class BinaryReaderExtensions
    {
        public static void AlignStream(this BinaryReader reader, int alignment = 4)
        {
            var pos = reader.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0) reader.BaseStream.Position += alignment - mod;
        }
        public static string ReadAlignedString(this BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length > 0 && length <= reader.BaseStream.Length - reader.BaseStream.Position)
            {
                var result = Encoding.UTF8.GetString(reader.ReadBytes(length));
                reader.AlignStream(4);
                return result;
            }
            return "";
        }

        public static float[] ReadSingleArray(this BinaryReader reader) 
            => ReadArray(reader.ReadSingle, reader.ReadInt32());
        
        public static string[] ReadStringArray(this BinaryReader reader) 
            => ReadArray(reader.ReadAlignedString, reader.ReadInt32());
        
        public static uint[] ReadUInt32Array(this BinaryReader reader) 
            => ReadArray(reader.ReadUInt32, reader.ReadInt32());
       
        public static uint[][] ReadUInt32ArrayArray(this BinaryReader reader)
        {
            int dim = reader.ReadInt32();
            uint[][] array = new uint[dim][];
            for (int i=0;i<dim;i++)
            {
                array[i] = reader.ReadUInt32Array(reader.ReadInt32());
            }
            return array;
        }

        private static T[] ReadArray<T>(Func<T> del, int length)
        {
            var array = new T[length];
            Array.ForEach(array, x => x = del());
            //for (int i = 0; i < length; i++) array[i] = del();
            return array;
        }
    }
    public static class BinaryWriterExtensions
    {
        public static void AlignStream(this BinaryWriter bw, int alignment)
        {
            var pos = bw.BaseStream.Position;
            int mod = (int)(pos % alignment);
            bw.WriteZero(alignment - mod);
        }
    }
}
