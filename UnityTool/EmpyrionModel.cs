using Common;
using Common.Maths;
using Common.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using static Common.Maths.Mesh;


namespace UnityTool
{

    /// <summary>
    /// Semplified and Filter scene tree, contains only relevant meshes
    /// </summary>
    public class EmpyrionModel
    {
        public readonly static long EmpSignature = BitConverterExt.ToInt64("EMPMODEL");

        public readonly SceneTree Tree;

        //the asset contain many nodes for other functions (like collision or meshshadow) that it don't need
        static HashSet<string> skipfilter;
        static HashSet<string> nodefilter;


        public string Name => Tree.Name;

        static EmpyrionModel()
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

        //can be omitted but it's possible to avoid to read more time the same asset
        //internal HashSet<Renderer> loadedAssetRenderer = new HashSet<Renderer>();

        public EmpyrionModel(string name = "EmpyrionModel") 
        {
            Tree = new SceneTree(name);
        }

        public EmpyrionModel(BinaryReader reader) : this()
        {
            Read(reader);
        }

        public EmpyrionModel(XmlReader reader) : this()
        {
            Read(reader);
        }

        private EmpyrionModel(GameObject obj, UnityFileReader reader) : this(obj.Name)
        {
            RecursiveMeshObjectWalker(Tree.Root, obj, reader);

            for (int i = 0; i < Tree.ElementsCount; i++)
            {
                if (Tree.Element[i] is TriMesh mesh)
                {
                    //var submesh = mesh.CollapseSubMeshes();
                    //if (submesh != null) submesh.Name = mesh.Name;
                }
            }
        }

        public static EmpyrionModel FromAsset(GameObject obj, UnityFileReader reader)
        {
            return new EmpyrionModel(obj, reader);
        }

        /// <summary>
        /// Add mesh if found
        /// </summary>
        static void RecursiveMeshObjectWalker(SceneNode node, GameObject gameobject, UnityFileReader reader)
        {
            //if (gmobj.m_Name== "BlastShutterDoorsBase")
            //    Debug.WriteLine(gmobj.m_Transform.m_LocalRotation);

            //TODO: this must not happen
            if (gameobject.Transform == null)
            {
                gameobject.LinkTransforms(reader);
                if (gameobject.Transform == null) return;
            }

            node.Name = gameobject.Name;
            node.LocalTransform = Convert(gameobject.Transform);


            //link the other mesh's container
            gameobject.LinkComponents(reader);

            Renderer renderer = gameobject.MeshRenderer;
            if (renderer == null) renderer = gameobject.SkinnedMeshRenderer;
            if (renderer != null)
            {
                node.Element = AssetManager.GetGeometry(reader, renderer);
            }
            foreach (var childPtr in gameobject.Transform.Children)
                if (childPtr.TryGet(reader, out var childTrans))
                {
                    if (childTrans.GameObject.TryGet(reader, out var childobject))
                    {
                        // all children don't contain the data that i want
                        if (skipfilter.Contains(childobject.Name))
                        {
                            //SceneNode childnode = new SceneNode(node.Tree, childobject.Name);
                            //node.Children.AddLast(childnode);
                            //RecursiveNodeObjectWalker(childnode, childobject, reader);
                        }
                        else
                        {
                            SceneNode childnode = new SceneNode(node.Tree, childobject.Name);
                            node.Children.AddLast(childnode);
                            RecursiveMeshObjectWalker(childnode, childobject, reader);
                        }
                    }
                }
        }
        /// <summary>
        /// add only empty nodes
        /// </summary>
        static void RecursiveNodeObjectWalker(SceneNode node, GameObject gmobj, UnityFileReader reader)
        {
            node.Name = gmobj.Name;
            node.LocalTransform = Convert(gmobj.Transform);

            foreach (var childPtr in gmobj.Transform.Children)
                if (childPtr.TryGet(reader, out var childTrans))
                {
                    if (childTrans.GameObject.TryGet(reader, out var childObject))
                    {
                        SceneNode childnode = new SceneNode(node.Tree, childObject.Name);
                        node.Children.AddLast(childnode);
                        RecursiveNodeObjectWalker(childnode, childObject, reader);
                    }
                }
        }

