using System;
using System.Collections.Generic;
using System.Diagnostics;

using Common;
using Common.Maths;

using Common.IO.Wavefront;
using Common.Tools;

namespace Blue3DPrinter
{
    /// <summary>
    /// SubMeshes rappresent the 6 possible surface that lies on cube's sides + 1 for generics
    /// </summary>
    public class BlockMesh : Mesh
    {
        static Matrix4x4f[] projectToPlane;

        /// <summary>
        /// use a small dictionary and avoid to create 42 items for SubMeshes
        /// </summary>
        sbyte[] m_subMap;


        static SubMesh[] Llist = new SubMesh[6];
        static SubMesh[] Rlist = new SubMesh[6];


        static BlockMesh()
        {
            projectToPlane = new Matrix4x4f[3];
            projectToPlane[0] = default(Matrix4x4f);
            projectToPlane[1] = default(Matrix4x4f);
            projectToPlane[2] = default(Matrix4x4f);

            //X projection (YZ plane)
            projectToPlane[0].m10 = projectToPlane[0].m21 = 1;
            //Y projection (XZ plane)
            projectToPlane[1].m00 = projectToPlane[0].m21 = 1;
            //Z projection (XY plane)
            projectToPlane[2].m10 = projectToPlane[0].m11 = 1;

        }

        public BlockMesh(string Name) : base(Primitive.TriangleList, Name)
        {
            Transform = Matrix4x4f.Identity;

            m_subMap = new sbyte[42];
            for (int i = 0; i < 42; i++) m_subMap[i] = -1;

            SubMeshes = new List<SubMesh>(1);

            Vertices = new StructBuffer<Vector3f>();
        }

        /// <summary>
        /// </summary>
        /// <param name="surface">from 0 to 6</param>
        /// <param name="material">from 0 to 5 (the undefined material is always the first)</param>
        static int get_subindex(int surface, int material)
        {
            return surface * 6 + material;
        }

        public SubMesh GetSubMesh(int surface, int material)
        {
            int i = m_subMap[get_subindex(surface, material)];
            if (i < 0 || i > SubMeshes.Count) return null;
            return SubMeshes[i];
        }

        public bool HasSubMesh(int surface)
        {
            for (int m = 0; m < 6; m++) if (m_subMap[get_subindex(surface, m)] >= 0) return true;
            return false;
        }

