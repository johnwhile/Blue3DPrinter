using System;
using System.Diagnostics;
using Common.Maths;

namespace Blue3DPrinter
{
    /// <summary>
    /// The theory tell that there are 24 possible rotations (including zero rotation)
    /// </summary>
    /// <remarks>
    /// These informations has been extracted one by one trying all combinations so take care of my work.
    /// ATTENTION: these rotations are relative to blueprint's coordinates, the in-game directx use inverted x
    /// </remarks>
    public enum CubeRotation : byte
    {
        i = 0,
        y = 1,
        yy = 2,
        yyy = 3,
        zz = 4,
        xxy = 5,
        xx = 6,
        yxx = 7,
        yyx = 8,
        yyyx = 9,
        x = 10,
        yx = 11,
        z = 12,
        yz = 13,
        xyyyx = 14,
        zx = 15,
        xxx = 16,
        yxxx = 17,
        xyy = 18,
        xxyx = 19,
        zzz = 20,       
        xy = 21,
        xyx = 22,
        xyxx = 23,
    }

    /// <summary>
    /// 6 axis orientation (from bytes 0 to 5), last byte.maxvalue is reserved
    /// These cardinal match with meshes's name parsing and have also the same six index [0-5] of cardinal face array
    /// </summary>
    public enum Cardinal : byte
    {
        Undefined = 6,
        /// <summary>
        /// +X
        /// </summary>
        /// <remarks>
        /// Carefull, there is a misconception form original coordinate +X and converted Directx -X.
        /// I'm using the Directx coords system to render the meshes.
        /// </remarks>
        Est = 5,
        /// <summary>
        /// -X
        /// </summary>
        /// <remarks>
        /// Carefull, there is a misconception form original coordinate -X and converted Directx +X.
        /// I'm using the Directx coords system to render the meshes.
        /// </remarks>
        West = 3,
        /// <summary>
        /// +Y
        /// </summary>
        Top = 0,
        /// <summary>
        /// -Y
        /// </summary>
        Bottom = 1,
        /// <summary>
        /// +Z
        /// </summary>
        North = 2,
        /// <summary>
        /// -Z
        /// </summary>
        South = 4,
    }


    /// <summary>
    /// Necessary utils to do a correct conversion between blueprint and directx coordinates
    /// </summary>
    public static class BlueprintRotation
    {
        static readonly float Angle90;
        public static readonly Matrix4x4f[] precomputed = new Matrix4x4f[24];
        public static readonly Matrix4x4f[] precomputedDx = new Matrix4x4f[24];
        public static readonly byte[,] prerotated = new byte[6, 24];

        public static readonly Vector3f[] lockat = new Vector3f[6];
        
        static BlueprintRotation()
        {
            Angle90 = Mathelp.DegreeToRadian(90);
            computeRotations();
            computeDirections();


            lockat[(int)Cardinal.Est] = Vector3f.UnitX;
            lockat[(int)Cardinal.Top] = Vector3f.UnitY;
            lockat[(int)Cardinal.North] = Vector3f.UnitZ;
            lockat[(int)Cardinal.West] = -lockat[(int)Cardinal.Est];
            lockat[(int)Cardinal.Bottom] = -lockat[(int)Cardinal.Top];
            lockat[(int)Cardinal.South] = -lockat[(int)Cardinal.North];


        }
        public static Vector3f LockAt(Cardinal direction)
        {
            return lockat[(int)direction];
        }


        /// <summary>
        /// Precomputer cube's rotations matrix (rotation is in Blueprint's coordinate, and Matrix4 in Directx LH coordinate)
        /// </summary>
        public static Matrix4x4f GetRotationMatrix(CubeRotation rotation)
        {
            return precomputed[(int)rotation];
        }
        public static Matrix4x4f GetDxRotationMatrix(CubeRotation rotation)
        {
            return precomputedDx[(int)rotation];
        }
        /// <summary>
        /// Rotate direction in Directx LH coordinate (not blueprint's coordinate)
        /// </summary>
        public static Cardinal RotateDirection(Cardinal direction, CubeRotation rotation)
        {
            return (Cardinal)prerotated[(byte)direction, (byte)rotation];
        }

