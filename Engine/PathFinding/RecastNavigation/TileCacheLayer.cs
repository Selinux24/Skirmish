
namespace Engine.PathFinding.RecastNavigation
{
    public struct TileCacheLayer
    {
        public TileCacheLayerHeader header;
        /// <summary>
        /// Region count.
        /// </summary>
        public int regCount;
        public int[] heights;
        public TileCacheAreas[] areas;
        public int[] cons;
        public int[] regs;
    }
}
