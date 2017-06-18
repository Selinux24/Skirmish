using System.Collections.Generic;

#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using DepthStencilClearFlags = SharpDX.Direct3D11.DepthStencilClearFlags;
using DepthStencilView = SharpDX.Direct3D11.DepthStencilView;

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
        /// Cull index for low definition shadows
        /// </summary>
        private const int CullIndexShadowLowIndex = 0;
        /// <summary>
        /// Cull index for high definition shadows
        /// </summary>
        private const int CullIndexShadowHighIndex = 1;
        /// <summary>
        /// Cull index for drawing
        /// </summary>
        private const int CullIndexDrawIndex = 2;

        /// <summary>
        /// High definition shadow mapper
        /// </summary>
        private ShadowMap shadowMapperHigh = null;
        /// <summary>
        /// Low definition shadow mapper
        /// </summary>
        private ShadowMap shadowMapperLow = null;
        /// <summary>
        /// Cull manager
        /// </summary>
        private SceneCullManager cullManager = null;

        /// <summary>
        /// Game
        /// </summary>
        protected Game Game;
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
        protected DrawContext DrawShadowsContext = null;
        /// <summary>
        /// Low definition shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapLow
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
        protected ShaderResourceView ShadowMapHigh
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

            this.DrawShadowsContext = new DrawContext()
            {
                Name = "Secondary",
                DrawerMode = DrawerModesEnum.ShadowMap,
            };
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public virtual void Dispose()
        {
            Helper.Dispose(this.shadowMapperLow);
            Helper.Dispose(this.shadowMapperHigh);
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
                    this.DrawContext.ShadowMapLow = null;
                    this.DrawContext.ShadowMapHigh = null;
                    this.DrawContext.FromLightViewProjectionLow = Matrix.Identity;
                    this.DrawContext.FromLightViewProjectionHigh = Matrix.Identity;
#if DEBUG
                    swStartup.Stop();

                    start = swStartup.ElapsedTicks;
#endif
                    #endregion

                    #region Shadow mapping

                    var shadowCastingLights = scene.Lights.ShadowCastingLights;
                    if (shadowCastingLights.Length > 0)
                    {
                        #region Preparation
#if DEBUG
                        Stopwatch swShadowsPreparation = Stopwatch.StartNew();
#endif
                        Vector3 lightPosition;
                        Vector3 lightDirection;
                        ShadowMap.SetLight(shadowCastingLights[0], scene.Lights.FarLightsDistance, out lightPosition, out lightDirection);

                        ShadowMapFlags flags = ShadowMapFlags.None;

                        this.DrawShadowsContext.GameTime = gameTime;
                        this.DrawShadowsContext.World = this.UpdateContext.World;
                        this.DrawShadowsContext.EyePosition = lightPosition;
                        this.DrawShadowsContext.EyeTarget = lightDirection;

#if DEBUG
                        swShadowsPreparation.Stop();

                        shadowMap_cull = 0;
                        shadowMap_draw = 0;

                        shadowMap_start = swShadowsPreparation.ElapsedTicks;
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
                            var toCullShadowObjs = shadowObjs.FindAll(s => s.Is<ICull>()).ConvertAll<ICull>(s => s.Get<ICull>());

                            var sph = new BoundingSphere(this.DrawContext.EyePosition, scene.Lights.ShadowLDDistance);

                            var doLowShadows = this.cullManager.Cull(sph, CullIndexShadowLowIndex, toCullShadowObjs);
#if DEBUG
                            swCull.Stop();

                            shadowMap_cull += swCull.ElapsedTicks;
#endif
                            #endregion

                            if (doLowShadows)
                            {
                                flags |= ShadowMapFlags.LowDefinition;

                                #region Draw

#if DEBUG
                                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                                var fromLightVP = this.shadowMapperLow.GetFromLightViewProjection(
                                    lightPosition,
                                    this.DrawContext.EyePosition,
                                    scene.Lights.ShadowLDDistance);

                                this.DrawShadowsContext.ViewProjection = fromLightVP;

                                this.BindShadowMap(this.shadowMapperLow.Viewport, this.shadowMapperLow.DepthMap);
                                this.DrawShadowComponents(gameTime, this.DrawShadowsContext, CullIndexShadowLowIndex, shadowObjs);

                                this.DrawContext.ShadowMapLow = this.shadowMapperLow.Texture;
                                this.DrawContext.FromLightViewProjectionLow = fromLightVP;
#if DEBUG
                                swDraw.Stop();

                                shadowMap_draw += swDraw.ElapsedTicks;
#endif
                                #endregion
                            }

                            #region Cull
#if DEBUG
                            swCull = Stopwatch.StartNew();
#endif
                            toCullShadowObjs = shadowObjs.FindAll(s => s.Is<ICull>()).ConvertAll<ICull>(s => s.Get<ICull>());

                            sph = new BoundingSphere(this.DrawContext.EyePosition, scene.Lights.ShadowHDDistance);

                            var doHighShadows = this.cullManager.Cull(sph, CullIndexShadowHighIndex, toCullShadowObjs);
#if DEBUG
                            swCull.Stop();

                            shadowMap_cull += swCull.ElapsedTicks;
#endif
                            #endregion

                            if (doHighShadows)
                            {
                                flags |= ShadowMapFlags.HighDefinition;

                                #region Draw
#if DEBUG
                                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                                Matrix fromLightVP = this.shadowMapperHigh.GetFromLightViewProjection(
                                    lightPosition,
                                    this.DrawContext.EyePosition,
                                    scene.Lights.ShadowHDDistance);

                                this.DrawShadowsContext.ViewProjection = fromLightVP;

                                this.BindShadowMap(this.shadowMapperHigh.Viewport, this.shadowMapperHigh.DepthMap);
                                this.DrawShadowComponents(gameTime, this.DrawShadowsContext, CullIndexShadowHighIndex, shadowObjs);

                                this.DrawContext.ShadowMapHigh = this.shadowMapperHigh.Texture;
                                this.DrawContext.FromLightViewProjectionHigh = fromLightVP;
#if DEBUG
                                swDraw.Stop();

                                shadowMap_draw += swDraw.ElapsedTicks;
#endif

                                #endregion
                            }
                        }

                        #endregion

                        //Set shadow map flags to drawing context
                        this.DrawContext.ShadowMaps = (int)flags;
                    }

                    #endregion

                    #region Render

                    #region Forward rendering

                    #region Preparation
