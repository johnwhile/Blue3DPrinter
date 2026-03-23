using Common.Maths;
using System.IO;

namespace Blue3DPrinter
{
    public class Utils
    {
        public static float RoundFloat(float f)
        {
            return (float)System.Math.Round(f, 3);
        }
        public static Vector2f RoundVector2f(Vector2f v)
        {
            return new Vector2f(System.Math.Round(v.x, 3), System.Math.Round(v.y, 3));
        }
        public static Vector2f RoundVector2f(Vector3f v)
        {
            return new Vector2f(System.Math.Round(v.x, 3), System.Math.Round(v.y, 3));
        }
        public static Vector3ui ReadVector3ui(BinaryReader br)
        {
            return new Vector3ui(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
        }


        /// <summary>
        /// 32bit :
        /// x = (last 12 bit) 
        /// y = (mid 8 bit = 1byte)
        /// z = (first 12 bit)
        /// </summary>
        public static Vector3i ReadVector3i_12_8_12(BinaryReader br)
        {
            int tmp = (int)br.ReadUInt32();
            return new Vector3i(((tmp >> 20) & 4095) - 2048, (tmp >> 12) & 255, (tmp & 4095) - 2048);
        }

        /// <summary>
        /// 32 bit:
        /// x = (last 11 bit)
        /// y = (mid 10 bit)
        /// z = (first 11 bit)
        /// </summary>
        public static Vector3i ReadVector3i_11_10_11(BinaryReader br)
        {
            int tmp = (int)br.ReadUInt32();
            return new Vector3ui(((tmp >> 21) & 2047) - 1024, (tmp >> 11) & 1023, (tmp & 2047) - 1024);
        }

        /// <summary>
        /// 64 bit :
        /// x = last 18 bit + (10bit zero ?)
        /// y = mid 18 bit
        /// z = fist 18 bit
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static Vector3i ReadVector3i_18_18_18(BinaryReader br)
        {
            long tmp = (long)br.ReadUInt64();
            return new Vector3i((int)((tmp >> 36) & 262143L) - 131072, (int)((tmp >> 18) & 262143L) - 131072, (int)(tmp & 262143L) - 131072);
        }

        /// <summary>
        /// new version of CopyTo 
        /// </summary>
        public static void CopyStream(Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[1024];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, System.Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }

        public static int CharToInt(char c)
        {
            return c - '0';
        }

        public static string mytxtformat(string key, object value, int bigger = 22)
        {
            var name = key.ToString();
            var val = value != null ? value.ToString() : "";
            string whitespace = "";
            for (int i = 0; i < bigger - name.Length; i++) whitespace += " ";
            return "<" + name + ">" + whitespace + val;
        }

    }
}
