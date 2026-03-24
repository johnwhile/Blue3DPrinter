using System.Collections.Generic;
using System.Linq;

using Common;
using Common.Maths;

using MyMesh = Common.Maths.TriMesh;
using MySubMesh = Common.Maths.SubMesh;

namespace UnityTool
{
    /// <summary>
    /// multi asset reader not implemented, data must be contained in only one asset file
    /// </summary>
    public class AssetManager
    {
        UnityFileReader unityreader;

        Dictionary<long, GameObject> loaded = new Dictionary<long, GameObject>();

        public List<GameObject> MainObjects = new List<GameObject>();

        public AssetManager()
        {

        }

        public static Mesh GetMesh(UnityFileReader reader, Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer skinned)
            {
                if (skinned.m_Mesh.TryGet(reader, out var mesh)) return mesh;
            }
            else
            {
                if (renderer.GameObject.TryGet(reader, out var gameobj))
                {
                    if (gameobj.MeshFilter != null && gameobj.MeshFilter.m_Mesh.TryGet(reader, out var mesh)) return mesh;
                }
            }
            return null;
        }

        public static MyMesh GetGeometry(Mesh mesh)
        {
            MyMesh mymesh = new MyMesh(Primitive.TriangleList, mesh.Name);

            // Shared vertex list
            if (mesh.Vertices?.Length > 0)
            {
                int dim = mesh.Vertices.Length == mesh.VertexCount * 4 ? 4 : 3;
                mymesh.Vertices = new StructBuffer<Vector3f>(mesh.VertexCount);
                for (int i = 0; i < mesh.VertexCount; i++)
                    mymesh.Vertices.Add(new Vector3f(
                        -mesh.Vertices[i * dim + 0],
                        mesh.Vertices[i * dim + 1],
                        mesh.Vertices[i * dim + 2]));
            }

            if (mesh.Normals?.Length > 0)
            {
                int dim = mesh.Normals.Length == mesh.VertexCount * 4 ? 4 : 3;
                mymesh.Normals = new StructBuffer<Vector3f>(mesh.VertexCount);
                for (int i = 0; i < mesh.VertexCount; i++)
                    mymesh.Normals.Add(new Vector3f(
                        -mesh.Normals[i * dim + 0],
                        mesh.Normals[i * dim + 1],
                        mesh.Normals[i * dim + 2]));
            }

            var uv = mesh.GetUV(0);
            if (uv?.Length > 0)
            {
                int dim = uv.Length == mesh.VertexCount * 3 ? 3 : 2;
                mymesh.TexCoords = new StructBuffer<Vector2f>(mesh.VertexCount);

                for (int i = 0; i < mesh.VertexCount; i++)
                    mymesh.TexCoords.Add(new Vector2f(
                        uv[i * dim + 0],
                        uv[i * dim + 1]));
            }

            if (mesh.Tangents?.Length > 0 && mesh.Tangents.Length == mesh.VertexCount * 4)
            {
                mymesh.Tangents = new StructBuffer<Vector4f>(mesh.VertexCount);

                for (int i = 0; i < mesh.VertexCount; i++)
                    mymesh.Tangents.Add(new Vector4f(
                        mesh.Tangents[i * 4 + 0],
                        mesh.Tangents[i * 4 + 1],
                        mesh.Tangents[i * 4 + 2],
                        mesh.Tangents[i * 4 + 3]));
            }
            
            if (mesh.Colors?.Length > 0)
            {
                bool hasAlpha = mesh.Colors.Length == mesh.VertexCount * 4;

                mymesh.Colors = new StructBuffer<Color4b>(mesh.VertexCount);

                if (hasAlpha) for (int i = 0; i < mesh.VertexCount; i++) mymesh.Colors.Add(new Color4b(
                    mesh.Colors[i * 4 + 0],
                    mesh.Colors[i * 4 + 1],
                    mesh.Colors[i * 4 + 2],
                    mesh.Colors[i * 4 + 3]));
                else for (int i = 0; i < mesh.VertexCount; i++) mymesh.Colors.Add(new Color4b(
                    mesh.Colors[i * 3 + 0],
                    mesh.Colors[i * 3 + 1],
                    mesh.Colors[i * 3 + 2],
                    1.0f));

            }

            for (int i = 0; i < mesh.SubMeshes.Length; i++)
            {
                int numFaces = (int)mesh.SubMeshes[i].indexCount / 3;
                var submesh = mesh.SubMeshes[i];
                MySubMesh mysub = mymesh.AddSubMesh();

                //Material mat = null;
                //if (i - firstSubMesh < Renderer.Materials.Length)
                //    if (!Renderer.Materials[i - firstSubMesh].TryGet(reader, out mat)) mat = null;

                mysub.FirstVertex = (int)mesh.SubMeshes[i].firstVertex;
                //mysub.Indices = new AttributeIndex();

                for (int f = 0; f < numFaces; f++)
                {
                    mysub.Indices.Add((int)(mesh.Indices[f * 3 + 2] - submesh.firstVertex));
                    mysub.Indices.Add((int)(mesh.Indices[f * 3 + 1] - submesh.firstVertex));
                    mysub.Indices.Add((int)(mesh.Indices[f * 3 + 0] - submesh.firstVertex));
                }
            }

            return mymesh;
        }

