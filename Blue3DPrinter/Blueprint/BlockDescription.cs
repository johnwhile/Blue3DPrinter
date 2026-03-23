using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using Common;
using Common.Maths;

namespace Blue3DPrinter
{
    /// <summary>
    /// Extracted data from "BlockConfig.ecf"
    /// </summary>
    public class BlockDescription
    {
        static Dictionary<int, Vector3f> OffsetFixMap = new Dictionary<int, Vector3f>();

        //the index of my blockdescription list
        internal int index;

        #region BlocksConfig.ecf's keys
        /// <summary>
        /// Parse "+Block Id" and "Block Id", make attention for block id over 2048, use block name instead
        /// </summary>
        public int BlockId = -1;
        /// <summary>
        /// Parse "Name" Key
        /// </summary>
        public string Name = null;

        /// <summary>
        /// A block device size
        /// </summary>
        public Vector3b SizeInBlocks = new Vector3b(1, 1, 1);

        /// <summary>
        /// <see cref="Model"/>
        /// </summary>
        //public string AssetFileName;
        /// <summary>
        /// Parse "Model" key, i stores only the filename and not the full path. If it's "cube" version, use childshape instead.
        /// For parent-referenced-model, it's null untill complete parsing of BlocksConfigs.ecf and resolved parent references
        /// </summary>
        /// <remarks>
        /// "Model: Truss/TrussWall" => the Path.GetFileName() return "TrussWall" but it's correct ?
        /// </remarks>
        public string Model;
        /// <summary>
        /// define is the model variant require a -0.5 y fix
        /// </summary>
        public string Category;
        /// <summary>
        /// NOT IMPLEMENTED. The "Mesh-Damage-1" key, usefull to get only first mesh
        /// </summary>
        public string Damage0;
        /// <summary>
        /// NOT IMPLEMENTED. Parse "Shape". = "new" for cube blocks
        /// </summary>
        public string Shape;
        /// <summary>
        /// Parse "ChildShapes" or "ChildBlocks" array of string, mantain Low and Upper case
        /// </summary>
        public string[] ChildName;
        #endregion

        /// <summary>
        /// The <see cref="Parent"/> can't be set untill config's file is fully read.
        /// </summary>
        internal string tmp_unresolvedparent;
        internal bool tmp_iscubeshape = true;
        /// <summary>
        /// Some blocks inherit parameters from over blocks
        /// </summary>
        public BlockDescription Parent { get; internal set; }

        public int ChildCount => ChildName == null ? 0 : ChildName.Length;
        
        public BlockDescription()
        {
            Name = "";
            BlockId = -1;
            ChildName = null;
            Parent = null;
        }
        /// <summary>
        /// Debug purpose
        /// </summary>
        public void WriteToFile(StreamWriter file)
        {
            file.WriteLine("{");
            file.WriteLine("  BlockId : " + BlockId);
            file.WriteLine("  Name    : " + Name);
            if (!string.IsNullOrEmpty(Model)) file.WriteLine("  Model   : " + Path.GetFileNameWithoutExtension(Model));
            if (!string.IsNullOrEmpty(Damage0)) file.WriteLine("  D0      : " + Damage0);
            if (!string.IsNullOrEmpty(Shape)) file.WriteLine("  Shape   : " + Shape);
            if (ChildCount > 0)
            {
                file.Write(" ChildShapes : ");
                file.Write("\"" + ChildName[0]);
                for (int i = 1; i < ChildCount; i++)
                    file.Write(", " + ChildName[i]);
                file.WriteLine("\"");
            }
            file.WriteLine("}");
            file.WriteLine("");
        }
        /// <summary>
        /// Return null if not found. ChildShape contain the file name of asset. Use <see cref="GetFilenameAsset(int)"/> instead.
        /// </summary>
        public string GetChildShape(int childindex)
        {
            if (childindex < ChildCount) return ChildName[childindex];
            return null;
        }

