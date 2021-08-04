using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;
    using Engine.Effects;
    using Engine.PathFinding;
    using Engine.Tween;
    using Engine.UI;

    /// <summary>
    /// Render scene
    /// </summary>
    public class Scene : IHasGameState, IDisposable
    {
        /// <summary>
        /// Ground usage enum for ground picking
        /// </summary>
        private const SceneObjectUsages GroundUsage = SceneObjectUsages.Ground | SceneObjectUsages.FullPathFinding | SceneObjectUsages.CoarsePathFinding;

        /// <summary>
        /// Sky layer
        /// </summary>
        public const int LayerSky = 1;
        /// <summary>
        /// Default layer
        /// </summary>
        public const int LayerDefault = 50;
        /// <summary>
        /// 3D effects layer, like particles or water
        /// </summary>
        public const int LayerEffects = 75;
        /// <summary>
        /// User interface layer
        /// </summary>
        public const int LayerUI = 100;
        /// <summary>
        /// User interface effects layer
        /// </summary>
        public const int LayerUIEffects = 150;
        /// <summary>
        /// User interface cursor
        /// </summary>
        public const int LayerCursor = int.MaxValue;

        /// <summary>
        /// Performs coarse ray picking over the specified collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="maxDistance">Maximum distance to test</param>
        /// <param name="list">Collection of objects to test</param>
        /// <returns>Returns a list of ray pickable objects order by distance to ray origin</returns>
        private static List<Tuple<ISceneObject, float>> PickCoarse(ref Ray ray, float maxDistance, IEnumerable<ISceneObject> list)
        {
            List<Tuple<ISceneObject, float>> coarse = new List<Tuple<ISceneObject, float>>();

            foreach (var gObj in list)
            {
                if (gObj is IComposed componsed)
                {
                    var pickComponents = componsed.GetComponents<IRayPickable<Triangle>>();
                    foreach (var pickable in pickComponents)
                    {
                        if (TestCoarse(ref ray, pickable, maxDistance, out float d))
                        {
                            coarse.Add(new Tuple<ISceneObject, float>(gObj, d));
                        }
                    }
                }
                else if (
                    gObj is IRayPickable<Triangle> pickable &&
                    TestCoarse(ref ray, pickable, maxDistance, out float d))
                {
                    coarse.Add(new Tuple<ISceneObject, float>(gObj, d));
                }
            }

            //Sort by distance
            coarse.Sort((i1, i2) =>
            {
                return i1.Item2.CompareTo(i2.Item2);
            });

            return coarse;
        }
        /// <summary>
        /// Perfors coarse picking between the specified ray and the bounding volume of the object
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Ray</param>
        /// <param name="obj">Object</param>
        /// <param name="maxDistance">Maximum distance to test</param>
        /// <param name="distance">Gets the picking distance if intersection exists</param>
        /// <returns>Returns true if exists intersection between the ray and the bounding volume of the object, into the maximum distance</returns>
        private static bool TestCoarse<T>(ref Ray ray, IRayPickable<T> obj, float maxDistance, out float distance) where T : IRayIntersectable
        {
            distance = float.MaxValue;

            var bsph = obj.GetBoundingSphere();
            var intersects = Collision.RayIntersectsSphere(ref ray, ref bsph, out float d);
            if (intersects && (maxDistance == 0 || d <= maxDistance))
            {
                distance = d;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Gets first normal texture size for the specified pixel count
        /// </summary>
        /// <param name="pixelCount">Pixel count</param>
        /// <returns>Returns the texture size</returns>
        private static int GetTextureSize(int pixelCount)
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
        /// Gets wether the ray picks the object nearest to the specified best distance
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="obj">Object to test</param>
        /// <param name="bestDistance">Best distance</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if the ray picks the object nearest to the specified best distance</returns>
        private static bool PickNearestSingle<T>(Ray ray, RayPickingParams rayPickingParams, IRayPickable<T> obj, float bestDistance, out PickingResult<T> result) where T : IRayIntersectable
        {
            bool pickedNearest = false;

            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            var picked = obj.PickNearest(ray, rayPickingParams, out var r);
            if (picked && r.Distance < bestDistance)
            {
                result = r;
                pickedNearest = true;
            }

            return pickedNearest;
        }
        /// <summary>
        /// Gets wether the ray picks the object nearest to the specified best distance
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="obj">Object to test</param>
        /// <param name="bestDistance">Best distance</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if the ray picks the object nearest to the specified best distance</returns>
        private static bool PickNearestComposed<T>(Ray ray, RayPickingParams rayPickingParams, IComposed obj, float bestDistance, out PickingResult<T> result) where T : IRayIntersectable
        {
            bool pickedNearest = false;

            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            float dist = bestDistance;

            var pickComponents = obj.GetComponents<IRayPickable<T>>();

            foreach (var pickable in pickComponents)
            {
                var picked = pickable.PickNearest(ray, rayPickingParams, out var r);
                if (picked && r.Distance < dist)
                {
                    dist = r.Distance;
                    result = r;
                    pickedNearest = true;
                }
            }

            return pickedNearest;
        }
        /// <summary>
        /// Gets the current object triangle collection
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object</param>
        /// <returns>Returns the triangle list</returns>
        private static IEnumerable<T> GetTrianglesForNavigationGraph<T>(IDrawable obj) where T : IRayIntersectable
        {
            List<T> tris = new List<T>();

            List<IRayPickable<T>> volumes = new List<IRayPickable<T>>();

            if (obj is IComposed composed)
            {
                volumes.AddRange(GetVolumesForNavigationGraph<T>(composed));
            }
            else if (obj is IRayPickable<T> pickable)
            {
                if (obj is ITransformable3D transformable)
                {
                    transformable.Manipulator.UpdateInternals(true);
                }

                volumes.Add(pickable);
            }

            for (int p = 0; p < volumes.Count; p++)
            {
                var full = obj.Usage.HasFlag(SceneObjectUsages.FullPathFinding);

                var vTris = volumes[p].GetVolume(full);
                if (vTris.Any())
                {
                    //Use volume mesh
                    tris.AddRange(vTris);
                }
            }

            return tris;
        }
        /// <summary>
        /// Get volumes from composed object
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="composed">Composed</param>
        /// <returns>Returns a list of volumes</returns>
        private static IEnumerable<IRayPickable<T>> GetVolumesForNavigationGraph<T>(IComposed composed) where T : IRayIntersectable
        {
            List<IRayPickable<T>> volumes = new List<IRayPickable<T>>();

            var trnChilds = composed.GetComponents<ITransformable3D>();
            if (trnChilds.Any())
            {
                foreach (var child in trnChilds)
                {
                    child.Manipulator.UpdateInternals(true);
                }
            }

            var pickableChilds = composed.GetComponents<IRayPickable<T>>();
            if (pickableChilds.Any())
            {
                volumes.AddRange(pickableChilds);
            }

            return volumes.ToArray();
        }

        /// <summary>
        /// Scene world matrix
        /// </summary>
        private readonly Matrix world = Matrix.Identity;
        /// <summary>
        /// Scene component list
        /// </summary>
        private List<ISceneObject> internalComponents = new List<ISceneObject>();
        /// <summary>
        /// Scene mode
        /// </summary>
        private SceneModes sceneMode = SceneModes.Unknown;
        /// <summary>
        /// Ground bounding box
        /// </summary>
        private BoundingBox? groundBoundingBox;
        /// <summary>
        /// Navigation bounding box
        /// </summary>
        private BoundingBox? navigationBoundingBox;

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

        /// <summary>
        /// Game class
        /// </summary>
        public Game Game { get; private set; }
        /// <summary>
        /// Game environment
        /// </summary>
        public GameEnvironment GameEnvironment { get; private set; } = new GameEnvironment();
        /// <summary>
        /// Scene renderer
        /// </summary>
        protected ISceneRenderer Renderer = null;
        /// <summary>
        /// Path finder
        /// </summary>
        protected PathFinderDescription PathFinderDescription { get; set; }
        /// <summary>
        /// Graph used for pathfinding
        /// </summary>
        protected IGraph NavigationGraph { get; private set; }
        /// <summary>
        /// Audio manager
        /// </summary>
        protected GameAudioManager AudioManager { get; private set; }

        /// <summary>
        /// Camera
        /// </summary>
        public Camera Camera { get; protected set; }
        /// <summary>
        /// Indicates whether the current scene is active
        /// </summary>
        public bool Active { get; set; } = false;
        /// <summary>
        /// Scene processing order
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Scene lights
        /// </summary>
        public SceneLights Lights { get; protected set; }
        /// <summary>
        /// Gets or sets if scene has to perform frustum culling with objects
        /// </summary>
        public bool PerformFrustumCulling { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene(Game game)
        {
            Game = game;

            Game.ResourcesLoading += FireResourcesLoading;
            Game.ResourcesLoaded += FireResourcesLoaded;
            Game.Graphics.Resized += FireGraphicsResized;

            AudioManager = new GameAudioManager();

            Camera = Camera.CreateFree(
                new Vector3(0.0f, 0.0f, -10.0f),
                Vector3.Zero);

            Camera.SetLens(
                Game.Form.RenderWidth,
                Game.Form.RenderHeight);

            Lights = SceneLights.CreateDefault(this);

            PerformFrustumCulling = true;

            updateMaterialsPalette = true;
            updateAnimationsPalette = true;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Scene()
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
                Game.ResourcesLoading -= FireResourcesLoading;
                Game.ResourcesLoaded -= FireResourcesLoaded;
                Game.Graphics.Resized -= FireGraphicsResized;

                Renderer?.Dispose();
                Renderer = null;

                AudioManager?.Dispose();
                AudioManager = null;

                Camera?.Dispose();
                Camera = null;

                if (internalComponents != null)
                {
                    for (int i = 0; i < internalComponents.Count; i++)
                    {
                        var disposableCmp = internalComponents[i] as IDisposable;
                        disposableCmp?.Dispose();

                        internalComponents[i] = null;
                    }

                    internalComponents.Clear();
                    internalComponents = null;
                }

                NavigationGraph?.Dispose();
                NavigationGraph = null;
            }
        }

        /// <summary>
        /// Initialize scene
        /// </summary>
        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }
        /// <summary>
        /// Update scene objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            try
            {
                bool updateEnvironment = GameEnvironment.Update(gameTime);

                UpdateGlobals(updateEnvironment);

                // Lights
                Lights?.Update();

                // Camera!
                Camera?.Update(gameTime);

                AudioManager?.Update(gameTime);

                NavigationGraph?.Update(gameTime);

                UIControl.EvaluateInput(this);

                FloatTweenManager.Update(gameTime);

                // Action!
                Renderer?.Update(gameTime);
            }
            catch (EngineException ex)
            {
                Logger.WriteError(this, $"Scene Updating error: {ex.Message}", ex);

                throw;
            }
        }
        /// <summary>
        /// Draw scene objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Draw(GameTime gameTime)
        {
            try
            {
                Renderer?.Draw(gameTime);
            }
            catch (EngineException ex)
            {
                Logger.WriteError(this, $"Scene Drawing error {Renderer?.GetType()}: {ex.Message}", ex);

                throw;
            }
        }

        /// <summary>
        /// Gets the render mode
        /// </summary>
        /// <returns>Returns the render mode</returns>
        public SceneModes GetRenderMode()
        {
            return sceneMode;
        }
        /// <summary>
        /// Change renderer mode
        /// </summary>
        /// <param name="mode">New renderer mode</param>
        /// <returns>Returns true if the renderer changes correctly</returns>
        public bool SetRenderMode(SceneModes mode)
        {
            var graphics = Game.Graphics;

            ISceneRenderer renderer;

            if (mode == SceneModes.ForwardLigthning && SceneRendererForward.Validate(graphics))
            {
                renderer = new SceneRendererForward(this);
            }
            else if (mode == SceneModes.DeferredLightning && SceneRendererDeferred.Validate(graphics))
            {
                renderer = new SceneRendererDeferred(this);
            }
            else
            {
                return false;
            }

            Renderer?.Dispose();
            Renderer = renderer;
            sceneMode = mode;

            Counters.ClearAll();

            return true;
        }

        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        public async Task<bool> LoadResourcesAsync(Task task, Action<LoadResourcesResult> callback = null)
        {
            return await Game.LoadResourcesAsync(this, LoadResourceGroup.FromTasks(task), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="tasks">Task list</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        public async Task<bool> LoadResourcesAsync(IEnumerable<Task> tasks, Action<LoadResourcesResult> callback = null)
        {
            return await Game.LoadResourcesAsync(this, LoadResourceGroup.FromTasks(tasks), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        public async Task<bool> LoadResourcesAsync(LoadResourceGroup taskGroup, Action<LoadResourcesResult> callback = null)
        {
            return await Game.LoadResourcesAsync(this, taskGroup, callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        public async Task<bool> LoadResourcesAsync<T>(Task<T> task, Action<LoadResourcesResult<T>> callback = null)
        {
            return await Game.LoadResourcesAsync(this, LoadResourceGroup<T>.FromTasks(task), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="tasks">Task list</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        public async Task<bool> LoadResourcesAsync<T>(IEnumerable<Task<T>> tasks, Action<LoadResourcesResult<T>> callback = null)
        {
            return await Game.LoadResourcesAsync(this, LoadResourceGroup<T>.FromTasks(tasks), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        public async Task<bool> LoadResourcesAsync<T>(LoadResourceGroup<T> taskGroup, Action<LoadResourcesResult<T>> callback = null)
        {
            return await Game.LoadResourcesAsync(this, taskGroup, callback);
        }

        /// <summary>
        /// Fires when a requested resouce load process starts
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void FireResourcesLoading(object sender, GameLoadResourcesEventArgs e)
        {
            if (e.Scene == this)
            {
                GameResourcesLoading(e.Id);
            }
        }
        /// <summary>
        /// Fires when a requested resouce load process ends
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void FireResourcesLoaded(object sender, GameLoadResourcesEventArgs e)
        {
            if (e.Scene == this)
            {
                GameResourcesLoaded(e.Id);
            }
        }
        /// <summary>
        /// Fires when the render window has been resized
        /// </summary>
        /// <param name="sender">Graphis device</param>
        /// <param name="e">Event arguments</param>
        private void FireGraphicsResized(object sender, EventArgs e)
        {
            Renderer?.Resize();

            var fittedComponents = GetComponents<IScreenFitted>();
            if (fittedComponents.Any())
            {
                fittedComponents.ToList().ForEach(c => c.Resize());
            }

            GameGraphicsResized();
        }

        /// <summary>
        /// Progress reporting
        /// </summary>
        /// <param name="value">Progress value from 0.0f to 1.0f</param>
        public virtual void OnReportProgress(LoadResourceProgress value)
        {

        }
        /// <summary>
        /// Game resources loading event
        /// </summary>
        /// <param name="id">Batch id</param>
        public virtual void GameResourcesLoading(Guid id)
        {

        }
        /// <summary>
        /// Game resources loaded event
        /// </summary>
        /// <param name="id">Batch id</param>
        public virtual void GameResourcesLoaded(Guid id)
        {

        }
        /// <summary>
        /// Grame graphics resized
        /// </summary>
        public virtual void GameGraphicsResized()
        {

        }

        /// <summary>
        /// Gets the screen coordinates
        /// </summary>
        /// <param name="position">3D position</param>
        /// <param name="inside">Returns true if the resulting point is inside the screen</param>
        /// <returns>Returns the screen coordinates</returns>
        public Vector2 GetScreenCoordinates(Vector3 position, out bool inside)
        {
            return Helper.UnprojectToScreen(
                position,
                Game.Graphics.Viewport,
                Camera.View * Camera.Projection,
                out inside);
        }

        /// <summary>
        /// Adds component to collection
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="component">Component</param>
        /// <param name="usage">Usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the added component</returns>
        public void AddComponent(ISceneObject component, SceneObjectUsages usage = SceneObjectUsages.None, int layer = LayerDefault)
        {
            Monitor.Enter(internalComponents);
            
            if (internalComponents.Contains(component))
            {
                return;
            }

            if (internalComponents.Any(c => component.Id == c.Id))
            {
                throw new EngineException($"The specified component id {component.Id} already exists.");
            }

            if (component is IDrawable drawable)
            {
                drawable.Usage |= usage;

                if (layer != 0)
                {
                    drawable.Layer = layer;
                }
            }

            internalComponents.Add(component);
            internalComponents.Sort((p1, p2) =>
            {
                //First by type
                bool p1D = p1 is IDrawable;
                bool p2D = p2 is IDrawable;
                int i = p1D.CompareTo(p2D);
                if (i != 0) return i;

                if (!p1D || !p2D)
                {
                    return 0;
                }

                IDrawable drawable1 = (IDrawable)p1;
                IDrawable drawable2 = (IDrawable)p2;

                //First by order index
                i = drawable1.Layer.CompareTo(drawable2.Layer);
                if (i != 0) return i;

                //Then opaques
                i = drawable1.BlendMode.CompareTo(drawable2.BlendMode);
                if (i != 0) return i;

                //Then z-buffer writers
                i = drawable1.DepthEnabled.CompareTo(drawable2.DepthEnabled);

                return i;
            });

            Monitor.Exit(internalComponents);

            updateMaterialsPalette = true;
            updateAnimationsPalette = true;
        }
        /// <summary>
        /// Removes and disposes the specified component
        /// </summary>
        /// <param name="component">Component</param>
        public void RemoveComponent(ISceneObject component)
        {
            if (!internalComponents.Contains(component))
            {
                return;
            }

            Monitor.Enter(internalComponents);
            internalComponents.Remove(component);
            Monitor.Exit(internalComponents);

            updateMaterialsPalette = true;
            updateAnimationsPalette = true;

            if (component is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        /// <summary>
        /// Removes and disposes the specified component list
        /// </summary>
        /// <param name="components">List of components</param>
        public void RemoveComponents(IEnumerable<ISceneObject> components)
        {
            Monitor.Enter(internalComponents);
            foreach (var component in components)
            {
                if (internalComponents.Contains(component))
                {
                    internalComponents.Remove(component);

                    updateMaterialsPalette = true;
                    updateAnimationsPalette = true;
                }

                if (component is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            Monitor.Exit(internalComponents);
        }

        /// <summary>
        /// Gets full component collection
        /// </summary>
        /// <returns>Returns the full component collection</returns>
        public IEnumerable<ISceneObject> GetComponents()
        {
            return internalComponents.ToArray();
        }
        /// <summary>
        /// Gets component collection of the specified type
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>Returns the component collection</returns>
        public IEnumerable<T> GetComponents<T>()
        {
            return internalComponents.OfType<T>().ToArray();
        }

        /// <summary>
        /// Update global resources
        /// </summary>
        protected virtual void UpdateGlobals(bool updateEnvironment)
        {
            bool updateGlobals = updateEnvironment;

            if (updateMaterialsPalette)
            {
                Logger.WriteInformation(this, "Updating Material palette.");

                UpdateMaterialPalette(out materialPalette, out materialPaletteWidth);

                updateGlobals = true;

                updateMaterialsPalette = false;
            }

            if (updateAnimationsPalette)
            {
                Logger.WriteInformation(this, "Updating Animation palette.");

                UpdateAnimationPalette(out animationPalette, out animationPaletteWidth);

                updateGlobals = true;

                updateAnimationsPalette = false;
            }

            if (updateGlobals)
            {
                Logger.WriteInformation(this, "Updating Scene Globals.");

                Renderer?.UpdateGlobals();

                DrawerPool.UpdateSceneGlobals(GameEnvironment, materialPalette, materialPaletteWidth, animationPalette, animationPaletteWidth);
            }
        }
        /// <summary>
        /// Updates the global material palette
        /// </summary>
        /// <param name="materialPalette">Material palette</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        private void UpdateMaterialPalette(out EngineShaderResourceView materialPalette, out uint materialPaletteWidth)
        {
            List<IMeshMaterial> mats = new List<IMeshMaterial>
            {
                MeshMaterial.DefaultBlinnPhong,
            };

            var matComponents = GetComponents<IUseMaterials>();

            foreach (var component in matComponents)
            {
                var matList = component.Materials;
                if (matList.Any())
                {
                    mats.AddRange(matList);
                }
            }

            List<Vector4> values = new List<Vector4>();

            for (int i = 0; i < mats.Count; i++)
            {
                var mat = mats[i];
                var matV = mat.Material.Convert().Pack();

                mat.UpdateResource((uint)i, (uint)values.Count, (uint)matV.Length);

                values.AddRange(matV);
            }

            int texWidth = GetTextureSize(values.Count);

            materialPalette = Game.ResourceManager.CreateGlobalResource("MaterialPalette", values, texWidth);
            materialPaletteWidth = (uint)texWidth;
        }
        /// <summary>
        /// Updates the global animation palette
        /// </summary>
        /// <param name="animationPalette">Animation palette</param>
        /// <param name="animationPaletteWidth">Animation palette width</param>
        private void UpdateAnimationPalette(out EngineShaderResourceView animationPalette, out uint animationPaletteWidth)
        {
            List<SkinningData> skData = new List<SkinningData>();

            var skComponents = GetComponents<IUseSkinningData>();

            foreach (var component in skComponents)
            {
                var cmpSkData = component.SkinningData;
                if (cmpSkData != null)
                {
                    skData.Add(cmpSkData);
                }
            }

            List<SkinningData> addedSks = new List<SkinningData>();

            List<Vector4> values = new List<Vector4>();

            for (int i = 0; i < skData.Count; i++)
            {
                var sk = skData[i];

                if (!addedSks.Contains(sk))
                {
                    var skV = sk.Pack();

                    sk.UpdateResource((uint)addedSks.Count, (uint)values.Count, (uint)skV.Length);

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

            animationPalette = Game.ResourceManager.CreateGlobalResource("AnimationPalette", values.ToArray(), texWidth);
            animationPaletteWidth = (uint)texWidth;
        }

        /// <summary>
        /// Gets picking ray from current mouse position
        /// </summary>
        /// <returns>Returns picking ray from current mouse position</returns>
        public Ray GetPickingRay()
        {
            int mouseX = Game.Input.MouseX;
            int mouseY = Game.Input.MouseY;
            Matrix worldViewProjection = world * Camera.View * Camera.Projection;
            float nDistance = Camera.NearPlaneDistance;
            float fDistance = Camera.FarPlaneDistance;
            ViewportF viewport = Game.Graphics.Viewport;

            Vector3 nVector = new Vector3(mouseX, mouseY, nDistance);
            Vector3 fVector = new Vector3(mouseX, mouseY, fDistance);

            Vector3 nPoint = Vector3.Unproject(nVector, 0, 0, viewport.Width, viewport.Height, nDistance, fDistance, worldViewProjection);
            Vector3 fPoint = Vector3.Unproject(fVector, 0, 0, viewport.Width, viewport.Height, nDistance, fDistance, worldViewProjection);

            return new Ray(nPoint, Vector3.Normalize(fPoint - nPoint));
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public Ray GetTopDownRay(Point position)
        {
            return GetTopDownRay(position.X, position.Y);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public Ray GetTopDownRay(Vector2 position)
        {
            return GetTopDownRay(position.X, position.Y);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public Ray GetTopDownRay(Vector3 position)
        {
            return GetTopDownRay(position.X, position.Z);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public Ray GetTopDownRay(float x, float z)
        {
            var bbox = GetGroundBoundingBox();

            if (!bbox.HasValue || bbox == new BoundingBox())
            {
                Logger.WriteWarning(this, $"Scene Picking test: A ground must be defined into the scene in the first place.");
            }

            float maxY = (bbox?.Maximum.Y + 1.0f) ?? float.MaxValue;

            return new Ray()
            {
                Position = new Vector3(x, maxY, z),
                Direction = Vector3.Down,
            };
        }

        /// <summary>
        /// Gets the nearest pickable object in the ray path
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="maxDistance">Maximum distance for test</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Object usage mask</param>
        /// <param name="model">Gets the resulting ray pickable object</param>
        /// <returns>Returns true if a pickable object in the ray path was found</returns>
        public bool PickNearest(Ray ray, float maxDistance, RayPickingParams rayPickingParams, SceneObjectUsages usage, out ISceneObject model)
        {
            model = null;

            var cmpList = GetComponents<IDrawable>();

            if (usage != SceneObjectUsages.None)
            {
                cmpList = cmpList.Where(c => (c.Usage & usage) != SceneObjectUsages.None);
            }

            var coarse = PickCoarse(ref ray, maxDistance, cmpList);

            foreach (var obj in coarse)
            {
                if (obj.Item1 is IRayPickable<Triangle> pickable && pickable.PickNearest(ray, rayPickingParams, out _))
                {
                    model = obj.Item1;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickNearest<T>(Ray ray, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(ray, rayPickingParams, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickNearest<T>(Ray ray, RayPickingParams rayPickingParams, SceneObjectUsages usage, out PickingResult<T> result) where T : IRayIntersectable
        {
            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            var cmpList = GetComponents<IDrawable>();

            if (usage != SceneObjectUsages.None)
            {
                cmpList = cmpList.Where(c => (c.Usage & usage) != SceneObjectUsages.None);
            }

            var coarse = PickCoarse(ref ray, float.MaxValue, cmpList);

            bool picked = false;
            float bestDistance = float.MaxValue;

            foreach (var obj in coarse)
            {
                if (obj.Item2 > bestDistance)
                {
                    break;
                }

                if (obj.Item1 is IComposed composed)
                {
                    bool pickedComposed = PickNearestComposed<T>(ray, rayPickingParams, composed, bestDistance, out var r);
                    if (pickedComposed)
                    {
                        result = r;

                        bestDistance = r.Distance;
                        picked = true;
                    }
                }
                else if (obj.Item1 is IRayPickable<T> pickable)
                {
                    bool pickedSingle = PickNearestSingle(ray, rayPickingParams, pickable, bestDistance, out var r);
                    if (pickedSingle)
                    {
                        result = r;

                        bestDistance = r.Distance;
                        picked = true;
                    }
                }
            }

            return picked;
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickFirst<T>(Ray ray, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(ray, rayPickingParams, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickFirst<T>(Ray ray, RayPickingParams rayPickingParams, SceneObjectUsages usage, out PickingResult<T> result) where T : IRayIntersectable
        {
            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            IEnumerable<IDrawable> cmpList = GetComponents<IDrawable>();

            if (usage != SceneObjectUsages.None)
            {
                cmpList = cmpList.Where(c => (c.Usage & usage) != SceneObjectUsages.None);
            }

            var coarse = PickCoarse(ref ray, float.MaxValue, cmpList);

            foreach (var obj in coarse)
            {
                if (obj.Item1 is IComposed composed)
                {
                    var pickComponents = composed.GetComponents<IRayPickable<T>>();
                    foreach (var pickable in pickComponents)
                    {
                        if (pickable.PickFirst(ray, rayPickingParams, out var r))
                        {
                            result = r;

                            return true;
                        }
                    }
                }
                else if (
                    obj.Item1 is IRayPickable<T> pickable &&
                    pickable.PickFirst(ray, rayPickingParams, out var r))
                {
                    result = r;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickAll<T>(Ray ray, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            return PickAll(ray, rayPickingParams, SceneObjectUsages.None, out results);
        }
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Component usage</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickAll<T>(Ray ray, RayPickingParams rayPickingParams, SceneObjectUsages usage, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            results = null;

            IEnumerable<IDrawable> cmpList = GetComponents<IDrawable>();

            if (usage != SceneObjectUsages.None)
            {
                cmpList = cmpList.Where(c => (c.Usage & usage) != SceneObjectUsages.None);
            }

            var coarse = PickCoarse(ref ray, float.MaxValue, cmpList);

            List<PickingResult<T>> lResults = new List<PickingResult<T>>();

            foreach (var obj in coarse)
            {
                if (obj.Item1 is IComposed composed)
                {
                    var pickComponents = composed.GetComponents<IRayPickable<T>>();
                    foreach (var pickable in pickComponents)
                    {
                        if (pickable.PickAll(ray, rayPickingParams, out var r))
                        {
                            lResults.AddRange(r);
                        }
                    }
                }
                else if (
                    obj.Item1 is IRayPickable<T> pickable &&
                    pickable.PickAll(ray, rayPickingParams, out var r))
                {
                    lResults.AddRange(r);
                }
            }

            results = lResults.ToArray();

            return results.Any();
        }

        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindTopGroundPosition<T>(float x, float z, out PickingResult<T> result) where T : IRayIntersectable
        {
            var ray = GetTopDownRay(x, z);

            return PickNearest(ray, RayPickingParams.Default, GroundUsage, out result);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindFirstGroundPosition<T>(float x, float z, out PickingResult<T> result) where T : IRayIntersectable
        {
            var ray = GetTopDownRay(x, z);

            return PickFirst(ray, RayPickingParams.Default, GroundUsage, out result);
        }
        /// <summary>
        /// Gets all ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition<T>(float x, float z, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            var ray = GetTopDownRay(x, z);

            return PickAll(ray, RayPickingParams.Default, GroundUsage, out results);
        }
        /// <summary>
        /// Gets nearest ground position to "from" position
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindNearestGroundPosition<T>(Vector3 from, out PickingResult<T> result) where T : IRayIntersectable
        {
            var ray = GetTopDownRay(from.X, from.Z);

            bool picked = PickAll<T>(ray, RayPickingParams.Default, GroundUsage, out var pResults);
            if (picked)
            {
                int index = -1;
                float dist = float.MaxValue;
                for (int i = 0; i < pResults.Count(); i++)
                {
                    float d = Vector3.DistanceSquared(from, pResults.ElementAt(i).Position);
                    if (d <= dist)
                    {
                        dist = d;

                        index = i;
                    }
                }

                result = pResults.ElementAt(index);

                return true;
            }
            else
            {
                result = new PickingResult<T>()
                {
                    Distance = float.MaxValue,
                };

                return false;
            }
        }

        /// <summary>
        /// Gets the whole ground bounding box
        /// </summary>
        /// <returns>Returns the whole ground bounding box.</returns>
        public BoundingBox? GetGroundBoundingBox()
        {
            if (groundBoundingBox.HasValue && groundBoundingBox != new BoundingBox())
            {
                return groundBoundingBox;
            }

            //Try to get a bounding box from the current ground objects
            var cmpList = GetComponents<IDrawable>().Where(c => c.Usage.HasFlag(SceneObjectUsages.Ground));

            if (cmpList.Any())
            {
                List<BoundingBox> boxes = new List<BoundingBox>();

                foreach (var obj in cmpList)
                {
                    if (obj is IComposed composed)
                    {
                        var pickComponents = composed.GetComponents<IRayPickable<Triangle>>();
                        foreach (var pickable in pickComponents)
                        {
                            boxes.Add(pickable.GetBoundingBox());
                        }
                    }
                    else if (obj is IRayPickable<Triangle> pickable)
                    {
                        boxes.Add(pickable.GetBoundingBox());
                    }
                }

                groundBoundingBox = Helper.MergeBoundingBox(boxes);
            }

            return groundBoundingBox;
        }
        /// <summary>
        /// Gets the current navigation bounding box
        /// </summary>
        /// <returns>Returns the current navigation bounding box.</returns>
        public BoundingBox? GetNavigationBoundingBox()
        {
            return navigationBoundingBox;
        }

        /// <summary>
        /// Gets the scene volume for culling tests
        /// </summary>
        /// <returns>Returns the scene volume</returns>
        public IIntersectionVolume GetSceneVolume()
        {
            var ground = GetComponents<Ground>().FirstOrDefault(c => c.Usage.HasFlag(SceneObjectUsages.Ground));

            return ground?.GetCullingVolume();
        }

        /// <summary>
        /// Set ground geometry
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="fullGeometryPathFinding">Sets whether use full triangle list or volumes for navigation graphs</param>
        public void SetGround(IDrawable obj, bool fullGeometryPathFinding)
        {
            groundBoundingBox = null;

            obj.Usage |= SceneObjectUsages.Ground;
            obj.Usage |= fullGeometryPathFinding ? SceneObjectUsages.FullPathFinding : SceneObjectUsages.CoarsePathFinding;
        }
        /// <summary>
        /// Attach geometry to ground
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="x">X position</param>
        /// <param name="z">Z position</param>
        /// <param name="transform">Transform</param>
        /// <param name="fullGeometryPathFinding">Sets whether use full triangle list or volumes for navigation graphs</param>
        public void AttachToGround(IDrawable obj, bool fullGeometryPathFinding)
        {
            obj.Usage |= fullGeometryPathFinding ? SceneObjectUsages.FullPathFinding : SceneObjectUsages.CoarsePathFinding;
        }

        /// <summary>
        /// Updates the navigation graph
        /// </summary>
        public virtual async Task UpdateNavigationGraph()
        {
            if (PathFinderDescription == null)
            {
                SetNavigationGraph(null);

                return;
            }

            var graph = await PathFinderDescription.Build();

            SetNavigationGraph(graph);

            NavigationGraphUpdated();
        }
        /// <summary>
        /// Sets a navigation graph
        /// </summary>
        /// <param name="graph">Navigation graph</param>
        public virtual void SetNavigationGraph(IGraph graph)
        {
            NavigationGraphUpdating();

            if (NavigationGraph != null)
            {
                NavigationGraph.Updating -= GraphUpdating;
                NavigationGraph.Updated -= GraphUpdated;

                NavigationGraph.Dispose();
                NavigationGraph = null;

                navigationBoundingBox = new BoundingBox();
            }

            if (graph != null)
            {
                NavigationGraph = graph;
                NavigationGraph.Updating += GraphUpdating;
                NavigationGraph.Updated += GraphUpdated;

                if (PathFinderDescription?.Input != null)
                {
                    navigationBoundingBox = PathFinderDescription.Input.BoundingBox;
                }
            }

            NavigationGraphUpdated();
        }

        /// <summary>
        /// Graph updating event
        /// </summary>
        /// <param name="sender">Sender graph</param>
        /// <param name="e">Event args</param>
        private void GraphUpdating(object sender, EventArgs e)
        {
            Logger.WriteInformation(this, $"GraphUpdating - {sender}");

            Logger.WriteInformation(this, $"GraphUpdating - NavigationGraphUpdating Call");
            NavigationGraphUpdating();
            Logger.WriteInformation(this, $"GraphUpdating - NavigationGraphUpdating End");
        }
        /// <summary>
        /// Graph updated event
        /// </summary>
        /// <param name="sender">Sender graph</param>
        /// <param name="e">Event args</param>
        private void GraphUpdated(object sender, EventArgs e)
        {
            Logger.WriteInformation(this, $"GraphUpdated - {sender}");

            Logger.WriteInformation(this, $"GraphUpdating - NavigationGraphUpdated Call");
            NavigationGraphUpdated();
            Logger.WriteInformation(this, $"GraphUpdating - NavigationGraphUpdated End");
        }
        /// <summary>
        /// Fires when graph is updating
        /// </summary>
        public virtual void NavigationGraphUpdating()
        {

        }
        /// <summary>
        /// Fires when graph is updated
        /// </summary>
        public virtual void NavigationGraphUpdated()
        {

        }

        /// <summary>
        /// Gets the objects triangle list for navigation graph construction
        /// </summary>
        /// <returns>Returns a triangle list</returns>
        public virtual IEnumerable<Triangle> GetTrianglesForNavigationGraph()
        {
            List<Triangle> tris = new List<Triangle>();

            var pfComponents = GetComponents<IDrawable>()
                .Where(c =>
                {
                    return
                        !c.HasOwner &&
                        c.Visible &&
                        (c.Usage.HasFlag(SceneObjectUsages.FullPathFinding) || c.Usage.HasFlag(SceneObjectUsages.CoarsePathFinding));
                });

            foreach (var cmp in pfComponents)
            {
                var currTris = GetTrianglesForNavigationGraph<Triangle>(cmp);
                if (currTris.Any())
                {
                    tris.AddRange(currTris);
                }
            }

            var bounds = PathFinderDescription.Settings.Bounds;
            if (bounds.HasValue)
            {
                tris = tris.FindAll(t =>
                {
                    var tbbox = BoundingBox.FromPoints(t.GetVertices().ToArray());

                    return bounds.Value.Contains(ref tbbox) != ContainmentType.Disjoint;
                });
            }

            return tris.ToArray();
        }

        /// <summary>
        /// Gets the path finder grid nodes
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns the path finder grid nodes</returns>
        public virtual IEnumerable<IGraphNode> GetNodes(AgentType agent)
        {
            return NavigationGraph?.GetNodes(agent) ?? new IGraphNode[] { };
        }
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <param name="useGround">Find nearest real ground position for "from" and "to" parameters, and all path results</param>
        /// <returns>Return path if exists</returns>
        public virtual PathFindingPath FindPath(AgentType agent, Vector3 from, Vector3 to, bool useGround = false)
        {
            if (NavigationGraph?.Initialized != true)
            {
                return null;
            }

            if (useGround)
            {
                Logger.WriteTrace(this, $"FindPath looking for nearest ground end-point positions {from} -> {to}.");

                if (FindNearestGroundPosition<Triangle>(from, out var rFrom))
                {
                    from = rFrom.Position;
                }
                if (FindNearestGroundPosition<Triangle>(to, out var rTo))
                {
                    to = rTo.Position;
                }

                Logger.WriteTrace(this, $"FindPath Found nearest ground end-point positions {from} -> {to}.");
            }

            var path = NavigationGraph.FindPath(agent, from, to);

            Logger.WriteTrace(this, $"FindPath path result: {path.Count()} nodes.");

            if (path.Count() > 1)
            {
                List<Vector3> positions = new List<Vector3>(path);
                List<Vector3> normals = new List<Vector3>(Helper.CreateArray(path.Count(), Vector3.Up));

                if (useGround)
                {
                    Logger.WriteTrace(this, "FindPath compute ground positions.");

                    ComputeGroundPositions(positions, normals);

                    Logger.WriteTrace(this, "FindPath ground positions computed.");
                }

                return new PathFindingPath(positions, normals);
            }

            return null;
        }
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <param name="useGround">Find nearest real ground position for "from" and "to" parameters, and all path results</param>
        /// <returns>Return path if exists</returns>
        public virtual async Task<PathFindingPath> FindPathAsync(AgentType agent, Vector3 from, Vector3 to, bool useGround = false)
        {
            if (NavigationGraph?.Initialized != true)
            {
                return null;
            }

            if (useGround)
            {
                Logger.WriteTrace(this, $"FindPathAsync looking for nearest ground end-point positions {from} -> {to}.");

                if (FindNearestGroundPosition(from, out PickingResult<Triangle> rFrom))
                {
                    from = rFrom.Position;
                }
                if (FindNearestGroundPosition(to, out PickingResult<Triangle> rTo))
                {
                    to = rTo.Position;
                }

                Logger.WriteTrace(this, $"FindPathAsync Found nearest ground end-point positions {from} -> {to}.");
            }

            var path = await NavigationGraph.FindPathAsync(agent, from, to);

            Logger.WriteTrace(this, $"FindPathAsync path result: {path.Count()} nodes.");

            if (path.Count() > 1)
            {
                List<Vector3> positions = new List<Vector3>(path);
                List<Vector3> normals = new List<Vector3>(Helper.CreateArray(path.Count(), Vector3.Up));

                if (useGround)
                {
                    Logger.WriteTrace(this, "FindPathAsync compute ground positions.");

                    ComputeGroundPositions(positions, normals);

                    Logger.WriteTrace(this, "FindPathAsync ground positions computed.");
                }

                return new PathFindingPath(positions, normals);
            }

            return null;
        }
        /// <summary>
        /// Updates the path positions and normals using current ground info
        /// </summary>
        /// <param name="positions">Positions</param>
        /// <param name="normals">Normals</param>
        private void ComputeGroundPositions(List<Vector3> positions, List<Vector3> normals)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                if (FindNearestGroundPosition<Triangle>(positions[i], out var r))
                {
                    positions[i] = r.Position;
                    normals[i] = r.Item.Normal;
                }
            }
        }

        /// <summary>
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="position">Position</param>
        /// <param name="nearest">Gets the nearest walkable position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public virtual bool IsWalkable(AgentType agent, Vector3 position, out Vector3? nearest)
        {
            if (NavigationGraph != null)
            {
                return NavigationGraph.IsWalkable(agent, position, out nearest);
            }

            nearest = position;

            return true;
        }
        /// <summary>
        /// Gets final position for agents walking over the ground if exists
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="prevPosition">Previous position</param>
        /// <param name="newPosition">New position</param>
        /// <param name="adjustHeight">Set whether use the agent height or not when resolving the final position. Usually true when the camera sets the agent's position</param>
        /// <param name="finalPosition">Returns the final position if exists</param>
        /// <returns>Returns true if final position found</returns>
        public virtual bool Walk(AgentType agent, Vector3 prevPosition, Vector3 newPosition, bool adjustHeight, out Vector3 finalPosition)
        {
            finalPosition = prevPosition;

            if (prevPosition == newPosition)
            {
                return false;
            }

            bool isInGround = FindAllGroundPosition<Triangle>(newPosition.X, newPosition.Z, out var results);
            if (!isInGround)
            {
                return false;
            }

            Vector3 newFeetPosition = newPosition;

            if (adjustHeight)
            {
                float offset = agent.Height;
                newFeetPosition.Y -= offset;

                results = results
                    .Where(r => Vector3.Distance(r.Position, newFeetPosition) < offset)
                    .OrderBy(r => r.Distance).ToArray();
            }

            foreach (var result in results)
            {
                if (IsWalkable(agent, result.Position, out Vector3? nearest))
                {
                    finalPosition = GetPositionWalkable(agent, prevPosition, newPosition, result.Position, adjustHeight);

                    return true;
                }
                else if (nearest.HasValue)
                {
                    //Not walkable but nearest position found
                    finalPosition = GetPositionNonWalkable(agent, prevPosition, newPosition, nearest.Value, adjustHeight);

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets the new agent position when target position is walkable
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="prevPosition">Previous position</param>
        /// <param name="newPosition">New position</param>
        /// <param name="position">Test position</param>
        /// <param name="adjustHeight">Set whether use the agent height or not when resolving the final position. Usually true when the camera sets the agent's position</param>
        /// <returns>Returns the new agent position</returns>
        private Vector3 GetPositionWalkable(AgentType agent, Vector3 prevPosition, Vector3 newPosition, Vector3 position, bool adjustHeight)
        {
            Vector3 finalPosition = position;

            if (adjustHeight)
            {
                finalPosition.Y += agent.Height;
            }

            var moveP = newPosition - prevPosition;
            var moveV = finalPosition - prevPosition;
            if (moveV.LengthSquared() > moveP.LengthSquared())
            {
                finalPosition = prevPosition + (Vector3.Normalize(moveV) * moveP.Length());
            }

            return finalPosition;
        }
        /// <summary>
        /// Gets the new agent position when target position is not walkable
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="prevPosition">Previous position</param>
        /// <param name="newPosition">New position</param>
        /// <param name="position">Test position</param>
        /// <param name="adjustHeight">Set whether use the agent height or not when resolving the final position. Usually true when the camera sets the agent's position</param>
        /// <returns>Returns the new agent position</returns>
        private Vector3 GetPositionNonWalkable(AgentType agent, Vector3 prevPosition, Vector3 newPosition, Vector3 position, bool adjustHeight)
        {
            //Find nearest ground position
            Vector3 finalPosition;
            if (FindNearestGroundPosition(position, out PickingResult<Triangle> nearestResult))
            {
                //Use nearest ground position found
                finalPosition = nearestResult.Position;
            }
            else
            {
                //Use nearest position provided by path finding graph
                finalPosition = position;
            }

            if (adjustHeight)
            {
                //Adjust height
                finalPosition.Y += agent.Height;
            }

            var moveP = newPosition - prevPosition;
            var moveV = finalPosition - prevPosition;
            if (moveV.LengthSquared() > moveP.LengthSquared())
            {
                finalPosition = prevPosition + (Vector3.Normalize(moveV) * moveP.Length());
            }

            return finalPosition;
        }

        /// <summary>
        /// Adds cylinder obstacle
        /// </summary>
        /// <param name="cylinder">Cylinder</param>
        /// <returns>Returns the obstacle Id</returns>
        public virtual int AddObstacle(BoundingCylinder cylinder)
        {
            return NavigationGraph?.AddObstacle(cylinder) ?? -1;
        }
        /// <summary>
        /// Adds AABB obstacle
        /// </summary>
        /// <param name="bbox">AABB</param>
        /// <returns>Returns the obstacle Id</returns>
        public virtual int AddObstacle(BoundingBox bbox)
        {
            return NavigationGraph?.AddObstacle(bbox) ?? -1;
        }
        /// <summary>
        /// Adds OBB obstacle
        /// </summary>
        /// <param name="obb">OBB</param>
        /// <returns>Returns the obstacle Id</returns>
        public virtual int AddObstacle(OrientedBoundingBox obb)
        {
            return NavigationGraph?.AddObstacle(obb) ?? -1;
        }
        /// <summary>
        /// Removes obstable by id
        /// </summary>
        /// <param name="obstacle">Obstacle id</param>
        public virtual void RemoveObstacle(int obstacle)
        {
            NavigationGraph?.RemoveObstacle(obstacle);
        }

        /// <summary>
        /// Updates the graph at position
        /// </summary>
        /// <param name="position">Position</param>
        public virtual async void UpdateGraph(Vector3 position)
        {
            await PathFinderDescription?.Input.Refresh();

            NavigationGraph?.UpdateAt(position);
        }
        /// <summary>
        /// Updates the graph at positions in the specified list
        /// </summary>
        /// <param name="positions">Positions list</param>
        public virtual async void UpdateGraph(IEnumerable<Vector3> positions)
        {
            if (positions?.Any() == true)
            {
                await PathFinderDescription?.Input.Refresh();

                NavigationGraph?.UpdateAt(positions);
            }
        }

        /// <summary>
        /// Gets a random point over the ground
        /// </summary>
        /// <param name="rnd">Random instance</param>
        /// <param name="offset">Search offset</param>
        /// <returns>Returns a position over the ground</returns>
        public Vector3 GetRandomPoint(Random rnd, Vector3 offset)
        {
            var bbox = GetGroundBoundingBox();
            if (!bbox.HasValue)
            {
                Vector3 min = Vector3.One * float.MinValue;
                Vector3 max = Vector3.One * float.MaxValue;

                return rnd.NextVector3(min, max);
            }

            return GetRandomPoint(rnd, offset, bbox.Value);
        }
        /// <summary>
        /// Gets a random point over the ground
        /// </summary>
        /// <param name="rnd">Random instance</param>
        /// <param name="offset">Search offset</param>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns a position over the ground</returns>
        public Vector3 GetRandomPoint(Random rnd, Vector3 offset, BoundingBox bbox)
        {
            while (true)
            {
                Vector3 v = rnd.NextVector3(bbox.Minimum * 0.9f, bbox.Maximum * 0.9f);

                if (FindTopGroundPosition(v.X, v.Z, out PickingResult<Triangle> r))
                {
                    return r.Position + offset;
                }
            }
        }
        /// <summary>
        /// Gets a random point over the ground
        /// </summary>
        /// <param name="rnd">Random instance</param>
        /// <param name="offset">Search offset</param>
        /// <param name="bsph">Bounding sphere</param>
        /// <returns>Returns a position over the ground</returns>
        public Vector3 GetRandomPoint(Random rnd, Vector3 offset, BoundingSphere bsph)
        {
            while (true)
            {
                float dist = rnd.NextFloat(0, bsph.Radius);

                Vector3 dir = new Vector3(rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1));

                Vector3 v = bsph.Center + (dist * Vector3.Normalize(dir));

                if (FindTopGroundPosition(v.X, v.Z, out PickingResult<Triangle> r))
                {
                    return r.Position + offset;
                }
            }
        }

        /// <inheritdoc/>
        public virtual IGameState GetState()
        {
            return new SceneState
            {
                GameTime = Game.GameTime.Ticks,

                Active = Active,
                Order = Order,
                SceneMode = sceneMode,
                GroundBoundingBoxMin = groundBoundingBox?.Minimum,
                GroundBoundingBoxMax = groundBoundingBox?.Maximum,
                NavigationBoundingBoxMin = navigationBoundingBox?.Minimum,
                NavigationBoundingBoxMax = navigationBoundingBox?.Maximum,
                GameEnvironment = GameEnvironment.GetState(),
                SceneLights = Lights.GetState(),
                Camera = Camera.GetState(),
                Components = internalComponents.OfType<IHasGameState>().Select(c => c.GetState()).OfType<ISceneObjectState>().ToArray(),
            };
        }
        /// <inheritdoc/>
        public virtual void SetState(IGameState state)
        {
            if (!(state is SceneState sceneState))
            {
                return;
            }

            Game.GameTime.Reset(sceneState.GameTime);

            Active = sceneState.Active;
            Order = sceneState.Order;
            sceneMode = sceneState.SceneMode;
            if (sceneState.GroundBoundingBoxMin.HasValue && sceneState.GroundBoundingBoxMax.HasValue)
            {
                groundBoundingBox = new BoundingBox(
                    sceneState.GroundBoundingBoxMin.Value,
                    sceneState.GroundBoundingBoxMax.Value);
            }
            if (sceneState.NavigationBoundingBoxMin.HasValue && sceneState.NavigationBoundingBoxMax.HasValue)
            {
                navigationBoundingBox = new BoundingBox(
                    sceneState.NavigationBoundingBoxMin.Value,
                    sceneState.NavigationBoundingBoxMax.Value);
            }
            GameEnvironment.SetState(sceneState.GameEnvironment);
            Lights.SetState(sceneState.SceneLights);
            Camera.SetState(sceneState.Camera);
            foreach (var componentState in sceneState.Components)
            {
                var component = internalComponents
                    .Where(c => c.Id == componentState.Id)
                    .OfType<IHasGameState>()
                    .FirstOrDefault();

                component?.SetState(componentState);
            }
        }

        /// <summary>
        /// Saves the scene into a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public void SaveScene(string fileName)
        {
            var state = GetState();

            SerializationHelper.SerializeToFile(state, fileName);
        }
        /// <summary>
        /// Loads a scene from a file
        /// </summary>
        /// <param name="filename">File name</param>
        public void LoadScene(string filename)
        {
            var state = SerializationHelper.DeserializeFromFile<SceneState>(filename);

            SetState(state);
        }
    }
}
