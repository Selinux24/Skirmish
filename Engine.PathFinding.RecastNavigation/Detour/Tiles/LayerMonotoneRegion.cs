
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    public struct LayerMonotoneRegion
    {
        public int Area { get; set; }
        public int[] Neis { get; set; }
        public int NNeis { get; set; }
        public int RegId { get; set; }
        public AreaTypes AreaId { get; set; }

        public void AddUniqueLast(int v)
        {
            int n = NNeis;
            if (n > 0 && Neis[n - 1] == v)
            {
                return;
            }
            Neis[NNeis] = v;
            NNeis++;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Area: {Area}; AreaId: {AreaId}; RegionId: {RegId}; Neighbors: {NNeis}";
        }
    }
}