        public static void DebugPrintPrerotated()
        {
            for (byte r =0; r < 24; r++)
            {
                Debug.WriteLine("rotation " + ((CubeRotation)r).ToString());
                for (byte c = 0; c < 6; c++)
                {
                    Debug.Write(prerotated[c, r] + " ");
                }
                Debug.WriteLine("");
            }
        }

        static void computeRotations()
        {
            Debug.WriteLine(">> precompute cube's rotation matrices");

            precomputed[0] = Matrix4x4f.Identity;
            

            for (byte r = 1; r < 24; r++)
            {
                string sequence = ((CubeRotation)r).ToString();
                Matrix4x4f matrix = Matrix4x4f.Identity;
                for (int i = 0; i < sequence.Length; i++)
                {
                    switch (sequence[i])
                    {
                        // x axis is inverted because blueprint's coordinate space is different than show in game
                        case 'x': matrix = Matrix4x4f.RotationX(Angle90) * matrix; break;
                        case 'y': matrix = Matrix4x4f.RotationY(-Angle90) * matrix; break;
                        case 'z': matrix = Matrix4x4f.RotationZ(-Angle90) * matrix; break;
                    }
                }
                for (int i = 0; i < 16; i++)
                    matrix[i] = (float)Math.Round(matrix[i]);
                precomputed[r] = matrix;
            }

            precomputedDx[0] = Matrix4x4f.Identity;
            for (byte r = 1; r < 24; r++)
            {
                string sequence = ((CubeRotation)r).ToString();
                Matrix4x4f matrix = Matrix4x4f.Identity;
                for (int i = 0; i < sequence.Length; i++)
                {
                    switch (sequence[i])
                    {
                        case 'x': matrix = Matrix4x4f.RotationX(Angle90) * matrix; break;
                        case 'y': matrix = Matrix4x4f.RotationY(Angle90) * matrix; break;
                        case 'z': matrix = Matrix4x4f.RotationZ(Angle90) * matrix; break;
                    }
                }
                for (int i = 0; i < 16; i++)
                    matrix[i] = (float)Math.Round(matrix[i]);
                precomputedDx[r] = matrix;
            }
        }

        static void computeDirections()
        {
            Debug.WriteLine(">> prerotate cardinal directions");

            //zero rotation is T-B-N-W-S-E
            
            byte T = (byte)Cardinal.Top;
            byte B = (byte)Cardinal.Bottom;
            byte N = (byte)Cardinal.North;
            byte W = (byte)Cardinal.West;
            byte S = (byte)Cardinal.South;
            byte E = (byte)Cardinal.Est;

            for (byte r = 0; r < 24; r++)
            {
                string sequence = ((CubeRotation)r).ToString();

                //initial zero rotation is T-B-N-W-S-E
                for (byte c = 0; c < 6; c++) prerotated[c, r] = c;

                for (int i = 0; i < sequence.Length; i++)
                {
                    switch (sequence[i])
                    {
                        case 'x':
                            //xp and xn remain fix, other 4 direction rotates and follow left handle rule
                            byte tmp_T = prerotated[T, r];
                            prerotated[T, r] = prerotated[S, r];
                            prerotated[S, r] = prerotated[B, r];
                            prerotated[B, r] = prerotated[N, r];
                            prerotated[N, r] = tmp_T;

                            break;
                        case 'y':
                            //yp and yn remain fix other 4 direction rotates and follow left handle rule
                            byte tmp_N = prerotated[N, r];
                            prerotated[N, r] = prerotated[W, r];
                            prerotated[W, r] = prerotated[S, r];
                            prerotated[S, r] = prerotated[E, r];
                            prerotated[E, r] = tmp_N;
                            break;
                        case 'z':
                            //zp and zn remain fix other 4 direction rotates and follow left handle rule
                            byte tmp_E = prerotated[E, r];
                            prerotated[E, r] = prerotated[B, r];
                            prerotated[B, r] = prerotated[W, r];
                            prerotated[W, r] = prerotated[T, r];
                            prerotated[T, r] = tmp_E;
                            break;
                    }
                }
            }
        }


    }
}
