using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Tile build parameters
    /// </summary>
    public struct TileParams
    {
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Tile cell size
        /// </summary>
        public float TileCellSize { get; set; }
        /// <summary>
        /// Bounds
        /// </summary>
        public BoundingBox Bounds { get; set; }
    }
}
