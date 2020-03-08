
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public struct LayerMonotoneRegion
    {
        public int Area { get; set; }
        public int[] Neis { get; set; }
        public int NNeis { get; set; }
        public int RegId { get; set; }
        public AreaTypes AreaId { get; set; }

        public override string ToString()
        {
            return string.Format("Area: {0}; AreaId: {1}; RegionId: {2}; Neighbors: {3}", Area, AreaId, RegId, NNeis);
        }
    }
}
