using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using Common.Maths;
using System.IO;
using System.Collections;

namespace UnityTool
{
    public partial class Mesh
    {
        public class VertexData
        {
            public uint m_CurrentChannels;
            public uint m_VertexCount;
            public ChannelInfo[] m_Channels;
            public StreamInfo[] m_Streams;
            public byte[] m_DataSize;


            public VertexData(UnityFileReader reader)
            {
                var version = reader.Build;

                if (version.Major < 2018)//2018 down
                    m_CurrentChannels = reader.ReadUInt32();

                m_VertexCount = reader.ReadUInt32();
                if (version.Major >= 4) //4.0 and up
                {
                    reader.CreateInstanceArray(out m_Channels);
                    //m_Channels = new ChannelInfo[reader.ReadInt32()];
                    //for (int i = 0; i < m_Channels.Length; i++)
                    //    m_Channels[i] = new ChannelInfo(reader);
                }

                if (version.Major < 5) //5.0 down
                {
                    m_Streams = new StreamInfo[version.Major < 4 ? 4 : reader.ReadInt32()];

                    for (int i = 0; i < m_Streams.Length; i++)
                        m_Streams[i] = new StreamInfo(reader);
                    
                    if (version.Major < 4) GetChannels(version);//4.0 down
                }
                else GetStreams(version);//5.0 and up


                m_DataSize = reader.ReadBytes(reader.ReadInt32());
                reader.AlignStream();
            }

            private void GetStreams(BuildVersion version)
            {
                var streamCount = m_Channels.Max(x => x.stream) + 1;
                m_Streams = new StreamInfo[streamCount];
                uint offset = 0;
                for (int s = 0; s < streamCount; s++)
                {
                    uint chnMask = 0;
                    uint stride = 0;
                    for (int chn = 0; chn < m_Channels.Length; chn++)
                    {
                        var m_Channel = m_Channels[chn];
                        if (m_Channel.stream == s)
                        {
                            if (m_Channel.dimension > 0)
                            {
                                chnMask |= 1u << chn;
                                stride += m_Channel.dimension * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.format, version));
                            }
                        }
                    }
                    m_Streams[s] = new StreamInfo
                    {
                        channelMask = chnMask,
                        offset = offset,
                        stride = stride,
                        dividerOp = 0,
                        frequency = 0
                    };
                    offset += m_VertexCount * stride;
                    //static size_t AlignStreamSize (size_t size) { return (size + (kVertexStreamAlign-1)) & ~(kVertexStreamAlign-1); }
                    offset = (offset + (16u - 1u)) & ~(16u - 1u);
                }
            }

            private void GetChannels(BuildVersion version)
            {
                m_Channels = new ChannelInfo[6];
                for (int i = 0; i < 6; i++)
                    m_Channels[i] = new ChannelInfo();
                
                for (var s = 0; s < m_Streams.Length; s++)
                {
                    var m_Stream = m_Streams[s];
                    var channelMask = new BitArray(new[] { (int)m_Stream.channelMask });
                    byte offset = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        if (channelMask.Get(i))
                        {
                            var m_Channel = m_Channels[i];
                            m_Channel.stream = (byte)s;
                            m_Channel.offset = offset;
                            switch (i)
                            {
                                case 0: //kShaderChannelVertex
                                case 1: //kShaderChannelNormal
                                    m_Channel.format = 0; //kChannelFormatFloat
                                    m_Channel.dimension = 3;
                                    break;
                                case 2: //kShaderChannelColor
                                    m_Channel.format = 2; //kChannelFormatColor
                                    m_Channel.dimension = 4;
                                    break;
                                case 3: //kShaderChannelTexCoord0
                                case 4: //kShaderChannelTexCoord1
                                    m_Channel.format = 0; //kChannelFormatFloat
                                    m_Channel.dimension = 2;
                                    break;
                                case 5: //kShaderChannelTangent
                                    m_Channel.format = 0; //kChannelFormatFloat
                                    m_Channel.dimension = 4;
                                    break;
                            }
                            offset += (byte)(m_Channel.dimension * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.format, version)));
                        }
                    }
                }
            }
        }



        public struct ChannelInfo
        {
            public byte stream;
            public byte offset;
            public byte format;
            public byte dimension;

            public ChannelInfo(BinaryReader reader)
            {
                stream = reader.ReadByte();
                offset = reader.ReadByte();
                format = reader.ReadByte();
                dimension = (byte)(reader.ReadByte() & 0xF);
            }
        }

        public struct StreamInfo
        {
            public uint channelMask;
            public uint offset;
            public uint stride;
            public uint align;
            public byte dividerOp;
            public ushort frequency;

            public StreamInfo(UnityFileReader reader)
            {
                channelMask = reader.ReadUInt32();
                offset = reader.ReadUInt32();

                if (reader.Build.Major < 4) //4.0 down
                {
                    stride = reader.ReadUInt32();
                    align = reader.ReadUInt32();
                    dividerOp = 0;
                    frequency = 0;
                }
                else
                {
                    stride = reader.ReadByte();
                    dividerOp = reader.ReadByte();
                    frequency = reader.ReadUInt16();
                    align = 0;
                }
            }
        }
    }
}
