#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    using Engine.BuiltIn.PostProcess;

    /// <summary>
    /// Base scene renderer
    /// </summary>
    public abstract class BaseSceneRenderer : ISceneRenderer
    {
        /// <summary>
        /// Post-processing effect descriptor
        /// </summary>
        struct PostProcessingEffect
        {
            /// <summary>
            /// Render pass
            /// </summary>
            public RenderPass RenderPass { get; set; }
            /// <summary>
            /// State
            /// </summary>
            public BuiltInPostProcessState State { get; set; }
        }

        /// <summary>
        /// Render targets
        /// </summary>
        protected enum Targets
        {
            /// <summary>
            /// Objects target
            /// </summary>
            /// <remarks>
            /// All scene objects and their post-processing effects are drawn to this target
            /// </remarks>
            Objects,
            /// <summary>
            /// UI target
            /// </summary>
            /// <remarks>
            /// All user interface components and their post-processing effects are drawn to this target
            /// </remarks>
            UI,
            /// <summary>
            /// Results target
            /// </summary>
            Result,
            /// <summary>
            /// Screen
            /// </summary>
            Screen,
        }

        /// <summary>
        /// Post-processing drawer
        /// </summary>
        private readonly IPostProcessingDrawer processingDrawer = null;
        /// <summary>
        /// Scene objects target
        /// </summary>
        private RenderTarget sceneObjectsTarget = null;
        /// <summary>
        /// Scene UI target
        /// </summary>
        private RenderTarget sceneUITarget = null;
        /// <summary>
        /// Scene results target
        /// </summary>
        private RenderTarget sceneResultsTarget = null;
        /// <summary>
        /// Post-processing render target A
        /// </summary>
        private RenderTarget postProcessingTargetA = null;
        /// <summary>
        /// Post-processing render target B
        /// </summary>
        private RenderTarget postProcessingTargetB = null;
        /// <summary>
        /// Post-processing effects
        /// </summary>
        private PostProcessingEffect postProcessingEffect;

        /// <summary>
        /// Shadow map size
        /// </summary>
        protected const int DirectionalShadowMapSize = 1024 * 4;
        /// <summary>
        /// Maximum number of directional shadow maps
        /// </summary>
        protected const int MaxDirectionalShadowMaps = 1;
        /// <summary>
        /// Maximum number of cascade shadow maps per directional light
        /// </summary>
        protected const int MaxDirectionalCascadeShadowMaps = 3;

        /// <summary>
        /// Cubic shadow map size
        /// </summary>
        protected const int CubicShadowMapSize = 1024;
        /// <summary>
        /// Maximum number of cubic shadow maps
        /// </summary>
        protected const int MaxCubicShadows = 16;

        /// <summary>
        /// Spot light shadow map size
        /// </summary>
        protected const int SpotShadowMapSize = 1024;
        /// <summary>
        /// Max spot shadows
        /// </summary>
        protected const int MaxSpotShadows = 16;

        /// <summary>
        /// Cull index for drawing
        /// </summary>
        protected const int CullIndexDrawIndex = 0;
        /// <summary>
        /// Cull index for low definition shadows
        /// </summary>
        protected const int CullIndexShadowMaps = 100;

        /// <summary>
        /// Shadow mapper for directional lights
        /// </summary>
        protected IShadowMap ShadowMapperDirectional { get; private set; }
        /// <summary>
        /// Cube shadow mapper for point lights
        /// </summary>
        protected IShadowMap ShadowMapperPoint { get; private set; }
        /// <summary>
        /// Shadow mapper for spot lights
        /// </summary>
        protected IShadowMap ShadowMapperSpot { get; private set; }

        /// <summary>
        /// Scene
        /// </summary>
        protected Scene Scene;
        /// <summary>
        /// Renderer width
        /// </summary>
        protected int Width;
        /// <summary>
        /// Renderer height
        /// </summary>
        protected int Height;
        /// <summary>
        /// Update context
        /// </summary>
        protected UpdateContext UpdateContext = null;
        /// <summary>
        /// Draw context
        /// </summary>
        protected DrawContext DrawContext = null;
        /// <summary>
        /// Context for shadow map drawing
        /// </summary>
        protected DrawContextShadows DrawShadowsContext = null;
        /// <summary>
        /// Cull manager
        /// </summary>
        protected SceneCullManager cullManager = null;
        /// <summary>
        /// Directional shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapDirectional
        {
            get
            {
                return ShadowMapperDirectional?.Texture;
            }
        }
        /// <summary>
        /// Point lights shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapPoint
        {
            get
            {
                return ShadowMapperPoint?.Texture;
            }
        }
        /// <summary>
        /// Spot lights shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapSpot
        {
            get
            {
                return ShadowMapperSpot?.Texture;
            }
        }
        /// <summary>
        /// Gets or sets whether the renderer was updated
        /// </summary>
        protected bool Updated { get; set; }

        /// <summary>
        /// Gets or sets whether the post processing effect is enabled.
        /// </summary>
        public bool PostProcessingEnabled { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        protected BaseSceneRenderer(Scene scene)
        {
            Scene = scene;

            // Directional shadow mapper
            ShadowMapperDirectional = new ShadowMapCascade(
                scene,
                "Directional Shadow Mapper",
                DirectionalShadowMapSize,
                MaxDirectionalCascadeShadowMaps, MaxDirectionalShadowMaps,
                scene.GameEnvironment.CascadeShadowMapsDistances)
            {
                HighResolutionMap = true
            };

            // Point shadow mapper
            ShadowMapperPoint = new ShadowMapPoint(
                scene,
                "Point Shadow Mapper",
                CubicShadowMapSize, CubicShadowMapSize,
                MaxCubicShadows)
            {
                HighResolutionMap = true
            };

            // Spot shadow mapper
            ShadowMapperSpot = new ShadowMapSpot(
                scene,
                "Spot Shadow Mapper",
                SpotShadowMapSize, SpotShadowMapSize,
                MaxSpotShadows)
            {
                HighResolutionMap = true
            };

            cullManager = new SceneCullManager();

            UpdateContext = new UpdateContext()
            {
                Name = "Primary",
            };

            DrawContext = new DrawContext()
            {
                Name = "Primary",
                DrawerMode = DrawerModes.Forward,
                Form = scene.Game.Form,
            };

            DrawShadowsContext = new DrawContextShadows()
            {
                Name = "Shadow mapping",
            };

            var targetFormat = SharpDX.DXGI.Format.R32G32B32A32_Float;

            sceneObjectsTarget = new RenderTarget(scene.Game, "SceneObjectsTarget", targetFormat, false, 1);
            sceneUITarget = new RenderTarget(scene.Game, "SceneUITarget", targetFormat, false, 1);
            sceneResultsTarget = new RenderTarget(scene.Game, "SceneResultsTarget", targetFormat, false, 1);

            postProcessingTargetA = new RenderTarget(scene.Game, "PostProcessingTargetA", targetFormat, false, 1);
            postProcessingTargetB = new RenderTarget(scene.Game, "PostProcessingTargetB", targetFormat, false, 1);
            processingDrawer = new PostProcessingDrawer(scene.Game.Graphics, scene.Game.BufferManager);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BaseSceneRenderer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ShadowMapperDirectional?.Dispose();
                ShadowMapperDirectional = null;

                ShadowMapperPoint?.Dispose();
                ShadowMapperPoint = null;

                ShadowMapperSpot?.Dispose();
                ShadowMapperSpot = null;

                sceneObjectsTarget?.Dispose();
                sceneObjectsTarget = null;
                sceneUITarget?.Dispose();
                sceneUITarget = null;
                sceneResultsTarget?.Dispose();
                sceneResultsTarget = null;
                postProcessingTargetA?.Dispose();
                postProcessingTargetA = null;
                postProcessingTargetB?.Dispose();
                postProcessingTargetB = null;
            }
        }

        /// <summary>
        /// Resizes buffers
        /// </summary>
        public virtual void Resize()
        {
            sceneObjectsTarget?.Resize();
            sceneUITarget?.Resize();
            sceneResultsTarget?.Resize();
            postProcessingTargetA?.Resize();
            postProcessingTargetB?.Resize();
            processingDrawer?.Resize();
        }
        /// <summary>
        /// Gets renderer resources
        /// </summary>
        /// <param name="result">Resource type</param>
        /// <returns>Returns renderer specified resource, if renderer produces that resource.</returns>
        public virtual EngineShaderResourceView GetResource(SceneRendererResults result)
        {
            if (result == SceneRendererResults.ShadowMapDirectional) return ShadowMapDirectional;
            if (result == SceneRendererResults.ShadowMapPoint) return ShadowMapPoint;
            if (result == SceneRendererResults.ShadowMapSpot) return ShadowMapSpot;
            return null;
        }

        /// <summary>
        /// Updates scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            Matrix viewProj = Scene.Camera.View * Scene.Camera.Projection;
            UpdateContext.GameTime = gameTime;
            UpdateContext.View = Scene.Camera.View;
            UpdateContext.Projection = Scene.Camera.Projection;
            UpdateContext.NearPlaneDistance = Scene.Camera.NearPlaneDistance;
            UpdateContext.FarPlaneDistance = Scene.Camera.FarPlaneDistance;
            UpdateContext.ViewProjection = viewProj;
            UpdateContext.EyePosition = Scene.Camera.Position;
            UpdateContext.EyeDirection = Scene.Camera.Direction;
            UpdateContext.Lights = Scene.Lights;
            UpdateContext.CameraVolume = new IntersectionVolumeFrustum(viewProj);

            //Cull lights
            Scene.Lights.Cull(UpdateContext.CameraVolume, UpdateContext.EyePosition, Scene.GameEnvironment.LODDistanceLow);

            //Update active components
            var updatables = Scene
                .GetComponents<IUpdatable>()
                .Where(c => c.Active);

            if (updatables.Any())
            {
                updatables
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(EarlyUpdateCall);

                updatables
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(UpdateCall);

                updatables
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(LateUpdateCall);
            }

            Updated = true;
        }
        /// <summary>
        /// Early update loop call
        /// </summary>
        /// <param name="c">Component</param>
        private void EarlyUpdateCall(IUpdatable c)
        {
            c.EarlyUpdate(UpdateContext);
        }
        /// <summary>
        /// Update loop call
        /// </summary>
        /// <param name="c">Component</param>
        private void UpdateCall(IUpdatable c)
        {
            c.Update(UpdateContext);
        }
        /// <summary>
        /// Late update loop call
        /// </summary>
        /// <param name="c">Component</param>
        private void LateUpdateCall(IUpdatable c)
        {
            c.LateUpdate(UpdateContext);
        }

        /// <summary>
        /// Draws scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        public abstract void Draw(GameTime gameTime);

        /// <summary>
        /// Update renderer globals
        /// </summary>
        public virtual void UpdateGlobals()
        {
            ShadowMapperDirectional.UpdateGlobals();
            ShadowMapperPoint.UpdateGlobals();
            ShadowMapperSpot.UpdateGlobals();
        }

        /// <summary>
        /// Gets opaque components
        /// </summary>
        /// <param name="index">Cull index</param>
        /// <param name="components">Component list</param>
        /// <returns>Returns the opaque components</returns>
        protected virtual List<IDrawable> GetOpaques(int index, IEnumerable<IDrawable> components)
        {
            var opaques = components.Where(c =>
            {
                if (!(c is IDrawable)) return false;

                if (!c.BlendMode.HasFlag(BlendModes.Opaque)) return false;

                if (c is ICullable cull)
                {
                    return !cullManager.GetCullValue(index, cull).Culled;
                }

                return true;
            });

            return opaques.ToList();
        }
        /// <summary>
        /// Sorting opaque list comparer
        /// </summary>
        /// <param name="index">Cull index</param>
        /// <param name="c1">First component</param>
        /// <param name="c2">Second component</param>
        /// <returns>Returns sorting order (nearest first)</returns>
        protected virtual int SortOpaques(int index, IDrawable c1, IDrawable c2)
        {
            int res = c1.Layer.CompareTo(c2.Layer);

            if (res == 0)
            {
                res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
            }

            if (res == 0)
            {
                float d1 = float.MaxValue;
                if (c1 is ICullable cull1)
                {
                    d1 = cullManager.GetCullValue(index, cull1).Distance;
                }

                float d2 = float.MaxValue;
                if (c2 is ICullable cull2)
                {
                    d2 = cullManager.GetCullValue(index, cull2).Distance;
                }

                // Nearest first
                res = -d1.CompareTo(d2);
            }

            if (res == 0)
            {
                res = c1.BlendMode.CompareTo(c2.BlendMode);
            }

            return res;
        }
        /// <summary>
        /// Gets transparent components
        /// </summary>
        /// <param name="index">Cull index</param>
        /// <param name="components">Component list</param>
        /// <returns>Returns the transparent components</returns>
        protected virtual List<IDrawable> GetTransparents(int index, IEnumerable<IDrawable> components)
        {
            var transparents = components.Where(c =>
            {
                if (!(c is IDrawable)) return false;

                if (!c.BlendMode.HasFlag(BlendModes.Alpha) && !c.BlendMode.HasFlag(BlendModes.Transparent)) return false;

                if (c is ICullable cull)
                {
                    return !cullManager.GetCullValue(index, cull).Culled;
                }

                return true;
            });

            return transparents.ToList();
        }
        /// <summary>
        /// Sorting transparent list comparer
        /// </summary>
        /// <param name="index">Cull index</param>
        /// <param name="c1">First component</param>
        /// <param name="c2">Second component</param>
        /// <returns>Returns sorting order (far first)</returns>
        protected virtual int SortTransparents(int index, IDrawable c1, IDrawable c2)
        {
            int res = c1.Layer.CompareTo(c2.Layer);

            if (res == 0)
            {
                res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
            }

            if (res == 0)
            {
                float d1 = float.MaxValue;
                if (c1 is ICullable cull1)
                {
                    d1 = cullManager.GetCullValue(index, cull1).Distance;
                }

                float d2 = float.MaxValue;
                if (c2 is ICullable cull2)
                {
                    d2 = cullManager.GetCullValue(index, cull2).Distance;
                }

                // Far objects first
                res = d1.CompareTo(d2);
            }

            if (res == 0)
            {
                res = c1.BlendMode.CompareTo(c2.BlendMode);
            }

            return res;
        }

        /// <summary>
        /// Draws an object
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="c">Component</param>
        protected virtual void Draw(DrawContext context, IDrawable c)
        {
            if (c is IDrawable drawable)
            {
                Counters.MaxInstancesPerFrame += c.InstanceCount;

                BlendModes blend = c.BlendMode;
                if (c.Usage.HasFlag(SceneObjectUsages.UI))
                {
                    blend |= BlendModes.PostProcess;
                }

                SetRasterizer(context);

                SetBlendState(context, blend);

                SetDepthStencil(context, c.DepthEnabled);

                drawable.Draw(context);
            }
        }
        /// <summary>
        /// Sets the rasterizer state
        /// </summary>
        /// <param name="context">Drawing context</param>
        protected virtual void SetRasterizer(DrawContext context)
        {
            Scene.Game.Graphics.SetRasterizerDefault();
        }
        /// <summary>
        /// Sets the blend state
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="blendMode">Blend mode</param>
        protected virtual void SetBlendState(DrawContext context, BlendModes blendMode)
        {
            Scene.Game.Graphics.SetBlendState(blendMode);
        }
        /// <summary>
        /// Sets the depth-stencil buffer state
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="enable">Enables the z-buffer</param>
        protected virtual void SetDepthStencil(DrawContext context, bool enable)
        {
            if (enable)
            {
                Scene.Game.Graphics.SetDepthStencilWRZEnabled();
            }
            else
            {
                Scene.Game.Graphics.SetDepthStencilWRZDisabled();
            }
        }

        /// <summary>
        /// Draw shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        protected virtual void DoShadowMapping(GameTime gameTime)
        {
            int cullIndex = CullIndexShadowMaps;

            DoDirectionalShadowMapping(gameTime, ref cullIndex);

            DoPointShadowMapping(gameTime, ref cullIndex);

            DoSpotShadowMapping(gameTime, ref cullIndex);
        }
        /// <summary>
        /// Draw directional shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoDirectionalShadowMapping(GameTime gameTime, ref int cullIndex)
        {
#if DEBUG
            Dictionary<string, double> dict = new Dictionary<string, double>();

            Stopwatch gStopwatch = new Stopwatch();
            gStopwatch.Start();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            var shadowCastingLights = Scene.Lights.GetDirectionalShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
#if DEBUG
            stopwatch.Stop();
            dict.Add($"DoDirectionalShadowMapping Getting lights", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowCastingLights.Any())
            {
                return;
            }

            //Objects that cast shadows
#if DEBUG
            stopwatch.Restart();
#endif
            var shadowObjs = Scene.GetComponents<IDrawable>()
                .Where(c => c.Visible && c.CastShadow);
#if DEBUG
            stopwatch.Stop();
            dict.Add($"DoDirectionalShadowMapping Getting components", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowObjs.Any())
            {
                return;
            }

            //Objects that cast shadows and suitable for culling test
            var toCullShadowObjs = shadowObjs.OfType<ICullable>();
            if (toCullShadowObjs.Any())
            {
                //All objects suitable for culling
                bool allCullingObjects = shadowObjs.Count() == toCullShadowObjs.Count();
                var camVolume = DrawContext.CameraVolume;

#if DEBUG
                stopwatch.Restart();
#endif
                var shadowSph = new IntersectionVolumeSphere(camVolume.Position, camVolume.Radius);
                var doShadows = cullManager.Cull(shadowSph, cullIndex, toCullShadowObjs);
#if DEBUG
                stopwatch.Stop();
                dict.Add($"DoDirectionalShadowMapping - Cull {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

                if (allCullingObjects && !doShadows)
                {
                    //All objects suitable for culling but no one pass the culling test
                    return;
                }
            }

            var graphics = Scene.Game.Graphics;
            int assigned = 0;

            int l = 0;
            foreach (var light in shadowCastingLights)
            {
                light.ClearShadowParameters();

                if (assigned >= MaxDirectionalShadowMaps)
                {
                    continue;
                }

                //Draw shadows
#if DEBUG
                stopwatch.Restart();
#endif
                var shadowMapper = DrawShadowsContext.ShadowMap = ShadowMapperDirectional;
                shadowMapper.UpdateFromLightViewProjection(Scene.Camera, light);
                shadowMapper.Bind(graphics, assigned * MaxDirectionalCascadeShadowMaps);
                DrawShadowsContext.EyePosition = shadowMapper.LightPosition;
                DrawShadowsContext.ViewProjection = shadowMapper.ToShadowMatrix;
                DrawShadowComponents(DrawShadowsContext, cullIndex, shadowObjs);
#if DEBUG
                stopwatch.Stop();
                dict.Add($"DoDirectionalShadowMapping {l} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

                //Assign light parameters
                light.ShadowMapIndex = assigned;
                light.ShadowMapCount++;

                assigned++;

                cullIndex++;

                l++;
            }

#if DEBUG
            gStopwatch.Stop();
            dict.Add($"DoDirectionalShadowMapping TOTAL", gStopwatch.Elapsed.TotalMilliseconds);

            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(dict);
            }
#endif
        }
        /// <summary>
        /// Draw point light shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoPointShadowMapping(GameTime gameTime, ref int cullIndex)
        {
#if DEBUG
            Dictionary<string, double> dict = new Dictionary<string, double>();

            Stopwatch gStopwatch = new Stopwatch();
            gStopwatch.Start();

            Stopwatch stopwatch = new Stopwatch();
#endif
            var shadowCastingLights = Scene.Lights.GetPointShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
#if DEBUG
            stopwatch.Stop();
            dict.Add($"DoPointShadowMapping Getting lights", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowCastingLights.Any())
            {
                return;
            }

            //Draw components if drop shadow (opaque)
#if DEBUG
            stopwatch.Restart();
#endif
            var shadowObjs = Scene.GetComponents<IDrawable>()
                .Where(c => c.Visible && c.CastShadow);
#if DEBUG
            stopwatch.Stop();
            dict.Add($"DoPointShadowMapping Getting components", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowObjs.Any())
            {
                return;
            }

            var toCullShadowObjs = shadowObjs.OfType<ICullable>();

            //All objects suitable for culling
            bool allCullingObjects = shadowObjs.Count() == toCullShadowObjs.Count();

            var graphics = Scene.Game.Graphics;
            int assigned = 0;

            int l = 0;
            foreach (var light in shadowCastingLights)
            {
                light.ClearShadowParameters();

                if (assigned >= MaxCubicShadows)
                {
                    continue;
                }

                cullIndex++;
                l++;

                //Cull test
#if DEBUG
                stopwatch.Restart();
#endif
                var sph = new IntersectionVolumeSphere(light.Position, light.Radius);
                var doShadows = cullManager.Cull(sph, cullIndex, toCullShadowObjs);
#if DEBUG
                stopwatch.Stop();
                dict.Add($"DoPointShadowMapping {l} - Cull {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

                if (allCullingObjects && !doShadows)
                {
                    //All objects suitable for culling but no one pass the culling test
                    continue;
                }

                //Draw shadows
#if DEBUG
                stopwatch.Restart();
#endif
                var shadowMapper = DrawShadowsContext.ShadowMap = ShadowMapperPoint;
                shadowMapper.UpdateFromLightViewProjection(Scene.Camera, light);
                shadowMapper.Bind(graphics, assigned);
                DrawShadowsContext.EyePosition = shadowMapper.LightPosition;
                DrawShadowsContext.ViewProjection = shadowMapper.ToShadowMatrix;
                DrawShadowComponents(DrawShadowsContext, cullIndex, shadowObjs);
#if DEBUG
                stopwatch.Stop();
                dict.Add($"DoPointShadowMapping {l} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

                //Assign light parameters
                light.ShadowMapIndex = assigned;

                assigned++;
            }

#if DEBUG
            gStopwatch.Stop();
            dict.Add($"DoPointShadowMapping TOTAL", gStopwatch.Elapsed.TotalMilliseconds);

            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(dict);
            }
#endif
        }
        /// <summary>
        /// Draw spot light shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoSpotShadowMapping(GameTime gameTime, ref int cullIndex)
        {
#if DEBUG
            Dictionary<string, double> dict = new Dictionary<string, double>();

            Stopwatch gStopwatch = new Stopwatch();
            gStopwatch.Start();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            var shadowCastingLights = Scene.Lights.GetSpotShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
#if DEBUG
            stopwatch.Stop();
            dict.Add($"DoSpotShadowMapping Getting lights", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowCastingLights.Any())
            {
                return;
            }

            //Draw components if drop shadow (opaque)
#if DEBUG
            stopwatch.Restart();
#endif
            var shadowObjs = Scene.GetComponents<IDrawable>()
                .Where(c => c.Visible && c.CastShadow);
#if DEBUG
            stopwatch.Stop();
            dict.Add($"DoSpotShadowMapping Getting components", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowObjs.Any())
            {
                return;
            }

            var toCullShadowObjs = shadowObjs.OfType<ICullable>();

            //All objects suitable for culling
            bool allCullingObjects = shadowObjs.Count() == toCullShadowObjs.Count();

            var graphics = Scene.Game.Graphics;
            int assigned = 0;

            int l = 0;
            foreach (var light in shadowCastingLights)
            {
                light.ClearShadowParameters();

                if (assigned >= MaxCubicShadows)
                {
                    continue;
                }

                //Cull test
#if DEBUG
                stopwatch.Restart();
#endif
                var sph = new IntersectionVolumeSphere(light.Position, light.Radius);
                var doShadows = cullManager.Cull(sph, cullIndex, toCullShadowObjs);
#if DEBUG
                stopwatch.Stop();
                dict.Add($"DoSpotShadowMapping {l} - Cull {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

                if (allCullingObjects && !doShadows)
                {
                    //All objects suitable for culling but no one pass the culling test
                    continue;
                }

                //Draw shadows
#if DEBUG
                stopwatch.Restart();
#endif
                var shadowMapper = DrawShadowsContext.ShadowMap = ShadowMapperSpot;
                shadowMapper.UpdateFromLightViewProjection(Scene.Camera, light);
                shadowMapper.Bind(graphics, assigned);
                DrawShadowsContext.EyePosition = shadowMapper.LightPosition;
                DrawShadowsContext.ViewProjection = shadowMapper.ToShadowMatrix;
                DrawShadowComponents(DrawShadowsContext, cullIndex, shadowObjs);
#if DEBUG
                stopwatch.Stop();
                dict.Add($"DoSpotShadowMapping {l} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

                //Assign light parameters
                light.FromLightVP = shadowMapper.FromLightViewProjectionArray;
                light.ShadowMapIndex = assigned;
                light.ShadowMapCount = 1;

                assigned++;

                cullIndex++;

                l++;
            }

#if DEBUG
            gStopwatch.Stop();
            dict.Add($"DoSpotShadowMapping TOTAL", gStopwatch.Elapsed.TotalMilliseconds);

            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(dict);
            }
#endif
        }

        /// <summary>
        /// Draw components for shadow mapping
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="index">Culling index</param>
        /// <param name="components">Components to draw</param>
        protected void DrawShadowComponents(DrawContextShadows context, int index, IEnumerable<IDrawable> components)
        {
            var graphics = Scene.Game.Graphics;

            var objects = components
                .Where(c => IsVisible(c, index)).ToList();
            if (objects.Any())
            {
                objects.Sort((c1, c2) => Sort(c1, c2, index));

                objects.ForEach((c) => DrawShadows(graphics, context, c));
            }
        }
        /// <summary>
        /// Gets if the specified object is not culled by the cull index
        /// </summary>
        /// <param name="c">Scene object</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns>Returns true if the object is not culled</returns>
        private bool IsVisible(IDrawable c, int cullIndex)
        {
            if (!(c is IDrawable)) return false;

            if (c is ICullable cull)
            {
                return !cullManager.GetCullValue(cullIndex, cull).Culled;
            }

            return true;
        }
        /// <summary>
        /// Sorts an object list by distance to culling point of view
        /// </summary>
        /// <param name="c1">Scene object one</param>
        /// <param name="c2">Scene object two</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns></returns>
        private int Sort(IDrawable c1, IDrawable c2, int cullIndex)
        {
            int res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
            if (res == 0)
            {
                float d1 = float.MaxValue;
                if (c1 is ICullable cull1)
                {
                    d1 = cullManager.GetCullValue(cullIndex, cull1).Distance;
                }

                float d2 = float.MaxValue;
                if (c2 is ICullable cull2)
                {
                    d2 = cullManager.GetCullValue(cullIndex, cull2).Distance;
                }

                res = -d1.CompareTo(d2);
            }

            if (res == 0)
            {
                res = c1.Layer.CompareTo(c2.Layer);
            }

            return res;
        }
        /// <summary>
        /// Draws the specified object shadows
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="context">Context</param>
        /// <param name="c">Scene object</param>
        private void DrawShadows(Graphics graphics, DrawContextShadows context, IDrawable c)
        {
            if (c is IDrawable drawable)
            {
                graphics.SetRasterizerShadowMapping();
                graphics.SetDepthStencilShadowMapping();

                if (c.BlendMode.HasFlag(BlendModes.Alpha) || c.BlendMode.HasFlag(BlendModes.Transparent))
                {
                    graphics.SetBlendAlpha();
                }
                else
                {
                    graphics.SetBlendDefault();
                }

                drawable.DrawShadows(context);
            }
        }

        /// <summary>
        /// Binds graphics for results pass
        /// </summary>
        /// <param name="target">Target type</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        protected virtual void SetTarget(Targets target, bool clearRT, Color4 clearRTColor, bool clearDepth = false, bool clearStencil = false)
        {
            switch (target)
            {
                case Targets.Screen:
                    BindDefaultTarget(clearRT, clearRTColor);
                    break;
                case Targets.Objects:
                    BindObjectsTarget(clearRT, clearRTColor, clearDepth, clearStencil);
                    break;
                case Targets.UI:
                    BindUITarget(clearRT, clearRTColor);
                    break;
                case Targets.Result:
                    BindResultsTarget(clearRT, clearRTColor);
                    break;
                default:
                    BindDefaultTarget(clearRT, clearRTColor);
                    break;
            }
        }
        /// <summary>
        /// Binds the default render target
        /// </summary>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        private void BindDefaultTarget(bool clearRT, Color4 clearRTColor)
        {
            var graphics = Scene.Game.Graphics;

            //Restore backbuffer as render target and clear it
            graphics.SetDefaultRenderTarget(clearRT, clearRTColor);
            graphics.SetDefaultViewport();
        }
        /// <summary>
        /// Binds the objects render target
        /// </summary>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        private void BindObjectsTarget(bool clearRT, Color4 clearRTColor, bool clearDepth, bool clearStencil)
        {
            var graphics = Scene.Game.Graphics;

            graphics.SetRenderTargets(sceneObjectsTarget.Targets, clearRT, clearRTColor, clearDepth, clearStencil);
            graphics.SetDefaultViewport();
        }
        /// <summary>
        /// Binds the UI render target
        /// </summary>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        private void BindUITarget(bool clearRT, Color4 clearRTColor)
        {
            var graphics = Scene.Game.Graphics;

            graphics.SetRenderTargets(sceneUITarget.Targets, clearRT, clearRTColor);
            graphics.SetDefaultViewport();
        }
        /// <summary>
        /// Binds the results render target
        /// </summary>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        private void BindResultsTarget(bool clearRT, Color4 clearRTColor)
        {
            var graphics = Scene.Game.Graphics;

            graphics.SetRenderTargets(sceneResultsTarget.Targets, clearRT, clearRTColor);
            graphics.SetDefaultViewport();
        }
        /// <summary>
        /// Binds graphics for post-processing pass
        /// </summary>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        private void BindPostProcessingTarget(bool clearRT, Color4 clearRTColor)
        {
            var graphics = Scene.Game.Graphics;

            graphics.SetRenderTargets(postProcessingTargetA.Targets, clearRT, clearRTColor);

            //Set local viewport
            var viewport = Scene.Game.Form.GetViewport();
            graphics.SetViewport(viewport);
        }
        /// <summary>
        /// Toggles post-processing render targets
        /// </summary>
        private void TogglePostProcessingTargets()
        {
            var tmp = postProcessingTargetA;
            postProcessingTargetA = postProcessingTargetB;
            postProcessingTargetB = tmp;
        }
        /// <summary>
        /// Does the post-processing draw
        /// </summary>
        /// <param name="target">Target to set restul</param>
        /// <param name="renderPass">Render pass</param>
        /// <param name="gameTime">Game time</param>
        protected virtual bool DoPostProcessing(Targets target, RenderPass renderPass, GameTime gameTime)
        {
            if (!PostProcessingEnabled)
            {
                return false;
            }

            if (postProcessingEffect.RenderPass != renderPass)
            {
                return false;
            }

            //Gets the last used target texture
            var texture = GetTargetTextures(target)?.FirstOrDefault();

            var graphics = Scene.Game.Graphics;

            graphics.SetRasterizerCullNone();
            graphics.SetDepthStencilNone();
            graphics.SetBlendDefault();

            var drawer = processingDrawer.UpdateEffectParameters(postProcessingEffect.State);
            if (drawer == null)
            {
                return false;
            }

            var activeEffects = postProcessingEffect.State.GetEffects();

            for (int i = 0; i < activeEffects.Count(); i++)
            {
                var effect = activeEffects.ElementAt(i);
                if (effect == BuiltInPostProcessEffects.None)
                {
                    break;
                }

                //Toggles post-processing buffers
                TogglePostProcessingTargets();

                //Use the next buffer as render target
                BindPostProcessingTarget(false, Color.Transparent);

                processingDrawer.UpdateEffect(texture, effect);
                processingDrawer.Draw(drawer);

                //Gets the source texture
                texture = postProcessingTargetA.Textures?.FirstOrDefault();
            }

            //Set the result render target
            SetTarget(target, false, Color.Transparent);

            //Draw the result
            var resultDrawer = processingDrawer.UpdateEffect(texture, BuiltInPostProcessEffects.None);
            processingDrawer.Draw(resultDrawer);

            return true;
        }
        /// <summary>
        /// Gets the target textures
        /// </summary>
        /// <param name="target">Target type</param>
        /// <returns>Returns the target texture list</returns>
        protected virtual IEnumerable<EngineShaderResourceView> GetTargetTextures(Targets target)
        {
            switch (target)
            {
                case Targets.Screen:
                    return Enumerable.Empty<EngineShaderResourceView>();
                case Targets.Objects:
                    return sceneObjectsTarget?.Textures;
                case Targets.UI:
                    return sceneUITarget?.Textures;
                case Targets.Result:
                    return sceneResultsTarget?.Textures;
                default:
                    return Enumerable.Empty<EngineShaderResourceView>();
            }
        }
        /// <summary>
        /// Combine the specified targets into the result target
        /// </summary>
        /// <param name="target1">Target 1</param>
        /// <param name="target2">Target 2</param>
        /// <param name="resultTarget">Result target</param>
        protected virtual void CombineTargets(Targets target1, Targets target2, Targets resultTarget)
        {
            SetTarget(resultTarget, false, Color.Transparent);

            var graphics = Scene.Game.Graphics;

            graphics.SetDepthStencilNone();
            graphics.SetRasterizerDefault();
            graphics.SetBlendDefault();

            var texture1 = GetTargetTextures(target1)?.FirstOrDefault();
            var texture2 = GetTargetTextures(target2)?.FirstOrDefault();

            var drawer = processingDrawer.UpdateEffectCombine(texture1, texture2);
            processingDrawer.Draw(drawer);
        }
        /// <summary>
        /// Draws the specified target to screen
        /// </summary>
        /// <param name="target">Target</param>
        protected virtual void DrawToScreen(Targets target)
        {
            SetTarget(Targets.Screen, false, Color.Transparent);

            var graphics = Scene.Game.Graphics;

            graphics.SetDepthStencilNone();
            graphics.SetRasterizerDefault();
            graphics.SetBlendDefault();

            var texture = GetTargetTextures(target)?.FirstOrDefault();

            var drawer = processingDrawer.UpdateEffect(texture, BuiltInPostProcessEffects.None);
            processingDrawer.Draw(drawer);
        }

        /// <summary>
        /// Sets the post-processing effect
        /// </summary>
        /// <param name="renderPass">Render pass</param>
        /// <param name="state">State</param>
        public void SetPostProcessingEffect(RenderPass renderPass, BuiltInPostProcessState state)
        {
            postProcessingEffect = new PostProcessingEffect
            {
                RenderPass = renderPass,
                State = state,
            };

            PostProcessingEnabled = true;
        }
        /// <summary>
        /// Clears the post-processing effect
        /// </summary>
        public void ClearPostProcessingEffects()
        {
            postProcessingEffect = new PostProcessingEffect();
            PostProcessingEnabled = false;
        }
    }
}
