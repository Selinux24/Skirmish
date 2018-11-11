
namespace Engine
{
    /// <summary>
    /// Frame stats
    /// </summary>
    class FrameStatsForward
    {
        /// <summary>
        /// Total frame ticks
        /// </summary>
        public long Total { get; private set; } = 0;
        /// <summary>
        /// Shadow map start ticks
        /// </summary>
        public long ShadowMapStart { get; set; } = 0;
        /// <summary>
        /// Shadow map cull ticks
        /// </summary>
        public long ShadowMapCull { get; set; } = 0;
        /// <summary>
        /// Shadow map draw ticks
        /// </summary>
        public long ShadowMapDraw { get; set; } = 0;
        /// <summary>
        /// Forward start ticks
        /// </summary>
        public long ForwardStart { get; set; } = 0;
        /// <summary>
        /// Forward cull ticks
        /// </summary>
        public long ForwardCull { get; set; } = 0;
        /// <summary>
        /// Forward draw ticks
        /// </summary>
        public long ForwardDraw { get; set; } = 0;
        /// <summary>
        /// Forward draw 2D ticks
        /// </summary>
        public long ForwardDraw2D { get; set; } = 0;

        /// <summary>
        /// Clear frame
        /// </summary>
        public void Clear()
        {
            Total = 0;
            ShadowMapStart = 0;
            ShadowMapCull = 0;
            ShadowMapDraw = 0;
            ForwardStart = 0;
            ForwardCull = 0;
            ForwardDraw = 0;
            ForwardDraw2D = 0;
        }
        /// <summary>
        /// Update stats into counter
        /// </summary>
        /// <param name="elapsedTicks">Elapsed ticks</param>
        public void UpdateCounters(long elapsedTicks)
        {
            Total = elapsedTicks;

            long totalShadowMap = ShadowMapStart + ShadowMapCull + ShadowMapDraw;
            if (totalShadowMap > 0)
            {
                float prcStart = (float)ShadowMapStart / (float)totalShadowMap;
                float prcCull = (float)ShadowMapCull / (float)totalShadowMap;
                float prcDraw = (float)ShadowMapDraw / (float)totalShadowMap;

                Counters.SetStatistics("Scene.Draw.totalShadowMap", string.Format(
                    "SM = {0:000000}; Start {1:00}%; Cull {2:00}%; Draw {3:00}%",
                    totalShadowMap,
                    prcStart * 100f,
                    prcCull * 100f,
                    prcDraw * 100f));
            }

            long totalForward = ForwardStart + ForwardCull + ForwardDraw + ForwardDraw2D;
            if (totalForward > 0)
            {
                float prcStart = (float)ForwardStart / (float)totalForward;
                float prcCull = (float)ForwardCull / (float)totalForward;
                float prcDraw = (float)ForwardDraw / (float)totalForward;
                float prcDraw2D = (float)ForwardDraw2D / (float)totalForward;

                Counters.SetStatistics("Scene.Draw.totalForward", string.Format(
                    "FR = {0:000000}; Start {1:00}%; Cull {2:00}%; Draw {3:00}%; Draw2D {4:00}%",
                    totalForward,
                    prcStart * 100f,
                    prcCull * 100f,
                    prcDraw * 100f,
                    prcDraw2D * 100f));
            }

            long other = Total - (totalShadowMap + totalForward);

            float prcSM = (float)totalShadowMap / (float)Total;
            float prcFR = (float)totalForward / (float)Total;
            float prcOther = (float)other / (float)Total;

            Counters.SetStatistics("Scene.Draw", string.Format(
                "TOTAL = {0:000000}; Shadows {1:00}%; Forwars {2:00}%; Other {3:00}%;",
                Total,
                prcSM * 100f,
                prcFR * 100f,
                prcOther * 100f));
        }
    }
}
