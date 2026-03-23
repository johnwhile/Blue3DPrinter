
using Common;

namespace UnityTool
{
    public class ObjectBase
    {
        public virtual ClassIDType ClassID => ClassIDType.Object;

        public AssetFile Asset;


        public ObjectBase(UnityFileReader reader)
        {
            Asset = reader.Asset;

            if (reader.Target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
        }

        public byte[] GetRawData(UnityFileReader reader)
        {
            reader.Position = reader.ObjectInfo.byteStart;
            return reader.ReadBytes((int)reader.ObjectInfo.byteSize);
        }

        public override string ToString()
        {
            return $"{ClassID}";
        }
    }

    public abstract class EditorExtension : ObjectBase
    {
        public override ClassIDType ClassID => ClassIDType.EditorExtension;

        protected EditorExtension(UnityFileReader reader) : base(reader)
        {
            if (reader.Target == BuildTarget.NoTarget)
            {
                var m_PrefabParentObject = new PPtr<EditorExtension>(reader);
                var m_PrefabInternal = new PPtr<ObjectBase>(reader); //PPtr<Prefab>
            }
        }
    }

    public abstract class NamedObject : EditorExtension
    {
        public override ClassIDType ClassID => ClassIDType.NamedObject;

        public string Name;

        protected NamedObject(UnityFileReader reader) : base(reader)
        {
            Name = reader.ReadAlignedString();
        }

        public override string ToString()
        {
            return $"{ClassID}-{Name}";
        }
    }

    public abstract class Component : EditorExtension
    {
        public override ClassIDType ClassID => ClassIDType.Component;
        public PPtr<GameObject> GameObject;
        
        protected Component(UnityFileReader reader) : base(reader)
        {
            GameObject = new PPtr<GameObject>(reader);
        }
        public override string ToString()
        {
            return $"{ClassID}";
        }
    }
}
