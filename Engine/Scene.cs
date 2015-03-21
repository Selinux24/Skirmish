using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Render scene
    /// </summary>
    public class Scene
    {
        /// <summary>
        /// Scene world matrix
        /// </summary>
        private Matrix world = Matrix.Identity;
        /// <summary>
        /// Scene inverse world matrix
        /// </summary>
        private Matrix worldInverse = Matrix.Identity;
        /// <summary>
        /// Scene component list
        /// </summary>
        private List<Drawable> components = new List<Drawable>();
        /// <summary>
        /// Control captured with mouse
        /// </summary>
        /// <remarks>When mouse was pressed, the control beneath him was stored here. When mouse is released, if it is above this control, an click event occurs</remarks>
        private IControl capturedControl = null;
        /// <summary>
        /// Shadow mapper
        /// </summary>
        private ShadowMap shadowMap = null;

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
        /// Draw context
        /// </summary>
        protected Context DrawContext = new Context();
        /// <summary>
        /// Gets or sets whether the scene was handling control captures
        /// </summary>
        protected bool CapturedControl { get; private set; }
        /// <summary>
        /// Shadow map
        /// </summary>
        protected ShadowMap ShadowMap
        {
            get
            {
                if (this.shadowMap == null)
                {
                    this.shadowMap = new ShadowMap(this.Game, 2048, 2048);
                }

                return this.shadowMap;
            }
        }
        /// <summary>
        /// Context for shadow map drawing
        /// </summary>
        protected Context DrawShadowsContext = new Context();

        /// <summary>
        /// Indicates whether the current scene is active
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Processing order
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Camera
        /// </summary>
        public Camera Camera { get; protected set; }
        /// <summary>
        /// Scene lights
        /// </summary>
        public SceneLight Lights { get; private set; }
        /// <summary>
        /// Scene volume
        /// </summary>
        public BoundingSphere SceneVolume { get; protected set; }
        /// <summary>
        /// Gets or sets whether the scene has shadows activated
        /// </summary>
        public bool EnableShadows { get; set; }
        /// <summary>
        /// Gets or sets if scene has to perform frustum culling with objects
        /// </summary>
        public bool PerformFrustumCulling { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene(Game game)
        {
            this.Game = game;

            this.Game.Graphics.Resized += new EventHandler(Resized);

            this.Camera = Camera.CreateFree(
                new Vector3(0.0f, 0.0f, -10.0f),
                Vector3.Zero);

            this.Camera.SetLens(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight);

            this.Lights = new SceneLight();

            this.Lights.DirectionalLight1.Ambient = new Color4(0.8f, 0.8f, 0.8f, 1.0f);
            this.Lights.DirectionalLight1.Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            this.Lights.DirectionalLight1.Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            this.Lights.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0.57735f, -0.57735f, 0.57735f));
            this.Lights.DirectionalLight1Enabled = true;

            this.Lights.DirectionalLight2.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.DirectionalLight2.Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            this.Lights.DirectionalLight2.Specular = new Color4(0.25f, 0.25f, 0.25f, 1.0f);
            this.Lights.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(-0.57735f, -0.57735f, 0.57735f));
            this.Lights.DirectionalLight2Enabled = true;

            this.Lights.DirectionalLight3.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.DirectionalLight3.Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            this.Lights.DirectionalLight3.Specular = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.DirectionalLight3.Direction = Vector3.Normalize(new Vector3(0.0f, -0.707f, -0.707f));
            this.Lights.DirectionalLight3Enabled = true;

            this.Lights.PointLightEnabled = false;

            this.Lights.SpotLightEnabled = false;

            this.SceneVolume = new BoundingSphere(Vector3.Zero, 1000);
            this.EnableShadows = false;

            this.PerformFrustumCulling = true;
        }

        /// <summary>
        /// Initialize scene objects
        /// </summary>
        public virtual void Initialize()
        {

        }
        /// <summary>
        /// Update scene objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            this.Camera.Update(gameTime);

            if (this.EnableShadows)
            {
                this.ShadowMap.Update(this.Lights.DirectionalLight1.Direction, this.SceneVolume);

                this.DrawShadowsContext.DrawerMode = DrawerModesEnum.ShadowMap;
                this.DrawShadowsContext.World = Matrix.Identity;
                this.DrawShadowsContext.ViewProjection = this.ShadowMap.View * this.ShadowMap.Projection;
                this.DrawShadowsContext.ShadowMap = null;
            }

            this.DrawContext.DrawerMode = DrawerModesEnum.Default;
            this.DrawContext.World = this.world;
            this.DrawContext.ViewProjection = this.Camera.View * this.Camera.Projection;
            this.DrawContext.EyePosition = this.Camera.Position;
            this.DrawContext.Lights = this.Lights;
            this.DrawContext.ShadowMap = null;

            //Update active components
            List<Drawable> activeComponents = this.components.FindAll(c => c.Active);
            for (int i = 0; i < activeComponents.Count; i++)
            {
                activeComponents[i].Update(gameTime, this.DrawContext);
            }

            this.CapturedControl = this.capturedControl != null;

            //Process 2D controls
            List<Drawable> ctrls = this.components.FindAll(c => c.Active && c is IControl);
            for (int i = 0; i < ctrls.Count; i++)
            {
                IControl ctrl = (IControl)ctrls[i];

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
            //Draw visible components
            List<Drawable> visibleComponents = this.components.FindAll(c => c.Visible);
            if (visibleComponents.Count > 0)
            {
                if (this.EnableShadows)
                {
                    this.DrawContext.ShadowMap = null;

                    List<Drawable> shadowComponents = visibleComponents.FindAll(c => c.DropShadow);
                    if (shadowComponents.Count > 0)
                    {
                        this.Game.Graphics.SetRenderTarget(this.ShadowMap.Viewport, this.ShadowMap.DepthMap, null, true, Color.Silver);
                        this.DrawComponents(gameTime, this.DrawShadowsContext, shadowComponents.ToArray());
                        this.Game.Graphics.SetDefaultRenderTarget(false);

                        this.DrawContext.ShadowMap = this.ShadowMap.ShadowMapTexture;
                    }
                }

                this.DrawComponents(gameTime, this.DrawContext, visibleComponents.ToArray());
            }
        }
        /// <summary>
        /// Drawing of scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Drawing context</param>
        /// <param name="components">Components</param>
        protected virtual void DrawComponents(GameTime gameTime, Context context, Drawable[] components)
        {
            if (this.PerformFrustumCulling)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    components[i].FrustumCulling(this.Camera.Frustum);

                    if (!components[i].Cull)
                    {
                        this.Game.Graphics.SetDefaultRasterizer();
                        this.Game.Graphics.SetBlendAlphaToCoverage();

                        components[i].Draw(gameTime, context);
                    }
                }
            }
            else
            {
                for (int i = 0; i < components.Length; i++)
                {
                    this.Game.Graphics.SetDefaultRasterizer();
                    this.Game.Graphics.SetBlendAlphaToCoverage();

                    components[i].Draw(gameTime, context);
                }
            }
        }
        /// <summary>
        /// Dispose scene objects
        /// </summary>
        public virtual void Dispose()
        {
            if (this.Camera != null)
            {
                this.Camera.Dispose();
                this.Camera = null;
            }

            for (int i = 0; i < this.components.Count; i++)
            {
                this.components[i].Dispose();
            }

            this.components.Clear();
            this.components = null;
        }

        /// <summary>
        /// Fires when render window resized
        /// </summary>
        /// <param name="sender">Graphis device</param>
        /// <param name="e">Event arguments</param>
        protected virtual void Resized(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Adds new model
        /// </summary>
        /// <param name="description">Model description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Model AddModel(ModelDescription description, bool optimize = true, int order = 0)
        {
            return AddModel(description, Matrix.Identity, optimize, order);
        }
        /// <summary>
        /// Adds new model
        /// </summary>
        /// <param name="description">Model description</param>
        /// <param name="transform">Initial transform to apply to loaded geometry</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Model AddModel(ModelDescription description, Matrix transform, bool optimize = true, int order = 0)
        {
            ModelContent geo = LoaderCOLLADA.Load(description.ContentPath, description.ModelFileName, transform);

            if (optimize) geo.Optimize();

            Model newModel = new Model(this.Game, geo);

            newModel.DropShadow = description.DropShadow;
            newModel.TextureIndex = description.TextureIndex;

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new model
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Model AddModel(ModelContent content, int order = 0)
        {
            Model newModel = new Model(this.Game, content);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new instanced model
        /// </summary>
        /// <param name="description">Model description</param>
        /// <param name="instances">Number of instances for the model</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public ModelInstanced AddInstancingModel(ModelInstancedDescription description, bool optimize = true, int order = 0)
        {
            return AddInstancingModel(description, Matrix.Identity, optimize, order);
        }
        /// <summary>
        /// Adds new instanced model
        /// </summary>
        /// <param name="description">Model description</param>
        /// <param name="transform">Initial transform to apply to loaded geometry</param>
        /// <param name="instances">Number of instances for the model</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public ModelInstanced AddInstancingModel(ModelInstancedDescription description, Matrix transform, bool optimize = true, int order = 0)
        {
            ModelContent geo = LoaderCOLLADA.Load(description.ContentPath, description.ModelFileName, transform);

            if (optimize) geo.Optimize();

            ModelInstanced newModel = new ModelInstanced(this.Game, geo, description.Instances);

            newModel.DropShadow = description.DropShadow;

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new instanced model
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="instances">Number of instances for the model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public ModelInstanced AddInstancingModel(ModelContent content, int instances, int order = 0)
        {
            ModelInstanced newModel = new ModelInstanced(this.Game, content, instances);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="description">Terrain description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Terrain AddTerrain(TerrainDescription description, bool optimize = true, int order = 0)
        {
            return AddTerrain(description, Matrix.Identity, optimize, order);
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="transform">Initial transform to apply to loaded geometry</param>
        /// <param name="description">Terrain description</param>
        /// <param name="optimize">Optimize model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Terrain AddTerrain(TerrainDescription description, Matrix transform, bool optimize = true, int order = 0)
        {
            ModelContent geo = LoaderCOLLADA.Load(description.ContentPath, description.ModelFileName, transform);

            if (optimize) geo.Optimize();

            return AddTerrain(geo, description, order);
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="description">Terrain description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Terrain AddTerrain(ModelContent content, TerrainDescription description, int order = 0)
        {
            Terrain newModel = new Terrain(this.Game, content, description.ContentPath, description);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new mini-map
        /// </summary>
        /// <param name="description">Mini-map description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new mini-map</returns>
        public Minimap AddMinimap(MinimapDescription description, int order = 0)
        {
            Minimap newModel = new Minimap(this.Game, description);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new skydom
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Cubemap AddSkydom(CubemapDescription description, int order = 0)
        {
            ModelContent skydom = ModelContent.GenerateSkydom(description.ContentPath, description.Texture, description.Radius);

            Cubemap newModel = new Cubemap(this.Game, skydom);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new background sprite
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="order">Order</param>
        /// <returns>Return new model</returns>
        public Sprite AddBackgroud(BackgroundDescription description, int order = 0)
        {
            Sprite newModel = new Sprite(this.Game, description);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new sprite
        /// </summary>
        /// <param name="description">Sprite description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Sprite AddSprite(SpriteDescription description, int order = 0)
        {
            Sprite newModel = new Sprite(
                this.Game,
                description);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new sprite texture
        /// </summary>
        /// <param name="description">Sprite texture description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public SpriteTexture AddSpriteTexture(SpriteTextureDescription description, int order = 0)
        {
            SpriteTexture newModel = new SpriteTexture(
                this.Game,
                description);

            this.AddComponent(newModel, order);

            return newModel;
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
        public SpriteButton AddSpriteButton(SpriteButtonDescription description, int order = 0)
        {
            SpriteButton newModel = new SpriteButton(
                this.Game,
                description);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new game cursor
        /// </summary>
        /// <param name="description">Sprite description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Cursor AddCursor(SpriteDescription description, int order = 0)
        {
            Cursor newModel = new Cursor(
                this.Game,
                description);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds text
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="color">Color</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new text</returns>
        public TextDrawer AddText(string font, int fontSize, Color4 color, int order = 0)
        {
            TextDrawer newModel = new TextDrawer(this.Game, font, fontSize, color);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds text
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="color">Color</param>
        /// <param name="shadowColor">Shadow color</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new text</returns>
        public TextDrawer AddText(string font, int fontSize, Color4 color, Color4 shadowColor, int order = 0)
        {
            TextDrawer newModel = new TextDrawer(this.Game, font, fontSize, color, shadowColor);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds particle system
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new particle system</returns>
        public ParticleSystem AddParticleSystem(ParticleSystemDescription description, int order = 0)
        {
            ParticleSystem newModel = new ParticleSystem(this.Game, description);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds a line list drawer
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="color">Color</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new line list drawer</returns>
        public LineListDrawer AddLineListDrawer(Line[] lines, Color4 color, int order = 0)
        {
            LineListDrawer newModel = new LineListDrawer(this.Game, lines, color);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds a line list drawer
        /// </summary>
        /// <param name="count">Line count</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new line list drawer</returns>
        public LineListDrawer AddLineListDrawer(int count, int order = 0)
        {
            LineListDrawer newModel = new LineListDrawer(this.Game, count);

            this.AddComponent(newModel, order);

            return newModel;
        }

        /// <summary>
        /// Add component to collection
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="order">Processing order</param>
        private void AddComponent(Drawable component, int order)
        {
            if (!this.components.Contains(component))
            {
                if (order == 0)
                {
                    component.Order = this.components.Count + 1;
                }
                else
                {
                    component.Order = order;
                }

                this.components.Add(component);
                this.components.Sort((p1, p2) =>
                {
                    return p1.Order.CompareTo(p2.Order);
                });
            }
        }
        /// <summary>
        /// Remove and dispose component
        /// </summary>
        /// <param name="component">Component</param>
        public void RemoveComponent(Drawable component)
        {
            if (this.components.Contains(component))
            {
                this.components.Remove(component);

                component.Dispose();
                component = null;
            }
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
    }
}
