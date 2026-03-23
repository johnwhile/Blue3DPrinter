using System;
using System.Diagnostics;
using Common.IO.Wavefront;
using Common.Maths;
using Common.Tools;

namespace Blue3DPrinter
{
    public static class WavefrontExporter
    {
        static int[] materialMap = new int[7];

        static void write(WavefrontObj file, WavefrontMat material, BlockObject block, BlockMesh mesh, bool mergeblock = true, bool writeGroupName = false)
        {
            write(file, material, block.GetBlockTransform(), mesh, mergeblock, writeGroupName, block.ToStringUnique, block.Position);
        }
        static void write(WavefrontObj file, WavefrontMat material, Matrix4x4f transform, BlockMesh mesh, bool mergeblock = true, bool writeGroupName = false, string blockname = null, Vector3i blockPosition = default(Vector3i))
        {
            int vertexOffset = 0;
            WaveObject waveobj = null;

            if (blockname == null) blockname = mesh.Name;

            vertexOffset = file.GetTotalVerticesCount();

            if (mergeblock)
            {
                //generate one object for all submeshes
                waveobj = file.Create(blockname);
                waveobj.AddVertex(mesh.Vertices);

                for (int i = 0; i < waveobj.VertsCount; i++)
                    waveobj.Vertices[i] = waveobj.Vertices[i].TransformCoordinate(in transform);
            }

            //sort by material because can improve rendering performance
            bool writesomething = false;

            for (int m = 0; m < 6; m++)
            {
                for (int s = 0; s < 7; s++)
                {
                    var subMesh = mesh.GetSubMesh(s, m);
                    if (subMesh == null) continue;

                    writesomething = true;

                    //if name is not unique, the 3ds max imported merge the objects with same name
                    string uniqueName = string.Format("{0}_{1}_Mat{2}_{3}", blockPosition.ToHexString(), subMesh.Name, m, (Cardinal)s);

                    //if not merge blocks, generate one object foreach blocks
                    if (!mergeblock)
                    {
                        waveobj = file.Create(uniqueName);
                        waveobj.CommentName = blockname;

                        var splitted = subMesh.ConvertToMesh();
                        
                        //vertexOffset = waveobj.GetVertexIndexOffset();

                        waveobj.AddVertex(splitted.Vertices);

                        for (int i = 0; i < waveobj.VertsCount; i++)
                            waveobj.Vertices[i] = waveobj.Vertices[i].TransformCoordinate(in transform);

                        

                        subMesh = splitted.SubMeshes[0];
                    }

                    WaveGroup wavegroup = waveobj.Create(WavePrimitive.Triangle, uniqueName);
                    if (!writeGroupName) wavegroup.Name = null;

                    if (material != null)
                    {
                        var mat = material.TryGetByIndex(materialMap[m]);
                        if (mat != null)
                        {
                            wavegroup.Material = mat.Name;
                            mat.IsUsed = true;
                        }
                    }

                    foreach (int i in subMesh.Indices)
                        wavegroup.indexV.Add(i + vertexOffset);
                }
            }

            if (!writesomething)
                Debug.WriteLine("not submesh write ?");
        }



        /// <summary>
        /// Write the block to wavefront
        /// </summary>
        public static void WriteToWavefront(WavefrontObj file, WavefrontMat material, BlockObject block, bool mergeblock)
        {
            for (int i = 0; i < 6; i++) materialMap[i] = block.GetColorIndex(i);

            if (block.Model==null || block.Model.Mesh==null)
            {
                Debug.WriteLine("# the block " + block.ToStringUnique + " doesn't have a mesh");
                return;
            }


            if (block.HideMapInstance != null)
            {
                var newMesh = block.Model.Mesh.RemapMesh(block.HideMapInstance);
                if (newMesh == null)
                {
                    Debug.WriteLine("# the block " + block.ToStringUnique + " return empty mesh");
                    return;
                }

                //write(file, material, block, newMesh, mergeblock);
                write(file, material, block, block.Model.Mesh, mergeblock);
            }
            else
            {
                write(file, material, block, block.Model.Mesh, mergeblock);
            }
        }

        /// <summary>
        /// Create a debug object with material for each direction
        /// </summary>
        public static void WriteToWavefront_Merge(WavefrontObj file, WavefrontMat material, BlockMesh mesh, Matrix4x4f? customTransform = null)
        {
            for (int i = 0; i < 6; i++) materialMap[i] = i;

            var transform = customTransform != null ? (Matrix4x4f)customTransform : Matrix4x4f.Identity;
            write(file, material, transform, mesh, true, true);
        }
        public static void WriteToWavefront_Separate(WavefrontObj file, WavefrontMat material, BlockMesh mesh, Matrix4x4f? customTransform = null, bool colorByDirection = false)
        {
            var transform = customTransform != null ? (Matrix4x4f)customTransform : Matrix4x4f.Identity;
            
            for (int i = 0; i < 7; i++) materialMap[i] = i;

            string materialName = null;

            for (int m = 0; m < 6; m++)
            {
                if (!colorByDirection)
                    materialName = material?.TryGetNameByIndex(materialMap[m]) ?? null;
                

                for (int s = 0; s < 7; s++)
                {
                    if (colorByDirection)
                        materialName = material?.TryGetNameByIndex(materialMap[s]) ?? null;
                    

                    var subMesh = mesh.GetSubMesh(s, m);
                    if (subMesh == null) continue;

                    var singleMesh = subMesh.ConvertToMesh();


                    WaveObject waveobj = file.Create(subMesh.Name);
                    waveobj.AddVertex(singleMesh.Vertices);

                    int vertexOffset = waveobj.GetVertexIndexOffset();

                    for (int i = 0; i < waveobj.VertsCount; i++)
                        waveobj.Vertices[i] = waveobj.Vertices[i].TransformCoordinate(in transform);

                    WaveGroup wavegroup = waveobj.Create(WavePrimitive.Triangle);
                    wavegroup.Name = string.Format("{0}_Material:{1}_Surface:{2}", subMesh.Name, m, ((Cardinal)s).ToString());
                    wavegroup.Material = materialName;

                    subMesh = singleMesh.SubMeshes[0];

                    foreach (int i in subMesh.Indices)
                        wavegroup.indexV.Add(i + vertexOffset);
                }
            }


        }

    }
}
