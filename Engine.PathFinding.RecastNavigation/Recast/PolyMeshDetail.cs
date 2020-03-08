using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Contains triangle meshes that represent detailed height data associated with the polygons in its associated polygon mesh object.
    /// </summary>
    public class PolyMeshDetail
    {
        /// <summary>
        /// The sub-mesh data. [Size: 4*#nmeshes] 
        /// </summary>
        public Int4[] meshes { get; set; }
        /// <summary>
        /// The mesh vertices. [Size: 3*#nverts] 
        /// </summary>
        public Vector3[] verts { get; set; }
        /// <summary>
        /// The mesh triangles. [Size: 4*#ntris] 
        /// </summary>
        public Int4[] tris { get; set; }
        /// <summary>
        /// The number of sub-meshes defined by #meshes.
        /// </summary>
        public int nmeshes { get; set; }
        /// <summary>
        /// The number of vertices in #verts.
        /// </summary>
        public int nverts { get; set; }
        /// <summary>
        /// The number of triangles in #tris.
        /// </summary>
        public int ntris { get; set; }
    }
}
