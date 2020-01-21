using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Forward renderer class
    /// </summary>
    public class SceneRendererForward : BaseSceneRenderer
    {
#if DEBUG
        private readonly FrameStatsForward frameStats = new FrameStatsForward();
#endif

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
                this.frameStats.Clear();

                Stopwatch swTotal = Stopwatch.StartNew();
#endif
                //Draw visible components
                var visibleComponents = scene.GetComponents(c => c.Visible);
                if (visibleComponents.Any())
                {
                    //Initialize context data from update context
                    this.DrawContext.GameTime = gameTime;
                    this.DrawContext.DrawerMode = DrawerModes.Forward;
                    this.DrawContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawContext.CameraVolume = this.UpdateContext.CameraVolume;
                    this.DrawContext.EyePosition = this.UpdateContext.EyePosition;
                    this.DrawContext.EyeTarget = this.UpdateContext.EyeDirection;

                    //Initialize context data from scene
                    this.DrawContext.Lights = scene.Lights;
                    this.DrawContext.ShadowMapDirectional = this.ShadowMapperDirectional;
                    this.DrawContext.ShadowMapPoint = this.ShadowMapperPoint;
                    this.DrawContext.ShadowMapSpot = this.ShadowMapperSpot;

                    //Shadow mapping
                    DoShadowMapping(gameTime, scene);

                    //Rendering
                    DoRender(scene, visibleComponents);
                }
#if DEBUG
                swTotal.Stop();

                this.frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
            }
        }

        /// <summary>
        /// Do rendering
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="components">Components</param>
        private void DoRender(Scene scene, IEnumerable<ISceneObject> components)
        {
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

            this.frameStats.ForwardStart = swPreparation.ElapsedTicks;
#endif
            #endregion

            //Forward rendering
            if (components.Any())
            {
                #region Cull
#if DEBUG
                Stopwatch swCull = Stopwatch.StartNew();
#endif
                var toCullVisible = components.OfType<ICullable>();

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
                    if (groundVolume != null)
                    {
                        //Ground culling
                        draw = this.cullManager.Cull(groundVolume, CullIndexDrawIndex, toCullVisible);
                    }
                }

#if DEBUG
                swCull.Stop();

                this.frameStats.ForwardCull = swCull.ElapsedTicks;
#endif
                #endregion

                #region Draw

                if (draw)
                {
#if DEBUG
                    Stopwatch swDraw = Stopwatch.StartNew();
#endif
                    //Draw solid
                    this.DrawResultComponents(this.DrawContext, CullIndexDrawIndex, components);
#if DEBUG
                    swDraw.Stop();

                    this.frameStats.ForwardDraw = swDraw.ElapsedTicks;
#endif
                }

                #endregion
            }
        }
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="index">Cull results index</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(DrawContext context, int index, IEnumerable<ISceneObject> components)
        {
            //Save current drawer mode
            var mode = context.DrawerMode;

            Dictionary<string, double> dict = new Dictionary<string, double>();
            Stopwatch stopwatch = new Stopwatch();

            //First opaques
            stopwatch.Start();
            var opaques = this.GetOpaques(index, components);
            stopwatch.Stop();
            dict.Add("Opaques Selection", stopwatch.Elapsed.TotalMilliseconds);

            if (opaques.Any())
            {
                //Set mode to opaque only
                context.DrawerMode = mode | DrawerModes.OpaqueOnly;

                //Sort items nearest first
                stopwatch.Restart();
                opaques.Sort((c1, c2) => SortOpaques(index, c1, c2));
                stopwatch.Stop();
                dict.Add("Opaques Sort", stopwatch.Elapsed.TotalMilliseconds);

                //Draw items
                stopwatch.Restart();
                int oDIndex = 0;
                opaques.ForEach((c) =>
                {
                    Stopwatch stopwatch2 = new Stopwatch();
                    stopwatch2.Start();
                    this.DrawOpaque(context, c);
                    stopwatch2.Stop();
                    dict.Add($"Opaque Draw {oDIndex++} {c.Name}", stopwatch2.Elapsed.TotalMilliseconds);
                });
                stopwatch.Stop();
                dict.Add("Opaques Draw", stopwatch.Elapsed.TotalMilliseconds);
            }

            //Then transparents
            stopwatch.Restart();
            var transparents = this.GetTransparents(index, components);
            stopwatch.Stop();
            dict.Add("Transparents Selection", stopwatch.Elapsed.TotalMilliseconds);

            if (transparents.Any())
            {
                //Set drawer mode to transparent
                context.DrawerMode = mode | DrawerModes.TransparentOnly;

                //Sort items far first
                stopwatch.Restart();
                transparents.Sort((c1, c2) => SortTransparents(index, c1, c2));
                stopwatch.Stop();
                dict.Add("Transparents Sort", stopwatch.Elapsed.TotalMilliseconds);

                //Draw items
                stopwatch.Restart();
                int oTIndex = 0;
                transparents.ForEach((c) =>
                {
                    Stopwatch stopwatch2 = new Stopwatch();
                    stopwatch2.Start();
                    this.DrawTransparent(context, c);
                    stopwatch2.Stop();
                    dict.Add($"Transparent Draw {oTIndex++} {c.Name}", stopwatch2.Elapsed.TotalMilliseconds);
                });
                stopwatch.Stop();
                dict.Add("Transparents Draw", stopwatch.Elapsed.TotalMilliseconds);
            }

            //Reset drawer mode
            context.DrawerMode = mode;

            if (this.Game.CollectGameStatus)
            {
                this.Game.GameStatus.Add(dict);
            }
        }
    }
}
