
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public struct LayerMonotoneRegion
    {
        public int Area { get; set; }
        public int[] Neis { get; set; }
        public int NNeis { get; set; }
        public int RegId { get; set; }
        public AreaTypes AreaId { get; set; }

        public override readonly string ToString()
        {
            return $"Area: {Area}; AreaId: {AreaId}; RegionId: {RegId}; Neighbors: {NNeis}";
        }
    }
}
