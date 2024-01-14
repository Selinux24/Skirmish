
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Sweep span
    /// </summary>
    struct SweepSpan
    {
        /// <summary>
        /// Null index
        /// </summary>
        const int RC_NULL_NEI = -1;

        /// <summary>
        /// Empty instance
        /// </summary>
        public static SweepSpan Empty
        {
            get
            {
                return new()
                {
                    RegId = 0,
                    SampleCount = 0,
                    NeiRegId = 0,
                };
            }
        }

        /// <summary>
        /// Region id
        /// </summary>
        public int RegId { get; set; }
        /// <summary>
        /// Number of samples
        /// </summary>
        public int SampleCount { get; set; }
        /// <summary>
        /// Neighbour region id
        /// </summary>
        public int NeiRegId { get; set; }

        /// <summary>
        /// Creates an updates the unique id's for the sweeps list
        /// </summary>
        /// <param name="sweeps">Sweeps</param>
        /// <param name="sweepCount">Sweep count</param>
        /// <param name="samples">Samples list</param>
        /// <param name="nRegions">Number of resulting regions</param>
        public static void CreateUniqueIds(SweepSpan[] sweeps, int sweepCount, int[] samples, ref int nRegions)
        {
            // Create unique ID.
            for (int i = 1; i < sweepCount; ++i)
            {
                if (sweeps[i].NeiRegId != RC_NULL_NEI && sweeps[i].NeiRegId != 0 && samples[sweeps[i].NeiRegId] == sweeps[i].SampleCount)
                {
                    sweeps[i].RegId = sweeps[i].NeiRegId;
                }
                else
                {
                    sweeps[i].RegId = nRegions++;
                }
            }
        }
        /// <summary>
        /// Updates the region id
        /// </summary>
        /// <param name="regId">Region id</param>
        /// <param name="samples">Samples list</param>
        public void Update(int regId, int[] samples)
        {
            if (NeiRegId == 0 || NeiRegId == regId)
            {
                NeiRegId = regId;
                SampleCount++;
                samples[regId]++;
            }
            else
            {
                NeiRegId = RC_NULL_NEI;
            }
        }
        /// <summary>
        /// Resets the sweep state
        /// </summary>
        public void Reset()
        {
            var empty = Empty;

            SampleCount = empty.SampleCount;
            NeiRegId = empty.NeiRegId;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            var empty = Empty;

            if (RegId != empty.RegId || SampleCount != empty.SampleCount || NeiRegId != empty.NeiRegId)
            {
                return $"Region Id: {RegId}; Samples: {SampleCount}; Neighbor Id: {NeiRegId};";
            }
            else
            {
                return "Empty";
            }
        }
    }
}
