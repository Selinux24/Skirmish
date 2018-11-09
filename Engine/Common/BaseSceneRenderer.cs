using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Base scene renderer
    /// </summary>
    public abstract class BaseSceneRenderer : ISceneRenderer
    {
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
        /// Shadow map sampling distances
        /// </summary>
        public static float[] CascadeShadowMapsDistances { get; set; } = new[] { 10f, 25f, 50f };

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
        /// Game
        /// </summary>
        protected Game Game;
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
                if (this.ShadowMapperDirectional != null)
                {
                    return this.ShadowMapperDirectional.Texture;
                }

                return null;
            }
        }
        /// <summary>
        /// Point lights shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapPoint
        {
            get
            {
                if (this.ShadowMapperPoint != null)
                {
                    return this.ShadowMapperPoint.Texture;
                }

                return null;
            }
        }
        /// <summary>
        /// Spot lights shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapSpot
        {
            get
            {
                if (this.ShadowMapperSpot != null)
                {
                    return this.ShadowMapperSpot.Texture;
                }

                return null;
            }
        }
        /// <summary>
        /// Gets or sets whether the renderer was updated
        /// </summary>
        protected bool Updated { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        protected BaseSceneRenderer(Game game)
        {
            this.Game = game;

            // Directional shadow mapper
            this.ShadowMapperDirectional = new ShadowMapCascade(game,
                DirectionalShadowMapSize,
                MaxDirectionalCascadeShadowMaps, MaxDirectionalShadowMaps,
                CascadeShadowMapsDistances);

            // Point shadow mapper
            this.ShadowMapperPoint = new ShadowMapPoint(game,
                CubicShadowMapSize, CubicShadowMapSize,
                MaxCubicShadows);

            // Spot shadow mapper
            this.ShadowMapperSpot = new ShadowMapSpot(game,
                SpotShadowMapSize, SpotShadowMapSize,
                MaxSpotShadows);

            this.cullManager = new SceneCullManager();

            this.UpdateContext = new UpdateContext()
            {
                Name = "Primary",
            };

            this.DrawContext = new DrawContext()
            {
                Name = "Primary",
                DrawerMode = DrawerModes.Forward,
            };

            this.DrawShadowsContext = new DrawContextShadows()
            {
                Name = "Shadow mapping",
            };
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
                if (this.ShadowMapperDirectional != null)
                {
                    this.ShadowMapperDirectional.Dispose();
                    this.ShadowMapperDirectional = null;
                }

                if (this.ShadowMapperPoint != null)
                {
                    this.ShadowMapperPoint.Dispose();
                    this.ShadowMapperPoint = null;
                }

                if (this.ShadowMapperSpot != null)
                {
                    this.ShadowMapperSpot.Dispose();
                    this.ShadowMapperSpot = null;
                }
            }
        }

        /// <summary>
        /// Resizes buffers
        /// </summary>
        public virtual void Resize()
        {

        }
        /// <summary>
        /// Gets renderer resources
        /// </summary>
        /// <param name="result">Resource type</param>
        /// <returns>Returns renderer specified resource, if renderer produces that resource.</returns>
        public virtual EngineShaderResourceView GetResource(SceneRendererResults result)
        {
            if (result == SceneRendererResults.ShadowMapDirectional) return this.ShadowMapDirectional;
            if (result == SceneRendererResults.ShadowMapPoint) return this.ShadowMapPoint;
            if (result == SceneRendererResults.ShadowMapSpot) return this.ShadowMapSpot;
            return null;
        }
        /// <summary>
        /// Updates scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        public virtual void Update(GameTime gameTime, Scene scene)
        {
#if DEBUG
            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            Matrix viewProj = scene.Camera.View * scene.Camera.Projection;

            this.UpdateContext.GameTime = gameTime;
            this.UpdateContext.View = scene.Camera.View;
            this.UpdateContext.Projection = scene.Camera.Projection;
            this.UpdateContext.NearPlaneDistance = scene.Camera.NearPlaneDistance;
            this.UpdateContext.FarPlaneDistance = scene.Camera.FarPlaneDistance;
            this.UpdateContext.ViewProjection = viewProj;
            this.UpdateContext.EyePosition = scene.Camera.Position;
            this.UpdateContext.EyeDirection = scene.Camera.Direction;
            this.UpdateContext.Lights = scene.Lights;
            this.UpdateContext.CameraVolume = new CullingVolumeCamera(viewProj);

            //Cull lights
            scene.Lights.Cull(this.UpdateContext.CameraVolume, this.UpdateContext.EyePosition);

            //Update active components
            scene.GetComponents<IUpdatable>(c => c.Active)
                .ToList().ForEach(c => c.Update(this.UpdateContext));

            this.Updated = true;
#if DEBUG
            swTotal.Stop();
#endif
#if DEBUG
            Counters.SetStatistics("Scene.Update", string.Format("Update = {0:000000}", swTotal.ElapsedTicks));
#endif
        }
        /// <summary>
        /// Draws scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        public abstract void Draw(GameTime gameTime, Scene scene);

        /// <summary>
        /// Draw shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        protected virtual void DoShadowMapping(GameTime gameTime, Scene scene)
        {
            int cullIndex = CullIndexShadowMaps;

            Dictionary<string, double> dict = new Dictionary<string, double>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            DoDirectionalShadowMapping(gameTime, scene, ref cullIndex);
            stopwatch.Stop();
            dict.Add("DoDirectionalShadowMapping", stopwatch.Elapsed.TotalMilliseconds);

            stopwatch.Restart();
            DoPointShadowMapping(gameTime, scene, ref cullIndex);
            stopwatch.Stop();
            dict.Add("DoPointShadowMapping", stopwatch.Elapsed.TotalMilliseconds);

            stopwatch.Restart();
            DoSpotShadowMapping(gameTime, scene, ref cullIndex);
            stopwatch.Stop();
            dict.Add("DoSpotShadowMapping", stopwatch.Elapsed.TotalMilliseconds);

            if (this.Game.TakeFrameShoot)
            {
                foreach (var item in dict)
                {
                    this.Game.FrameShoot.Add(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// Draw directional shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoDirectionalShadowMapping(GameTime gameTime, Scene scene, ref int cullIndex)
        {
            var shadowCastingLights = scene.Lights.GetDirectionalShadowCastingLights();
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

                var shadowObjs = scene.GetComponents(c => c.Visible && c.CastShadow);
                if (shadowObjs.Count > 0)
                {
                    this.DrawShadowsContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawShadowsContext.EyePosition = this.DrawContext.EyePosition;

                    var toCullShadowObjs = shadowObjs.Where(s => s.Is<ICullable>()).Select(s => s.Get<ICullable>());

                    int assigned = 0;

                    for (int l = 0; l < shadowCastingLights.Length; l++)
                    {
                        var light = shadowCastingLights[l];
                        light.ShadowMapIndex = -1;
                        light.ShadowMapCount = 0;
                        light.ToShadowSpace = Matrix.Identity;
                        light.ToCascadeOffsetX = Vector4.Zero;
                        light.ToCascadeOffsetY = Vector4.Zero;
                        light.ToCascadeScale = Vector4.Zero;

                        if (assigned < MaxDirectionalShadowMaps)
                        {
                            var camVolume = this.DrawContext.CameraVolume;
                            var shadowSph = new CullingVolumeSphere(camVolume.Position, camVolume.Radius);

                            var doShadows = this.cullManager.Cull(shadowSph, cullIndex, toCullShadowObjs);
                            if (doShadows)
                            {
                                var shadowMapper = this.DrawShadowsContext.ShadowMap = this.ShadowMapperDirectional;

                                shadowMapper.UpdateFromLightViewProjection(scene.Camera, light);
                                shadowMapper.Bind(graphics, l * MaxDirectionalCascadeShadowMaps);

                                light.ShadowMapIndex = assigned;
                                light.ShadowMapCount++;

                                this.DrawShadowComponents(gameTime, this.DrawShadowsContext, cullIndex, shadowObjs);
                            }

                            assigned++;

                            cullIndex++;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Draw point light shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoPointShadowMapping(GameTime gameTime, Scene scene, ref int cullIndex)
        {
            var shadowCastingLights = scene.Lights.GetPointShadowCastingLights(scene.Camera.Position);
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

                //Draw components if drop shadow (opaque)
                var shadowObjs = scene.GetComponents(c => c.Visible && c.CastShadow);
                if (shadowObjs.Count > 0)
                {
                    var toCullShadowObjs = shadowObjs.Where(s => s.Is<ICullable>()).Select(s => s.Get<ICullable>());

                    int assigned = 0;

                    for (int l = 0; l < shadowCastingLights.Length; l++)
                    {
                        var light = shadowCastingLights[l];
                        light.ShadowMapIndex = -1;

                        if (assigned < MaxCubicShadows)
                        {
                            var sph = new CullingVolumeSphere(light.Position, light.Radius);

                            var doShadows = this.cullManager.Cull(sph, cullIndex, toCullShadowObjs);

                            if (doShadows)
                            {
                                var shadowMapper = this.DrawShadowsContext.ShadowMap = this.ShadowMapperPoint;

                                shadowMapper.UpdateFromLightViewProjection(scene.Camera, light);
                                shadowMapper.Bind(graphics, l);

                                light.ShadowMapIndex = assigned;

                                this.DrawShadowComponents(gameTime, this.DrawShadowsContext, cullIndex, shadowObjs);
                            }

                            assigned++;

                            cullIndex++;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Draw spot light shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoSpotShadowMapping(GameTime gameTime, Scene scene, ref int cullIndex)
        {
            var shadowCastingLights = scene.Lights.GetSpotShadowCastingLights(scene.Camera.Position);
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

                //Draw components if drop shadow (opaque)
                var shadowObjs = scene.GetComponents(c => c.Visible && c.CastShadow);
                if (shadowObjs.Count > 0)
                {
                    var toCullShadowObjs = shadowObjs.Where(s => s.Is<ICullable>()).Select(s => s.Get<ICullable>());

                    int assigned = 0;

                    for (int l = 0; l < shadowCastingLights.Length; l++)
                    {
                        var light = shadowCastingLights[l];
                        light.ShadowMapIndex = -1;
                        light.ShadowMapCount = 0;
                        light.FromLightVP = new Matrix[1];

                        if (assigned < MaxSpotShadows)
                        {
                            var sph = new CullingVolumeSphere(light.Position, light.Radius);

                            var doShadows = this.cullManager.Cull(sph, cullIndex, toCullShadowObjs);

                            if (doShadows)
                            {
                                var shadowMapper = this.DrawShadowsContext.ShadowMap = this.ShadowMapperSpot;

                                shadowMapper.UpdateFromLightViewProjection(scene.Camera, light);
                                shadowMapper.Bind(graphics, l);

                                light.FromLightVP = shadowMapper.FromLightViewProjectionArray;
                                light.ShadowMapIndex = assigned;
                                light.ShadowMapCount = 1;

                                this.DrawShadowComponents(gameTime, this.DrawShadowsContext, cullIndex, shadowObjs);
                            }

                            assigned++;

                            cullIndex++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw components for shadow mapping
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="components">Components</param>
        protected void DrawShadowComponents(GameTime gameTime, DrawContextShadows context, int index, IEnumerable<SceneObject> components)
        {
            var graphics = this.Game.Graphics;

            var objects = components.Where(c =>
            {
                if (!c.Is<Drawable>()) return false;

                var cull = c.Get<ICullable>();
                if (cull != null)
                {
                    return !this.cullManager.GetCullValue(index, cull).Culled;
                }

                return true;
            }).ToList();
            if (objects.Count > 0)
            {
                objects.Sort((c1, c2) =>
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
                        res = c1.Order.CompareTo(c2.Order);
                    }

                    return res;
                });

                objects.ForEach((c) =>
                {
                    graphics.SetRasterizerShadowMapping();
                    graphics.SetDepthStencilShadowMapping();

                    if (c.AlphaEnabled)
                    {
                        graphics.SetBlendTransparent();
                    }
                    else
                    {
                        graphics.SetBlendDefault();
                    }

                    c.Get<IDrawable>().DrawShadows(context);
                });
            }
        }
    }
}
