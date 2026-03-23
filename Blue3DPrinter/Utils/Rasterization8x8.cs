using System;

using System.Text;



using Common.Maths;

namespace Blue3DPrinter
{
    /// <summary>
    /// Rasterize a triangle into a 8x8 table. This table will be use to compare different surfaces faster,
    /// considering a big approximation.
    /// </summary>
    /// <remarks>
    /// <code>
    ///<para>1  2  3  4  5  6  7  8</para>
    ///<para>9  _  _  _  _  _  _  16</para>
    ///<para>17 _  _  _  _  _  _  24</para>
    ///<para>25 _  _  _  _  _  _  32</para>
    ///<para>33 _  _  _  _  _  _  40</para>
    ///<para>41 _  _  _  _  _  _  48</para>
    ///<para>49 _  _  _  _  _  _  56</para>
    ///<para>57 58 59 60 61 62 63 64</para>
    /// </code>
    /// ----> j
    /// |
    /// v
    /// i
    ///      y
    ///      |
    ///   x--+
    ///       \
    ///        z
    /// </remarks>
    public class RasterizedPoly64
    {
        const ulong BIT1 = 1;
        const ulong FULL = ulong.MaxValue;
        float xmin, ymin, xmax, ymax;
        float dx, dy;
        ulong table;


        public RasterizedPoly64(ulong initialTable) : this()
        {
            table = initialTable;
        }
        public RasterizedPoly64(float xmin = -.49f, float ymin = -.49f, float xmax = .49f, float ymax = .49f, ulong initialTable = 0)
        {
            this.xmin = xmin;
            this.ymin = ymin;
            this.xmax = xmax;
            this.ymax = ymax;
            dx = (xmax - xmin) / 7f;
            dy = (ymax - ymin) / 7f;
            table = initialTable;
        }

        /// <summary>
        /// A point is out of range if &lt; (xmin - deltax) or &gt; (xmax + deltax)
        /// this is useful for correct issue about float operations
        /// </summary>
        public float GetDiscretizedDeltaX => dx;
        /// <summary>
        /// <see cref="GetDiscretizedDeltaX"/>
        /// </summary>
        public float GetDiscretizedDeltaY => dy;
        /// <summary>
        /// get the surface mask as 8x8 bit field
        /// </summary>
        public ulong GetDiscretizedMask => table;

        /// <summary>
        /// default block cube face is in range [-0.5 ; 0.5] where +Z same orientation of face
        /// Initial state is empty set
        /// </summary>
        public static RasterizedPoly64 Empty() => new RasterizedPoly64();
        /// <summary>
        /// default block cube face is in range [-0.5 ; 0.5] where +Z same orientation of face.
        /// Initial state is fully set
        /// </summary>
        public static RasterizedPoly64 Full() => new RasterizedPoly64(FULL);

        private void CoordToLocation(float x, float y, out int i, out int j)
        {
            x = (x - xmin) / (xmax - xmin) / dx;
            y = (y - ymin) / (ymax - ymin) / dy;
            j = (int)(7 - x);
            i = (int)(7 - y);
            if (j < 0 || i < 0 || j > 7 || i > 7) throw new ArithmeticException("the coord are out of table range");
        }

        private bool CoordToBit(float x, float y)
        {
            int i, j;
            CoordToLocation(x, y, out i, out j);
            ulong bit = BIT1 << (8 * i + j);
            return (table & bit) > 0;
        }

        private void OffsetToCoord(int n, out float x, out float y)
        {
            if (n < 0 || n > 63) throw new ArgumentOutOfRangeException("the table is 8x8 so n must be in range [0;63]");
            int i = n / 8;
            int j = n % 8;
            x = xmax - j * dx;
            y = ymax - i * dy;
        }
        public void AddTriangleMask(Vector2f a, Vector2f b, Vector2f c)
        {
            var v1 = b - a;
            var v2 = c - a;

            //where j = -X and i = -Y and Face direction is +Z
            ulong bit = BIT1;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    //float x, y; GetCoordAtBit(8 * i + j, out x, out y);
                    //float x = xmax - j * dx;
                    //float y = ymax - i * dy;

                    //Vector2f p = new Vector2f(x - a.x, y - a.y);

                    //float d = Vector2f.Cross(v1, v2);

                    //float s = (float)Vector2f.Cross(p, v2) / d;
                    //float t = (float)Vector2f.Cross(v1, p) / d;

                    //if ((s >= 0) && (t >= 0) && (s + t <= 1))
                    //{
                    //    table |= bit;
                    //}
                    if (Mathelp.IsPointInsideTriangle(a, b, c, xmax - j * dx, ymax - i * dy)) table |= bit;
                    bit <<= 1;
                }
        }


