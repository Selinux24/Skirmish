namespace Engine
{
    /// <summary>
    /// Deferred renderer frame stats
    /// </summary>
    class FrameStatsDeferred
    {
        public long Total { get; private set; } = 0;
        public long ShadowMapStart { get; set; } = 0;
        public long ShadowMapCull { get; set; } = 0;
        public long ShadowMapDraw { get; set; } = 0;
        public long DeferredCull { get; set; } = 0;
        public long DeferredGbuffer { get; set; } = 0;
        public long DeferredGbufferInit { get; set; } = 0;
        public long DeferredGbufferDraw { get; set; } = 0;
        public long DeferredGbufferResolve { get; set; } = 0;
        public long DeferredLbuffer { get; set; } = 0;
        public long DeferredLbufferInit { get; set; } = 0;
        public long DeferredLbufferDir { get; set; } = 0;
        public long DeferredLbufferPoi { get; set; } = 0;
        public long DeferredLbufferSpo { get; set; } = 0;
        public long DeferredCompose { get; set; } = 0;
        public long DeferredComposeInit { get; set; } = 0;
        public long DeferredComposeDraw { get; set; } = 0;
        public long DisabledDeferredCull { get; set; } = 0;
        public long DisabledDeferredDraw { get; set; } = 0;

        /// <summary>
        /// Clear frame
        /// </summary>
        public void Clear()
        {
            Total = 0;
            ShadowMapStart = 0;
            ShadowMapCull = 0;
            ShadowMapDraw = 0;
            DeferredCull = 0;
            DeferredGbuffer = 0;
            DeferredGbufferInit = 0;
            DeferredGbufferDraw = 0;
            DeferredGbufferResolve = 0;
            DeferredLbuffer = 0;
            DeferredLbufferInit = 0;
            DeferredLbufferDir = 0;
            DeferredLbufferPoi = 0;
            DeferredLbufferSpo = 0;
            DeferredCompose = 0;
            DeferredComposeInit = 0;
            DeferredComposeDraw = 0;
            DisabledDeferredCull = 0;
            DisabledDeferredDraw = 0;
        }
        /// <summary>
        /// Update stats into counter
        /// </summary>
        /// <param name="elapsedTicks">Elapsed ticks</param>
        public void UpdateCounters(long elapsedTicks)
        {
            Total = elapsedTicks;

            if (Counters.GetStatistics("DEFERRED_COMPOSITION") is long[] deferredCompositionCounters)
            {
                DeferredComposeInit = deferredCompositionCounters[0];
                DeferredComposeDraw = deferredCompositionCounters[1];
            }

            if (Counters.GetStatistics("DEFERRED_LIGHTING") is long[] deferredCounters)
            {
                DeferredLbufferInit = deferredCounters[0];
                DeferredLbufferDir = deferredCounters[1];
                DeferredLbufferPoi = deferredCounters[2];
                DeferredLbufferSpo = deferredCounters[3];
            }

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

            long totalDeferred = DeferredCull + DeferredGbuffer + DeferredLbuffer + DeferredCompose + DisabledDeferredCull + DisabledDeferredDraw;
            if (totalDeferred > 0)
            {
                float prcCull = (float)DeferredCull / (float)totalDeferred;
                float prcDCull = (float)DisabledDeferredCull / (float)totalDeferred;
                float prcGBuffer = (float)DeferredGbuffer / (float)totalDeferred;
                float prcLBuffer = (float)DeferredLbuffer / (float)totalDeferred;
                float prcCompose = (float)DeferredCompose / (float)totalDeferred;
                float prcDraw = (float)DisabledDeferredDraw / (float)totalDeferred;

                Counters.SetStatistics("Scene.Draw.totalDeferred", string.Format(
                    "DR = {0:000000}; Cull {1:00}%; GBuffer {2:00}%; LBuffer {3:00}%; Compose {4:00}%; DeferredDisabledCull {5:00}%; DeferredDisabledDraw {6:00}%",
                    totalDeferred,
                    prcCull * 100f,
                    prcGBuffer * 100f,
                    prcLBuffer * 100f,
                    prcCompose * 100f,
                    prcDCull * 100f,
                    prcDraw * 100f));

                if (DeferredGbuffer > 0)
                {
                    float prcPass1 = (float)DeferredGbufferInit / (float)DeferredGbuffer;
                    float prcPass2 = (float)DeferredGbufferDraw / (float)DeferredGbuffer;
                    float prcPass3 = (float)DeferredGbufferResolve / (float)DeferredGbuffer;

                    Counters.SetStatistics("Scene.Draw.deferred_gbuffer PRC", string.Format(
                        "GBuffer = {0:000000}; Init {1:00}%; Draw {2:00}%; Resolve {3:00}%",
                        DeferredGbuffer,
                        prcPass1 * 100f,
                        prcPass2 * 100f,
                        prcPass3 * 100f));

                    Counters.SetStatistics("Scene.Draw.deferred_gbuffer CNT", string.Format(
                        "GBuffer = {0:000000}; Init {1:000000}; Draw {2:000000}; Resolve {3:000000}",
                        DeferredGbuffer,
                        DeferredGbufferInit,
                        DeferredGbufferDraw,
                        DeferredGbufferResolve));
                }

                if (DeferredLbuffer > 0)
                {
                    float prcPass1 = (float)DeferredLbufferInit / (float)DeferredLbuffer;
                    float prcPass2 = (float)DeferredLbufferDir / (float)DeferredLbuffer;
                    float prcPass3 = (float)DeferredLbufferPoi / (float)DeferredLbuffer;
                    float prcPass4 = (float)DeferredLbufferSpo / (float)DeferredLbuffer;

                    Counters.SetStatistics("Scene.Draw.deferred_lbuffer PRC", string.Format(
                        "LBuffer = {0:000000}; Init {1:00}%; Directionals {2:00}%; Points {3:00}%; Spots {4:00}%",
                        DeferredLbuffer,
                        prcPass1 * 100f,
                        prcPass2 * 100f,
                        prcPass3 * 100f,
                        prcPass4 * 100f));

                    Counters.SetStatistics("Scene.Draw.deferred_lbuffer CNT", string.Format(
                        "LBuffer = {0:000000}; Init {1:000000}; Directionals {2:000000}; Points {3:000000}; Spots {4:000000}",
                        DeferredLbuffer,
                        DeferredLbufferInit,
                        DeferredLbufferDir,
                        DeferredLbufferPoi,
                        DeferredLbufferSpo));
                }

                if (DeferredCompose > 0)
                {
                    float prcPass1 = (float)DeferredComposeInit / (float)DeferredCompose;
                    float prcPass2 = (float)DeferredComposeDraw / (float)DeferredCompose;

                    Counters.SetStatistics("Scene.Draw.deferred_compose PRC", string.Format(
                        "Compose = {0:000000}; Init {1:00}%; Draw {2:00}%",
                        DeferredCompose,
                        prcPass1 * 100f,
                        prcPass2 * 100f));

                    Counters.SetStatistics("Scene.Draw.deferred_compose CNT", string.Format(
                        "Compose = {0:000000}; Init {1:000000}; Draw {2:000000}",
                        DeferredCompose,
                        DeferredComposeInit,
                        DeferredComposeDraw));
                }
            }

            long other = Total - (totalShadowMap + totalDeferred);

            float prcSM = (float)totalShadowMap / (float)Total;
            float prcDR = (float)totalDeferred / (float)Total;
            float prcOther = (float)other / (float)Total;

            Counters.SetStatistics("Scene.Draw", string.Format(
                "TOTAL = {0:000000}; Shadows {1:00}%; Deferred {2:00}%; Other {3:00}%;",
                Total,
                prcSM * 100f,
                prcDR * 100f,
                prcOther * 100f));
        }
    }
}
