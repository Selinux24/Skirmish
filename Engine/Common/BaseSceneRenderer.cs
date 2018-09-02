#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using System;
using System.Collections.Generic;
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
        protected const int DirectionalShadowMapSize = 1024 * 2;
        /// <summary>
        /// Maximum number of directional shadow maps
        /// </summary>
        protected const int MaxDirectionalShadowMaps = 3;
        /// <summary>
        /// Maximum number of shadow maps per light
        /// </summary>
        public const int MaxDirectionalSubshadowMaps = 2;
        /// <summary>
        /// Shadow map sampling distances
        /// </summary>
        public static float[] DirectionalShadowMapDistances = new[] { 50f, 100f };

        /// <summary>
        /// Cubic shadow map size
        /// </summary>
        protected const int CubicShadowMapSize = 1024;
        /// <summary>
        /// Maximum number of cubic shadow maps
        /// </summary>
        protected const int MaxCubicShadows = 8;

        /// <summary>
        /// Spot light shadow map size
        /// </summary>
        protected const int SpotShadowMapSize = 1024;
        /// <summary>
        /// Max spot shadows
        /// </summary>
        protected const int MaxSpotShadows = 8;

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
        protected IShadowMap ShadowMapperOmnidirectional { get; private set; }
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
        /// Omnidirectional Shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapOmnidirectional
        {
            get
            {
                if (this.ShadowMapperOmnidirectional != null)
                {
                    return this.ShadowMapperOmnidirectional.Texture;
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
        public BaseSceneRenderer(Game game)
        {
            this.Game = game;

            this.ShadowMapperDirectional = new ShadowMap(game,
                DirectionalShadowMapSize, DirectionalShadowMapSize,
                MaxDirectionalShadowMaps * MaxDirectionalSubshadowMaps);

            this.ShadowMapperOmnidirectional = new CubicShadowMap(game,
                CubicShadowMapSize, CubicShadowMapSize,
                MaxCubicShadows);

            this.ShadowMapperSpot = new ShadowMap(game,
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
                DrawerMode = DrawerModesEnum.Forward,
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
                Helper.Dispose(this.ShadowMapperDirectional);
                Helper.Dispose(this.ShadowMapperOmnidirectional);
                Helper.Dispose(this.ShadowMapperSpot);
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
        public virtual EngineShaderResourceView GetResource(SceneRendererResultEnum result)
        {
            if (result == SceneRendererResultEnum.ShadowMapDirectional) return this.ShadowMapDirectional;
            if (result == SceneRendererResultEnum.ShadowMapOmnidirectional) return this.ShadowMapOmnidirectional;
            if (result == SceneRendererResultEnum.ShadowMapSpot) return this.ShadowMapSpot;
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
        protected virtual int DoShadowMapping(GameTime gameTime, Scene scene)
        {
            int cullIndex = CullIndexShadowMaps;

            cullIndex = DoDirectionalShadowMapping(gameTime, scene, cullIndex);

            cullIndex = DoOmnidirectionalShadowMapping(gameTime, scene, cullIndex);

            cullIndex = DoSpotShadowMapping(gameTime, scene, cullIndex);

            return cullIndex;
        }
        /// <summary>
        /// Draw directional shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns>Returns the resulting cull index</returns>
        protected virtual int DoDirectionalShadowMapping(GameTime gameTime, Scene scene, int cullIndex)
        {
            var shadowCastingLights = scene.Lights.GetDirectionalShadowCastingLights();
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

                var shadowObjs = scene.GetComponents(c => c.Visible == true && c.CastShadow == true);
                if (shadowObjs.Count > 0)
                {
                    this.DrawShadowsContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawShadowsContext.EyePosition = this.DrawContext.EyePosition;

                    var toCullShadowObjs = shadowObjs.Where(s => s.Is<ICullable>()).Select(s => s.Get<ICullable>());

                    uint assigned = 0;

                    for (int l = 0; l < shadowCastingLights.Length; l++)
                    {
                        if (assigned >= MaxDirectionalShadowMaps)
                        {
                            break;
                        }

                        var light = shadowCastingLights[l];
                        light.ShadowMapIndex = 0;
                        light.ShadowMapCount = 0;
                        light.FromLightVP = new Matrix[Effects.BufferDirectionalLight.MAXSubMaps];

                        if (scene.Lights.GetDirectionalLightShadowParams(
                            light,
                            out Vector3 lightPosition,
                            out Vector3 lightDirection))
                        {
                            for (int sm = 0; sm < MaxDirectionalSubshadowMaps; sm++)
                            {
                                float distance = DirectionalShadowMapDistances[sm];

                                var sph = new CullingVolumeSphere(this.DrawContext.EyePosition, distance);

                                var doShadows = this.cullManager.Cull(sph, cullIndex, toCullShadowObjs);

                                if (doShadows)
                                {
                                    var fromLightVP = SceneLights.GetFromLightViewProjection(
                                        lightPosition,
                                        this.DrawContext.EyePosition,
                                        distance);

                                    light.ShadowMapIndex = assigned;
                                    light.ShadowMapCount++;
                                    light.FromLightVP[sm] = fromLightVP;

                                    var shadowMapper = this.DrawShadowsContext.ShadowMap = this.ShadowMapperDirectional;

                                    shadowMapper.FromLightViewProjectionArray[0] = fromLightVP;
                                    shadowMapper.Bind(graphics, (l * MaxDirectionalSubshadowMaps) + sm);

                                    this.DrawShadowComponents(gameTime, this.DrawShadowsContext, cullIndex, shadowObjs);
                                }

                                cullIndex++;
                            }

                            assigned++;
                        }
                    }
                }
            }

            return cullIndex;
        }
        /// <summary>
        /// Draw omnidirectional shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns>Returns the resulting cull index</returns>
        protected virtual int DoOmnidirectionalShadowMapping(GameTime gameTime, Scene scene, int cullIndex)
        {
            var shadowCastingLights = scene.Lights.GetOmnidirectionalShadowCastingLights(scene.Camera.Position);
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

                //Draw components if drop shadow (opaque)
                var shadowObjs = scene.GetComponents(c => c.Visible == true && c.CastShadow == true);
                if (shadowObjs.Count > 0)
                {
                    var toCullShadowObjs = shadowObjs.Where(s => s.Is<ICullable>()).Select(s => s.Get<ICullable>());

                    uint assigned = 0;

                    for (int l = 0; l < shadowCastingLights.Length; l++)
                    {
                        if (assigned >= MaxCubicShadows)
                        {
                            break;
                        }

                        var light = shadowCastingLights[l];

                        var sph = new CullingVolumeSphere(light.Position, light.Radius);

                        var doShadows = this.cullManager.Cull(sph, cullIndex, toCullShadowObjs);

                        if (doShadows)
                        {
                            light.ShadowMapIndex = assigned;
                            var shadowMapper = this.ShadowMapperOmnidirectional;
                            assigned++;

                            this.DrawShadowsContext.ShadowMap = shadowMapper;

                            var vpArray = SceneLights.GetFromOmniLightViewProjection(light);

                            shadowMapper.FromLightViewProjectionArray = vpArray;
                            shadowMapper.Bind(graphics, l);

                            this.DrawShadowComponents(gameTime, this.DrawShadowsContext, cullIndex, shadowObjs);
                        }

                        cullIndex++;
                    }
                }
            }

            return cullIndex;
        }
        /// <summary>
        /// Draw spot light shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns>Returns the resulting cull index</returns>
        protected virtual int DoSpotShadowMapping(GameTime gameTime, Scene scene, int cullIndex)
        {
            var shadowCastingLights = scene.Lights.GetSpotShadowCastingLights(scene.Camera.Position);
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

                //Draw components if drop shadow (opaque)
                var shadowObjs = scene.GetComponents(c => c.Visible == true && c.CastShadow == true);
                if (shadowObjs.Count > 0)
                {
                    var toCullShadowObjs = shadowObjs.Where(s => s.Is<ICullable>()).Select(s => s.Get<ICullable>());

                    uint assigned = 0;

                    for (int l = 0; l < shadowCastingLights.Length; l++)
                    {
                        if (assigned >= MaxCubicShadows)
                        {
                            break;
                        }

                        var light = shadowCastingLights[l];

                        var sph = new CullingVolumeSphere(light.Position, light.Radius);

                        var doShadows = this.cullManager.Cull(sph, cullIndex, toCullShadowObjs);

                        if (doShadows)
                        {
                            light.ShadowMapIndex = assigned;
                            var shadowMapper = this.ShadowMapperSpot;
                            assigned++;

                            this.DrawShadowsContext.ShadowMap = shadowMapper;

                            var vp = SceneLights.GetFromSpotLightViewProjection(
                                light.Position,
                                light.Direction,
                                light.Radius);

                            light.FromLightVP = new[] { vp };

                            shadowMapper.FromLightViewProjectionArray = new[] { vp };
                            shadowMapper.Bind(graphics, l);

                            this.DrawShadowComponents(gameTime, this.DrawShadowsContext, cullIndex, shadowObjs);
                        }

                        cullIndex++;
                    }
                }
            }

            return cullIndex;
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

                return cull != null ? !this.cullManager.GetCullValue(index, cull).Culled : true;
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