        public static MyMesh GetGeometry(UnityFileReader reader, Renderer Renderer)
        {
            Mesh mesh = GetMesh(reader, Renderer);
            
            if (mesh == null) return null;


            MyMesh mymesh = GetGeometry(mesh);

            // Method extracted from AssetStudio repository
            int firstSubMesh = 0;
            var subHashSet = new HashSet<int>();

            if (Renderer.StaticBatchInfo.subMeshCount > 0)
            {
                firstSubMesh = Renderer.StaticBatchInfo.firstSubMesh;
                var finalSubMesh = Renderer.StaticBatchInfo.firstSubMesh + Renderer.StaticBatchInfo.subMeshCount;
                for (int i = Renderer.StaticBatchInfo.firstSubMesh; i < finalSubMesh; i++) subHashSet.Add(i);
            }
            else if (Renderer.SubsetIndices?.Length > 0)
            {
                firstSubMesh = (int)Renderer.SubsetIndices.Min(x => x);
                foreach (var index in Renderer.SubsetIndices) subHashSet.Add((int)index);
            }
            //return mymesh;

            mymesh.SubMeshes?.Clear();

            int firstFace = 0;
            for (int i = 0; i < mesh.SubMeshes.Length; i++)
            {
                int numFaces = (int)mesh.SubMeshes[i].indexCount / 3;
                
                if (subHashSet.Count > 0 && !subHashSet.Contains(i))
                {
                    firstFace += numFaces;
                    continue;
                }
                var submesh = mesh.SubMeshes[i];
                
                MySubMesh mysub = mymesh.AddSubMesh();

                //Material mat = null;
                //if (i - firstSubMesh < Renderer.Materials.Length)
                //    if (!Renderer.Materials[i - firstSubMesh].TryGet(reader, out mat)) mat = null;

                mysub.FirstVertex = (int)mesh.SubMeshes[i].firstVertex;
                //mysub.Indices = new AttributeIndex();

                var end = firstFace + numFaces;
                for (int f = firstFace; f < end; f++)
                {
                    mysub.Indices.Add((int)(mesh.Indices[f * 3 + 2]));
                    mysub.Indices.Add((int)(mesh.Indices[f * 3 + 1]));
                    mysub.Indices.Add((int)(mesh.Indices[f * 3 + 0]));



                    //mysub.Indices.Add((int)(mesh.Indices[f * 3 + 2] - submesh.firstVertex));
                    //mysub.Indices.Add((int)(mesh.Indices[f * 3 + 1] - submesh.firstVertex));
                    //mysub.Indices.Add((int)(mesh.Indices[f * 3 + 0] - submesh.firstVertex));
                }
                firstFace = end;
            }

            return mymesh;
        }

    }

}
