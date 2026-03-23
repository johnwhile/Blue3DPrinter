using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Common;
using Common.Maths;
using Common.Tools;
using FbxWrapper;

namespace FbxTool
{
    public static class FbxImporter
    {
        static bool IsValidated = false;

        static FBXManager fbxManager;

        /// <summary>
        /// The asset contain many nodes used for other functions (like collision or meshshadow) that it don't need and that
        /// not contain a 3d mesh
        /// </summary>
        public static HashSet<string> skipEmpyrionFbxNodeName;

        static HashSet<string> skip;

        static FbxImporter()
        {
            // node that preclude the insertion of the whole hierarchy
            skipEmpyrionFbxNodeName = new HashSet<string>(10);
            skipEmpyrionFbxNodeName.Add("LOD0_D1");
            skipEmpyrionFbxNodeName.Add("LOD0_D2");
            skipEmpyrionFbxNodeName.Add("LOD0_D3");
            skipEmpyrionFbxNodeName.Add("MeshShadow");
            skipEmpyrionFbxNodeName.Add("Collider");
            skipEmpyrionFbxNodeName.Add("SymType_1");
            skipEmpyrionFbxNodeName.Add("SymType_2");
            skipEmpyrionFbxNodeName.Add("D1");
            skipEmpyrionFbxNodeName.Add("D2");
            skipEmpyrionFbxNodeName.Add("D3");
        }

