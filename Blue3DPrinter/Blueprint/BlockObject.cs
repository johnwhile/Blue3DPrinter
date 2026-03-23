using System;
using System.Diagnostics;

using Common;
using Common.Maths;
using Common.IO.Wavefront;

namespace Blue3DPrinter
{
    public abstract class Block
    {
        const int MAXBLOCKSPERSIZE = 250;

        public readonly Blueprint blueprint;
        internal byte x, y, z;
        
        /// <summary>
        /// Position in the blueprint's grid coordinates. Remember that for directx coords system the x is inverted
        /// </summary>
        public Vector3i Position => new Vector3i(x, y, z);

        protected Block(Blueprint owner, int x, int y, int z)
        {
            blueprint = owner;
            if (x >= MAXBLOCKSPERSIZE || y >= MAXBLOCKSPERSIZE || z >= MAXBLOCKSPERSIZE)
                throw new ArithmeticException("the limit of 250 block per size is hardcoded");
            this.x = (byte)x;
            this.y = (byte)y;
            this.z = (byte)z;
        }

    }

    /// <summary>
    /// A block's device can contain more block positions
    /// </summary>
    public class BlockReference : Block
    {
        public BlockObject Reference;

        public BlockReference(BlockObject parent, int x, int y, int z) : base(parent.blueprint, x, y, z)
        {

        }
    }

    /// <summary>
    /// Blueprint's visible block. Contain the specified information of block like: position, rotation, 6xcolor, etc...
    /// ATTENTION: this class are generated for each block in the blueprint's grid and contain only not-instantiable data,
    /// for example, instead, BlockModel and BlueprintMesh are instantiable data and are shared between more BlockObject
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class BlockObject : Block
    {
        const uint MASK_24 = 0x1000000;   //   0000 0001    0    0    0    0    0    0  24 << 1 
        const uint MASK_ID = 0x07ff;       //  0000 0000 0000 0000 0000 0111 1111 1111   0 << 7FF
        const uint MASK_ROT = 0xf800;      //  0000 0000 0000 0000 1111 1000 0000 0000  11 << 1F  (63.488u) (neg = -63.489)
        const uint MASK_DEM = 0xff0000;    //  0000 0000 1111 1111 0000 0000 0000 0000  16 << FF
        const uint MASK_CHILD = 0x3E000000;//  0011 1110 0000 0000 0000 0000 0000 0000  25 << 1F
        //set_SearchSelection
        const uint MASK_SAVE = 0x80000000; //  1000 0000 0000 0000 0000 0000 0000 0000  31 << 1
        const uint MASK_30 = 0x1000000;    //  0100    0    0    0    0    0    0    0  30 << 1 

        BlockDescription description;

        internal uint caption;
        internal int color;

        #region not used by this tool
        internal ushort unknow;
        internal long texture;
        internal byte texturerot; //encoded rotation
        internal int overlay;
        internal int overlayrot; //encoded rotation
        #endregion

        internal BlockObject(Blueprint blueprint, int x, int y, int z) : base(blueprint, x, y, z)
        {

        }
        
        public BlockDescription Description
        {
            get { return description; }
            set { description = value; }
        }

        public string Name => description != null ? description.Name : null;

        /// <summary>
        /// Enable or Disable it if something wrong reading description or model, or you want not to be esported
        /// </summary>
        public bool Enable { get; set; } = true;
        /// <summary>
        /// The Model file, not contain all meshes from asset but only those relevant for the creation of the visible objects.
        /// the meshes are organized in a tree-structure
        /// </summary>
        public BlockModel Model;
        /// <summary>
        /// Use <seealso cref="BlocksConfig.GetDescription(int)"/> to get the description.
        /// </summary>
        public int BlockId
        {
            get
            {
                int id = (int)(caption & MASK_ID);
                return (caption & MASK_24) > 0 ? id + 2048 : id;
            }
            set 
            { 
                caption &= ~(MASK_ID & MASK_24);
                if (value > 2047)
                { 
                    caption |= MASK_24;
                    value -= 2047;
                }
                caption |= (uint)value & MASK_ID;
            }
        }
        /// <summary>
        /// reference to childshapes list in <see cref="Description"/>
        /// </summary>
        public int ChildIndex
        {
            get { return (int)(caption & MASK_CHILD) >> 25; }
            set { caption &= ~MASK_CHILD; caption |= ((uint)value << 25) & MASK_CHILD; }
        }
        /// <summary>
        /// Use <see cref="BlueprintRotation.GetRotationMatrix(CubeRotation)"/> to get the rotation in transform notation
        /// </summary>
        public CubeRotation Rotation
        {
            get { return (CubeRotation)((caption & MASK_ROT) >> 11); }
            set { caption &= ~MASK_ROT;caption |= ((uint)value << 11) & MASK_ROT; }
        }

        /// <summary>
        /// not investigated
        /// </summary>
        public int Demain
        {
            get { return (int)(caption & MASK_DEM) >> 16; }
            set { caption &= ~MASK_DEM; caption |= ((uint)value << 16) & MASK_DEM; }
        }
        /// <summary>
        /// not investigated, possible blockid over 2048 special case
        /// </summary>
        public bool Save
        {
            get { return (caption & MASK_SAVE) >> 31 > 0; }
            set { caption &= ~MASK_SAVE; if (value) caption |= ((uint)1 << 31); }
        }


        public byte[] GetColorArray()
        {
            byte[] colors = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                int shift = i * 5;
                colors[i] = (byte)((color >> shift) & 0x1F);
            }
            return colors;
        }


