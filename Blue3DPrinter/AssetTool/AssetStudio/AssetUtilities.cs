// The loading asset using this library: https://github.com/nesrak1/AssetsTools.NET
// although it's very fast, it is abandoned because of the absurdity of complexity.
// AssetStudio allows you to read meshes "quite simply" but is slow

using System.Linq;
using System.IO;
using System.Collections.Generic;

using Common;
using Common.Maths;
using Common.Tools;

namespace AssetStudio
{
    public class AssetUtils
    {
        /// <summary>
        /// untouched conversion, not coodinates fix
        /// </summary>
        public static Matrix4x4f convert_nofix(Transform T)
        {
            var s = Matrix4x4f.Scaling(T.m_LocalScale.X, T.m_LocalScale.Y, T.m_LocalScale.Z);
            var r = Matrix4x4f.Rotating(T.m_LocalRotation.X, T.m_LocalRotation.Y, T.m_LocalRotation.Z, T.m_LocalRotation.W);
            var t = Matrix4x4f.Translating(T.m_LocalPosition.X, T.m_LocalPosition.Y, T.m_LocalPosition.Z);
            var m = t * r * s;
            return m;
        }
        /// <summary>
        /// try the -x conversion like in asset studio source code
        /// </summary>
        public static Matrix4x4f convert_coordfix(Transform T)
        {
            //the asset transform are in row-majour vector so use post-muliply instea pre-multiply
            var s = Matrix4x4f.Scaling(T.m_LocalScale.X, T.m_LocalScale.Y, T.m_LocalScale.Z);
            var r = Matrix4x4f.Rotating(T.m_LocalRotation.X, -T.m_LocalRotation.Y, -T.m_LocalRotation.Z, T.m_LocalRotation.W);
            var t = Matrix4x4f.Translating(-T.m_LocalPosition.X, T.m_LocalPosition.Y, T.m_LocalPosition.Z);
            var m = t * r * s;
            return m;

            //Matrix4x4f m = scale * rotate;
            //m.Translate(-T.m_LocalPosition.X, T.m_LocalPosition.Y, T.m_LocalPosition.Z);
            //return m;
        }


        /// <summary>
        /// copy same data
        /// </summary>
        public static Vector3f CastConversion(Vector3 v)
        {
            return new Vector3f(v.X, v.Y, v.Z);
        }
        /// <summary>
        /// copy same data
        /// </summary>
        public static Quaternion4f CastConversion(Quaternion q)
        {
            return new Quaternion4f(q.X, q.Y, q.Z, q.W);
        }

        public static Mesh GetMesh(Renderer meshR)
        {
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                if (sMesh.m_Mesh.TryGet(out var m_Mesh))
                {
                    return m_Mesh;
                }
            }
            else
            {
                meshR.m_GameObject.TryGet(out var m_GameObject);
                if (m_GameObject.m_MeshFilter != null)
                {
                    if (m_GameObject.m_MeshFilter.m_Mesh.TryGet(out var m_Mesh))
                    {
                        return m_Mesh;
                    }
                }
            }

