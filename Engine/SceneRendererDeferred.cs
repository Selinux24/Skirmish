#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Deferred renderer class
    /// </summary>
    public class SceneRendererDeferred : BaseSceneRenderer
    {
#if DEBUG
        private readonly FrameStatsDeferred frameStats = new FrameStatsDeferred();

        private readonly FrameStatsLight lightStats = new FrameStatsLight();
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

                return new EngineShaderResourceView[] { };
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

                return new EngineShaderResourceView[] { };
            }
        }

        /// <summary>
        /// View port
        /// </summary>
        public Viewport Viewport { get; set; }

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
            this.blendDeferredComposerAlpha = EngineBlendState.DeferredComposerAlpha(game.Graphics, 3);
            this.blendDeferredComposerAdditive = EngineBlendState.DeferredComposerAdditive(game.Graphics, 3);
            this.blendDeferredLighting = EngineBlendState.DeferredLighting(game.Graphics);
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
                geometryBuffer?.Dispose();
                geometryBuffer = null;
                lightBuffer?.Dispose();
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
        /// <inheritdoc/>
        public override EngineShaderResourceView GetResource(SceneRendererResults result)
        {
            if (result == SceneRendererResults.LightMap) return this.LightMap[0];

            var colorMap = this.GeometryMap?.Length > 0 ? this.GeometryMap[0] : null;
            var normalMap = this.GeometryMap?.Length > 1 ? this.GeometryMap[1] : null;
            var depthMap = this.GeometryMap?.Length > 2 ? this.GeometryMap[2] : null;

            if (result == SceneRendererResults.ColorMap) return colorMap;
            if (result == SceneRendererResults.NormalMap) return normalMap;
            if (result == SceneRendererResults.DepthMap) return depthMap;

            return base.GetResource(result);
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime, Scene scene)
        {
            if (this.Updated)
            {
                this.Updated = false;
#if DEBUG
                this.frameStats.Clear();

                Stopwatch swTotal = Stopwatch.StartNew();
#endif
                //Draw visible components
                var visibleComponents = scene.GetComponents().Where(c => c.Visible);
                if (visibleComponents.Any())
                {
                    //Initialize context data from update context
                    this.DrawContext.GameTime = gameTime;
                    this.DrawContext.DrawerMode = DrawerModes.Deferred;
                    this.DrawContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawContext.CameraVolume = this.UpdateContext.CameraVolume;
                    this.DrawContext.EyePosition = this.UpdateContext.EyePosition;
                    this.DrawContext.EyeTarget = this.UpdateContext.EyeDirection;

                    //Initialize context data from scene
                    this.DrawContext.Lights = scene.Lights;
                    //Initialize context data from shadow mapping

                    this.DrawContext.ShadowMapDirectional = this.ShadowMapperDirectional;
                    this.DrawContext.ShadowMapPoint = this.ShadowMapperPoint;
                    this.DrawContext.ShadowMapSpot = this.ShadowMapperSpot;

                    //Shadow mapping
                    DoShadowMapping(gameTime, scene);

                    #region Deferred rendering

                    //Render to G-Buffer only opaque objects
                    var deferredEnabledComponents = visibleComponents.Where(c => c.DeferredEnabled);
                    if (deferredEnabledComponents.Any())
                    {
                        DoDeferred(scene, deferredEnabledComponents);
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

                    this.frameStats.DeferredCompose = swComponsition.ElapsedTicks;
#endif
                    #endregion

                    #endregion

                    #region Forward rendering

                    //Render to screen deferred disabled components
                    var deferredDisabledComponents = visibleComponents.Where(c => !c.DeferredEnabled);
                    if (deferredDisabledComponents.Any())
                    {
                        DoForward(scene, deferredDisabledComponents);
                    }

                    #endregion
                }
#if DEBUG
                swTotal.Stop();

                this.frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
            }
        }
        /// <summary>
        /// Do deferred rendering
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="deferredEnabledComponents">Components</param>
        private void DoDeferred(Scene scene, IEnumerable<ISceneObject> deferredEnabledComponents)
        {
#if DEBUG
            Stopwatch swCull = Stopwatch.StartNew();
#endif
            var toCullDeferred = deferredEnabledComponents.OfType<ICullable>();

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

            this.frameStats.DeferredCull = swCull.ElapsedTicks;
#endif

            if (draw)
            {
#if DEBUG
                Stopwatch swGeometryBuffer = Stopwatch.StartNew();

                Stopwatch swGeometryBufferInit = Stopwatch.StartNew();
#endif
                this.BindGBuffer();
#if DEBUG
                swGeometryBufferInit.Stop();

                Stopwatch swGeometryBufferDraw = Stopwatch.StartNew();
#endif
                //Draw scene on g-buffer render targets
                this.DrawResultComponents(this.DrawContext, CullIndexDrawIndex, deferredEnabledComponents);
#if DEBUG
                swGeometryBufferDraw.Stop();

                swGeometryBuffer.Stop();

                this.frameStats.DeferredGbuffer = swGeometryBuffer.ElapsedTicks;
                this.frameStats.DeferredGbufferInit = swGeometryBufferInit.ElapsedTicks;
                this.frameStats.DeferredGbufferDraw = swGeometryBufferDraw.ElapsedTicks;
#endif

#if DEBUG
                Stopwatch swLightBuffer = Stopwatch.StartNew();
#endif
                this.BindLights();

                //Draw scene lights on light buffer using g-buffer output
                this.DrawLights(this.DrawContext);
#if DEBUG
                swLightBuffer.Stop();

                this.frameStats.DeferredLbuffer = swLightBuffer.ElapsedTicks;
#endif
            }
        }
        /// <summary>
        /// Do forward rendering (UI, transparents, etc.)
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="deferredDisabledComponents">Components</param>
        private void DoForward(Scene scene, IEnumerable<ISceneObject> deferredDisabledComponents)
        {
#if DEBUG
            Stopwatch swCull = Stopwatch.StartNew();
#endif
            var toCullNotDeferred = deferredDisabledComponents.OfType<ICullable>();

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

            this.frameStats.DisabledDeferredCull = swCull.ElapsedTicks;
#endif

            if (draw)
            {
#if DEBUG
                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                //Set forward mode
                this.DrawContext.DrawerMode = DrawerModes.Forward;

                //Draw scene
                this.DrawResultComponents(this.DrawContext, CullIndexDrawIndex, deferredDisabledComponents);

                //Set deferred mode
                this.DrawContext.DrawerMode = DrawerModes.Deferred;
#if DEBUG
                swDraw.Stop();

                this.frameStats.DisabledDeferredDraw = swDraw.ElapsedTicks;
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

            this.Viewport = this.Game.Form.GetViewport();

            this.ViewProjection = this.Game.Form.GetOrthoProjectionMatrix();

            this.lightDrawer.Update(this.Game.Graphics, this.Width, this.Height);
        }
        /// <summary>
        /// Binds graphics for g-buffer pass
        /// </summary>
        private void BindGBuffer()
        {
            var graphics = this.Game.Graphics;

            //Set local viewport
            graphics.SetViewport(this.Viewport);

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
            graphics.SetViewport(this.Viewport);

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
#if DEBUG
            this.lightStats.Clear();

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            var graphics = this.Game.Graphics;

            Counters.MaxInstancesPerFrame += context.Lights.DirectionalLights.Length;
            Counters.MaxInstancesPerFrame += context.Lights.PointLights.Length;
            Counters.MaxInstancesPerFrame += context.Lights.SpotLights.Length;

            #region Initialization
#if DEBUG
            Stopwatch swPrepare = Stopwatch.StartNew();
#endif
            var effect = DrawerPool.EffectDeferredComposer;

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
            if (directionalLights.Any())
            {
                this.lightDrawer.BindGobalLight(graphics);

                foreach (var light in directionalLights)
                {
                    effect.UpdatePerLight(
                        light,
                        context.ShadowMapDirectional);

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
            if (pointLights.Any())
            {
                this.lightDrawer.BindPoint(graphics);

                foreach (var light in pointLights)
                {
                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        light.Local,
                        context.ViewProjection,
                        context.ShadowMapPoint);

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
            if (spotLights.Any())
            {
                this.lightDrawer.BindSpot(graphics);

                foreach (var light in spotLights)
                {
                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        light.Local,
                        context.ViewProjection,
                        context.ShadowMapSpot);

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
            this.lightStats.Prepare = swPrepare.ElapsedTicks;
            this.lightStats.Directional = swDirectional.ElapsedTicks;
            this.lightStats.Point = swPoint.ElapsedTicks;
            this.lightStats.Spot = swSpot.ElapsedTicks;
            this.lightStats.DirectionalLights = directionalLights?.Count() ?? 0;
            this.lightStats.PointLights = pointLights?.Count() ?? 0;
            this.lightStats.SpotLights = spotLights?.Count() ?? 0;

            this.lightStats.UpdateCounters(swTotal.ElapsedTicks);
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
        /// <param name="context">Context</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(DrawContext context, int index, IEnumerable<ISceneObject> components)
        {
            //Save current drawing mode
            var mode = context.DrawerMode;

            //First opaques
            var opaques = this.GetOpaques(index, components);
            if (opaques.Any())
            {
                //Set opaques draw mode
                context.DrawerMode = mode | DrawerModes.OpaqueOnly;

                //Sort items (nearest first)
                opaques.Sort((c1, c2) => this.SortOpaques(index, c1, c2));

                //Draw items
                opaques.ForEach((c) => this.Draw(context, c));
            }

            //Then transparents
            var transparents = this.GetTransparents(index, components);
            if (transparents.Any())
            {
                //Set transparents draw mode
                context.DrawerMode = mode | DrawerModes.TransparentOnly;

                //Sort items (far first)
                transparents.Sort((c1, c2) => this.SortTransparents(index, c1, c2));

                //Draw items
                transparents.ForEach((c) => this.Draw(context, c));
            }

            //Restore drawing mode
            context.DrawerMode = mode;
        }

        /// <inheritdoc/>
        protected override void SetBlendState(DrawContext context, BlendModes blendMode)
        {
            if (context.DrawerMode.HasFlag(DrawerModes.Deferred))
            {
                if (blendMode.HasFlag(BlendModes.Additive))
                {
                    this.SetBlendDeferredComposerAdditive();
                }
                else if (blendMode.HasFlag(BlendModes.Transparent))
                {
                    this.SetBlendDeferredComposerTransparent();
                }
                else if (blendMode.HasFlag(BlendModes.Alpha))
                {
                    this.SetBlendDeferredComposerAlpha();
                }
                else
                {
                    this.SetBlendDeferredComposer();
                }
            }
            else
            {
                base.SetBlendState(context, blendMode);
            }
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
        /// Sets alpha deferred composer blend state
        /// </summary>
        private void SetBlendDeferredComposerAlpha()
        {
            this.Game.Graphics.SetBlendState(this.blendDeferredComposerAlpha);
        }
        /// <summary>
        /// Sets additive deferred composer blend state
        /// </summary>
        private void SetBlendDeferredComposerAdditive()
        {
            this.Game.Graphics.SetBlendState(this.blendDeferredComposerAdditive);
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
