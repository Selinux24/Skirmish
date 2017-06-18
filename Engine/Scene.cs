using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;
    using Engine.Content;
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
        private static List<Tuple<IRayPickable<Triangle>, float>> PickCoarse(ref Ray ray, float maxDistance, IEnumerable<SceneObject> list)
        {
            List<Tuple<IRayPickable<Triangle>, float>> coarse = new List<Tuple<IRayPickable<Triangle>, float>>();

            foreach (var gObj in list)
            {
                if (gObj.Is<IComposed>())
                {
                    var components = gObj.Get<IComposed>().GetComponents<IRayPickable<Triangle>>();
                    foreach (var pickable in components)
                    {
                        float d;
                        if (TestCoarse(ref ray, pickable, maxDistance, out d))
                        {
                            coarse.Add(new Tuple<IRayPickable<Triangle>, float>(pickable, d));
                        }
                    }
                }
                else if (gObj.Is<IRayPickable<Triangle>>())
                {
                    var pickable = gObj.Get<IRayPickable<Triangle>>();

                    float d;
                    if (TestCoarse(ref ray, pickable, maxDistance, out d))
                    {
                        coarse.Add(new Tuple<IRayPickable<Triangle>, float>(pickable, d));
                    }
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
            float d;
            if (Collision.RayIntersectsSphere(ref ray, ref bsph, out d))
            {
                if (d <= maxDistance)
                {
                    distance = d;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Camera
        /// </summary>
        private Camera camera = null;
        /// <summary>
        /// Scene world matrix
        /// </summary>
        private Matrix world = Matrix.Identity;
        /// <summary>
        /// Scene component list
        /// </summary>
        private List<SceneObject> components = new List<SceneObject>();
        /// <summary>
        /// Control captured with mouse
        /// </summary>
        /// <remarks>When mouse was pressed, the control beneath him was stored here. When mouse is released, if it is above this control, an click event occurs</remarks>
        private IControl capturedControl = null;
        /// <summary>
        /// Scene mode
        /// </summary>
        private SceneModesEnum sceneMode = SceneModesEnum.Unknown;
        /// <summary>
        /// Graph used for pathfinding
        /// </summary>
        protected IGraph navigationGraph = null;

        /// <summary>
        /// Game class
        /// </summary>
        protected Game Game { get; private set; }
        /// <summary>
        /// Graphics Device
        /// </summary>
        protected Device Device
        {
            get
            {
                return this.Game.Graphics.Device;
            }
        }
        /// <summary>
        /// Graphics Context
        /// </summary>
        protected DeviceContext DeviceContext
        {
            get
            {
                return this.Game.Graphics.DeviceContext;
            }
        }
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
        /// Buffer manager
        /// </summary>
        protected BufferManager BufferManager = null;

        /// <summary>
        /// Gets the scen world matrix
        /// </summary>
        public Matrix World
        {
            get
            {
                return this.world;
            }
            set
            {
                this.world = value;
            }
        }
        /// <summary>
        /// Camera
        /// </summary>
        public Camera Camera
        {
            get
            {
                return this.camera;
            }
            protected set
            {
                this.camera = value;
            }
        }
        /// <summary>
        /// Time of day controller
        /// </summary>
        public TimeOfDay TimeOfDay { get; set; }
        /// <summary>
        /// Indicates whether the current scene is active
        /// </summary>
        public bool Active { get; set; }
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
        /// Scene render mode
        /// </summary>
        public SceneModesEnum RenderMode
        {
            get
            {
                return this.sceneMode;
            }
            set
            {
                if (this.sceneMode != value)
                {
                    this.sceneMode = value;

                    this.SetRenderMode();
                }
            }
        }
        /// <summary>
        /// Path finder
        /// </summary>
        public PathFinderDescription PathFinderDescription = new PathFinderDescription();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene(Game game, SceneModesEnum sceneMode = SceneModesEnum.ForwardLigthning)
        {
            this.Game = game;

            this.Game.Graphics.Resized += new EventHandler(Resized);

            this.BufferManager = new BufferManager(game);

            this.TimeOfDay = new TimeOfDay();

            this.camera = Camera.CreateFree(
                new Vector3(0.0f, 0.0f, -10.0f),
                Vector3.Zero);

            this.camera.SetLens(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight);

            this.Lights = SceneLights.Default;

            this.PerformFrustumCulling = true;

            this.RenderMode = sceneMode;

            this.UpdateGlobalResources = true;
        }

        /// <summary>
        /// Initialize scene objects
        /// </summary>
        public virtual void Initialize()
        {

        }
        /// <summary>
        /// Scene objects initialized
        /// </summary>
        public virtual void Initialized()
        {
            this.UpdateNavigationGraph();
        }
        /// <summary>
        /// Generates scene resources
        /// </summary>
        public virtual void SetResources()
        {
            this.BufferManager.CreateBuffers();
        }
        /// <summary>
        /// Update scene objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            if (this.UpdateGlobalResources)
            {
                this.UpdateGlobals();

                this.UpdateGlobalResources = false;
            }

            this.camera.Update(gameTime);

            this.TimeOfDay.Update(gameTime);

            this.Lights.UpdateLights(this.TimeOfDay);

            this.Renderer.Update(gameTime, this);

            this.CapturedControl = this.capturedControl != null;

            //Process 2D controls
            var ctrls = this.components.FindAll(c => c.Active && c.Is<IControl>());
            for (int i = 0; i < ctrls.Count; i++)
            {
                var ctrl = ctrls[i].Get<IControl>();

                ctrl.MouseOver = ctrl.Rectangle.Contains(this.Game.Input.MouseX, this.Game.Input.MouseY);

                if (this.Game.Input.LeftMouseButtonJustPressed)
                {
                    if (ctrl.MouseOver)
                    {
                        this.capturedControl = ctrl;
                    }
                }

                if (this.Game.Input.LeftMouseButtonJustReleased)
                {
                    if (this.capturedControl == ctrl && ctrl.MouseOver)
                    {
                        ctrl.FireOnClickEvent();
                    }
                }

                ctrl.Pressed = this.Game.Input.LeftMouseButtonPressed && this.capturedControl == ctrl;
            }

            if (!this.Game.Input.LeftMouseButtonPressed) this.capturedControl = null;
        }

        /// <summary>
        /// Draw scene objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Draw(GameTime gameTime)
        {
            this.BufferManager.SetVertexBuffers();

            this.Renderer.Draw(gameTime, this);
        }
        /// <summary>
        /// Dispose scene objects
        /// </summary>
        public virtual void Dispose()
        {
            Helper.Dispose(this.Renderer);
            Helper.Dispose(this.camera);
            Helper.Dispose(this.components);
        }
        /// <summary>
        /// Change renderer mode
        /// </summary>
        private void SetRenderMode()
        {
            Helper.Dispose(this.Renderer);

            Counters.ClearAll();

            if (this.sceneMode == SceneModesEnum.ForwardLigthning)
            {
                this.Renderer = new SceneRendererForward(this.Game);
            }
            else if (this.sceneMode == SceneModesEnum.DeferredLightning)
            {
                this.Renderer = new SceneRendererDeferred(this.Game);
            }
        }

        /// <summary>
        /// Fires when render window resized
        /// </summary>
        /// <param name="sender">Graphis device</param>
        /// <param name="e">Event arguments</param>
        protected virtual void Resized(object sender, EventArgs e)
        {
            if (this.Renderer != null)
            {
                this.Renderer.Resize();
            }

            for (int i = 0; i < this.components.Count; i++)
            {
                var fitted = this.components[i].Get<IScreenFitted>();
                if (fitted != null)
                {
                    fitted.Resize();
                }
            }
        }
        /// <summary>
        /// Adds new model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="modelContentFile">Model content file name</param>
        /// <param name="description">Model description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Model> AddModel(string contentFolder, string modelContentFile, ModelDescription description, bool optimize = true, int order = 0)
        {
            var content = Helper.DeserializeFromFile<ModelContentDescription>(Path.Combine(contentFolder, modelContentFile));

            return this.AddModel(contentFolder, content, description, optimize, order);
        }
        /// <summary>
        /// Adds new model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Model content description</param>
        /// <param name="description">Model description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Model> AddModel(string contentFolder, ModelContentDescription content, ModelDescription description, bool optimize = true, int order = 0)
        {
            Model newModel = null;

            ModelContent[] geo = LoaderCOLLADA.Load(contentFolder, content);
            if (geo.Length == 1)
            {
                if (optimize) geo[0].Optimize();

                newModel = new Model(this.Game, this.BufferManager, geo[0], description);
            }
            else
            {
                var lod = new LODModelContent(geo, optimize);

                newModel = new Model(this.Game, this.BufferManager, lod, description);
            }

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new model
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="description">Model description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Model> AddModel(ModelContent content, ModelDescription description, int order = 0)
        {
            var newModel = new Model(this.Game, this.BufferManager, content, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new instanced model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="modelContentFile">Model content file name</param>
        /// <param name="description">Model description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<ModelInstanced> AddInstancingModel(string contentFolder, string modelContentFile, ModelInstancedDescription description, bool optimize = true, int order = 0)
        {
            var content = Helper.DeserializeFromFile<ModelContentDescription>(Path.Combine(contentFolder, modelContentFile));

            return this.AddInstancingModel(contentFolder, content, description, optimize, order);
        }
        /// <summary>
        /// Adds new instanced model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Model content description</param>
        /// <param name="description">Model description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<ModelInstanced> AddInstancingModel(string contentFolder, ModelContentDescription content, ModelInstancedDescription description, bool optimize = true, int order = 0)
        {
            ModelInstanced newModel = null;

            ModelContent[] geo = LoaderCOLLADA.Load(contentFolder, content);
            if (geo.Length == 1)
            {
                if (optimize) geo[0].Optimize();

                newModel = new ModelInstanced(this.Game, this.BufferManager, geo[0], description);
            }
            else
            {
                var lod = new LODModelContent(geo, optimize);

                newModel = new ModelInstanced(this.Game, this.BufferManager, lod, description);
            }

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new instanced model
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="description">Model description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<ModelInstanced> AddInstancingModel(ModelContent content, ModelInstancedDescription description, int order = 0)
        {
            var newModel = new ModelInstanced(this.Game, this.BufferManager, content, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="modelContentFile">Model content file name</param>
        /// <param name="description">Terrain description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Scenery> AddScenery(string contentFolder, string modelContentFile, GroundDescription description, bool optimize = true, int order = 0)
        {
            var content = Helper.DeserializeFromFile<ModelContentDescription>(Path.Combine(contentFolder, modelContentFile));

            return this.AddScenery(contentFolder, content, description, optimize, order);
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Content</param>
        /// <param name="description">Terrain description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Scenery> AddScenery(string contentFolder, ModelContentDescription content, GroundDescription description, bool optimize = true, int order = 0)
        {
            var t = LoaderCOLLADA.Load(contentFolder, content);
            ModelContent geo = t[0];

            if (optimize) geo.Optimize();

            return AddScenery(geo, description, order);
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="description">Terrain description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Scenery> AddScenery(HeightmapDescription content, GroundDescription description, bool optimize = true, int order = 0)
        {
            ModelContent geo = ModelContent.FromHeightmap(
                content.ContentPath,
                content.HeightmapFileName,
                content.Textures.TexturesLR,
                content.CellSize,
                content.MaximumHeight);

            if (optimize) geo.Optimize();

            return AddScenery(geo, description, order);
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="description">Terrain description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Scenery> AddScenery(ModelContent content, GroundDescription description, int order = 0)
        {
            var newModel = new Scenery(this.Game, this.BufferManager, content, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="content">Content description</param>
        /// <param name="description">Terrain description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Terrain> AddTerrain(HeightmapDescription content, GroundDescription description, bool optimize = true, int order = 0)
        {
            var newModel = new Terrain(this.Game, this.BufferManager, content, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new gardener
        /// </summary>
        /// <param name="description">Gardener description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<GroundGardener> AddGardener(GroundGardenerDescription description, int order = 0)
        {
            var newModel = new GroundGardener(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new mini-map
        /// </summary>
        /// <param name="description">Mini-map description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new mini-map</returns>
        public SceneObject<Minimap> AddMinimap(MinimapDescription description, int order = 0)
        {
            var newModel = new Minimap(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new cubemap
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Cubemap> AddCubemap(CubemapDescription description, int order = 0)
        {
            var newModel = new Cubemap(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new skydom
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Skydom> AddSkydom(SkydomDescription description)
        {
            var newModel = new Skydom(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, -1);
        }
        /// <summary>
        /// Adds new sky scattering component
        /// </summary>
        /// <param name="description">Description</param>
        /// <returns>Returns new model</returns>
        public SceneObject<SkyScattering> AddSkyScattering(SkyScatteringDescription description)
        {
            var newModel = new SkyScattering(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, -1);
        }
        /// <summary>
        /// Adds new sky plane component
        /// </summary>
        /// <param name="description">Description</param>
        /// <returns>Returns new model</returns>
        public SceneObject<SkyPlane> AddSkyPlane(SkyPlaneDescription description)
        {
            var newModel = new SkyPlane(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, -1);
        }
        /// <summary>
        /// Adds new background sprite
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="order">Order</param>
        /// <returns>Return new model</returns>
        public SceneObject<Sprite> AddBackgroud(SpriteBackgroundDescription description, int order = 0)
        {
            var newModel = new Sprite(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new sprite
        /// </summary>
        /// <param name="description">Sprite description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Sprite> AddSprite(SpriteDescription description, int order = 0)
        {
            var newModel = new Sprite(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new sprite texture
        /// </summary>
        /// <param name="description">Sprite texture description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<SpriteTexture> AddSpriteTexture(SpriteTextureDescription description, int order = 0)
        {
            var newModel = new SpriteTexture(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new sprite button
        /// </summary>
        /// <param name="description">Sprite button description</param>
        /// <param name="textureOff">Texture when button off</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<SpriteButton> AddSpriteButton(SpriteButtonDescription description, int order = 0)
        {
            var newModel = new SpriteButton(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds new game cursor
        /// </summary>
        /// <param name="description">Sprite description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SceneObject<Cursor> AddCursor(SpriteDescription description, int order = 0)
        {
            var newModel = new Cursor(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds text
        /// </summary>
        /// <param name="description">Text description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new text</returns>
        public SceneObject<TextDrawer> AddText(TextDrawerDescription description, int order = 0)
        {
            var newModel = new TextDrawer(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds a new particle manager
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new particle manager</returns>
        public SceneObject<ParticleManager> AddParticleManager(ParticleManagerDescription description, int order = 0)
        {
            var newModel = new ParticleManager(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds a line list drawer
        /// </summary>
        /// <param name="description">Line drawer description</param>
        /// <param name="count">Line count</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new line list drawer</returns>
        public SceneObject<LineListDrawer> AddLineListDrawer(LineListDrawerDescription description, int count, int order = 0)
        {
            var newModel = new LineListDrawer(this.Game, this.BufferManager, description, count);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds a line list drawer
        /// </summary>
        /// <param name="description">Line drawer description</param>
        /// <param name="lines">Line list</param>
        /// <param name="color">Color</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new line list drawer</returns>
        public SceneObject<LineListDrawer> AddLineListDrawer(LineListDrawerDescription description, Line3D[] lines, Color4 color, int order = 0)
        {
            var newModel = new LineListDrawer(this.Game, this.BufferManager, description, lines, color);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds a line list drawer
        /// </summary>
        /// <param name="description">Line drawer description</param>
        /// <param name="triangles">Triangles list</param>
        /// <param name="color">Color</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new line list drawer</returns>
        public SceneObject<LineListDrawer> AddLineListDrawer(LineListDrawerDescription description, Triangle[] triangles, Color4 color, int order = 0)
        {
            var newModel = new LineListDrawer(this.Game, this.BufferManager, description, triangles, color);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds a triangle list drawer
        /// </summary>
        /// <param name="description">Triangle drawer description</param>
        /// <param name="count">Triangle count</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new triangle list drawer</returns>
        public SceneObject<TriangleListDrawer> AddTriangleListDrawer(TriangleListDrawerDescription description, int count, int order = 0)
        {
            var newModel = new TriangleListDrawer(this.Game, this.BufferManager, description, count);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds a triangle list drawer
        /// </summary>
        /// <param name="description">Triangle drawer description</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="color">Color</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new triangle list drawer</returns>
        public SceneObject<TriangleListDrawer> AddTriangleListDrawer(TriangleListDrawerDescription description, Triangle[] triangles, Color4 color, int order = 0)
        {
            var newModel = new TriangleListDrawer(this.Game, this.BufferManager, description, triangles, color);

            return this.AddComponent(newModel, description, order);
        }
        /// <summary>
        /// Adds a lens flare drawer
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new lens flare drawer</returns>
        public SceneObject<LensFlare> AddLensFlare(LensFlareDescription description, int order = 0)
        {
            var newModel = new LensFlare(this.Game, this.BufferManager, description);

            return this.AddComponent(newModel, description, order);
        }

        /// <summary>
        /// Add component to collection
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="order">Processing order</param>
        public SceneObject<T> AddComponent<T>(SceneObject<T> component, int order)
        {
            if (!this.components.Contains(component))
            {
                if (order != 0)
                {
                    component.Order = order;
                }

                this.components.Add(component);
                this.components.Sort((p1, p2) =>
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

                this.UpdateGlobalResources = true;
            }

            return component;
        }

        public SceneObject<T> AddComponent<T>(T component, SceneObjectDescription description, int order = 0)
        {
            var sceneObject = new SceneObject<T>(component, description);

            this.AddComponent(sceneObject, 0);

            return sceneObject;
        }
        /// <summary>
        /// Remove and dispose component
        /// </summary>
        /// <param name="component">Component</param>
        public void RemoveComponent(SceneObject component)
        {
            if (this.components.Contains(component))
            {
                this.components.Remove(component);

                this.UpdateGlobalResources = true;
            }

            component.Dispose();
            component = null;
        }

        public ReadOnlyCollection<SceneObject> GetComponents()
        {
            return new ReadOnlyCollection<SceneObject>(this.components);
        }

        public ReadOnlyCollection<SceneObject> GetComponents(Func<SceneObject, bool> func)
        {
            if (func != null)
            {
                return new ReadOnlyCollection<SceneObject>(this.components.FindAll(c => func(c)));
            }
            else
            {
                return new ReadOnlyCollection<SceneObject>(this.components);
            }
        }

        public ReadOnlyCollection<T> GetComponents<T>()
        {
            List<T> res = new List<T>();

            for (int i = 0; i < this.components.Count; i++)
            {
                if (this.components[i] is T)
                {
                    res.Add((T)(object)this.components[i]);
                }
            }

            return new ReadOnlyCollection<T>(res);
        }

        public ReadOnlyCollection<T> GetComponents<T>(Func<SceneObject, bool> func)
        {
            List<T> res = new List<T>();

            for (int i = 0; i < this.components.Count; i++)
            {
                if (func == null || func(this.components[i]))
                {
                    if (this.components[i].Is<T>())
                    {
                        res.Add(this.components[i].Get<T>());
                    }
                }
            }

            return new ReadOnlyCollection<T>(res);
        }

        /// <summary>
        /// Update global resources
        /// </summary>
        protected virtual void UpdateGlobals()
        {
            ShaderResourceView materialPalette;
            uint materialPaletteWidth;
            this.UpdateMaterialPalette(out materialPalette, out materialPaletteWidth);

            ShaderResourceView animationPalette;
            uint animationPaletteWidth;
            this.UpdateAnimationPalette(out animationPalette, out animationPaletteWidth);

            DrawerPool.UpdateSceneGlobals(materialPalette, materialPaletteWidth, animationPalette, animationPaletteWidth);
        }
        /// <summary>
        /// Updates the global material palette
        /// </summary>
        /// <param name="materialPalette">Material palette</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        private void UpdateMaterialPalette(out ShaderResourceView materialPalette, out uint materialPaletteWidth)
        {
            List<MeshMaterial> mats = new List<MeshMaterial>();

            mats.Add(MeshMaterial.Default);

            var matComponents = this.components.FindAll(c => c.Is<UseMaterials>());

            foreach (var component in matComponents)
            {
                var matList = component.Get<UseMaterials>().Materials;
                if (matList != null && matList.Length > 0)
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

            int texWidth = Helper.GetTextureSize(values.Count);

            materialPalette = this.Game.ResourceManager.CreateGlobalResourceTexture2D("MaterialPalette", values.ToArray(), texWidth);
            materialPaletteWidth = (uint)texWidth;
        }
        /// <summary>
        /// Updates the global animation palette
        /// </summary>
        /// <param name="animationPalette">Animation palette</param>
        /// <param name="animationPaletteWidth">Animation palette width</param>
        private void UpdateAnimationPalette(out ShaderResourceView animationPalette, out uint animationPaletteWidth)
        {
            List<SkinningData> skData = new List<SkinningData>();

            var skComponents = this.components.FindAll(c => c.Is<UseSkinningData>());

            foreach (var component in skComponents)
            {
                var skList = component.Get<UseSkinningData>().SkinningData;
                if (skList != null && skList.Length > 0)
                {
                    skData.AddRange(skList);
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

            int texWidth = Helper.GetTextureSize(values.Count);

            animationPalette = this.Game.ResourceManager.CreateGlobalResourceTexture2D("AnimationPalette", values.ToArray(), texWidth);
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
            Matrix worldViewProjection = this.world * this.camera.View * this.camera.Projection;
            float nDistance = this.camera.NearPlaneDistance;
            float fDistance = this.camera.FarPlaneDistance;
            ViewportF viewport = this.Game.Graphics.Viewport;

            Vector3 nVector = new Vector3(mouseX, mouseY, nDistance);
            Vector3 fVector = new Vector3(mouseX, mouseY, fDistance);

            Vector3 nPoint = Vector3.Unproject(nVector, 0, 0, viewport.Width, viewport.Height, nDistance, fDistance, worldViewProjection);
            Vector3 fPoint = Vector3.Unproject(fVector, 0, 0, viewport.Width, viewport.Height, nDistance, fDistance, worldViewProjection);

            return new Ray(nPoint, Vector3.Normalize(fPoint - nPoint));
        }
        /// <summary>
        /// Gets the nearest pickable object in the ray path
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="maxDistance">Maximum distance for test</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="model">Gets the resulting ray pickable object</param>
        /// <returns>Returns true if a pickable object in the ray path was found</returns>
        public virtual bool PickNearest(ref Ray ray, float maxDistance, bool facingOnly, out IRayPickable<Triangle> model)
        {
            model = null;

            var usage = SceneObjectUsageEnum.Agent &
                SceneObjectUsageEnum.CoarsePathFinding &
                SceneObjectUsageEnum.FullPathFinding;

            var cmpList = this.components.FindAll(c => c.Usage.HasFlag(usage));

            var coarse = PickCoarse(ref ray, maxDistance, cmpList);

            foreach (var obj in coarse)
            {
                Vector3 p;
                Triangle t;
                float d;
                if (obj.Item1.PickNearest(ref ray, facingOnly, out p, out t, out d))
                {
                    model = obj.Item1;

                    return true;
                }
            }

            return false;
        }

        public bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle item, out float distance)
        {
            return PickNearest(ref ray, facingOnly, SceneObjectUsageEnum.None, out position, out item, out distance);
        }

        public bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle item, out float distance)
        {
            return PickFirst(ref ray, facingOnly, SceneObjectUsageEnum.None, out position, out item, out distance);
        }

        public bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] item, out float[] distances)
        {
            return PickAll(ref ray, facingOnly, SceneObjectUsageEnum.None, out positions, out item, out distances);
        }

        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="position">Ground position if exists</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindTopGroundPosition(float x, float z, out Vector3 position, out Triangle triangle, out float distance)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.1f, z),
                Direction = Vector3.Down,
            };

            var usage = SceneObjectUsageEnum.CoarsePathFinding | SceneObjectUsageEnum.FullPathFinding;

            return this.PickNearest(ref ray, true, usage, out position, out triangle, out distance);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindFirstGroundPosition(float x, float z, out Vector3 position, out Triangle triangle, out float distance)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.1f, z),
                Direction = Vector3.Down,
            };

            var usage = SceneObjectUsageEnum.CoarsePathFinding & SceneObjectUsageEnum.FullPathFinding;

            return this.PickFirst(ref ray, true, usage, out position, out triangle, out distance);
        }
        /// <summary>
        /// Gets all ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <param name="triangles">Triangles found</param>
        /// <param name="distances">Distances to positions</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition(float x, float z, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.01f, z),
                Direction = Vector3.Down,
            };

            var usage = SceneObjectUsageEnum.CoarsePathFinding & SceneObjectUsageEnum.FullPathFinding;

            return this.PickAll(ref ray, true, usage, out positions, out triangles, out distances);
        }
        /// <summary>
        /// Gets nearest ground position to "from" position
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindNearestGroundPosition(Vector3 from, out Vector3 position, out Triangle triangle, out float distance)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(from.X, bbox.Maximum.Y + 0.01f, from.Z),
                Direction = Vector3.Down,
            };

            var usage = SceneObjectUsageEnum.CoarsePathFinding & SceneObjectUsageEnum.FullPathFinding;

            Vector3[] pArray;
            Triangle[] tArray;
            float[] dArray;
            if (this.PickAll(ref ray, true, usage, out pArray, out tArray, out dArray))
            {
                int index = -1;
                float dist = float.MaxValue;
                for (int i = 0; i < pArray.Length; i++)
                {
                    float d = Vector3.DistanceSquared(from, pArray[i]);
                    if (d <= dist)
                    {
                        dist = d;

                        index = i;
                    }
                }

                position = pArray[index];
                triangle = tArray[index];
                distance = dArray[index];

                return true;
            }
            else
            {
                position = Vector3.Zero;
                triangle = new Triangle();
                distance = float.MaxValue;

                return false;
            }
        }

        private bool PickNearest(ref Ray ray, bool facingOnly, SceneObjectUsageEnum usage, out Vector3 position, out Triangle item, out float distance)
        {
            position = Vector3.Zero;
            item = new Triangle();
            distance = float.MaxValue;

            var cmpList = usage == SceneObjectUsageEnum.None ?
                this.components :
                this.components.FindAll(c => (c.Usage & usage) != SceneObjectUsageEnum.None);

            var coarse = PickCoarse(ref ray, float.MaxValue, cmpList);

            bool picked = false;
            float bestDistance = float.MaxValue;

            foreach (var obj in coarse)
            {
                if (obj.Item2 > bestDistance)
                {
                    break;
                }

                Vector3 p;
                Triangle t;
                float d;
                if (obj.Item1.PickNearest(ref ray, facingOnly, out p, out t, out d))
                {
                    if (d < bestDistance)
                    {
                        bestDistance = d;

                        position = p;
                        item = t;
                        distance = d;
                    }

                    picked = true;
                }
            }

            return picked;
        }

        private bool PickFirst(ref Ray ray, bool facingOnly, SceneObjectUsageEnum usage, out Vector3 position, out Triangle item, out float distance)
        {
            position = Vector3.Zero;
            item = new Triangle();
            distance = float.MaxValue;

            var cmpList = usage == SceneObjectUsageEnum.None ?
                this.components :
                this.components.FindAll(c => c.Usage.HasFlag(usage));

            var coarse = PickCoarse(ref ray, float.MaxValue, cmpList);

            foreach (var obj in coarse)
            {
                Vector3 p;
                Triangle t;
                float d;
                if (obj.Item1.PickFirst(ref ray, facingOnly, out p, out t, out d))
                {
                    position = p;
                    item = t;
                    distance = d;

                    return true;
                }
            }

            return false;
        }

        private bool PickAll(ref Ray ray, bool facingOnly, SceneObjectUsageEnum usage, out Vector3[] positions, out Triangle[] item, out float[] distances)
        {
            positions = null;
            item = null;
            distances = null;

            var cmpList = usage == SceneObjectUsageEnum.None ?
                this.components :
                this.components.FindAll(c => c.Usage.HasFlag(usage));

            var coarse = PickCoarse(ref ray, float.MaxValue, cmpList);

            List<Vector3> lPositions = new List<Vector3>();
            List<Triangle> lTriangles = new List<Triangle>();
            List<float> lDistances = new List<float>();

            foreach (var obj in coarse)
            {
                Vector3[] p;
                Triangle[] t;
                float[] d;
                if (obj.Item1.PickAll(ref ray, facingOnly, out p, out t, out d))
                {
                    lPositions.AddRange(p);
                    lTriangles.AddRange(t);
                    lDistances.AddRange(d);
                }
            }

            positions = lPositions.ToArray();
            item = lTriangles.ToArray();
            distances = lDistances.ToArray();

            return lPositions.Count > 0;
        }




        public void SetGround<T>(SceneObject<T> obj, bool fullGeometryPathFinding)
        {
            obj.Usage |= (fullGeometryPathFinding ? SceneObjectUsageEnum.FullPathFinding : SceneObjectUsageEnum.CoarsePathFinding);
        }

        public void AttachToGround<T>(SceneObject<T> obj, float x, float z, Matrix transform, bool fullGeometryPathFinding)
        {
            Vector3 pos;
            Triangle tri;
            float dist;
            if (this.FindTopGroundPosition(x, z, out pos, out tri, out dist))
            {
                obj.Transform.LocalTransform = transform;
                obj.Transform.SetPosition(pos);
            }

            obj.Usage |= (fullGeometryPathFinding ? SceneObjectUsageEnum.FullPathFinding : SceneObjectUsageEnum.CoarsePathFinding);
        }




        public void UpdateNavigationGraph()
        {
            if (this.PathFinderDescription != null)
            {
                var gTriangles = this.GetTrianglesForNavigationGraph();

                this.boundingBox = GeometryUtil.CreateBoundingBox(gTriangles);
                this.boundingSphere = GeometryUtil.CreateBoundingSphere(gTriangles);

                this.navigationGraph = PathFinder.Build(this.PathFinderDescription.Settings, gTriangles);
            }
        }

        /// <summary>
        /// Gets the objects triangle list for navigation graph construction
        /// </summary>
        /// <returns>Returns a triangle list</returns>
        protected Triangle[] GetTrianglesForNavigationGraph()
        {
            List<Triangle> tris = new List<Triangle>();

            var pfComponents = this.components.FindAll(c => c.Usage.HasFlag(SceneObjectUsageEnum.FullPathFinding) || c.Usage.HasFlag(SceneObjectUsageEnum.CoarsePathFinding));

            for (int i = 0; i < pfComponents.Count; i++)
            {
                var curr = pfComponents[i];

                List<IVolume> volumes = new List<IVolume>();

                bool isComposed = curr.Is<IComposed>();
                if (!isComposed)
                {
                    var trn = curr.Get<ITransformable3D>();
                    if (trn != null)
                    {
                        trn.Manipulator.UpdateInternals(true);
                    }

                    var pickable = curr.Get<IVolume>();
                    if (pickable != null)
                    {
                        volumes.Add(pickable);
                    }
                }
                else
                {
                    var trnChilds = curr.Get<IComposed>().GetComponents<ITransformable3D>();
                    foreach (var child in trnChilds)
                    {
                        child.Manipulator.UpdateInternals(true);
                    }

                    var pickableChilds = curr.Get<IComposed>().GetComponents<IVolume>();
                    volumes.AddRange(pickableChilds);
                }

                for (int p = 0; p < volumes.Count; p++)
                {
                    var full = curr.Usage.HasFlag(SceneObjectUsageEnum.FullPathFinding);

                    var vTris = volumes[p].GetVolume(full);
                    if (vTris != null && vTris.Length > 0)
                    {
                        //Use volume mesh
                        tris.AddRange(vTris);
                    }
                }
            }

            return tris.ToArray();
        }

        /// <summary>
        /// Gets the path finder grid nodes
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns the path finder grid nodes</returns>
        public IGraphNode[] GetNodes(AgentType agent)
        {
            IGraphNode[] nodes = null;

            if (this.navigationGraph != null)
            {
                nodes = this.navigationGraph.GetNodes(agent);
            }

            return nodes;
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
        public virtual PathFindingPath FindPath(AgentType agent, Vector3 from, Vector3 to, bool useGround = true, float delta = 0f)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();

            var path = this.navigationGraph.FindPath(agent, from, to);
            if (path != null && path.Length > 1)
            {
                if (delta == 0)
                {
                    positions.AddRange(path);
                    normals.AddRange(Helper.CreateArray(path.Length, new Vector3(0, 1, 0)));
                }
                else
                {
                    positions.Add(path[0]);
                    normals.Add(Vector3.Up);

                    var p0 = path[0];
                    var p1 = path[1];

                    int index = 0;
                    while (index < path.Length - 1)
                    {
                        var s = p1 - p0;
                        var v = Vector3.Normalize(s) * delta;
                        var l = delta - s.Length();

                        if (l <= 0f)
                        {
                            //Into de segment
                            p0 += v;
                        }
                        else if (index < path.Length - 2)
                        {
                            //Next segment
                            var p2 = path[index + 2];
                            p0 = p1 + ((p2 - p1) * l);
                            p1 = p2;

                            index++;
                        }
                        else
                        {
                            //End
                            p0 = path[index + 1];

                            index++;
                        }

                        positions.Add(p0);
                        normals.Add(Vector3.Up);
                    }
                }
            }

            if (useGround)
            {
                for (int i = 0; i < positions.Count; i++)
                {
                    Vector3 position;
                    Triangle triangle;
                    float distance;
                    if (FindNearestGroundPosition(positions[i], out position, out triangle, out distance))
                    {
                        positions[i] = position;
                        normals[i] = triangle.Normal;
                    }
                }
            }

            return new PathFindingPath(positions.ToArray(), normals.ToArray());
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
            if (this.navigationGraph != null)
            {
                return this.navigationGraph.IsWalkable(agent, position, out nearest);
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
        /// <param name="finalPosition">Returns the final position if exists</param>
        /// <returns>Returns true if final position found</returns>
        public virtual bool Walk(AgentType agent, Vector3 prevPosition, Vector3 newPosition, out Vector3 finalPosition)
        {
            finalPosition = Vector3.Zero;

            Vector3 walkerPos;
            Triangle t;
            float d;
            if (this.FindNearestGroundPosition(newPosition, out walkerPos, out t, out d))
            {
                Vector3? nearest;
                if (this.IsWalkable(agent, walkerPos, out nearest))
                {
                    finalPosition = walkerPos;
                    finalPosition.Y += agent.Height;

                    var moveP = newPosition - prevPosition;
                    var moveV = finalPosition - prevPosition;
                    if (moveV.LengthSquared() > moveP.LengthSquared())
                    {
                        finalPosition = prevPosition + (Vector3.Normalize(moveV) * moveP.Length());
                    }

                    return true;
                }
                else
                {
                    //Not walkable but nearest position found
                    if (nearest.HasValue)
                    {
                        //Adjust height
                        var p = nearest.Value;
                        p.Y = prevPosition.Y;

                        if (this.FindNearestGroundPosition(p, out walkerPos, out t, out d))
                        {
                            finalPosition = walkerPos;
                            finalPosition.Y += agent.Height;

                            var moveP = newPosition - prevPosition;
                            var moveV = finalPosition - prevPosition;
                            if (moveV.LengthSquared() > moveP.LengthSquared())
                            {
                                finalPosition = prevPosition + (Vector3.Normalize(moveV) * moveP.Length());
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }




        private BoundingBox boundingBox;
        private BoundingSphere boundingSphere;

        public BoundingSphere GetBoundingSphere()
        {
            return this.boundingSphere;
        }

        public BoundingBox GetBoundingBox()
        {
            return this.boundingBox;
        }
    }
}
