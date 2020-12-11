using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Engine.Common
{
    using Engine.Effects;

    /// <summary>
    /// Base scene renderer
    /// </summary>
    public abstract class BaseSceneRenderer : ISceneRenderer
    {
        /// <summary>
        /// Post-processing effect descriptor
        /// </summary>
        class PostProcessingEffect
        {
            /// <summary>
            /// Technique
            /// </summary>
            public EngineEffectTechnique Technique { get; set; }
            /// <summary>
            /// Parameters
            /// </summary>
            public IDrawerPostProcessParams Parameters { get; set; }
        }

        /// <summary>
        /// Post-processing render target 1
        /// </summary>
        private RenderTarget postProcessingBuffer1 = null;
        /// <summary>
        /// Post-processing render target 2
        /// </summary>
        private RenderTarget postProcessingBuffer2 = null;
        /// <summary>
        /// Post-processing drawer
        /// </summary>
        private PostProcessingDrawer processingDrawer = null;
        /// <summary>
        /// Post-processing effects
        /// </summary>
        private readonly List<PostProcessingEffect> postProcessingEffects = new List<PostProcessingEffect>();

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
                DirectionalShadowMapSize,
                MaxDirectionalCascadeShadowMaps, MaxDirectionalShadowMaps,
                scene.GameEnvironment.CascadeShadowMapsDistances)
            {
                HighResolutionMap = true
            };

            // Point shadow mapper
            ShadowMapperPoint = new ShadowMapPoint(
                scene,
                CubicShadowMapSize, CubicShadowMapSize,
                MaxCubicShadows)
            {
                HighResolutionMap = true
            };

            // Spot shadow mapper
            ShadowMapperSpot = new ShadowMapSpot(
                scene,
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
            };

            DrawShadowsContext = new DrawContextShadows()
            {
                Name = "Shadow mapping",
            };

            postProcessingBuffer1 = new RenderTarget(scene.Game, SharpDX.DXGI.Format.R32G32B32A32_Float, false, 1);
            postProcessingBuffer2 = new RenderTarget(scene.Game, SharpDX.DXGI.Format.R32G32B32A32_Float, false, 1);
            processingDrawer = new PostProcessingDrawer(scene.Game.Graphics);
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

                postProcessingBuffer1?.Dispose();
                postProcessingBuffer1 = null;
                postProcessingBuffer2?.Dispose();
                postProcessingBuffer2 = null;
                processingDrawer?.Dispose();
                processingDrawer = null;
            }
        }

        /// <summary>
        /// Resizes buffers
        /// </summary>
        public virtual void Resize()
        {
            postProcessingBuffer1?.Resize();
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
            Stopwatch swTotal = Stopwatch.StartNew();

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
            Stopwatch swLights = Stopwatch.StartNew();
            Scene.Lights.Cull(UpdateContext.CameraVolume, UpdateContext.EyePosition, Scene.GameEnvironment.LODDistanceLow);
            swLights.Stop();
            Logger.WriteTrace(this, $"Cull lights in {swLights.ElapsedTicks:0.000000}");

            //Update active components
            Stopwatch swUpdate = Stopwatch.StartNew();
            var updatables = Scene.GetComponents().Where(c => c.Active).OfType<IUpdatable>().ToList();
            if (updatables.Any())
            {
                updatables.ForEach(EarlyUpdateCall);
                updatables.ForEach(UpdateCall);
                updatables.ForEach(LateUpdateCall);
            }
            Updated = true;
            swUpdate.Stop();
            Logger.WriteTrace(this, $"Update active components in {swUpdate.ElapsedTicks:0.000000}");

            swTotal.Stop();
            Logger.WriteTrace(this, $"Scene.Update in {swTotal.ElapsedTicks:0.000000}");
        }
        /// <summary>
        /// Early update loop call
        /// </summary>
        /// <param name="c">Component</param>
        private void EarlyUpdateCall(IUpdatable c)
        {
            Stopwatch swCUpdate = Stopwatch.StartNew();
            c.EarlyUpdate(UpdateContext);
            swCUpdate.Stop();

            Logger.WriteTrace(this, $"Early update component {c} in {swCUpdate.ElapsedTicks:0.000000}");
        }
        /// <summary>
        /// Update loop call
        /// </summary>
        /// <param name="c">Component</param>
        private void UpdateCall(IUpdatable c)
        {
            Stopwatch swCUpdate = Stopwatch.StartNew();
            c.Update(UpdateContext);
            swCUpdate.Stop();

            Logger.WriteTrace(this, $"Update component {c} in {swCUpdate.ElapsedTicks:0.000000}");
        }
        /// <summary>
        /// Late update loop call
        /// </summary>
        /// <param name="c">Component</param>
        private void LateUpdateCall(IUpdatable c)
        {
            Stopwatch swCUpdate = Stopwatch.StartNew();
            c.LateUpdate(UpdateContext);
            swCUpdate.Stop();

            Logger.WriteTrace(this, $"Late update component {c} in {swCUpdate.ElapsedTicks:0.000000}");
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
        protected virtual List<ISceneObject> GetOpaques(int index, IEnumerable<ISceneObject> components)
        {
            var opaques = components.Where(c =>
            {
                if (!(c is Drawable)) return false;

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
        protected virtual int SortOpaques(int index, ISceneObject c1, ISceneObject c2)
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
        protected virtual List<ISceneObject> GetTransparents(int index, IEnumerable<ISceneObject> components)
        {
            var transparents = components.Where(c =>
            {
                if (!(c is Drawable)) return false;

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
        protected virtual int SortTransparents(int index, ISceneObject c1, ISceneObject c2)
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
        protected virtual void Draw(DrawContext context, ISceneObject c)
        {
            if (c is IDrawable drawable)
            {
                Counters.MaxInstancesPerFrame += c.InstanceCount;

                SetRasterizer(context);

                SetBlendState(context, c.BlendMode);

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
            Dictionary<string, double> dict = new Dictionary<string, double>();

            Stopwatch gStopwatch = new Stopwatch();
            gStopwatch.Start();

            //And there were lights
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var shadowCastingLights = Scene.Lights.GetDirectionalShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
            stopwatch.Stop();
            dict.Add($"DoDirectionalShadowMapping Getting lights", stopwatch.Elapsed.TotalMilliseconds);

            if (!shadowCastingLights.Any())
            {
                return;
            }

            //Objects that cast shadows
            stopwatch.Restart();
            var shadowObjs = Scene.GetComponents().Where(c => c.Visible && c.CastShadow);
            stopwatch.Stop();
            dict.Add($"DoDirectionalShadowMapping Getting components", stopwatch.Elapsed.TotalMilliseconds);

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

                stopwatch.Restart();
                var shadowSph = new IntersectionVolumeSphere(camVolume.Position, camVolume.Radius);
                var doShadows = cullManager.Cull(shadowSph, cullIndex, toCullShadowObjs);
                stopwatch.Stop();
                dict.Add($"DoDirectionalShadowMapping - Cull {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);

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
                stopwatch.Restart();
                var shadowMapper = DrawShadowsContext.ShadowMap = ShadowMapperDirectional;
                shadowMapper.UpdateFromLightViewProjection(Scene.Camera, light);
                shadowMapper.Bind(graphics, assigned * MaxDirectionalCascadeShadowMaps);
                DrawShadowsContext.EyePosition = shadowMapper.LightPosition;
                DrawShadowsContext.ViewProjection = shadowMapper.ToShadowMatrix;
                DrawShadowComponents(DrawShadowsContext, cullIndex, shadowObjs);
                stopwatch.Stop();
                dict.Add($"DoDirectionalShadowMapping {l} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);

                //Assign light parameters
                light.ShadowMapIndex = assigned;
                light.ShadowMapCount++;

                assigned++;

                cullIndex++;

                l++;
            }

            gStopwatch.Stop();
            dict.Add($"DoDirectionalShadowMapping TOTAL", gStopwatch.Elapsed.TotalMilliseconds);

            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(dict);
            }
        }
        /// <summary>
        /// Draw point light shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoPointShadowMapping(GameTime gameTime, ref int cullIndex)
        {
            Dictionary<string, double> dict = new Dictionary<string, double>();

            Stopwatch gStopwatch = new Stopwatch();
            gStopwatch.Start();

            //And there were lights
            Stopwatch stopwatch = new Stopwatch();
            var shadowCastingLights = Scene.Lights.GetPointShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
            stopwatch.Stop();
            dict.Add($"DoPointShadowMapping Getting lights", stopwatch.Elapsed.TotalMilliseconds);

            if (!shadowCastingLights.Any())
            {
                return;
            }

            //Draw components if drop shadow (opaque)
            stopwatch.Restart();
            var shadowObjs = Scene.GetComponents().Where(c => c.Visible && c.CastShadow);
            stopwatch.Stop();
            dict.Add($"DoPointShadowMapping Getting components", stopwatch.Elapsed.TotalMilliseconds);

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
                stopwatch.Restart();
                var sph = new IntersectionVolumeSphere(light.Position, light.Radius);
                var doShadows = cullManager.Cull(sph, cullIndex, toCullShadowObjs);
                stopwatch.Stop();
                dict.Add($"DoPointShadowMapping {l} - Cull {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);

                if (allCullingObjects && !doShadows)
                {
                    //All objects suitable for culling but no one pass the culling test
                    continue;
                }

                //Draw shadows
                stopwatch.Restart();
                var shadowMapper = DrawShadowsContext.ShadowMap = ShadowMapperPoint;
                shadowMapper.UpdateFromLightViewProjection(Scene.Camera, light);
                shadowMapper.Bind(graphics, assigned);
                DrawShadowsContext.EyePosition = shadowMapper.LightPosition;
                DrawShadowsContext.ViewProjection = shadowMapper.ToShadowMatrix;
                DrawShadowComponents(DrawShadowsContext, cullIndex, shadowObjs);
                stopwatch.Stop();
                dict.Add($"DoPointShadowMapping {l} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);

                //Assign light parameters
                light.ShadowMapIndex = assigned;

                assigned++;
            }

            gStopwatch.Stop();
            dict.Add($"DoPointShadowMapping TOTAL", gStopwatch.Elapsed.TotalMilliseconds);

            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(dict);
            }
        }
        /// <summary>
        /// Draw spot light shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoSpotShadowMapping(GameTime gameTime, ref int cullIndex)
        {
            Dictionary<string, double> dict = new Dictionary<string, double>();

            Stopwatch gStopwatch = new Stopwatch();
            gStopwatch.Start();

            //And there were lights
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var shadowCastingLights = Scene.Lights.GetSpotShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
            stopwatch.Stop();
            dict.Add($"DoSpotShadowMapping Getting lights", stopwatch.Elapsed.TotalMilliseconds);

            if (!shadowCastingLights.Any())
            {
                return;
            }

            //Draw components if drop shadow (opaque)
            stopwatch.Restart();
            var shadowObjs = Scene.GetComponents().Where(c => c.Visible && c.CastShadow);
            stopwatch.Stop();
            dict.Add($"DoSpotShadowMapping Getting components", stopwatch.Elapsed.TotalMilliseconds);

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
                stopwatch.Restart();
                var sph = new IntersectionVolumeSphere(light.Position, light.Radius);
                var doShadows = cullManager.Cull(sph, cullIndex, toCullShadowObjs);
                stopwatch.Stop();
                dict.Add($"DoSpotShadowMapping {l} - Cull {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);

                if (allCullingObjects && !doShadows)
                {
                    //All objects suitable for culling but no one pass the culling test
                    continue;
                }

                //Draw shadows
                stopwatch.Restart();
                var shadowMapper = DrawShadowsContext.ShadowMap = ShadowMapperSpot;
                shadowMapper.UpdateFromLightViewProjection(Scene.Camera, light);
                shadowMapper.Bind(graphics, assigned);
                DrawShadowsContext.EyePosition = shadowMapper.LightPosition;
                DrawShadowsContext.ViewProjection = shadowMapper.ToShadowMatrix;
                DrawShadowComponents(DrawShadowsContext, cullIndex, shadowObjs);
                stopwatch.Stop();
                dict.Add($"DoSpotShadowMapping {l} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);

                //Assign light parameters
                light.FromLightVP = shadowMapper.FromLightViewProjectionArray;
                light.ShadowMapIndex = assigned;
                light.ShadowMapCount = 1;

                assigned++;

                cullIndex++;

                l++;
            }

            gStopwatch.Stop();
            dict.Add($"DoSpotShadowMapping TOTAL", gStopwatch.Elapsed.TotalMilliseconds);

            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(dict);
            }
        }

        /// <summary>
        /// Draw components for shadow mapping
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="index">Culling index</param>
        /// <param name="components">Components to draw</param>
        protected void DrawShadowComponents(DrawContextShadows context, int index, IEnumerable<ISceneObject> components)
        {
            var graphics = Scene.Game.Graphics;

            var objects = components.Where(c => IsVisible(c, index)).ToList();
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
        private bool IsVisible(ISceneObject c, int cullIndex)
        {
            if (!(c is Drawable)) return false;

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
        private int Sort(ISceneObject c1, ISceneObject c2, int cullIndex)
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
        private void DrawShadows(Graphics graphics, DrawContextShadows context, ISceneObject c)
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
        protected virtual void BindResult()
        {
            if (PostProcessingEnabled)
            {
                BindPostProcessing();
            }
            else
            {
                BindDefault();
            }
        }
        /// <summary>
        /// Binds graphics for post-processing pass
        /// </summary>
        private void BindPostProcessing()
        {
            var graphics = Scene.Game.Graphics;

            var viewport = Scene.Game.Form.GetViewport();

            //Set local viewport
            graphics.SetViewport(viewport);

            //Set light buffer to draw lights
            graphics.SetRenderTargets(
                postProcessingBuffer1.Targets, true, GameEnvironment.Background,
                graphics.DefaultDepthStencil, false, false,
                false);
        }
        /// <summary>
        /// Toggles post-processing buffers
        /// </summary>
        private void TogglePostProcessingBuffers()
        {
            var tmp = postProcessingBuffer1;
            postProcessingBuffer1 = postProcessingBuffer2;
            postProcessingBuffer2 = tmp;
        }
        /// <summary>
        /// Binds the default render target
        /// </summary>
        private void BindDefault()
        {
            var graphics = Scene.Game.Graphics;

            //Restore backbuffer as render target and clear it
            graphics.SetDefaultViewport();
            graphics.SetDefaultRenderTarget(true, false, true);
        }
        /// <summary>
        /// Does the post-processing draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        protected virtual void DoPostProcessing(GameTime gameTime)
        {
            if (!PostProcessingEnabled)
            {
                return;
            }

            if (!postProcessingEffects.Any())
            {
                return;
            }

            Scene.Game.Graphics.SetRasterizerDefault();
            Scene.Game.Graphics.SetBlendState(BlendModes.Default);
            Scene.Game.Graphics.SetDepthStencilWRZDisabled();

            var effect = DrawerPool.EffectPostProcess;
            var viewProj = Scene.Game.Form.GetOrthoProjectionMatrix();
            var screenRect = Scene.Game.Form.RenderRectangle;

            foreach (var postEffect in postProcessingEffects)
            {
                //Sets as effect source the first buffer
                effect.UpdatePerFrame(
                    viewProj,
                    new Vector2(screenRect.Width, screenRect.Height),
                    postProcessingBuffer1.Textures?.FirstOrDefault());

                //Toggles post-processing buffers
                TogglePostProcessingBuffers();

                //Use the second buffer as render target
                BindPostProcessing();

                effect.UpdatePerEffect(postEffect.Parameters);

                processingDrawer.SetDrawer(postEffect.Technique);
                processingDrawer.Bind();
                processingDrawer.Draw();
            }

            //Sets as effect source the last used buffer
            effect.UpdatePerFrame(
                viewProj,
                new Vector2(screenRect.Width, screenRect.Height),
                postProcessingBuffer1.Textures?.FirstOrDefault());

            //Set the default render target
            BindDefault();

            //Draw the result
            processingDrawer.SetDrawer(DrawerPool.EffectPostProcess.Empty);
            processingDrawer.Bind();
            processingDrawer.Draw();
        }

        /// <summary>
        /// Sets the post-processing effect
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="parameters">Parameters</param>
        public void SetPostProcessingEffect(PostProcessingEffects effect, IDrawerPostProcessParams parameters)
        {
            var technique = DrawerPool.EffectPostProcess.GetTechnique(effect);

            postProcessingEffects.Add(new PostProcessingEffect
            {
                Technique = technique,
                Parameters = parameters,
            });

            PostProcessingEnabled = true;
        }
        /// <summary>
        /// Crears the post-processing effect
        /// </summary>
        public void CrearPostProcessingEffects()
        {
            postProcessingEffects.Clear();
            PostProcessingEnabled = false;
        }
    }
}
