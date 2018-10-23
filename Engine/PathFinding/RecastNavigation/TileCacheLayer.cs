
namespace Engine.PathFinding.RecastNavigation
{
    public struct TileCacheLayer
    {
        public TileCacheLayerHeader Header { get; set; }
        /// <summary>
        /// Region count.
        /// </summary>
        public int RegCount { get; set; }
        public int[] Heights { get; set; }
        public TileCacheAreas[] areas { get; set; }
        public int[] cons { get; set; }
        public int[] regs { get; set; }
    }
}
