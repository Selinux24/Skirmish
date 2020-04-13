
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Tile cache header
    /// </summary>
    public struct TileCacheLayer
    {
        /// <summary>
        /// Header
        /// </summary>
        public TileCacheLayerHeader Header { get; set; }
        /// <summary>
        /// Region count.
        /// </summary>
        public int RegCount { get; set; }
        /// <summary>
        /// Height list
        /// </summary>
        public int[] Heights { get; set; }
        /// <summary>
        /// Area list
        /// </summary>
        public AreaTypes[] Areas { get; set; }
        /// <summary>
        /// Connections
        /// </summary>
        public int[] Cons { get; set; }
        /// <summary>
        /// Regions
        /// </summary>
        public int[] Regs { get; set; }
    }
}
