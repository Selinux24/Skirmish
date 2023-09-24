using System;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Defines the location of detail sub-mesh data within a dtMeshTile.
    /// </summary>
    [Serializable]
    public struct PolyDetail
    {
        /// <summary>
        /// The offset of the vertices in the dtMeshTile::detailVerts array.
        /// </summary>
        public int VertBase { get; set; }
        /// <summary>
        /// The offset of the triangles in the dtMeshTile::detailTris array.
        /// </summary>
        public int TriBase { get; set; }
        /// <summary>
        /// The number of vertices in the sub-mesh.
        /// </summary>
        public int VertCount { get; set; }
        /// <summary>
        /// The number of triangles in the sub-mesh.
        /// </summary>
        public int TriCount { get; set; }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"VertBase {VertBase}; TriBase {TriBase}; VertCount {VertCount}; TriCount {TriCount};";
        }
    };
}
