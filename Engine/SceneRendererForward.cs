using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using DepthStencilClearFlags = SharpDX.Direct3D11.DepthStencilClearFlags;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Forward renderer class
    /// </summary>
    public class SceneRendererForward : ISceneRenderer
    {
        /// <summary>
        /// Shadow mapper
        /// </summary>
        private ShadowMap shadowMapper = null;

        /// <summary>
        /// Game
        /// </summary>
        protected Game Game;
        /// <summary>
        /// Draw context
        /// </summary>
        protected Context DrawContext = null;
        /// <summary>
        /// Context for shadow map drawing
        /// </summary>
        protected Context DrawShadowsContext = null;
        /// <summary>
        /// Shadow map
        /// </summary>
        protected ShaderResourceView ShadowMap
        {
            get
            {
                if (this.shadowMapper != null)
                {
                    return this.shadowMapper.Texture;
                }

                return null;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneRendererForward(Game game)
        {
            this.Game = game;

            this.shadowMapper = new ShadowMap(game, 2048, 2048);

            this.DrawContext = new Context()
            {
                DrawerMode = DrawerModesEnum.Forward,
            };

            this.DrawShadowsContext = new Context()
            {
                DrawerMode = DrawerModesEnum.ShadowMap,
            };
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
        /// Draws scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        public virtual void Draw(GameTime gameTime, Scene scene)
        {
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
            List<Drawable> visibleComponents = scene.Components.FindAll(c => c.Visible);
            if (visibleComponents.Count > 0)
            {
                #region Preparation
#if DEBUG
                Stopwatch swStartup = Stopwatch.StartNew();
#endif
                //Initialize context data
                Matrix viewProj = scene.Camera.View * scene.Camera.Projection;

                this.DrawContext.World = scene.World;
                this.DrawContext.ViewProjection = viewProj;
                this.DrawContext.Frustum = new BoundingFrustum(viewProj);
                this.DrawContext.EyePosition = scene.Camera.Position;
                this.DrawContext.Lights = scene.Lights;
                this.DrawContext.ShadowMap = null;
                this.DrawContext.ShadowMapViewProjection = Matrix.Identity;
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
                    Matrix shadowViewProj = this.shadowMapper.View * this.shadowMapper.Projection;

                    this.DrawShadowsContext.World = Matrix.Identity;
                    this.DrawShadowsContext.ViewProjection = shadowViewProj;
                    this.DrawShadowsContext.Frustum = new BoundingFrustum(shadowViewProj);
                    this.DrawShadowsContext.EyePosition = scene.Camera.Position;

                    //Update shadow transform using first ligth direction
                    this.shadowMapper.Update(shadowCastingLights[0].Direction, scene.SceneVolume);
#if DEBUG
                    swShadowsPreparation.Stop();

                    shadowMap_start = swShadowsPreparation.ElapsedTicks;
#endif
                    #endregion

                    //Draw components if drop shadow (opaque)
                    List<Drawable> shadowComponents = visibleComponents.FindAll(c => c.Opaque);
                    if (shadowComponents.Count > 0)
                    {
                        #region Cull
#if DEBUG
                        Stopwatch swCull = Stopwatch.StartNew();
#endif
                        bool draw = false;
                        if (scene.PerformFrustumCulling)
                        {
                            //Frustum culling
                            draw = scene.CullTest(gameTime, this.DrawShadowsContext, shadowComponents);
                        }
                        else
                        {
                            draw = true;
                        }
#if DEBUG
                        swCull.Stop();

                        shadowMap_cull = swCull.ElapsedTicks;
#endif
                        #endregion

                        #region Draw

                        if (draw)
                        {
#if DEBUG
                            Stopwatch swDraw = Stopwatch.StartNew();
#endif
                            this.Game.Graphics.SetViewport(this.shadowMapper.Viewport);

                            //Set shadow map depth map without render target
                            this.Game.Graphics.SetRenderTarget(
                                null,
                                false,
                                Color.Transparent,
                                this.shadowMapper.DepthMap,
                                true,
                                DepthStencilClearFlags.Depth);

                            //Draw scene using depth map
                            this.DrawShadowComponents(gameTime, this.DrawShadowsContext, shadowComponents);

                            //Set shadow map and transform to drawing context
                            this.DrawContext.ShadowMap = this.shadowMapper.Texture;
                            this.DrawContext.ShadowMapViewProjection = this.shadowMapper.View * this.shadowMapper.Projection;
#if DEBUG
                            swDraw.Stop();

                            shadowMap_draw = swDraw.ElapsedTicks;
#endif
                        }

                        #endregion
                    }
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

                List<Drawable> solidComponents = visibleComponents.FindAll(c => c.Opaque);
                if (solidComponents.Count > 0)
                {
                    #region Cull
#if DEBUG
                    Stopwatch swCull = Stopwatch.StartNew();
#endif
                    bool draw = false;
                    if (scene.PerformFrustumCulling)
                    {
                        //Frustum culling
                        draw = scene.CullTest(gameTime, this.DrawContext, solidComponents);
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

                    #region Draw 3D

                    if (draw)
                    {
#if DEBUG
                        Stopwatch swDraw = Stopwatch.StartNew();
#endif
                        //Draw solid
                        this.DrawResultComponents(gameTime, this.DrawContext, solidComponents);
#if DEBUG
                        swDraw.Stop();

                        forward_draw = swDraw.ElapsedTicks;
#endif
                    }

                    #endregion
                }

                List<Drawable> otherComponents = visibleComponents.FindAll(c => !c.Opaque);
                if (otherComponents.Count > 0)
                {
                    #region Draw 2D
#if DEBUG
                    Stopwatch swDraw = Stopwatch.StartNew();
#endif
                    //Draw other
                    this.DrawResultComponents(gameTime, this.DrawContext, otherComponents);
#if DEBUG
                    swDraw.Stop();

                    forward_draw2D = swDraw.ElapsedTicks;
#endif
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
        /// <summary>
        /// Gets renderer resources
        /// </summary>
        /// <param name="result">Resource type</param>
        /// <returns>Returns renderer specified resource, if renderer produces that resource.</returns>
        public virtual ShaderResourceView GetResource(SceneRendererResultEnum result)
        {
            if (result == SceneRendererResultEnum.ShadowMap) return this.ShadowMap;
            return null;
        }
        /// <summary>
        /// Draw components for shadow mapping
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="components">Components</param>
        private void DrawShadowComponents(GameTime gameTime, Context context, IList<Drawable> components)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (!components[i].Cull)
                {
                    this.Game.Graphics.SetRasterizerShadows();
                    this.Game.Graphics.SetBlendDefault();

                    components[i].Draw(gameTime, context);
                }
            }
        }
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(GameTime gameTime, Context context, IList<Drawable> components)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (!components[i].Cull)
                {
                    this.Game.Graphics.SetRasterizerDefault();
                    if (components[i].Opaque)
                    {
                        this.Game.Graphics.SetBlendDefault();
                    }
                    else
                    {
                        this.Game.Graphics.SetBlendTransparent();
                    }

                    components[i].Draw(gameTime, context);
                }
            }
        }
    }
}
