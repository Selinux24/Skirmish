﻿#if DEBUG
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
        /// Command list
        /// </summary>
        private readonly List<IEngineCommandList> commands = new();

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
        /// Geometry map
        /// </summary>
        protected IEnumerable<EngineShaderResourceView> GeometryMap
        {
            get
            {
                return geometryBuffer?.Textures ?? Enumerable.Empty<EngineShaderResourceView>();
            }
        }
        /// <summary>
        /// Light map
        /// </summary>
        protected IEnumerable<EngineShaderResourceView> LightMap
        {
            get
            {
                return lightBuffer?.Textures ?? Enumerable.Empty<EngineShaderResourceView>();
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
            var visibleComponents = Scene.Components.Get<IDrawable>(c => c.Visible);
            if (!visibleComponents.Any())
            {
                return;
            }

            commands.Clear();

#if DEBUG
            frameStats.Clear();
            var swTotal = Stopwatch.StartNew();
#endif
            //Updates the draw context
            var drawContext = GetImmediateDrawContext(DrawerModes.Deferred, false);
            var graphics = drawContext.Graphics;
            var ic = drawContext.DeviceContext;

            ic.SetViewport(graphics.Viewport);
            ic.SetRenderTargets(
                graphics.DefaultRenderTarget, true, Scene.GameEnvironment.Background,
                graphics.DefaultDepthStencil, true, true,
                false);

            int passIndex = 0;

            //Shadow mapping
            commands.AddRange(DoShadowMapping(ref passIndex));

            var deferredEnabledComponents = visibleComponents.Where(c => c.DeferredEnabled && !c.Usage.HasFlag(SceneObjectUsages.UI));
            bool anyDeferred = deferredEnabledComponents.Any();
            var deferredDisabledComponents = visibleComponents.Where(c => !c.DeferredEnabled && !c.Usage.HasFlag(SceneObjectUsages.UI));
            bool anyForward = deferredDisabledComponents.Any();

            if (anyForward || anyDeferred)
            {
                if (anyDeferred)
                {
                    //Render to G-Buffer deferred enabled components
                    commands.AddRange(DoDeferred(deferredEnabledComponents, ref passIndex));
                }

                if (anyForward)
                {
                    //Render to screen deferred disabled components
                    commands.AddRange(DoForward(Targets.Objects, false, Scene.GameEnvironment.Background, false, false, deferredDisabledComponents, ref passIndex));
                }

                //Post-processing
                commands.AddRange(DoPostProcessing(Targets.Objects, RenderPass.Objects, ref passIndex));
            }

            //Render to screen deferred disabled components
            var uiComponents = visibleComponents.Where(c => c.Usage.HasFlag(SceneObjectUsages.UI));
            if (uiComponents.Any())
            {
                //UI render
                commands.AddRange(DoForward(Targets.UI, true, Color.Transparent, false, false, uiComponents, ref passIndex));
                //UI post-processing
                commands.AddRange(DoPostProcessing(Targets.UI, RenderPass.UI, ref passIndex));
            }

            //Combine to result
            commands.AddRange(CombineTargets(Targets.Objects, Targets.UI, Targets.Result, ref passIndex));

            //Final post-processing
            commands.AddRange(DoPostProcessing(Targets.Result, RenderPass.Final, ref passIndex));

            //Draw to screen
            commands.AddRange(DrawToScreen(Targets.Result, ref passIndex));

            ic.SetViewport(graphics.Viewport);
            ic.SetRenderTargets(
                graphics.DefaultRenderTarget, true, Scene.GameEnvironment.Background,
                graphics.DefaultDepthStencil, true, true,
                false);

            //Execute command list
            ic.ExecuteCommandLists(commands);

            ic.SetViewport(graphics.Viewport);
            ic.SetRenderTargets(graphics.DefaultRenderTarget, graphics.DefaultDepthStencil);

#if DEBUG
            swTotal.Stop();

            frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
        }
        /// <summary>
        /// Do deferred rendering
        /// </summary>
        /// <param name="components">Components</param>
        /// <param name="passIndex">Pass index</param>
        private IEnumerable<IEngineCommandList> DoDeferred(IEnumerable<IDrawable> components, ref int passIndex)
        {
            if (!components.Any())
            {
                return Enumerable.Empty<IEngineCommandList>();
            }

#if DEBUG
            var swCull = Stopwatch.StartNew();
#endif
            var context = GetDeferredDrawContext(DrawerModes.Deferred, $"{nameof(DoDeferred)}", passIndex++, false);
            bool draw = CullingTest(Scene, context.CameraVolume, components.OfType<ICullable>(), CullIndexDrawIndex);
#if DEBUG
            swCull.Stop();
            frameStats.DeferredCull = swCull.ElapsedTicks;
#endif

            if (!draw)
            {
                return Enumerable.Empty<IEngineCommandList>();
            }

            var dc = context.DeviceContext;

#if DEBUG
            var swGeometryBuffer = Stopwatch.StartNew();
            var swGeometryBufferInit = Stopwatch.StartNew();
#endif
            if (!Scene.Game.BufferManager.SetVertexBuffers(dc))
            {
                return Enumerable.Empty<IEngineCommandList>();
            }

            UpdateGlobalState(dc);

            BindGBuffer(dc);
#if DEBUG
            swGeometryBufferInit.Stop();
            var swGeometryBufferDraw = Stopwatch.StartNew();
#endif
            //Draw scene on g-buffer render targets
            DrawResultComponents(context, CullIndexDrawIndex, components);
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
            DrawLights(context);
#if DEBUG
            swLightBuffer.Stop();

            frameStats.DeferredLbuffer = swLightBuffer.ElapsedTicks;
#endif

            //Binds the result target
            SetTarget(dc, Targets.Objects, false, Color.Transparent);

            #region Final composition
#if DEBUG
            Stopwatch swComponsition = Stopwatch.StartNew();
#endif
            //Draw scene result on screen using g-buffer and light buffer
            DrawResult(context);

#if DEBUG
            swComponsition.Stop();

            frameStats.DeferredCompose = swComponsition.ElapsedTicks;
#endif
            #endregion

            return new[] { dc.FinishCommandList() };
        }
        /// <summary>
        /// Do forward rendering (UI, transparents, etc.)
        /// </summary>
        /// <param name="components">Components</param>
        /// <param name="passIndex">Pass index</param>
        private IEnumerable<IEngineCommandList> DoForward(Targets target, bool clearRT, Color4 clearRTColor, bool clearDepth, bool clearStencil, IEnumerable<IDrawable> components, ref int passIndex)
        {
            if (!components.Any())
            {
                return Enumerable.Empty<IEngineCommandList>();
            }

#if DEBUG
            var swCull = Stopwatch.StartNew();
#endif
            var context = GetDeferredDrawContext(DrawerModes.Forward, $"{nameof(DoForward)}", passIndex++, false);
            bool draw = CullingTest(Scene, context.CameraVolume, components.OfType<ICullable>(), CullIndexDrawIndex);
#if DEBUG
            swCull.Stop();
            frameStats.DisabledDeferredCull = swCull.ElapsedTicks;
#endif

            if (!draw)
            {
                return Enumerable.Empty<IEngineCommandList>();
            }

            var dc = context.DeviceContext;

#if DEBUG
            var swDraw = Stopwatch.StartNew();
#endif
            if (!Scene.Game.BufferManager.SetVertexBuffers(dc))
            {
                return Enumerable.Empty<IEngineCommandList>();
            }

            UpdateGlobalState(dc);

            //Binds the result target
            SetTarget(dc, target, clearRT, clearRTColor, clearDepth, clearStencil);

            //Draw scene
            DrawResultComponents(context, CullIndexDrawIndex, components);

#if DEBUG
            swDraw.Stop();
            frameStats.DisabledDeferredDraw = swDraw.ElapsedTicks;
#endif

            return new[] { dc.FinishCommandList() };
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
        private void BindGBuffer(EngineDeviceContext dc)
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
        private void BindLights(EngineDeviceContext dc)
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
        /// <param name="context">Drawing context</param>
        private void DrawLights(DrawContext context)
        {
            var dc = context.DeviceContext;

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
            lightDrawer.WriteBuffers(dc);

            var directionalLights = context.Lights.GetVisibleDirectionalLights();
            var spotLights = context.Lights.GetVisibleSpotLights();
            var pointLights = context.Lights.GetVisiblePointLights();

            dc.SetDepthStencilState(graphics.GetDepthStencilRDZDisabled());
            SetBlendDeferredLighting(dc);

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

                lightDirectionalDrawer.UpdateGeometryMap(GeometryMap);

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

                lightPointDrawer.UpdateGeometryMap(GeometryMap);

                foreach (var light in pointLights)
                {
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

                lightSpotDrawer.UpdateGeometryMap(GeometryMap);

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
        /// <param name="context">Drawing context</param>
        private void DrawResult(DrawContext context)
        {
            var graphics = Scene.Game.Graphics;
            var dc = context.DeviceContext;

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
        /// <param name="cullIndex">Cull index</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(DrawContext context, int cullIndex, IEnumerable<IDrawable> components)
        {
            BuiltInShaders.UpdatePerFrame(context);

            //Save current drawing mode
            var mode = context.DrawerMode;

            //First opaques
            var opaques = GetOpaques(cullIndex, components);
            if (opaques.Any())
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
            if (transparents.Any())
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
        protected override void SetBlendState(EngineDeviceContext dc, DrawerModes drawerMode, BlendModes blendMode)
        {
            if (drawerMode.HasFlag(DrawerModes.Deferred))
            {
                if (blendMode.HasFlag(BlendModes.Additive))
                {
                    SetBlendDeferredComposerAdditive(dc);
                }
                else if (blendMode.HasFlag(BlendModes.Transparent) || blendMode.HasFlag(BlendModes.Alpha))
                {
                    SetBlendDeferredComposerTransparent(dc);
                }
                else
                {
                    SetBlendDeferredComposer(dc);
                }
            }
            else
            {
                base.SetBlendState(dc, drawerMode, blendMode);
            }
        }
        /// <summary>
        /// Sets deferred composer blend state
        /// </summary>
        /// <param name="dc">Device context</param>
        private void SetBlendDeferredComposer(EngineDeviceContext dc)
        {
            dc.SetBlendState(blendDeferredComposer);
        }
        /// <summary>
        /// Sets transparent deferred composer blend state
        /// </summary>
        /// <param name="dc">Device context</param>
        private void SetBlendDeferredComposerTransparent(EngineDeviceContext dc)
        {
            dc.SetBlendState(blendDeferredComposerTransparent);
        }
        /// <summary>
        /// Sets additive deferred composer blend state
        /// </summary>
        /// <param name="dc">Device context</param>
        private void SetBlendDeferredComposerAdditive(EngineDeviceContext dc)
        {
            dc.SetBlendState(blendDeferredComposerAdditive);
        }
        /// <summary>
        /// Sets deferred lighting blend state
        /// </summary>
        /// <param name="dc">Device context</param>
        private void SetBlendDeferredLighting(EngineDeviceContext dc)
        {
            dc.SetBlendState(blendDeferredLighting);
        }
    }
}
