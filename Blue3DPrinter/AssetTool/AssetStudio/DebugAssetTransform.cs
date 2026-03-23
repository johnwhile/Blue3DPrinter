
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using Common.Maths;

namespace AssetStudio
{
    /// <summary>
    /// temporary class, I store the original vector from asset reading
    /// </summary>
    /*
    public class AssetTransform : FileBase
    {
        const FileExtension ASSET_TRANS_EXT = (FileExtension)11;

        public Vector3f traslation = Vector3f.Zero;
        public Quaternion4f rotation = Quaternion4f.Identity;
        public Vector3f scale = Vector3f.One;
        public Matrix4x4f transform = Matrix4x4f.Identity;


        static AssetTransform()
        {
            Debug.WriteLine("CALL STATIC CONSTRUCTOR for AssetTransform");
            if (!AddFileExtensionAssociation<AssetTransform>(ASSET_TRANS_EXT))
            {

            }
        }


        private AssetTransform() : base()
        {
            header.Extension = ASSET_TRANS_EXT;
        }

        public AssetTransform(string name = null) : base(name)
        {
            header.Extension = ASSET_TRANS_EXT;
        }

        public override bool Read(BinaryReader br)
        {
            long begin = br.BaseStream.Position;
            if (!header.Read(br)) { br.BaseStream.Position = begin; return false; }

            if (header.Extension != ASSET_TRANS_EXT) throw new Exception("the header of AssetTransform return a different extension, please check");

            traslation = new Vector3f(br);
            rotation = new Vector4f(br);
            scale = new Vector3f(br);
            transform = new Matrix4x4f(br);

            // check file integrity
            long end = br.BaseStream.Position;
            if (header.FileSize != (uint)(end - begin))
                Debug.WriteLine("> the returned size of AssetTransform is different");
            return true;
        }

        public override uint Write(BinaryWriter bw)
        {
            long begin = bw.BaseStream.Position;

            if (!header.Write(bw)) return 0;

            bw.Write(traslation);
            bw.Write(rotation);
            bw.Write(scale);
            bw.Write(transform);

            long end = bw.BaseStream.Position;

            //rewrite the file size value
            uint size = (uint)(end - begin);
            bw.BaseStream.Position = begin;
            bw.Write(size);
            bw.BaseStream.Position = end;
            return size;
        }
    }*/
}
