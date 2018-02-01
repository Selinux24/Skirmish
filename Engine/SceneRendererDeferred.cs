#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using SharpDX.DXGI;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Deferred renderer class
    /// </summary>
    public class SceneRendererDeferred : BaseSceneRenderer
    {
        /// <summary>
        /// Validates the renderer against the current device configuration
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns true if the renderer is valid</returns>
        public static bool Validate(Graphics graphics)
        {
            return !graphics.MultiSampled;
        }

        /// <summary>
        /// View port
        /// </summary>
        public Viewport viewport;
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
        /// View * OrthoProjection Matrix
        /// </summary>
        protected Matrix ViewProjection;
        /// <summary>
        /// Geometry map
        /// </summary>
        protected EngineShaderResourceView[] GeometryMap
        {
            get
            {
                if (this.geometryBuffer != null)
                {
                    return this.geometryBuffer.Textures;
                }

                return null;
            }
        }
        /// <summary>
        /// Light map
        /// </summary>
        protected EngineShaderResourceView[] LightMap
        {
            get
            {
                if (this.lightBuffer != null)
                {
                    return this.lightBuffer.Textures;
                }

                return null;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneRendererDeferred(Game game) : base(game)
        {
            this.lightDrawer = new SceneRendererDeferredLights(game.Graphics);

            this.UpdateRectangleAndView();

            this.geometryBuffer = new RenderTarget(game, Format.R32G32B32A32_Float, false, 3);
            this.lightBuffer = new RenderTarget(game, Format.R32G32B32A32_Float, false, 1);

            this.blendDeferredComposer = EngineBlendState.DeferredComposer(game.Graphics, 3);
            this.blendDeferredComposerTransparent = EngineBlendState.DeferredComposerTransparent(game.Graphics, 3);
            this.blendDeferredLighting = EngineBlendState.DeferredLighting(game.Graphics);
        }

        /// <summary>
        /// Dispose objects
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.geometryBuffer);
            Helper.Dispose(this.lightBuffer);
            Helper.Dispose(this.lightDrawer);

            Helper.Dispose(this.blendDeferredLighting);
            Helper.Dispose(this.blendDeferredComposer);
            Helper.Dispose(this.blendDeferredComposerTransparent);

            base.Dispose();
        }
        /// <summary>
        /// Resizes buffers
        /// </summary>
        public override void Resize()
        {
            this.UpdateRectangleAndView();

            if (this.geometryBuffer != null)
            {
                this.geometryBuffer.Resize();
            }

            if (this.lightBuffer != null)
            {
                this.lightBuffer.Resize();
            }

            base.Resize();
        }
        /// <summary>
        /// Gets renderer resources
        /// </summary>
        /// <param name="result">Resource type</param>
        /// <returns>Returns renderer specified resource, if renderer produces that resource.</returns>
        public override EngineShaderResourceView GetResource(SceneRendererResultEnum result)
        {
            if (result == SceneRendererResultEnum.LightMap) return this.LightMap[0];

            if (this.GeometryMap != null && this.GeometryMap.Length > 0)
            {
                if (result == SceneRendererResultEnum.ColorMap) return this.GeometryMap.Length > 0 ? this.GeometryMap[0] : null;
                if (result == SceneRendererResultEnum.NormalMap) return this.GeometryMap.Length > 1 ? this.GeometryMap[1] : null;
                if (result == SceneRendererResultEnum.DepthMap) return this.GeometryMap.Length > 2 ? this.GeometryMap[2] : null;
            }

            return base.GetResource(result);
        }
        /// <summary>
        /// Draws scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        public override void Draw(GameTime gameTime, Scene scene)
        {
            if (this.Updated)
            {
                this.Updated = false;
#if DEBUG
                long total = 0;
                long start = 0;
                long shadowMap_start = 0;
                long shadowMap_cull = 0;
                long shadowMap_draw = 0;
                long deferred_cull = 0;
                long deferred_gbuffer = 0;
                long deferred_gbufferInit = 0;
                long deferred_gbufferDraw = 0;
                long deferred_gbufferResolve = 0;
                long deferred_lbuffer = 0;
                long deferred_lbufferInit = 0;
                long deferred_lbufferDir = 0;
                long deferred_lbufferPoi = 0;
                long deferred_lbufferSpo = 0;
                long deferred_compose = 0;
                long deferred_composeInit = 0;
                long deferred_composeDraw = 0;
                long disabledDeferred_cull = 0;
                long disabledDeferred_draw = 0;
#endif
#if DEBUG
                Stopwatch swTotal = Stopwatch.StartNew();
#endif
                //Draw visible components
                var visibleComponents = scene.GetComponents(c => c.Visible);
                if (visibleComponents.Count > 0)
                {
                    #region Preparation
#if DEBUG
                    Stopwatch swStartup = Stopwatch.StartNew();
#endif
                    //Initialize context data from update context
                    this.DrawContext.GameTime = gameTime;
                    this.DrawContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawContext.CameraVolume = this.UpdateContext.CameraVolume;
                    this.DrawContext.EyePosition = this.UpdateContext.EyePosition;
                    this.DrawContext.EyeTarget = this.UpdateContext.EyeDirection;
                    //Initialize context data from scene
                    this.DrawContext.Lights = scene.Lights;
                    //Initialize context data from shadow mapping
                    this.DrawContext.ShadowMaps = 0;
                    this.DrawContext.ShadowMapLow = this.ShadowMapperLow;
                    this.DrawContext.ShadowMapHigh = this.ShadowMapperHigh;
                    this.DrawContext.ShadowMapCube = this.ShadowMapperCube;

#if DEBUG
                    swStartup.Stop();

                    start = swStartup.ElapsedTicks;
#endif
                    #endregion

                    #region Shadow mapping

                    ShadowMapFlags flags = ShadowMapFlags.None;

                    flags |= DoShadowMapping(gameTime, scene);

                    flags |= DoCubicShadowMapping(gameTime, scene);

                    //Set shadow map flags to drawing context
                    this.DrawContext.ShadowMaps = flags;

                    #endregion

                    #region Deferred rendering

                    //Render to G-Buffer only opaque objects
                    var deferredEnabledComponents = visibleComponents.FindAll(c => c.DeferredEnabled);
                    if (deferredEnabledComponents.Count > 0)
                    {
                        #region Cull
#if DEBUG
                        Stopwatch swCull = Stopwatch.StartNew();
#endif
                        var toCullDeferred = deferredEnabledComponents.FindAll(s => s.Is<ICullable>()).ConvertAll(s => s.Get<ICullable>());

                        bool draw = false;
                        if (scene.PerformFrustumCulling)
                        {

                            //Frustum culling
                            draw = this.cullManager.Cull(this.DrawContext.CameraVolume, CullIndexDrawIndex, toCullDeferred);
                        }
                        else
                        {
                            draw = true;
                        }

                        if (draw)
                        {
                            var groundVolume = scene.GetSceneVolume();
                            if (groundVolume != null)
                            {
                                //Ground culling
                                draw = this.cullManager.Cull(groundVolume, CullIndexDrawIndex, toCullDeferred);
                            }
                        }
#if DEBUG
                        swCull.Stop();

                        deferred_cull = swCull.ElapsedTicks;
#endif
                        #endregion

                        if (draw)
                        {
                            #region Geometry Buffer
#if DEBUG
                            Stopwatch swGeometryBuffer = Stopwatch.StartNew();
#endif
#if DEBUG
                            Stopwatch swGeometryBufferInit = Stopwatch.StartNew();
#endif
                            this.BindGBuffer();
#if DEBUG
                            swGeometryBufferInit.Stop();
#endif
#if DEBUG
                            Stopwatch swGeometryBufferDraw = Stopwatch.StartNew();
#endif
                            //Draw scene on g-buffer render targets
                            this.DrawResultComponents(gameTime, this.DrawContext, CullIndexDrawIndex, deferredEnabledComponents, true);
#if DEBUG
                            swGeometryBufferDraw.Stop();
#endif
#if DEBUG
                            Stopwatch swGeometryBufferResolve = Stopwatch.StartNew();
#endif
#if DEBUG
                            swGeometryBufferResolve.Stop();
#endif
#if DEBUG
                            swGeometryBuffer.Stop();
#endif
#if DEBUG
                            deferred_gbuffer = swGeometryBuffer.ElapsedTicks;
                            deferred_gbufferInit = swGeometryBufferInit.ElapsedTicks;
                            deferred_gbufferDraw = swGeometryBufferDraw.ElapsedTicks;
                            deferred_gbufferResolve = swGeometryBufferResolve.ElapsedTicks;
#endif
                            #endregion

                            #region Light Buffer
#if DEBUG
                            Stopwatch swLightBuffer = Stopwatch.StartNew();
#endif
                            this.BindLights();

                            //Draw scene lights on light buffer using g-buffer output
                            this.DrawLights(this.DrawContext);
#if DEBUG
                            swLightBuffer.Stop();
#endif
#if DEBUG
                            deferred_lbuffer = swLightBuffer.ElapsedTicks;

                            long[] deferredCounters = Counters.GetStatistics("DEFERRED_LIGHTING") as long[];
                            if (deferredCounters != null)
                            {
                                deferred_lbufferInit = deferredCounters[0];
                                deferred_lbufferDir = deferredCounters[1];
                                deferred_lbufferPoi = deferredCounters[2];
                                deferred_lbufferSpo = deferredCounters[3];
                            }
#endif
                            #endregion
                        }
                    }

                    #region Final composition
#if DEBUG
                    Stopwatch swComponsition = Stopwatch.StartNew();
#endif
                    this.BindResult();

                    //Draw scene result on screen using g-buffer and light buffer
                    this.DrawResult(this.DrawContext);

#if DEBUG
                    swComponsition.Stop();

                    deferred_compose = swComponsition.ElapsedTicks;

                    long[] deferredCompositionCounters = Counters.GetStatistics("DEFERRED_COMPOSITION") as long[];
                    if (deferredCompositionCounters != null)
                    {
                        deferred_composeInit = deferredCompositionCounters[0];
                        deferred_composeDraw = deferredCompositionCounters[1];
                    }
#endif

                    #endregion

                    #endregion

                    #region Forward rendering

                    //Render to screen deferred disabled components
                    var deferredDisabledComponents = visibleComponents.FindAll(c => !c.DeferredEnabled);
                    if (deferredDisabledComponents.Count > 0)
                    {
                        #region Cull
#if DEBUG
                        Stopwatch swCull = Stopwatch.StartNew();
#endif
                        var toCullNotDeferred = deferredDisabledComponents.FindAll(s => s.Is<ICullable>()).ConvertAll(s => s.Get<ICullable>());

                        bool draw = false;
                        if (scene.PerformFrustumCulling)
                        {
                            //Frustum culling
                            draw = this.cullManager.Cull(this.DrawContext.CameraVolume, CullIndexDrawIndex, toCullNotDeferred);
                        }
                        else
                        {
                            draw = true;
                        }

                        if (draw)
                        {
                            var groundVolume = scene.GetSceneVolume();
                            if (groundVolume != null)
                            {
                                //Ground culling
                                draw = this.cullManager.Cull(groundVolume, CullIndexDrawIndex, toCullNotDeferred);
                            }
                        }
#if DEBUG
                        swCull.Stop();

                        disabledDeferred_cull = swCull.ElapsedTicks;
#endif
                        #endregion

                        if (draw)
                        {
                            #region Draw deferred disabled components
#if DEBUG
                            Stopwatch swDraw = Stopwatch.StartNew();
#endif
                            //Set forward mode
                            this.DrawContext.DrawerMode = DrawerModesEnum.Forward;

                            //Draw scene
                            this.DrawResultComponents(gameTime, this.DrawContext, CullIndexDrawIndex, deferredDisabledComponents, false);

                            //Set deferred mode
                            this.DrawContext.DrawerMode = DrawerModesEnum.Deferred;
#if DEBUG
                            swDraw.Stop();

                            disabledDeferred_draw = swDraw.ElapsedTicks;
#endif
                            #endregion
                        }
                    }

                    #endregion
                }
#if DEBUG
                swTotal.Stop();

                total = swTotal.ElapsedTicks;
#endif
#if DEBUG
                long totalShadowMap = shadowMap_start + shadowMap_cull + shadowMap_draw;
                if (totalShadowMap > 0)
                {
                    float prcStart = (float)shadowMap_start / (float)totalShadowMap;
                    float prcCull = (float)shadowMap_cull / (float)totalShadowMap;
                    float prcDraw = (float)shadowMap_draw / (float)totalShadowMap;

                    Counters.SetStatistics("Scene.Draw.totalShadowMap", string.Format(
                        "SM = {0:000000}; Start {1:00}%; Cull {2:00}%; Draw {3:00}%",
                        totalShadowMap,
                        prcStart * 100f,
                        prcCull * 100f,
                        prcDraw * 100f));
                }

                long totalDeferred = deferred_cull + deferred_gbuffer + deferred_lbuffer + deferred_compose + disabledDeferred_cull + disabledDeferred_draw;
                if (totalDeferred > 0)
                {
                    float prcCull = (float)deferred_cull / (float)totalDeferred;
                    float prcDCull = (float)disabledDeferred_cull / (float)totalDeferred;
                    float prcGBuffer = (float)deferred_gbuffer / (float)totalDeferred;
                    float prcLBuffer = (float)deferred_lbuffer / (float)totalDeferred;
                    float prcCompose = (float)deferred_compose / (float)totalDeferred;
                    float prcDraw = (float)disabledDeferred_draw / (float)totalDeferred;

                    Counters.SetStatistics("Scene.Draw.totalDeferred", string.Format(
                        "DR = {0:000000}; Cull {1:00}%; GBuffer {2:00}%; LBuffer {3:00}%; Compose {4:00}%; DeferredDisabledCull {5:00}%; DeferredDisabledDraw {6:00}%",
                        totalDeferred,
                        prcCull * 100f,
                        prcGBuffer * 100f,
                        prcLBuffer * 100f,
                        prcCompose * 100f,
                        prcDCull * 100f,
                        prcDraw * 100f));

                    if (deferred_gbuffer > 0)
                    {
                        float prcPass1 = (float)deferred_gbufferInit / (float)deferred_gbuffer;
                        float prcPass2 = (float)deferred_gbufferDraw / (float)deferred_gbuffer;
                        float prcPass3 = (float)deferred_gbufferResolve / (float)deferred_gbuffer;

                        Counters.SetStatistics("Scene.Draw.deferred_gbuffer PRC", string.Format(
                            "GBuffer = {0:000000}; Init {1:00}%; Draw {2:00}%; Resolve {3:00}%",
                            deferred_gbuffer,
                            prcPass1 * 100f,
                            prcPass2 * 100f,
                            prcPass3 * 100f));

                        Counters.SetStatistics("Scene.Draw.deferred_gbuffer CNT", string.Format(
                            "GBuffer = {0:000000}; Init {1:000000}; Draw {2:000000}; Resolve {3:000000}",
                            deferred_gbuffer,
                            deferred_gbufferInit,
                            deferred_gbufferDraw,
                            deferred_gbufferResolve));
                    }

                    if (deferred_lbuffer > 0)
                    {
                        float prcPass1 = (float)deferred_lbufferInit / (float)deferred_lbuffer;
                        float prcPass2 = (float)deferred_lbufferDir / (float)deferred_lbuffer;
                        float prcPass3 = (float)deferred_lbufferPoi / (float)deferred_lbuffer;
                        float prcPass4 = (float)deferred_lbufferSpo / (float)deferred_lbuffer;

                        Counters.SetStatistics("Scene.Draw.deferred_lbuffer PRC", string.Format(
                            "LBuffer = {0:000000}; Init {1:00}%; Directionals {2:00}%; Points {3:00}%; Spots {4:00}%",
                            deferred_lbuffer,
                            prcPass1 * 100f,
                            prcPass2 * 100f,
                            prcPass3 * 100f,
                            prcPass4 * 100f));

                        Counters.SetStatistics("Scene.Draw.deferred_lbuffer CNT", string.Format(
                            "LBuffer = {0:000000}; Init {1:000000}; Directionals {2:000000}; Points {3:000000}; Spots {4:000000}",
                            deferred_lbuffer,
                            deferred_lbufferInit,
                            deferred_lbufferDir,
                            deferred_lbufferPoi,
                            deferred_lbufferSpo));
                    }

                    if (deferred_compose > 0)
                    {
                        float prcPass1 = (float)deferred_composeInit / (float)deferred_compose;
                        float prcPass2 = (float)deferred_composeDraw / (float)deferred_compose;

                        Counters.SetStatistics("Scene.Draw.deferred_compose PRC", string.Format(
                            "Compose = {0:000000}; Init {1:00}%; Draw {2:00}%",
                            deferred_compose,
                            prcPass1 * 100f,
                            prcPass2 * 100f));

                        Counters.SetStatistics("Scene.Draw.deferred_compose CNT", string.Format(
                            "Compose = {0:000000}; Init {1:000000}; Draw {2:000000}",
                            deferred_compose,
                            deferred_composeInit,
                            deferred_composeDraw));
                    }
                }

                long other = total - (totalShadowMap + totalDeferred);

                float prcSM = (float)totalShadowMap / (float)total;
                float prcDR = (float)totalDeferred / (float)total;
                float prcOther = (float)other / (float)total;

                Counters.SetStatistics("Scene.Draw", string.Format(
                    "TOTAL = {0:000000}; Shadows {1:00}%; Deferred {2:00}%; Other {3:00}%;",
                    total,
                    prcSM * 100f,
                    prcDR * 100f,
                    prcOther * 100f));
#endif
            }
        }

        /// <summary>
        /// Updates renderer parameters
        /// </summary>
        private void UpdateRectangleAndView()
        {
            this.Width = this.Game.Form.RenderWidth;
            this.Height = this.Game.Form.RenderHeight;

            this.viewport = new Viewport(0, 0, this.Width, this.Height, 0, 1.0f);

            this.ViewProjection = Sprite.CreateViewOrthoProjection(this.Width, this.Height);

            this.lightDrawer.Update(this.Game.Graphics, this.Width, this.Height);
        }
        /// <summary>
        /// Binds graphics for g-buffer pass
        /// </summary>
        private void BindGBuffer()
        {
            var graphics = this.Game.Graphics;

            //Set local viewport
            graphics.SetViewport(this.viewport);

            //Set g-buffer render targets
            graphics.SetRenderTargets(
                this.geometryBuffer.Targets, true, Color.Black,
                graphics.DefaultDepthStencil, true, true,
                true);
        }
        /// <summary>
        /// Binds graphics for light acummulation pass
        /// </summary>
        private void BindLights()
        {
            var graphics = this.Game.Graphics;

            //Set local viewport
            graphics.SetViewport(this.viewport);

            //Set light buffer to draw lights
            graphics.SetRenderTargets(
                this.lightBuffer.Targets, true, Color.Black,
                graphics.DefaultDepthStencil, false, false,
                false);
        }
        /// <summary>
        /// Binds graphics for results pass
        /// </summary>
        private void BindResult()
        {
            var graphics = this.Game.Graphics;

            //Restore backbuffer as render target and clear it
            graphics.SetDefaultViewport();
            graphics.SetDefaultRenderTarget(true, false, true);
        }
        /// <summary>
        /// Draw lights
        /// </summary>
        /// <param name="context">Drawing context</param>
        private void DrawLights(DrawContext context)
        {
            var graphics = this.Game.Graphics;

#if DEBUG
            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            Counters.MaxInstancesPerFrame += context.Lights.DirectionalLights.Length;
            Counters.MaxInstancesPerFrame += context.Lights.PointLights.Length;
            Counters.MaxInstancesPerFrame += context.Lights.SpotLights.Length;

            #region Initialization
#if DEBUG
            Stopwatch swPrepare = Stopwatch.StartNew();
#endif
            var effect = DrawerPool.EffectDeferredComposer;

            var hemisphericLight = context.Lights.GetVisibleHemisphericLight();
            var directionalLights = context.Lights.GetVisibleDirectionalLights();
            var spotLights = context.Lights.GetVisibleSpotLights();
            var pointLights = context.Lights.GetVisiblePointLights();

            effect.UpdatePerFrame(
                this.ViewProjection,
                context.EyePosition,
                this.GeometryMap[0],
                this.GeometryMap[1],
                this.GeometryMap[2]);

            graphics.SetDepthStencilRDZDisabled();
            this.SetBlendDeferredLighting();

            this.lightDrawer.BindGeometry(graphics);
#if DEBUG
            swPrepare.Stop();
#endif
            #endregion

            #region Directional Lights
#if DEBUG
            Stopwatch swDirectional = Stopwatch.StartNew();
#endif
            if (directionalLights != null && directionalLights.Length > 0)
            {
                this.lightDrawer.BindDirectional(graphics);

                for (int i = 0; i < directionalLights.Length; i++)
                {
                    effect.UpdatePerLight(
                        directionalLights[i],
                        context.ShadowMaps,
                        context.ShadowMapLow,
                        context.ShadowMapHigh);

                    this.lightDrawer.DrawDirectional(graphics, effect);
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
            if (pointLights != null && pointLights.Length > 0)
            {
                this.lightDrawer.BindPoint(graphics);

                for (int i = 0; i < pointLights.Length; i++)
                {
                    var light = pointLights[i];

                    ShadowMapFlags flags = ShadowMapFlags.None;
                    IShadowMap shadowMap = null;
                    if (light.CastShadow)
                    {
                        flags = context.ShadowMaps;
                        shadowMap = context.ShadowMapCube;
                    }

                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        light.Local,
                        context.ViewProjection,
                        flags,
                        shadowMap);

                    this.lightDrawer.DrawPoint(graphics, effect);
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
            if (spotLights != null && spotLights.Length > 0)
            {
                this.lightDrawer.BindSpot(graphics);

                for (int i = 0; i < spotLights.Length; i++)
                {
                    var light = spotLights[i];

                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        light.Local,
                        context.ViewProjection);

                    this.lightDrawer.DrawSpot(graphics, effect);
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
            long total = swPrepare.ElapsedTicks + swDirectional.ElapsedTicks + swPoint.ElapsedTicks + swSpot.ElapsedTicks;
            if (total > 0)
            {
                float prcPrepare = (float)swPrepare.ElapsedTicks / (float)total;
                float prcDirectional = (float)swDirectional.ElapsedTicks / (float)total;
                float prcPoint = (float)swPoint.ElapsedTicks / (float)total;
                float prcSpot = (float)swSpot.ElapsedTicks / (float)total;
                float prcWasted = (float)(swTotal.ElapsedTicks - total) / (float)total;

                Counters.SetStatistics("DeferredRenderer.DrawLights", string.Format(
                    "{0:000000}; Init {1:00}%; Directional {2:00}%; Point {3:00}%; Spot {4:00}%; Other {5:00}%",
                    swTotal.ElapsedTicks,
                    prcPrepare * 100f,
                    prcDirectional * 100f,
                    prcPoint * 100f,
                    prcSpot * 100f,
                    prcWasted * 100f));
            }

            float perDirectionalLight = 0f;
            float perPointLight = 0f;
            float perSpotLight = 0f;

            if (directionalLights != null && directionalLights.Length > 0)
            {
                long totalDirectional = swDirectional.ElapsedTicks;
                if (totalDirectional > 0)
                {
                    perDirectionalLight = (float)totalDirectional / (float)directionalLights.Length;
                }
            }

            if (pointLights != null && pointLights.Length > 0)
            {
                long totalPoint = swPoint.ElapsedTicks;
                if (totalPoint > 0)
                {
                    perPointLight = (float)totalPoint / (float)pointLights.Length;
                }
            }

            if (spotLights != null && spotLights.Length > 0)
            {
                long totalSpot = swSpot.ElapsedTicks;
                if (totalSpot > 0)
                {
                    perSpotLight = (float)totalSpot / (float)spotLights.Length;
                }
            }

            Counters.SetStatistics("DeferredRenderer.DrawLights.Types", string.Format(
                "Directional {0:000000}; Point {1:000000}; Spot {2:000000}",
                perDirectionalLight,
                perPointLight,
                perSpotLight));

            Counters.SetStatistics("DEFERRED_LIGHTING", new[]
            {
                swPrepare.ElapsedTicks,
                swDirectional.ElapsedTicks,
                swPoint.ElapsedTicks,
                swSpot.ElapsedTicks,
            });
#endif
        }
        /// <summary>
        /// Draw result
        /// </summary>
        /// <param name="context">Drawing context</param>
        private void DrawResult(DrawContext context)
        {
            var graphics = this.Game.Graphics;

#if DEBUG
            long total = 0;
            long init = 0;
            long draw = 0;

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            if (this.GeometryMap != null && this.LightMap != null)
            {
#if DEBUG
                Stopwatch swInit = Stopwatch.StartNew();
#endif
                var effect = DrawerPool.EffectDeferredComposer;

                effect.UpdateComposer(
                    this.ViewProjection,
                    this.GeometryMap[2],
                    this.LightMap[0],
                    context);

                this.lightDrawer.BindResult(graphics);

                graphics.SetDepthStencilNone();
                graphics.SetRasterizerDefault();
                graphics.SetBlendDefault();
#if DEBUG
                swInit.Stop();

                init = swInit.ElapsedTicks;
#endif
#if DEBUG
                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                this.lightDrawer.DrawResult(graphics, effect);
#if DEBUG
                swDraw.Stop();

                draw = swDraw.ElapsedTicks;
#endif
            }
#if DEBUG
            swTotal.Stop();

            total = swTotal.ElapsedTicks;
#endif
#if DEBUG
            Counters.SetStatistics("DEFERRED_COMPOSITION", new[]
            {
                init,
                draw,
            });
#endif
        }
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="components">Components</param>
        /// <param name="deferred">Deferred drawing</param>
        private void DrawResultComponents(GameTime gameTime, DrawContext context, int index, IEnumerable<SceneObject> components, bool deferred)
        {
            var mode = context.DrawerMode;
            var graphics = this.Game.Graphics;

            //First opaques
            var opaques = components.FindAll(c =>
            {
                if (!c.Is<Drawable>()) return false;

                var cull = c.Get<ICullable>();

                return cull != null ? !this.cullManager.GetCullValue(index, cull).Culled : true;
            });
            if (opaques.Count > 0)
            {
                context.DrawerMode = mode | DrawerModesEnum.OpaqueOnly;

                opaques.Sort((c1, c2) =>
                {
                    int res = c1.Order.CompareTo(c2.Order);

                    if (res == 0)
                    {
                        res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
                    }

                    if (res == 0)
                    {
                        var cull1 = c1.Get<ICullable>();
                        var cull2 = c2.Get<ICullable>();

                        var d1 = cull1 != null ? this.cullManager.GetCullValue(index, cull1).Distance : float.MaxValue;
                        var d2 = cull2 != null ? this.cullManager.GetCullValue(index, cull2).Distance : float.MaxValue;

                        res = -d1.CompareTo(d2);
                    }

                    return res;
                });

                opaques.ForEach((c) =>
                {
                    Counters.MaxInstancesPerFrame += c.Count;

                    graphics.SetRasterizerDefault();

                    if (deferred)
                    {
                        this.SetBlendDeferredComposer();
                    }
                    else
                    {
                        graphics.SetBlendDefault();
                    }

                    if (c.DepthEnabled)
                    {
                        graphics.SetDepthStencilZEnabled();
                    }
                    else
                    {
                        graphics.SetDepthStencilZDisabled();
                    }

                    c.Get<IDrawable>().Draw(context);
                });
            }

            //Then transparents
            var transparents = components.FindAll(c =>
            {
                if (!c.AlphaEnabled) return false;

                if (!c.Is<Drawable>()) return false;

                var cull = c.Get<ICullable>();

                return cull != null ? !this.cullManager.GetCullValue(index, cull).Culled : true;
            });
            if (transparents.Count > 0)
            {
                context.DrawerMode = mode | DrawerModesEnum.TransparentOnly;

                transparents.Sort((c1, c2) =>
                {
                    int res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
                    if (res == 0)
                    {
                        var cull1 = c1.Get<ICullable>();
                        var cull2 = c2.Get<ICullable>();

                        var d1 = cull1 != null ? this.cullManager.GetCullValue(index, cull1).Distance : float.MaxValue;
                        var d2 = cull2 != null ? this.cullManager.GetCullValue(index, cull2).Distance : float.MaxValue;

                        res = -d1.CompareTo(d2);
                    }

                    if (res == 0)
                    {
                        res = -c1.Order.CompareTo(c2.Order);
                    }

                    return -res;
                });

                transparents.ForEach((c) =>
                {
                    Counters.MaxInstancesPerFrame += c.Count;

                    graphics.SetRasterizerDefault();

                    if (deferred)
                    {
                        this.SetBlendDeferredComposerTransparent();
                    }
                    else
                    {
                        graphics.SetBlendTransparent();
                    }

                    if (c.DepthEnabled)
                    {
                        graphics.SetDepthStencilZEnabled();
                    }
                    else
                    {
                        graphics.SetDepthStencilZDisabled();
                    }

                    c.Get<IDrawable>().Draw(context);
                });
            }

            context.DrawerMode = mode;
        }

        /// <summary>
        /// Sets deferred composer blend state
        /// </summary>
        private void SetBlendDeferredComposer()
        {
            this.Game.Graphics.SetBlendState(this.blendDeferredComposer);
        }
        /// <summary>
        /// Sets transparent deferred composer blend state
        /// </summary>
        private void SetBlendDeferredComposerTransparent()
        {
            this.Game.Graphics.SetBlendState(this.blendDeferredComposerTransparent);
        }
        /// <summary>
        /// Sets deferred lighting blend state
        /// </summary>
        private void SetBlendDeferredLighting()
        {
            this.Game.Graphics.SetBlendState(this.blendDeferredLighting);
        }
    }
}