        /// <summary>
        /// Add a new geometry and sort triangles by its direction and by material
        /// </summary>
        public bool AddBlockMesh(Mesh mesh, Cardinal material, Hide removable, bool sort = true, bool collapse = false)
        {
            if (collapse) return AddBlockMeshCollapse(mesh);
            
            //for devices the material is always first
            if (material == Cardinal.Undefined) material = Cardinal.Top;

            if (sort) material = Cardinal.Top;

            int vertexOffset = Vertices.Count;

            // collapse all vertice in the world transform coordinate, because it is no longer necessary to keep this information
            //List<Vector3f> tmp_vertices = new List<Vector3f>(mesh.Vertices);
            //for (int i = 0; i < mesh.VerticesCount; i++)
            //    tmp_vertices[i] = Vector3f.TransformCoordinate(tmp_vertices[i], mesh.Transform);

            Vertices.AddRange(mesh.Vertices);
            for (int i = vertexOffset; i < mesh.VerticesCount + vertexOffset; i++)
                Vertices[i] = Vertices[i].TransformCoordinate(in mesh.Transform);

            foreach (var submesh in mesh.SubMeshes)
            {
                for (int f = 0; f < submesh.IndincesCount/3; f++)
                {
                    int i = submesh.Indices[f * 3 + 0] + vertexOffset;
                    int j = submesh.Indices[f * 3 + 1] + vertexOffset;
                    int k = submesh.Indices[f * 3 + 2] + vertexOffset;

                    if (!sort)
                    {
                        Cardinal direction = Cardinal.Undefined;
                        int index = get_subindex((int)direction, (int)material);
                        
                        if (m_subMap[index] < 0)
                        {
                            m_subMap[index] = (sbyte)SubMeshes.Count;
                            SubMeshes[m_subMap[index]] = AddSubMesh(0, IndexFormat.Index32bit, "d" + (int)direction + "m" + (int)material);
                        }
                        index = m_subMap[index];
                        SubMeshes[index].Name = mesh.Name;
                        SubMeshes[index].AddPrimitive(i, j, k);
                    }
                    else
                    {
                        var v = Vertices[j] - Vertices[i];
                        var w = Vertices[k] - Vertices[i];
                        var n = Vector3f.Cross(v, w);
                        n.Normalize();
                        var o = (Vertices[i] + Vertices[j] + Vertices[k]) / 3.0f;

                        Cardinal direction = Cardinal.Undefined;
                        float EPSILON = 0.0001f;
                        int found = 0;
                        if (n.y > EPSILON && Math.Abs(o.y - 0.5f) < EPSILON) { direction = Cardinal.Top; found++; }
                        if (-n.y > EPSILON && Math.Abs(o.y + 0.5f) < EPSILON) { direction = Cardinal.Bottom; found++; }
                        if (n.z > EPSILON && Math.Abs(o.z - 0.5f) < EPSILON) { direction = Cardinal.North; found++; }
                        if (-n.z > EPSILON && Math.Abs(o.z + 0.5f) < EPSILON) { direction = Cardinal.South; found++; }
                        // Attention there is a misconception form original coordinate +X and converted Directx -X.
                        // the x coords is in Directx notation
                        if (n.x > EPSILON && Math.Abs(o.x - 0.5f) < EPSILON) { direction = Cardinal.West; found++; }
                        if (-n.x > EPSILON && Math.Abs(o.x + 0.5f) < EPSILON) { direction = Cardinal.Est; found++; }

                        if (found > 1) direction = Cardinal.Undefined;


                        int index = get_subindex((int)direction, (int)material);

                        if (m_subMap[index] < 0)
                        {
                            m_subMap[index] = (sbyte)SubMeshes.Count;
                            SubMeshes[m_subMap[index]] = AddSubMesh(0, IndexFormat.Index32bit, "d" + (int)direction + "m" + (int)material);
                        }
                        index = m_subMap[index];

                        //only for debug because can be a conflict when you are more mesh that have triangles inside this group
                        SubMeshes[index].Name = mesh.Name;
                        SubMeshes[index].Indices.Add(i);
                        SubMeshes[index].Indices.Add(j);
                        SubMeshes[index].Indices.Add(k);
                    }
                }
            }
            return true;
        }



