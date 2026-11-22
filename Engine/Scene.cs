using Engine.Common;
using Engine.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
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
        private readonly Matrix4x4 world = Matrix4x4.Identity;
        /// <summary>
        /// Scene mode
        /// </summary>
        private SceneModes sceneMode = SceneModes.Unknown;

        /// <summary>
        /// Scene bounding box
        /// </summary>
        protected BoundingBox? Bounds;
        /// <summary>
        /// Scene renderer
        /// </summary>
        protected ISceneRenderer Renderer = null;

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
        /// <inheritdoc/>
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

            Camera?.Dispose();
            Camera = null;

            Components?.Dispose();
            Components = null;
        }

        /// <summary>
        /// Initialize scene
        /// </summary>
        public virtual void Initialize()
        {

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

                // Action!
                Renderer?.Update(gameTime);
            }
            catch (EngineException ex)
            {
                Game.Logger.WriteError(this, $"{nameof(Scene)} => Updating error: {ex.Message}", ex);

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
                Game.Logger.WriteInformation(this, $"{nameof(Scene)} =>Updating Scene Globals.");

                Renderer?.Draw(gameTime);
            }
            catch (EngineException ex)
            {
                Game.Logger.WriteError(this, $"{nameof(Scene)} => Drawing error {Renderer?.GetType()}: {ex.Message}", ex);

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
        public void LoadResources(ILoadResourceGroup taskGroup)
        {
            Game.LoadResources(taskGroup);
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
            if (fittedComponents.Length != 0)
            {
                fittedComponents
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(c => c.Resize());
            }

            GameGraphicsResized();
        }

        /// <summary>
        /// Grame graphics resized
        /// </summary>
        public virtual void GameGraphicsResized()
        {
            Game.Logger.WriteTrace(this, $"{nameof(Scene)} => Graphics resized.");
        }

        /// <summary>
        /// Gets the screen coordinates
        /// </summary>
        /// <param name="position">3D position</param>
        /// <param name="inside">Returns true if the resulting point is inside the screen</param>
        /// <returns>Returns the screen coordinates</returns>
        public Vector2 GetScreenCoordinates(Vector3 position, out bool inside)
        {
            return Game.Graphics.Viewport.UnprojectToScreen(position, Camera.ViewProjection, out inside);
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
        /// Creates a component
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <typeparam name="TDescription">Component description type</typeparam>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="additionalParams">Additional instace constructor parameters</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> CreateComponent<TObj, TDescription>(string id, string name, TDescription description, params object[] additionalParams)
            where TObj : BaseSceneObject<TDescription>
            where TDescription : SceneObjectDescription
        {
            object[] instanceParams = [this, id, name, .. additionalParams ?? []];

            var component = (TObj)Activator.CreateInstance(typeof(TObj), instanceParams);

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

            Components.AddComponent(component, usage, layer, ComponentsUpdated);

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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public async Task<TObj> AddComponent<TObj>(string id, string name, SceneObjectUsages usage = SceneObjectUsages.None, int layer = LayerDefault)
            where TObj : BaseSceneObject<SceneObjectDescription>
        {
            return await AddComponentInternal<TObj, SceneObjectDescription>(id, name, new(), usage, layer);
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

            Components.AddComponent(component, usage, layer, ComponentsUpdated);

            return await Task.FromResult(component);
        }

        /// <summary>
        /// Removes the specified component
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <param name="component">Component instance</param>
        public void RemoveComponent<TObj>(TObj component) where TObj : ISceneObject
        {
            Components.RemoveComponent(component, ComponentsUpdated);
        }
        /// <summary>
        /// Removes the specified component list
        /// </summary>
        /// <typeparam name="TObj">Component type</typeparam>
        /// <param name="components">Component instance list</param>
        public void RemoveComponents<TObj>(TObj[] components) where TObj : ISceneObject
        {
            Components.RemoveComponents([.. components.OfType<ISceneObject>()], ComponentsUpdated);
        }

        /// <summary>
        /// Components updated event
        /// </summary>
        private void ComponentsUpdated(ISceneObject[] components)
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
            if (Bounds.HasValue && Bounds != new BoundingBox())
            {
                return Bounds.Value;
            }

            var boxes = GetBoundingBoxes(usage);
            if (boxes.Length == 0)
            {
                return new BoundingBox(Vector3.One * -100f, Vector3.One * 100f);
            }

            Bounds = BoundingBox.Merge([.. boxes]);

            return Bounds.Value;
        }
        /// <summary>
        /// Gets the bounding box list
        /// </summary>
        /// <param name="usage">Object usage</param>
        public BoundingBox[] GetBoundingBoxes(SceneObjectUsages usage)
        {
            var cmpList = Components.Get(usage);
            if (cmpList.Length == 0)
            {
                return [];
            }

            List<BoundingBox> res = [];
            foreach (var obj in cmpList)
            {
                if (obj is IComposed composed)
                {
                    var pickComponents = composed.GetComponents<IRayPickable<Triangle>>();
                    foreach (var pickable in pickComponents)
                    {
                        res.Add(pickable.GetBoundingBox());
                    }
                }
                else if (obj is IRayPickable<Triangle> pickable)
                {
                    res.Add(pickable.GetBoundingBox());
                }
            }
            return [.. res];
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
        public PickingRay GetTopDownRay(ref Point position, PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            return GetTopDownRay(position.X, position.Y, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(ref Vector2 position, PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            return GetTopDownRay(position.X, position.Y, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(ref Vector3 position, PickingHullTypes pickingParams = PickingHullTypes.Default)
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

            if (!this.PickNearest<T>(ref ray, SceneObjectUsages.Ground, out var res))
            {
                result = PickingResult<T>.Empty;
                return false;
            }

            result = res.PickingResult;
            return true;
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

            if (!this.PickFirst<T>(ref ray, SceneObjectUsages.Ground, out var res))
            {
                result = PickingResult<T>.Empty;
                return false;
            }

            result = res.PickingResult;
            return true;
        }
        /// <summary>
        /// Gets all ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition<T>(float x, float z, out PickingResult<T>[] results) where T : IRayIntersectable
        {
            var ray = GetTopDownRay(x, z);

            if (!this.PickAll<T>(ref ray, SceneObjectUsages.Ground, out var res))
            {
                results = [];
                return false;
            }

            if (res.Length == 1)
            {
                results = [.. res[0].GetResults()];
                return true;
            }

            List<PickingResult<T>> picks = [];
            for (int i = 0; i < res.Length; i++)
            {
                picks.AddRange(res[i].GetResults());
            }

            results = [.. picks];
            return true;
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

            if (!this.PickAll<T>(ref ray, SceneObjectUsages.Ground, out var res))
            {
                result = PickingResult<T>.Empty;
                return false;
            }

            PickingResult<T> nearest = default;
            float d = float.MaxValue;
            foreach (var pr in res)
            {
                foreach (var curr in pr.GetResults())
                {
                    float dsq = Vector3.DistanceSquared(from, curr.Position);
                    if (dsq < d)
                    {
                        nearest = curr;
                        d = dsq;
                    }
                }
            }

            result = nearest;
            return true;
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
                GroundBoundingBoxMin = Bounds?.Minimum,
                GroundBoundingBoxMax = Bounds?.Maximum,
                GameEnvironment = GameEnvironment.GetState(),
                SceneLights = Lights.GetState(),
                Camera = Camera.GetState(),
                Components = [.. Components.Get<IHasGameState>().Select(c => c.GetState()).OfType<ISceneObjectState>()],
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
                Bounds = new BoundingBox(
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
