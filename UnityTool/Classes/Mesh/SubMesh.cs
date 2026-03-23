using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using Common.Maths;

namespace UnityTool
{
    public partial class Mesh
    {
        public enum GfxPrimitiveType : int
        {
            kPrimitiveTriangles = 0,
            kPrimitiveTriangleStrip = 1,
            kPrimitiveQuads = 2,
            kPrimitiveLines = 3,
            kPrimitiveLineStrip = 4,
            kPrimitivePoints = 5,
        };

        public class SubMesh
        {
            public uint firstByte;
            public uint indexCount;
            public GfxPrimitiveType topology;
            public uint triangleCount;
            public uint baseVertex;
            public uint firstVertex;
            public uint vertexCount;
            public BoundingBoxCenter localAABB;


            bool Read(EndianBinaryReader reader, BuildVersion version)
            {
                firstByte = reader.ReadUInt32();
                indexCount = reader.ReadUInt32();
                topology = (GfxPrimitiveType)reader.ReadInt32();

                if (!Enum.IsDefined(typeof(GfxPrimitiveType), topology))
                {
                    Debugg.Error($"submesh topology wrong {topology}");
                    throw new Exception();
                }

                if (version.IsEarlier(4)) //4.0 down
                    triangleCount = reader.ReadUInt32();

                if (version.IsGreaterEqual(2017, 3))//2017.3 and up
                    baseVertex = reader.ReadUInt32();

                if (version.IsGreaterEqual(3))//3.0 and up
                {
                    firstVertex = reader.ReadUInt32();
                    vertexCount = reader.ReadUInt32();
                    localAABB = new BoundingBoxCenter(reader);
                }
                return true;
            }

            public static SubMesh Get(EndianBinaryReader reader, BuildVersion version)
            {
                var sub = new SubMesh();
                if (!sub.Read(reader, version)) return null;
                return sub;
            }
        }
    }
}
