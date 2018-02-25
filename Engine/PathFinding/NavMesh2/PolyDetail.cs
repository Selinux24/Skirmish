
namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Defines the location of detail sub-mesh data within a dtMeshTile.
    /// </summary>
    public struct PolyDetail
    {
        /// <summary>
        /// The offset of the vertices in the dtMeshTile::detailVerts array.
        /// </summary>
        public uint vertBase;
        /// <summary>
        /// The offset of the triangles in the dtMeshTile::detailTris array.
        /// </summary>
        public uint triBase;
        /// <summary>
        /// The number of vertices in the sub-mesh.
        /// </summary>
        public int vertCount;
        /// <summary>
        /// The number of triangles in the sub-mesh.
        /// </summary>
        public int triCount;
    };
}
