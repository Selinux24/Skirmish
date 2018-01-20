#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Base scene renderer
    /// </summary>
    public abstract class SceneRendererBase : ISceneRenderer
    {
        /// <summary>
        /// Shadow map size
        /// </summary>
        protected const int ShadowMapSize = 1024 * 4;
        /// <summary>
        /// Cubic shadow map size
        /// </summary>
        protected const int CubicShadowMapSize = 1024;
        /// <summary>
        /// Maximum number of cubic shadow maps
        /// </summary>
        protected const int MaxCubicShadows = 8;
        /// <summary>
        /// Cull index for low definition shadows
        /// </summary>
        protected const int CullIndexShadowLowIndex = 0;
        /// <summary>
        /// Cull index for high definition shadows
        /// </summary>
        protected const int CullIndexShadowHighIndex = 1;
        /// <summary>
        /// Cull index for cubic shadow mapping
        /// </summary>
        protected const int CullIndexShadowCubicIndex = 2;
        /// <summary>
        /// Cull index for drawing
        /// </summary>
        protected const int CullIndexDrawIndex = 3;

        /// <summary>
        /// High definition shadow mapper
        /// </summary>
        protected IShadowMap ShadowMapperHigh { get; private set; }
        /// <summary>
        /// Low definition shadow mapper
        /// </summary>
        protected IShadowMap ShadowMapperLow { get; private set; }
        /// <summary>
        /// Cube shadow mapper for point lights
        /// </summary>
        protected IShadowMap[] ShadowMapperCube { get; private set; }

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
        /// Low definition shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapLow
        {
            get
            {
                if (this.ShadowMapperLow != null)
                {
                    return this.ShadowMapperLow.Texture;
                }

                return null;
            }
        }
        /// <summary>
        /// High definition shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapHigh
        {
            get
            {
                if (this.ShadowMapperHigh != null)
                {
                    return this.ShadowMapperHigh.Texture;
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
        public SceneRendererBase(Game game)
        {
            this.Game = game;

            this.ShadowMapperLow = new ShadowMap(game, ShadowMapSize, ShadowMapSize);
            this.ShadowMapperHigh = new ShadowMap(game, ShadowMapSize, ShadowMapSize);
            this.ShadowMapperCube = new CubicShadowMap[MaxCubicShadows];
            for (int i = 0; i < MaxCubicShadows; i++)
            {
                this.ShadowMapperCube[i] = new CubicShadowMap(game, CubicShadowMapSize, CubicShadowMapSize);
            }

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
        /// Dispose objects
        /// </summary>
        public virtual void Dispose()
        {
            Helper.Dispose(this.ShadowMapperLow);
            Helper.Dispose(this.ShadowMapperHigh);
            Helper.Dispose(this.ShadowMapperCube);
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
            if (result == SceneRendererResultEnum.ShadowMapStatic) return this.ShadowMapLow;
            if (result == SceneRendererResultEnum.ShadowMapDynamic) return this.ShadowMapHigh;
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
            this.UpdateContext.Frustum = new BoundingFrustum(viewProj);
            this.UpdateContext.EyePosition = scene.Camera.Position;
            this.UpdateContext.EyeDirection = scene.Camera.Direction;
            this.UpdateContext.Lights = scene.Lights;

            //Cull lights
            scene.Lights.Cull(this.UpdateContext.Frustum, this.UpdateContext.EyePosition);

            //Update active components
            scene.GetComponents<IUpdatable>(c => c.Active)
                .ForEach(c => c.Update(this.UpdateContext));

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
        /// Draw directional shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        /// <returns>Returns drawn map flag</returns>
        protected ShadowMapFlags DoShadowMapping(GameTime gameTime, Scene scene)
        {
            ShadowMapFlags flags = ShadowMapFlags.None;

            var shadowCastingLights = scene.Lights.GetDirectionalShadowCastingLights();
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

                Vector3 lightPosition;
                Vector3 lightDirection;
                if (scene.Lights.GetDirectionalLightShadowParams(
                    shadowCastingLights[0],
                    out lightPosition,
                    out lightDirection))
                {
                    #region Preparation

                    this.DrawShadowsContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawShadowsContext.EyePosition = this.DrawContext.EyePosition;

                    #endregion

                    #region Shadow map

                    //Draw components if drop shadow (opaque)
                    var shadowObjs = scene.GetComponents(c => c.CastShadow == true);
                    if (shadowObjs.Count > 0)
                    {
                        #region Cull

                        var toCullShadowObjs = shadowObjs.FindAll(s => s.Is<ICullable>()).ConvertAll<ICullable>(s => s.Get<ICullable>());

                        var sph = new BoundingSphere(this.DrawContext.EyePosition, scene.Lights.ShadowLDDistance);

                        var doLowShadows = this.cullManager.Cull(sph, CullIndexShadowLowIndex, toCullShadowObjs);

                        #endregion

                        if (doLowShadows)
                        {
                            flags |= ShadowMapFlags.LowDefinition;
                            this.DrawShadowsContext.ShadowMap = this.ShadowMapperLow;

                            #region Draw

                            var fromLightVP = SceneLights.GetFromLightViewProjection(
                                lightPosition,
                                this.DrawContext.EyePosition,
                                scene.Lights.ShadowLDDistance);

                            this.ShadowMapperLow.FromLightViewProjectionArray = new[] { fromLightVP };
                            this.ShadowMapperLow.Bind(graphics);

                            this.DrawShadowComponents(gameTime, this.DrawShadowsContext, CullIndexShadowLowIndex, shadowObjs);

                            #endregion
                        }

                        #region Cull

                        toCullShadowObjs = shadowObjs.FindAll(s => s.Is<ICullable>()).ConvertAll<ICullable>(s => s.Get<ICullable>());

                        sph = new BoundingSphere(this.DrawContext.EyePosition, scene.Lights.ShadowHDDistance);

                        var doHighShadows = this.cullManager.Cull(sph, CullIndexShadowHighIndex, toCullShadowObjs);

                        #endregion

                        if (doHighShadows)
                        {
                            flags |= ShadowMapFlags.HighDefinition;
                            this.DrawShadowsContext.ShadowMap = this.ShadowMapperHigh;

                            #region Draw

                            var fromLightVP = SceneLights.GetFromLightViewProjection(
                                lightPosition,
                                this.DrawContext.EyePosition,
                                scene.Lights.ShadowHDDistance);

                            this.ShadowMapperHigh.FromLightViewProjectionArray = new[] { fromLightVP };
                            this.ShadowMapperHigh.Bind(graphics);

                            this.DrawShadowComponents(gameTime, this.DrawShadowsContext, CullIndexShadowHighIndex, shadowObjs);

                            #endregion
                        }
                    }

                    #endregion
                }
            }

            return flags;
        }
        /// <summary>
        /// Draw omnidirectional shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        /// <returns>Returns drawn map flag</returns>
        protected ShadowMapFlags DoCubicShadowMapping(GameTime gameTime, Scene scene)
        {
            ShadowMapFlags flags = ShadowMapFlags.None;

            var shadowCastingLights = scene.Lights.GetOmnidirectionalShadowCastingLights(scene.Camera.Position);
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

                //Draw components if drop shadow (opaque)
                var shadowObjs = scene.GetComponents(c => c.CastShadow == true);
                if (shadowObjs.Count > 0)
                {
                    int assigned = 0;

                    for (int l = 0; l < shadowCastingLights.Length; l++)
                    {
                        if (assigned >= MaxCubicShadows)
                        {
                            break;
                        }

                        var light = shadowCastingLights[l];

                        #region Cull

                        var toCullShadowObjs = shadowObjs.FindAll(s => s.Is<ICullable>()).ConvertAll(s => s.Get<ICullable>());

                        var sph = new BoundingSphere(light.Position, light.Radius);

                        var doShadows = this.cullManager.Cull(sph, CullIndexShadowCubicIndex, toCullShadowObjs);

                        #endregion

                        if (doShadows)
                        {
                            light.ShadowMapIndex = assigned;
                            var shadowMapper = this.ShadowMapperCube[assigned];
                            assigned++;

                            flags |= ShadowMapFlags.CubeMap;
                            this.DrawShadowsContext.ShadowMap = shadowMapper;

                            #region Draw

                            var vpArray = SceneLights.GetFromOmniLightViewProjection(light);

                            shadowMapper.FromLightViewProjectionArray = vpArray;
                            shadowMapper.Bind(graphics);

                            this.DrawShadowComponents(gameTime, this.DrawShadowsContext, CullIndexShadowCubicIndex, shadowObjs);

                            #endregion
                        }
                    }
                }
            }

            return flags;
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

            var objects = components.FindAll(c =>
            {
                if (!c.Is<Drawable>()) return false;

                var cull = c.Get<ICullable>();

                return cull != null ? !this.cullManager.GetCullValue(index, cull).Culled : true;
            });
            if (objects.Count > 0)
            {
                objects.Sort((c1, c2) =>
                {
                    int res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
                    if (res == 0)
                    {
                        var cull1 = c1.Get<ICullable>();
                        var cull2 = c2.Get<ICullable>();

                        var d1 = cull1 != null ? this.cullManager.GetCullValue(index, cull1).Distance.Value : float.MaxValue;
                        var d2 = cull2 != null ? this.cullManager.GetCullValue(index, cull2).Distance.Value : float.MaxValue;

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
