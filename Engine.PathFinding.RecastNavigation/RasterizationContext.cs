
namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Recast;
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;

    struct RasterizationContext
    {
        public const int MaxLayers = 32;

        public Heightfield solid;
        public AreaTypes[] triareas;
        public HeightfieldLayerSet lset;
        public CompactHeightfield chf;
        public TileCacheData[] tiles;
        public int ntiles;
    }
}
