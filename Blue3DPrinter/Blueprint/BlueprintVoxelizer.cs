using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Common;
using Common.Maths;
using Common.IO.Wavefront;
using System.Diagnostics;

namespace Blue3DPrinter
{
    public class BlueprintVoxelizer
    {
        /// <summary> X size </summary>
        int width;
        /// <summary> Z size </summary>
        int length;
        /// <summary> Y size</summary>
        int height;
        bool alignobj;

        eAxis mirror;


        public BlueprintVoxelizer(int Width, int Length, int Height, bool alignobj, eAxis mirror = eAxis.None)
        {
            width = Width;
            length = Length;
            height = Height;
            this.alignobj = alignobj;
            this.mirror = mirror;
        }

        void CreateBlockIfNotExist(Blueprint blueprint, int x, int y, int z)
        {
            if (mirror == eAxis.None)
            {
                if (x >= width || y >= height || z >= length)
                {
                    Console.WriteLine(string.Format("> the coord {0} {1} {2} is out of bound, this must never happen", x, y, z));
                    return;
                }
            }
            else
            {
                if ((mirror & eAxis.X) > 0 && x > width / 2) return;
                if ((mirror & eAxis.Y) > 0 && y > height / 2) return;
                if ((mirror & eAxis.Z) > 0 && z > length / 2) return;
            }

            blueprint.Blocks.Create(x, y, z);
        }

        void ClampVector(ref Vector3i vector)
        {
            vector.x = Mathelp.CLAMP(vector.x, 0, width - 1);
            vector.y = Mathelp.CLAMP(vector.y, 0, height - 1);
            vector.z = Mathelp.CLAMP(vector.z, 0, length - 1);
        }