        bool AddBlockMeshCollapse(Mesh mesh)
        {
            int vertexOffset = Vertices.Count;

            Vertices.AddRange(mesh.Vertices);
            for (int i = vertexOffset; i < mesh.VerticesCount + vertexOffset; i++)
                Vertices[i] = Vertices[i].TransformCoordinate(in mesh.Transform);

            if (SubMeshCount == 0) AddSubMesh(0, IndexFormat.Index32bit, "collapsed_submesh");

            m_subMap[0] = 0;

            foreach (var submesh in mesh.SubMeshes)
            {
                foreach (int i in submesh.Indices)
                {
                    SubMeshes[0].Indices.Add(i + vertexOffset);
                }
            }
            
            
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Lmask"></param>
        /// <param name="Rmask"></param>
        public static void Compare(BlockHideInstance Lmask, BlockHideInstance Rmask)
        {
            if (Lmask == null || Rmask == null) return;

            var Rblock = Rmask.block;
            var Lblock = Lmask.block;


            BlockMesh Lmesh = Lblock.Model.Mesh;
            BlockMesh Rmesh = Rblock.Model.Mesh;

            int xaxis = Rblock.Position.x - Lblock.Position.x;
            int yaxis = Rblock.Position.y - Lblock.Position.y;
            int zaxis = Rblock.Position.z - Lblock.Position.z;

            eAxis axis = eAxis.None;
            if (xaxis != 0) axis |= eAxis.X;
            if (yaxis != 0) axis |= eAxis.Y;
            if (zaxis != 0) axis |= eAxis.Z;

            // get the local orientation because the object has its own custom rotation
            Cardinal Ldir;
            Cardinal Rdir;
            Matrix4x4f Ltrs;
            Matrix4x4f Rtrs;
            switch (axis)
            {
                case eAxis.X:
                    // x is in blueprint's coords system
                    Ldir = Lblock.GetRelativeCardinal(xaxis > 0 ? Cardinal.Est : Cardinal.West);
                    Rdir = Rblock.GetRelativeCardinal(xaxis > 0 ? Cardinal.West : Cardinal.Est);
                    Rtrs = Ltrs = projectToPlane[0];
                    break;
                case eAxis.Y:
                    Ldir = Lblock.GetRelativeCardinal(yaxis > 0 ? Cardinal.Top : Cardinal.Bottom);
                    Rdir = Rblock.GetRelativeCardinal(yaxis > 0 ? Cardinal.Bottom : Cardinal.Top);
                    Rtrs = Ltrs = projectToPlane[1];
                    break;
                case eAxis.Z:
                    Ldir = Lblock.GetRelativeCardinal(zaxis > 0 ? Cardinal.North : Cardinal.South);
                    Rdir = Rblock.GetRelativeCardinal(zaxis > 0 ? Cardinal.South : Cardinal.North);
                    Rtrs = Ltrs = projectToPlane[2];
                    break;
                default:
                    Debug.WriteLine("> left or right is not an adjacent block");
                    return;
            }

            //doesn't exit a mesh for selected surface
            if (!Lmesh.HasSubMesh((int)Ldir)) return;
            if (!Rmesh.HasSubMesh((int)Rdir)) return;

            //foreach vertex in Right's surface of Left mesh check if it's inside the Left's surface of Right Mesh
            CompareSurfaces_new(Lmask, Ldir, Rmask, Rdir, axis);
            CompareSurfaces_new(Rmask, Rdir, Lmask, Ldir, axis);

        }
        static void CompareSurfaces_new(BlockHideInstance Lmask, Cardinal LlookAt, BlockHideInstance Rmask, Cardinal RlookAt, eAxis axis)
        {
            var Rblock = Rmask.block;
            var Lblock = Lmask.block;

            BlockMesh Lmesh = Lblock.Model.Mesh;
            BlockMesh Rmesh = Rblock.Model.Mesh;

            //build rasterized surfaces
            RasterizedPoly64 lsurface = Rasterize(Lblock, LlookAt, axis);
            RasterizedPoly64 rsurface = Rasterize(Rblock, RlookAt, axis);

            var result = RasterizedPoly64.Compare(lsurface, rsurface);

            if ((result & HideResult.Left)>0)
            {
                for (int m = 0; m < 6; m++)
                {
                    SubMesh sub = Lmesh.GetSubMesh((int)LlookAt, m);
                    if (sub == null) continue;

                    for (int f = 0; f < sub.IndincesCount/3; f++)
                    {
                        int i = sub.Indices[f * 3];
                        int j = sub.Indices[f * 3 + 1];
                        int k = sub.Indices[f * 3 + 2];

                        Lmask.verticesMap[i] = false;
                        Lmask.verticesMap[j] = false;
                        Lmask.verticesMap[k] = false;
                    }
                }
            }
            if ((result & HideResult.Right) > 0)
            {
                for (int m = 0; m < 6; m++)
                {
                    SubMesh sub = Rmesh.GetSubMesh((int)RlookAt, m);
                    if (sub == null) continue;

                    for (int f = 0; f < sub.IndincesCount / 3; f++)
                    {
                        int i = sub.Indices[f * 3];
                        int j = sub.Indices[f * 3 + 1];
                        int k = sub.Indices[f * 3 + 2];

                        Rmask.verticesMap[i] = false;
                        Rmask.verticesMap[j] = false;
                        Rmask.verticesMap[k] = false;
                    }
                }
            }

            //Debug.WriteLine("surface " + Lmesh.Name);
            //Debug.WriteLine(lsurface);

            //Debug.WriteLine("surface " + Rmesh.Name);
            //Debug.WriteLine(rsurface);

        }


        static RasterizedPoly64 Rasterize(BlockObject block, Cardinal LlookAt, eAxis axis)
        {
            var surface = RasterizedPoly64.Empty();
            var bmesh = block.Model.Mesh;

            //build rasterized surfaces
            for (int m = 0; m < 6; m++)
            {
                SubMesh sub = bmesh.GetSubMesh((int)LlookAt, m);
                if (sub == null) continue;

                for (int f = 0; f < sub.IndincesCount /3; f++)
                {
                    int i = sub.Indices[f * 3];
                    int j = sub.Indices[f * 3 + 1];
                    int k = sub.Indices[f * 3 + 2];

                    GetProjectedTriangle(bmesh.Vertices, block.GetBlockRotation(), i, j, k, axis, out Vector2f vi, out Vector2f vj, out Vector2f vk);

                    surface.AddTriangleMask(vi, vj, vk);
                }
            }
            return surface;
        }



        /// <summary>
        /// not working well
        /// </summary>
        static void CompareSurfaces(BlockHideInstance Lmask, Cardinal LlookAt, BlockHideInstance Rmask, Cardinal RlookAt, eAxis axis)
        {
            var Rblock = Rmask.block;
            var Lblock = Lmask.block;

            BlockMesh Lmesh = Lblock.Model.Mesh;
            BlockMesh Rmesh = Rblock.Model.Mesh;

            //foreach vertex in Right's surface of Left mesh check if it's inside the Left's surface of Right Mesh
            for (int m = 0; m < 6; m++)
            {
                SubMesh Lsub = Lmesh.GetSubMesh((int)LlookAt, m);
                if (Lsub == null) continue;

                for (int lf = 0; lf < Lsub.IndincesCount/3; lf++)
                {
                    int li = Lsub.Indices[lf * 3];
                    int lj = Lsub.Indices[lf * 3 + 1];
                    int lk = Lsub.Indices[lf * 3 + 2];

                    GetProjectedTriangle(Lmesh.Vertices, Lblock.GetBlockRotation(), li, lj, lk, axis, out Vector2f vLi, out Vector2f vLj, out Vector2f vLk);

                    for (int mm = 0; mm < 6; mm++)
                    {
                        SubMesh Rsub = Rmesh.GetSubMesh((int)RlookAt, mm);
                        if (Rsub == null) continue;

                        for (int rf = 0; rf < Rsub.IndincesCount / 3; rf++)
                        {
                            int ri = Rsub.Indices[rf * 3];
                            int rj = Rsub.Indices[rf * 3 + 1];
                            int rk = Rsub.Indices[rf * 3 + 2];

                            GetProjectedTriangle(Rmesh.Vertices, Rblock.GetBlockRotation(), ri, rj, rk, axis, out Vector2f vRi, out Vector2f vRj, out Vector2f vRk);

                            if (Lmask.verticesMap[li]) Lmask.verticesMap[li] = !Mathelp.IsPointInsideTriangle(vRi, vRj, vRk, vLi);
                            if (Lmask.verticesMap[lj]) Lmask.verticesMap[lj] = !Mathelp.IsPointInsideTriangle(vRi, vRj, vRk, vLj);
                            if (Lmask.verticesMap[lk]) Lmask.verticesMap[lk] = !Mathelp.IsPointInsideTriangle(vRi, vRj, vRk, vLk);
                        }
                    }
                }
            }
        }


        static void GetProjectedTriangle(StructBuffer<Vector3f> vertices, Matrix4x4f transform, int i, int j, int k, eAxis axis, out Vector2f pi, out Vector2f pj, out Vector2f pk)
        {
            Vector3f vi = vertices[i].TransformCoordinate(in transform);
            Vector3f vj = vertices[j].TransformCoordinate(in transform);
            Vector3f vk = vertices[k].TransformCoordinate(in transform);

            switch (axis)
            {
                case eAxis.X:
                    pi = Utils.RoundVector2f(new Vector2f(vi.y, vi.z));
                    pj = Utils.RoundVector2f(new Vector2f(vj.y, vj.z));
                    pk = Utils.RoundVector2f(new Vector2f(vk.y, vk.z));
                    return;
                case eAxis.Y:
                    pi = Utils.RoundVector2f(new Vector2f(vi.x, vi.z));
                    pj = Utils.RoundVector2f(new Vector2f(vj.x, vj.z));
                    pk = Utils.RoundVector2f(new Vector2f(vk.x, vk.z));
                    return;
                default:
                    pi = Utils.RoundVector2f(new Vector2f(vi.x, vi.y));
                    pj = Utils.RoundVector2f(new Vector2f(vj.x, vj.y));
                    pk = Utils.RoundVector2f(new Vector2f(vk.x, vk.y));
                    return;
            }
        }



        /// <summary>
        /// Remove ONLY triangles, because vertex can be shared for other visible triangles
        /// <see cref="Mesh.RemapMesh(BitArray1, BitArray1)"/>
        /// </summary>
        public BlockMesh RemapMesh(BlockHideInstance geometryMask)
        {
            if (SubMeshes.Count == 0) return null;

            BlockMesh mesh = new BlockMesh(Name);
            mesh.Name = Name;
            mesh.SubMeshes = new List<SubMesh>(SubMeshes.Count);
            for (int s = 0; s < SubMeshes.Count; s++) mesh.SubMeshes.Add(null);
            mesh.Transform = Transform;
            mesh.m_subMap = m_subMap;


            int[] vertexRemap = new int[VerticesCount];
            int vertCounter = 1;
            int triCounter = 0;
            var addTriangles = geometryMask.triangleMap;
            var addVertices = geometryMask.verticesMap;


            for (int s = 0; s < SubMeshes.Count; s++)
            {
                var sub = SubMeshes[s];
                if (sub == null || !sub.Enable) continue;

                SubMesh newSub = new SubMesh(this, sub.IndincesCount / 3, IndexFormat.Index32bit, sub.Name);
                mesh.SubMeshes[s] = newSub;

                int numTriangles = 0;

                for (int f = 0; f < sub.IndincesCount / 3; f++)
                {
                    if (addTriangles != null && !addTriangles[triCounter++]) continue;

                    int i = sub.Indices[f * 3];
                    int j = sub.Indices[f * 3 + 1];
                    int k = sub.Indices[f * 3 + 2];
                    //if (addVertices != null && (!addVertices[i] || !addVertices[j] || !addVertices[k])) continue;

                    if (vertexRemap[i] == 0) vertexRemap[i] = vertCounter++;
                    if (vertexRemap[j] == 0) vertexRemap[j] = vertCounter++;
                    if (vertexRemap[k] == 0) vertexRemap[k] = vertCounter++;

                    newSub.AddPrimitive(
                        vertexRemap[i] - 1,
                        vertexRemap[j] - 1,
                        vertexRemap[k] - 1);

                    numTriangles++;
                }
            }
            vertCounter--;

            mesh.Vertices = new StructBuffer<Vector3f>(vertCounter);
            for (int i = 0; i < vertCounter; i++) mesh.Vertices.Add(default(Vector3f));

            for (int i = 0; i < vertexRemap.Length; i++)
                if (vertexRemap[i] > 0)
                    mesh.Vertices[vertexRemap[i] - 1] = Vertices[i];

            if (mesh.HasNormals)
            {
                mesh.Normals = new StructBuffer<Vector3f>(vertCounter);
                for (int i = 0; i < vertCounter; i++) mesh.Normals.Add(default(Vector3f));

                for (int i = 0; i < vertexRemap.Length; i++)
                    if (vertexRemap[i] > 0)
                        mesh.Normals[vertexRemap[i] - 1] = Normals[i];
            }
            if (mesh.HasTexCoords)
            {
                mesh.TexCoords = new StructBuffer<Vector2f>(vertCounter);
                for (int i = 0; i < vertCounter; i++) mesh.TexCoords.Add(default(Vector2f));

                for (int i = 0; i < vertexRemap.Length; i++)
                    if (vertexRemap[i] > 0)
                        mesh.TexCoords[vertexRemap[i] - 1] = TexCoords[i];
            }

            if (vertCounter == 0 || mesh.SubMeshes.Count == 0) return null;

            return mesh;
        }




        public void Clear()
        {
            //Vertices.Clear();
            SubMeshes.Clear();

        }

        /// <summary>
        /// Create a debug object with material for each direction
        /// </summary>
        public void InitializeWavefontFile(out WavefrontObj objfile, out WavefrontMat matfile)
        {
            objfile = new WavefrontObj();
            Wavefront.NumberDecimalDigits = 4;

            matfile = new WavefrontMat();
            matfile.Create("ColorTop", Color4b.Green);
            matfile.Create("ColorBottom", new Color4b(128, 255, 128));
            matfile.Create("ColorNorth", Color4b.Blue);
            matfile.Create("ColorWest", new Color4b(255, 128, 128));
            matfile.Create("ColorSouth", new Color4b(128, 128, 255));
            matfile.Create("ColorEst", Color4b.Red);
            matfile.Create("ColorUndefined", Color4b.Gray);
        }
    }
}
