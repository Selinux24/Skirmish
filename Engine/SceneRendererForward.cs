#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Forward renderer class
    /// </summary>
    public class SceneRendererForward : ISceneRenderer
    {
        /// <summary>
        /// Shadow map size
        /// </summary>
        private const int ShadowMapSize = 1024 * 4;
        /// <summary>
        /// Cubic shadow map size
        /// </summary>
        private const int CubicShadowMapSize = 1024;
        /// <summary>
        /// Maximum number of cubic shadow maps
        /// </summary>
        private const int MaxCubicShadows = 8;
        /// <summary>
        /// Cull index for low definition shadows
        /// </summary>
        private const int CullIndexShadowLowIndex = 0;
        /// <summary>
        /// Cull index for high definition shadows
        /// </summary>
        private const int CullIndexShadowHighIndex = 1;
        /// <summary>
        /// Cull index for cubic shadow mapping
        /// </summary>
        private const int CullIndexShadowCubicIndex = 2;
        /// <summary>
        /// Cull index for drawing
        /// </summary>
        private const int CullIndexDrawIndex = 3;

        /// <summary>
        /// Validates the renderer against the current device configuration
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <returns>Returns true if the renderer is valid</returns>
        public static bool Validate(Graphics graphics)
        {
            return true;
        }

        /// <summary>
        /// High definition shadow mapper
        /// </summary>
        private IShadowMap shadowMapperHigh = null;
        /// <summary>
        /// Low definition shadow mapper
        /// </summary>
        private IShadowMap shadowMapperLow = null;
        /// <summary>
        /// Cube shadow mapper for point lights
        /// </summary>
        private IShadowMap[] shadowMapperCube = null;
        /// <summary>
        /// Cull manager
        /// </summary>
        private SceneCullManager cullManager = null;

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
        /// Low definition shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapLow
        {
            get
            {
                if (this.shadowMapperLow != null)
                {
                    return this.shadowMapperLow.Texture;
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
                if (this.shadowMapperHigh != null)
                {
                    return this.shadowMapperHigh.Texture;
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
        public SceneRendererForward(Game game)
        {
            this.Game = game;

            this.shadowMapperLow = new ShadowMap(game, ShadowMapSize, ShadowMapSize);
            this.shadowMapperHigh = new ShadowMap(game, ShadowMapSize, ShadowMapSize);
            this.shadowMapperCube = new CubicShadowMap[MaxCubicShadows];
            for (int i = 0; i < MaxCubicShadows; i++)
            {
                this.shadowMapperCube[i] = new CubicShadowMap(game, CubicShadowMapSize, CubicShadowMapSize);
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
            Helper.Dispose(this.shadowMapperLow);
            Helper.Dispose(this.shadowMapperHigh);
            Helper.Dispose(this.shadowMapperCube);
        }
        /// <summary>
        /// Resizes buffers
        /// </summary>
        public virtual void Resize()
        {

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
            this.UpdateContext.World = scene.World;
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
        public virtual void Draw(GameTime gameTime, Scene scene)
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
                long forward_start = 0;
                long forward_cull = 0;
                long forward_draw = 0;
                long forward_draw2D = 0;
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
                    this.DrawContext.World = this.UpdateContext.World;
                    this.DrawContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawContext.Frustum = this.UpdateContext.Frustum;
                    this.DrawContext.EyePosition = this.UpdateContext.EyePosition;
                    this.DrawContext.EyeTarget = this.UpdateContext.EyeDirection;
                    //Initialize context data from scene
                    this.DrawContext.Lights = scene.Lights;
                    this.DrawContext.ShadowMaps = 0;
                    this.DrawContext.ShadowMapLow = this.shadowMapperLow;
                    this.DrawContext.ShadowMapHigh = this.shadowMapperHigh;
                    this.DrawContext.ShadowMapCube = this.shadowMapperCube;

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

                    #region Render

                    #region Forward rendering

                    #region Preparation
#if DEBUG
                    Stopwatch swPreparation = Stopwatch.StartNew();
#endif
                    //Set default render target and depth buffer, and clear it
                    var graphics = this.Game.Graphics;
                    graphics.SetDefaultViewport();
                    graphics.SetDefaultRenderTarget();
#if DEBUG
                    swPreparation.Stop();

                    forward_start = swPreparation.ElapsedTicks;
#endif
                    #endregion

                    if (visibleComponents.Count > 0)
                    {
                        #region Cull
#if DEBUG
                        Stopwatch swCull = Stopwatch.StartNew();
#endif
                        bool draw = false;
                        if (scene.PerformFrustumCulling)
                        {
                            var toCullVisible = visibleComponents.FindAll(s => s.Is<ICullable>()).ConvertAll<ICullable>(s => s.Get<ICullable>());

                            //Frustum culling
                            draw = this.cullManager.Cull(this.DrawContext.Frustum, CullIndexDrawIndex, toCullVisible);
                        }
                        else
                        {
                            draw = true;
                        }
#if DEBUG
                        swCull.Stop();

                        forward_cull = swCull.ElapsedTicks;
#endif
                        #endregion

                        #region Draw

                        if (draw)
                        {
#if DEBUG
                            Stopwatch swDraw = Stopwatch.StartNew();
#endif
                            //Draw solid
                            this.DrawResultComponents(gameTime, this.DrawContext, CullIndexDrawIndex, visibleComponents);
#if DEBUG
                            swDraw.Stop();

                            forward_draw = swDraw.ElapsedTicks;
#endif
                        }

                        #endregion
                    }

                    #endregion

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

                long totalForward = forward_start + forward_cull + forward_draw + forward_draw2D;
                if (totalForward > 0)
                {
                    float prcStart = (float)forward_start / (float)totalForward;
                    float prcCull = (float)forward_cull / (float)totalForward;
                    float prcDraw = (float)forward_draw / (float)totalForward;
                    float prcDraw2D = (float)forward_draw2D / (float)totalForward;

                    Counters.SetStatistics("Scene.Draw.totalForward", string.Format(
                        "FR = {0:000000}; Start {1:00}%; Cull {2:00}%; Draw {3:00}%; Draw2D {4:00}%",
                        totalForward,
                        prcStart * 100f,
                        prcCull * 100f,
                        prcDraw * 100f,
                        prcDraw2D * 100f));
                }

                long other = total - (totalShadowMap + totalForward);

                float prcSM = (float)totalShadowMap / (float)total;
                float prcFR = (float)totalForward / (float)total;
                float prcOther = (float)other / (float)total;

                Counters.SetStatistics("Scene.Draw", string.Format(
                    "TOTAL = {0:000000}; Shadows {1:00}%; Forwars {2:00}%; Other {3:00}%;",
                    total,
                    prcSM * 100f,
                    prcFR * 100f,
                    prcOther * 100f));
#endif
            }
        }
        /// <summary>
        /// Draw directional shadow maps
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        /// <returns>Returns drawn map flag</returns>
        private ShadowMapFlags DoShadowMapping(GameTime gameTime, Scene scene)
        {
            ShadowMapFlags flags = ShadowMapFlags.None;

            var shadowCastingLights = scene.Lights.GetDirectionalShadowCastingLights();
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

#if DEBUG
                Stopwatch swShadowsPreparation = Stopwatch.StartNew();
#endif
                Vector3 lightPosition;
                Vector3 lightDirection;
                if (scene.Lights.GetDirectionalLightShadowParams(
                    shadowCastingLights[0],
                    out lightPosition,
                    out lightDirection))
                {
                    #region Preparation

                    this.DrawShadowsContext.World = this.UpdateContext.World;
                    this.DrawShadowsContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawShadowsContext.EyePosition = this.DrawContext.EyePosition;

#if DEBUG
                    swShadowsPreparation.Stop();
#endif
                    #endregion

                    #region Shadow map

                    //Draw components if drop shadow (opaque)
                    var shadowObjs = scene.GetComponents(c => c.CastShadow == true);
                    if (shadowObjs.Count > 0)
                    {
                        #region Cull

#if DEBUG
                        Stopwatch swCull = Stopwatch.StartNew();
#endif
                        var toCullShadowObjs = shadowObjs.FindAll(s => s.Is<ICullable>()).ConvertAll<ICullable>(s => s.Get<ICullable>());

                        var sph = new BoundingSphere(this.DrawContext.EyePosition, scene.Lights.ShadowLDDistance);

                        var doLowShadows = this.cullManager.Cull(sph, CullIndexShadowLowIndex, toCullShadowObjs);
#if DEBUG
                        swCull.Stop();
#endif
                        #endregion

                        if (doLowShadows)
                        {
                            flags |= ShadowMapFlags.LowDefinition;
                            this.DrawShadowsContext.ShadowMap = this.shadowMapperLow;

                            #region Draw

#if DEBUG
                            Stopwatch swDraw = Stopwatch.StartNew();
#endif
                            var fromLightVP = SceneLights.GetFromLightViewProjection(
                                lightPosition,
                                this.DrawContext.EyePosition,
                                scene.Lights.ShadowLDDistance);

                            this.shadowMapperLow.FromLightViewProjectionArray = new[] { fromLightVP };
                            this.shadowMapperLow.Bind(graphics);

                            this.DrawShadowComponents(gameTime, this.DrawShadowsContext, CullIndexShadowLowIndex, shadowObjs);
#if DEBUG
                            swDraw.Stop();
#endif
                            #endregion
                        }

                        #region Cull
#if DEBUG
                        swCull = Stopwatch.StartNew();
#endif
                        toCullShadowObjs = shadowObjs.FindAll(s => s.Is<ICullable>()).ConvertAll<ICullable>(s => s.Get<ICullable>());

                        sph = new BoundingSphere(this.DrawContext.EyePosition, scene.Lights.ShadowHDDistance);

                        var doHighShadows = this.cullManager.Cull(sph, CullIndexShadowHighIndex, toCullShadowObjs);
#if DEBUG
                        swCull.Stop();
#endif
                        #endregion

                        if (doHighShadows)
                        {
                            flags |= ShadowMapFlags.HighDefinition;
                            this.DrawShadowsContext.ShadowMap = this.shadowMapperHigh;

                            #region Draw
#if DEBUG
                            Stopwatch swDraw = Stopwatch.StartNew();
#endif
                            var fromLightVP = SceneLights.GetFromLightViewProjection(
                                lightPosition,
                                this.DrawContext.EyePosition,
                                scene.Lights.ShadowHDDistance);

                            this.shadowMapperHigh.FromLightViewProjectionArray = new[] { fromLightVP };
                            this.shadowMapperHigh.Bind(graphics);

                            this.DrawShadowComponents(gameTime, this.DrawShadowsContext, CullIndexShadowHighIndex, shadowObjs);
#if DEBUG
                            swDraw.Stop();
#endif

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
        private ShadowMapFlags DoCubicShadowMapping(GameTime gameTime, Scene scene)
        {
            ShadowMapFlags flags = ShadowMapFlags.None;

            var shadowCastingLights = scene.Lights.GetOmnidirectionalShadowCastingLights(scene.Camera.Position);
            if (shadowCastingLights.Length > 0)
            {
                var graphics = this.Game.Graphics;

#if DEBUG
                Stopwatch swShadowsPreparation = Stopwatch.StartNew();
#endif
                #region Preparation

                this.DrawShadowsContext.World = this.UpdateContext.World;

#if DEBUG
                swShadowsPreparation.Stop();
#endif
                #endregion

                #region Shadow map

                //Draw components if drop shadow (opaque)
                var shadowObjs = scene.GetComponents(c => c.CastShadow == true);
                if (shadowObjs.Count > 0)
                {
                    for (int c = 0; c < Math.Min(MaxCubicShadows, shadowCastingLights.Length); c++)
                    {
                        var light = shadowCastingLights[c];
                        light.ShadowMapIndex = c;

                        var shadowMapper = this.shadowMapperCube[c];

                        #region Cull
#if DEBUG
                        Stopwatch swCull = Stopwatch.StartNew();
#endif
                        var toCullShadowObjs = shadowObjs.FindAll(s => s.Is<ICullable>()).ConvertAll(s => s.Get<ICullable>());

                        var sph = new BoundingSphere(light.Position, light.Radius);

                        var doShadows = this.cullManager.Cull(sph, CullIndexShadowCubicIndex, toCullShadowObjs);
#if DEBUG
                        swCull.Stop();
#endif
                        #endregion

                        if (doShadows)
                        {
                            flags |= ShadowMapFlags.CubeMap;
                            this.DrawShadowsContext.ShadowMap = shadowMapper;

                            #region Draw
#if DEBUG
                            Stopwatch swDraw = Stopwatch.StartNew();
#endif
                            var vpArray = SceneLights.GetFromOmniLightViewProjection(light);

                            shadowMapper.FromLightViewProjectionArray = vpArray;
                            shadowMapper.Bind(graphics);

                            this.DrawShadowComponents(gameTime, this.DrawShadowsContext, CullIndexShadowCubicIndex, shadowObjs);
#if DEBUG
                            swDraw.Stop();
#endif
                            #endregion
                        }
                    }
                }

                #endregion
            }

            return flags;
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
        /// Draw components for shadow mapping
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="index">Cull results index</param>
        /// <param name="components">Components</param>
        private void DrawShadowComponents(GameTime gameTime, DrawContextShadows context, int index, IEnumerable<SceneObject> components)
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
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="index">Cull results index</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(GameTime gameTime, DrawContext context, int index, IEnumerable<SceneObject> components)
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

                        var d1 = cull1 != null ? this.cullManager.GetCullValue(index, cull1).Distance.Value : float.MaxValue;
                        var d2 = cull2 != null ? this.cullManager.GetCullValue(index, cull2).Distance.Value : float.MaxValue;

                        res = -d1.CompareTo(d2);
                    }

                    return res;
                });

                opaques.ForEach((c) =>
                {
                    Counters.MaxInstancesPerFrame += c.Count;

                    graphics.SetRasterizerDefault();
                    graphics.SetBlendDefault();

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

                        var d1 = cull1 != null ? this.cullManager.GetCullValue(index, cull1).Distance.Value : float.MaxValue;
                        var d2 = cull2 != null ? this.cullManager.GetCullValue(index, cull2).Distance.Value : float.MaxValue;

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
                    graphics.SetBlendTransparent();

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
    }
}
