using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache contour
    /// </summary>
    public struct TileCacheContour
    {
        /// <summary>
        /// Number of vertices
        /// </summary>
        public int NVerts { get; set; }
        /// <summary>
        /// Vertices (indices)
        /// </summary>
        public Int4[] Verts { get; set; }
        /// <summary>
        /// Regions
        /// </summary>
        public int Reg { get; set; }
        /// <summary>
        /// Areas
        /// </summary>
        public AreaTypes Area { get; set; }
    }
}
