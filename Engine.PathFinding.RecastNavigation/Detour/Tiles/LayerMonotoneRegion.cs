
namespace Engine.PathFinding.RecastNavigation.Detour.Tiles
{
    /// <summary>
    /// Layer monotone region
    /// </summary>
    public struct LayerMonotoneRegion
    {
        /// <summary>
        /// Maximum neighbours
        /// </summary>
        const int MAX_NEIS = 16;
        /// <summary>
        /// Null id value
        /// </summary>
        public const int NULL_ID = 0xff;

        /// <summary>
        /// Neighbour list
        /// </summary>
        private readonly int[] neis;
        /// <summary>
        /// Number of Neighbours
        /// </summary>
        private int nneis;

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
        /// Constructor
        /// </summary>
        public LayerMonotoneRegion()
        {
            AreaId = 0;
            neis = new int[MAX_NEIS];
            nneis = 0;
            RegId = NULL_ID;
            Area = AreaTypes.RC_NULL_AREA;
        }

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

                int merge = FindMergeValue(reg, regs, nregs);
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
        /// Finds the merge index
        /// </summary>
        /// <param name="reg">Region</param>
        /// <param name="regs">Region list</param>
        /// <param name="nregs">Number of regions</param>
        private static int FindMergeValue(LayerMonotoneRegion reg, LayerMonotoneRegion[] regs, int nregs)
        {
            int merge = -1;
            int mergea = 0;
            for (int j = 0; j < reg.nneis; ++j)
            {
                int nei = reg.neis[j];
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

            return merge;
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
                int nnei = reg.nneis;
                for (int j = 0; j < nnei; ++j)
                {
                    if (regs[reg.neis[j]].RegId == newRegId)
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
            int n = nneis;
            if (n > 0 && neis[n - 1] == v)
            {
                return;
            }
            neis[nneis++] = v;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"AreaId: {AreaId}; Area: {Area}; RegionId: {RegId}; Neighbors: {neis?.Join(",") ?? "empty"}";
        }
    }
}