        static bool recursiveWalker(FBXNode fbxnode, SceneNode node)
        {
            node.Name = fbxnode.Name;
            //node.LocalTransform = getLocalTransform(fbxnode);
            node.LocalTransform = convert(fbxnode.LocalTransformTrue);

            if (fbxnode.Attribute.Type == AttributeType.Mesh)
            {
                node.Element = convert(fbxnode.Mesh);
            }

            int childcount = fbxnode.GetChildCount();
            if (childcount > 0)
            {
                for (int i = 0; i < childcount; i++)
                {
                    FBXNode fbxchild = fbxnode.GetChild(i);

                    //remove the whole branch
                    if (skip != null && skip.Contains(fbxchild.Name)) continue;

                    SceneNode child = new SceneNode(node.Tree, fbxnode.Name);
                    if (!recursiveWalker(fbxchild, child)) return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Convert the fbx file into my similar node's scene structure. 
        /// </summary>
        /// <param name="filename">the fbx file</param>
        /// <param name="skipNodeName">Like for "asset-importer" library i apply a filter to avoid importing not necessary geometry.</param>
        /// <remarks>
        /// The fbx files can contain a lot of data, unfortunately this version only loads the mesh list and
        /// may be incompatible or return an incorrect mesh. I build a c# wrapper using the fbx library: FBX SDK 2020.2
        /// </remarks>
        public static SceneTree Import(string filename, HashSet<string> skipNodeName = null)
        {
            if (!IsValidated) return null;

            if (!File.Exists(filename))
                throw new FileNotFoundException("the fbx file not exist " + filename);

            if (fbxManager == null && !Initialize())
            {
                return null;
            }

            SceneTree scene = null;
            
            skip = skipNodeName;

            try
            {
                FBXScene fbxscene = FBXScene.Import(fbxManager, filename, -1);
                Version version = fbxscene.FileVersion;

                scene = new SceneTree(Path.GetFileName(filename));

                //bypass root node is always a identity matrix
                FBXNode fbxRoot = fbxscene.RootNode.GetChild(0);

                //generally the name corresponds to the model
                scene.Name = fbxRoot.Name;

                if (!recursiveWalker(fbxRoot, scene.Root))
                {
                    Debugg.Print("FbxImporter : FAIL TO READ HIERARCHY", DebugInfo.Error);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debugg.Print("FbxImporter : " + e.Message, DebugInfo.Error);
                scene = null;
            }
            skip = null;
            return scene;
        }

        /// <summary>
        /// Before using the importer is necessary to check if external library are loaded
        /// </summary>
        /// <returns></returns>
        public static bool Validate()
        {
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string dllfilename = Path.Combine(Path.GetDirectoryName(strExeFilePath), "libfbxsdk.dll");

            if (!File.Exists(dllfilename))
            {
                MessageBox.Show("FbxImporter can't work if there isn't the \"libfbxsdk.dll\" in the same folder of executable", "Missing FBX library", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            IsValidated = true;
            return IsValidated;
        }

        public static bool Initialize()
        {
            if (!IsValidated) return false;

            Dispose();
            try
            {
                fbxManager = new FBXManager();
            }
            catch (Exception e)
            {
                Debugg.Print("FbxImporter : " + e.Message, DebugInfo.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Remove any reference from wrapper
        /// </summary>
        public static void Dispose()
        {
            if (!IsValidated) return;

            if (fbxManager != null) fbxManager.Dispose();
            fbxManager = null;
        }

        #region Conversion
        static Matrix4x4f getLocalTransform(FBXNode node)
        {
            Matrix4x4f scale = Matrix4x4f.Scaling((float)node.Scale.X, (float)node.Scale.Y, (float)node.Scale.Z);
            Matrix4x4f rotate = Matrix4x4f.RotationYawPitchRoll(
                Mathelp.DegreeToRadian((float)node.Rotation.X),
                Mathelp.DegreeToRadian((float)node.Rotation.Y),
                Mathelp.DegreeToRadian((float)node.Rotation.Z));

            Matrix4x4f traslate = Matrix4x4f.Translating((float)node.Position.X, (float)node.Position.Y, (float)node.Position.Z);

            Matrix4x4f trs = scale * rotate * traslate;

            return trs;
        }

        static Vector3f convert(FBXVector3 vector){return new Vector3f(vector.X, vector.Y, vector.Z);}

        static Vector2f convert(FBXVector2 vector) { return new Vector2f(vector.X, vector.Y); }

        static Matrix4x4f convert(FBXMatrix4 m)
        {
            Matrix4x4f mat = new Matrix4x4f(
                m.m00, m.m01, m.m02, m.m03,
                m.m10, m.m11, m.m12, m.m13,
                m.m20, m.m21, m.m22, m.m23,
                m.m30, m.m31, m.m32, m.m33);

            mat.Traspose();
            return mat;
        }

        static string ToString(FBXMatrix4 m)
        {
            StringBuilder str = new StringBuilder();
            str.Append(string.Format("{0:0.000} {1:0.000} {2:0.000} {3:0.000}\n", m.m00, m.m01, m.m02, m.m03));
            str.Append(string.Format("{0:0.000} {1:0.000} {2:0.000} {3:0.000}\n", m.m10, m.m11, m.m12, m.m13));
            str.Append(string.Format("{0:0.000} {1:0.000} {2:0.000} {3:0.000}\n", m.m20, m.m21, m.m22, m.m23));
            str.Append(string.Format("{0:0.000} {1:0.000} {2:0.000} {3:0.000}", m.m30, m.m31, m.m32, m.m33));
            return str.ToString();
        }

        static Mesh convert(FBXMesh fbxmesh)
        {

            if (!fbxmesh.Triangulated)
            {
                Debugg.Warning("> not supported function : fbx report it's a not triangulated mesh");
                return null;
            }

            Mesh mesh = new Mesh();

            int vcount = fbxmesh.ControlPointsCount;
            mesh.Vertices = new StructBuffer<Vector3f>(vcount);

            for (int i = 0; i < vcount; i++)
            {
                mesh.Vertices.Add(convert(fbxmesh.GetControlPointAt(i)));
            }

            Polygon[] polygons = fbxmesh.GetPolygons();
            if (polygons == null || polygons.Length == 0) return null;

            SubMesh submesh = mesh.AddSubMesh(polygons.Length);

            foreach (var poly in polygons)
                submesh.AddPrimitive(poly.Indices[0], poly.Indices[1], poly.Indices[2]);


            return mesh;
        }

        #endregion
    }
}