            return null;
        }
    }

    public class BundleUtilities
    {
        //the asset contain many nodes for other functions (like collision or meshshadow) that it don't need
        static HashSet<string> skipfilter;
        static HashSet<string> nodefilter;

        public BundleUtilities()
        {
            // node that preclude the insertion of the whole hierarchy
            skipfilter = new HashSet<string>(10)
            {
                "LOD0_D1",
                "LOD0_D2",
                "LOD0_D3",
                "MeshShadow",
                "Collider",
                "SymType_1",
                "SymType_2",
                "D1",
                "D2",
                "D3"
            };

            // node that contain the necessary geometry
            nodefilter = new HashSet<string>(2)
            {
                "LOD0", //contain cardinal's mesh, require a name parser
                "Mesh" //contain mesh or node "D0" "D1" "D2" "D3", take only D0 and possibly all the children if exist
            };
        }


        /// <summary>
        /// Return a list of Parent GameObject, all other GameObects are children
        /// </summary>
        public List<GameObject> GetMainObjects(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("");

            Debugg.Info("> reading Bundle asset list from : " + Path.GetFileName(filename));

            AssetsManager assetsManager = new AssetsManager();
            assetsManager.LoadFiles(filename);

            List<GameObject> mainobjects = new List<GameObject>();

            //only for this game, the first assetfiles contain the list
            if (assetsManager.assetsFileList.Count > 0)
            {

                var assetsFile = assetsManager.assetsFileList[0];
                //foreach (var assetsFile in assetsManager.assetsFileList)
                {
                    Debugg.Info($"> building hierarchy for {assetsFile.Objects.Count} total assets");

                    foreach (var obj in assetsFile.Objects)
                    {
                        if (obj is GameObject m_GameObject)
                        {
                            if (m_GameObject.m_Transform != null)
                            {
                                if (m_GameObject.m_Transform.m_Father.TryGet(out var m_Father))
                                {
                                    //if (m_Father.m_GameObject.TryGet(out var m_ParentGameObject)) { }
                                }
                                else
                                {
                                    mainobjects.Add(m_GameObject);
                                }
                            }
                        }
                    }
                }
                Debugg.Info("> found " + mainobjects.Count + " GameObjects");
            }
            else
            {
                Debugg.Error("> error reading bundles, the list is empty");
            }
            return mainobjects;
        }

        /// <summary>
        /// This function converts the asset into a simple structure.
        /// The SceneFile is ready to be save but require another method to get only the meshes useful to create the 3d object
        /// </summary>
        /// <remarks>
        /// The node hierarchy differs a lot for each model type, for example:
        /// the animatable model contain a bone's hierarchy, hull model contain mesh for each 6 cardinal direction etc...
        /// Some branches of the hierarchy are removed because not contain the necessary geometry, some one are empty and someone else may contain unnecessary items.
        /// The <see cref="skipfilter"/> can help to reduce the number of "dead branches"
        /// Only mesh are estracted and put to <see cref="SceneNode.SceneObject"/>
        /// </remarks>
        public static SceneTree ConvertAssetToScene(GameObject m_MainObject)
        {
            SceneTree scene = new SceneTree(m_MainObject.m_Name);
            RecursiveMeshObjectWalker(scene.Root, m_MainObject);
            return scene;
        }

        /// <summary>
        /// add only empty nodes
        /// </summary>
        static void RecursiveNodeObjectWalker(SceneNode node, GameObject gmobj)
        {
            node.Name = gmobj.m_Name;
            node.LocalTransform = AssetUtils.convert_coordfix(gmobj.m_Transform);

            foreach (var childPtr in gmobj.m_Transform.m_Children)
                if (childPtr.TryGet(out var childTrans))
                {
                    if (childTrans.m_GameObject.TryGet(out var m_ChildObject))
                    {
                        SceneNode childnode = new SceneNode(node.Tree, m_ChildObject.m_Name);
                        RecursiveNodeObjectWalker(childnode, m_ChildObject);
                    }
                }
        }

        /// <summary>
        /// Add mesh if found
        /// </summary>
        static void RecursiveMeshObjectWalker(SceneNode node, GameObject gmobj)
        {
            //if (gmobj.m_Name== "BlastShutterDoorsBase")
            //    Debug.WriteLine(gmobj.m_Transform.m_LocalRotation);

            node.Name = gmobj.m_Name;
            node.LocalTransform = AssetUtils.convert_coordfix(gmobj.m_Transform);

            //debug purpose, i need to store original data to check the math conversion

            /*
            AssetTransform original = new AssetTransform("FromAssetStudio_UntouchedCoords");
            original.traslation = AssetUtils.CastConversion(gmobj.m_Transform.m_LocalPosition);
            original.rotation = AssetUtils.CastConversion(gmobj.m_Transform.m_LocalRotation);
            original.scale = AssetUtils.CastConversion(gmobj.m_Transform.m_LocalScale);
            original.transform = AssetUtils.convert_nofix(gmobj.m_Transform);
            node.Tag = original;
            */

            if (true)
            {
                ImportedMesh imesh = null;

                if (gmobj.m_MeshRenderer != null)
                    imesh = ConvertMeshRenderer(gmobj.m_MeshRenderer);

                if (gmobj.m_SkinnedMeshRenderer != null)
                    imesh = ConvertMeshRenderer(gmobj.m_SkinnedMeshRenderer);

                if (imesh != null)
                {
                    var mesh = ConvertAssetToMesh(imesh);
                    mesh.Name = node.Name;
                    node.Element = mesh;
                }
            }
            else
            {
                ///ERROR: SUB MESH WRONG INDEX OFFSET
                ///
                Mesh amesh = null;
                if (gmobj.m_MeshRenderer != null)
                    amesh = AssetUtils.GetMesh(gmobj.m_MeshRenderer);

                if (gmobj.m_SkinnedMeshRenderer != null)
                    amesh = AssetUtils.GetMesh(gmobj.m_SkinnedMeshRenderer);

                if (amesh != null)
                {
                    Common.Maths.Mesh mesh = ConvertAssetToMesh(amesh);
                    mesh.Name = node.Name;
                    node.Element = mesh;
                }
            }


            foreach (var childPtr in gmobj.m_Transform.m_Children)
                if (childPtr.TryGet(out var childTrans))
                {
                    if (childTrans.m_GameObject.TryGet(out var m_ChildObject))
                    {
                        // all children don't contain the data that i want
                        if (skipfilter.Contains(m_ChildObject.m_Name))
                        {
                            SceneNode childnode = new SceneNode(node.Tree, m_ChildObject.m_Name);
                            RecursiveNodeObjectWalker(childnode, m_ChildObject);
                        }
                        else
                        {
                            SceneNode childnode = new SceneNode(node.Tree, m_ChildObject.m_Name);
                            RecursiveMeshObjectWalker(childnode, m_ChildObject);
                        }
                    }
                }
        }

        static TriMesh ConvertAssetToMesh(ImportedMesh imesh)
        {
            var mesh = new TriMesh();

            mesh.Vertices = new StructBuffer<Vector3f>(imesh.VertexList.Count);

            foreach (var ivertex in imesh.VertexList)
                mesh.Vertices.Add(AssetUtils.CastConversion(ivertex.Vertex));

            if (imesh.hasNormal)
            {
                mesh.Normals = new StructBuffer<Vector3f>(imesh.VertexList.Count);
                foreach (var ivertex in imesh.VertexList)
                    mesh.Normals.Add(AssetUtils.CastConversion(ivertex.Normal));
            }

            if (imesh.hasUV[0])
            {
                mesh.TexCoords = new StructBuffer<Vector2f>(imesh.VertexList.Count);
                foreach (var ivertex in imesh.VertexList)
                    mesh.TexCoords.Add(new Vector2f(ivertex.UV[0][0], ivertex.UV[0][1]));
            }
            foreach (var isub in imesh.SubmeshList)
            {
                Common.Maths.SubMesh sub = mesh.AddSubMesh(isub.FaceList.Count);
                int offset = isub.BaseVertex;

                foreach (var iface in isub.FaceList)
                {
                    sub.AddPrimitive(
                        iface.VertexIndices[0]+ offset, 
                        iface.VertexIndices[1]+ offset, 
                        iface.VertexIndices[2]+ offset);
                }
            }

            return mesh;
        }

        /// <summary>
        /// NOT WORK SUBMESH WRONG OFFSET
        /// My version of <see cref="ConvertMeshRenderer"/>
        /// The coordinates system result in Directx-LH-Yup
        /// </summary>
        static TriMesh ConvertAssetToMesh(Mesh AssMesh)
        {
            var mesh = new TriMesh();


            int vcount = AssMesh.m_VertexCount;

            mesh.Vertices = new StructBuffer<Vector3f>(vcount);

            //get vertex format
            int c = AssMesh.m_Vertices.Length == vcount * 4 ? 4 : 3;

            for (int i = 0; i < vcount; i++)
                mesh.Vertices.Add(new Vector3f(
                    -AssMesh.m_Vertices[i * c],
                    AssMesh.m_Vertices[i * c + 1],
                    AssMesh.m_Vertices[i * c + 2]));


            bool HasNormals = AssMesh.m_Normals?.Length > 0;

            if (HasNormals)
            {
                mesh.Normals = new StructBuffer<Vector3f>(vcount);
                //get normal format
                c = AssMesh.m_Normals.Length == vcount * 4 ? 4 : 3;

                for (int i = 0; i < vcount; i++)
                    mesh.Normals.Add(new Vector3f(
                        -AssMesh.m_Normals[i * c],
                        AssMesh.m_Normals[i * c + 1],
                        AssMesh.m_Normals[i * c + 2]));
            }

            int subcount = AssMesh.m_SubMeshes.Length;
            mesh.SubMeshes = new List<Common.Maths.SubMesh>(subcount);


            int firstFace = 0;
            for (int i = 0; i < subcount; i++)
            {
                var AssSub = AssMesh.m_SubMeshes[i];

                int numFaces = (int)AssSub.indexCount / 3;
                var end = firstFace + numFaces;

                var submesh = mesh.AddSubMesh(numFaces);

                for (int f = firstFace; f < end; f++)
                {
                    submesh.AddPrimitive(
                        (int)(AssMesh.m_Indices[f * 3 + 2] - AssSub.firstVertex),
                        (int)(AssMesh.m_Indices[f * 3 + 1] - AssSub.firstVertex),
                        (int)(AssMesh.m_Indices[f * 3 + 0] - AssSub.firstVertex));
                }
                firstFace = end;

            }

            return mesh;
        }

        /// <summary>
        /// Method extracted from AssetStudio repository
        /// </summary>
        public static ImportedMesh ConvertMeshRenderer(Renderer meshR)
        {
            var mesh = AssetUtils.GetMesh(meshR);
            if (mesh == null)
                return null;
            var iMesh = new ImportedMesh();
            meshR.m_GameObject.TryGet(out var m_GameObject2);
            //iMesh.Path = GetTransformPath(m_GameObject2.m_Transform);
            iMesh.SubmeshList = new List<ImportedSubmesh>();
            var subHashSet = new HashSet<int>();
            //var combine = false;
            int firstSubMesh = 0;
            if (meshR.m_StaticBatchInfo?.subMeshCount > 0)
            {
                firstSubMesh = meshR.m_StaticBatchInfo.firstSubMesh;
                var finalSubMesh = meshR.m_StaticBatchInfo.firstSubMesh + meshR.m_StaticBatchInfo.subMeshCount;
                for (int i = meshR.m_StaticBatchInfo.firstSubMesh; i < finalSubMesh; i++)
                {
                    subHashSet.Add(i);
                }
                //combine = true;
            }
            else if (meshR.m_SubsetIndices?.Length > 0)
            {
                firstSubMesh = (int)meshR.m_SubsetIndices.Min(x => x);
                foreach (var index in meshR.m_SubsetIndices)
                {
                    subHashSet.Add((int)index);
                }
                //combine = true;
            }

            iMesh.hasNormal = mesh.m_Normals?.Length > 0;
            iMesh.hasUV = new bool[8];
            for (int uv = 0; uv < 8; uv++)
            {
                iMesh.hasUV[uv] = mesh.GetUV(uv)?.Length > 0;
            }
            iMesh.hasTangent = mesh.m_Tangents != null && mesh.m_Tangents.Length == mesh.m_VertexCount * 4;
            iMesh.hasColor = mesh.m_Colors?.Length > 0;

            int firstFace = 0;
            for (int i = 0; i < mesh.m_SubMeshes.Length; i++)
            {
                int numFaces = (int)mesh.m_SubMeshes[i].indexCount / 3;
                if (subHashSet.Count > 0 && !subHashSet.Contains(i))
                {
                    firstFace += numFaces;
                    continue;
                }
                var submesh = mesh.m_SubMeshes[i];
                var iSubmesh = new ImportedSubmesh();

                Material mat = null;
                if (i - firstSubMesh < meshR.m_Materials.Length)
                {
                    if (meshR.m_Materials[i - firstSubMesh].TryGet(out var m_Material))
                    {
                        mat = m_Material;
                    }
                }
                //ImportedMaterial iMat = ConvertMaterial(mat);
                //iSubmesh.Material = iMat.Name;
                iSubmesh.BaseVertex = (int)mesh.m_SubMeshes[i].firstVertex;

                //Face
                iSubmesh.FaceList = new List<ImportedFace>(numFaces);
                var end = firstFace + numFaces;
                for (int f = firstFace; f < end; f++)
                {
                    var face = new ImportedFace();
                    face.VertexIndices = new int[3];
                    face.VertexIndices[0] = (int)(mesh.m_Indices[f * 3 + 2] - submesh.firstVertex);
                    face.VertexIndices[1] = (int)(mesh.m_Indices[f * 3 + 1] - submesh.firstVertex);
                    face.VertexIndices[2] = (int)(mesh.m_Indices[f * 3 + 0] - submesh.firstVertex);
                    iSubmesh.FaceList.Add(face);
                }
                firstFace = end;

                iMesh.SubmeshList.Add(iSubmesh);
            }

            // Shared vertex list
            iMesh.VertexList = new List<ImportedVertex>((int)mesh.m_VertexCount);
            for (var j = 0; j < mesh.m_VertexCount; j++)
            {
                var iVertex = new ImportedVertex();
                //Vertices
                int c = 3;
                if (mesh.m_Vertices.Length == mesh.m_VertexCount * 4)
                {
                    c = 4;
                }
                iVertex.Vertex = new AssetStudio.Vector3(
                    -mesh.m_Vertices[j * c], 
                    mesh.m_Vertices[j * c + 1],
                    mesh.m_Vertices[j * c + 2]);

                //Normals
                if (iMesh.hasNormal)
                {
                    if (mesh.m_Normals.Length == mesh.m_VertexCount * 3)
                    {
                        c = 3;
                    }
                    else if (mesh.m_Normals.Length == mesh.m_VertexCount * 4)
                    {
                        c = 4;
                    }
                    iVertex.Normal = new AssetStudio.Vector3(
                        -mesh.m_Normals[j * c],
                        mesh.m_Normals[j * c + 1], 
                        mesh.m_Normals[j * c + 2]);
                }
                //UV
                iVertex.UV = new float[8][];
                for (int uv = 0; uv < 8; uv++)
                {
                    if (iMesh.hasUV[uv])
                    {
                        var m_UV = mesh.GetUV(uv);
                        if (m_UV.Length == mesh.m_VertexCount * 2)
                        {
                            c = 2;
                        }
                        else if (m_UV.Length == mesh.m_VertexCount * 3)
                        {
                            c = 3;
                        }
                        iVertex.UV[uv] = new[] { m_UV[j * c], m_UV[j * c + 1] };
                    }
                }
                //Tangent
                if (iMesh.hasTangent)
                {
                    iVertex.Tangent = new AssetStudio.Vector4(
                        -mesh.m_Tangents[j * 4+0],
                        mesh.m_Tangents[j * 4 + 1], 
                        mesh.m_Tangents[j * 4 + 2], 
                        mesh.m_Tangents[j * 4 + 3]);
                }
                //Colors
                if (iMesh.hasColor)
                {
                    if (mesh.m_Colors.Length == mesh.m_VertexCount * 3)
                    {
                        iVertex.Color = new AssetStudio.Color(
                            mesh.m_Colors[j * 3+0],
                            mesh.m_Colors[j * 3 + 1], 
                            mesh.m_Colors[j * 3 + 2], 
                            1.0f);
                    }
                    else
                    {
                        iVertex.Color = new AssetStudio.Color(
                            mesh.m_Colors[j * 4+0],
                            mesh.m_Colors[j * 4 + 1],
                            mesh.m_Colors[j * 4 + 2], 
                            mesh.m_Colors[j * 4 + 3]);
                    }
                }
                //BoneInfluence
                if (mesh.m_Skin?.Length > 0)
                {
                    var inf = mesh.m_Skin[j];
                    iVertex.BoneIndices = new int[4];
                    iVertex.Weights = new float[4];
                    for (var k = 0; k < 4; k++)
                    {
                        iVertex.BoneIndices[k] = inf.boneIndex[k];
                        iVertex.Weights[k] = inf.weight[k];
                    }
                }
                iMesh.VertexList.Add(iVertex);
            }
            /*
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                //Bone
                // 0 - None
                // 1 - m_Bones
                // 2 - m_BoneNameHashes

                var boneType = 0;
                if (sMesh.m_Bones.Length > 0)
                {
                    if (sMesh.m_Bones.Length == mesh.m_BindPose.Length)
                    {
                        var verifiedBoneCount = sMesh.m_Bones.Count(x => x.TryGet(out _));
                        if (verifiedBoneCount > 0)
                        {
                            boneType = 1;
                        }
                        if (verifiedBoneCount != sMesh.m_Bones.Length)
                        {
                            //尝试使用m_BoneNameHashes 4.3 and up
                            if (mesh.m_BindPose.Length > 0 && (mesh.m_BindPose.Length == mesh.m_BoneNameHashes?.Length))
                            {
                                //有效bone数量是否大于SkinnedMeshRenderer
                                var verifiedBoneCount2 = mesh.m_BoneNameHashes.Count(x => FixBonePath(GetPathFromHash(x)) != null);
                                if (verifiedBoneCount2 > verifiedBoneCount)
                                {
                                    boneType = 2;
                                }
                            }
                        }
                    }
                }
                if (boneType == 0)
                {
                    //尝试使用m_BoneNameHashes 4.3 and up
                    if (mesh.m_BindPose.Length > 0 && (mesh.m_BindPose.Length == mesh.m_BoneNameHashes?.Length))
                    {
                        var verifiedBoneCount = mesh.m_BoneNameHashes.Count(x => FixBonePath(GetPathFromHash(x)) != null);
                        if (verifiedBoneCount > 0)
                        {
                            boneType = 2;
                        }
                    }
                }

                if (boneType == 1)
                {
                    var boneCount = sMesh.m_Bones.Length;
                    iMesh.BoneList = new List<ImportedBone>(boneCount);
                    for (int i = 0; i < boneCount; i++)
                    {
                        var bone = new ImportedBone();
                        if (sMesh.m_Bones[i].TryGet(out var m_Transform))
                        {
                            bone.Path = GetTransformPath(m_Transform);
                        }
                        var convert = Matrix4x4.Scale(new AssetStudio.Vector3(-1, 1, 1));
                        bone.Matrix = convert * mesh.m_BindPose[i] * convert;
                        iMesh.BoneList.Add(bone);
                    }
                }
                else if (boneType == 2)
                {
                    var boneCount = mesh.m_BindPose.Length;
                    iMesh.BoneList = new List<ImportedBone>(boneCount);
                    for (int i = 0; i < boneCount; i++)
                    {
                        var bone = new ImportedBone();
                        var boneHash = mesh.m_BoneNameHashes[i];
                        var path = GetPathFromHash(boneHash);
                        bone.Path = FixBonePath(path);
                        var convert = Matrix4x4.Scale(new AssetStudio.Vector3(-1, 1, 1));
                        bone.Matrix = convert * mesh.m_BindPose[i] * convert;
                        iMesh.BoneList.Add(bone);
                    }
                }
            }
            
            //TODO combine mesh
            if (combine)
            {
                meshR.m_GameObject.TryGet(out var m_GameObject);
                var frame = RootFrame.FindChild(m_GameObject.m_Name);
                if (frame != null)
                {
                    frame.LocalPosition = RootFrame.LocalPosition;
                    frame.LocalRotation = RootFrame.LocalRotation;
                    while (frame.Parent != null)
                    {
                        frame = frame.Parent;
                        frame.LocalPosition = RootFrame.LocalPosition;
                        frame.LocalRotation = RootFrame.LocalRotation;
                    }
                }
            }
            */
            //MeshList.Add(iMesh);

            return iMesh;
        }



    }
}