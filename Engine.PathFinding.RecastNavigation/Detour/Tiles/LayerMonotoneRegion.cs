
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Layer monotone region
    /// </summary>
    public struct LayerMonotoneRegion
    {
        /// <summary>
        /// Area
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        /// Area type
        /// </summary>
        public AreaTypes Area { get; set; }
        /// <summary>
        /// Region id
        /// </summary>
        public int RegId { get; set; }
        /// <summary>
        /// Neighbour list
        /// </summary>
        public int[] Neis { get; set; }
        /// <summary>
        /// Number of Neighbours
        /// </summary>
        public int NNeis { get; set; }

        /// <summary>
        /// Initializes region ids
        /// </summary>
        /// <param name="regs">Region list</param>
        /// <param name="nregs">Number of regions</param>
        public static void InitializeIds(LayerMonotoneRegion[] regs, int nregs)
        {
            for (int i = 0; i < nregs; ++i)
            {
                regs[i].RegId = i;
            }
        }
        /// <summary>
        /// Merges the region list
        /// </summary>
        /// <param name="regs">Region list</param>
        /// <param name="nregs">Number of regions</param>
        public static void Merge(LayerMonotoneRegion[] regs, int nregs)
        {
            for (int i = 0; i < nregs; ++i)
            {
                var reg = regs[i];

                int merge = -1;
                int mergea = 0;
                for (int j = 0; j < reg.NNeis; ++j)
                {
                    int nei = reg.Neis[j];
                    var regn = regs[nei];
                    if (reg.RegId == regn.RegId)
                    {
                        continue;
                    }
                    if (reg.Area != regn.Area)
                    {
                        continue;
                    }
                    if (regn.AreaId > mergea && CanMerge(regs, nregs, reg.RegId, regn.RegId))
                    {
                        mergea = regn.AreaId;
                        merge = nei;
                    }
                }

                if (merge == -1)
                {
                    continue;
                }

                int oldId = reg.RegId;
                int newId = regs[merge].RegId;
                for (int j = 0; j < nregs; ++j)
                {
                    if (regs[j].RegId == oldId)
                    {
                        regs[j].RegId = newId;
                    }
                }
            }
        }
        /// <summary>
        /// Gets whether two regions can merge or not
        /// </summary>
        /// <param name="regs">Region list</param>
        /// <param name="nregs">Number of regions in the list</param>
        /// <param name="oldRegId">Old region id</param>
        /// <param name="newRegId">New region id</param>
        private static bool CanMerge(LayerMonotoneRegion[] regs, int nregs, int oldRegId, int newRegId)
        {
            int count = 0;
            for (int i = 0; i < nregs; ++i)
            {
                var reg = regs[i];
                if (reg.RegId != oldRegId)
                {
                    continue;
                }
                int nnei = reg.NNeis;
                for (int j = 0; j < nnei; ++j)
                {
                    if (regs[reg.Neis[j]].RegId == newRegId)
                    {
                        count++;
                    }
                }
            }
            return count == 1;
        }
        /// <summary>
        /// Adds unique last
        /// </summary>
        /// <param name="v">Value</param>
        public void AddUniqueLast(int v)
        {
            int n = NNeis;
            if (n > 0 && Neis[n - 1] == v)
            {
                return;
            }
            Neis[NNeis++] = v;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"AreaId: {AreaId}; Area: {Area}; RegionId: {RegId}; Neighbors: {NNeis}";
        }
    }
}
