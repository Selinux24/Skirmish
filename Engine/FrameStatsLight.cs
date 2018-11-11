
namespace Engine
{
    /// <summary>
    /// Light renderer frame stats
    /// </summary>
    class FrameStatsLight
    {
        public long Total { get; private set; } = 0;
        public long Prepare { get; set; } = 0;
        public long Directional { get; set; } = 0;
        public long Point { get; set; } = 0;
        public long Spot { get; set; } = 0;
        public long Wasted { get; set; } = 0;
        public int DirectionalLights { get; set; } = 0;
        public int PointLights { get; set; } = 0;
        public int SpotLights { get; set; } = 0;

        /// <summary>
        /// Clear frame
        /// </summary>
        public void Clear()
        {
            Total = 0;
            Prepare = 0;
            Directional = 0;
            Point = 0;
            Spot = 0;
            Wasted = 0;
            DirectionalLights = 0;
            PointLights = 0;
            SpotLights = 0;
        }
        /// <summary>
        /// Update stats into counter
        /// </summary>
        /// <param name="elapsedTicks">Elapsed ticks</param>
        public void UpdateCounters(long elapsedTicks)
        {
            Total = elapsedTicks;

            long totalLights = Prepare + Directional + Point + Spot;
            if (totalLights > 0)
            {
                float prcPrepare = (float)Prepare / (float)totalLights;
                float prcDirectional = (float)Directional / (float)totalLights;
                float prcPoint = (float)Point / (float)totalLights;
                float prcSpot = (float)Spot / (float)totalLights;
                float prcWasted = (float)(Total - totalLights) / (float)totalLights;

                Counters.SetStatistics("DeferredRenderer.DrawLights", string.Format(
                    "{0:000000}; Init {1:00}%; Directional {2:00}%; Point {3:00}%; Spot {4:00}%; Other {5:00}%",
                    Total,
                    prcPrepare * 100f,
                    prcDirectional * 100f,
                    prcPoint * 100f,
                    prcSpot * 100f,
                    prcWasted * 100f));
            }

            float perDirectionalLight = 0f;
            float perPointLight = 0f;
            float perSpotLight = 0f;

            if (Directional > 0)
            {
                perDirectionalLight = (float)Directional / (float)DirectionalLights;
            }

            if (Point > 0)
            {
                perPointLight = (float)Point / (float)PointLights;
            }

            if (Spot > 0)
            {
                perSpotLight = (float)Spot / (float)SpotLights;
            }

            Counters.SetStatistics("DeferredRenderer.DrawLights.Types", string.Format(
                "Directional {0:000000}; Point {1:000000}; Spot {2:000000}",
                perDirectionalLight,
                perPointLight,
                perSpotLight));

            Counters.SetStatistics("DEFERRED_LIGHTING", new[]
            {
                Prepare,
                Directional,
                Point,
                Spot,
            });
        }
    }
}
