
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
        public TileCacheAreas[] Areas { get; set; }
        public int[] Cons { get; set; }
        public int[] Regs { get; set; }
    }
}
