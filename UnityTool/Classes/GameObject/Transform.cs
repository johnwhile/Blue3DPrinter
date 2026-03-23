using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using Common.Maths;

namespace UnityTool
{
    public class Transform : Component
    {
        public override ClassIDType ClassID => ClassIDType.Transform;

        public Quaternion4f LocalRotation;
        public Vector3f LocalPosition;
        public Vector3f LocalScale;
        public PPtr<Transform>[] Children;
        public PPtr<Transform> Father;

        public Transform(UnityFileReader reader) : base(reader)
        {
            LocalRotation = new Vector4f(reader);
            LocalPosition = new Vector3f(reader);
            LocalScale = new Vector3f(reader);

            Children = new PPtr<Transform>[reader.ReadInt32()];
            for (int i = 0; i < Children.Length; i++)
                Children[i] = new PPtr<Transform>(reader);
            
            Father = new PPtr<Transform>(reader);
        }
    }
}
