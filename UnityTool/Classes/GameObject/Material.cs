using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using Common.Maths;

namespace UnityTool
{
    public class UnityTexEnv
    {
        public PPtr<Texture> m_Texture;
        public Vector2f m_Scale;
        public Vector2f m_Offset;

        public UnityTexEnv(UnityFileReader reader)
        {
            m_Texture = new PPtr<Texture>(reader);
            m_Scale = new Vector2f(reader);
            m_Offset = new Vector2f(reader);
        }
    }

    public class UnityPropertySheet
    {
        public KeyValuePair<string, UnityTexEnv>[] m_TexEnvs;
        public KeyValuePair<string, int>[] m_Ints;
        public KeyValuePair<string, float>[] m_Floats;
        public KeyValuePair<string, Vector4f>[] m_Colors;

        public UnityPropertySheet(UnityFileReader reader)
        {
            var version = reader.Build;

            m_TexEnvs = new KeyValuePair<string, UnityTexEnv>[reader.ReadInt32()];
            for (int i = 0; i < m_TexEnvs.Length; i++)
                m_TexEnvs[i] = new KeyValuePair<string, UnityTexEnv>(reader.ReadAlignedString(), new UnityTexEnv(reader));
            
            if (version.Major >= 2021) //2021.1 and up
            {
                m_Ints = new KeyValuePair<string, int>[reader.ReadInt32()];
                for (int i = 0; i < m_Ints.Length; i++)
                    m_Ints[i] = new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32());
            }

            m_Floats = new KeyValuePair<string, float>[reader.ReadInt32()];
            for (int i = 0; i < m_Floats.Length; i++)
                m_Floats[i] = new KeyValuePair<string, float>(reader.ReadAlignedString(), reader.ReadSingle());
            

            m_Colors = new KeyValuePair<string, Vector4f>[reader.ReadInt32()];
            for (int i = 0; i < m_Colors.Length; i++)
                m_Colors[i] = new KeyValuePair<string, Vector4f>(reader.ReadAlignedString(), new Vector4f(reader));
            
        }
    }


    public sealed class Material : NamedObject
    {
        public override ClassIDType ClassID => ClassIDType.Material;

        public PPtr<Shader> m_Shader;
        public UnityPropertySheet m_SavedProperties;

        public Material(UnityFileReader reader) : base(reader)
        {
            var version = reader.Build;

            m_Shader = new PPtr<Shader>(reader);

            if (version.IsGreaterEqual(4,1)) //4.x
            {
                var m_ShaderKeywords = reader.ReadStringArray();
            }

            if (version.IsGreaterEqual(5)) //5.0 and up
            {
                var m_ShaderKeywords = reader.ReadAlignedString();
                var m_LightmapFlags = reader.ReadUInt32();
            }

            if (version.IsGreaterEqual(5,6)) //5.6 and up
            {
                var m_EnableInstancingVariants = reader.ReadBoolean();
                //var m_DoubleSidedGI = a_Stream.ReadBoolean(); //2017 and up
                reader.AlignStream();
            }

            if (version.IsGreaterEqual(4,3)) //4.3 and up
            {
                var m_CustomRenderQueue = reader.ReadInt32();
            }

            if (version.IsGreaterEqual(5,1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }

            if (version.IsGreaterEqual(5,6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadStringArray();
            }

            m_SavedProperties = new UnityPropertySheet(reader);

            //vector m_BuildTextureStacks 2020 and up
        }
    }
}
