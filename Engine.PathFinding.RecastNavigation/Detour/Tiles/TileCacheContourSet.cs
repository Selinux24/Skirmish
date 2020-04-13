
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache contour set
    /// </summary>
    public struct TileCacheContourSet
    {
        /// <summary>
        /// Number con contours
        /// </summary>
        public int NConts { get; set; }
        /// <summary>
        /// Contour list
        /// </summary>
        public TileCacheContour[] Conts { get; set; }
    }
}
