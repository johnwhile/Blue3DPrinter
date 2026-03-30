using System;

using Common;
using Common.Maths;
using Common.IO.Wavefront;
using System.IO;
using System.Collections.Generic;

namespace Blue3DPrinter
{
    public enum HideLevel
    {
        /// <summary>
        /// Display all meshes. I suggest this only for debugging because the number of vertices for big ships is very high
        /// </summary>
        None = 0,
        /// <summary>
        /// Like game hide method, it use only the informations parsed from meshes's name
        /// </summary>
        Simple = 1,
        /// <summary>
        /// The definitive solution
        /// </summary>
        Complete = 2,
    }
    /// <summary>
    /// The face to hide
    /// </summary>
    [Flags]
    public enum HideResult : byte
    {
        Both = 3,
        Right = 1,
        Left = 2,
        Undefined = 4,
        None = 0,
    }

    public static class SceneGenerator
    {
        /// <summary>
        /// comparison is done using only mesh's name parsing 
        /// </summary>
        static HideResult CompareFace(BlueprintMesh left, BlueprintMesh right)
        {
            if (left.Removable == Hide.Total)
            {
                if (right.Removable == Hide.Total) return HideResult.Both;
                else if (right.Removable == Hide.Partial) return HideResult.Right;
            }
            else if (left.Removable == Hide.Partial)
            {
                if (right.Removable == Hide.Total) return HideResult.Left;

                else if (right.Removable == Hide.Partial)
                {
                    //both partial but i need a function to calculate who hides who
                    return HideResult.Undefined;
                }
            }
            return HideResult.None;
        }



        static BlockObject GetBlockAt(Blueprint blueprint, int x, int y, int z)
        {
            BlockObject block = blueprint.Blocks[x, y, z];
            if (block == null || !block.Enable || block.Model == null || !block.Model.IsLoaded) return null;
            return block;
        }