        /// <summary>
        /// Return the surface that can be hide
        /// </summary>
        public static HideResult Compare(RasterizedPoly64 left, RasterizedPoly64 right)
        {
            ulong L = left.table;
            ulong R = right.table;

            if (L == 0 && R == 0) return HideResult.None;
            if (L == FULL && R== FULL) return HideResult.Both;
            if (L == FULL && R != FULL) return HideResult.Right;
            if (L != FULL && R == FULL) return HideResult.Left;

            if (L != FULL && R != FULL)
            {
                //the R surface is completly inside L
                if ((L | R) == L) return HideResult.Right;
                if ((L | R) == R) return HideResult.Left;
            }     
            
            return HideResult.None;
        }



        public override string ToString()
        {
            StringBuilder s = new StringBuilder(64 + 8);
            ulong bit = BIT1;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    s.Append((table & bit) > 0 ? '1' : '0');
                    s.Append(' ');
                    bit <<= 1;
                }
                s.AppendLine();
            }
            return s.ToString();
        }


    }


    public class RasterizedPoly8x8
    {
        public readonly Cardinal Direction;

        static readonly Matrix4x4f[] lockat;
        static readonly Matrix4x4f[] lockatInv;
        /// <summary>
        /// local transform of vertices to plane oriented to "direction"
        /// </summary>
        public static Matrix4x4f GetViewMatrix(Cardinal direction) { return lockat[(int)direction]; }
        public static Matrix4x4f GetInvertedViewMatrix(Cardinal direction) { return lockatInv[(int)direction]; }

        public static Vector3f GetUpView(Cardinal direction)
        {
            Matrix4x4f view = GetViewMatrix(direction);
            return new Vector3f(view.m01, view.m11, view.m21);
        }

        const ulong BIT1 = 1;

        float xmin, ymin, xmax, ymax;
        float dx, dy;
        ulong table;

        static Matrix4x4f getView(Cardinal direction)
        {
            Vector3f targhet = BlueprintRotation.LockAt(direction);

            Vector3f up;
            switch (direction)
            {
                case Cardinal.Top: up = -Vector3f.UnitZ; break;
                case Cardinal.Bottom: up = Vector3f.UnitZ; break;
                default: up = Vector3f.UnitY;break;
            }
            return Matrix4x4f.MakeViewLH(Vector3f.Zero, targhet, up);
        }

        static RasterizedPoly8x8()
        {
            lockat = new Matrix4x4f[6];
            lockatInv = new Matrix4x4f[6];
            for (byte c = 0; c < 6; c++)
            {
                lockatInv[c] = getView((Cardinal)c);
                lockat[c] = Matrix4x4f.Inverse(lockatInv[c]);
            }
        }

        public RasterizedPoly8x8(Cardinal Direction, ulong initialTable):this(Direction)
        {
            table = initialTable;
        }
        public RasterizedPoly8x8(Cardinal Direction, float xmin = -.49f, float ymin = -.49f, float xmax = .49f, float ymax = .49f, ulong initialTable = 0)
        {
            this.xmin = xmin;
            this.ymin = ymin;
            this.xmax = xmax;
            this.ymax = ymax;
            dx = (xmax - xmin) / 7f;
            dy = (ymax - ymin) / 7f;
            table = initialTable;
            this.Direction = Direction;
        }


        /// <summary>
        /// A point is out of range if &lt; (xmin - deltax) or &gt; (xmax + deltax)
        /// this is useful for correct issue about float operations
        /// </summary>
        public float GetDiscretizedDeltaX => dx;
        /// <summary>
        /// <see cref="GetDiscretizedDeltaX"/>
        /// </summary>
        public float GetDiscretizedDeltaY => dy;
        /// <summary>
        /// get the surface mask as 8x8 bit field
        /// </summary>
        public ulong GetDiscretizedMask => table;

        /// <summary>
        /// default block cube face is in range [-0.5 ; 0.5] where +Z same orientation of face
        /// Initial state is empty set
        /// </summary>
        public static RasterizedPoly8x8 Empty(Cardinal direction) => new RasterizedPoly8x8(direction, 0);
        /// <summary>
        /// default block cube face is in range [-0.5 ; 0.5] where +Z same orientation of face.
        /// Initial state is fully set
        /// </summary>
        public static RasterizedPoly8x8 Full(Cardinal direction) => new RasterizedPoly8x8(direction, ulong.MaxValue);

        public void CoordToLocation(float x, float y, out int i, out int j)
        {
            x = (x - xmin) / (xmax - xmin) / dx;
            y = (y - ymin) / (ymax - ymin) / dy;
            j = (int)(7 - x);
            i = (int)(7 - y);
            if (j < 0 || i < 0 || j > 7 || i > 7) throw new ArithmeticException("the coord are out of table range");
        }

        public bool CoordToBit(float x, float y)
        {
            int i, j;
            CoordToLocation(x, y, out i, out j);
            ulong bit = BIT1 << (8 * i + j);
            return (table & bit) > 0;
        }

        public void OffsetToCoord(int n, out float x, out float y)
        {
            if (n < 0 || n > 63) throw new ArgumentOutOfRangeException("the table is 8x8 so n must be in range [0;63]");
            int i = n / 8;
            int j = n % 8;
            x = xmax - j * dx;
            y = ymax - i * dy;
        }


        public Vector3f OffsetToWorld(int n)
        {
            Vector3f worldPoint = new Vector3f();
            OffsetToCoord(n, out worldPoint.x, out worldPoint.y);
            worldPoint.TransformCoordinate(GetInvertedViewMatrix(Direction));
            return worldPoint;
        }


        /// <summary>
        /// Add Local vertices
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public void RasterizeTriangle(Vector3f a, Vector3f b, Vector3f c)
        {
            Matrix4x4f transform = GetViewMatrix(Direction);
            a.TransformCoordinate(transform);
            b.TransformCoordinate(transform);
            c.TransformCoordinate(transform);

            //loking at +Z mean all z coord lies to plane
            AddTriangleMask(new Vector2f(a.x, a.y), new Vector2f(b.x, b.y), new Vector2f(c.x, c.y));
        }

        private void AddTriangleMask(Vector2f a, Vector2f b, Vector2f c)
        {
            var v1 = b - a;
            var v2 = c - a;

            //where j = -X and i = -Y and Face direction is +Z
            ulong bit = BIT1;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    //float x, y; GetCoordAtBit(8 * i + j, out x, out y);
                    //float x = xmax - j * dx;
                    //float y = ymax - i * dy;

                    //Vector2f p = new Vector2f(x - a.x, y - a.y);

                    //float d = Vector2f.Cross(v1, v2);

                    //float s = (float)Vector2f.Cross(p, v2) / d;
                    //float t = (float)Vector2f.Cross(v1, p) / d;

                    //if ((s >= 0) && (t >= 0) && (s + t <= 1))
                    //{
                    //    table |= bit;
                    //}
                    if (Mathelp.IsPointInsideTriangle(a, b, c, xmax - j * dx, ymax - i * dy)) table |= bit;
                    bit <<= 1;
                }
        }

        public void RotateCW90()
        {
            ulong new_table = 0;
            ulong bit = BIT1;

            for (int j = 0; j < 8; j++)
                for (int i = 7; i >= 0; i--)
                {
                    int n = 8 * i + j;
                    if ((table & (ulong)1 << n) > 0)
                        new_table |= bit;
                    bit <<= 1;
                }
            table = new_table;
        }
        public void RotateCW180()
        {
            ulong new_table = 0;
            ulong bit = BIT1;

            for (int j = 7; j >= 0; j--)
                for (int i = 7; i >= 0; i--)
                {
                    int n = 8 * j + i;
                    if ((table & (ulong)1 << n) > 0)
                        new_table |= bit;
                    bit <<= 1;
                }
            table = new_table;
        }
        public void RotateCW270()
        {
            ulong new_table = 0;
            ulong bit = BIT1;

            for (int j = 7; j >= 0; j--)
                for (int i = 0; i < 8; i++)
                {
                    int n = 8 * i + j;
                    if ((table & (ulong)1 << n) > 0)
                        new_table |= bit;
                    bit <<= 1;
                }
            table = new_table;
        }


        public static HideResult Overlap(
            RasterizedPoly8x8 L, Cardinal Ldir, Matrix4x4f Ltrs,
            RasterizedPoly8x8 R, Cardinal Rdir, Matrix4x4f Rtrs)
        {
            if (L.table == 0 && R.table == 0) return HideResult.None;
            if (L.table == ulong.MaxValue && R.table == ulong.MaxValue) return HideResult.Both;


            for (int n = 0; n < 64; n++)
            {
                float x, y;
                L.OffsetToCoord(n, out x, out y);
                var Lview = GetViewMatrix(Ldir);
                var worldVector = new Vector3f(x, y, 0).TransformCoordinate(in Lview);
            }
            



            return HideResult.None;
        }


        public override string ToString()
        {
            StringBuilder s = new StringBuilder(64 + 8);
            ulong bit = BIT1;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    s.Append((table & bit) > 0 ? '1' : '0');
                    s.Append(' ');
                    bit <<= 1;
                }
                s.AppendLine();
            }
            return s.ToString();
        }

    }
}