#if DEBUG
                    Stopwatch swPreparation = Stopwatch.StartNew();
#endif
                    //Set default render target and depth buffer, and clear it
                    this.Game.Graphics.SetDefaultViewport();
                    this.Game.Graphics.SetDefaultRenderTarget(true);
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
                            var toCullVisible = visibleComponents.FindAll(s => s.Is<ICull>()).ConvertAll<ICull>(s => s.Get<ICull>());

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
        /// Gets renderer resources
        /// </summary>
        /// <param name="result">Resource type</param>
        /// <returns>Returns renderer specified resource, if renderer produces that resource.</returns>
        public virtual ShaderResourceView GetResource(SceneRendererResultEnum result)
        {
            if (result == SceneRendererResultEnum.ShadowMapStatic) return this.ShadowMapLow;
            if (result == SceneRendererResultEnum.ShadowMapDynamic) return this.ShadowMapHigh;
            return null;
        }

        /// <summary>
        /// Binds graphics for shadow mapping pass
        /// </summary>
        /// <param name="viewport">Viewport</param>
        /// <param name="dsv">Deph stencil buffer</param>
        private void BindShadowMap(Viewport viewport, DepthStencilView dsv)
        {
            //Set shadow mapper viewport
            this.Game.Graphics.SetViewport(viewport);

            //Set shadow map depth map without render target
            this.Game.Graphics.SetRenderTarget(
                null,
                false,
                Color.Transparent,
                dsv,
                true,
                DepthStencilClearFlags.Depth);
        }
        /// <summary>
        /// Draw components for shadow mapping
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="index">Cull results index</param>
        /// <param name="components">Components</param>
        private void DrawShadowComponents(GameTime gameTime, DrawContext context, int index, IEnumerable<SceneObject> components)
        {
            components.FindAll(c => c.Is<IDrawable>()).ForEach((c) =>
            {
                var cull = c.Get<ICull>();

                var visible = cull != null ? !this.cullManager.IsVisible(index, cull) : true;
                if (visible)
                {
                    this.Game.Graphics.SetRasterizerCullFrontFace();

                    if (c.DepthEnabled) this.Game.Graphics.SetDepthStencilZEnabled();
                    else this.Game.Graphics.SetDepthStencilZDisabled();

                    if (c.AlphaEnabled) this.Game.Graphics.SetBlendTransparent();
                    else this.Game.Graphics.SetBlendDefault();

                    c.Get<IDrawable>().Draw(context);
                }
            });
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
            components.FindAll(c => c.Is<IDrawable>()).ForEach((c) =>
            {
                Counters.MaxInstancesPerFrame += c.Count;

                var visible = (c is ICull) ? !this.cullManager.IsVisible(index, (ICull)c) : true;
                if (visible)
                {
                    this.Game.Graphics.SetRasterizerDefault();

                    if (c.DepthEnabled) this.Game.Graphics.SetDepthStencilZEnabled();
                    else this.Game.Graphics.SetDepthStencilZDisabled();

                    if (c.AlphaEnabled) this.Game.Graphics.SetBlendTransparent();
                    else this.Game.Graphics.SetBlendDefault();

                    c.Get<IDrawable>().Draw(context);
                }
            });
        }
    }
}
