
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Layer sweep span
    /// </summary>
    struct LayerSweepSpan
    {
        /// <summary>
        /// Null index
        /// </summary>
        const int RC_NULL_NEI = 0xff;

        /// <summary>
        /// Number of samples
        /// </summary>
        public int SampleCount { get; set; }
        /// <summary>
        /// Region id
        /// </summary>
        public int RegId { get; set; }
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
        public static bool CreateUniqueIds(LayerSweepSpan[] sweeps, int sweepCount, int[] samples, ref int nRegions)
        {
            // Create unique ID.
            for (int i = 0; i < sweepCount; ++i)
            {
                // If the neighbour is set and there is only one continuous connection to it,
                // the sweep will be merged with the previous one, else new region is created.
                if (sweeps[i].NeiRegId != RC_NULL_NEI && samples[sweeps[i].NeiRegId] == sweeps[i].SampleCount)
                {
                    sweeps[i].RegId = sweeps[i].NeiRegId;

                    continue;
                }

                if (nRegions == RC_NULL_NEI)
                {
                    // Region ID's overflow.
                    return false;
                }

                sweeps[i].RegId = nRegions++;
            }

            return true;
        }
        /// <summary>
        /// Updates the region id
        /// </summary>
        /// <param name="regId">Region id</param>
        /// <param name="samples">Samples list</param>
        public void Update(int regId, int[] samples)
        {
            // Set neighbour when first valid neighbour is encountered.
            if (SampleCount == 0)
            {
                NeiRegId = regId;
            }

            if (NeiRegId == regId)
            {
                // Update existing neighbour
                SampleCount++;
                samples[regId]++;
            }
            else
            {
                // This is hit if there is nore than one neighbour.
                // Invalidate the neighbour.
                NeiRegId = RC_NULL_NEI;
            }
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            if (SampleCount != 0 || RegId != 0 || NeiRegId != 0)
            {
                return $"Samples: {SampleCount}; Region Id: {RegId}; Neighbor Id: {NeiRegId};";
            }
            else
            {
                return "Empty";
            }
        }
    }
}