        /// <summary>
        /// For hull version the block can be splitted in 6 or less group, for devices the first index is the color of block
        /// </summary>
        public byte GetColorIndex(int face)
        {
            // the Cardinal value match with face index
            int shift = face * 5;
            return (byte)((color >> shift) & 0x1F);
        }

        /// <summary>
        /// <see cref="GetColorIndex(int)"/>
        /// </summary>
        public byte GetColorIndex(Cardinal face)
        {
            return GetColorIndex((int)face);
        }

        /// <summary>
        /// </summary>
        /// <param name="value">Must be in [0-31] range</param>
        public void SetColorIndex(Cardinal face, byte value)
        {
            int shift = (int)face * 5;
            value &= 0x1F;
            color = (color & ~0x1F) | (value & 0x1F) << shift;
        }

        /// <summary>
        /// not implemented
        /// </summary>
        public byte GetTextureIndex(Cardinal face)
        {
            int shift = (int)face * 6;
            return (byte)((texture >> shift) & 0x3F);
        }
        /// <summary>
        /// not implemented
        /// </summary>
        public bool GetTextureRotation(Cardinal face)
        {
            return (texturerot & (1 << (int)face)) > 0;
        }

        /// <summary>
        /// Triangle and/or Vertices mask for blockmodel, must be generate for each block and it's used to mask the triangles. 
        /// </summary>
        /// <remarks>
        /// the idea is to separate the instantiable information for each block's object because same BlockModel is shared between more blocks
        /// </remarks>
        public BlockHideInstance HideMapInstance;

        /// <summary>
        /// Cardinal meshes are resorted for each blocks because depend also by block orientation
        /// </summary>
        /// <param name="absolute">the direction without rotation, in main blueprint orientation</param>
        public Cardinal GetRelativeCardinal(Cardinal absolute)
        {
            return BlueprintRotation.RotateDirection(absolute, Rotation);
        }

        /// <summary>
        /// Matrix to convert releted <see cref="Model"/> in blueprint world space
        /// </summary>
        /// <remarks>some auto correction are unknow.... i noticed what x axis of blueprint is inverted</remarks>
        /// <param name="setRotationToZero">for debug, set cube rotation to identity</param>
        public Matrix4x4f GetBlockTransform_old(bool setRotationToZero = false)
        {
            Matrix4x4f R = BlueprintRotation.GetRotationMatrix(setRotationToZero ? CubeRotation.i : Rotation);

            // the 3d model extracted from asset report a different pivot point... 
            if (BlockId == 1510) R.PreTranslating(-5.266f, 0, 0);
            if (BlockId == 1498) R.PreTranslating(0, 0 , -4.1f);

            // fix difference from shape and model files:
            // the model version have the pivot at the center of the block coordinates
            // so for cube version do a pre-traslation
            if (Description.IsCubeShape)
            {
                //R = R * Matrix4.Translating(0, -0.5f, 0);
                //R.Translate(0, -0.5f, 0); // this not work because you have to traslate BEFORE rotate
                R.PreTranslating(0, -0.5f, 0);
            }
            var T = Matrix4x4f.Translating(-x, y, z);
            return T * R;
        }

        /// <summary>
        /// Matrix to convert releted <see cref="Model"/> in blueprint world space
        /// </summary>
        /// <remarks>some auto correction are unknow.... i noticed what x axis of blueprint is inverted</remarks>
        /// <param name="setRotationToZero">for debug, set cube rotation to identity</param>
        public Matrix4x4f GetBlockTransform(bool setRotationToZero = false)
        {
            var R = BlueprintRotation.GetRotationMatrix(setRotationToZero ? CubeRotation.i : Rotation);
            var T = Matrix4x4f.Translating(-x, y, z);
            return T * R;
        }

        public Matrix4x4f GetBlockRotation()
        {
            return BlueprintRotation.GetRotationMatrix(Rotation);
        }

        public override string ToString()
        {
            return string.Format("Id:{0} x{1}y{2}z{3} descr: {4}  model: {5}",
                BlockId, x, y, z,
                Description != null ? Description.ToString() : "-null-",
                Model != null ? Model.Name : "notfound");
        }
        /// <summary>
        /// Unique printable name: BlockID_Rotation_Position_ModelName
        /// Useful for wavefront
        /// </summary>
        public string ToStringUnique
        {
            get { return string.Format("{0}_{1}_[{2},{3},{4}]_{5}", BlockId, Rotation.ToString(), x, y, z, Model != null ? Model.Name : "-nomodel-"); }
        }
        public string ToStringUnique2
        {
            get { return string.Format("{0}{1}{2}", BlockId, Rotation.ToString(), Position.ToHexString("X2")); }
        }
    }
}
