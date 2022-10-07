using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.Common;
    using Engine.Coroutines;
    using Engine.Tween;

    /// <summary>
    /// Render scene
    /// </summary>
    public class Scene : IHasGameState, IDisposable
    {
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
        /// Corotine manager
        /// </summary>
        private readonly Yielder coroutineManager = new Yielder();

        /// <summary>
        /// Scene bounding box
        /// </summary>
        protected BoundingBox? BoundingBox;
        /// <summary>
        /// Scene renderer
        /// </summary>
        protected ISceneRenderer Renderer = null;
        /// <summary>
        /// Audio manager
        /// </summary>
        protected GameAudioManager AudioManager { get; private set; }

        /// <summary>
        /// Game class
        /// </summary>
        public Game Game { get; private set; }
        /// <summary>
        /// Game environment
        /// </summary>
        public GameEnvironment GameEnvironment { get; private set; } = new GameEnvironment();
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
        /// Gets or sets the top most control in the UI hierarchy
        /// </summary>
        public IUIControl TopMostControl { get; set; }
        /// <summary>
        /// Gets or sets the focused control the UI
        /// </summary>
        public IUIControl FocusedControl { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene(Game game)
        {
            Game = game;

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

                coroutineManager.ProcessCoroutines();

                this.EvaluateInput();

                FloatTweenManager.Update(gameTime);

                // Action!
                Renderer?.Update(gameTime);
            }
            catch (EngineException ex)
            {
                Logger.WriteError(this, $"{nameof(Scene)} => Updating error: {ex.Message}", ex);

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
                Logger.WriteError(this, $"{nameof(Scene)} => Drawing error {Renderer?.GetType()}: {ex.Message}", ex);

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
        /// Starts a new coroutine
        /// </summary>
        /// <param name="routine">Enumerator routine</param>
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return coroutineManager.StartCoroutine(routine);
        }

        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="task">Task</param>
        public Task LoadResourcesAsync(Task task)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup.FromTasks(task));
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="tasks">Task list</param>
        public Task LoadResourcesAsync(IEnumerable<Task> tasks)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup.FromTasks(tasks));
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="taskGroup">Resource load tasks</param>
        public Task LoadResourcesAsync(LoadResourceGroup taskGroup)
        {
            return Game.LoadResourcesAsync(taskGroup);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync(Task task, Action<LoadResourcesResult> callback)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup.FromTasks(task), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="tasks">Task list</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync(IEnumerable<Task> tasks, Action<LoadResourcesResult> callback)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup.FromTasks(tasks), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync(LoadResourceGroup taskGroup, Action<LoadResourcesResult> callback)
        {
            return Game.LoadResourcesAsync(taskGroup, callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync(Task task, Func<LoadResourcesResult, Task> callback)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup.FromTasks(task), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="tasks">Task list</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync(IEnumerable<Task> tasks, Func<LoadResourcesResult, Task> callback)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup.FromTasks(tasks), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync(LoadResourceGroup taskGroup, Func<LoadResourcesResult, Task> callback)
        {
            return Game.LoadResourcesAsync(taskGroup, callback);
        }

        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        public Task LoadResourcesAsync<T>(Task<T> task)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup<T>.FromTasks(task));
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="tasks">Task list</param>
        public Task LoadResourcesAsync<T>(IEnumerable<Task<T>> tasks)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup<T>.FromTasks(tasks));
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="taskGroup">Resource load tasks</param>
        public Task LoadResourcesAsync<T>(LoadResourceGroup<T> taskGroup)
        {
            return Game.LoadResourcesAsync(taskGroup);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync<T>(Task<T> task, Action<LoadResourcesResult<T>> callback)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup<T>.FromTasks(task), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="tasks">Task list</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync<T>(IEnumerable<Task<T>> tasks, Action<LoadResourcesResult<T>> callback)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup<T>.FromTasks(tasks), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync<T>(LoadResourceGroup<T> taskGroup, Action<LoadResourcesResult<T>> callback)
        {
            return Game.LoadResourcesAsync(taskGroup, callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="task">Task</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync<T>(Task<T> task, Func<LoadResourcesResult<T>, Task> callback)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup<T>.FromTasks(task), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="tasks">Task list</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync<T>(IEnumerable<Task<T>> tasks, Func<LoadResourcesResult<T>, Task> callback)
        {
            return Game.LoadResourcesAsync(LoadResourceGroup<T>.FromTasks(tasks), callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        public Task LoadResourcesAsync<T>(LoadResourceGroup<T> taskGroup, Func<LoadResourcesResult<T>, Task> callback)
        {
            return Game.LoadResourcesAsync(taskGroup, callback);
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
                fittedComponents
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(c => c.Resize());
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
        /// Buffer progress reporting
        /// </summary>
        /// <param name="value">Progress value from 0.0f to 1.0f</param>
        public virtual void OnReportProgressBuffers(LoadResourceProgress value)
        {

        }
        /// <summary>
        /// Grame graphics resized
        /// </summary>
        public virtual void GameGraphicsResized()
        {
            Logger.WriteTrace(this, $"{nameof(Scene)} => Graphics resized.");
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
        /// Creates a component
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> CreateComponent<TObj, TDescription>(string id, string name, TDescription description)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            var component = (TObj)Activator.CreateInstance(typeof(TObj), new object[] { this, id, name });

            await component.InitializeAssets(description);

            return component;
        }

        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        private async Task<TObj> AddComponentInternal<TObj, TDescription>(string id, string name, TDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = LayerDefault)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            var component = await CreateComponent<TObj, TDescription>(id, name, description);

            AddComponent(component, usage, layer);

            return component;
        }
        /// <summary>
        /// Adds an agent component to the scene
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> AddComponentAgent<TObj, TDescription>(string id, string name, TDescription description, int layer = LayerDefault)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            return await AddComponentInternal<TObj, TDescription>(id, name, description, SceneObjectUsages.Agent, layer);
        }
        /// <summary>
        /// Adds a component to the scene's sky
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> AddComponentSky<TObj, TDescription>(string id, string name, TDescription description, int layer = LayerSky)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            return await AddComponentInternal<TObj, TDescription>(id, name, description, SceneObjectUsages.None, layer);
        }
        /// <summary>
        /// Adds a component to the scene's ground
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> AddComponentGround<TObj, TDescription>(string id, string name, TDescription description, int layer = LayerDefault)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            return await AddComponentInternal<TObj, TDescription>(id, name, description, SceneObjectUsages.Ground, layer);
        }
        /// <summary>
        /// Adds a cursor component to the scene's user interface
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> AddComponentCursor<TObj, TDescription>(string id, string name, TDescription description, int layer = LayerCursor)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            return await AddComponentInternal<TObj, TDescription>(id, name, description, SceneObjectUsages.UI, layer);
        }
        /// <summary>
        /// Adds a component to the scene's user interface
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> AddComponentUI<TObj, TDescription>(string id, string name, TDescription description, int layer = LayerUI)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            return await AddComponentInternal<TObj, TDescription>(id, name, description, SceneObjectUsages.UI, layer);
        }
        /// <summary>
        /// Adds a component to the scene's effects
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> AddComponentEffect<TObj, TDescription>(string id, string name, TDescription description, int layer = LayerEffects)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            return await AddComponentInternal<TObj, TDescription>(id, name, description, SceneObjectUsages.None, layer);
        }
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> AddComponent<TObj, TDescription>(string id, string name, TDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = LayerDefault)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            return await AddComponentInternal<TObj, TDescription>(id, name, description, usage, layer);
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
            try
            {
                if (internalComponents.Contains(component))
                {
                    return;
                }

                if (internalComponents.Any(c => component.Id == c.Id))
                {
                    throw new EngineException($"{nameof(Scene)} => The specified component id {component.Id} already exists.");
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
            }
            finally
            {
                Monitor.Exit(internalComponents);
            }

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
            try
            {
                internalComponents.Remove(component);
            }
            finally
            {
                Monitor.Exit(internalComponents);
            }

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
            try
            {
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
            }
            finally
            {
                Monitor.Exit(internalComponents);
            }
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
            return internalComponents
                .OfType<T>()
                .ToArray();
        }
        /// <summary>
        /// Gets component collection of the specified type
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="predicate">Function to test every item in the collection</param>
        /// <returns>Returns the component collection</returns>
        public IEnumerable<T> GetComponents<T>(Func<T, bool> predicate)
        {
            return internalComponents
                .OfType<T>()
                .Where(predicate)
                .ToArray();
        }
        /// <summary>
        /// Gets drawable component collection by usage
        /// </summary>
        /// <param name="usage">Usage</param>
        /// <returns>Returns the component list</returns>
        public IEnumerable<IDrawable> GetComponentsByUsage(SceneObjectUsages usage)
        {
            if (usage != SceneObjectUsages.None)
            {
                return GetComponents<IDrawable>(c => (c.Usage & usage) != SceneObjectUsages.None);
            }

            return GetComponents<IDrawable>();
        }

        /// <summary>
        /// Update global resources
        /// </summary>
        protected virtual void UpdateGlobals(bool updateEnvironment)
        {
            bool updateGlobals = updateEnvironment;

            if (updateMaterialsPalette)
            {
                Logger.WriteInformation(this, $"{nameof(Scene)} =>Updating Material palette.");

                UpdateMaterialPalette(out materialPalette, out materialPaletteWidth);

                updateGlobals = true;

                updateMaterialsPalette = false;
            }

            if (updateAnimationsPalette)
            {
                Logger.WriteInformation(this, $"{nameof(Scene)} =>Updating Animation palette.");

                UpdateAnimationPalette(out animationPalette, out animationPaletteWidth);

                updateGlobals = true;

                updateAnimationsPalette = false;
            }

            if (updateGlobals)
            {
                Logger.WriteInformation(this, $"{nameof(Scene)} =>Updating Scene Globals.");

                Renderer?.UpdateGlobals();

                BuiltInShaders.UpdateGlobals(materialPalette, materialPaletteWidth, animationPalette, animationPaletteWidth);
            }
        }
        /// <summary>
        /// Updates the materials palette
        /// </summary>
        public virtual void UpdateMaterialPalette()
        {
            updateMaterialsPalette = true;

            UpdateGlobals(false);
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
                var matList = component.GetMaterials();
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
            var skData = GetComponents<IUseSkinningData>()
                .Where(c => c.SkinningData != null)
                .Select(c => c.SkinningData)
                .ToArray();

            List<ISkinningData> addedSks = new List<ISkinningData>();

            List<Vector4> values = new List<Vector4>();

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

            animationPalette = Game.ResourceManager.CreateGlobalResource("AnimationPalette", values.ToArray(), texWidth);
            animationPaletteWidth = (uint)texWidth;
        }

        /// <summary>
        /// Gets the whole ground bounding box
        /// </summary>
        /// <returns>Returns the whole ground bounding box.</returns>
        public BoundingBox? GetGroundBoundingBox()
        {
            if (BoundingBox.HasValue && BoundingBox != new BoundingBox())
            {
                return BoundingBox;
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

                BoundingBox = Helper.MergeBoundingBox(boxes);
            }

            return BoundingBox;
        }

        /// <summary>
        /// Gets picking ray from current mouse position
        /// </summary>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns picking ray from current mouse position</returns>
        public PickingRay GetPickingRay(RayPickingParams pickingParams = RayPickingParams.Default)
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

            return new PickingRay(nPoint, Vector3.Normalize(fPoint - nPoint), pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(Point position, RayPickingParams pickingParams = RayPickingParams.Default)
        {
            return GetTopDownRay(position.X, position.Y, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(Vector2 position, RayPickingParams pickingParams = RayPickingParams.Default)
        {
            return GetTopDownRay(position.X, position.Y, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(Vector3 position, RayPickingParams pickingParams = RayPickingParams.Default)
        {
            return GetTopDownRay(position.X, position.Z, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(float x, float z, RayPickingParams pickingParams = RayPickingParams.Default)
        {
            var bbox = GetGroundBoundingBox();

            if (!bbox.HasValue || bbox == new BoundingBox())
            {
                Logger.WriteWarning(this, $"{nameof(Scene)} => Picking test: A ground must be defined into the scene in the first place.");
            }

            float maxY = (bbox?.Maximum.Y + 1.0f) ?? float.MaxValue;

            return new PickingRay(new Vector3(x, maxY, z), Vector3.Down, pickingParams);
        }

        /// <summary>
        /// Gets the scene volume for culling tests
        /// </summary>
        /// <returns>Returns the scene volume</returns>
        public IIntersectionVolume GetSceneVolume()
        {
            var ground = GetComponents<IDrawable>()
                .Where(c => c.Usage.HasFlag(SceneObjectUsages.Ground))
                .OfType<IGround>()
                .FirstOrDefault();

            return ground?.GetCullingVolume();
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
                GroundBoundingBoxMin = BoundingBox?.Minimum,
                GroundBoundingBoxMax = BoundingBox?.Maximum,
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
                BoundingBox = new BoundingBox(
                    sceneState.GroundBoundingBoxMin.Value,
                    sceneState.GroundBoundingBoxMax.Value);
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
