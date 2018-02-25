
namespace Engine.PathFinding.NavMesh2
{
    public struct TileCacheLayer
    {
        public TileCacheLayerHeader header;
        /// <summary>
        /// Region count.
        /// </summary>
        public byte regCount;
        public byte[] heights;
        public TileCacheAreas[] areas;
        public byte[] cons;
        public byte[] regs;
    }
}
