#if DEBUG
using System.Diagnostics;
#endif
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Forward renderer class
    /// </summary>
    public class SceneRendererForward : BaseSceneRenderer
    {
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
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneRendererForward(Game game) : base(game)
        {

        }

        /// <summary>
        /// Draws scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        public override void Draw(GameTime gameTime, Scene scene)
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
                    this.DrawContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawContext.CameraVolume = this.UpdateContext.CameraVolume;
                    this.DrawContext.EyePosition = this.UpdateContext.EyePosition;
                    this.DrawContext.EyeTarget = this.UpdateContext.EyeDirection;
                    //Initialize context data from scene
                    this.DrawContext.Lights = scene.Lights;
                    this.DrawContext.ShadowMaps = 0;
                    this.DrawContext.ShadowMapLow = this.ShadowMapperLow;
                    this.DrawContext.ShadowMapHigh = this.ShadowMapperHigh;
                    this.DrawContext.ShadowMapCube = this.ShadowMapperCube;

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
                        var toCullVisible = visibleComponents.FindAll(s => s.Is<ICullable>()).ConvertAll(s => s.Get<ICullable>());

                        bool draw = false;
                        if (scene.PerformFrustumCulling)
                        {
                            //Frustum culling
                            draw = this.cullManager.Cull(this.DrawContext.CameraVolume, CullIndexDrawIndex, toCullVisible);
                        }
                        else
                        {
                            draw = true;
                        }

                        if (draw)
                        {
                            var groundVolume = scene.GetSceneVolume();
                            if(groundVolume!= null)
                            {
                                //Ground culling
                                draw = this.cullManager.Cull(groundVolume, CullIndexDrawIndex, toCullVisible);
                            }
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

                        var d1 = cull1 != null ? this.cullManager.GetCullValue(index, cull1).Distance : float.MaxValue;
                        var d2 = cull2 != null ? this.cullManager.GetCullValue(index, cull2).Distance : float.MaxValue;

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

                        var d1 = cull1 != null ? this.cullManager.GetCullValue(index, cull1).Distance : float.MaxValue;
                        var d2 = cull2 != null ? this.cullManager.GetCullValue(index, cull2).Distance : float.MaxValue;

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