        /// <summary>
        /// Return the correct filename of model file. The filename is stored as childshape items or as Model key parameters.
        /// If not found in this description it try to find in the parent description
        /// </summary>
        /// <param name="childIndex">for Model key = "cube" the filename is stored in ChildShapes key, use -1 to force use Model key but it's not correct</param>
        /// <returns>return null if not found</returns>
        public string GetFilenameAsset(int childIndex = 0)
        {
            string assetName = null;

            if (childIndex >= 0 && ChildCount > 0)
            {
                if (ChildCount > 0 && childIndex < ChildCount)
                {
                    assetName = ChildName[childIndex];
                }
                if (assetName != null) return assetName;
            }

            if (assetName == null)
            {
                if (Model != null) return Path.GetFileNameWithoutExtension(Model);

                if (Model == null && Parent != null)
                {
                    // force to use Model key instead use childindex
                    return Parent.GetFilenameAsset(-1);
                }
                // filename can't be not found
                LogMsg.Error("wrong data for blockdescription : " + Name + ", model key is inconsistent, or wrong parsed, or parents are not resolved");
            }
            return null;
        }

        /// <summary>
        /// Model key don't match with filename for cube version, to get the correct filename use <see cref="GetFilenameAsset(int)"/>.
        /// The correct reading of this value is still very confusing...
        /// </summary>
        /// <remarks>The cube model is processed in a different way during the construction of the 3d object.</remarks>
        public bool IsCubeShape
        {
            get
            {
                if (Model != null && string.Compare(Model, 0, "@models", 0, 7) == 0) return false;

                //parent has priority, get recursively untill root
                if (Parent!=null) return Parent.IsCubeShape;

                //seam wrong this code
                if (Model != null) if (Model.ToLower() == "cube") return true ;
                if (Category != null) return Category.ToLower() == "buildingblocks";

                LogMsg.Error("wrong data for blockdescription : " + Name + ", i can't resolve IsCubeModel info, set to false for convenience");
                return false;
            }
        }


        /// <summary>
        /// CubeShape have +0.5 y offset, and some 3d model extracted from asset report a different pivot point...
        /// Must be apply after get mesh process and before traslating in block's coordinate as a transform matrix
        /// </summary>
        public Matrix4x4f GetCustomFix
        {
            get
            {
                Vector3f offsetfix = Vector3f.Zero;
                
                if (IsCubeShape)
                    offsetfix.y = -.5f;
                else if (!OffsetFixMap.TryGetValue(BlockId, out offsetfix)) 
                    offsetfix = Vector3f.Zero;
                
                /*
                if (IsCubeShape) offsetfix.y = -0.5f;
                if (BlockId == 1695) offsetfix.x = -1.7f;
                if (BlockId == 1510) offsetfix.x = -5.266f;
                if (BlockId == 1498) offsetfix.z = -4.1f;
                if (BlockId == 1495) offsetfix.x = -1.213f;
                */
                return Matrix4x4f.Translating(offsetfix);
            }
        }

        static BlockDescription()
        {
            if (!File.Exists("ManualFix.xml"))
            {
                LogMsg.Error("Not found the optional \"ManualFix.xml\" file. This file needed to manually fix some block's positions because it doesn't seem to match the game");
                return;
            }
            try
            {
                OffsetFixMap.Clear();

                using (var reader = XmlReader.Create("ManualFix.xml"))
                {
                    while(reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "BlockId":
                                Vector3f offset = Vector3f.Zero;
                                int blockid = 0;
                                if (reader.MoveToAttribute("ID")) if (!Mathelp.TryParse(reader.Value, out blockid)) blockid = 0;
                                if (reader.MoveToAttribute("OffsetX")) if (!Mathelp.TryParse(reader.Value, out offset.x)) offset.x = 0;
                                if (reader.MoveToAttribute("OffsetY")) if (!Mathelp.TryParse(reader.Value, out offset.y)) offset.y = 0;
                                if (reader.MoveToAttribute("OffsetZ")) if (!Mathelp.TryParse(reader.Value, out offset.z)) offset.z = 0;
                                if (blockid>0 && !OffsetFixMap.ContainsKey(blockid))
                                    OffsetFixMap.Add(blockid, offset);
                                break;
                        }
                    }
                    LogMsg.Message("> \"ManualFix.xml\" ok.");
                }
            }
            catch
            {
                LogMsg.Error("Error readding \"ManualFix.xml\" file.");
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ID:{1} {2} {3} childs:{4}", Name, BlockId, Path.GetFileNameWithoutExtension(Model), Shape, ChildName!=null ? ChildName.Length : 0);
        }
    }
}
