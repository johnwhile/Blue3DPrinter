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
        public class BlendShapeData
        {
            public BlendShapeVertex[] vertices;
            public MeshBlendShape[] shapes;
            public MeshBlendShapeChannel[] channels;
            public float[] fullWeights;
            

            public bool Read(EndianBinaryReader reader, BuildVersion version)
            {
                if (version.IsGreaterEqual(4,3))//4.3 and up
                {
                    vertices = new BlendShapeVertex[reader.ReadInt32()];
                    for (int i = 0; i < vertices.Length; i++)
                        vertices[i] = BlendShapeVertex.Read(reader);

                    shapes = new MeshBlendShape[reader.ReadInt32()];
                    for (int i = 0; i < shapes.Length; i++)
                        shapes[i] = MeshBlendShape.Read(reader, version);

                    channels = new MeshBlendShapeChannel[reader.ReadInt32()];
                    for (int i = 0; i < channels.Length; i++)
                        channels[i] = MeshBlendShapeChannel.Read(reader);

                    fullWeights = reader.ReadSingleArray(reader.ReadInt32());
                }
                else
                {
                    var m_Shapes = new MeshBlendShape[reader.ReadInt32()];
                    for (int i = 0; i < m_Shapes.Length; i++)
                        m_Shapes[i] = MeshBlendShape.Read(reader, version);

                    reader.AlignStream();
                    var m_ShapeVertices = new BlendShapeVertex[reader.ReadInt32()]; //MeshBlendShapeVertex
                    for (int i = 0; i < m_ShapeVertices.Length; i++)
                        m_ShapeVertices[i] = BlendShapeVertex.Read(reader);

                }
                return true;
            }


            public static BlendShapeData Get(EndianBinaryReader reader, BuildVersion version)
            {
                var data = new BlendShapeData();
                if (!data.Read(reader, version)) return null;
                return data;
            }
        }

        public struct BlendShapeVertex
        {
            public Vector3f vertex;
            public Vector3f normal;
            public Vector3f tangent;
            public uint index;

            public static BlendShapeVertex Read(BinaryReader reader)
            {
                var shape = new BlendShapeVertex
                {
                    vertex = new Vector3f(reader),
                    normal = new Vector3f(reader),
                    tangent = new Vector3f(reader),
                    index = reader.ReadUInt32()
                };
                return shape;
            }
        }

        public struct MeshBlendShape
        {
            public uint firstVertex;
            public uint vertexCount;
            public bool hasNormals;
            public bool hasTangents;

            public static MeshBlendShape Read(EndianBinaryReader reader, BuildVersion version)
            {
                var mesh = new MeshBlendShape();

                if (version.IsEarlier(4,3)) //4.3 down
                {
                    var name = reader.ReadAlignedString();
                }

                mesh.firstVertex = reader.ReadUInt32();
                mesh.vertexCount = reader.ReadUInt32();
                
                if (version.IsEarlier(4, 3)) //4.3 down
                {
                    var aabbMinDelta = new Vector3f(reader);
                    var aabbMaxDelta = new Vector3f(reader);
                }
                mesh.hasNormals = reader.ReadBoolean();
                mesh.hasTangents = reader.ReadBoolean();
                
                if (version.IsGreaterEqual(4,3)) //4.3 and up
                    reader.AlignStream();
                
                return mesh;
            }
        }

        public struct MeshBlendShapeChannel
        {
            public string name;
            public uint nameHash;
            public int frameIndex;
            public int frameCount;

            public static MeshBlendShapeChannel Read(BinaryReader reader)
            {
                var channel = new MeshBlendShapeChannel
                {
                    name = reader.ReadAlignedString(),
                    nameHash = reader.ReadUInt32(),
                    frameIndex = reader.ReadInt32(),
                    frameCount = reader.ReadInt32()
                };
                return channel;
            }
        }
    }
}
