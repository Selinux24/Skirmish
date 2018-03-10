using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Contains triangle meshes that represent detailed height data associated with the polygons in its associated polygon mesh object.
    /// </summary>
    public class PolyMeshDetail
    {
        /// <summary>
        /// The sub-mesh data. [Size: 4*#nmeshes] 
        /// </summary>
        public Int4[] meshes;
        /// <summary>
        /// The mesh vertices. [Size: 3*#nverts] 
        /// </summary>
        public Vector3[] verts;
        /// <summary>
        /// The mesh triangles. [Size: 4*#ntris] 
        /// </summary>
        public Int4[] tris;
        /// <summary>
        /// The number of sub-meshes defined by #meshes.
        /// </summary>
        public int nmeshes;
        /// <summary>
        /// The number of vertices in #verts.
        /// </summary>
        public int nverts;
        /// <summary>
        /// The number of triangles in #tris.
        /// </summary>
        public int ntris;
    }
}
