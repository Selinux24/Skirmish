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
        /// Cell size
        /// </summary>
        public float CellSize { get; set; }
        /// <summary>
        /// Bounds
        /// </summary>
        public BoundingBox Bounds { get; set; }
    }
}
