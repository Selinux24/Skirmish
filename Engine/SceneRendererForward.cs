#if DEBUG
using System.Diagnostics;
#endif
using System.Collections.Generic;
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

        /// <inheritdoc/>
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

                //Binds the result target
                BindResult(true, true, true);

                //Render components
                DoRender(Scene, visibleComponents.Where(c => !c.Usage.HasFlag(SceneObjectUsages.UI)));

                //Post-processing
                DoPostProcessing(gameTime);

                //Render UI
                DoRender(Scene, visibleComponents.Where(c => c.Usage.HasFlag(SceneObjectUsages.UI)));

                //Write result
                WriteResult(DrawContext);
            }
#if DEBUG
            swTotal.Stop();

            frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
        }

        /// <summary>
        /// Do rendering
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="components">Components</param>
        private void DoRender(Scene scene, IEnumerable<ISceneObject> components)
        {
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
#if DEBUG
            Dictionary<string, double> dict = new Dictionary<string, double>();
            Stopwatch stopwatch = new Stopwatch();
#endif
            //First opaques
#if DEBUG
            stopwatch.Start();
#endif
            var opaques = GetOpaques(index, components);
#if DEBUG
            stopwatch.Stop();
            dict.Add("Opaques Selection", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (opaques.Any())
            {
                //Set mode to opaque only
                context.DrawerMode = mode | DrawerModes.OpaqueOnly;

                //Sort items nearest first
#if DEBUG
                stopwatch.Restart();
#endif
                opaques.Sort((c1, c2) => SortOpaques(index, c1, c2));
#if DEBUG
                stopwatch.Stop();
                dict.Add("Opaques Sort", stopwatch.Elapsed.TotalMilliseconds);
#endif

                //Draw items
#if DEBUG
                stopwatch.Restart();
                int oDIndex = 0;
#endif
                opaques.ForEach((c) =>
                {
#if DEBUG
                    Stopwatch stopwatch2 = new Stopwatch();
                    stopwatch2.Start();
#endif
                    Draw(context, c);
#if DEBUG
                    stopwatch2.Stop();
                    dict.Add($"Opaque Draw {oDIndex++} {c.Name}", stopwatch2.Elapsed.TotalMilliseconds);
#endif
                });
#if DEBUG
                stopwatch.Stop();
                dict.Add("Opaques Draw", stopwatch.Elapsed.TotalMilliseconds);
#endif
            }

            //Then transparents
#if DEBUG
            stopwatch.Restart();
#endif
            var transparents = GetTransparents(index, components);
#if DEBUG
            stopwatch.Stop();
            dict.Add("Transparents Selection", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (transparents.Any())
            {
                //Set drawer mode to transparent
                context.DrawerMode = mode | DrawerModes.TransparentOnly;

                //Sort items far first
#if DEBUG
                stopwatch.Restart();
#endif
                transparents.Sort((c1, c2) => SortTransparents(index, c1, c2));
#if DEBUG
                stopwatch.Stop();
                dict.Add("Transparents Sort", stopwatch.Elapsed.TotalMilliseconds);
#endif

                //Draw items
#if DEBUG
                stopwatch.Restart();
                int oTIndex = 0;
#endif
                transparents.ForEach((c) =>
                {
#if DEBUG
                    Stopwatch stopwatch2 = new Stopwatch();
                    stopwatch2.Start();
#endif
                    Draw(context, c);
#if DEBUG
                    stopwatch2.Stop();
                    dict.Add($"Transparent Draw {oTIndex++} {c.Name}", stopwatch2.Elapsed.TotalMilliseconds);
#endif
                });
#if DEBUG
                stopwatch.Stop();
                dict.Add("Transparents Draw", stopwatch.Elapsed.TotalMilliseconds);
#endif
            }

            //Reset drawer mode
            context.DrawerMode = mode;

#if DEBUG
            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(dict);
            }
#endif
        }
    }
}
