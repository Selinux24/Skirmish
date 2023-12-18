#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Forward renderer class
    /// </summary>
    public class SceneRendererForward : BaseSceneRenderer
    {
        /// <summary>
        /// Objects pass index
        /// </summary>
        private const int ObjectsPass = NextPass;
        /// <summary>
        /// Objects post-processing pass index
        /// </summary>
        private const int ObjectsPostProcessingPass = NextPass + 1;
        /// <summary>
        /// UI pass index
        /// </summary>
        private const int UIPass = NextPass + 2;
        /// <summary>
        /// UI post processing pass index
        /// </summary>
        private const int UIPostProcessingPass = NextPass + 3;

#if DEBUG

        /// <summary>
        /// Frame stats
        /// </summary>
        class FrameStatsForward
        {
            /// <summary>
            /// Total frame ticks
            /// </summary>
            public long Total { get; private set; } = 0;
            /// <summary>
            /// Shadow map start ticks
            /// </summary>
            public long ShadowMapStart { get; set; } = 0;
            /// <summary>
            /// Shadow map cull ticks
            /// </summary>
            public long ShadowMapCull { get; set; } = 0;
            /// <summary>
            /// Shadow map draw ticks
            /// </summary>
            public long ShadowMapDraw { get; set; } = 0;
            /// <summary>
            /// Forward start ticks
            /// </summary>
            public long ForwardStart { get; set; } = 0;
            /// <summary>
            /// Forward cull ticks
            /// </summary>
            public long ForwardCull { get; set; } = 0;
            /// <summary>
            /// Forward draw ticks
            /// </summary>
            public long ForwardDraw { get; set; } = 0;
            /// <summary>
            /// Forward draw 2D ticks
            /// </summary>
            public long ForwardDraw2D { get; set; } = 0;

            /// <summary>
            /// Clear frame
            /// </summary>
            public void Clear()
            {
                Total = 0;
                ShadowMapStart = 0;
                ShadowMapCull = 0;
                ShadowMapDraw = 0;
                ForwardStart = 0;
                ForwardCull = 0;
                ForwardDraw = 0;
                ForwardDraw2D = 0;
            }
            /// <summary>
            /// Update stats into counter
            /// </summary>
            /// <param name="elapsedTicks">Elapsed ticks</param>
            public void UpdateCounters(long elapsedTicks)
            {
                Total = elapsedTicks;

                long totalShadowMap = ShadowMapStart + ShadowMapCull + ShadowMapDraw;
                if (totalShadowMap > 0)
                {
                    float prcStart = (float)ShadowMapStart / (float)totalShadowMap;
                    float prcCull = (float)ShadowMapCull / (float)totalShadowMap;
                    float prcDraw = (float)ShadowMapDraw / (float)totalShadowMap;

                    FrameCounters.SetStatistics("Scene.Draw.totalShadowMap", string.Format(
                        "SM = {0:000000}; Start {1:00}%; Cull {2:00}%; Draw {3:00}%",
                        totalShadowMap,
                        prcStart * 100f,
                        prcCull * 100f,
                        prcDraw * 100f));
                }

                long totalForward = ForwardStart + ForwardCull + ForwardDraw + ForwardDraw2D;
                if (totalForward > 0)
                {
                    float prcStart = (float)ForwardStart / (float)totalForward;
                    float prcCull = (float)ForwardCull / (float)totalForward;
                    float prcDraw = (float)ForwardDraw / (float)totalForward;
                    float prcDraw2D = (float)ForwardDraw2D / (float)totalForward;

                    FrameCounters.SetStatistics("Scene.Draw.totalForward", string.Format(
                        "FR = {0:000000}; Start {1:00}%; Cull {2:00}%; Draw {3:00}%; Draw2D {4:00}%",
                        totalForward,
                        prcStart * 100f,
                        prcCull * 100f,
                        prcDraw * 100f,
                        prcDraw2D * 100f));
                }

                long other = Total - (totalShadowMap + totalForward);

                float prcSM = (float)totalShadowMap / (float)Total;
                float prcFR = (float)totalForward / (float)Total;
                float prcOther = (float)other / (float)Total;

                FrameCounters.SetStatistics("Scene.Draw", string.Format(
                    "TOTAL = {0:000000}; Shadows {1:00}%; Forwars {2:00}%; Other {3:00}%;",
                    Total,
                    prcSM * 100f,
                    prcFR * 100f,
                    prcOther * 100f));
            }
        }

        /// <summary>
        /// Frame statistics
        /// </summary>
        private readonly FrameStatsForward frameStats = new();
        /// <summary>
        /// Statistics dictionary
        /// </summary>
        private readonly ConcurrentDictionary<string, double> dict = new();

        /// <summary>
        /// Writes a trace in the trace dictionary
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="milliseconds">Milliseconds</param>
        private void WriteTrace(string key, double milliseconds)
        {
            dict.AddOrUpdate(key, milliseconds, (k, o) => o);
        }
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
        public override void PrepareScene()
        {
            base.PrepareScene();

            //Create commandList
            AddPassContext(ObjectsPass, "Objects");
            AddPassContext(ObjectsPostProcessingPass, "Objects Post processing");

            AddPassContext(UIPass, "UI");
            AddPassContext(UIPostProcessingPass, "UI Post processing");
        }

        /// <inheritdoc/>
        public override void Draw(IGameTime gameTime)
        {
            if (!Updated)
            {
                return;
            }

            Updated = false;

            //Select visible components
            var visibleComponents = Scene.Components.Get<IDrawable>(c => c.Visible);
            if (!visibleComponents.Any())
            {
                return;
            }

#if DEBUG
            frameStats.Clear();
            var swTotal = Stopwatch.StartNew();
#endif
            //Initializes the scene graphics state
            InitializeScene();

            //Shadow mapping
            QueueAction(DoShadowMapping);

            //Draw objects
            bool hasObjects = DrawObjects(visibleComponents.Where(c => !c.Usage.HasFlag(SceneObjectUsages.UI)));

            //Draw UI
            bool hasUI = DrawUI(visibleComponents.Where(c => c.Usage.HasFlag(SceneObjectUsages.UI)));

            //Merge to screen
            QueueAction(() => MergeSceneToScreen(hasObjects, hasUI));

            EndScene();

#if DEBUG
            swTotal.Stop();

            frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
        }
        /// <summary>
        /// Draws the specified object collection
        /// </summary>
        /// <param name="objectComponents">Object collection</param>
        private bool DrawObjects(IEnumerable<IDrawable> objectComponents)
        {
            if (!objectComponents.Any())
            {
                return false;
            }

            QueueAction(() =>
            {
                //Render objects
                var rt = new RenderTargetParameters
                {
                    Target = Targets.Objects,
                    ClearRT = true,
                    ClearRTColor = Scene.GameEnvironment.Background,
                    ClearDepth = true,
                    ClearStencil = true,
                };
                DoRender(rt, objectComponents, CullObjects, ObjectsPass);
            });

            QueueAction(() =>
            {
                //Post-processing
                var rtpp = new RenderTargetParameters
                {
                    Target = Targets.Objects
                };
                DoPostProcessing(rtpp, RenderPass.Objects, ObjectsPostProcessingPass);
            });

            return true;
        }
        /// <summary>
        /// Draws the specified UI components collection
        /// </summary>
        /// <param name="uiComponents">UI components collection</param>
        private bool DrawUI(IEnumerable<IDrawable> uiComponents)
        {
            if (!uiComponents.Any())
            {
                return false;
            }

            QueueAction(() =>
            {
                //Render UI
                var rt = new RenderTargetParameters
                {
                    Target = Targets.UI,
                    ClearRT = true,
                    ClearRTColor = Color.Transparent,
                };
                DoRender(rt, uiComponents, CullUI, UIPass);
            });

            QueueAction(() =>
            {
                //UI post-processing
                var rtpp = new RenderTargetParameters
                {
                    Target = Targets.UI
                };
                DoPostProcessing(rtpp, RenderPass.UI, UIPostProcessingPass);
            });

            return true;
        }
        /// <summary>
        /// Do rendering
        /// </summary>
        /// <param name="renderTarget">Render target</param>
        /// <param name="components">Components</param>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="passIndex">Pass index</param>
        private void DoRender(RenderTargetParameters renderTarget, IEnumerable<IDrawable> components, int cullIndex, int passIndex)
        {
            if (!components.Any())
            {
                return;
            }

#if DEBUG
            var swCull = Stopwatch.StartNew();
#endif
            //Get draw context
            var context = GetDeferredDrawContext(passIndex, "Forward", DrawerModes.Forward);
            bool draw = CullingTest(Scene, context.Camera.Frustum, components, cullIndex);
#if DEBUG
            swCull.Stop();
            frameStats.ForwardCull = swCull.ElapsedTicks;
#endif

            if (!draw)
            {
                return;
            }

            //Store a command list
            var dc = context.DeviceContext;

#if DEBUG
            var swDraw = Stopwatch.StartNew();
#endif
            if (!Scene.Game.BufferManager.SetVertexBuffers(dc))
            {
                return;
            }

            //Binds the result target
            SetTarget(dc, renderTarget);

            //Draw solid
            DrawResultComponents(context, cullIndex, components);
#if DEBUG
            swDraw.Stop();
            frameStats.ForwardDraw = swDraw.ElapsedTicks;
#endif

            QueueCommand(dc.FinishCommandList($"{nameof(DoRender)} {renderTarget.Target}"), passIndex);
        }
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="cullIndex">Cull results index</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(DrawContext context, int cullIndex, IEnumerable<IDrawable> components)
        {
            //Save current drawer mode
            var mode = context.DrawerMode;

            //First opaques
            DrawComponents(context, mode | DrawerModes.OpaqueOnly, cullIndex, components, GetOpaques, SortOpaques);

            //Then transparents
            DrawComponents(context, mode | DrawerModes.TransparentOnly, cullIndex, components, GetTransparents, SortTransparents);

            //Reset drawer mode
            context.DrawerMode = mode;

#if DEBUG
            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(dict);
            }
#endif
        }
        /// <summary>
        /// Draws scene components
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="mode">Draw mode to apply</param>
        /// <param name="cullIndex">Cull results index</param>
        /// <param name="components">Component list</param>
        /// <param name="get">Function to get the drawable component list</param>
        /// <param name="sort">Function to sort the drawable component list</param>
        private void DrawComponents(DrawContext context, DrawerModes mode, int cullIndex, IEnumerable<IDrawable> components, Func<int, IEnumerable<IDrawable>, List<IDrawable>> get, Func<int, IDrawable, IDrawable, int> sort)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            var toDrawComponents = get(cullIndex, components);
