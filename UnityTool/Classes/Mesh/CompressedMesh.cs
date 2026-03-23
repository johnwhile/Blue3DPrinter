using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Common;

namespace UnityTool
{
    public partial class Mesh
    {
        public class CompressedMesh
        {
            public PackedFloatVector m_Vertices;
            public PackedFloatVector m_UV;
            public PackedFloatVector m_BindPoses;
            public PackedFloatVector m_Normals;
            public PackedFloatVector m_Tangents;
            public PackedIntVector m_Weights;
            public PackedIntVector m_NormalSigns;
            public PackedIntVector m_TangentSigns;
            public PackedFloatVector m_FloatColors;
            public PackedIntVector m_BoneIndices;
            public PackedIntVector m_Triangles;
            public PackedIntVector m_Colors;
            public uint m_UVInfo;

            public CompressedMesh(UnityFileReader reader)
            {
                var version = reader.Build;

                m_Vertices = new PackedFloatVector(reader);
                m_UV = new PackedFloatVector(reader);
                
                if (version.Major < 5) 
                    m_BindPoses = new PackedFloatVector(reader);
                
                m_Normals = new PackedFloatVector(reader);
                m_Tangents = new PackedFloatVector(reader);
                m_Weights = new PackedIntVector(reader);
                m_NormalSigns = new PackedIntVector(reader);
                m_TangentSigns = new PackedIntVector(reader);
                
                if (version.Major >= 5)
                    m_FloatColors = new PackedFloatVector(reader);
                
                m_BoneIndices = new PackedIntVector(reader);
                m_Triangles = new PackedIntVector(reader);

                if (version.Major > 3 || (version.Major == 3 && version.Minor >= 5)) //3.5 and up
                    if (version.Major < 5)
                        m_Colors = new PackedIntVector(reader);
                    else
                        m_UVInfo = reader.ReadUInt32();
            }
        }


    }
}
