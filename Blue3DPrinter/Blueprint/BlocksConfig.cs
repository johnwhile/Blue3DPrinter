using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using Common;
using Common.Maths;

namespace Blue3DPrinter
{
    /// <summary>
    /// Parse the BlocksConfig.ecf. Attention, no check about presence of more blocks with same blockID or same blockName,
    /// the file must be correctly formatted 
    /// </summary>
    [DebuggerDisplay("Totals Blocks = {m_DescriptionList.Count}")]
    public class BlocksConfig
    {
        /// <summary>
        /// the BlockConfig.ecf big list
        /// </summary>
        List<BlockDescription> m_DescriptionList;
        /// <summary>
        /// find the <see cref="m_DescriptionList"/> index using the block's id
        /// </summary>
        Dictionary<int, int> m_indexByBlockId;
        /// <summary>
        /// find the <see cref="m_DescriptionList"/> index using the block's name (not child name)
        /// </summary>
        Dictionary<string, int> m_indexByName;
        /// <summary>
        /// childshapes or childblocks have same blockid
        /// </summary>
        Dictionary<string, int> m_indexByChildname;

        /// <summary>
        /// Usefull to find the asset name of model
        /// <para>at first try it searches the block name. If description!=null : the returned value of childindex is -1</para>
        /// <para>at second try it searches childshape name. If description!=null : the returned value of childindex is the index found</para>
        /// </summary>
        /// <remarks>
        /// for the 2° try, this function returns the first description's relation found because the same child model can be used
        /// for different block's description. Example for the "Cube" version, the same child model is linked to different Hull Type descriptor
        /// </remarks>
        /// <returns>return false if not found any reference</returns>
        public bool TryGetFromName(string name, out BlockDescription description, out int childIndex)
        {
            string lowername = name.ToLower(); //if prefer not use case sensitive
            description = null;
            int idx;

            if (!m_indexByName.TryGetValue(lowername, out idx))
            {
                if (!m_indexByChildname.TryGetValue(lowername, out idx))
                {
                    childIndex = -1;
                    return false;
                }
                else
                {
                    //found as a child shape
                    childIndex = Array.IndexOf(m_DescriptionList[idx].ChildName, name);
                    if (childIndex < 0) return false;
                }
            }
            else
            {
                //found as block's name
                childIndex = -1;

            }
            description = m_DescriptionList[idx];
            return true;
        }
        /// <summary>
        /// Block description have its unique block id
        /// </summary>
        /// <returns>return null if not found</returns>
        public BlockDescription GetDescription(int blockid)
        {
            int index;
            if (m_indexByBlockId.TryGetValue(blockid, out index))
                return m_DescriptionList[index];
            return null;
        }
        /// <summary>
        /// I assume that each block has its own unique name
        /// </summary>
        /// <returns>return null if not found</returns>
        public BlockDescription GetDescription(string blockname)
        {
            int index;
            if (m_indexByName.TryGetValue(blockname.ToLower(), out index))
                return m_DescriptionList[index];
            return null;
        }

        /// <summary>
        /// return null if not correctly load
        /// </summary>
        public static BlocksConfig LoadConfig(string filename = "BlocksConfig.ecf")
        {
            LogMsg.Message("> Loading block config...", ConsoleColor.Yellow);
            BlocksConfig configfile = new BlocksConfig();
            if (!configfile.ReadFile(filename))
            {
                LogMsg.Error("> FAIL to load blockconfig.ecf");
                return null;
            }
            LogMsg.Message("> BlockConfig OK");
            return configfile;
        }

