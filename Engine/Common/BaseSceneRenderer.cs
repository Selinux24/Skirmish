#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Engine.Common
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.PostProcess;

    /// <summary>
    /// Base scene renderer
    /// </summary>
    public abstract class BaseSceneRenderer : ISceneRenderer
    {
        /// <summary>
        /// Render targets
        /// </summary>
        protected enum Targets
        {
            /// <summary>
            /// Objects target
            /// </summary>
            /// <remarks>
            /// All scene objects and their post-processing effects are drawn to this target
            /// </remarks>
            Objects,
            /// <summary>
            /// UI target
            /// </summary>
            /// <remarks>
            /// All user interface components and their post-processing effects are drawn to this target
            /// </remarks>
            UI,
            /// <summary>
            /// Results target
            /// </summary>
            Result,
            /// <summary>
            /// Screen
            /// </summary>
            Screen,
        }
        /// <summary>
        /// Render target parameters
        /// </summary>
        protected struct RenderTargetParameters
        {
            /// <summary>
            /// Render target
            /// </summary>
            public Targets Target { get; set; }
            /// <summary>
            /// Clears the render target using the <see cref="ClearRTColor"/> value
            /// </summary>
            public bool ClearRT { get; set; }
            /// <summary>
            /// Render target clear color
            /// </summary>
            public Color4 ClearRTColor { get; set; }
            /// <summary>
            /// Clears the depth buffer
            /// </summary>
            public bool ClearDepth { get; set; }
            /// <summary>
            /// Clears the stencil buffer
            /// </summary>
            public bool ClearStencil { get; set; }
        }
        /// <summary>
        /// Post-processing target state data
        /// </summary>
        struct PostProcessinStateData
        {
            /// <summary>
            /// Render pass
            /// </summary>
            public RenderPass RenderPass;
            /// <summary>
            /// Post-process state
            /// </summary>
            public BuiltInPostProcessState State;
            /// <summary>
            /// Effect list
            /// </summary>
            public IEnumerable<(BuiltInPostProcessEffects Effect, int TargetIndex)> Effects;
        }

        /// <summary>
        /// Directional shadows pass index
        /// </summary>
        protected const int ShadowsDirectionalPass = 0;
        /// <summary>
        /// Spot shadows pass index
        /// </summary>
        protected const int ShadowsSpotPass = 1;
        /// <summary>
        /// Point shadows pass index
        /// </summary>
        protected const int ShadowsPointPass = 2;
        /// <summary>
        /// Merge to screen pass index
        /// </summary>
        protected const int MergeScreenPass = 3;
        /// <summary>
        /// Next free pass index
        /// </summary>
        protected const int NextPass = 4;

        /// <summary>
        /// Cull index for objects
        /// </summary>
        protected const int CullObjects = 0;
        /// <summary>
        /// Cull index for UI components
        /// </summary>
        protected const int CullUI = 1;
        /// <summary>
        /// First cull index for shadows
        /// </summary>
        protected const int CullShadows = 2;

        /// <summary>
        /// Post-processing objects drawer
        /// </summary>
        private readonly IPostProcessingDrawer processingDrawerObjects = null;
        /// <summary>
        /// Post-processing UI drawer
        /// </summary>
        private readonly IPostProcessingDrawer processingDrawerUI = null;
        /// <summary>
        /// Post-processing results drawer
        /// </summary>
        private readonly IPostProcessingDrawer processingDrawerFinal = null;
        /// <summary>
        /// Scene objects target
        /// </summary>
        private readonly RenderTarget sceneObjectsTarget = null;
        /// <summary>
        /// Scene UI target
        /// </summary>
        private readonly RenderTarget sceneUITarget = null;
        /// <summary>
        /// Scene results target
        /// </summary>
        private readonly RenderTarget sceneResultsTarget = null;

        /// <summary>
        /// First post-processing render target
        /// </summary>
        private readonly RenderTarget postProcessingTarget0 = null;
        /// <summary>
        /// Second post-processing render target
        /// </summary>
        private readonly RenderTarget postProcessingTarget1 = null;
        /// <summary>
        /// Post-processing effects list
        /// </summary>
        private readonly List<PostProcessinStateData> postProcessingEffects = new();

        /// <summary>
        /// Deferred context list
        /// </summary>
        private readonly List<IEngineDeviceContext> deferredContextList = new();
        /// <summary>
        /// Pass list
        /// </summary>
        private readonly List<PassContext> passLists = new();
        /// <summary>
        /// Command list
        /// </summary>
        private readonly ConcurrentBag<(IEngineCommandList Command, int Order)> commandList = new();
        /// <summary>
        /// Action queue
        /// </summary>
        private readonly ConcurrentBag<Action> actions = new();

        /// <summary>
        /// Update materials palette flag
        /// </summary>
        private bool updateMaterialsPalette;
        /// <summary>
        /// Material palette resource
        /// </summary>
        private EngineShaderResourceView materialPalette;
        /// <summary>
        /// Material palette width
        /// </summary>
        private uint materialPaletteWidth;

        /// <summary>
        /// Update animation palette flag
        /// </summary>
        private bool updateAnimationsPalette;
        /// <summary>
        /// Animation palette resource
        /// </summary>
        private EngineShaderResourceView animationPalette;
        /// <summary>
        /// Animation palette width
        /// </summary>
        private uint animationPaletteWidth;

#if DEBUG
        /// <summary>
        /// Shadow mapping stats dictionary
        /// </summary>
        private readonly Dictionary<string, double> shadowMappingDict = new();
#endif

        /// <summary>
        /// Shadow map size
        /// </summary>
        protected const int DirectionalShadowMapSize = 1024 * 4;
        /// <summary>
        /// Maximum number of directional shadow maps
        /// </summary>
        protected const int MaxDirectionalShadowMaps = 1;
        /// <summary>
        /// Maximum number of cascade shadow maps per directional light
        /// </summary>
        protected const int MaxDirectionalCascadeShadowMaps = 3;

        /// <summary>
        /// Cubic shadow map size
        /// </summary>
        protected const int CubicShadowMapSize = 1024;
        /// <summary>
        /// Maximum number of cubic shadow maps
        /// </summary>
        protected const int MaxCubicShadows = 16;

        /// <summary>
        /// Spot light shadow map size
        /// </summary>
        protected const int SpotShadowMapSize = 1024;
        /// <summary>
        /// Max spot shadows
        /// </summary>
        protected const int MaxSpotShadows = 16;

        /// <summary>
        /// Shadow mapper for directional lights
        /// </summary>
        protected IShadowMap ShadowMapperDirectional { get; private set; }
        /// <summary>
        /// Cube shadow mapper for point lights
        /// </summary>
        protected IShadowMap ShadowMapperPoint { get; private set; }
        /// <summary>
        /// Shadow mapper for spot lights
        /// </summary>
        protected IShadowMap ShadowMapperSpot { get; private set; }

        /// <summary>
        /// Scene
        /// </summary>
        protected Scene Scene;
        /// <summary>
        /// Renderer width
        /// </summary>
        protected int Width;
        /// <summary>
        /// Renderer height
        /// </summary>
        protected int Height;
        /// <summary>
        /// Cull manager
        /// </summary>
        protected SceneCullManager cullManager = null;
        /// <summary>
        /// Directional shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapDirectional
        {
            get
            {
                return ShadowMapperDirectional.DepthMapTexture;
            }
        }
        /// <summary>
        /// Point lights shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapPoint
        {
            get
            {
                return ShadowMapperPoint.DepthMapTexture;
            }
        }
        /// <summary>
        /// Spot lights shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapSpot
        {
            get
            {
                return ShadowMapperSpot.DepthMapTexture;
            }
        }
        /// <summary>
        /// Gets or sets whether the renderer was updated
        /// </summary>
        protected bool Updated { get; set; }

        /// <inheritdoc/>
        public bool PostProcessingEnabled
        {
            get
            {
                if (PostProcessingObjectsEffects?.Ready == true)
                {
                    return true;
                }

                if (PostProcessingUIEffects?.Ready == true)
                {
                    return true;
                }

                if (PostProcessingFinalEffects?.Ready == true)
                {
                    return true;
                }

                return false;
            }
        }
        /// <inheritdoc/>
        public BuiltInPostProcessState PostProcessingObjectsEffects { get; set; } = BuiltInPostProcessState.Empty;
        /// <inheritdoc/>
        public BuiltInPostProcessState PostProcessingUIEffects { get; set; } = BuiltInPostProcessState.Empty;
        /// <inheritdoc/>
        public BuiltInPostProcessState PostProcessingFinalEffects { get; set; } = BuiltInPostProcessState.Empty;

        /// <summary>
        /// Gets first normal texture size for the specified pixel count
        /// </summary>
        /// <param name="pixelCount">Pixel count</param>
        /// <returns>Returns the texture size</returns>
        protected static int GetTextureSize(int pixelCount)
        {
            int texWidth = (int)Math.Sqrt((float)pixelCount) + 1;
            int texHeight = 1;
            while (texHeight < texWidth)
            {
                texHeight <<= 1;
            }

            return texHeight;
        }
        /// <summary>
        /// Executes the specified action list in parallel
        /// </summary>
        /// <param name="actionList">Action list</param>
        protected static bool ExecuteParallel(params Action[] actionList)
        {
            return ExecuteParallel(actionList.AsEnumerable());
        }
        /// <summary>
        /// Executes the specified action list in parallel
        /// </summary>
        /// <param name="actionList">Action list</param>
        protected static bool ExecuteParallel(IEnumerable<Action> actionList)
        {
            ParallelOptions options = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            };

            var res = Parallel.ForEach(actionList, options, action =>
            {
                action.Invoke();
            });

            return res.IsCompleted;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        protected BaseSceneRenderer(Scene scene)
        {
            Scene = scene;

            // Directional shadow mapper
            ShadowMapperDirectional = new ShadowMapCascade(
                scene,
                "Directional Shadow Mapper",
                DirectionalShadowMapSize,
                MaxDirectionalCascadeShadowMaps, MaxDirectionalShadowMaps,
                scene.GameEnvironment.CascadeShadowMapsDistances)
            {
                HighResolutionMap = true
            };

            // Point shadow mapper
            ShadowMapperPoint = new ShadowMapPoint(
                scene,
                "Point Shadow Mapper",
                CubicShadowMapSize, CubicShadowMapSize,
                MaxCubicShadows)
            {
                HighResolutionMap = true
            };

            // Spot shadow mapper
            ShadowMapperSpot = new ShadowMapSpot(
                scene,
                "Spot Shadow Mapper",
                SpotShadowMapSize, SpotShadowMapSize,
                MaxSpotShadows)
            {
                HighResolutionMap = true
            };

            cullManager = new SceneCullManager();

            var targetFormat = SharpDX.DXGI.Format.R32G32B32A32_Float;

            sceneObjectsTarget = new RenderTarget(scene.Game, "SceneObjectsTarget", targetFormat, false, 1);
            sceneUITarget = new RenderTarget(scene.Game, "SceneUITarget", targetFormat, false, 1);
            sceneResultsTarget = new RenderTarget(scene.Game, "SceneResultsTarget", targetFormat, false, 1);

            postProcessingTarget0 = new RenderTarget(scene.Game, "PostProcessingTargetA", targetFormat, false, 1);
            postProcessingTarget1 = new RenderTarget(scene.Game, "PostProcessingTargetB", targetFormat, false, 1);
            processingDrawerObjects = new PostProcessingDrawer(scene.Game);
            processingDrawerUI = new PostProcessingDrawer(scene.Game);
            processingDrawerFinal = new PostProcessingDrawer(scene.Game);

            updateMaterialsPalette = true;
            updateAnimationsPalette = true;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BaseSceneRenderer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ShadowMapperDirectional.Dispose();
                ShadowMapperPoint.Dispose();
                ShadowMapperSpot.Dispose();

                sceneObjectsTarget.Dispose();
                sceneUITarget.Dispose();
                sceneResultsTarget.Dispose();
                postProcessingTarget0.Dispose();
                postProcessingTarget1.Dispose();

                deferredContextList.ForEach(dc => dc.Dispose());
                deferredContextList.Clear();
            }
        }

        /// <inheritdoc/>
        public virtual void Resize()
        {
            sceneObjectsTarget.Resize();
            sceneUITarget.Resize();
            sceneResultsTarget.Resize();
            postProcessingTarget0.Resize();
            postProcessingTarget1.Resize();

            processingDrawerObjects.Resize();
            processingDrawerUI.Resize();
            processingDrawerFinal.Resize();
        }
        /// <inheritdoc/>
        public virtual EngineShaderResourceView GetResource(SceneRendererResults result)
        {
            if (result == SceneRendererResults.ShadowMapDirectional) return ShadowMapDirectional;
            if (result == SceneRendererResults.ShadowMapPoint) return ShadowMapPoint;
            if (result == SceneRendererResults.ShadowMapSpot) return ShadowMapSpot;
            return null;
        }

        /// <inheritdoc/>
        public virtual void Update(GameTime gameTime)
        {
            //Updates the update context
            var updateContext = GetUpdateContext(gameTime);

            //Cull lights
            Scene.Lights.Cull((IntersectionVolumeFrustum)updateContext.Camera.Frustum, updateContext.Camera.Position, Scene.GameEnvironment.LODDistanceLow);

            //Update active components
            var updatables = Scene.Components.Get<IUpdatable>(c => c.Active);
            if (updatables.Any())
            {
                updatables
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(c => c.EarlyUpdate(updateContext));

                updatables
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(c => c.Update(updateContext));

                updatables
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(c => c.LateUpdate(updateContext));
            }

            Updated = true;
        }
        /// <summary>
        /// Updates the update context
        /// </summary>
        /// <param name="gameTime">Game time</param>
        protected virtual UpdateContext GetUpdateContext(GameTime gameTime)
        {
            return new UpdateContext
            {
                GameTime = gameTime,
                Camera = Scene.Camera,
                Lights = Scene.Lights,
            };
        }

        /// <summary>
        /// Creates a deferred context
        /// </summary>
        /// <param name="name">Pass name</param>
        /// <param name="passIndex">Pass index</param>
        private IEngineDeviceContext GetDeferredContext(string name, int passIndex)
        {
            var graphics = Scene.Game.Graphics;

            while (passIndex >= deferredContextList.Count)
            {
                deferredContextList.Add(graphics.CreateDeferredContext(name, passIndex));
            }

            return deferredContextList[passIndex];
        }
        /// <summary>
        /// Gets the immediate draw context
        /// </summary>
        /// <param name="drawMode">Draw mode</param>
        protected DrawContext GetImmediateDrawContext(DrawerModes drawMode)
        {
            return new DrawContext
            {
                Name = $"{drawMode} immediate context.",

                DrawerMode = drawMode,

                //Scene data
                GameTime = Scene.Game.GameTime,
                Form = Scene.Game.Form,
                Camera = Scene.Camera,
                Lights = Scene.Lights,
                LevelOfDetail = Scene.GameEnvironment.GetLODDistances(),

                //Shadow mappers
                ShadowMapDirectional = ShadowMapperDirectional,
                ShadowMapPoint = ShadowMapperPoint,
                ShadowMapSpot = ShadowMapperSpot,

                //Pass context
                PassContext = new PassContext
                {
                    Name = "Immediate",
                    PassIndex = -1,
                    DeviceContext = Scene.Game.Graphics.ImmediateContext,
                },
            };
        }
        /// <summary>
        /// Gets a deferred draw context
        /// </summary>
        /// <param name="passIndex">Pass index</param>
        /// <param name="drawMode">Draw mode</param>
        protected DrawContext GetDeferredDrawContext(int passIndex, DrawerModes drawMode)
        {
            var passContext = passLists[passIndex];
            passContext.DeviceContext.ClearState();

            return new DrawContext
            {
                Name = $"{drawMode} pass[{passIndex}] context.",

                DrawerMode = drawMode,

                //Scene data
                GameTime = Scene.Game.GameTime,
                Form = Scene.Game.Form,
                Camera = Scene.Camera,
                Lights = Scene.Lights,
                LevelOfDetail = Scene.GameEnvironment.GetLODDistances(),

                //Shadow mappers
                ShadowMapDirectional = ShadowMapperDirectional,
                ShadowMapPoint = ShadowMapperPoint,
                ShadowMapSpot = ShadowMapperSpot,

                //Pass context
                PassContext = passContext,
            };
        }
        /// <summary>
        /// Gets per light draw context
        /// </summary>
        /// <param name="passIndex">Pass index</param>
        /// <param name="name">Name</param>
        /// <param name="shadowMapper">Shadow mapper</param>
        protected virtual DrawContextShadows GetPerLightDrawContext(int passIndex, string name, IShadowMap shadowMapper)
        {
            var passContext = passLists[passIndex];
            passContext.DeviceContext.ClearState();

            return new DrawContextShadows()
            {
                Name = $"{name} pass[{passIndex}] context.",

                //Scene data
                Camera = Scene.Camera,

                //Shadow mapper
                ShadowMap = shadowMapper,

                //Pass context
                PassContext = passContext,
            };
        }

        /// <inheritdoc/>
        public abstract void Draw(GameTime gameTime);

        /// <summary>
        /// Gets opaque components
        /// </summary>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="components">Component list</param>
        /// <returns>Returns the opaque components</returns>
        protected virtual List<IDrawable> GetOpaques(int cullIndex, IEnumerable<IDrawable> components)
        {
            var opaques = components.Where(c =>
            {
                if (c is null) return false;

                if (!c.BlendMode.HasFlag(BlendModes.Opaque)) return false;

                if (c is ICullable cull)
                {
                    return !cullManager.GetCullValue(cullIndex, cull).Culled;
                }

                return true;
            });

            return opaques.ToList();
        }
        /// <summary>
        /// Sorting opaque list comparer
        /// </summary>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="c1">First component</param>
        /// <param name="c2">Second component</param>
        /// <returns>Returns sorting order (nearest first)</returns>
        protected virtual int SortOpaques(int cullIndex, IDrawable c1, IDrawable c2)
        {
            int res = c1.Layer.CompareTo(c2.Layer);

            if (res == 0)
            {
                res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
            }

            if (res == 0)
            {
                float d1 = float.MaxValue;
                if (c1 is ICullable cull1)
                {
                    d1 = cullManager.GetCullValue(cullIndex, cull1).Distance;
                }

                float d2 = float.MaxValue;
                if (c2 is ICullable cull2)
                {
                    d2 = cullManager.GetCullValue(cullIndex, cull2).Distance;
                }

                // Nearest first
                res = -d1.CompareTo(d2);
            }

            if (res == 0)
            {
                res = c1.BlendMode.CompareTo(c2.BlendMode);
            }

            return res;
        }
        /// <summary>
        /// Gets transparent components
        /// </summary>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="components">Component list</param>
        /// <returns>Returns the transparent components</returns>
        protected virtual List<IDrawable> GetTransparents(int cullIndex, IEnumerable<IDrawable> components)
        {
            var transparents = components.Where(c =>
            {
                if (c is null) return false;

                if (!c.BlendMode.HasFlag(BlendModes.Alpha) && !c.BlendMode.HasFlag(BlendModes.Transparent))
                {
                    return false;
                }

                if (c is ICullable cull)
                {
                    return !cullManager.GetCullValue(cullIndex, cull).Culled;
                }

                return true;
            });

            return transparents.ToList();
        }
        /// <summary>
        /// Sorting transparent list comparer
        /// </summary>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="c1">First component</param>
        /// <param name="c2">Second component</param>
        /// <returns>Returns sorting order (far first)</returns>
        protected virtual int SortTransparents(int cullIndex, IDrawable c1, IDrawable c2)
        {
            int res = c1.Layer.CompareTo(c2.Layer);

            if (res == 0)
            {
                res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
            }

            if (res == 0)
            {
                float d1 = float.MaxValue;
                if (c1 is ICullable cull1)
                {
                    d1 = cullManager.GetCullValue(cullIndex, cull1).Distance;
                }

                float d2 = float.MaxValue;
                if (c2 is ICullable cull2)
                {
                    d2 = cullManager.GetCullValue(cullIndex, cull2).Distance;
                }

                // Far objects first
                res = d1.CompareTo(d2);
            }

            if (res == 0)
            {
                res = c1.BlendMode.CompareTo(c2.BlendMode);
            }

            return res;
        }

        /// <inheritdoc/>
        public virtual void PrepareScene()
        {
            passLists.Clear();

            AddPassContext(ShadowsDirectionalPass, "Directional Shadows");
            AddPassContext(ShadowsSpotPass, "Spot Shadows");
            AddPassContext(ShadowsPointPass, "Point Shadows");
            AddPassContext(MergeScreenPass, "Merge to Screen");
        }
        /// <summary>
        /// Adds a new pass to the pass list collection
        /// </summary>
        /// <param name="passIndex">Pass index</param>
        /// <param name="name">Pass name</param>
        protected void AddPassContext(int passIndex, string name)
        {
            if (passLists.Exists(c => c.PassIndex == passIndex))
            {
                return;
            }

            var dc = GetDeferredContext($"Deferred Context({name}.{passIndex})", passIndex);
            passLists.Add(new PassContext
            {
                PassIndex = passIndex,
                Name = name,
                DeviceContext = dc,
            });
        }
        /// <summary>
        /// Queues a command in the command list by order
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="order">Order</param>
        protected void QueueCommand(IEngineCommandList command, int order)
        {
            if (command == null)
            {
                return;
            }

            commandList.Add((command, order));
        }
        /// <summary>
        /// Queues a action to the action queue
        /// </summary>
        /// <param name="action">Action</param>
        protected void QueueAction(Action action)
        {
            actions.Add(action);
        }

        /// <inheritdoc/>
        public virtual void UpdateGlobals()
        {
            updateMaterialsPalette = true;
            updateAnimationsPalette = true;
        }

        /// <summary>
        /// Performs the culling test
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="cameraVolume">Camera volume</param>
        /// <param name="components">Components collection to test</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns>Returns true if the test find components to draw</returns>
        protected virtual bool CullingTest(Scene scene, IntersectionVolumeFrustum cameraVolume, IEnumerable<IDrawable> components, int cullIndex)
        {
            if (!scene.PerformFrustumCulling)
            {
                return false;
            }

            var cullables = components?.OfType<ICullable>() ?? Enumerable.Empty<ICullable>();
            if (!cullables.Any())
            {
                return false;
            }

            return cullManager.Cull(cullIndex, cameraVolume, cullables);
        }

        /// <summary>
        /// Draws an object
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="drawable">Drawable component</param>
        protected virtual bool Draw(DrawContext context, IDrawable drawable)
        {
            var blend = drawable.BlendMode;
            if (drawable.Usage.HasFlag(SceneObjectUsages.UI))
            {
                blend |= BlendModes.PostProcess;
            }

            var dc = context.DeviceContext;

            SetRasterizerDefault(dc);
            SetBlendState(dc, context.DrawerMode, blend);
            SetDepthStencil(dc, drawable.DepthEnabled);

            return drawable.Draw(context);
        }
        /// <summary>
        /// Sets the rasterizer state
        /// </summary>
        /// <param name="dc">Device context</param>
        protected virtual void SetRasterizerDefault(IEngineDeviceContext dc)
        {
            dc.SetRasterizerState(Scene.Game.Graphics.GetRasterizerDefault());
        }
        /// <summary>
        /// Sets the blend state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="drawerMode">Draw mode</param>
        /// <param name="blendMode">Blend mode</param>
        protected virtual void SetBlendState(IEngineDeviceContext dc, DrawerModes drawerMode, BlendModes blendMode)
        {
            dc.SetBlendState(Scene.Game.Graphics.GetBlendState(blendMode));
        }
        /// <summary>
        /// Sets the depth-stencil buffer state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="enableWrite">Enables the z-buffer writing</param>
        protected virtual void SetDepthStencil(IEngineDeviceContext dc, bool enableWrite)
        {
            if (enableWrite)
            {
                dc.SetDepthStencilState(Scene.Game.Graphics.GetDepthStencilWRZEnabled());
            }
            else
            {
                dc.SetDepthStencilState(Scene.Game.Graphics.GetDepthStencilWRZDisabled());
            }
        }

        /// <summary>
        /// Draw shadow maps
        /// </summary>
        protected void DoShadowMapping()
        {
#if DEBUG
            shadowMappingDict.Clear();
#endif

            int cullIndexDir = CullShadows;
            int cullIndexPoint = cullIndexDir + Scene.Lights.DirectionalLights.Length;
            int cullIndexSpot = cullIndexPoint + Scene.Lights.PointLights.Length;

            var camPosition = Scene.Camera.Position;

            //Get directional lights which cast shadows
            var dirLights = Scene.Lights.GetDirectionalShadowCastingLights(Scene.GameEnvironment, camPosition);
            //Get the object list affected by directional shadows
            var dirObjs = Scene.Components.Get<IDrawable>(c => c.Visible && c.CastShadow.HasFlag(ShadowCastingAlgorihtms.Directional));
            //Draw shadow map
            DoShadowMapping(ShadowsDirectionalPass, "Directional", ShadowMapperDirectional, dirLights, dirObjs, cullIndexDir);

            //Get point lights which cast shadows
            var pointLights = Scene.Lights.GetPointShadowCastingLights(Scene.GameEnvironment, camPosition);
            //Get the object list affected by point shadows
            var pointObjs = Scene.Components.Get<IDrawable>(c => c.Visible && c.CastShadow.HasFlag(ShadowCastingAlgorihtms.Point));
            //Draw shadow map
            DoShadowMapping(ShadowsPointPass, "Point", ShadowMapperPoint, pointLights, pointObjs, cullIndexPoint);

            //Get spot lights which cast shadows
            var spotLights = Scene.Lights.GetSpotShadowCastingLights(Scene.GameEnvironment, camPosition);
            //Get the object list affected by spot shadows
            var spotObjs = Scene.Components.Get<IDrawable>(c => c.Visible && c.CastShadow.HasFlag(ShadowCastingAlgorihtms.Spot));
            //Draw shadow map
            DoShadowMapping(ShadowsSpotPass, "Spot", ShadowMapperSpot, spotLights, spotObjs, cullIndexSpot);

#if DEBUG
            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(shadowMappingDict);
            }