        public Blueprint Voxelize(WavefrontObj file, BlocksConfig config = null, uint preferedblock = 403)
        {
            LogMsg.Message("> voxelizing " + Path.GetFileName(file.Filename), ConsoleColor.Yellow);

            if (file.Bound.isNaN) file.UpdateBound();
            if (file.Bound.isNaN)
            {
                LogMsg.Message($"> the bound size of wavefront object is wrong {file.Bound}");
                return null;
            }
            BoundingBoxMinMax objBound = file.Bound;
            Vector3f sizeObj = objBound.Size;
            Vector3f sizeEpb = new Vector3f(width, height, length);

            Vector3f scale = sizeEpb / sizeObj;

            //mantain proportions
            if (!alignobj)
            {
                float scaleFactor = Mathelp.MIN(scale.x, scale.y, scale.z);
                scale = new Vector3f(scaleFactor, scaleFactor, scaleFactor);
            }

            int sizex = Convert.ToInt32(sizeObj.x * scale.x);
            int sizey = Convert.ToInt32(sizeObj.y * scale.y);
            int sizez = Convert.ToInt32(sizeObj.z * scale.z);

            sizex = Mathelp.CLAMP(sizex, 0, 250);
            sizey = Mathelp.CLAMP(sizey, 0, 250);
            sizez = Mathelp.CLAMP(sizez, 0, 250);

            var coordChange = Matrix4x4f.Scaling(scale) * Matrix4x4f.Translating(-objBound.Min);

            Blueprint blueprint = new Blueprint(new Vector3i(width, height, length));


            foreach (var obj in file)
            {
                int fix_index = obj.GetVertexIndexOffset();
                foreach (var grp in obj.Groups)
                {
                    for (int f = 0;f<grp.indexV.Count/3;f++)
                    {
                        int i = grp.indexV[f * 3 + 0] - fix_index;
                        int j = grp.indexV[f * 3 + 1] - fix_index;
                        int k = grp.indexV[f * 3 + 2] - fix_index;


                        //apply change of coordinate
                        Vector3f a = obj.Vertices[i].TransformCoordinate(in coordChange);
                        Vector3f b = obj.Vertices[j].TransformCoordinate(in coordChange);
                        Vector3f c = obj.Vertices[k].TransformCoordinate(in coordChange);

                        //how vertex are in blueprint coordinates, just convert float to int to get relative block position
                        Vector3i min = new Vector3i(
                            Convert.ToInt32(Mathelp.MIN(a.x, b.x, c.x)),
                            Convert.ToInt32(Mathelp.MIN(a.y, b.y, c.y)),
                            Convert.ToInt32(Mathelp.MIN(a.z, b.z, c.z)));

                        Vector3i max = new Vector3i(
                            Convert.ToInt32(Mathelp.MAX(a.x, b.x, c.x)),
                            Convert.ToInt32(Mathelp.MAX(a.y, b.y, c.y)),
                            Convert.ToInt32(Mathelp.MAX(a.z, b.z, c.z)));

                        ClampVector(ref min);
                        ClampVector(ref max);

                        //the triangle it's completly inside block
                        if (min == max)
                        {
                            blueprint.Blocks.Create(min.x, min.y, min.z);
                            continue;
                        }

                        //for mirror case, evaluate only blocks inside semi-size cutting down the max's bound
                        if ((mirror & eAxis.X) > 0)
                            if (max.x > sizex / 2) max.x = sizex / 2 + 1;
                        if ((mirror & eAxis.Y) > 0)
                            if (max.y > sizey / 2) max.y = sizey / 2 + 1;
                        if ((mirror & eAxis.Z) > 0)
                            if (max.z > sizez / 2) max.z = sizez / 2 + 1;

                        //foreach possible block check if intersect the triangle
                        //loop must be inclusive and iterate at least the first interation

                        for (int x = min.x; x <= max.x; x++)
                            for (int y = min.y; y <= max.y; y++)
                                for (int z = min.z; z <= max.z; z++)
                                {
                                    Vector3f aabb_min = new Vector3f(x, y, z);
                                    Vector3f aabb_max = aabb_min + 1;

                                    bool result =
                                        BoundingBoxMinMax.IsPointInside(aabb_min, aabb_max, a, 0.1f) ||
                                        BoundingBoxMinMax.IsPointInside(aabb_min, aabb_max, b, 0.1f) ||
                                        BoundingBoxMinMax.IsPointInside(aabb_min, aabb_max, c, 0.1f);

                                    if (!result)
                                        result = PrimitiveIntersections.IntersectAABBTriangle(aabb_min, aabb_max, a, b, c);

                                    if (result)
                                        blueprint.Blocks.Create(x, y, z);
                                }
                    }
                }
            }

            if (mirror == eAxis.X)
                for (int x = 0, xx = sizex - 1; x < sizex / 2; x++, xx--)
                    for (int y = 0; y < sizey; y++)
                        for (int z = 0; z < sizez; z++)
                            if (blueprint.Blocks[x, y, z] != null)
                                blueprint.Blocks.Create(xx, y, z);

            if (mirror == eAxis.Y)
                for (int x = 0; x < sizex; x++)
                    for (int y = 0, yy = sizey - 1; y < sizey / 2; y++, yy--)
                        for (int z = 0; z < sizez; z++)
                            if (blueprint.Blocks[x, y, z] != null)
                                blueprint.Blocks.Create(x, yy, z);


            if (mirror == eAxis.Z)
                for (int x = 0; x < sizex; x++)
                    for (int y = 0; y < sizey; y++)
                        for (int z = 0, zz = sizez - 1; z < sizez / 2; z++, zz--)
                            if (blueprint.Blocks[x, y, z] != null)
                                blueprint.Blocks.Create(x, y, zz);


            LogMsg.Message("> build blueprint file, num of blocks = " + blueprint.Blocks.Count);
            //Apply blueprint's missing data

            BlockDescription cubeDescr = null;

            if (config != null) cubeDescr = config.GetDescription((int)preferedblock);

            if (cubeDescr != null)
            {
                foreach (var block in blueprint.Blocks)
                {
                    block.BlockId = cubeDescr.BlockId;
                    block.Description = cubeDescr;
                }
            }
            else
            {
                foreach (var block in blueprint.Blocks)
                {
                    block.BlockId = 403;
                    block.Description = null;
                }
            }

            /*----------------------------------------------------------*/
            //                         HEADER
            /*----------------------------------------------------------*/
            blueprint.Blocks.UpdateSize();
            BlueprintHeader header = blueprint.Header;
            header.Properties.GenerateDefaultProperties();
            header.BlockMap.Add("Air", 0);

            if (cubeDescr != null) header.BlockMap.Add(cubeDescr.Name, cubeDescr.BlockId);
            else header.BlockMap.Add("HullFullLarge", 403);

            header.prefabType = PrefabType.BA;
            header.Size = blueprint.Blocks.Size;
            /*----------------------------------------------------------*/
            //                         REST
            /*----------------------------------------------------------*/

#if DEBUG
            file.Clear();
            WaveObject waveobj = file.Create(null);
            WaveGroup wavegrp = waveobj.Create(WavePrimitive.Triangle);

            int voffset = 0;
            foreach (var block in blueprint.Blocks)
            {
                int x = block.x;
                int y = block.y;
                int z = block.z;

                //Console.WriteLine(string.Format("build cube {0} {1} {2}", x, y, z));

                waveobj.Vertices.Add(new Vector3f(x, y + 0, z + 0));
                waveobj.Vertices.Add(new Vector3f(x, y + 0, z + 1));
                waveobj.Vertices.Add(new Vector3f(x, y + 1, z + 0));
                waveobj.Vertices.Add(new Vector3f(x, y + 1, z + 1));

                waveobj.Vertices.Add(new Vector3f(x + 1, y + 0, z + 0));
                waveobj.Vertices.Add(new Vector3f(x + 1, y + 0, z + 1));
                waveobj.Vertices.Add(new Vector3f(x + 1, y + 1, z + 0));
                waveobj.Vertices.Add(new Vector3f(x + 1, y + 1, z + 1));

                int[] cube = new int[]
                {
                    0,1,3,0,3,2,
                    0,2,4,4,2,6,
                    0,5,1,0,4,5,
                    1,5,7,1,7,3,
                    4,6,5,5,6,7,
                    2,3,7,2,7,6
                };

                /*
                int[] cube = new int[]
                {
                    0, 2, 3, 1,
                    0, 4, 6, 2,
                    0, 1, 5, 4,
                    1, 5, 7, 3,
                    4, 5, 7, 6,
                    2, 6, 7, 3
                };
                */
                for (int i = 0; i < cube.Length; i++) cube[i] += voffset;

                wavegrp.indexV.AddRange(cube);

                voffset += 8;
            }

#endif
            return blueprint;
        }
    }
}