#if DEBUG
            sw.Stop();
            WriteTrace($"Mode[{mode}] *Get[{toDrawComponents.Count}]", sw.Elapsed.TotalMilliseconds);
#endif


            if (!toDrawComponents.Any())
            {
                return;
            }

            //Set drawer mode
            context.DrawerMode = mode;


            if (toDrawComponents.Count > 1)
            {
#if DEBUG
                sw.Restart();
#endif
                //Sort items
                toDrawComponents.Sort((c1, c2) => sort(cullIndex, c1, c2));
#if DEBUG
                sw.Stop();
                WriteTrace($"Mode[{mode}] *Sort[{toDrawComponents.Count}]", sw.Elapsed.TotalMilliseconds);
#endif
            }


#if DEBUG
            sw.Restart();
            var swd = Stopwatch.StartNew();
            int count = 0;
#endif
            //Draw items
            for (int i = 0; i < toDrawComponents.Count; i++)
            {
#if DEBUG
                swd.Restart();
#endif
                var c = toDrawComponents[i];
#if DEBUG
                if (Draw(context, c))
                {
                    count++;
                    swd.Stop();
                    WriteTrace($"Mode[{mode}]     Draw[{i}.{c.Name}]=>{c.GetType()}", swd.Elapsed.TotalMilliseconds);
                }
#else
                Draw(context, c);
#endif
            }
#if DEBUG
            sw.Stop();
            WriteTrace($"Mode[{mode}] *Draw[{count} drawn]", sw.Elapsed.TotalMilliseconds);
#endif
        }
    }
}

