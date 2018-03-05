
namespace Engine.PathFinding.NavMesh2
{
    public struct LayerMonotoneRegion
    {
        public const int DT_LAYER_MAX_NEIS = 16;

        public static LayerMonotoneRegion CreateEmpty()
        {
            return new LayerMonotoneRegion()
            {
                area = 0,
                neis = new byte[DT_LAYER_MAX_NEIS],
                nneis = 0,
                regId = 0,
                areaId = TileCacheAreas.NullArea,
            };
        }

        public int area;
        public byte[] neis;
        public byte nneis;
        public byte regId;
        public TileCacheAreas areaId;
    }
}