        bool ReadFile(string filename)
        {
            if (!File.Exists(filename)) return false;

            EcfParser parser = new EcfParser();
            
            m_DescriptionList = new List<BlockDescription>();
            m_indexByBlockId = new Dictionary<int, int>();
            m_indexByName = new Dictionary<string, int>();

            using (StreamReader file = File.OpenText(filename))
            {
                EcfParser.PropPair pair;
                EcfParseResult result;
                BlockDescription pendingDescr = null;
                int idx = 0;

                int blockIdOver2048 = 0;
                try
                {

                    do
                    {
                        result = parser.GetNextPropertyPair(file, out pair);

                        switch (result)
                        {
                            case EcfParseResult.EOF:
                                break;

                            case EcfParseResult.BEGINLEVEL:
                                if (parser.CurrentLevel == 1)
                                {
                                    pendingDescr = new BlockDescription();
                                }
                                else
                                {
                                    // all group property that start with a new level are not implemented yet
                                }
                                break;

                            case EcfParseResult.ENDLEVEL:
                                if (parser.CurrentLevel == 0)
                                {
                                    if (pendingDescr != null)
                                    {
                                        if (pendingDescr.BlockId < 0)
                                        {
                                            // reforged eden mod use also block id over 2048 but use the block name in the blueprint's blockmappingid,
                                            // instead this recalculated blockId
                                            pendingDescr.BlockId = 2048 + blockIdOver2048;
                                            blockIdOver2048++;
                                        }

                                        //generate error if find same blockId or same name
                                        m_indexByBlockId.Add(pendingDescr.BlockId, m_DescriptionList.Count);

                                        if (!string.IsNullOrEmpty(pendingDescr.Name))
                                            m_indexByName.Add(pendingDescr.Name.ToLower(), m_DescriptionList.Count);

                                        pendingDescr.index = idx++;
                                        m_DescriptionList.Add(pendingDescr);

                                        pendingDescr = null;
                                    }
                                }
                                else { }// all group property that start with a new level are not implemented yet
                                break;

                            case EcfParseResult.SUCCESS:
                                //Console.WriteLine(parser.CurrentLevel + "> " + pair.ToString());

                                string matchkey = pair.PropKey.ToLower();

                                switch (matchkey)
                                {
                                    case "block id":
                                    case "+block id":
                                        int blockid = -1;
                                        if (pair.PropValue is string && int.TryParse((string)pair.PropValue, out blockid))
                                            pendingDescr.BlockId = blockid;
                                        else
                                            pendingDescr.BlockId = -1;
                                        break;

                                    case "sizeinblocks":
                                        if (pair.PropValue.GetType().IsArray)
                                        {
                                            var vector = pair.PropValue as string[];
                                            if (vector.Length >= 3)
                                            {
                                                int.TryParse(vector[0], out int x);
                                                int.TryParse(vector[1], out int y);
                                                int.TryParse(vector[2], out int z);
                                                pendingDescr.SizeInBlocks = new Vector3b((byte)x, (byte)y, (byte)z);
                                            }
                                        }
                                        break;

                                    case "block name": //block name if for new Non-ID block definition, where blockId is a number up 2048
                                    case "name":
                                        if (pair.PropValue is string)
                                            pendingDescr.Name = pair.PropValue as string;
                                        break;

                                    case "shape":
                                        if (pair.PropValue is string)
                                            pendingDescr.Shape = pair.PropValue as string;
                                        break;
                                    case "model":
                                        if (pair.PropValue is string)
                                            pendingDescr.Model = pair.PropValue as string;
                                        break;

                                    case "mesh - damage - 1":
                                        if (pair.PropValue is string)
                                            pendingDescr.Damage0 = pair.PropValue as string;
                                        break;

                                    case "childshapes":
                                    case "childblocks":
                                        if (pair.PropValue.GetType().IsArray)
                                            pendingDescr.ChildName = pair.PropValue as string[];
                                        break;

                                    case "ref": // parents can be resolved only after complete construction of list
                                        if (pair.PropValue is string)
                                            pendingDescr.tmp_unresolvedparent = pair.PropValue as string;
                                        break;

                                    case "category":
                                        if (pair.PropValue is string)
                                            pendingDescr.Category = pair.PropValue as string;
                                        break;
                                }
                                break;

                            default:
                                LogMsg.Error("> ERROR parsing blockconfig.ecf at row " + parser.CurrentLine);
                                return false;
                        }
                    }
                    while (result >= 0);

                }

                catch(Exception e)
                {
                    LogMsg.Error("> ERROR parsing blockconfig.ecf at row " + parser.CurrentLine);
                    LogMsg.Message(e.Message.ToString());
                    return false;
                }
                //resolve all parent's references
                foreach (var descr in m_DescriptionList)
                {
                    if (descr.tmp_unresolvedparent!=null)
                    {
                        var parent = GetDescription(descr.tmp_unresolvedparent);
                        if (parent!=null)
                        {
                            descr.Parent = parent;
                            descr.tmp_unresolvedparent = null;
                        }
                        descr.tmp_unresolvedparent = null;
                    }
                }
                //resolve is-child-shape
                foreach (var descr in m_DescriptionList)
                {
                    //if (string.Compare(descr.Model, 0, "@models", 0, 7) == 0)
                    //    descr.tmp_iscubeshape = false;


                }

                



                //build useful childname to blockid table
                m_indexByChildname = new Dictionary<string, int>();
                foreach (var descr in m_DescriptionList)
                {
                    if (descr.ChildCount>0)
                    {
                        foreach(string name in descr.ChildName)
                        {
                            if (!m_indexByChildname.ContainsKey(name))
                                m_indexByChildname.Add(name, descr.index);
                        }
                    }
                }
                return true;
            }
            
        }


        /// <summary>
        /// </summary>
        public void WriteDebugLog(string filename)
        {
            Console.WriteLine("> Writing " + filename);

            using (StreamWriter writer = new StreamWriter(Path.GetFileNameWithoutExtension(filename) + ".txt"))
            {
                foreach (var descr in m_DescriptionList)
                {
                    descr.WriteToFile(writer);
                }
                Console.WriteLine("> end ");
            }
        }

        public void BuildMapList(TreeNode root)
        {
            foreach (var decr in m_DescriptionList)
            {
                TreeNode block = new TreeNode(decr.BlockId.ToString() + " " + decr.Name);

                if (decr.ChildName != null)
                {
                    foreach (var child in decr.ChildName)
                        block.Nodes.Add(child);
                }
                else
                    block.Text += " !";
                root.Nodes.Add(block);
            }
        }
    }
}
