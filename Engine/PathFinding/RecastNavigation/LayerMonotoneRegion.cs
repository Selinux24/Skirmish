
namespace Engine.PathFinding.RecastNavigation
{
    public struct LayerMonotoneRegion
    {
        public const int DT_LAYER_MAX_NEIS = 16;

        public static LayerMonotoneRegion CreateEmpty()
        {
            return new LayerMonotoneRegion()
            {
                area = 0,
                neis = new int[DT_LAYER_MAX_NEIS],
                nneis = 0,
                regId = 0,
                areaId = TileCacheAreas.RC_NULL_AREA,
            };
        }

        public int area;
        public int[] neis;
        public int nneis;
        public int regId;
        public TileCacheAreas areaId;
    }
}
