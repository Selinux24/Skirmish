
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache build context
    /// </summary>
    public struct TileCacheBuildContext
    {
        /// <summary>
        /// Tile cache layer
        /// </summary>
        public TileCacheLayer Layer;
        /// <summary>
        /// Tile cache contour set
        /// </summary>
        public TileCacheContourSet ContourSet;
        /// <summary>
        /// Tile cache polygon mesh
        /// </summary>
        public TileCachePolyMesh PolyMesh;
    }
}
