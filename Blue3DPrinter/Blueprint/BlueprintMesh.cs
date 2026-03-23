using System;
using System.Collections.Generic;
using System.Diagnostics;

using Common.Maths;


namespace Blue3DPrinter
{

    /// <summary>
    /// Contain the BlockModel's geometry.  
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class BlueprintMesh
    {
        Mesh geometry;

        public string Name { get; set; }

        /// <summary>
        /// Optional flag to mark this mesh
        /// </summary>
        public bool Enable = true;
        /// <summary>
        /// Only for cardinals, a mesh can be hide or not, depends by its neighbor's blocks
        /// </summary>
        public Hide Removable = Hide.None;
        /// <summary>
        /// Only for <see cref="Hide.Partial"/> case, will be use to improve comparison
        /// </summary>
        /// <remarks>NOT IMPLEMENTED</remarks>
        public RasterizedPoly8x8 RemovableMask;
        /// <summary>
        /// Define the direction of mesh relative to its block, 
        /// </summary>
        public Cardinal Direction = Cardinal.Undefined;
        /// <summary>
        /// Some meshes in game have a special function
        /// </summary>
        public bool IsCardinalMesh => Direction != Cardinal.Undefined;

        public List<int> SubMeshMaterials;

        private BlueprintMesh()
        {

        }

        public BlueprintMesh(Mesh geometry, Matrix4x4f worldtransform)
        {
            this.geometry = geometry;
            this.geometry.Transform = geometry.Transform * worldtransform;

            Name = geometry.Name;

            SubMeshMaterials = new List<int>(geometry.SubMeshes.Count);
            for (int i = 0; i < geometry.SubMeshes.Count; i++) SubMeshMaterials.Add(0);
        }

        /// <summary>
        /// submesh material will be lost
        /// </summary>
        public static implicit operator Mesh(BlueprintMesh bluemesh)
        {
            return bluemesh.geometry;
        }
    }


    /// <summary>
    /// Relative to hide method to reduce number of triangles, like in-game.
    /// This information was parsed by mesh name of game's assets
    /// </summary>
    public enum Hide
    {
        /// <summary>
        /// All faces not lies on cube's face surface so they don't interact with the hide method
        /// </summary>
        None,
        /// <summary>
        /// Not all faces lies on cube's face surface
        /// </summary>
        Partial,
        /// <summary>
        /// All faces lies on total cube's face surface so can hide every adjacent block surface
        /// </summary>
        Total,
    }
}
