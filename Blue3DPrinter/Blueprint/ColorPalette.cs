using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Maths;
using Common.IO.Wavefront;

namespace Blue3DPrinter
{
    /// <summary>
    /// The enumerable method return only colors used by block
    /// </summary>
    /// <remarks>
    /// the first color are stored as black but this would create a "misunderstanding" when rendering so i set it as Grey
    /// </remarks>
    public class ColorPalette : IEnumerable<Color4b>
    {
        const uint bit1 = 1;
        uint markAsUsed;

        Color4b[] colors; 

        public ColorPalette()
        {
            colors = new Color4b[32];
            markAsUsed = 0;
        }

        public static ColorPalette Default
        {
            get
            {
                ColorPalette palette = new ColorPalette();
                palette.colors[0] = new Color4b(0, 0, 0);
                palette.colors[8] = new Color4b(220, 220, 220);
                palette.colors[16] = new Color4b(190, 190, 190);
                palette.colors[24] = new Color4b(130, 130, 130);

                /*
                palette.colors[0] = new Vector4b(0, 0, 0);
                palette.colors[1] = new Vector4b(255, 255, 255);
                palette.colors[2] = new Vector4b(0, 0, 0);
                palette.colors[3] = new Vector4b(0, 0, 0);
                palette.colors[4] = new Vector4b(0, 0, 0);
                palette.colors[5] = new Vector4b(0, 0, 0);
                palette.colors[6] = new Vector4b(0, 0, 0);
                palette.colors[7] = new Vector4b(0, 0, 0);
                palette.colors[8] = new Vector4b(0, 0, 0);
                palette.colors[9] = new Vector4b(0, 0, 0);
                palette.colors[10] = new Vector4b(0, 0, 0);
                palette.colors[11] = new Vector4b(0, 0, 0);
                palette.colors[12] = new Vector4b(0, 0, 0);
                palette.colors[13] = new Vector4b(0, 0, 0);
                palette.colors[14] = new Vector4b(0, 0, 0);
                palette.colors[15] = new Vector4b(0, 0, 0);
                palette.colors[16] = new Vector4b(0, 0, 0);
                palette.colors[17] = new Vector4b(0, 0, 0);
                palette.colors[18] = new Vector4b(0, 0, 0);
                palette.colors[19] = new Vector4b(0, 0, 0);
                palette.colors[20] = new Vector4b(0, 0, 0);
                palette.colors[21] = new Vector4b(0, 0, 0);
                palette.colors[22] = new Vector4b(0, 0, 0);
                palette.colors[23] = new Vector4b(0, 0, 0);
                palette.colors[24] = new Vector4b(0, 0, 0);
                palette.colors[25] = new Vector4b(0, 0, 0);
                palette.colors[26] = new Vector4b(0, 0, 0);
                palette.colors[27] = new Vector4b(0, 0, 0);
                palette.colors[28] = new Vector4b(0, 0, 0);
                palette.colors[29] = new Vector4b(0, 0, 0);
                palette.colors[30] = new Vector4b(0, 0, 0);
                palette.colors[31] = new Vector4b(0, 0, 0);
                */
                return palette;
            }
        }


        public int ColorUsed
        {
            get
            {
                int count = 0;
                uint b = bit1;
                for (int i = 0; i < 32; i++, b <<= 1)
                    if ((markAsUsed & b) > 0) count++;
                return count;
            }
        }

        public void SetUsedByModel(int i, bool used)
        {
            uint b = (uint)1 << i;
            if (used) markAsUsed |= b;
            else markAsUsed &= ~b;
        }

        public bool GetUsedByModel(int i)
        {
            uint b = (uint)1 << i;
            return (markAsUsed & b) > 0;
        }

        public IEnumerator<Color4b> GetEnumerator()
        {
            for (int i = 0; i < 32; i++)
                yield return colors[i];

            //uint b = bit1;
            //for (int i = 0; i < 32; i++, b <<= 1)
            //    if ((markAsUsed & b) > 0)
            //        yield return colors[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ColorPalette(BinaryReader br)
        {
            int count = br.ReadByte();
            colors = new Color4b[count];
            for (int i = 0; i < count; i++)
                colors[i] = new Color4b(br.ReadByte(), br.ReadByte(), br.ReadByte());
            colors[0] = Color4b.Gray;
        }
        public bool Write(BinaryWriter bw)
        {
            bw.Write((byte)colors.Length);

            if (colors.Length > 0)
            {
                bw.Write(false);
                bw.Write(false);
                bw.Write(false);
            }
            for (int i = 1; i < colors.Length; i++)
            {
                bw.Write(colors[i].r);
                bw.Write(colors[i].g);
                bw.Write(colors[i].b);
            }
            return true;
        }

    }
}

