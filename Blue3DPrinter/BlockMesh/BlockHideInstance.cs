using System;
using Common.Tools;

namespace Blue3DPrinter
{
    /// <summary>
    ///  Represents a triangles flag of visibility. It's generate for each BlockObject
    /// </summary>
    public class BlockHideInstance
    {
        public BlockObject block;

        /// <summary>
        /// true if face is hide
        /// </summary>
        public BitArray1 triangleMap;
        /// <summary>
        /// True if vertex is inside the triangle of adjacent block
        /// </summary>
        public BitArray1 verticesMap;

        private BlockHideInstance(BlockObject block, int triangles, int vertices)
        {
            this.block = block;
            triangleMap = new BitArray1(triangles, true);
            verticesMap = new BitArray1(vertices, true);
        }

        public static BlockHideInstance Create(BlockObject block)
        {
            if (block.Model == null || block.Model.Mesh == null) return null;

            int numTriangles = block.Model.Mesh.IndicesCount / 3;
            int numVertices = block.Model.Mesh.VerticesCount;


            if (numTriangles <= 0 || numVertices <= 0)
            {
                throw new Exception("BlockHideInstance can't be created");
            }

            return new BlockHideInstance(block, numTriangles, numVertices);
        }


        public override string ToString()
        {
            return string.Format("{0}_Triangles:{1}", GetHashCode(), triangleMap.Count);
        }
    }


}
