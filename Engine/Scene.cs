using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Audio;
    using Engine.Common;

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
        /// Scene world matrix
        /// </summary>
        private readonly Matrix world = Matrix.Identity;
        /// <summary>
        /// Scene mode
        /// </summary>
        private SceneModes sceneMode = SceneModes.Unknown;
        /// <summary>
        /// Audio manager
        /// </summary>
        private GameAudioManager audioManager = null;

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
        protected GameAudioManager AudioManager
        {
            get
            {
                return audioManager ??= new();
            }
        }

        /// <summary>
        /// Game class
        /// </summary>
        public Game Game { get; private set; }
        /// <summary>
        /// Game environment
        /// </summary>
        public GameEnvironment GameEnvironment { get; private set; } = new();
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
        /// Scene component list
        /// </summary>
        public SceneComponentCollection Components { get; private set; } = new();
        /// <summary>
        /// Scene lights
        /// </summary>
        public SceneLights Lights { get; protected set; }
        /// <summary>
        /// Gets or sets if scene has to perform frustum culling with objects
        /// </summary>
        public bool PerformFrustumCulling { get; protected set; } = true;

        /// <summary>
        /// Gets or sets the top most control in the UI hierarchy
        /// </summary>
        public IUIControl TopMostControl { get; set; }
        /// <summary>
        /// Gets or sets the focused control the UI
        /// </summary>
        public IUIControl FocusedControl { get; set; }
        /// <summary>
        /// Gets whether the input is being processed by user interface controls
        /// </summary>
        public bool InputProcessedByUI { get { return FocusedControl != null && TopMostControl != null; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene(Game game)
        {
            if (game != null)
            {
                Game = game;
                Game.Graphics.Resized += FireGraphicsResized;

                var form = Game.Form;
                Camera = Camera.CreateFree(
                    Vector3.ForwardLH * -10f,
                    Vector3.Zero,
                    form.RenderWidth,
                    form.RenderHeight);
            }

            Components.Updated += ComponentsUpdated;

            Lights = SceneLights.CreateDefault(this);
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
            if (!disposing)
            {
                return;
            }

            Game.Graphics.Resized -= FireGraphicsResized;

            Renderer?.Dispose();
            Renderer = null;

            audioManager?.Dispose();
            audioManager = null;

            Camera?.Dispose();
            Camera = null;

            if (Components != null)
            {
                Components.Updated -= ComponentsUpdated;
                Components.Dispose();
                Components = null;
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
        public virtual void Update(IGameTime gameTime)
        {
            try
            {
                GameEnvironment.Update(gameTime);

                this.EvaluateInput();

                // Lights
                Lights?.Update();

                // Camera!
                Camera?.Update(gameTime);

                AudioManager?.Update(gameTime);

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
        public virtual void Draw(IGameTime gameTime)
        {
            try
            {
                Logger.WriteInformation(this, $"{nameof(Scene)} =>Updating Scene Globals.");

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

            renderer.PrepareScene();

            ReplaceRenderer(renderer);
            sceneMode = mode;

            FrameCounters.ClearAll();

            return true;
        }
        /// <summary>
        /// Replaces the renderer
        /// </summary>
        /// <param name="renderer">New renderer</param>
        private void ReplaceRenderer(ISceneRenderer renderer)
        {
            if (Renderer != null && renderer != null)
            {
                renderer.PostProcessingObjectsEffects = Renderer.PostProcessingObjectsEffects;
                renderer.PostProcessingUIEffects = Renderer.PostProcessingUIEffects;
                renderer.PostProcessingFinalEffects = Renderer.PostProcessingFinalEffects;
            }

            Renderer?.Dispose();
            Renderer = renderer;
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

            var fittedComponents = Components.Get<IScreenFitted>();
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
                Camera.ViewProjection,
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
            var component = (TObj)Activator.CreateInstance(typeof(TObj), this, id, name);

            await component.ReadAssets(description);
            await component.InitializeAssets();

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

            Components.AddComponent(component, usage, layer);

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
        /// Adds a component to the scene
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <param name="component">Component instance</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> AddComponent<TObj>(TObj component, SceneObjectUsages usage = SceneObjectUsages.None, int layer = LayerDefault)
            where TObj : ISceneObject
        {
            if (Equals(component, default(TObj)))
            {
                throw new ArgumentNullException(nameof(component));
            }

            Components.AddComponent(component, usage, layer);

            return await Task.FromResult(component);
        }

        /// <summary>
        /// Components updated event
        /// </summary>
        private void ComponentsUpdated(object sender, EventArgs e)
        {
            Renderer?.UpdateGlobals();
        }

        /// <summary>
        /// Gets the whole ground bounding box
        /// </summary>
        /// <param name="usage">Object usage</param>
        /// <returns>Returns the whole ground bounding box.</returns>
        public BoundingBox GetBoundingBox(SceneObjectUsages usage)
        {
            if (BoundingBox.HasValue && BoundingBox != new BoundingBox())
            {
                return BoundingBox.Value;
            }

            var boxes = GetBoundingBoxes(usage);
            if (!boxes.Any())
            {
                return new BoundingBox(Vector3.One * float.MinValue, Vector3.One * float.MaxValue);
            }

            BoundingBox = Helper.MergeBoundingBox(boxes);

            return BoundingBox.Value;
        }
        /// <summary>
        /// Gets the bounding box list
        /// </summary>
        /// <param name="usage">Object usage</param>
        public IEnumerable<BoundingBox> GetBoundingBoxes(SceneObjectUsages usage)
        {
            var cmpList = Components.Get(usage);
            if (!cmpList.Any())
            {
                yield break;
            }

            foreach (var obj in cmpList)
            {
                if (obj is IComposed composed)
                {
                    var pickComponents = composed.GetComponents<IRayPickable<Triangle>>();
                    foreach (var pickable in pickComponents)
                    {
                        yield return pickable.GetBoundingBox();
                    }
                }
                else if (obj is IRayPickable<Triangle> pickable)
                {
                    yield return pickable.GetBoundingBox();
                }
            }
        }

        /// <summary>
        /// Gets picking ray from current mouse position
        /// </summary>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns picking ray from current mouse position</returns>
        public PickingRay GetPickingRay(PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            int mouseX = Game.Input.MouseX;
            int mouseY = Game.Input.MouseY;
            var worldViewProjection = world * Camera.ViewProjection;
            float nDistance = Camera.NearPlaneDistance;
            float fDistance = Camera.FarPlaneDistance;
            var viewport = Game.Graphics.Viewport;

            var nVector = new Vector3(mouseX, mouseY, nDistance);
            var fVector = new Vector3(mouseX, mouseY, fDistance);

            var nPoint = Vector3.Unproject(nVector, 0, 0, viewport.Width, viewport.Height, nDistance, fDistance, worldViewProjection);
            var fPoint = Vector3.Unproject(fVector, 0, 0, viewport.Width, viewport.Height, nDistance, fDistance, worldViewProjection);

            return new PickingRay(nPoint, Vector3.Normalize(fPoint - nPoint), pickingParams);
        }

        /// <summary>
        /// Gets the scene volume for culling tests
        /// </summary>
        /// <returns>Returns the scene volume</returns>
        public ICullingVolume GetSceneVolume()
        {
            return Components.First<IGround>(SceneObjectUsages.Ground)?.GetCullingVolume();
        }

        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(Point position, PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            return GetTopDownRay(position.X, position.Y, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(Vector2 position, PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            return GetTopDownRay(position.X, position.Y, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(Vector3 position, PickingHullTypes pickingParams = PickingHullTypes.Default)
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
        public PickingRay GetTopDownRay(float x, float z, PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            var bbox = GetBoundingBox(SceneObjectUsages.Ground);

            float maxY = bbox.Maximum.Y + 1.0f;

            return new PickingRay(new Vector3(x, maxY, z), Vector3.Down, pickingParams);
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

            if (this.PickNearest<T>(ray, SceneObjectUsages.Ground, out var res))
            {
                result = res.PickingResult;

                return true;
            }

            result = new PickingResult<T>
            {
                Distance = float.MaxValue
            };

            return false;
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

            if (this.PickFirst<T>(ray, SceneObjectUsages.Ground, out var res))
            {
                result = res.PickingResult;

                return true;
            }

            result = new PickingResult<T>
            {
                Distance = float.MaxValue
            };

            return false;
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

            if (this.PickAll<T>(ray, SceneObjectUsages.Ground, out var res))
            {
                results = res.SelectMany(r => r.PickingResults);

                return true;
            }

            results = [];

            return false;
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

            bool picked = this.PickAll<T>(ray, SceneObjectUsages.Ground, out var pResults);
            if (picked)
            {
                result = pResults
                    .SelectMany(r => r.PickingResults)
                    .OrderBy(r => Vector3.DistanceSquared(from, r.Position))
                    .First();

                return true;
            }

            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            return false;
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
                Components = Components.Get<IHasGameState>().Select(c => c.GetState()).OfType<ISceneObjectState>().ToArray(),
            };
        }
        /// <inheritdoc/>
        public virtual void SetState(IGameState state)
        {
            if (state is not SceneState sceneState)
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
                var component = Components.ById(componentState.Id) as IHasGameState;

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
