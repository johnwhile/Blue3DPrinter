using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Common;
using Common.Maths;

namespace UnityTool
{
    public struct StaticBatchInfo
    {
        public ushort firstSubMesh;
        public ushort subMeshCount;

        public StaticBatchInfo(BinaryReader reader)
        {
            firstSubMesh = reader.ReadUInt16();
            subMeshCount = reader.ReadUInt16();
        }
    }
    public sealed class MeshRenderer : Renderer
    {
        public override ClassIDType ClassID => ClassIDType.MeshRenderer;

        public MeshRenderer(UnityFileReader reader) : base(reader)
        {

        }
    }
    public abstract class Renderer : Component
    {
        public override ClassIDType ClassID => ClassIDType.Renderer;

        public PPtr<Material>[] Materials;
        public StaticBatchInfo StaticBatchInfo;
        public uint[] SubsetIndices;

        protected Renderer(UnityFileReader reader) : base(reader)
        {
            var version = reader.Build;

            if (version.IsEarlier(5)) //5.0 down
            {
                var m_Enabled = reader.ReadBoolean();
                var m_CastShadows = reader.ReadBoolean();
                var m_ReceiveShadows = reader.ReadBoolean();
                var m_LightmapIndex = reader.ReadByte();
            }
            else //5.0 and up
            {
                if (version.IsGreaterEqual(5, 4)) //5.4 and up
                {
                    var m_Enabled = reader.ReadBoolean();
                    var m_CastShadows = reader.ReadByte();
                    var m_ReceiveShadows = reader.ReadByte();
                    if (version.IsGreaterEqual(2017, 2)) //2017.2 and up
                    {
                        var m_DynamicOccludee = reader.ReadByte();
                    }
                    if (version.IsGreaterEqual(2021)) //2021.1 and up
                    {
                        var m_StaticShadowCaster = reader.ReadByte();
                    }
                    var m_MotionVectors = reader.ReadByte();
                    var m_LightProbeUsage = reader.ReadByte();
                    var m_ReflectionProbeUsage = reader.ReadByte();
                    if (version.IsGreaterEqual(2019, 3)) //2019.3 and up
                    {
                        var m_RayTracingMode = reader.ReadByte();
                    }
                    if (version.IsGreaterEqual(2020)) //2020.1 and up
                    {
                        var m_RayTraceProcedural = reader.ReadByte();
                    }
                    reader.AlignStream();
                }
                else
                {
                    var m_Enabled = reader.ReadBoolean();
                    reader.AlignStream();
                    var m_CastShadows = reader.ReadByte();
                    var m_ReceiveShadows = reader.ReadBoolean();
                    reader.AlignStream();
                }

                if (version.IsGreaterEqual(2018)) //2018 and up
                {
                    var m_RenderingLayerMask = reader.ReadUInt32();
                }

                if (version.IsGreaterEqual(2018, 3)) //2018.3 and up
                {
                    var m_RendererPriority = reader.ReadInt32();
                }

                var m_LightmapIndex = reader.ReadUInt16();
                var m_LightmapIndexDynamic = reader.ReadUInt16();
            }

            if (version.IsGreaterEqual(3)) //3.0 and up
            {
                var m_LightmapTilingOffset = new Vector4f(reader);
            }

            if (version.IsGreaterEqual(5)) //5.0 and up
            {
                var m_LightmapTilingOffsetDynamic = new Vector4f(reader);
            }

            Materials = new PPtr<Material>[reader.ReadInt32()];
            for (int i = 0; i < Materials.Length; i++)
                Materials[i] = new PPtr<Material>(reader);
            

            if (version.IsEarlier(3)) //3.0 down
            {
                var m_LightmapTilingOffset = new Vector4f(reader);
            }
            else //3.0 and up
            {
                if (version.IsGreaterEqual(5, 5)) //5.5 and up
                    StaticBatchInfo = new StaticBatchInfo(reader);
                else
                    SubsetIndices = reader.ReadUInt32Array(reader.ReadInt32());
                
                var m_StaticBatchRoot = new PPtr<Transform>(reader);
            }

            if (version.IsGreaterEqual(5, 4)) //5.4 and up
            {
                var m_ProbeAnchor = new PPtr<Transform>(reader);
                var m_LightProbeVolumeOverride = new PPtr<GameObject>(reader);
            }
            else if (version.IsGreaterEqual(3, 5)) //3.5 - 5.3
            {
                var m_UseLightProbes = reader.ReadBoolean();
                reader.AlignStream();

                if (version.IsGreaterEqual(5))//5.0 and up
                {
                    var m_ReflectionProbeUsage = reader.ReadInt32();
                }

                var m_LightProbeAnchor = new PPtr<Transform>(reader); //5.0 and up m_ProbeAnchor
            }

            if (version.IsGreaterEqual(4, 3)) //4.3 and up
            {
                if (version.IsVersion(4, 3)) //4.3
                {
                    var m_SortingLayer = reader.ReadInt16();
                }
                else
                {
                    var m_SortingLayerID = reader.ReadUInt32();
                }

                //SInt16 m_SortingLayer 5.6 and up
                var m_SortingOrder = reader.ReadInt16();
                reader.AlignStream();
            }
        }
    }
}
