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
                return geometryBuffer?.Textures ?? new EngineShaderResourceView[] { };
            }
        }
        /// <summary>
        /// Light map
        /// </summary>
        protected EngineShaderResourceView[] LightMap
        {
            get
            {
                return lightBuffer?.Textures ?? new EngineShaderResourceView[] { };
            }
        }

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

            geometryBuffer = new RenderTarget(scene.Game, Format.R32G32B32A32_Float, false, 3);
            lightBuffer = new RenderTarget(scene.Game, Format.R32G32B32A32_Float, false, 1);

            blendDeferredComposer = EngineBlendState.DeferredComposer(scene.Game.Graphics, 3);
            blendDeferredComposerTransparent = EngineBlendState.DeferredComposerTransparent(scene.Game.Graphics, 3);
            blendDeferredComposerAlpha = EngineBlendState.DeferredComposerAlpha(scene.Game.Graphics, 3);
            blendDeferredComposerAdditive = EngineBlendState.DeferredComposerAdditive(scene.Game.Graphics, 3);
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
            UpdateRectangleAndView();

            geometryBuffer?.Resize();

            lightBuffer?.Resize();

            base.Resize();
        }
        /// <inheritdoc/>
        public override EngineShaderResourceView GetResource(SceneRendererResults result)
        {
            if (result == SceneRendererResults.LightMap) return LightMap[0];

            var colorMap = GeometryMap?.Length > 0 ? GeometryMap[0] : null;
            var normalMap = GeometryMap?.Length > 1 ? GeometryMap[1] : null;
            var depthMap = GeometryMap?.Length > 2 ? GeometryMap[2] : null;

            if (result == SceneRendererResults.ColorMap) return colorMap;
            if (result == SceneRendererResults.NormalMap) return normalMap;
            if (result == SceneRendererResults.DepthMap) return depthMap;

            return base.GetResource(result);
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime)
        {
            if (!Updated)
            {
                return;
            }

            Updated = false;
#if DEBUG
            frameStats.Clear();

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            //Draw visible components
            var visibleComponents = Scene.GetComponents().Where(c => c.Visible);
            if (visibleComponents.Any())
            {
                //Initialize context data from update context
                DrawContext.GameTime = gameTime;
                DrawContext.DrawerMode = DrawerModes.Deferred;
                DrawContext.ViewProjection = UpdateContext.ViewProjection;
                DrawContext.CameraVolume = UpdateContext.CameraVolume;
                DrawContext.EyePosition = UpdateContext.EyePosition;
                DrawContext.EyeTarget = UpdateContext.EyeDirection;

                //Initialize context data from scene
                DrawContext.Lights = Scene.Lights;

                //Initialize context data from shadow mapping
                DrawContext.ShadowMapDirectional = ShadowMapperDirectional;
                DrawContext.ShadowMapPoint = ShadowMapperPoint;
                DrawContext.ShadowMapSpot = ShadowMapperSpot;

                //Shadow mapping
                DoShadowMapping(gameTime);

                #region Deferred rendering

                //Render to G-Buffer only opaque objects
                var deferredEnabledComponents = visibleComponents.Where(c => c.DeferredEnabled && !c.Usage.HasFlag(SceneObjectUsages.UI));
                if (deferredEnabledComponents.Any())
                {
                    DoDeferred(deferredEnabledComponents);
                }

                #region Final composition
#if DEBUG
                Stopwatch swComponsition = Stopwatch.StartNew();
#endif
                BindResult();

                //Draw scene result on screen using g-buffer and light buffer
                DrawResult(DrawContext);

#if DEBUG
                swComponsition.Stop();

                frameStats.DeferredCompose = swComponsition.ElapsedTicks;
#endif
                #endregion

                #endregion

                #region Forward rendering

                //Render to screen deferred disabled components
                var deferredDisabledComponents = visibleComponents.Where(c => !c.DeferredEnabled && !c.Usage.HasFlag(SceneObjectUsages.UI));
                if (deferredDisabledComponents.Any())
                {
                    DoForward(deferredDisabledComponents);
                }

                #endregion

                //Post-processing
                DoPostProcessing(gameTime);

                #region UI rendering (Forward)

                //Render to screen deferred disabled components
                var uiComponents = visibleComponents.Where(c => c.Usage.HasFlag(SceneObjectUsages.UI));
                if (uiComponents.Any())
                {
                    DoForward(uiComponents);
                }

                #endregion

                //Writes result
                WriteResult();
            }
#if DEBUG
            swTotal.Stop();

            frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
        }
        /// <summary>
        /// Do deferred rendering
        /// </summary>
        /// <param name="deferredEnabledComponents">Components</param>
        private void DoDeferred(IEnumerable<ISceneObject> deferredEnabledComponents)
        {
#if DEBUG
            Stopwatch swCull = Stopwatch.StartNew();
#endif
            var toCullDeferred = deferredEnabledComponents.OfType<ICullable>();

            bool draw = false;
            if (Scene.PerformFrustumCulling)
            {
                //Frustum culling
                draw = cullManager.Cull(DrawContext.CameraVolume, CullIndexDrawIndex, toCullDeferred);
            }
            else
            {
                draw = true;
            }

            if (draw)
            {
                var groundVolume = Scene.GetSceneVolume();
                if (groundVolume != null)
                {
                    //Ground culling
                    draw = cullManager.Cull(groundVolume, CullIndexDrawIndex, toCullDeferred);
                }
            }
#if DEBUG
            swCull.Stop();

            frameStats.DeferredCull = swCull.ElapsedTicks;
#endif

            if (draw)
            {
#if DEBUG
                Stopwatch swGeometryBuffer = Stopwatch.StartNew();

                Stopwatch swGeometryBufferInit = Stopwatch.StartNew();
#endif
                BindGBuffer();
#if DEBUG
                swGeometryBufferInit.Stop();

                Stopwatch swGeometryBufferDraw = Stopwatch.StartNew();
#endif
                //Draw scene on g-buffer render targets
                DrawResultComponents(DrawContext, CullIndexDrawIndex, deferredEnabledComponents);
#if DEBUG
                swGeometryBufferDraw.Stop();

                swGeometryBuffer.Stop();

                frameStats.DeferredGbuffer = swGeometryBuffer.ElapsedTicks;
                frameStats.DeferredGbufferInit = swGeometryBufferInit.ElapsedTicks;
                frameStats.DeferredGbufferDraw = swGeometryBufferDraw.ElapsedTicks;
#endif

#if DEBUG
                Stopwatch swLightBuffer = Stopwatch.StartNew();
#endif
                BindLights();

                //Draw scene lights on light buffer using g-buffer output
                DrawLights(DrawContext);
#if DEBUG
                swLightBuffer.Stop();

                frameStats.DeferredLbuffer = swLightBuffer.ElapsedTicks;
#endif
            }
        }
        /// <summary>
        /// Do forward rendering (UI, transparents, etc.)
        /// </summary>
        /// <param name="deferredDisabledComponents">Components</param>
        private void DoForward(IEnumerable<ISceneObject> deferredDisabledComponents)
        {
#if DEBUG
            Stopwatch swCull = Stopwatch.StartNew();
#endif
            var toCullNotDeferred = deferredDisabledComponents.OfType<ICullable>();

            bool draw = false;
            if (Scene.PerformFrustumCulling)
            {
                //Frustum culling
                draw = cullManager.Cull(DrawContext.CameraVolume, CullIndexDrawIndex, toCullNotDeferred);
            }
            else
            {
                draw = true;
            }

            if (draw)
            {
                var groundVolume = Scene.GetSceneVolume();
                if (groundVolume != null)
                {
                    //Ground culling
                    draw = cullManager.Cull(groundVolume, CullIndexDrawIndex, toCullNotDeferred);
                }
            }
#if DEBUG
            swCull.Stop();

            frameStats.DisabledDeferredCull = swCull.ElapsedTicks;
#endif

            if (draw)
            {
#if DEBUG
                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                //Set forward mode
                DrawContext.DrawerMode = DrawerModes.Forward;

                //Draw scene
                DrawResultComponents(DrawContext, CullIndexDrawIndex, deferredDisabledComponents);

                //Set deferred mode
                DrawContext.DrawerMode = DrawerModes.Deferred;
#if DEBUG
                swDraw.Stop();

                frameStats.DisabledDeferredDraw = swDraw.ElapsedTicks;
#endif
            }
        }

        /// <summary>
        /// Updates renderer parameters
        /// </summary>
        private void UpdateRectangleAndView()
        {
            Width = Scene.Game.Form.RenderWidth;
            Height = Scene.Game.Form.RenderHeight;

            Viewport = Scene.Game.Form.GetViewport();

            ViewProjection = Scene.Game.Form.GetOrthoProjectionMatrix();

            lightDrawer.Update(Scene.Game.Graphics, Width, Height);
        }
        /// <summary>
        /// Binds graphics for g-buffer pass
        /// </summary>
        private void BindGBuffer()
        {
            var graphics = Scene.Game.Graphics;

            //Set local viewport
            graphics.SetViewport(Viewport);

            //Set g-buffer render targets
            graphics.SetRenderTargets(
                geometryBuffer.Targets, true, Color.Black,
                graphics.DefaultDepthStencil, true, true,
                true);
        }
        /// <summary>
        /// Binds graphics for light acummulation pass
        /// </summary>
        private void BindLights()
        {
            var graphics = Scene.Game.Graphics;

            //Set local viewport
            graphics.SetViewport(Viewport);

            //Set light buffer to draw lights
            graphics.SetRenderTargets(
                lightBuffer.Targets, true, Color.Black,
                graphics.DefaultDepthStencil, false, false,
                false);
        }

        /// <summary>
        /// Draw lights
        /// </summary>
        /// <param name="context">Drawing context</param>
        private void DrawLights(DrawContext context)
        {
#if DEBUG
            lightStats.Clear();

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            var graphics = Scene.Game.Graphics;

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
                ViewProjection,
                context.EyePosition,
                GeometryMap[0],
                GeometryMap[1],
                GeometryMap[2]);

            graphics.SetDepthStencilRDZDisabled();
            SetBlendDeferredLighting();

            lightDrawer.BindGeometry(graphics);
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
                lightDrawer.BindGobalLight(graphics);

                foreach (var light in directionalLights)
                {
                    effect.UpdatePerLight(
                        light,
                        context.ShadowMapDirectional);

                    lightDrawer.DrawDirectional(graphics, effect);
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
                lightDrawer.BindPoint(graphics);

                foreach (var light in pointLights)
                {
                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        light.Local,
                        context.ViewProjection,
                        context.ShadowMapPoint);

                    lightDrawer.DrawPoint(graphics, effect);
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
                lightDrawer.BindSpot(graphics);

                foreach (var light in spotLights)
                {
                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        light.Local,
                        context.ViewProjection,
                        context.ShadowMapSpot);

                    lightDrawer.DrawSpot(graphics, effect);
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
        /// <param name="context">Drawing context</param>
        private void DrawResult(DrawContext context)
        {
            var graphics = Scene.Game.Graphics;

#if DEBUG
            long init = 0;
            long draw = 0;

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            if (GeometryMap != null && LightMap != null)
            {
#if DEBUG
                Stopwatch swInit = Stopwatch.StartNew();
#endif
                var effect = DrawerPool.EffectDeferredComposer;

                effect.UpdateComposer(
                    ViewProjection,
                    GeometryMap[2],
                    LightMap[0],
                    context);

                lightDrawer.BindResult(graphics);

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
                lightDrawer.DrawResult(graphics, effect);
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
            var opaques = GetOpaques(index, components);
            if (opaques.Any())
            {
                //Set opaques draw mode
                context.DrawerMode = mode | DrawerModes.OpaqueOnly;

                //Sort items (nearest first)
                opaques.Sort((c1, c2) => SortOpaques(index, c1, c2));

                //Draw items
                opaques.ForEach((c) => Draw(context, c));
            }

            //Then transparents
            var transparents = GetTransparents(index, components);
            if (transparents.Any())
            {
                //Set transparents draw mode
                context.DrawerMode = mode | DrawerModes.TransparentOnly;

                //Sort items (far first)
                transparents.Sort((c1, c2) => SortTransparents(index, c1, c2));

                //Draw items
                transparents.ForEach((c) => Draw(context, c));
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
                    SetBlendDeferredComposerAdditive();
                }
                else if (blendMode.HasFlag(BlendModes.Transparent) || blendMode.HasFlag(BlendModes.Alpha))
                {
                    SetBlendDeferredComposerTransparent();
                }
                else
                {
                    SetBlendDeferredComposer();
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
            Scene.Game.Graphics.SetBlendState(blendDeferredComposer);
        }
        /// <summary>
        /// Sets transparent deferred composer blend state
        /// </summary>
        private void SetBlendDeferredComposerTransparent()
        {
            Scene.Game.Graphics.SetBlendState(blendDeferredComposerTransparent);
        }
        /// <summary>
        /// Sets additive deferred composer blend state
        /// </summary>
        private void SetBlendDeferredComposerAdditive()
        {
            Scene.Game.Graphics.SetBlendState(blendDeferredComposerAdditive);
        }
        /// <summary>
        /// Sets deferred lighting blend state
        /// </summary>
        private void SetBlendDeferredLighting()
        {
            Scene.Game.Graphics.SetBlendState(blendDeferredLighting);
        }
    }
}
