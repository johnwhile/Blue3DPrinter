using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace UnityTool
{
    public sealed class GameObject : EditorExtension
    {
        public override ClassIDType ClassID => ClassIDType.GameObject;

        public PPtr<Component>[] Components;
        public string Name;
        
        Transform transform;

        public MeshRenderer MeshRenderer;
        public MeshFilter MeshFilter;
        public SkinnedMeshRenderer SkinnedMeshRenderer;
        //public Animator m_Animator;
        //public Animation m_Animation;

        public Transform Transform => transform;

        public GameObject(UnityFileReader reader) : base(reader)
        {
            var version = reader.Build;
            Components = new PPtr<Component>[reader.ReadInt32()];
            for (int i = 0; i < Components.Length; i++)
            {
                if (version.IsEarlier(5,5)) //5.5 down
                {
                    int first = reader.ReadInt32();
                }
                Components[i] = new PPtr<Component>(reader);
            }
            var m_Layer = reader.ReadInt32();
            Name = reader.ReadAlignedString();
        }

        /// <summary>
        /// to rebuild the hierarchical tree it is necessary to find all the transforms references
        /// </summary>
        public void LinkTransforms(UnityFileReader reader)
        {
            foreach (var pptr in Components)
                if (pptr.TryGetObjectInfo(reader, out var info))
                {
                    int index = info.index;
                    if (info.classID == ClassIDType.Transform)
                    {
                        if (!(Asset.GetObjectByIndex(reader, index) is Transform m_transform)) throw new InvalidCastException();
                        transform = m_transform;
                    }
                }
        }

        public void LinkComponents(UnityFileReader reader)
        {
            ObjectBase obj = null;

            foreach (var pptr in Components)
            {
                if (pptr.TryGetObjectInfo(reader, out var info))
                {
                    int index = info.index;
                    switch (info.classID)
                    {
                        case ClassIDType.MeshRenderer:
                            if (!(Asset.GetObjectByIndex(reader, index) is MeshRenderer m_meshr)) throw new InvalidCastException();
                            MeshRenderer = m_meshr;
                            break;

                        case ClassIDType.SkinnedMeshRenderer:
                            if (!(Asset.GetObjectByIndex(reader, index) is SkinnedMeshRenderer m_skinr)) throw new InvalidCastException();
                            SkinnedMeshRenderer = m_skinr;
                            break;

                        case ClassIDType.MeshFilter:
                            if (!(Asset.GetObjectByIndex(reader, index) is MeshFilter m_filter)) throw new InvalidCastException();
                            MeshFilter = m_filter;
                            break;
                    }
                }


                /*
                if (pptr.TryGet(reader, out var component))
                {
                    if (component is Transform m_transform)
                    {
                        Transform = m_transform;
                    }
                    else if (component is MeshRenderer m_meshr)
                    {
                        MeshRenderer = m_meshr;
                    }
                    else if (component is SkinnedMeshRenderer m_skinr)
                    {
                        SkinnedMeshRenderer = m_skinr;
                    }
                    else if (component is MeshFilter m_filter)
                    {
                        MeshFilter = m_filter;
                    }
                }*/
            }
        }

        public override string ToString()
        {
            return $"{ClassID} \"{Name}\"";
        }
    }
}