#endif
        }
        /// <summary>
        /// Draw shadow maps
        /// </summary>
        /// <param name="passIndex">Pass index</param>
        /// <param name="passName">Pass name</param>
        /// <param name="shadowMapper">Shadow mapper</param>
        /// <param name="shadowCastingLights">Lights</param>
        /// <param name="shadowObjs">Affected objects</param>
        /// <param name="cullIndex">Cull index</param>
        private void DoShadowMapping(int passIndex, string passName, IShadowMap shadowMapper, IEnumerable<ISceneLight> shadowCastingLights, IEnumerable<IDrawable> shadowObjs, int cullIndex)
        {
#if DEBUG
            var gStopwatch = Stopwatch.StartNew();
#endif

            if (!shadowCastingLights.Any())
            {
                return;
            }

            if (!shadowObjs.Any())
            {
                return;
            }

            //Crate the draw context for lights
            var drawContext = GetPerLightDrawContext(passIndex, passName, shadowMapper);
            var dc = drawContext.DeviceContext;

            if (!Scene.Game.BufferManager.SetVertexBuffers(dc))
            {
                return;
            }

            var toCullShadowObjs = shadowObjs.OfType<ICullable>();
            //Get if all affected objects are suitable for cull testing
            bool allCullingObjects = shadowObjs.Count() == toCullShadowObjs.Count();

            var lArray = shadowCastingLights.ToArray();
            for (int l = 0; l < lArray.Length; l++)
            {
                var light = lArray[l];

                if (light.ShadowMapIndex < 0)
                {
                    continue;
                }

                int lCullIndex = cullIndex + l;

                //Cull testing
                var lightVolume = light.GetLightVolume();
                if (!DoShadowCullingTest(toCullShadowObjs, lCullIndex, allCullingObjects, lightVolume))
                {
                    continue;
                }

                DrawLight(drawContext, shadowObjs, lCullIndex, light);
            }

#if DEBUG
            gStopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoShadowMapping)}.{passName}({passIndex}) TOTAL", gStopwatch.Elapsed.TotalMilliseconds);
