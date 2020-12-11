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
            return graphics != null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        public SceneRendererForward(Scene scene) : base(scene)
        {

        }

        /// <summary>
        /// Draws scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Draw(GameTime gameTime)
        {
            if (!Updated)
            {
                return;
            }

            Updated = false;
#if DEBUG
            frameStats.Clear();

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            //Draw visible components
            var visibleComponents = Scene.GetComponents().Where(c => c.Visible);
            if (visibleComponents.Any())
            {
                //Initialize context data from update context
                DrawContext.GameTime = gameTime;
                DrawContext.DrawerMode = DrawerModes.Forward;
                DrawContext.ViewProjection = UpdateContext.ViewProjection;
                DrawContext.CameraVolume = UpdateContext.CameraVolume;
                DrawContext.EyePosition = UpdateContext.EyePosition;
                DrawContext.EyeTarget = UpdateContext.EyeDirection;

                //Initialize context data from scene
                DrawContext.Lights = Scene.Lights;
                DrawContext.ShadowMapDirectional = ShadowMapperDirectional;
                DrawContext.ShadowMapPoint = ShadowMapperPoint;
                DrawContext.ShadowMapSpot = ShadowMapperSpot;

                //Shadow mapping
                DoShadowMapping(gameTime);

                //Rendering
                DoRender(Scene, visibleComponents);
            }
#if DEBUG
            swTotal.Stop();

            frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
            //Post-processing
            DoPostProcessing(gameTime);
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
            BindResult();
#if DEBUG
            swPreparation.Stop();

            frameStats.ForwardStart = swPreparation.ElapsedTicks;
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
                    draw = cullManager.Cull(DrawContext.CameraVolume, CullIndexDrawIndex, toCullVisible);
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
                        draw = cullManager.Cull(groundVolume, CullIndexDrawIndex, toCullVisible);
                    }
                }

#if DEBUG
                swCull.Stop();

                frameStats.ForwardCull = swCull.ElapsedTicks;
#endif
                #endregion

                #region Draw

                if (draw)
                {
#if DEBUG
                    Stopwatch swDraw = Stopwatch.StartNew();
#endif
                    //Draw solid
                    DrawResultComponents(DrawContext, CullIndexDrawIndex, components);
#if DEBUG
                    swDraw.Stop();

                    frameStats.ForwardDraw = swDraw.ElapsedTicks;
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
            var opaques = GetOpaques(index, components);
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
                    Draw(context, c);
                    stopwatch2.Stop();
                    dict.Add($"Opaque Draw {oDIndex++} {c.Name}", stopwatch2.Elapsed.TotalMilliseconds);
                });
                stopwatch.Stop();
                dict.Add("Opaques Draw", stopwatch.Elapsed.TotalMilliseconds);
            }

            //Then transparents
            stopwatch.Restart();
            var transparents = GetTransparents(index, components);
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
                    Draw(context, c);
                    stopwatch2.Stop();
                    dict.Add($"Transparent Draw {oTIndex++} {c.Name}", stopwatch2.Elapsed.TotalMilliseconds);
                });
                stopwatch.Stop();
                dict.Add("Transparents Draw", stopwatch.Elapsed.TotalMilliseconds);
            }

            //Reset drawer mode
            context.DrawerMode = mode;

            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(dict);
            }
        }
    }
}
