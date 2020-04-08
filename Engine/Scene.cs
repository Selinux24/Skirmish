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

    /// <summary>
    /// Render scene
    /// </summary>
    public class Scene : IDisposable
    {
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
        /// <param name="ray">Ray</param>
        /// <param name="obj">Object</param>
        /// <param name="maxDistance">Maximum distance to test</param>
        /// <param name="distance">Gets the picking distance if intersection exists</param>
        /// <returns>Returns true if exists intersection between the ray and the bounding volume of the object, into the maximum distance</returns>
        private static bool TestCoarse(ref Ray ray, IRayPickable<Triangle> obj, float maxDistance, out float distance)
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
        /// Ground usage enum for ground picking
        /// </summary>
        private const SceneObjectUsages GroundUsage = SceneObjectUsages.Ground | SceneObjectUsages.FullPathFinding | SceneObjectUsages.CoarsePathFinding;
        /// <summary>
        /// Gets wether the ray picks the object nearest to the specified best distance
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="obj">Object to test</param>
        /// <param name="bestDistance">Best distance</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if the ray picks the object nearest to the specified best distance</returns>
        private static bool PickNearestSingle(Ray ray, RayPickingParams rayPickingParams, IRayPickable<Triangle> obj, float bestDistance, out PickingResult<Triangle> result)
        {
            bool pickedNearest = false;

            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            var picked = obj.PickNearest(ray, rayPickingParams, out PickingResult<Triangle> r);
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
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="obj">Object to test</param>
        /// <param name="bestDistance">Best distance</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if the ray picks the object nearest to the specified best distance</returns>
        private static bool PickNearestComposed(Ray ray, RayPickingParams rayPickingParams, IComposed obj, float bestDistance, out PickingResult<Triangle> result)
        {
            bool pickedNearest = false;

            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            float dist = bestDistance;

            var pickComponents = obj.GetComponents<IRayPickable<Triangle>>();

            foreach (var pickable in pickComponents)
            {
                var picked = pickable.PickNearest(ray, rayPickingParams, out PickingResult<Triangle> r);
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
        /// <returns>Returns the triangle list</returns>
        private static IEnumerable<Triangle> GetTrianglesForNavigationGraph(ISceneObject obj)
        {
            List<Triangle> tris = new List<Triangle>();

            List<IRayPickable<Triangle>> volumes = new List<IRayPickable<Triangle>>();

            if (obj is IComposed composed)
            {
                volumes.AddRange(GetVolumesForNavigationGraph(composed));
            }
            else if (obj is IRayPickable<Triangle> pickable)
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
        /// <param name="composed">Composed</param>
        /// <returns>Returns a list of volumes</returns>
        private static IEnumerable<IRayPickable<Triangle>> GetVolumesForNavigationGraph(IComposed composed)
        {
            List<IRayPickable<Triangle>> volumes = new List<IRayPickable<Triangle>>();

            var trnChilds = composed.GetComponents<ITransformable3D>();
            if (trnChilds.Any())
            {
                foreach (var child in trnChilds)
                {
                    child.Manipulator.UpdateInternals(true);
                }
            }

            var pickableChilds = composed.GetComponents<IRayPickable<Triangle>>();
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
        /// Control captured with mouse
        /// </summary>
        /// <remarks>When mouse was pressed, the control beneath him was stored here. When mouse is released, if it is above this control, an click event occurs</remarks>
        private IControl capturedControl = null;
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
        /// Game class
        /// </summary>
        public Game Game { get; private set; }
        /// <summary>
        /// Scene renderer
        /// </summary>
        protected ISceneRenderer Renderer = null;
        /// <summary>
        /// Gets or sets whether the scene was handling control captures
        /// </summary>
        protected bool CapturedControl { get; private set; }
        /// <summary>
        /// Flag to update the scene global resources
        /// </summary>
        protected bool UpdateGlobalResources { get; set; }
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
        /// Time of day controller
        /// </summary>
        public TimeOfDay TimeOfDay { get; set; }
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
        public bool PerformFrustumCulling { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene(Game game) : this(game, SceneModes.ForwardLigthning)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene(Game game, SceneModes sceneMode)
        {
            this.Game = game;

            this.Game.ResourcesLoading += FireResourcesLoading;
            this.Game.ResourcesLoaded += FireResourcesLoaded;
            this.Game.Graphics.Resized += FireGraphicsResized;

            this.TimeOfDay = new TimeOfDay();

            this.AudioManager = new GameAudioManager();

            this.Camera = Camera.CreateFree(
                new Vector3(0.0f, 0.0f, -10.0f),
                Vector3.Zero);

            this.Camera.SetLens(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight);

            this.Lights = SceneLights.CreateDefault();

            this.PerformFrustumCulling = true;

            if (!this.SetRenderMode(sceneMode))
            {
                throw new EngineException($"Bad render mode: {sceneMode}");
            }

            this.UpdateGlobalResources = true;
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
                this.Game.ResourcesLoading -= FireResourcesLoading;
                this.Game.ResourcesLoaded -= FireResourcesLoaded;
                this.Game.Graphics.Resized -= FireGraphicsResized;

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
                        internalComponents[i]?.Dispose();
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
                if (this.UpdateGlobalResources)
                {
                    this.UpdateGlobals();

                    this.UpdateGlobalResources = false;
                }

                this.Camera?.Update(gameTime);

                this.TimeOfDay?.Update(gameTime);

                this.AudioManager?.Update();

                this.NavigationGraph?.Update(gameTime);

                this.Lights?.UpdateLights(this.TimeOfDay);

                this.Renderer?.Update(gameTime, this);

                this.CapturedControl = this.capturedControl != null;

                //Process 2D controls
                var ctrls = this.GetComponents()
                    .Where(c => c.Active)
                    .OfType<IControl>()
                    .ToArray();
                foreach (var ctrl in ctrls)
                {
                    ctrl.MouseOver = ctrl.Rectangle.Contains(this.Game.Input.MouseX, this.Game.Input.MouseY);

                    if (this.Game.Input.LeftMouseButtonJustPressed && ctrl.MouseOver)
                    {
                        this.capturedControl = ctrl;
                    }

                    if (this.Game.Input.LeftMouseButtonJustReleased && ctrl.MouseOver && this.capturedControl == ctrl)
                    {
                        ctrl.FireOnClickEvent();
                    }

                    ctrl.Pressed = this.Game.Input.LeftMouseButtonPressed && this.capturedControl == ctrl;
                }

                if (!this.Game.Input.LeftMouseButtonPressed) this.capturedControl = null;
            }
            catch (EngineException ex)
            {
                Console.WriteLine($"Scene Updating error: {ex.Message}");

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
                this.Renderer?.Draw(gameTime, this);
            }
            catch (EngineException ex)
            {
                Console.WriteLine($"Scene Drawing error {this.Renderer?.GetType()}: {ex.Message}");

                throw;
            }
        }

        /// <summary>
        /// Gets the render mode
        /// </summary>
        /// <returns>Returns the render mode</returns>
        public SceneModes GetRenderMode()
        {
            return this.sceneMode;
        }
        /// <summary>
        /// Change renderer mode
        /// </summary>
        /// <param name="mode">New renderer mode</param>
        /// <returns>Returns true if the renderer changes correctly</returns>
        public bool SetRenderMode(SceneModes mode)
        {
            var graphics = this.Game.Graphics;

            ISceneRenderer renderer;

            if (mode == SceneModes.ForwardLigthning && SceneRendererForward.Validate(graphics))
            {
                renderer = new SceneRendererForward(this.Game);
            }
            else if (mode == SceneModes.DeferredLightning && SceneRendererDeferred.Validate(graphics))
            {
                renderer = new SceneRendererDeferred(this.Game);
            }
            else
            {
                return false;
            }

            this.Renderer?.Dispose();
            this.Renderer = renderer;
            this.sceneMode = mode;

            Counters.ClearAll();

            return true;
        }

        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Load id</param>
        /// <param name="tasks">Resource load tasks</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        public bool LoadResources(Guid id, params Task[] tasks)
        {
            return this.Game.LoadResources(this, id, tasks);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Load id</param>
        /// <param name="tasks">Resource load tasks</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        public async Task<bool> LoadResourcesAsync(Guid id, params Task[] tasks)
        {
            return await this.Game.LoadResourcesAsync(this, id, tasks);
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
            this.Renderer?.Resize();

            var fittedComponents = this.GetComponents().OfType<IScreenFitted>();
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
        public virtual void OnReportProgress(float value)
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
                this.Game.Graphics.Viewport,
                this.Camera.View * this.Camera.Projection,
                out inside);
        }

        /// <summary>
        /// Adds component to collection
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        /// <param name="component">Component</param>
        /// <param name="usage">Usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the added component</returns>
        public void AddComponent(ISceneObject component, SceneObjectUsages usage, int order)
        {
            if (this.internalComponents.Contains(component))
            {
                return;
            }

            component.Usage |= usage;

            if (order != 0)
            {
                component.Order = order;
            }

            Monitor.Enter(this.internalComponents);
            this.internalComponents.Add(component);
            this.internalComponents.Sort((p1, p2) =>
            {
                //First by order index
                int i = p1.Order.CompareTo(p2.Order);
                if (i != 0) return i;

                //Then opaques
                i = p1.AlphaEnabled.CompareTo(p2.AlphaEnabled);
                if (i != 0) return i;

                //Then z-buffer writers
                i = p1.DepthEnabled.CompareTo(p2.DepthEnabled);

                return i;
            });
            Monitor.Exit(this.internalComponents);

            this.UpdateGlobalResources = true;
        }
        /// <summary>
        /// Removes and disposes the specified component
        /// </summary>
        /// <param name="component">Component</param>
        public void RemoveComponent(ISceneObject component)
        {
            if (!this.internalComponents.Contains(component))
            {
                return;
            }

            Monitor.Enter(this.internalComponents);
            this.internalComponents.Remove(component);
            Monitor.Exit(this.internalComponents);

            this.UpdateGlobalResources = true;

            component.Dispose();
        }
        /// <summary>
        /// Removes and disposes the specified component list
        /// </summary>
        /// <param name="components">List of components</param>
        public void RemoveComponents(IEnumerable<ISceneObject> components)
        {
            Monitor.Enter(this.internalComponents);
            foreach (var component in components)
            {
                if (this.internalComponents.Contains(component))
                {
                    this.internalComponents.Remove(component);

                    this.UpdateGlobalResources = true;
                }

                component.Dispose();
            }
            Monitor.Exit(this.internalComponents);
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
        /// Update global resources
        /// </summary>
        protected virtual void UpdateGlobals()
        {
            this.UpdateMaterialPalette(out EngineShaderResourceView materialPalette, out uint materialPaletteWidth);

            this.UpdateAnimationPalette(out EngineShaderResourceView animationPalette, out uint animationPaletteWidth);

            DrawerPool.UpdateSceneGlobals(materialPalette, materialPaletteWidth, animationPalette, animationPaletteWidth);
        }
        /// <summary>
        /// Updates the global material palette
        /// </summary>
        /// <param name="materialPalette">Material palette</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        private void UpdateMaterialPalette(out EngineShaderResourceView materialPalette, out uint materialPaletteWidth)
        {
            List<MeshMaterial> mats = new List<MeshMaterial>
            {
                MeshMaterial.Default
            };

            var matComponents = this.GetComponents().OfType<IUseMaterials>();

            foreach (var component in matComponents)
            {
                var matList = component.Materials;
                if (matList.Any())
                {
                    mats.AddRange(matList);
                }
            }

            List<MeshMaterial> addedMats = new List<MeshMaterial>();

            List<Vector4> values = new List<Vector4>();

            for (int i = 0; i < mats.Count; i++)
            {
                var mat = mats[i];
                if (!addedMats.Contains(mat))
                {
                    var matV = mat.Pack();

                    mat.ResourceIndex = (uint)addedMats.Count;
                    mat.ResourceOffset = (uint)values.Count;
                    mat.ResourceSize = (uint)matV.Length;

                    values.AddRange(matV);

                    addedMats.Add(mat);
                }
                else
                {
                    var cMat = addedMats.Find(m => m.Equals(mat));

                    mat.ResourceIndex = cMat.ResourceIndex;
                    mat.ResourceOffset = cMat.ResourceOffset;
                    mat.ResourceSize = cMat.ResourceSize;
                }
            }

            int texWidth = GetTextureSize(values.Count);

            materialPalette = this.Game.ResourceManager.CreateGlobalResource("MaterialPalette", values.ToArray(), texWidth);
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

            var skComponents = this.GetComponents().OfType<IUseSkinningData>();

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

                    sk.ResourceIndex = (uint)addedSks.Count;
                    sk.ResourceOffset = (uint)values.Count;
                    sk.ResourceSize = (uint)skV.Length;

                    values.AddRange(skV);

                    addedSks.Add(sk);
                }
                else
                {
                    var cMat = addedSks.Find(m => m.Equals(sk));

                    sk.ResourceIndex = cMat.ResourceIndex;
                    sk.ResourceOffset = cMat.ResourceOffset;
                    sk.ResourceSize = cMat.ResourceSize;
                }
            }

            int texWidth = GetTextureSize(values.Count);

            animationPalette = this.Game.ResourceManager.CreateGlobalResource("AnimationPalette", values.ToArray(), texWidth);
            animationPaletteWidth = (uint)texWidth;
        }

        /// <summary>
        /// Gets picking ray from current mouse position
        /// </summary>
        /// <returns>Returns picking ray from current mouse position</returns>
        public Ray GetPickingRay()
        {
            int mouseX = this.Game.Input.MouseX;
            int mouseY = this.Game.Input.MouseY;
            Matrix worldViewProjection = this.world * this.Camera.View * this.Camera.Projection;
            float nDistance = this.Camera.NearPlaneDistance;
            float fDistance = this.Camera.FarPlaneDistance;
            ViewportF viewport = this.Game.Graphics.Viewport;

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
            return this.GetTopDownRay(position.X, position.Y);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public Ray GetTopDownRay(Vector2 position)
        {
            return this.GetTopDownRay(position.X, position.Y);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public Ray GetTopDownRay(Vector3 position)
        {
            return this.GetTopDownRay(position.X, position.Z);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public Ray GetTopDownRay(float x, float z)
        {
            var bbox = this.GetGroundBoundingBox();

            if (!bbox.HasValue || bbox == new BoundingBox())
            {
                Console.WriteLine($"A ground must be defined into the scene in the first place.");
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
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="usage">Object usage mask</param>
        /// <param name="model">Gets the resulting ray pickable object</param>
        /// <returns>Returns true if a pickable object in the ray path was found</returns>
        public bool PickNearest(Ray ray, float maxDistance, RayPickingParams rayPickingParams, SceneObjectUsages usage, out ISceneObject model)
        {
            model = null;

            var cmpList = this.GetComponents().Where(c => c.Usage.HasFlag(usage));

            var coarse = PickCoarse(ref ray, maxDistance, cmpList);

            foreach (var obj in coarse)
            {
                if (obj.Item1 is IRayPickable<Triangle> pickable && pickable.PickNearest(ray, rayPickingParams, out PickingResult<Triangle> r))
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
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickNearest(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            return PickNearest(ray, rayPickingParams, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickNearest(Ray ray, RayPickingParams rayPickingParams, SceneObjectUsages usage, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            IEnumerable<ISceneObject> cmpList = this.GetComponents();

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
                    bool pickedComposed = PickNearestComposed(ray, rayPickingParams, composed, bestDistance, out var r);
                    if (pickedComposed)
                    {
                        result = r;

                        bestDistance = r.Distance;
                        picked = true;
                    }
                }
                else if (obj.Item1 is IRayPickable<Triangle> pickable)
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
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickFirst(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            return PickFirst(ray, rayPickingParams, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickFirst(Ray ray, RayPickingParams rayPickingParams, SceneObjectUsages usage, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            IEnumerable<ISceneObject> cmpList = this.GetComponents();

            if (usage != SceneObjectUsages.None)
            {
                cmpList = cmpList.Where(c => (c.Usage & usage) != SceneObjectUsages.None);
            }

            var coarse = PickCoarse(ref ray, float.MaxValue, cmpList);

            foreach (var obj in coarse)
            {
                if (obj.Item1 is IComposed composed)
                {
                    var pickComponents = composed.GetComponents<IRayPickable<Triangle>>();
                    foreach (var pickable in pickComponents)
                    {
                        if (pickable.PickFirst(ray, rayPickingParams, out PickingResult<Triangle> r))
                        {
                            result = r;

                            return true;
                        }
                    }
                }
                else if (
                    obj.Item1 is IRayPickable<Triangle> pickable &&
                    pickable.PickFirst(ray, rayPickingParams, out PickingResult<Triangle> r))
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
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickAll(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle>[] results)
        {
            return PickAll(ray, rayPickingParams, SceneObjectUsages.None, out results);
        }
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="usage">Component usage</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickAll(Ray ray, RayPickingParams rayPickingParams, SceneObjectUsages usage, out PickingResult<Triangle>[] results)
        {
            results = null;

            IEnumerable<ISceneObject> cmpList = this.GetComponents();

            if (usage != SceneObjectUsages.None)
            {
                cmpList = cmpList.Where(c => (c.Usage & usage) != SceneObjectUsages.None);
            }

            var coarse = PickCoarse(ref ray, float.MaxValue, cmpList);

            List<PickingResult<Triangle>> lResults = new List<PickingResult<Triangle>>();

            foreach (var obj in coarse)
            {
                if (obj.Item1 is IComposed composed)
                {
                    var pickComponents = composed.GetComponents<IRayPickable<Triangle>>();
                    foreach (var pickable in pickComponents)
                    {
                        if (pickable.PickAll(ray, rayPickingParams, out PickingResult<Triangle>[] r))
                        {
                            lResults.AddRange(r);
                        }
                    }
                }
                else if (
                    obj.Item1 is IRayPickable<Triangle> pickable &&
                    pickable.PickAll(ray, rayPickingParams, out PickingResult<Triangle>[] r))
                {
                    lResults.AddRange(r);
                }
            }

            results = lResults.ToArray();

            return results.Length > 0;
        }

        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindTopGroundPosition(float x, float z, out PickingResult<Triangle> result)
        {
            var ray = this.GetTopDownRay(x, z);

            return this.PickNearest(ray, RayPickingParams.Default, GroundUsage, out result);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindFirstGroundPosition(float x, float z, out PickingResult<Triangle> result)
        {
            var ray = this.GetTopDownRay(x, z);

            return this.PickFirst(ray, RayPickingParams.Default, GroundUsage, out result);
        }
        /// <summary>
        /// Gets all ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition(float x, float z, out PickingResult<Triangle>[] results)
        {
            var ray = this.GetTopDownRay(x, z);

            return this.PickAll(ray, RayPickingParams.Default, GroundUsage, out results);
        }
        /// <summary>
        /// Gets nearest ground position to "from" position
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindNearestGroundPosition(Vector3 from, out PickingResult<Triangle> result)
        {
            var ray = this.GetTopDownRay(from.X, from.Z);

            bool picked = this.PickAll(ray, RayPickingParams.Default, GroundUsage, out PickingResult<Triangle>[] pResults);
            if (picked)
            {
                int index = -1;
                float dist = float.MaxValue;
                for (int i = 0; i < pResults.Length; i++)
                {
                    float d = Vector3.DistanceSquared(from, pResults[i].Position);
                    if (d <= dist)
                    {
                        dist = d;

                        index = i;
                    }
                }

                result = pResults[index];

                return true;
            }
            else
            {
                result = new PickingResult<Triangle>()
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
            if (this.groundBoundingBox.HasValue && this.groundBoundingBox != new BoundingBox())
            {
                return this.groundBoundingBox;
            }

            //Try to get a bounding box from the current ground objects
            var cmpList = this.GetComponents().Where(c => c.Usage.HasFlag(SceneObjectUsages.Ground));

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

                this.groundBoundingBox = Helper.MergeBoundingBox(boxes);
            }

            return this.groundBoundingBox;
        }
        /// <summary>
        /// Gets the current navigation bounding box
        /// </summary>
        /// <returns>Returns the current navigation bounding box.</returns>
        public BoundingBox? GetNavigationBoundingBox()
        {
            return this.navigationBoundingBox;
        }

        /// <summary>
        /// Gets the scene volume for culling tests
        /// </summary>
        /// <returns>Returns the scene volume</returns>
        public ICullingVolume GetSceneVolume()
        {
            var ground = this.GetComponents()
                .Where(c => c.Usage.HasFlag(SceneObjectUsages.Ground))
                .OfType<Ground>()
                .FirstOrDefault();
            if (ground != null)
            {
                return ground.GetCullingVolume();
            }

            return null;
        }

        /// <summary>
        /// Set ground geometry
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="fullGeometryPathFinding">Sets whether use full triangle list or volumes for navigation graphs</param>
        public void SetGround(ISceneObject obj, bool fullGeometryPathFinding)
        {
            this.groundBoundingBox = null;

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
        public void AttachToGround(ISceneObject obj, bool fullGeometryPathFinding)
        {
            obj.Usage |= fullGeometryPathFinding ? SceneObjectUsages.FullPathFinding : SceneObjectUsages.CoarsePathFinding;
        }

        /// <summary>
        /// Updates the navigation graph
        /// </summary>
        public virtual async Task UpdateNavigationGraph()
        {
            if (this.PathFinderDescription == null)
            {
                this.SetNavigationGraph(null);

                return;
            }

            var graph = await this.PathFinderDescription.Build();

            this.SetNavigationGraph(graph);

            this.NavigationGraphUpdated();
        }
        /// <summary>
        /// Sets a navigation graph
        /// </summary>
        /// <param name="graph">Navigation graph</param>
        public virtual void SetNavigationGraph(IGraph graph)
        {
            if (this.NavigationGraph != null)
            {
                this.NavigationGraph.Updating -= GraphUpdating;
                this.NavigationGraph.Updated -= GraphUpdated;

                this.NavigationGraph.Dispose();
                this.NavigationGraph = null;

                this.navigationBoundingBox = new BoundingBox();
            }

            if (graph != null)
            {
                this.NavigationGraph = graph;
                this.NavigationGraph.Updating += GraphUpdating;
                this.NavigationGraph.Updated += GraphUpdated;

                if (this.PathFinderDescription?.Input != null)
                {
                    this.navigationBoundingBox = this.PathFinderDescription.Input.BoundingBox;
                }
            }
        }

        /// <summary>
        /// Graph updating event
        /// </summary>
        /// <param name="sender">Sender graph</param>
        /// <param name="e">Event args</param>
        private void GraphUpdating(object sender, EventArgs e)
        {
            NavigationGraphUpdating();
        }
        /// <summary>
        /// Graph updated event
        /// </summary>
        /// <param name="sender">Sender graph</param>
        /// <param name="e">Event args</param>
        private void GraphUpdated(object sender, EventArgs e)
        {
            NavigationGraphUpdated();
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

            var pfComponents = this.GetComponents().Where(c =>
            {
                return
                    !c.HasParent &&
                    c.Visible &&
                    (c.Usage.HasFlag(SceneObjectUsages.FullPathFinding) || c.Usage.HasFlag(SceneObjectUsages.CoarsePathFinding));
            });

            foreach (var cmp in pfComponents)
            {
                var currTris = GetTrianglesForNavigationGraph(cmp);
                if (currTris.Any())
                {
                    tris.AddRange(currTris);
                }
            }

            var bounds = this.PathFinderDescription.Settings.Bounds;
            if (bounds.HasValue)
            {
                tris = tris.FindAll(t =>
                {
                    var tbbox = BoundingBox.FromPoints(t.GetVertices());

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
            return this.NavigationGraph?.GetNodes(agent) ?? new IGraphNode[] { };
        }
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <param name="useGround">Use ground info</param>
        /// <param name="delta">Delta amount for path refinement</param>
        /// <returns>Return path if exists</returns>
        public virtual PathFindingPath FindPath(AgentType agent, Vector3 from, Vector3 to, bool useGround = false, float delta = 0f)
        {
            if (this.NavigationGraph?.Initialized != true)
            {
                return null;
            }

            if (useGround)
            {
                if (FindNearestGroundPosition(from, out PickingResult<Triangle> rFrom))
                {
                    from = rFrom.Position;
                }
                if (FindNearestGroundPosition(to, out PickingResult<Triangle> rTo))
                {
                    to = rTo.Position;
                }
            }

            var path = this.NavigationGraph.FindPath(agent, from, to);
            if (path.Count() > 1)
            {
                List<Vector3> positions;
                List<Vector3> normals;

                if (delta == 0)
                {
                    positions = new List<Vector3>(path);
                    normals = new List<Vector3>(Helper.CreateArray(path.Count(), Vector3.Up));
                }
                else
                {
                    ComputePath(path, delta, out positions, out normals);
                }

                if (useGround)
                {
                    ComputeGroundPositions(positions, normals);
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
        /// <param name="useGround">Use ground info</param>
        /// <param name="delta">Delta amount for path refinement</param>
        /// <returns>Return path if exists</returns>
        public virtual async Task<PathFindingPath> FindPathAsync(AgentType agent, Vector3 from, Vector3 to, bool useGround = false, float delta = 0f)
        {
            if (this.NavigationGraph?.Initialized != true)
            {
                return null;
            }

            if (useGround)
            {
                if (FindNearestGroundPosition(from, out PickingResult<Triangle> rFrom))
                {
                    from = rFrom.Position;
                }
                if (FindNearestGroundPosition(to, out PickingResult<Triangle> rTo))
                {
                    to = rTo.Position;
                }
            }

            var path = await this.NavigationGraph.FindPathAsync(agent, from, to);
            if (path.Count() > 1)
            {
                List<Vector3> positions;
                List<Vector3> normals;

                if (delta == 0)
                {
                    positions = new List<Vector3>(path);
                    normals = new List<Vector3>(Helper.CreateArray(path.Count(), Vector3.Up));
                }
                else
                {
                    ComputePath(path, delta, out positions, out normals);
                }

                if (useGround)
                {
                    ComputeGroundPositions(positions, normals);
                }

                return new PathFindingPath(positions, normals);
            }

            return null;
        }
        /// <summary>
        /// Compute path finding result
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="delta">Control point path deltas</param>
        /// <param name="positions">Resulting positions</param>
        /// <param name="normals">Resulting normals</param>
        private void ComputePath(IEnumerable<Vector3> path, float delta, out List<Vector3> positions, out List<Vector3> normals)
        {
            positions = new List<Vector3>();
            normals = new List<Vector3>();

            Vector3[] positionArray = path.ToArray();

            positions.Add(positionArray[0]);
            normals.Add(Vector3.Up);

            var p0 = positionArray[0];
            var p1 = positionArray[1];

            int index = 0;
            while (index < positionArray.Length - 1)
            {
                var s = p1 - p0;
                var v = Vector3.Normalize(s) * delta;
                var l = delta - s.Length();

                if (l <= 0f)
                {
                    //Into de segment
                    p0 += v;
                }
                else if (index < positionArray.Length - 2)
                {
                    //Next segment
                    var p2 = positionArray[index + 2];
                    p0 = p1 + ((p2 - p1) * l);
                    p1 = p2;

                    index++;
                }
                else
                {
                    //End
                    p0 = positionArray[index + 1];

                    index++;
                }

                positions.Add(p0);
                normals.Add(Vector3.Up);
            }
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
                if (FindNearestGroundPosition(positions[i], out PickingResult<Triangle> r))
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
            if (this.NavigationGraph != null)
            {
                return this.NavigationGraph.IsWalkable(agent, position, out nearest);
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

            bool isInGround = this.FindAllGroundPosition(newPosition.X, newPosition.Z, out PickingResult<Triangle>[] results);
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

            for (int i = 0; i < results.Length; i++)
            {
                if (this.IsWalkable(agent, results[i].Position, out Vector3? nearest))
                {
                    finalPosition = GetPositionWalkable(agent, prevPosition, newPosition, results[i].Position, adjustHeight);

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
            if (this.FindNearestGroundPosition(position, out PickingResult<Triangle> nearestResult))
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
            return this.NavigationGraph?.AddObstacle(cylinder) ?? -1;
        }
        /// <summary>
        /// Adds AABB obstacle
        /// </summary>
        /// <param name="bbox">AABB</param>
        /// <returns>Returns the obstacle Id</returns>
        public virtual int AddObstacle(BoundingBox bbox)
        {
            return this.NavigationGraph?.AddObstacle(bbox) ?? -1;
        }
        /// <summary>
        /// Adds OBB obstacle
        /// </summary>
        /// <param name="obb">OBB</param>
        /// <returns>Returns the obstacle Id</returns>
        public virtual int AddObstacle(OrientedBoundingBox obb)
        {
            return this.NavigationGraph?.AddObstacle(obb) ?? -1;
        }
        /// <summary>
        /// Removes obstable by id
        /// </summary>
        /// <param name="obstacle">Obstacle id</param>
        public virtual void RemoveObstacle(int obstacle)
        {
            this.NavigationGraph?.RemoveObstacle(obstacle);
        }

        /// <summary>
        /// Updates the graph at position
        /// </summary>
        /// <param name="position">Position</param>
        public virtual async void UpdateGraph(Vector3 position)
        {
            await this.PathFinderDescription?.Input.Refresh();

            this.NavigationGraph?.UpdateAt(position);
        }
        /// <summary>
        /// Updates the graph at positions in the specified list
        /// </summary>
        /// <param name="positions">Positions list</param>
        public virtual async void UpdateGraph(IEnumerable<Vector3> positions)
        {
            if (positions?.Any() == true)
            {
                await this.PathFinderDescription?.Input.Refresh();

                this.NavigationGraph?.UpdateAt(positions);
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
            var bbox = this.GetGroundBoundingBox();
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

                if (this.FindTopGroundPosition(v.X, v.Z, out PickingResult<Triangle> r))
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

                if (this.FindTopGroundPosition(v.X, v.Z, out PickingResult<Triangle> r))
                {
                    return r.Position + offset;
                }
            }
        }
    }
}
