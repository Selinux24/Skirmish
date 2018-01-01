
namespace Engine.PathFinding.NavMesh2
{
    struct RasterizationContext
    {
        public Heightfield solid;
        public byte[] triareas;
        public HeightfieldLayerSet lset;
        public CompactHeightfield chf;
        public TileCacheData[] tiles;
        public int ntiles;
    }
}
