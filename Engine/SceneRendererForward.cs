using System.Collections.Generic;
using System.Linq;
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
        private const int ShadowMapSize = 2048;

        /// <summary>
        /// Shadow mapper
        /// </summary>
        private ShadowMap shadowMapper = null;

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
        /// Static shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapStatic
        {
            get
            {
                if (this.shadowMapper != null)
                {
                    return this.shadowMapper.TextureStatic;
                }

                return null;
            }
        }
        /// <summary>
        /// Dynamic shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapDynamic
        {
            get
            {
                if (this.shadowMapper != null)
                {
                    return this.shadowMapper.TextureDynamic;
                }

                return null;
            }
        }
        /// <summary>
        /// Gets or sets whether the static shadow map must be updated in the next frame
        /// </summary>
        protected bool UpdateShadowMapStatic { get; set; }
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

            this.shadowMapper = new ShadowMap(game, ShadowMapSize, ShadowMapSize);

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

            this.UpdateShadowMapStatic = true;
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public virtual void Dispose()
        {
            Helper.Dispose(this.shadowMapper);
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
            this.UpdateContext.EyeTarget = scene.Camera.Direction;
            this.UpdateContext.Lights = scene.Lights;

            //Cull lights
            scene.Lights.Cull(this.UpdateContext.Frustum, this.UpdateContext.EyePosition);

            //Update active components
            var activeComponents = scene.Components.FindAll(c => c.Active);
            for (int i = 0; i < activeComponents.Count; i++)
            {
                activeComponents[i].Update(this.UpdateContext);
            }

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
                var visibleComponents = scene.Components.FindAll(c => c.Visible);
                if (visibleComponents.Count > 0)
                {
                    #region Preparation
#if DEBUG
                    Stopwatch swStartup = Stopwatch.StartNew();
#endif
                    //Initialize context data from update context
                    this.DrawContext.GameTime = gameTime;
                    this.DrawContext.World = this.UpdateContext.World;
                    this.DrawContext.View = this.UpdateContext.View;
                    this.DrawContext.Projection = this.UpdateContext.Projection;
                    this.DrawContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawContext.Frustum = this.UpdateContext.Frustum;
                    this.DrawContext.EyePosition = this.UpdateContext.EyePosition;
                    this.DrawContext.EyeTarget = this.UpdateContext.EyeTarget;
                    //Initialize context data from scene
                    this.DrawContext.Lights = scene.Lights;
                    this.DrawContext.ShadowMaps = 0;
                    this.DrawContext.ShadowMapStatic = null;
                    this.DrawContext.ShadowMapDynamic = null;
                    this.DrawContext.FromLightViewProjection = Matrix.Identity;
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
                        this.DrawShadowsContext.GameTime = gameTime;
                        this.DrawShadowsContext.World = this.UpdateContext.World;
#if DEBUG
                        swShadowsPreparation.Stop();

                        shadowMap_cull = 0;
                        shadowMap_draw = 0;

                        shadowMap_start = swShadowsPreparation.ElapsedTicks;
#endif
                        #endregion

                        if (this.UpdateShadowMapStatic)
                        {
                            #region Static shadow map

                            //Draw static components if drop shadow (opaque)
                            var staticObjs = visibleComponents.FindAll(c => c.CastShadow == true && c.Static == true);
                            if (staticObjs.Count > 0)
                            {
                                if (!this.shadowMapper.Flags.HasFlag(ShadowMapFlags.Static))
                                {
                                    this.shadowMapper.Flags |= ShadowMapFlags.Static;
                                }

                                #region Draw
#if DEBUG
                                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                                staticObjs.ForEach(o => o.SetCulling(false));

                                this.shadowMapper.Update(
                                    shadowCastingLights[0],
                                    scene.SceneVolume.Center,
                                    scene.SceneVolume.Radius,
                                    ref this.DrawShadowsContext);
                                this.BindShadowMap(this.shadowMapper.DepthMapStatic);
                                this.DrawShadowComponents(gameTime, this.DrawShadowsContext, staticObjs);
#if DEBUG
                                swDraw.Stop();

                                shadowMap_draw += swDraw.ElapsedTicks;
#endif
                                #endregion
                            }

                            #endregion

                            this.UpdateShadowMapStatic = true;
                        }

                        #region Dynamic shadow map

                        //Draw dynamic components if drop shadow (opaque)
                        var dynamicObjs = visibleComponents.FindAll(c => c.CastShadow == true && c.Static == false);
                        if (dynamicObjs.Count > 0)
                        {
                            #region Cull
#if DEBUG
                            Stopwatch swCull = Stopwatch.StartNew();
#endif
                            bool draw = false;
                            if (scene.PerformFrustumCulling)
                            {
                                //Frustum culling
                                draw = scene.CullTest(this.DrawContext.Frustum, dynamicObjs);
                            }
                            else
                            {
                                draw = true;
                            }
#if DEBUG
                            swCull.Stop();

                            shadowMap_cull += swCull.ElapsedTicks;
#endif
                            #endregion

                            #region Draw

                            if (draw)
                            {
                                if (!this.shadowMapper.Flags.HasFlag(ShadowMapFlags.Dynamic))
                                {
                                    this.shadowMapper.Flags |= ShadowMapFlags.Dynamic;
                                }
#if DEBUG
                                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                                this.shadowMapper.Update(
                                    shadowCastingLights[0],
                                    scene.SceneVolume.Center,
                                    scene.SceneVolume.Radius,
                                    ref this.DrawShadowsContext);
                                this.BindShadowMap(this.shadowMapper.DepthMapDynamic);
                                this.DrawShadowComponents(gameTime, this.DrawShadowsContext, dynamicObjs);
#if DEBUG
                                swDraw.Stop();

                                shadowMap_draw += swDraw.ElapsedTicks;
#endif
                            }

                            #endregion
                        }

                        #endregion

                        //Set shadow map and transform to drawing context
                        this.DrawContext.ShadowMaps = (int)this.shadowMapper.Flags;
                        this.DrawContext.ShadowMapStatic = this.shadowMapper.TextureStatic;
                        this.DrawContext.ShadowMapDynamic = this.shadowMapper.TextureDynamic;
                        this.DrawContext.FromLightViewProjection = this.shadowMapper.ViewProjection;
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
                            //Frustum culling
                            draw = scene.CullTest(this.DrawContext.Frustum, visibleComponents);
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
                            this.DrawResultComponents(gameTime, this.DrawContext, visibleComponents);
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
            if (result == SceneRendererResultEnum.ShadowMapStatic) return this.ShadowMapStatic;
            if (result == SceneRendererResultEnum.ShadowMapDynamic) return this.ShadowMapDynamic;
            return null;
        }

        /// <summary>
        /// Binds graphics for shadow mapping pass
        /// </summary>
        private void BindShadowMap(DepthStencilView dsv)
        {
            //Set shadow mapper viewport
            this.Game.Graphics.SetViewport(this.shadowMapper.Viewport);

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
        /// <param name="components">Components</param>
        private void DrawShadowComponents(GameTime gameTime, DrawContext context, List<Drawable> components)
        {
            var toDraw = components.FindAll(c => !c.Cull);
            if (toDraw.Count > 0)
            {
                toDraw.ForEach((c) =>
                {
                    this.Game.Graphics.SetRasterizerCullFrontFace();

                    if (c.EnableDepthStencil) this.Game.Graphics.SetDepthStencilZEnabled();
                    else this.Game.Graphics.SetDepthStencilZDisabled();

                    if (c.EnableAlphaBlending) this.Game.Graphics.SetBlendTransparent();
                    else this.Game.Graphics.SetBlendDefault();

                    c.Draw(context);
                });
            }
        }
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(GameTime gameTime, DrawContext context, List<Drawable> components)
        {
            var toDraw = components.FindAll(c => !c.Cull);
            if (toDraw.Count > 0)
            {
                toDraw.ForEach((c) =>
                {
                    this.Game.Graphics.SetRasterizerDefault();

                    if (c.EnableDepthStencil) this.Game.Graphics.SetDepthStencilZEnabled();
                    else this.Game.Graphics.SetDepthStencilZDisabled();

                    if (c.EnableAlphaBlending) this.Game.Graphics.SetBlendTransparent();
                    else this.Game.Graphics.SetBlendDefault();

                    c.Draw(context);
                });
            }
        }
    }
}
