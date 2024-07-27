#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Linq;
using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Drawers.Deferred;
using Engine.Common;

namespace Engine
{
    /// <summary>
    /// Deferred renderer class
    /// </summary>
    public class SceneRendererDeferred : BaseSceneRenderer
    {
        /// <summary>
        /// Deferred enable objects pass index
        /// </summary>
        private const int ObjectsDeferredPass = NextPass;
        /// <summary>
        /// Forward objects pass index
        /// </summary>
        private const int ObjectsForwardPass = NextPass + 1;
        /// <summary>
        /// Objects post processing pass index
        /// </summary>
        private const int ObjectsPostProcessingPass = NextPass + 2;
        /// <summary>
        /// UI pass index
        /// </summary>
        private const int UIPass = NextPass + 3;
        /// <summary>
        /// UI post processing pass index
        /// </summary>
        private const int UIPostProcessingPass = NextPass + 4;

#if DEBUG

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

                if (FrameCounters.GetStatistics("DEFERRED_COMPOSITION") is long[] deferredCompositionCounters)
                {
                    DeferredComposeInit = deferredCompositionCounters[0];
                    DeferredComposeDraw = deferredCompositionCounters[1];
                }

                if (FrameCounters.GetStatistics("DEFERRED_LIGHTING") is long[] deferredCounters)
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