#endif

            QueueCommand(dc.FinishCommandList($"{nameof(DoShadowMapping)}.{passName}"), passIndex);
        }
        /// <summary>
        /// Performs shadow culling testing
        /// </summary>
        /// <param name="components">Component list</param>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="allCullingObjects">All components were culling components</param>
        /// <param name="lightVolume">Light volume</param>
        private bool DoShadowCullingTest(IEnumerable<ICullable> components, int cullIndex, bool allCullingObjects, ICullingVolume lightVolume)
        {
            if (!components.Any())
            {
                return true;
            }

#if DEBUG
            var stopwatch = Stopwatch.StartNew();
#endif

            var doShadows = cullManager.Cull(cullIndex, lightVolume, components);

#if DEBUG
            stopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoShadowCullingTest)} - Cull {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (allCullingObjects && !doShadows)
            {
                //All objects suitable for culling but no one pass the culling test
                return false;
            }

            return true;
        }
        /// <summary>
        /// Draws a light in a shadow map
        /// </summary>
        /// <param name="drawContext">Drawing context</param>
        /// <param name="components">Object list affected by the light</param>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="light">Light</param>
        private void DrawLight(DrawContextShadows drawContext, IEnumerable<IDrawable> components, int cullIndex, ISceneLight light)
        {
#if DEBUG
            var stopwatch = Stopwatch.StartNew();
#endif
            drawContext.ShadowMap.LightSource = light;
            drawContext.ShadowMap.CullIndex = cullIndex;
            drawContext.ShadowMap.Bind(drawContext.DeviceContext);

            //Draw
            DrawShadowComponents(drawContext, components, cullIndex);
#if DEBUG
            stopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DrawLight)} {light.GetType()} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif
        }
        /// <summary>
        /// Draw components for shadow mapping
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="components">Components to draw</param>
        /// <param name="cullIndex">Culling index</param>
        private void DrawShadowComponents(DrawContextShadows context, IEnumerable<IDrawable> components, int cullIndex)
        {
            var objects = components
                .Where(c => IsVisible(c, cullIndex))
                .ToList();

            if (!objects.Any())
            {
                return;
            }

            objects.Sort((c1, c2) => Sort(c1, c2, cullIndex));

            objects.ForEach((c) => DrawShadows(context, c));
        }
        /// <summary>
        /// Gets if the specified object is not culled by the cull index
        /// </summary>
        /// <param name="c">Scene object</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns>Returns true if the object is not culled</returns>
        private bool IsVisible(IDrawable c, int cullIndex)
        {
            if (c is ICullable cull)
            {
                return !cullManager.GetCullValue(cullIndex, cull).Culled;
            }

            return true;
        }
        /// <summary>
        /// Sorts an object list by distance to culling point of view
        /// </summary>
        /// <param name="c1">Scene object one</param>
        /// <param name="c2">Scene object two</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns></returns>
        private int Sort(IDrawable c1, IDrawable c2, int cullIndex)
        {
            int res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
            if (res == 0)
            {
                float d1 = float.MaxValue;
                if (c1 is ICullable cull1)
                {
                    d1 = cullManager.GetCullValue(cullIndex, cull1).Distance;
                }

                float d2 = float.MaxValue;
                if (c2 is ICullable cull2)
                {
                    d2 = cullManager.GetCullValue(cullIndex, cull2).Distance;
                }

                res = -d1.CompareTo(d2);
            }

            if (res == 0)
            {
                res = c1.Layer.CompareTo(c2.Layer);
            }

            return res;
        }
        /// <summary>
        /// Draws the specified object shadows
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="drawable">Drawable object</param>
        private void DrawShadows(DrawContextShadows context, IDrawable drawable)
        {
            var graphics = Scene.Game.Graphics;
            var dc = context.DeviceContext;

            dc.SetRasterizerState(graphics.GetRasterizerShadowMapping());
            dc.SetDepthStencilState(graphics.GetDepthStencilShadowMapping());

            if (drawable.BlendMode.HasFlag(BlendModes.Alpha) || drawable.BlendMode.HasFlag(BlendModes.Transparent))
            {
                dc.SetBlendState(graphics.GetBlendAlpha());
            }
            else
            {
                dc.SetBlendState(graphics.GetBlendDefault());
            }

            drawable.DrawShadows(context);
        }

        /// <summary>
        /// Binds graphics for results pass
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="renderTarget">Render target parameters</param>
        protected virtual void SetTarget(IEngineDeviceContext dc, RenderTargetParameters renderTarget)
        {
            switch (renderTarget.Target)
            {
                case Targets.Screen:
                    BindDefaultTarget(dc, renderTarget.ClearRT, renderTarget.ClearRTColor);
                    break;
                case Targets.Objects:
                    BindObjectsTarget(dc, renderTarget.ClearRT, renderTarget.ClearRTColor, renderTarget.ClearDepth, renderTarget.ClearStencil);
                    break;
                case Targets.UI:
                    BindUITarget(dc, renderTarget.ClearRT, renderTarget.ClearRTColor);
                    break;
                case Targets.Result:
                    BindResultsTarget(dc, renderTarget.ClearRT, renderTarget.ClearRTColor);
                    break;
                default:
                    BindDefaultTarget(dc, renderTarget.ClearRT, renderTarget.ClearRTColor);
                    break;
            }
        }
        /// <summary>
        /// Binds the default render target
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        private void BindDefaultTarget(IEngineDeviceContext dc, bool clearRT, Color4 clearRTColor)
        {
            var graphics = Scene.Game.Graphics;

            //Restore back buffer as render target and clear it
            dc.SetRenderTargets(graphics.DefaultRenderTarget, clearRT, clearRTColor);
            dc.SetViewport(graphics.Viewport);
        }
        /// <summary>
        /// Binds the objects render target
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        private void BindObjectsTarget(IEngineDeviceContext dc, bool clearRT, Color4 clearRTColor, bool clearDepth, bool clearStencil)
        {
            var graphics = Scene.Game.Graphics;

            dc.SetRenderTargets(sceneObjectsTarget.Targets, clearRT, clearRTColor, graphics.DefaultDepthStencil, clearDepth, clearStencil, false);
            dc.SetViewport(graphics.Viewport); //Set default viewport
        }
        /// <summary>
        /// Binds the UI render target
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        private void BindUITarget(IEngineDeviceContext dc, bool clearRT, Color4 clearRTColor)
        {
            dc.SetRenderTargets(sceneUITarget.Targets, clearRT, clearRTColor);
            dc.SetViewport(Scene.Game.Graphics.Viewport); //Set default viewport
        }
        /// <summary>
        /// Binds the results render target
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        private void BindResultsTarget(IEngineDeviceContext dc, bool clearRT, Color4 clearRTColor)
        {
            dc.SetRenderTargets(sceneResultsTarget.Targets, clearRT, clearRTColor);
            dc.SetViewport(Scene.Game.Graphics.Viewport); //Set default viewport
        }
        /// <summary>
        /// Binds graphics for post-processing pass
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="target">Render target</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        private void BindPostProcessingTarget(IEngineDeviceContext dc, RenderTarget target, bool clearRT, Color4 clearRTColor)
        {
            dc.SetRenderTargets(target.Targets, clearRT, clearRTColor);
            dc.SetViewport(Scene.Game.Form.GetViewport()); //Set local viewport
        }
        /// <summary>
        /// Gets the target textures
        /// </summary>
        /// <param name="target">Target type</param>
        /// <returns>Returns the target texture list</returns>
        private IEnumerable<EngineShaderResourceView> GetTargetTextures(Targets target)
        {
            return target switch
            {
                Targets.Screen => Enumerable.Empty<EngineShaderResourceView>(),
                Targets.Objects => sceneObjectsTarget.Textures,
                Targets.UI => sceneUITarget.Textures,
                Targets.Result => sceneResultsTarget.Textures,
                _ => Enumerable.Empty<EngineShaderResourceView>(),
            };
        }

        /// <inheritdoc/>
        public void ClearPostProcessingEffects()
        {
            PostProcessingObjectsEffects = BuiltInPostProcessState.Empty;
            PostProcessingUIEffects = BuiltInPostProcessState.Empty;
            PostProcessingFinalEffects = BuiltInPostProcessState.Empty;
        }

        /// <summary>
        /// Does the post-processing draw
        /// </summary>
        /// <param name="renderTarget">Render target</param>
        /// <param name="renderPass">Render pass</param>
        /// <param name="passIndex">Pass index</param>
        protected void DoPostProcessing(RenderTargetParameters renderTarget, RenderPass renderPass, int passIndex)
        {
            var passEffects = postProcessingEffects.Where(ppe => ppe.RenderPass == renderPass);
            if (!passEffects.Any())
            {
                return;
            }

            var pass = passLists[passIndex];
            var dc = pass.DeviceContext;
            dc.ClearState();

            if (!Scene.Game.BufferManager.SetVertexBuffers(dc))
            {
                return;
            }

            DrawPostProcessing(dc, renderTarget, passEffects.First());

            QueueCommand(dc.FinishCommandList($"{nameof(DoPostProcessing)} {renderPass}"), passIndex);
        }
        /// <summary>
        /// Does the post-processing draw
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="renderTarget">Render target</param>
        /// <param name="renderPass">Render pass</param>
        private void DrawPostProcessing(IEngineDeviceContext dc, RenderTargetParameters renderTarget, PostProcessinStateData state)
        {
            if (state.Effects?.Any() != true)
            {
                return;
            }

            var processingDrawer = state.RenderPass switch
            {
                RenderPass.Objects => processingDrawerObjects,
                RenderPass.UI => processingDrawerUI,
                RenderPass.Final => processingDrawerFinal,
                _ => throw new NotImplementedException(),
            };

            //Gets the last used target as source texture for the post-processing shader
            var source = GetTargetTextures(renderTarget.Target)?.FirstOrDefault();

            var graphics = Scene.Game.Graphics;
            dc.SetRasterizerState(graphics.GetRasterizerCullNone());
            dc.SetDepthStencilState(graphics.GetDepthStencilNone());
            dc.SetBlendState(graphics.GetBlendDefault());

            for (int i = 0; i < state.Effects.Count(); i++)
            {
                var (effect, targetIndex) = state.Effects.ElementAt(i);

                //Toggles post-processing buffers
                var target = GetPostProcessingTargets(targetIndex);
                var targetTexture = target.Textures?.FirstOrDefault();

                //Use the next buffer as render target
                BindPostProcessingTarget(dc, target, false, Color.Transparent);

                processingDrawer.Draw(dc, source, effect, state.State);

                //Gets the source texture
                source = targetTexture;
            }

            //Set the result render target
            SetTarget(dc, renderTarget);

            //Draw the result
            processingDrawer.Draw(dc, source, BuiltInPostProcessEffects.None, state.State);
        }
        /// <summary>
        /// Toggles post-processing render targets
        /// </summary>
        /// <param name="index">Target index</param>
        private RenderTarget GetPostProcessingTargets(int index)
        {
            if (index % 2 == 0)
            {
                return postProcessingTarget0;
            }
            else
            {
                return postProcessingTarget1;
            }
        }

        /// <summary>
        /// Initializes the scene state
        /// </summary>
        protected void InitializeScene()
        {
            commandList.Clear();
            actions.Clear();

            AssignLightShadowMaps();
            AssignPostProcessTargets();

            var context = GetImmediateDrawContext(DrawerModes.None);

            if (RefreshGlobalState())
            {
                BuiltInShaders.UpdateGlobals(context, materialPalette, materialPaletteWidth, animationPalette, animationPaletteWidth);
            }

            BuiltInShaders.UpdatePerFrame(context);
        }
        /// <summary>
        /// Refresh the global resources state
        /// </summary>
        /// <returns>Returns true if the global state changes</returns>
        private bool RefreshGlobalState()
        {
            bool updated = false;

            if (updateMaterialsPalette)
            {
                BuildMaterialPalette(out materialPalette, out materialPaletteWidth);

                updateMaterialsPalette = false;

                updated = true;
            }

            if (updateAnimationsPalette)
            {
                BuildAnimationPalette(out animationPalette, out animationPaletteWidth);

                updateAnimationsPalette = false;

                updated = true;
            }

            return updated;
        }
        /// <summary>
        /// Builds the global material palette
        /// </summary>
        /// <param name="materialPalette">Material palette</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        private void BuildMaterialPalette(out EngineShaderResourceView materialPalette, out uint materialPaletteWidth)
        {
            Logger.WriteInformation(this, $"{nameof(BuildMaterialPalette)} =>Building Material palette.");

            List<IMeshMaterial> mats = new()
            {
                MeshMaterial.DefaultBlinnPhong,
            };

            var matComponents = Scene.Components.Get<IUseMaterials>().SelectMany(c => c.GetMaterials());
            if (matComponents.Any())
            {
                mats.AddRange(matComponents);
            }

            List<Vector4> values = new();

            for (int i = 0; i < mats.Count; i++)
            {
                var mat = mats[i];
                var matV = mat.Material.Convert().Pack();

                mat.UpdateResource((uint)i, (uint)values.Count, (uint)matV.Length);

                values.AddRange(matV);
            }

            int texWidth = GetTextureSize(values.Count);

            materialPalette = Scene.Game.ResourceManager.CreateGlobalResource("MaterialPalette", values, texWidth);
            materialPaletteWidth = (uint)texWidth;
        }
        /// <summary>
        /// Builds the global animation palette
        /// </summary>
        /// <param name="animationPalette">Animation palette</param>
        /// <param name="animationPaletteWidth">Animation palette width</param>
        private void BuildAnimationPalette(out EngineShaderResourceView animationPalette, out uint animationPaletteWidth)
        {
            Logger.WriteInformation(this, $"{nameof(BuildAnimationPalette)} =>Building Animation palette.");

            var skData = Scene.Components.Get<IUseSkinningData>(c => c.SkinningData != null)
                .Select(c => c.SkinningData)
                .ToArray();

            List<ISkinningData> addedSks = new();

            List<Vector4> values = new();

            for (int i = 0; i < skData.Length; i++)
            {
                var sk = skData[i];

                if (!addedSks.Contains(sk))
                {
                    var skV = sk.Pack();

                    sk.UpdateResource((uint)addedSks.Count, (uint)values.Count, (uint)skV.Count());

                    values.AddRange(skV);

                    addedSks.Add(sk);
                }
                else
                {
                    var cMat = addedSks.Find(m => m.Equals(sk));

                    sk.UpdateResource(cMat.ResourceIndex, cMat.ResourceOffset, cMat.ResourceSize);
                }
            }

            int texWidth = GetTextureSize(values.Count);

            animationPalette = Scene.Game.ResourceManager.CreateGlobalResource("AnimationPalette", values.ToArray(), texWidth);
            animationPaletteWidth = (uint)texWidth;
        }
        /// <summary>
        /// Assign light information for shadow mapping
        /// </summary>
        private void AssignLightShadowMaps()
        {
            ExecuteParallel(AssignLightShadowMapsDirectional, AssignLightShadowMapsPoint, AssignLightShadowMapsSpot);
        }
        /// <summary>
        /// Assign light information for directional shadow mapping
        /// </summary>
        private void AssignLightShadowMapsDirectional()
        {
            var dirLights = Scene.Lights.GetDirectionalShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position).ToArray();
            if (!dirLights.Any())
            {
                return;
            }

            var camera = Scene.Camera;

            int assigned = 0;
            foreach (var light in dirLights)
            {
                light.ClearShadowParameters();

                light.UpdateEnvironment(DirectionalShadowMapSize, Scene.GameEnvironment.CascadeShadowMapsDistances);

                if (assigned >= MaxDirectionalShadowMaps)
                {
                    continue;
                }

                //Assign light parameters
                light.SetShadowParameters(camera, assigned++);
            }
        }
        /// <summary>
        /// Assign light information for point shadow mapping
        /// </summary>
        private void AssignLightShadowMapsPoint()
        {
            var pointLights = Scene.Lights.GetPointShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
            if (!pointLights.Any())
            {
                return;
            }

            var camera = Scene.Camera;

            int assigned = 0;
            foreach (var light in pointLights)
            {
                light.ClearShadowParameters();

                if (assigned >= MaxCubicShadows)
                {
                    continue;
                }

                //Assign light parameters
                light.SetShadowParameters(camera, assigned++);
            }
        }
        /// <summary>
        /// Assign light information for spot shadow mapping
        /// </summary>
        private void AssignLightShadowMapsSpot()
        {
            var spotLights = Scene.Lights.GetSpotShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
            if (!spotLights.Any())
            {
                return;
            }

            var camera = Scene.Camera;

            int assigned = 0;
            foreach (var light in spotLights)
            {
                light.ClearShadowParameters();

                if (assigned >= MaxSpotShadows)
                {
                    continue;
                }

                //Assign light parameters
                light.SetShadowParameters(camera, assigned++);
            }
        }
        /// <summary>
        /// Pre-assing post-processing targets
        /// </summary>
        private void AssignPostProcessTargets()
        {
            postProcessingEffects.Clear();

            int targetIndex = 0;

            if (PostProcessingObjectsEffects.Ready)
            {
                var effects = PostProcessingObjectsEffects.GetEffects();

                postProcessingEffects.Add(new PostProcessinStateData
                {
                    State = PostProcessingObjectsEffects,
                    RenderPass = RenderPass.Objects,
                    Effects = effects.Select(e => (e, targetIndex++ % 2)).ToArray(),
                });
            }

            if (PostProcessingUIEffects.Ready)
            {
                var effects = PostProcessingUIEffects.GetEffects();

                postProcessingEffects.Add(new PostProcessinStateData
                {
                    State = PostProcessingUIEffects,
                    RenderPass = RenderPass.UI,
                    Effects = effects.Select(e => (e, targetIndex++ % 2)).ToArray(),
                });
            }

            if (PostProcessingFinalEffects.Ready)
            {
                var effects = PostProcessingFinalEffects.GetEffects();

                postProcessingEffects.Add(new PostProcessinStateData
                {
                    State = PostProcessingFinalEffects,
                    RenderPass = RenderPass.Final,
                    Effects = effects.Select(e => (e, targetIndex++ % 2)).ToArray(),
                });
            }
        }

        /// <summary>
        /// Merges the scene targets to screen
        /// </summary>
        /// <param name="hasObjects">Sets whether the current pass, includes objects phase or not</param>
        /// <param name="hasUI">Sets whether the current pass, includes UI phase or not</param>
        protected void MergeSceneToScreen(bool hasObjects, bool hasUI)
        {
            var pass = passLists[MergeScreenPass];
            var dc = pass.DeviceContext;
            dc.ClearState();

            if (!Scene.Game.BufferManager.SetVertexBuffers(dc))
            {
                return;
            }

            //Select source render target to copy to screen
            Targets source;
            if (hasObjects && hasUI)
            {
                //Combine object and ui targets into to result target
                CombineTargets(dc, Targets.Objects, Targets.UI, Targets.Result);
                source = Targets.Result;
            }
            else if (hasObjects)
            {
                source = Targets.Objects;
            }
            else if (hasUI)
            {
                source = Targets.UI;
            }
            else
            {
                //Nothing to do
                return;
            }

            var passEffects = postProcessingEffects.Where(ppe => ppe.RenderPass == RenderPass.Final);
            if (passEffects.Any())
            {
                //Final post-processing in the result target
                var rtpp = new RenderTargetParameters
                {
                    Target = source,
                };
                DrawPostProcessing(dc, rtpp, passEffects.First());
            }

            //Draw from source target to screen
            DrawToScreen(dc, source);

            QueueCommand(dc.FinishCommandList(nameof(MergeSceneToScreen)), int.MaxValue);
        }
        /// <summary>
        /// Combine the specified targets into the result target
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="target1">Target 1</param>
        /// <param name="target2">Target 2</param>
        /// <param name="resultTarget">Result target</param>
        private void CombineTargets(IEngineDeviceContext dc, Targets target1, Targets target2, Targets resultTarget)
        {
            var graphics = Scene.Game.Graphics;

            SetTarget(dc, new RenderTargetParameters { Target = resultTarget });

            dc.SetDepthStencilState(graphics.GetDepthStencilNone());
            dc.SetRasterizerState(graphics.GetRasterizerDefault());
            dc.SetBlendState(graphics.GetBlendDefault());

            var texture1 = GetTargetTextures(target1)?.FirstOrDefault();
            var texture2 = GetTargetTextures(target2)?.FirstOrDefault();

            processingDrawerFinal.Combine(dc, texture1, texture2);
        }
        /// <summary>
        /// Draws the specified target to screen
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="sourceTarget">Target</param>
        private void DrawToScreen(IEngineDeviceContext dc, Targets sourceTarget)
        {
            var graphics = Scene.Game.Graphics;

            var rtScreen = new RenderTargetParameters
            {
                Target = Targets.Screen
            };
            SetTarget(dc, rtScreen);

            dc.SetDepthStencilState(graphics.GetDepthStencilNone());
            dc.SetRasterizerState(graphics.GetRasterizerDefault());
            dc.SetBlendState(graphics.GetBlendDefault());

            var texture = GetTargetTextures(sourceTarget)?.FirstOrDefault();

            processingDrawerFinal.Draw(dc, texture, BuiltInPostProcessEffects.None, null);
        }

        /// <summary>
        /// Ends the scene
        /// </summary>
        protected void EndScene()
        {
            var graphics = Scene.Game.Graphics;
            var ic = graphics.ImmediateContext;
            ic.SetViewport(graphics.Viewport);
            ic.SetRenderTargets(
                graphics.DefaultRenderTarget, true, Scene.GameEnvironment.Background,
                graphics.DefaultDepthStencil, true, true,
                false);

            if (ExecuteParallel(actions))
            {
                //Execute command list
                var commands = commandList.OrderBy(c => c.Order).Select(c => c.Command);
                ic.ExecuteCommandLists(commands);

                ic.SetViewport(graphics.Viewport);
                ic.SetRenderTargets(graphics.DefaultRenderTarget, graphics.DefaultDepthStencil);
            }
        }
    }
}