        public static bool GenerateWaveFront(Blueprint blueprint, string exportfilename, HideLevel hidelevel = HideLevel.None, bool mergeblock = true, int maxVerticesAllowed = 0, bool exportcolor = true)
        {
            int currentBlock = 1;
            int omitted = 0;
            int MaxVertices = 0;
            int MaxTriangles = 0;
            int TotVertices = 0;
            int TotTriangles = 0;

            WavefrontMat material = null;
            // collect only colors used by blocks
            if (exportcolor && blueprint.MetaData.colorPalette != null)
            {
                LogMsg.Message("> creating color palette as wavefront mtl", ConsoleColor.Yellow);
                material = new WavefrontMat();
                int i = 0;
                foreach (var color in blueprint.MetaData.colorPalette)
                {
                    var mat = material.Create("ColorPalette" + i++, color); //name must be unique
                    mat.IsUsed = false; //enable only if used
                }

                material.Save(exportfilename);
            }

            LogMsg.Message("> begin building wavefront obj fileobj", ConsoleColor.Yellow);
            int cursorRow = Console.CursorTop;


            // this happen when console buffer is too small
            if (cursorRow > Console.BufferHeight - 10)
            {
                cursorRow = 0;
            }

            WavefrontObj fileobj = new WavefrontObj();
            Wavefront.NumberDecimalDigits = 4;

            Vector3i size = blueprint.Blocks.Size;

            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    for (int z = 0; z < size.z; z++)
                    {
                        BlockObject block = GetBlockAt(blueprint, x, y, z);
                        if (block == null) continue;
                        //write pseudo progress counter
                        Console.CursorTop = cursorRow;
                        Console.CursorLeft = 0;
                        Console.Write(string.Format("blocks processed = {0}/{1}", currentBlock, blueprint.Blocks.Count));
                        currentBlock++;

                        MaxVertices += block.Model.TotalVertices;
                        MaxTriangles += block.Model.TotalTriangles;

                        if (maxVerticesAllowed > 0 && block.Model.TotalVertices > maxVerticesAllowed)
                        {
                            Console.CursorTop = cursorRow + 1;
                            Console.CursorLeft = 0;
                            Console.Write("blocks omitted   = " + (++omitted) + " (due to the vertex limit per object you have set)");
                            continue;
                        }

                        if (hidelevel > HideLevel.Simple)
                        {
                            if (block.HideMapInstance == null)
                                block.HideMapInstance = BlockHideInstance.Create(block);
                        }
                        else block.HideMapInstance = null;

                        // can't process the hide method if map is null
                        if (block.HideMapInstance != null)
                        {
                            if (x < size.x - 1)
                            {
                                BlockObject neighbor = GetBlockAt(blueprint, x + 1, y, z);
                                if (neighbor != null)
                                {
                                    if (neighbor.HideMapInstance == null)
                                        neighbor.HideMapInstance = BlockHideInstance.Create(neighbor);
                                    BlockMesh.Compare(block.HideMapInstance, neighbor.HideMapInstance);
                                }
                            }
                            if (y < size.y - 1)
                            {
                                BlockObject neighbor = GetBlockAt(blueprint, x, y + 1, z);
                                if (neighbor != null)
                                {
                                    if (neighbor.HideMapInstance == null)
                                        neighbor.HideMapInstance = BlockHideInstance.Create(neighbor);
                                    BlockMesh.Compare(block.HideMapInstance, neighbor.HideMapInstance);
                                }
                            }
                            if (z < size.z - 1)
                            {
                                BlockObject neighbor = GetBlockAt(blueprint, x, y, z + 1);
                                if (neighbor != null)
                                {
                                    if (neighbor.HideMapInstance == null)
                                        neighbor.HideMapInstance = BlockHideInstance.Create(neighbor);
                                    BlockMesh.Compare(block.HideMapInstance, neighbor.HideMapInstance);
                                }
                            }

                            // flag the triangle but index must match with submesh order.
                            var verticesMap = block.HideMapInstance.verticesMap;
                            var triangleMap = block.HideMapInstance.triangleMap;
                            int t = 0;
                            foreach (var sub in block.Model.Mesh.SubMeshes)
                            {
                                if (sub == null) continue;

                                for (int f = 0; f < sub.IndincesCount/3; f++, t++)
                                {
                                    int i = sub.Indices[f * 3];
                                    int j = sub.Indices[f * 3 + 1];
                                    int k = sub.Indices[f * 3 + 2];


                                    bool trianglevisible = verticesMap[i] | verticesMap[j] | verticesMap[k];
                                    if (!trianglevisible) triangleMap[t] = false;
                                }
                            }
                            // remove vertex mask because ONLY triangle mask are used for hide method, some hide vertex can be 
                            // shared between other visible triangles
                            block.HideMapInstance.verticesMap = null;
                        }
                        
                        WavefrontExporter.WriteToWavefront(fileobj, material, block, mergeblock);
                        
                        //this block will be never revisited, it can be destroyed
                        block.HideMapInstance = null;
                    }


            Console.CursorTop = cursorRow + 1;
            Console.CursorLeft = 0;

            TotVertices = fileobj.GetTotalVerticesCount();
            TotTriangles = fileobj.GetTotalIndicesCount() / 3;
            LogMsg.Message("> garbage collect");
            GC.Collect();

            LogMsg.Message(string.Format("> info : Vertices = {0:n0}, Triangles = {1:n0} (without reduction method)", MaxVertices, MaxTriangles));
            LogMsg.Success(string.Format("> info : Vertices = {0:n0}, Triangles = {1:n0}", TotVertices, TotTriangles));

            if (material != null)
            {
                LogMsg.Message("> writing wavefront mtl file ...");
                material.Save(exportfilename);
                fileobj.MaterialFilename = Path.GetFileName(material.Filename);
            }

            LogMsg.Message("> writing wavefront obj file...");
            fileobj.Save(exportfilename);


            LogMsg.Message("> garbage collect");
            GC.Collect();
            return true;
        }

        public static bool GenerateWaveFront_Debug(Blueprint blueprint, string exportfilename)
        {
            Dictionary<string, BlockModel> UniqueModelsList = new Dictionary<string, BlockModel>();

            foreach (var block in blueprint.Blocks)
            {
                string modelFilename = block.Description.GetFilenameAsset(block.ChildIndex);
                if (modelFilename == null) continue;

                //first time
                if (!UniqueModelsList.TryGetValue(modelFilename, out BlockModel model))
                {
                    if (ModelResourceManager.TryGetModel(modelFilename, out var scene))
                    {
                        model = new BlockModel(scene, block.Description, modelFilename, false, true);
                        UniqueModelsList.Add(modelFilename, model);
                    }
                }
            }
            foreach (var pair in UniqueModelsList)
            {
                WavefrontObj wave = pair.Value.Mesh.ConvertToWavefront();
                wave.Save(@"C:\Users\johnw\Desktop\" + pair.Key);
            }


            return true;
        }


        public static bool GenerateFbxScene(Blueprint blueprint, string exportfilename, HideLevel hideLevel = HideLevel.None)
        {
            return false;
        }

    }
}
