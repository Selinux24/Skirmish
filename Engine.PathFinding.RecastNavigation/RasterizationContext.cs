
namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Recast;
    using Engine.PathFinding.RecastNavigation.Detour.Tiles;

    struct RasterizationContext
    {
        public const int MaxLayers = 32;

        public Heightfield Solid;
        public AreaTypes[] TriAreas;
        public HeightfieldLayerSet LayerSet;
        public CompactHeightfield CompactHeightField;
        public TileCacheData[] Tiles;
        public int NTiles;
    }
}
