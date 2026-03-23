using System.IO;
using System.Runtime.InteropServices;

using Common.Maths;

namespace UnityTool
{
    public static class Globals
    {
        public const int MaxCSharpArraySize = 0x7FFFFFC7;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct Hash128
    {
        [FieldOffset(0)]
        public unsafe fixed byte h[16];
        [FieldOffset(0)]
        ulong x;
        [FieldOffset(8)]
        ulong y;

        public static Hash128 Zero => new Hash128();

        /// <summary>
        /// TODO : check endianess
        /// </summary>
        public unsafe Hash128(BinaryReader reader) : this()
        {
            for (int i = 0; i < 16; i++) h[i] = reader.ReadByte();
        }

        /// <summary>
        /// TODO : check endianess
        /// </summary>
        public unsafe void Write(BinaryWriter writer)
        {
            for (int i = 0; i < 16; i++) writer.Write(h[i]);
        }

        public override string ToString()
        {
            return x.ToString("X8") + y.ToString("X8");
        }

    }
}
