#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using System;
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
        /// Frame statistics
        /// </summary>
        private readonly FrameStatsForward frameStats = new();
        /// <summary>
        /// Statistics dictionary
        /// </summary>
        private readonly Dictionary<string, double> dict = new();

        /// <summary>
        /// Writes a trace in the trace dictionary
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="milliseconds">Milliseconds</param>
        private void WriteTrace(string key, double milliseconds)
        {
            dict[key] = milliseconds;
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
        public override void Draw(GameTime gameTime)
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
            DoShadowMapping();

            var objectComponents = visibleComponents.Where(c => !c.Usage.HasFlag(SceneObjectUsages.UI));
            if (objectComponents.Any())
            {
                //Render objects
                DoRender(Targets.Objects, true, Scene.GameEnvironment.Background, true, true, objectComponents, CullObjects, ObjectsPass);
                //Post-processing
                DoPostProcessing(Targets.Objects, RenderPass.Objects, ObjectsPostProcessingPass);
            }

            var uiComponents = visibleComponents.Where(c => c.Usage.HasFlag(SceneObjectUsages.UI));
            if (uiComponents.Any())
            {
                //Render UI
                DoRender(Targets.UI, true, Color.Transparent, false, false, uiComponents, CullUI, UIPass);
                //UI post-processing
                DoPostProcessing(Targets.UI, RenderPass.UI, UIPostProcessingPass);
            }

            //Merge to screen
            MergeToScreen();

#if DEBUG
            swTotal.Stop();

            frameStats.UpdateCounters(swTotal.ElapsedTicks);
#endif
        }

        /// <summary>
        /// Do rendering
        /// </summary>
        /// <param name="target">Render target</param>
        /// <param name="clearRT">Clear render target</param>
        /// <param name="clearRTColor">Clear render target color</param>
        /// <param name="clearDepth">Clear depth buffer</param>
        /// <param name="clearStencil">Clear stencil buffer</param>
        /// <param name="components">Components</param>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="passIndex">Pass index</param>
        private void DoRender(Targets target, bool clearRT, Color4 clearRTColor, bool clearDepth, bool clearStencil, IEnumerable<IDrawable> components, int cullIndex, int passIndex)
        {
            if (!components.Any())
            {
                return;
            }

#if DEBUG
            var swCull = Stopwatch.StartNew();
#endif
            //Get draw context
            var context = GetDeferredDrawContext(passIndex, DrawerModes.Forward, false);
            bool draw = CullingTest(Scene, context.CameraVolume, components.OfType<ICullable>(), cullIndex);
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

            UpdateGlobalState(dc);

            //Binds the result target
            SetTarget(dc, target, clearRT, clearRTColor, clearDepth, clearStencil);

            //Draw solid
            DrawResultComponents(context, cullIndex, components);
#if DEBUG
            swDraw.Stop();
            frameStats.ForwardDraw = swDraw.ElapsedTicks;
#endif

            QueueCommand(dc.FinishCommandList(), passIndex);
        }
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="cullIndex">Cull results index</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(DrawContext context, int cullIndex, IEnumerable<IDrawable> components)
        {
#if DEBUG
            dict.Clear();
            var sw = Stopwatch.StartNew();
#endif
            //Update shaders state
            BuiltIn.BuiltInShaders.UpdatePerFrame(context);
#if DEBUG
            sw.Stop();
            WriteTrace("Built-In shaders Update Per Frame", sw.Elapsed.TotalMilliseconds);
#endif

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
