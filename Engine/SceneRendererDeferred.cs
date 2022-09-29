#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Deferred;
    using Engine.Common;

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
        /// View * OrthoProjection Matrix
        /// </summary>
        protected Matrix ViewProjection;
        /// <summary>
        /// Geometry map
        /// </summary>
        protected IEnumerable<EngineShaderResourceView> GeometryMap
        {
            get
            {
                return geometryBuffer?.Textures ?? new EngineShaderResourceView[] { };
            }
        }
        /// <summary>
        /// Light map
        /// </summary>
        protected IEnumerable<EngineShaderResourceView> LightMap
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
            if (result == SceneRendererResults.LightMap) return LightMap.FirstOrDefault();

            var colorMap = GeometryMap.ElementAtOrDefault(0);
            var normalMap = GeometryMap.ElementAtOrDefault(1);
            var depthMap = GeometryMap.ElementAtOrDefault(2);

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

            //Select visible components
            var visibleComponents = Scene.GetComponents<IDrawable>().Where(c => c.Visible);
            if (!visibleComponents.Any())
            {
                return;
            }

#if DEBUG
            frameStats.Clear();

            Stopwatch swTotal = Stopwatch.StartNew();
#endif

            //Initialize context data from update context
            DrawContext.GameTime = gameTime;
            DrawContext.DrawerMode = DrawerModes.Deferred;
            DrawContext.ViewProjection = UpdateContext.ViewProjection;
            DrawContext.CameraVolume = UpdateContext.CameraVolume;
            DrawContext.EyePosition = UpdateContext.EyePosition;
            DrawContext.EyeTarget = UpdateContext.EyeDirection;

            //Initialize context data from scene
            DrawContext.Lights = Scene.Lights;
            DrawContext.LevelOfDetail = new Vector3(Scene.GameEnvironment.LODDistanceHigh, Scene.GameEnvironment.LODDistanceMedium, Scene.GameEnvironment.LODDistanceLow);

            //Initialize context data from shadow mapping
            DrawContext.ShadowMapDirectional = ShadowMapperDirectional;
            DrawContext.ShadowMapPoint = ShadowMapperPoint;
            DrawContext.ShadowMapSpot = ShadowMapperSpot;

            //Shadow mapping
            DoShadowMapping(gameTime);

            //Binds the result target
            SetTarget(Targets.Objects, true, Scene.GameEnvironment.Background, true, true);

            var deferredEnabledComponents = visibleComponents.Where(c => c.DeferredEnabled && !c.Usage.HasFlag(SceneObjectUsages.UI));
            bool anyDeferred = deferredEnabledComponents.Any();
            var deferredDisabledComponents = visibleComponents.Where(c => !c.DeferredEnabled && !c.Usage.HasFlag(SceneObjectUsages.UI));
            bool anyForward = deferredDisabledComponents.Any();

            if (anyForward || anyDeferred)
            {
                if (anyDeferred)
                {
                    //Render to G-Buffer deferred enabled components
                    DoDeferred(deferredEnabledComponents);

                    //Binds the result target
                    SetTarget(Targets.Objects, false, Color.Transparent);

                    #region Final composition
#if DEBUG
                    Stopwatch swComponsition = Stopwatch.StartNew();
#endif
                    //Draw scene result on screen using g-buffer and light buffer
                    DrawResult();

#if DEBUG
                    swComponsition.Stop();

                    frameStats.DeferredCompose = swComponsition.ElapsedTicks;
#endif
                    #endregion
                }

                if (anyForward)
                {
                    //Render to screen deferred disabled components
                    DoForward(deferredDisabledComponents);
                }

                //Post-processing
                DoPostProcessing(Targets.Objects, RenderPass.Objects, gameTime);
            }

            //Binds the result target
            SetTarget(Targets.UI, true, Color.Transparent);

            //Render to screen deferred disabled components
            var uiComponents = visibleComponents.Where(c => c.Usage.HasFlag(SceneObjectUsages.UI));
            if (uiComponents.Any())
            {
                //UI render
                DoForward(uiComponents);
                //UI post-processing
                DoPostProcessing(Targets.UI, RenderPass.UI, gameTime);
            }

            //Combine to screen
            CombineTargets(Targets.Objects, Targets.UI, Targets.Result);

            //Final post-processing
            DoPostProcessing(Targets.Result, RenderPass.Final, gameTime);

            //Draw to screen
            DrawToScreen(Targets.Result);

#if DEBUG
            swTotal.Stop();

            frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
        }
        /// <summary>
        /// Do deferred rendering
        /// </summary>
        /// <param name="deferredEnabledComponents">Components</param>
        private void DoDeferred(IEnumerable<IDrawable> deferredEnabledComponents)
        {
#if DEBUG
            Stopwatch swCull = Stopwatch.StartNew();
#endif
            var toCullDeferred = deferredEnabledComponents.OfType<ICullable>();

            bool draw = true;
            if (Scene.PerformFrustumCulling)
            {
                //Frustum culling
                draw = cullManager.Cull(DrawContext.CameraVolume, CullIndexDrawIndex, toCullDeferred);
            }

            if (!draw)
            {
                return;
            }

            var groundVolume = Scene.GetSceneVolume();
            if (groundVolume != null)
            {
                //Ground culling
                draw = cullManager.Cull(groundVolume, CullIndexDrawIndex, toCullDeferred);
            }

#if DEBUG
            swCull.Stop();

            frameStats.DeferredCull = swCull.ElapsedTicks;
#endif

            if (!draw)
            {
                return;
            }

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
        /// <summary>
        /// Do forward rendering (UI, transparents, etc.)
        /// </summary>
        /// <param name="deferredDisabledComponents">Components</param>
        private void DoForward(IEnumerable<IDrawable> deferredDisabledComponents)
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
                geometryBuffer.Targets, true, Color.Transparent,
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
                lightBuffer.Targets, true, Color.Transparent,
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
            var directionalLights = context.Lights.GetVisibleDirectionalLights();
            var spotLights = context.Lights.GetVisibleSpotLights();
            var pointLights = context.Lights.GetVisiblePointLights();

            graphics.SetDepthStencilRDZDisabled();
            SetBlendDeferredLighting();

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
                lightDrawer.BindGlobalLight(graphics);

                lightDirectionalDrawer.UpdateGeometryMap(GeometryMap);

                foreach (var light in directionalLights)
                {
                    lightDirectionalDrawer.UpdatePerLight(light);

                    lightDrawer.DrawDirectional(lightDirectionalDrawer);
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

                lightPointDrawer.UpdateGeometryMap(GeometryMap);

                foreach (var light in pointLights)
                {
                    //Draw Pass
                    lightPointDrawer.UpdatePerLight(light);

                    lightDrawer.DrawPoint(graphics, stencilDrawer, lightPointDrawer);
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

                lightSpotDrawer.UpdateGeometryMap(GeometryMap);

                foreach (var light in spotLights)
                {
                    //Draw Pass
                    lightSpotDrawer.UpdatePerLight(light);

                    lightDrawer.DrawSpot(graphics, stencilDrawer, lightSpotDrawer);
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
        private void DrawResult()
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
                composer.UpdateGeometryMap(GeometryMap, LightMap.ElementAtOrDefault(0));

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
                lightDrawer.DrawResult(composer);
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
        private void DrawResultComponents(DrawContext context, int index, IEnumerable<IDrawable> components)
        {
            BuiltInShaders.UpdatePerFrame(context);

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
