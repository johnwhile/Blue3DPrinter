using System;
using System.Diagnostics;
using System.Collections.Generic;


using Common.Maths;
using Common;
using Common.Tools;


namespace Blue3DPrinter
{

    /// <summary>
    /// Contain the block geometry. It's shared by more <see cref="BlockObject_old"/> and is loaded only once.
    /// Consider it an instance so all blocks with same geometry have only one model instance.
    /// </summary>
    public class BlockModel
    {
        /*
        /// <summary>
        /// Only for debugging AssetTool
        /// </summary>
        public Matrix4x4f FixTheMatrixMath(AssetStudio.AssetTransform original)
        {
            var s = Matrix4x4f.Scaling(original.scale);
            var r = Matrix4x4f.Rotating(original.rotation.x, -original.rotation.y, -original.rotation.z, original.rotation.w);
            var t = Matrix4x4f.Translating(-original.traslation.x, original.traslation.y, original.traslation.z);
            return t * r * s; //for row-majour vector ???

            //Matrix4 m = scale * rotate;
            //m.Translate(-original.traslation.x, original.traslation.y, original.traslation.z);
            //return m;
        }
        */

        /// <summary>
        /// <seealso cref="BundleUtilities.ConvertAssetToScene"/>
        /// </summary>
        static HashSet<string> skip;
        static HashSet<string> filter;

        static BlockModel()
        {
            ////// this filtering is already done partially when the file storage was generated to reduce its size

            // node that preclude the insertion of the whole hierarchy
            skip = new HashSet<string>
            {
                "LOD0_D1", //lod0 - first damage 
                "LOD0_D2", //lod0 - second damage 
                "LOD0_D3", //lod0 - third damage 
                "MeshShadow", //mesh to generate shadow in game
                "Collider", // for collision calculations
                "Collider1",
                "Connector", //landing gear dummy
                "SymType_1",
                "SymType_2",
                "D1", //lod0 - first damage 
                "D2",
                "D3"
            };

            // node that contain the necessary geometry in custom structure
            filter = new HashSet<string>
            {
                "LOD0", //contain cardinal's mesh, require a name parser
                "Mesh" //contain mesh or node "D0" "D1" "D2" "D3", take only D0 and possibly all the children if exist
            };
        }

        BlockDescription description;

        public string Name { get; private set; }
        public bool IsLoaded { get; private set; }
        public int TotalVertices { get; private set; }
        public int TotalTriangles { get; private set; }

        /// <summary>
        /// 3D geometry data in Directx Left Handle coordinates (the x is inverted from asset data)
        /// </summary>
        public BlockMesh Mesh;

        public bool HasCardinals { get; private set; }

        /// <summary>
        /// Generate a unique block model and elaborate the data found in scene.
        /// A filter method try to find only necessary geometries, read ModelStructuresVariants.pptx for info 
        /// </summary>
        /// <param name="scene">contain all the data from game asset (but I actually removed the textures and other data to simplify)</param>
        //public BlockModel(SceneFile scene, BlockDescription description, string nameofmodel, bool unsort = false)
        public BlockModel(SceneTree scene, BlockDescription description, string nameofmodel, bool sort = true, bool collapse = false)
        {
            this.description = description;
            IsLoaded = false;
            HasCardinals = false;
            
            SceneNode root = scene.Root;
            if (root.Name != nameofmodel) Debug.WriteLine("# model name not match with root ?");

            string meshname = description != null ? description.BlockId.ToString() + "_" + nameofmodel : nameofmodel;

            Mesh = new BlockMesh(meshname);

            foreach (var firstnode in root.Children)
            {
                switch(firstnode.Name.ToLower())
                {
                    case "mesh":
                        ParseAndAddMesh(firstnode, sort, collapse);

                        foreach (var node in firstnode.Children)
                        {
                            string nodename = node.Name.ToLower();

                            //attention, need to skip all damage version D1 D2 D3 etc... 
                            if (nodename.Length==2 && nodename[0] == 'd')
                            {
                                int d = Utils.CharToInt(nodename[1]);
                                if (d > 0 && d < 10) continue;
                            }
                            //All nodes in the its hierarchy are eligible
                            ParseAndAddMesh(node, sort, collapse); //remember that TreeHierarchy not return root
                            foreach (var childnode in node.TreeHierarchy) ParseAndAddMesh(childnode, sort, collapse);
                        }

                        break;

                    case "lod0":
                        ParseAndAddMesh(firstnode, sort, collapse);//remember that TreeHierarchy not return root
                        foreach (var childnode in firstnode.TreeHierarchy) ParseAndAddMesh(childnode, sort, collapse);
                        break;

                    case "lod1":
                    case "lod2":
                    case "lod3":
                        break;
                }
            }
            Name = scene.Name;

            //if (!IsLoaded) Debug.WriteLine(">>> model "+ nameofmodel + " empty meshes ?");

            IsLoaded = Mesh.VerticesCount > 0;
        }

        private void ParseAndAddMesh(SceneNode node, bool sort, bool collapse)
        {
            if (node.Element is TriMesh geometry)
            {
                /*
                //only for debugging purpose: if AssetTransform static method is called, use this method
                if (node.Tag is AssetStudio.AssetTransform original)
                    node.LocalTransform = FixTheMatrixMath(original);
                */

                geometry.Transform = description != null ?
                    description.GetCustomFix * geometry.Transform * node.GetGlobalTransform() :
                    geometry.Transform = geometry.Transform * node.GetGlobalTransform();

                TotalVertices += geometry.VerticesCount;
                TotalTriangles += geometry.IndicesCount / 3;

                //Debug.WriteLine("global transform for node " + node.Name);
                //Debug.WriteLine(info.WorldMatrix.ToString());


                //parse the direction if found.
                // example "S_1" or "S" or "S_T" is valid, "Segment" is not valid
                Cardinal Direction = Cardinal.Undefined;
                Hide Removable = Hide.None;

                if (node.Name.Length == 1 || (node.Name.Length >= 2 && node.Name[1] == '_'))
                {
                    switch (char.ToLower(node.Name[0]))
                    {
                        case 't': Direction = Cardinal.Top; break;
                        case 'b': Direction = Cardinal.Bottom; break;
                        case 'n': Direction = Cardinal.North; break;
                        case 'w': Direction = Cardinal.West; break;
                        case 's': Direction = Cardinal.South; break;
                        case 'e': Direction = Cardinal.Est; break;
                    }
                    //parse hide value if found
                    if (node.Name.Length >= 3)
                    {
                        switch (char.ToLower(node.Name[2]))
                        {
                            case 'i':
                            case 'p':
                                Removable = Hide.Partial;
                                //bluemesh.RemovableMask = GetRasterizedSurfaces(geometry, bluemesh.Direction);
                                break;
                            case 'f':
                                Removable = Hide.Total;
                                //bluemesh.RemovableMask = RasterizedPoly8x8.Full(bluemesh.Direction);
                                break;
                        }
                    }
                }

                Mesh.AddBlockMesh(geometry, Direction, Removable, sort, collapse);
            }
        }

    }
}
