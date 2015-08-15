using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using System.Diagnostics;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Render scene
    /// </summary>
    public class Scene : IDisposable
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
        /// Deferred renderer
        /// </summary>
        private DeferredRenderer deferredRenderer = null;
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
        /// Gets or sets whether the scene was handling control captures
        /// </summary>
        protected bool CapturedControl { get; private set; }
        /// <summary>
        /// Draw context
        /// </summary>
        protected Context DrawContext = null;
        /// <summary>
        /// Context for shadow map drawing
        /// </summary>
        protected Context DrawShadowsContext = null;
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
        /// Scene debug text
        /// </summary>
        protected string[] DebugText = null;

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
        /// Gets or sets if scene has to perform frustum culling with objects
        /// </summary>
        public bool PerformFrustumCulling { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene(Game game, SceneModesEnum sceneMode = SceneModesEnum.ForwardLigthning)
        {
            this.Game = game;

            this.Game.Graphics.Resized += new EventHandler(Resized);

            this.Camera = Camera.CreateFree(
                new Vector3(0.0f, 0.0f, -10.0f),
                Vector3.Zero);

            this.Camera.SetLens(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight);

            this.Lights = SceneLight.Default;

            this.SceneVolume = new BoundingSphere(Vector3.Zero, 1000);

            this.PerformFrustumCulling = true;

            this.DrawContext = new Context()
            {
                DrawerMode = sceneMode == SceneModesEnum.ForwardLigthning ? DrawerModesEnum.Forward : DrawerModesEnum.Deferred,
            };

            this.DrawShadowsContext = new Context()
            {
                DrawerMode = DrawerModesEnum.ShadowMap,
            };

            this.deferredRenderer = new DeferredRenderer(game);

            this.DebugText = new string[15];
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
            Stopwatch swTotal = Stopwatch.StartNew();

            this.Camera.Update(gameTime);

            this.DrawContext.World = this.world;
            this.DrawContext.ViewProjection = this.Camera.View * this.Camera.Projection;
            this.DrawContext.EyePosition = this.Camera.Position;

            if (this.Lights.EnableShadows)
            {
                this.DrawShadowsContext.World = Matrix.Identity;
                this.DrawShadowsContext.ViewProjection = this.ShadowMap.View * this.ShadowMap.Projection;
            }

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

            swTotal.Stop();

            this.DebugText[4] = string.Format("Update = {0:000000}", swTotal.ElapsedTicks);
        }
        /// <summary>
        /// Draw scene objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Draw(GameTime gameTime)
        {
            long total = 0;
            long start = 0;
            long shadowMap_start = 0;
            long shadowMap_cull = 0;
            long shadowMap_draw = 0;
            long forward_start = 0;
            long forward_cull = 0;
            long forward_draw = 0;
            long forward_draw2D = 0;
            long deferred_cull = 0;
            long deferred_gbuffer = 0;
            long deferred_lbuffer = 0;
            long deferred_compose = 0;
            long deferred_draw2D = 0;

            Stopwatch swTotal = Stopwatch.StartNew();

            //Draw visible components
            List<Drawable> visibleComponents = this.components.FindAll(c => c.Visible);
            if (visibleComponents.Count > 0)
            {
                #region Preparation

                Stopwatch swStartup = Stopwatch.StartNew();

                //Set lights
                this.DrawContext.Lights = this.Lights;

                //Clear data
                this.DrawContext.GeometryMap = null;
                this.DrawContext.ShadowMap = null;
                this.DrawContext.ShadowTransform = Matrix.Identity;

                swStartup.Stop();

                start = swStartup.ElapsedTicks;

                #endregion

                #region Shadow mapping

                if (this.Lights.EnableShadows && this.Lights.DirectionalLights.Length > 0)
                {
                    #region Preparation

                    Stopwatch swShadowsPreparation = Stopwatch.StartNew();

                    //Clear context data
                    this.DrawShadowsContext.ShadowMap = null;
                    this.DrawShadowsContext.ShadowTransform = Matrix.Identity;

                    //Update shadow transform using first ligth direction
                    this.ShadowMap.Update(this.Lights.DirectionalLights[0].Direction, this.SceneVolume);

                    swShadowsPreparation.Stop();

                    shadowMap_start = swShadowsPreparation.ElapsedTicks;

                    #endregion

                    //Draw components if drop shadow (opaque)
                    List<Drawable> shadowComponents = visibleComponents.FindAll(c => c.Opaque);
                    if (shadowComponents.Count > 0)
                    {
                        #region Cull

                        Stopwatch swCull = Stopwatch.StartNew();

                        bool draw = false;
                        if (this.PerformFrustumCulling)
                        {
                            //Frustum culling
                            draw = this.CullTest(gameTime, this.DrawShadowsContext, shadowComponents);
                        }
                        else
                        {
                            draw = true;
                        }

                        swCull.Stop();

                        shadowMap_cull = swCull.ElapsedTicks;

                        #endregion

                        #region Draw

                        if (draw)
                        {
                            Stopwatch swDraw = Stopwatch.StartNew();

                            //Set shadow map depth map without render target
                            this.Game.Graphics.SetRenderTarget(
                                this.ShadowMap.Viewport,
                                this.ShadowMap.DepthMap,
                                null,
                                true,
                                Color.Silver,
                                DepthStencilClearFlags.Depth);

                            //Use z-buffer by default for opaque components
                            this.Game.Graphics.EnableZBuffer();

                            //Draw scene using depth map
                            this.DrawComponents(gameTime, this.DrawShadowsContext, shadowComponents);

                            //Set shadow map and transform to drawing context
                            this.DrawContext.ShadowMap = this.ShadowMap.ShadowMapTexture;
                            this.DrawContext.ShadowTransform = this.shadowMap.Transform;

                            swDraw.Stop();

                            shadowMap_draw = swDraw.ElapsedTicks;
                        }

                        #endregion
                    }
                }

                #endregion

                #region Render

                if (DrawContext.DrawerMode == DrawerModesEnum.Forward)
                {
                    #region Forward rendering

                    #region Preparation

                    Stopwatch swPreparation = Stopwatch.StartNew();

                    //Set default render target and depth buffer, and clear it
                    this.Game.Graphics.SetDefaultRenderTarget(true);

                    swPreparation.Stop();

                    forward_start = swPreparation.ElapsedTicks;

                    #endregion

                    List<Drawable> solidComponents = visibleComponents.FindAll(c => c.Opaque);
                    if (solidComponents.Count > 0)
                    {
                        #region Cull

                        Stopwatch swCull = Stopwatch.StartNew();

                        bool draw = false;
                        if (this.PerformFrustumCulling)
                        {
                            //Frustum culling
                            draw = this.CullTest(gameTime, this.DrawContext, solidComponents);
                        }
                        else
                        {
                            draw = true;
                        }

                        swCull.Stop();

                        forward_cull = swCull.ElapsedTicks;

                        #endregion

                        #region Draw 3D

                        if (draw)
                        {
                            Stopwatch swDraw = Stopwatch.StartNew();

                            //Use z-buffer by default for opaque components
                            this.Game.Graphics.EnableZBuffer();

                            //Draw solid
                            this.DrawComponents(gameTime, this.DrawContext, solidComponents);

                            swDraw.Stop();

                            forward_draw = swDraw.ElapsedTicks;
                        }

                        #endregion
                    }

                    List<Drawable> otherComponents = visibleComponents.FindAll(c => !c.Opaque);
                    if (otherComponents.Count > 0)
                    {
                        #region Draw 2D

                        Stopwatch swDraw = Stopwatch.StartNew();

                        //Disable z-buffer by default for non-opaque components
                        this.Game.Graphics.DisableZBuffer();

                        //Draw other
                        this.DrawComponents(gameTime, this.DrawContext, otherComponents);

                        swDraw.Stop();

                        forward_draw2D = swDraw.ElapsedTicks;

                        #endregion
                    }

                    #endregion
                }
                else if (DrawContext.DrawerMode == DrawerModesEnum.Deferred)
                {
                    #region Deferred rendering

                    //Render to G-Buffer only opaque objects
                    List<Drawable> solidComponents = visibleComponents.FindAll(c => c.Opaque);
                    if (solidComponents.Count > 0)
                    {
                        #region Cull

                        Stopwatch swCull = Stopwatch.StartNew();

                        bool draw = false;
                        if (this.PerformFrustumCulling)
                        {
                            //Frustum culling
                            draw = this.CullTest(gameTime, this.DrawContext, solidComponents);
                        }
                        else
                        {
                            draw = true;
                        }

                        swCull.Stop();

                        deferred_cull = swCull.ElapsedTicks;

                        #endregion

                        if (draw)
                        {
                            #region Geometry Buffer

                            Stopwatch swGeometryBuffer = Stopwatch.StartNew();

                            //Set g-buffer render targets
                            this.Game.Graphics.SetRenderTargets(
                                this.deferredRenderer.Viewport,
                                this.deferredRenderer.GeometryBuffer.DepthMap,
                                this.deferredRenderer.GeometryBuffer.RenderTargets,
                                true, Color.Black, DepthStencilClearFlags.Depth);

                            //Enable z-buffer by default for opaque components
                            this.Game.Graphics.EnableZBuffer();

                            //Draw scene on g-buffer render targets
                            this.DrawComponents(gameTime, this.DrawContext, solidComponents);

                            //Assign result of render in drawing context
                            this.DrawContext.GeometryMap = this.deferredRenderer.GeometryBuffer.Textures;

                            swGeometryBuffer.Stop();

                            deferred_gbuffer = swGeometryBuffer.ElapsedTicks;

                            #endregion

                            #region Light Buffer

                            Stopwatch swLightBuffer = Stopwatch.StartNew();

                            //Set light buffer to draw lights
                            this.Game.Graphics.SetRenderTarget(
                                this.deferredRenderer.Viewport,
                                null,
                                this.deferredRenderer.LightBuffer.RenderTarget,
                                true, Color.Transparent);

                            this.Game.Graphics.DisableDepthStencil();
                            this.Game.Graphics.SetBlendAlphaToCoverage();

                            //Draw scene lights on light buffer using g-buffer output
                            this.deferredRenderer.DrawLights(this.DrawContext);

                            //Assign result of render in drawing context
                            this.DrawContext.LightMap = this.deferredRenderer.LightBuffer.Texture;

                            swLightBuffer.Stop();

                            deferred_lbuffer = swLightBuffer.ElapsedTicks;

                            #endregion
                        }
                    }

                    #region Final composition

                    Stopwatch swComponsition = Stopwatch.StartNew();

                    //Restore backbuffer as render target and clear it
                    this.Game.Graphics.SetDefaultRenderTarget(true);

                    //Disable z-buffer for deferred rendering
                    this.Game.Graphics.DisableZBuffer();

                    //Draw scene result on screen using g-buffer and light buffer
                    this.deferredRenderer.DrawResult(this.DrawContext);

                    swComponsition.Stop();

                    deferred_compose = swComponsition.ElapsedTicks;

                    #endregion

                    //Render to screen the rest of objects
                    List<Drawable> otherComponents = visibleComponents.FindAll(c => !c.Opaque);
                    if (otherComponents.Count > 0)
                    {
                        Stopwatch swDraw = Stopwatch.StartNew();

                        //Disable z-buffer by default for non-opaque components
                        this.Game.Graphics.DisableZBuffer();

                        //Set forward mode
                        this.DrawContext.DrawerMode = DrawerModesEnum.Forward;

                        //Draw scene
                        this.DrawComponents(gameTime, this.DrawContext, otherComponents);

                        //Set deferred mode
                        this.DrawContext.DrawerMode = DrawerModesEnum.Deferred;

                        swDraw.Stop();

                        deferred_draw2D = swDraw.ElapsedTicks;
                    }

                    #endregion
                }

                #endregion
            }

            swTotal.Stop();

            total = swTotal.ElapsedTicks;

            long totalShadowMap = shadowMap_start + shadowMap_cull + shadowMap_draw;
            if (totalShadowMap > 0)
            {
                float prcStart = (float)shadowMap_start / (float)totalShadowMap;
                float prcCull = (float)shadowMap_cull / (float)totalShadowMap;
                float prcDraw = (float)shadowMap_draw / (float)totalShadowMap;

                this.DebugText[1] = string.Format(
                    "SM = {0:000000}; Start {1:00}%; Cull {2:00}%; Draw {3:00}%",
                    totalShadowMap,
                    prcStart * 100f,
                    prcCull * 100f,
                    prcDraw * 100f);
            }

            long totalForward = forward_start + forward_cull + forward_draw + forward_draw2D;
            if (totalForward > 0)
            {
                float prcStart = (float)forward_start / (float)totalForward;
                float prcCull = (float)forward_cull / (float)totalForward;
                float prcDraw = (float)forward_draw / (float)totalForward;
                float prcDraw2D = (float)forward_draw2D / (float)totalForward;

                this.DebugText[2] = string.Format(
                    "FR = {0:000000}; Start {1:00}%; Cull {2:00}%; Draw {3:00}%; Draw2D {4:00}%",
                    totalForward,
                    prcStart * 100f,
                    prcCull * 100f,
                    prcDraw * 100f,
                    prcDraw2D * 100f);
            }

            long totalDeferred = deferred_cull + deferred_gbuffer + deferred_lbuffer + deferred_compose + deferred_draw2D;
            if (totalDeferred > 0)
            {
                float prcCull = (float)deferred_cull / (float)totalDeferred;
                float prcGBuffer = (float)deferred_gbuffer / (float)totalDeferred;
                float prcLBuffer = (float)deferred_lbuffer / (float)totalDeferred;
                float prcCompose = (float)deferred_compose / (float)totalDeferred;
                float prcDraw2D = (float)deferred_draw2D / (float)totalDeferred;

                this.DebugText[3] = string.Format(
                    "DR = {0:000000}; Cull {1:00}%; GBuffer {2:00}%; LBuffer {3:00}%; Compose {4:00}%; Draw2D {5:00}%",
                    totalDeferred,
                    prcCull * 100f,
                    prcGBuffer * 100f,
                    prcLBuffer * 100f,
                    prcCompose * 100f,
                    prcDraw2D * 100f);
            }

            long other = total - (totalShadowMap + totalForward + totalDeferred);

            float prcSM = (float)totalShadowMap / (float)total;
            float prcFR = (float)totalForward / (float)total;
            float prcDR = (float)totalDeferred / (float)total;
            float prcOther = (float)other / (float)total;

            this.DebugText[0] = string.Format(
                "TOTAL = {0:000000}; Shadows {1:00}%; Forwars {2:00}%; Deferred {3:00}%; Other {4:00}%;",
                total,
                prcSM * 100f,
                prcFR * 100f,
                prcDR * 100f,
                prcOther * 100f);
        }
        /// <summary>
        /// Makes cull test for specified drawable collection
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Drawing context</param>
        /// <param name="components">Components</param>
        /// <returns>Returns true if any component passed culling test</returns>
        protected virtual bool CullTest(GameTime gameTime, Context context, IList<Drawable> components)
        {
            bool res = false;

            for (int i = 0; i < components.Count; i++)
            {
                components[i].FrustumCulling(this.Camera.Frustum);

                if (!components[i].Cull) res = true;
            }

            return res;
        }
        /// <summary>
        /// Drawing of scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Drawing context</param>
        /// <param name="components">Components</param>
        protected virtual void DrawComponents(GameTime gameTime, Context context, IList<Drawable> components)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (!components[i].Cull)
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

            if (this.shadowMap != null)
            {
                this.shadowMap.Dispose();
                this.shadowMap = null;
            }

            for (int i = 0; i < this.components.Count; i++)
            {
                this.components[i].Dispose();
            }

            this.components.Clear();
            this.components = null;

            if (this.deferredRenderer != null)
            {
                this.deferredRenderer.Dispose();
                this.deferredRenderer = null;
            }
        }

        /// <summary>
        /// Fires when render window resized
        /// </summary>
        /// <param name="sender">Graphis device</param>
        /// <param name="e">Event arguments</param>
        protected virtual void Resized(object sender, EventArgs e)
        {
            if (this.deferredRenderer != null)
            {
                this.deferredRenderer.Resize();
            }

            for (int i = 0; i < this.components.Count; i++)
            {
                var fitted = this.components[i] as IScreenFitted;
                if (fitted != null)
                {
                    fitted.Resize();
                }
            }
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

            newModel.Opaque = description.Opaque;
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

            newModel.Opaque = description.Opaque;

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
        /// Adds a triangle list drawer
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="color">Color</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new triangle list drawer</returns>
        public TriangleListDrawer AddTriangleListDrawer(Triangle[] triangles, Color4 color, int order = 0)
        {
            TriangleListDrawer newModel = new TriangleListDrawer(this.Game, triangles, color);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds a triangle list drawer
        /// </summary>
        /// <param name="count">Triangle count</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new triangle list drawer</returns>
        public TriangleListDrawer AddTriangleListDrawer(int count, int order = 0)
        {
            TriangleListDrawer newModel = new TriangleListDrawer(this.Game, count);

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
