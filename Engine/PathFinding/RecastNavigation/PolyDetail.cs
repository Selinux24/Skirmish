using System;

namespace Engine.PathFinding.RecastNavigation
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
        public int vertBase;
        /// <summary>
        /// The offset of the triangles in the dtMeshTile::detailTris array.
        /// </summary>
        public int triBase;
        /// <summary>
        /// The number of vertices in the sub-mesh.
        /// </summary>
        public int vertCount;
        /// <summary>
        /// The number of triangles in the sub-mesh.
        /// </summary>
        public int triCount;

        /// <summary>
        /// Gets the text representation of the polygon detail
        /// </summary>
        /// <returns>Returns the text representation of the polygon detail</returns>
        public override string ToString()
        {
            return string.Format("VertBase {0}; TriBase {1}; VertCount {2}; TriCount {3};",
                vertBase, triBase,
                vertCount, triCount);
        }
    };
}
