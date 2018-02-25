
namespace Engine.PathFinding.NavMesh2
{
    public struct LayerMonotoneRegion
    {
        public const int DT_LAYER_MAX_NEIS = 16;

        public int area;
        public byte[] neis;
        public byte nneis;
        public byte regId;
        public TileCacheAreas areaId;
    }
}