                    FrameCounters.SetStatistics("Scene.Draw.totalShadowMap", string.Format(
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

                    FrameCounters.SetStatistics("Scene.Draw.totalDeferred", string.Format(
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

                        FrameCounters.SetStatistics("Scene.Draw.deferred_gbuffer PRC", string.Format(
                            "GBuffer = {0:000000}; Init {1:00}%; Draw {2:00}%; Resolve {3:00}%",
                            DeferredGbuffer,
                            prcPass1 * 100f,
                            prcPass2 * 100f,
                            prcPass3 * 100f));

                        FrameCounters.SetStatistics("Scene.Draw.deferred_gbuffer CNT", string.Format(
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

                        FrameCounters.SetStatistics("Scene.Draw.deferred_lbuffer PRC", string.Format(
                            "LBuffer = {0:000000}; Init {1:00}%; Directionals {2:00}%; Points {3:00}%; Spots {4:00}%",
                            DeferredLbuffer,
                            prcPass1 * 100f,
                            prcPass2 * 100f,
                            prcPass3 * 100f,
                            prcPass4 * 100f));

                        FrameCounters.SetStatistics("Scene.Draw.deferred_lbuffer CNT", string.Format(
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

                        FrameCounters.SetStatistics("Scene.Draw.deferred_compose PRC", string.Format(
                            "Compose = {0:000000}; Init {1:00}%; Draw {2:00}%",
                            DeferredCompose,
                            prcPass1 * 100f,
                            prcPass2 * 100f));

                        FrameCounters.SetStatistics("Scene.Draw.deferred_compose CNT", string.Format(
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

                FrameCounters.SetStatistics("Scene.Draw", string.Format(
                    "TOTAL = {0:000000}; Shadows {1:00}%; Deferred {2:00}%; Other {3:00}%;",
                    Total,
                    prcSM * 100f,
                    prcDR * 100f,
                    prcOther * 100f));
            }
        }
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

                    FrameCounters.SetStatistics("DeferredRenderer.DrawLights", string.Format(
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

                FrameCounters.SetStatistics("DeferredRenderer.DrawLights.Types", string.Format(
                    "Directional {0:000000}; Point {1:000000}; Spot {2:000000}",
                    perDirectionalLight,
                    perPointLight,
                    perSpotLight));

                FrameCounters.SetStatistics("DEFERRED_LIGHTING", new[]
                {
                Prepare,
                Directional,
                Point,
                Spot,
            });
            }
        }

        /// <summary>
        /// Frame statistics
        /// </summary>
        private readonly FrameStatsDeferred frameStats = new();
        /// <summary>
        /// Frame light statistics
        /// </summary>
        private readonly FrameStatsLight lightStats = new();
#endif

        /// <summary>
        /// Validates the renderer against the current device configuration
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns true if the renderer is valid</returns>
        public static bool Validate(Graphics graphics)
        {
            return graphics != null && !graphics.MultiSampled;
        }

        /// <summary>
        /// Geometry buffer
        /// </summary>
        private RenderTarget geometryBuffer = null;
        /// <summary>
        /// Light buffer
        /// </summary>
        private RenderTarget lightBuffer = null;
        /// <summary>
        /// Light drawer
        /// </summary>
        private SceneRendererDeferredLights lightDrawer = null;
        /// <summary>
        /// Composer
        /// </summary>
        private readonly BuiltInComposer composer;
        /// <summary>
        /// Stencil drawer
        /// </summary>
        private readonly BuiltInStencil stencilDrawer;
        /// <summary>
        /// Directional light drawer
        /// </summary>
        private readonly BuiltInLightDirectional lightDirectionalDrawer;
        /// <summary>
        /// Spot light drawer
        /// </summary>
        private readonly BuiltInLightSpot lightSpotDrawer;
        /// <summary>
        /// Point light drawer
        /// </summary>
        private readonly BuiltInLightPoint lightPointDrawer;

        /// <summary>
        /// Blend state for deferred lighting blending
        /// </summary>
        private EngineBlendState blendDeferredLighting = null;
        /// <summary>
        /// Blend state for defered composer blending
        /// </summary>
        private EngineBlendState blendDeferredComposer = null;
        /// <summary>
        /// Blend state for transparent defered composer blending
        /// </summary>
        private EngineBlendState blendDeferredComposerTransparent = null;
        /// <summary>
        /// Blend state for alpha defered composer blending
        /// </summary>
        private EngineBlendState blendDeferredComposerAlpha = null;
        /// <summary>
        /// Blend state for additive defered composer blending
        /// </summary>
        private EngineBlendState blendDeferredComposerAdditive = null;

        /// <summary>
        /// View port
        /// </summary>
        public Viewport Viewport { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        public SceneRendererDeferred(Scene scene) : base(scene)
        {
            lightDrawer = new SceneRendererDeferredLights(scene.Game.Graphics);

            UpdateRectangleAndView();

            composer = BuiltInShaders.GetDrawer<BuiltInComposer>();
            stencilDrawer = BuiltInShaders.GetDrawer<BuiltInStencil>();
            lightDirectionalDrawer = BuiltInShaders.GetDrawer<BuiltInLightDirectional>();
            lightSpotDrawer = BuiltInShaders.GetDrawer<BuiltInLightSpot>();
            lightPointDrawer = BuiltInShaders.GetDrawer<BuiltInLightPoint>();

            int gbCount = 6;

            geometryBuffer = new RenderTarget(scene.Game, "GeometryBuffer", Format.R32G32B32A32_Float, false, gbCount);
            lightBuffer = new RenderTarget(scene.Game, "LightBuffer", Format.R32G32B32A32_Float, false, 1);

            blendDeferredComposer = EngineBlendState.DeferredComposer(scene.Game.Graphics, gbCount);
            blendDeferredComposerTransparent = EngineBlendState.DeferredComposerTransparent(scene.Game.Graphics, gbCount);
            blendDeferredComposerAlpha = EngineBlendState.DeferredComposerAlpha(scene.Game.Graphics, gbCount);
            blendDeferredComposerAdditive = EngineBlendState.DeferredComposerAdditive(scene.Game.Graphics, gbCount);
            blendDeferredLighting = EngineBlendState.DeferredLighting(scene.Game.Graphics);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SceneRendererDeferred()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                geometryBuffer.Dispose();
                geometryBuffer = null;
                lightBuffer.Dispose();
                lightBuffer = null;
                lightDrawer?.Dispose();
                lightDrawer = null;

                blendDeferredComposer?.Dispose();
                blendDeferredComposer = null;
                blendDeferredComposerTransparent?.Dispose();
                blendDeferredComposerTransparent = null;
                blendDeferredComposerAlpha?.Dispose();
                blendDeferredComposerAlpha = null;
                blendDeferredComposerAdditive?.Dispose();
                blendDeferredComposerAdditive = null;
                blendDeferredLighting?.Dispose();
                blendDeferredLighting = null;
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override void Resize()
        {
            UpdateRectangleAndView();

            geometryBuffer.Resize();

            lightBuffer.Resize();

            base.Resize();
        }
        /// <inheritdoc/>
        public override EngineShaderResourceView GetResource(SceneRendererResults result)
        {
            if (result == SceneRendererResults.LightMap) return lightBuffer.Texture;

            var colorMap = geometryBuffer.Textures.ElementAtOrDefault(0);
            var normalMap = geometryBuffer.Textures.ElementAtOrDefault(1);
            var depthMap = geometryBuffer.Textures.ElementAtOrDefault(2);

            if (result == SceneRendererResults.ColorMap) return colorMap;
            if (result == SceneRendererResults.NormalMap) return normalMap;
            if (result == SceneRendererResults.DepthMap) return depthMap;

            return base.GetResource(result);
        }

        /// <inheritdoc/>
        public override void PrepareScene()
        {
            base.PrepareScene();

            //Create commandList
            AddPassContext(ObjectsDeferredPass, "Objects Deferred");
            AddPassContext(ObjectsForwardPass, "Objects Forward");
            AddPassContext(ObjectsPostProcessingPass, "Objects Post processing");

            AddPassContext(UIPass, "UI");
            AddPassContext(UIPostProcessingPass, "UI Post processing");
        }

        /// <inheritdoc/>
        public override void Draw(IGameTime gameTime)
        {
            if (!Updated)
            {
                return;
            }

            Updated = false;

            //Select visible components
            var visibleComponents = Scene.Components.Get<IDrawable>(c => c.Visible);
            if (!visibleComponents.Any())
            {
                return;
            }

#if DEBUG
            frameStats.Clear();
            var swTotal = Stopwatch.StartNew();
#endif
            //Initializes the scene graphics state
            InitializeScene();

            //Shadow mapping
            QueueAction(DoShadowMapping);

            //Draw objects
            bool hasObjects = DrawObjects(
                visibleComponents.Where(c => c.DeferredEnabled && !c.Usage.HasFlag(SceneObjectUsages.UI)),
                visibleComponents.Where(c => !c.DeferredEnabled && !c.Usage.HasFlag(SceneObjectUsages.UI)));

            //Draw UI (ignoring DeferredEnabled flag)
            bool hasUI = DrawUI(visibleComponents.Where(c => c.Usage.HasFlag(SceneObjectUsages.UI)));

            //Merge to result
            QueueAction(() => MergeSceneToScreen(hasObjects, hasUI));

            EndScene();

#if DEBUG
            swTotal.Stop();

            frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
        }
        /// <summary>
        /// Draws the specified object collections
        /// </summary>
        /// <param name="deferredEnabledComponents">Deferred enabled components</param>
        /// <param name="deferredDisabledComponents">Deferred disabled components</param>
        private bool DrawObjects(IEnumerable<IDrawable> deferredEnabledComponents, IEnumerable<IDrawable> deferredDisabledComponents)
        {
            bool anyDeferred = deferredEnabledComponents.Any();
            bool anyForward = deferredDisabledComponents.Any();
            bool hasObjects = anyForward || anyDeferred;

            if (!hasObjects)
            {
                return false;
            }

            if (anyDeferred)
            {
                QueueAction(() =>
                {
                    //Render to G-Buffer deferred enabled components
                    var rt = new RenderTargetParameters
                    {
                        Target = Targets.Objects,
                        ClearRT = true,
                        ClearRTColor = Scene.GameEnvironment.Background,
                    };
                    DoDeferred(rt, deferredEnabledComponents, CullObjects, ObjectsDeferredPass);
                });
            }

            if (anyForward)
            {
                QueueAction(() =>
                {
                    //Render to screen deferred disabled components
                    var rt = new RenderTargetParameters
                    {
                        Target = Targets.Objects
                    };
                    DoForward(rt, deferredDisabledComponents, CullObjects, ObjectsForwardPass);
                });
            }

            QueueAction(() =>
            {
                //Post-processing
                var rtpp = new RenderTargetParameters
                {
                    Target = Targets.Objects
                };
                DoPostProcessing(rtpp, RenderPass.Objects, ObjectsPostProcessingPass);
            });

            return hasObjects;
        }
        /// <summary>
        /// Draws the specified UI components collection
        /// </summary>
        /// <param name="uiComponents">UI components collection</param>
        private bool DrawUI(IEnumerable<IDrawable> uiComponents)
        {
            if (!uiComponents.Any())
            {
                return false;
            }

            QueueAction(() =>
            {
                //Render UI
                var rt = new RenderTargetParameters
                {
                    Target = Targets.UI,
                    ClearRT = true,
                    ClearRTColor = Color.Transparent,
                };
                DoForward(rt, uiComponents, CullUI, UIPass);
            });

            QueueAction(() =>
            {
                //UI post-processing
                var rtpp = new RenderTargetParameters
                {
                    Target = Targets.UI
                };
                DoPostProcessing(rtpp, RenderPass.UI, UIPostProcessingPass);
            });

            return true;
        }
        /// <summary>
        /// Do deferred rendering
        /// </summary>
        /// <param name="renderTarget">Render target</param>
        /// <param name="components">Components</param>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="passIndex">Pass index</param>
        private void DoDeferred(RenderTargetParameters renderTarget, IEnumerable<IDrawable> components, int cullIndex, int passIndex)
        {
            if (!components.Any())
            {
                return;
            }

#if DEBUG
            var swCull = Stopwatch.StartNew();
#endif
            var context = GetDeferredDrawContext(passIndex, "Deferred", DrawerModes.Deferred);
            bool draw = CullingTest(Scene, context.Camera.Frustum, components, cullIndex);
#if DEBUG
            swCull.Stop();
            frameStats.DeferredCull = swCull.ElapsedTicks;
#endif

            if (!draw)
            {
                return;
            }

            var dc = context.DeviceContext;

#if DEBUG
            var swGeometryBuffer = Stopwatch.StartNew();
            var swGeometryBufferInit = Stopwatch.StartNew();
#endif
            if (!Scene.Game.SetVertexBuffers(dc))
            {
                return;
            }

            BindGBuffer(dc);
#if DEBUG
            swGeometryBufferInit.Stop();
            var swGeometryBufferDraw = Stopwatch.StartNew();
#endif
            //Draw scene on g-buffer render targets
            DrawResultComponents(context, cullIndex, components);
#if DEBUG
            swGeometryBufferDraw.Stop();
            swGeometryBuffer.Stop();

            frameStats.DeferredGbuffer = swGeometryBuffer.ElapsedTicks;
            frameStats.DeferredGbufferInit = swGeometryBufferInit.ElapsedTicks;
            frameStats.DeferredGbufferDraw = swGeometryBufferDraw.ElapsedTicks;
#endif

#if DEBUG
            var swLightBuffer = Stopwatch.StartNew();
#endif
            BindLights(dc);

            //Draw scene lights on light buffer using g-buffer output
            DrawLights(dc, context.Lights);
#if DEBUG
            swLightBuffer.Stop();

            frameStats.DeferredLbuffer = swLightBuffer.ElapsedTicks;
#endif

            //Binds the result target
            SetTarget(dc, renderTarget);

            #region Final composition
#if DEBUG
            Stopwatch swComponsition = Stopwatch.StartNew();
#endif
            //Draw scene result on screen using g-buffer and light buffer
            DrawResult(dc);

#if DEBUG
            swComponsition.Stop();

            frameStats.DeferredCompose = swComponsition.ElapsedTicks;
#endif
            #endregion

            QueueCommand(dc.FinishCommandList($"{nameof(DoDeferred)} {renderTarget.Target}"), passIndex);
        }
        /// <summary>
        /// Do forward rendering (UI, transparents, etc.)
        /// </summary>
        /// <param name="renderTarget">Render target</param>
        /// <param name="components">Components</param>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="passIndex">Pass index</param>
        private void DoForward(RenderTargetParameters renderTarget, IEnumerable<IDrawable> components, int cullIndex, int passIndex)
        {
            if (!components.Any())
            {
                return;
            }

#if DEBUG
            var swCull = Stopwatch.StartNew();
#endif
            var context = GetDeferredDrawContext(passIndex, "Forward", DrawerModes.Forward);
            bool draw = CullingTest(Scene, context.Camera.Frustum, components, cullIndex);
#if DEBUG
            swCull.Stop();
            frameStats.DisabledDeferredCull = swCull.ElapsedTicks;
#endif

            if (!draw)
            {
                return;
            }

            var dc = context.DeviceContext;

#if DEBUG
            var swDraw = Stopwatch.StartNew();
#endif
            if (!Scene.Game.SetVertexBuffers(dc))
            {
                return;
            }

            //Binds the result target
            SetTarget(dc, renderTarget);

            //Draw scene
            DrawResultComponents(context, cullIndex, components);

#if DEBUG
            swDraw.Stop();
            frameStats.DisabledDeferredDraw = swDraw.ElapsedTicks;
#endif

            QueueCommand(dc.FinishCommandList($"{nameof(DoForward)} {renderTarget.Target}"), passIndex);
        }

        /// <summary>
        /// Updates renderer parameters
        /// </summary>
        private void UpdateRectangleAndView()
        {
            Width = Scene.Game.Form.RenderWidth;
            Height = Scene.Game.Form.RenderHeight;

            Viewport = Scene.Game.Form.GetViewport();

            lightDrawer.Update(Width, Height);
        }
        /// <summary>
        /// Binds graphics for g-buffer pass
        /// </summary>
        /// <param name="dc">Device context</param>
        private void BindGBuffer(IEngineDeviceContext dc)
        {
            //Set local viewport
            dc.SetViewport(Viewport);

            //Set g-buffer render targets
            dc.SetRenderTargets(
                geometryBuffer.Targets, true, Color.Transparent,
                Scene.Game.Graphics.DefaultDepthStencil, true, true,
                true);
        }
        /// <summary>
        /// Binds graphics for light acummulation pass
        /// </summary>
        /// <param name="dc">Device context</param>
        private void BindLights(IEngineDeviceContext dc)
        {
            //Set local viewport
            dc.SetViewport(Viewport);

            //Set light buffer to draw lights
            dc.SetRenderTargets(
                lightBuffer.Targets, true, Color.Transparent,
                false);
        }

        /// <summary>
        /// Draw lights
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="lights">Light collection</param>
        private void DrawLights(IEngineDeviceContext dc, SceneLights lights)
        {
#if DEBUG
            lightStats.Clear();

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            var graphics = Scene.Game.Graphics;

            #region Initialization
#if DEBUG
            Stopwatch swPrepare = Stopwatch.StartNew();
#endif
            lightDrawer.WriteBuffers(dc);

            var directionalLights = lights.GetVisibleDirectionalLights();
            var spotLights = lights.GetVisibleSpotLights();
            var pointLights = lights.GetVisiblePointLights();

            dc.SetDepthStencilState(graphics.GetDepthStencilRDZDisabled());
            dc.SetBlendState(blendDeferredLighting);

#if DEBUG
            swPrepare.Stop();
#endif
            #endregion

            #region Directional Lights
#if DEBUG
            Stopwatch swDirectional = Stopwatch.StartNew();
#endif
            if (directionalLights.Any())
            {
                lightDrawer.BindGlobalLight(dc);

                lightDirectionalDrawer.UpdateGeometryMap(geometryBuffer.Textures);

                foreach (var light in directionalLights)
                {
                    lightDirectionalDrawer.UpdatePerLight(dc, light);

                    lightDrawer.DrawDirectional(dc, lightDirectionalDrawer);
                }
            }
#if DEBUG
            swDirectional.Stop();
#endif
            #endregion

            #region Point Lights
#if DEBUG
            Stopwatch swPoint = Stopwatch.StartNew();
#endif
            if (pointLights.Any())
            {
                lightDrawer.BindPoint(dc);

                lightPointDrawer.UpdateGeometryMap(geometryBuffer.Textures);

                foreach (var light in pointLights)
                {
                    if (light.ShadowMapIndex < 0)
                    {
                        continue;
                    }

                    //Draw Pass
                    lightPointDrawer.UpdatePerLight(dc, light);

                    lightDrawer.DrawPoint(dc, stencilDrawer, lightPointDrawer);
                }
            }
#if DEBUG
            swPoint.Stop();
#endif
            #endregion

            #region Spot Lights
#if DEBUG
            Stopwatch swSpot = Stopwatch.StartNew();
#endif
            if (spotLights.Any())
            {
                lightDrawer.BindSpot(dc);

                lightSpotDrawer.UpdateGeometryMap(geometryBuffer.Textures);

                foreach (var light in spotLights)
                {
                    //Draw Pass
                    lightSpotDrawer.UpdatePerLight(dc, light);

                    lightDrawer.DrawSpot(dc, stencilDrawer, lightSpotDrawer);
                }
            }
#if DEBUG
            swSpot.Stop();
#endif
            #endregion
#if DEBUG
            swTotal.Stop();
#endif
#if DEBUG
            lightStats.Prepare = swPrepare.ElapsedTicks;
            lightStats.Directional = swDirectional.ElapsedTicks;
            lightStats.Point = swPoint.ElapsedTicks;
            lightStats.Spot = swSpot.ElapsedTicks;
            lightStats.DirectionalLights = directionalLights?.Count() ?? 0;
            lightStats.PointLights = pointLights?.Count() ?? 0;
            lightStats.SpotLights = spotLights?.Count() ?? 0;

            lightStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
        }
        /// <summary>
        /// Draw result
        /// </summary>
        /// <param name="dc">Device context</param>
        private void DrawResult(IEngineDeviceContext dc)
        {
            var graphics = Scene.Game.Graphics;

#if DEBUG
            long init = 0;
            long draw = 0;

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            if (geometryBuffer.Textures != null && lightBuffer.Texture != null)
            {
#if DEBUG
                Stopwatch swInit = Stopwatch.StartNew();
#endif
                composer.UpdateGeometryMap(geometryBuffer.Textures, lightBuffer.Texture);

                lightDrawer.BindResult(dc);

                dc.SetDepthStencilState(graphics.GetDepthStencilNone());
                dc.SetRasterizerState(graphics.GetRasterizerDefault());
                dc.SetBlendState(graphics.GetBlendDefault());
#if DEBUG
                swInit.Stop();

                init = swInit.ElapsedTicks;
#endif
#if DEBUG
                var swDraw = Stopwatch.StartNew();
#endif
                lightDrawer.DrawResult(dc, composer);
#if DEBUG
                swDraw.Stop();

                draw = swDraw.ElapsedTicks;
#endif
            }
#if DEBUG
            swTotal.Stop();
#endif
#if DEBUG
            FrameCounters.SetStatistics("DEFERRED_COMPOSITION", new[]
            {
                init,
                draw,
            });
#endif
        }
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(DrawContext context, int cullIndex, IEnumerable<IDrawable> components)
        {
            //Save current drawing mode
            var mode = context.DrawerMode;

            //First opaques
            var opaques = GetOpaques(cullIndex, components);
            if (opaques.Count != 0)
            {
                //Set opaques draw mode
                context.DrawerMode = mode | DrawerModes.OpaqueOnly;

                //Sort items (nearest first)
                opaques.Sort((c1, c2) => SortOpaques(cullIndex, c1, c2));

                //Draw items
                opaques.ForEach((c) => Draw(context, c));
            }

            //Then transparents
            var transparents = GetTransparents(cullIndex, components);
            if (transparents.Count != 0)
            {
                //Set transparents draw mode
                context.DrawerMode = mode | DrawerModes.TransparentOnly;

                //Sort items (far first)
                transparents.Sort((c1, c2) => SortTransparents(cullIndex, c1, c2));

                //Draw items
                transparents.ForEach((c) => Draw(context, c));
            }

            //Restore drawing mode
            context.DrawerMode = mode;
        }

        /// <inheritdoc/>
        protected override void SetBlendState(IEngineDeviceContext dc, DrawerModes drawerMode, BlendModes blendMode)
        {
            if (!drawerMode.HasFlag(DrawerModes.Deferred))
            {
                base.SetBlendState(dc, drawerMode, blendMode);
            }

            if (blendMode.HasFlag(BlendModes.Additive))
            {
                dc.SetBlendState(blendDeferredComposerAdditive);
            }
            else if (blendMode.HasFlag(BlendModes.Transparent) || blendMode.HasFlag(BlendModes.Alpha))
            {
                dc.SetBlendState(blendDeferredComposerTransparent);
            }
            else
            {
                dc.SetBlendState(blendDeferredComposer);
            }
        }
    }
}
