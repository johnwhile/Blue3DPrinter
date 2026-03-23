using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTool
{
    public sealed class SkinnedMeshRenderer : Renderer
    {
        public override ClassIDType ClassID => ClassIDType.SkinnedMeshRenderer;

        public PPtr<Mesh> m_Mesh;
        public PPtr<Transform>[] m_Bones;
        public float[] m_BlendShapeWeights;


        public Mesh GetMesh(UnityFileReader reader)
        {
            if (m_Mesh.TryGet(reader, out var mesh))
            {
                return mesh;
            }
            return null;
        }


        public SkinnedMeshRenderer(UnityFileReader reader) : base(reader)
        {
            var version = reader.Build;

            int m_Quality = reader.ReadInt32();
            var m_UpdateWhenOffscreen = reader.ReadBoolean();
            var m_SkinNormals = reader.ReadBoolean(); //3.1.0 and below
            reader.AlignStream();

            if (version.Major == 2 && version.Minor < 6) //2.6 down
            {
                //var m_DisableAnimationWhenOffscreen = new PPtr<Animation>(reader);
            }

            m_Mesh = new PPtr<Mesh>(reader);

            reader.CreateInstanceArray(out m_Bones);

            if (version.IsGreaterEqual(4, 3)) //4.3 and up
                m_BlendShapeWeights = reader.ReadSingleArray();
            
        }
    }
}