        /// <summary>
        /// try the -x conversion like in asset studio source code
        /// </summary>
        public static Matrix4x4f Convert(Transform transform)
        {
            if (transform == null) throw new ArgumentNullException("transform is null");

            var quat = transform.LocalRotation;
            quat.y *= -1;
            quat.z *= -1;

            var pos = transform.LocalPosition;
            pos.x *= -1;

            //the asset transform are in row-major vector so use post-multiply instead pre-multiply
            return Matrix4x4f.Translating(pos) * Matrix4x4f.Rotating(quat) * Matrix4x4f.Scaling(transform.LocalScale);
        }


        public bool Read(BinaryReader reader)
        {
            long signature = reader.ReadInt64();
            long bytesize =  reader.ReadInt64();

            if (signature != EmpSignature) throw new Exception("Wrong header for Empyrion EmpyrionModel class");

            if (!Tree.Read(reader)) return false;

            for (int i = 0; i < Tree.ElementsCount; i++)
            {
                //read only mesh classes
                if (reader.ReadBoolean())
                {
                    (long meshsignature, long meshsize) = ReadHeaderAndBack(reader);
                    if (meshsignature != TriMesh.TriMeshSignature &&
                        meshsignature != Common.Maths.Mesh.MeshSignature)
                        throw new Exception("Can't read unknow elements, must be a mesh");

                    var tmesh = new TriMesh();
                    tmesh.Read(reader);
                    //tmesh.ReadOldVersion(reader);


                    Tree.Element[i] = tmesh;
                }
                else
                {
                    Tree.Element[i]= null;
                }
            }

            return true;
        }

        public bool Write(BinaryWriter writer, TransformVersion matversion = TransformVersion.Float16)
        {
            long begin = writer.BaseStream.Position;
            writer.WriteLong(EmpSignature);
            writer.WriteLong();
            //write the tree
            if (!Tree.Write(writer, matversion)) return false;

            //write the meshes
            for (int i = 0; i < Tree.ElementsCount; i++)
            {
                //write only mesh classes
                if (Tree.Element[i] is TriMesh mesh)
                {
                    writer.Write(true);
                    //var flag = matversion == TransformVersion.Decomposed ? Compression.MatrixTRS : Compression.None;
                    //flag |= Compression.PackedNormals24;
                    //mesh.Write(writer, flag);
                    mesh.Write(writer, CompressionTransform.MatrixTRS, CompressionIndices.None, CompressionVertices.None, CompressionNormals.Normals24, CompressionTexCoord.None, CompressionColor.None);
                }
                else
                {
                    writer.Write(false);
                }
            }
            long end = writer.BaseStream.Position;
            writer.BaseStream.Position = begin + 8;
            writer.WriteLong(end - begin);
            writer.BaseStream.Position = end;
            return true;
        }

        public bool Read(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public bool Write(XmlWriter writer, TransformVersion matversion = TransformVersion.Float16)
        {
            //write the tree
            writer.WriteComment($"This file version of {Tree.Name}.treemesh is only for debugging purpose");
            if (!Tree.Write(writer, matversion)) return false;

            //write the meshes
            writer.WriteComment("elements array index match with ElementRef of SceneTree");
            writer.WriteStartElement("Elements");
            writer.WriteAttributeString("Count", Tree.ElementsCount.ToString());

            for (int i = 0; i < Tree.ElementsCount; i++)
            {
                if (Tree.Element[i] is TriMesh mesh)
                {
                    //var flag = matversion == TransformVersion.Decomposed ? Compression.MatrixTRS : Compression.None;
                    //mesh.Write(writer, flag);
                    mesh.Write(writer);
                }
            }
            writer.WriteEndElement(); // end of "Elements"
            return true;
        }
    }
}
