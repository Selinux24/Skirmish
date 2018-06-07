
namespace Engine.PathFinding.RecastNavigation
{
    public struct LayerMonotoneRegion
    {
        public int area;
        public int[] neis;
        public int nneis;
        public int regId;
        public TileCacheAreas areaId;

        public override string ToString()
        {
            return string.Format("Area: {0}; AreaId: {1}; RegionId: {2}; Neighbors: {3}", area, areaId, regId, nneis);
        }
    }
}
