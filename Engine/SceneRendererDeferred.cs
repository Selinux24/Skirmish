using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;
using DepthStencilView = SharpDX.Direct3D11.DepthStencilView;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using DepthStencilClearFlags = SharpDX.Direct3D11.DepthStencilClearFlags;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Deferred renderer class
    /// </summary>
    public class SceneRendererDeferred : ISceneRenderer
    {
        private const int ShadowMapSize = 2048;

        /// <summary>
        /// Light geometry
        /// </summary>
        class LightGeometry : IDisposable
        {
            /// <summary>
            /// Window vertex buffer
            /// </summary>
            public Buffer VertexBuffer;
            /// <summary>
            /// Vertex buffer binding
            /// </summary>
            public VertexBufferBinding VertexBufferBinding;
            /// <summary>
            /// Window index buffer
            /// </summary>
            public Buffer IndexBuffer;
            /// <summary>
            /// Index count
            /// </summary>
            public int IndexCount;

            /// <summary>
            /// Dispose objects
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.VertexBuffer);
                Helper.Dispose(this.IndexBuffer);
            }
        }

        /// <summary>
        /// View port
        /// </summary>
        private Viewport viewport;
        /// <summary>
        /// Shadow mapper
        /// </summary>
        private ShadowMap shadowMapper = null;
        /// <summary>
        /// Geometry buffer
        /// </summary>
        private GBuffer geometryBuffer = null;
        /// <summary>
        /// Light buffer
        /// </summary>
        private LightBuffer lightBuffer = null;
        /// <summary>
        /// Light geometry collection
        /// </summary>
        private LightGeometry[] lightGeometry = null;

        /// <summary>
        /// Game
        /// </summary>
        protected Game Game;
        /// <summary>
        /// Renderer width
        /// </summary>
        protected int Width;
        /// <summary>
        /// Renderer height
        /// </summary>
        protected int Height;
        /// <summary>
        /// View * OrthoProjection Matrix
        /// </summary>
        protected Matrix ViewProjection;
        /// <summary>
        /// Update context
        /// </summary>
        protected UpdateContext UpdateContext = null;
        /// <summary>
        /// Draw context
        /// </summary>
        protected DrawContext DrawContext = null;
        /// <summary>
        /// Context for shadow map drawing
        /// </summary>
        protected DrawContext DrawShadowsContext = null;
        /// <summary>
        /// Static shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapStatic
        {
            get
            {
                if (this.shadowMapper != null)
                {
                    return this.shadowMapper.TextureStatic;
                }

                return null;
            }
        }
        /// <summary>
        /// Dynamic shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapDynamic
        {
            get
            {
                if (this.shadowMapper != null)
                {
                    return this.shadowMapper.TextureDynamic;
                }

                return null;
            }
        }
        /// <summary>
        /// Gets or sets whether the static shadow map must be updated in the next frame
        /// </summary>
        protected bool UpdateShadowMapStatic { get; set; }
        /// <summary>
        /// Gets or sets whether the renderer was updated
        /// </summary>
        protected bool Updated { get; set; }
        /// <summary>
        /// Geometry map
        /// </summary>
        protected ShaderResourceView[] GeometryMap
        {
            get
            {
                if (this.geometryBuffer != null)
                {
                    return this.geometryBuffer.Textures;
                }

                return null;
            }
        }
        /// <summary>
        /// Light map
        /// </summary>
        protected ShaderResourceView LightMap
        {
            get
            {
                if (this.lightBuffer != null)
                {
                    return this.lightBuffer.Texture;
                }

                return null;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneRendererDeferred(Game game)
        {
            this.Game = game;

            this.UpdateRectangleAndView();

            this.shadowMapper = new ShadowMap(game, ShadowMapSize, ShadowMapSize);
            this.geometryBuffer = new GBuffer(game);
            this.lightBuffer = new LightBuffer(game);

            this.UpdateContext = new UpdateContext()
            {
                Name = "Primary",
            };

            this.DrawContext = new DrawContext()
            {
                Name = "Primary",
                DrawerMode = DrawerModesEnum.Deferred,
            };

            this.DrawShadowsContext = new DrawContext()
            {
                Name = "Secondary",
                DrawerMode = DrawerModesEnum.ShadowMap,
            };

            this.UpdateShadowMapStatic = true;
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public virtual void Dispose()
        {
            Helper.Dispose(this.shadowMapper);
            Helper.Dispose(this.geometryBuffer);
            Helper.Dispose(this.lightBuffer);
            Helper.Dispose(this.lightGeometry);
        }
        /// <summary>
        /// Resizes buffers
        /// </summary>
        public virtual void Resize()
        {
            this.UpdateRectangleAndView();

            if (this.geometryBuffer != null)
            {
                this.geometryBuffer.Resize();
            }

            if (this.lightBuffer != null)
            {
                this.lightBuffer.Resize();
            }
        }
        /// <summary>
        /// Updates scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        public virtual void Update(GameTime gameTime, Scene scene)
        {
#if DEBUG
            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            Matrix viewProj = scene.Camera.View * scene.Camera.Projection;

            this.UpdateContext.GameTime = gameTime;
            this.UpdateContext.World = scene.World;
            this.UpdateContext.View = scene.Camera.View;
            this.UpdateContext.Projection = scene.Camera.Projection;
            this.UpdateContext.NearPlaneDistance = scene.Camera.NearPlaneDistance;
            this.UpdateContext.FarPlaneDistance = scene.Camera.FarPlaneDistance;
            this.UpdateContext.ViewProjection = viewProj;
            this.UpdateContext.Frustum = new BoundingFrustum(viewProj);
            this.UpdateContext.EyePosition = scene.Camera.Position;
            this.UpdateContext.EyeTarget = scene.Camera.Direction;
            this.UpdateContext.Lights = scene.Lights;

            //Cull lights
            scene.Lights.Cull(this.UpdateContext.Frustum, this.UpdateContext.EyePosition);

            //Update active components
            List<Drawable> activeComponents = scene.Components.FindAll(c => c.Active);
            for (int i = 0; i < activeComponents.Count; i++)
            {
                activeComponents[i].Update(this.UpdateContext);
            }

            this.Updated = true;
#if DEBUG
            swTotal.Stop();
#endif
#if DEBUG
            Counters.SetStatistics("Scene.Update", string.Format("Update = {0:000000}", swTotal.ElapsedTicks));
#endif
        }
        /// <summary>
        /// Draws scene components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="scene">Scene</param>
        public virtual void Draw(GameTime gameTime, Scene scene)
        {
            if (this.Updated)
            {
                this.Updated = false;
#if DEBUG
                long total = 0;
                long start = 0;
                long shadowMap_start = 0;
                long shadowMap_cull = 0;
                long shadowMap_draw = 0;
                long deferred_cull = 0;
                long deferred_gbuffer = 0;
                long deferred_gbufferInit = 0;
                long deferred_gbufferDraw = 0;
                long deferred_gbufferResolve = 0;
                long deferred_lbuffer = 0;
                long deferred_lbufferInit = 0;
                long deferred_lbufferDir = 0;
                long deferred_lbufferPoi = 0;
                long deferred_lbufferSpo = 0;
                long deferred_compose = 0;
                long deferred_composeInit = 0;
                long deferred_composeDraw = 0;
                long deferred_draw2D = 0;
#endif
#if DEBUG
                Stopwatch swTotal = Stopwatch.StartNew();
#endif
                //Draw visible components
                List<Drawable> visibleComponents = scene.Components.FindAll(c => c.Visible);
                if (visibleComponents.Count > 0)
                {
                    #region Preparation
#if DEBUG
                    Stopwatch swStartup = Stopwatch.StartNew();
#endif
                    //Initialize context data from update context
                    this.DrawContext.GameTime = gameTime;
                    this.DrawContext.World = this.UpdateContext.World;
                    this.DrawContext.View = this.UpdateContext.View;
                    this.DrawContext.Projection = this.UpdateContext.Projection;
                    this.DrawContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawContext.Frustum = this.UpdateContext.Frustum;
                    this.DrawContext.EyePosition = this.UpdateContext.EyePosition;
                    this.DrawContext.EyeTarget = this.UpdateContext.EyeTarget;
                    //Initialize context data from scene
                    this.DrawContext.Lights = scene.Lights;
                    this.DrawContext.ShadowMaps = 0;
                    this.DrawContext.ShadowMapStatic = null;
                    this.DrawContext.ShadowMapDynamic = null;
                    this.DrawContext.FromLightViewProjection = Matrix.Identity;
#if DEBUG
                    swStartup.Stop();

                    start = swStartup.ElapsedTicks;
#endif
                    #endregion

                    #region Shadow mapping

                    var shadowCastingLights = scene.Lights.ShadowCastingLights;
                    if (shadowCastingLights.Length > 0)
                    {
                        #region Preparation
#if DEBUG
                        Stopwatch swShadowsPreparation = Stopwatch.StartNew();
#endif

                        this.DrawShadowsContext.GameTime = gameTime;
                        this.DrawShadowsContext.World = this.UpdateContext.World;
#if DEBUG
                        swShadowsPreparation.Stop();

                        shadowMap_cull = 0;
                        shadowMap_draw = 0;

                        shadowMap_start = swShadowsPreparation.ElapsedTicks;
#endif
                        #endregion

                        if (this.UpdateShadowMapStatic)
                        {
                            #region Static shadow map

                            //Draw static components if cast shadow
                            List<Drawable> staticObjs = visibleComponents.FindAll(c => c.CastShadow == true && c.Static == true);
                            if (staticObjs.Count > 0)
                            {
                                if (!this.shadowMapper.Flags.HasFlag(ShadowMapFlags.Static))
                                {
                                    this.shadowMapper.Flags |= ShadowMapFlags.Static;
                                }

                                #region Draw
#if DEBUG
                                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                                staticObjs.ForEach(o => o.SetCulling(false));

                                this.shadowMapper.Update(
                                    shadowCastingLights[0],
                                    scene.SceneVolume.Center,
                                    scene.SceneVolume.Radius,
                                    ref this.DrawShadowsContext);
                                this.BindShadowMap(this.shadowMapper.DepthMapStatic);
                                this.DrawShadowsComponents(gameTime, this.DrawShadowsContext, staticObjs);
#if DEBUG
                                swDraw.Stop();

                                shadowMap_draw += swDraw.ElapsedTicks;
#endif
                                #endregion
                            }

                            #endregion

                            this.UpdateShadowMapStatic = false;
                        }

                        #region Dynamic shadow map

                        //Draw dynamic components if drop shadow (opaque)
                        List<Drawable> dynamicObjs = visibleComponents.FindAll(c => c.CastShadow == true && c.Static == false);
                        if (dynamicObjs.Count > 0)
                        {
                            #region Cull
#if DEBUG
                            Stopwatch swCull = Stopwatch.StartNew();
#endif
                            bool draw = false;
                            if (scene.PerformFrustumCulling)
                            {
                                //Frustum culling
                                draw = scene.CullTest(this.DrawContext.Frustum, dynamicObjs);
                            }
                            else
                            {
                                draw = true;
                            }
#if DEBUG
                            swCull.Stop();

                            shadowMap_cull += swCull.ElapsedTicks;
#endif
                            #endregion

                            #region Draw

                            if (draw)
                            {
                                if (!this.shadowMapper.Flags.HasFlag(ShadowMapFlags.Dynamic))
                                {
                                    this.shadowMapper.Flags |= ShadowMapFlags.Dynamic;
                                }

#if DEBUG
                                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                                this.shadowMapper.Update(
                                    shadowCastingLights[0],
                                    scene.SceneVolume.Center,
                                    scene.SceneVolume.Radius,
                                    ref this.DrawShadowsContext);
                                this.BindShadowMap(this.shadowMapper.DepthMapDynamic);
                                this.DrawShadowsComponents(gameTime, this.DrawShadowsContext, dynamicObjs);
#if DEBUG
                                swDraw.Stop();

                                shadowMap_draw += swDraw.ElapsedTicks;
#endif
                            }

                            #endregion
                        }

                        #endregion

                        //Set shadow map and transform to drawing context
                        this.DrawContext.ShadowMaps = (int)this.shadowMapper.Flags;
                        this.DrawContext.ShadowMapStatic = this.shadowMapper.TextureStatic;
                        this.DrawContext.ShadowMapDynamic = this.shadowMapper.TextureDynamic;
                        this.DrawContext.FromLightViewProjection = this.shadowMapper.ViewProjection;
                    }

                    #endregion

                    #region Render

                    #region Deferred rendering

                    //Render to G-Buffer only opaque objects
                    List<Drawable> solidComponents = visibleComponents.FindAll(c => c.DeferredEnabled);
                    if (solidComponents.Count > 0)
                    {
                        #region Cull
#if DEBUG
                        Stopwatch swCull = Stopwatch.StartNew();
#endif
                        bool draw = false;
                        if (scene.PerformFrustumCulling)
                        {
                            //Frustum culling
                            draw = scene.CullTest(this.DrawContext.Frustum, solidComponents);
                        }
                        else
                        {
                            draw = true;
                        }
#if DEBUG
                        swCull.Stop();

                        deferred_cull = swCull.ElapsedTicks;
#endif
                        #endregion

                        if (draw)
                        {
                            #region Geometry Buffer
#if DEBUG
                            Stopwatch swGeometryBuffer = Stopwatch.StartNew();
#endif
#if DEBUG
                            Stopwatch swGeometryBufferInit = Stopwatch.StartNew();
#endif
                            this.BindGBuffer();
#if DEBUG
                            swGeometryBufferInit.Stop();
#endif
#if DEBUG
                            Stopwatch swGeometryBufferDraw = Stopwatch.StartNew();
#endif
                            //Draw scene on g-buffer render targets
                            this.DrawResultComponents(gameTime, this.DrawContext, solidComponents);
#if DEBUG
                            swGeometryBufferDraw.Stop();
#endif
#if DEBUG
                            Stopwatch swGeometryBufferResolve = Stopwatch.StartNew();
#endif
#if DEBUG
                            swGeometryBufferResolve.Stop();
#endif
#if DEBUG
                            swGeometryBuffer.Stop();
#endif
#if DEBUG
                            deferred_gbuffer = swGeometryBuffer.ElapsedTicks;
                            deferred_gbufferInit = swGeometryBufferInit.ElapsedTicks;
                            deferred_gbufferDraw = swGeometryBufferDraw.ElapsedTicks;
                            deferred_gbufferResolve = swGeometryBufferResolve.ElapsedTicks;
#endif
                            #endregion

                            #region Light Buffer
#if DEBUG
                            Stopwatch swLightBuffer = Stopwatch.StartNew();
#endif
                            this.BindLights();

                            //Draw scene lights on light buffer using g-buffer output
                            this.DrawLights(this.DrawContext);
#if DEBUG
                            swLightBuffer.Stop();
#endif
#if DEBUG
                            deferred_lbuffer = swLightBuffer.ElapsedTicks;

                            long[] deferredCounters = Counters.GetStatistics("DEFERRED_LIGHTING") as long[];
                            if (deferredCounters != null)
                            {
                                deferred_lbufferInit = deferredCounters[0];
                                deferred_lbufferDir = deferredCounters[1];
                                deferred_lbufferPoi = deferredCounters[2];
                                deferred_lbufferSpo = deferredCounters[3];
                            }
#endif
                            #endregion
                        }
                    }

                    #region Final composition
#if DEBUG
                    Stopwatch swComponsition = Stopwatch.StartNew();
#endif
                    this.BindResult();

                    //Draw scene result on screen using g-buffer and light buffer
                    this.DrawResult(this.DrawContext);
#if DEBUG
                    swComponsition.Stop();

                    deferred_compose = swComponsition.ElapsedTicks;

                    long[] deferredCompositionCounters = Counters.GetStatistics("DEFERRED_COMPOSITION") as long[];
                    if (deferredCompositionCounters != null)
                    {
                        deferred_composeInit = deferredCompositionCounters[0];
                        deferred_composeDraw = deferredCompositionCounters[1];
                    }
#endif
                    #endregion

                    //Render to screen the rest of objects
                    List<Drawable> otherComponents = visibleComponents.FindAll(c => !c.DeferredEnabled);
                    if (otherComponents.Count > 0)
                    {
                        #region Draw other
#if DEBUG
                        Stopwatch swDraw = Stopwatch.StartNew();
#endif
                        //Set forward mode
                        this.DrawContext.DrawerMode = DrawerModesEnum.Forward;

                        //Draw scene
                        this.DrawResultComponents(gameTime, this.DrawContext, otherComponents);

                        //Set deferred mode
                        this.DrawContext.DrawerMode = DrawerModesEnum.Deferred;
#if DEBUG
                        swDraw.Stop();

                        deferred_draw2D = swDraw.ElapsedTicks;
#endif
                        #endregion
                    }

                    #endregion

                    #endregion
                }
#if DEBUG
                swTotal.Stop();

                total = swTotal.ElapsedTicks;
#endif
#if DEBUG
                long totalShadowMap = shadowMap_start + shadowMap_cull + shadowMap_draw;
                if (totalShadowMap > 0)
                {
                    float prcStart = (float)shadowMap_start / (float)totalShadowMap;
                    float prcCull = (float)shadowMap_cull / (float)totalShadowMap;
                    float prcDraw = (float)shadowMap_draw / (float)totalShadowMap;

                    Counters.SetStatistics("Scene.Draw.totalShadowMap", string.Format(
                        "SM = {0:000000}; Start {1:00}%; Cull {2:00}%; Draw {3:00}%",
                        totalShadowMap,
                        prcStart * 100f,
                        prcCull * 100f,
                        prcDraw * 100f));
                }

                long totalDeferred = deferred_cull + deferred_gbuffer + deferred_lbuffer + deferred_compose + deferred_draw2D;
                if (totalDeferred > 0)
                {
                    float prcCull = (float)deferred_cull / (float)totalDeferred;
                    float prcGBuffer = (float)deferred_gbuffer / (float)totalDeferred;
                    float prcLBuffer = (float)deferred_lbuffer / (float)totalDeferred;
                    float prcCompose = (float)deferred_compose / (float)totalDeferred;
                    float prcDraw2D = (float)deferred_draw2D / (float)totalDeferred;

                    Counters.SetStatistics("Scene.Draw.totalDeferred", string.Format(
                        "DR = {0:000000}; Cull {1:00}%; GBuffer {2:00}%; LBuffer {3:00}%; Compose {4:00}%; Draw2D {5:00}%",
                        totalDeferred,
                        prcCull * 100f,
                        prcGBuffer * 100f,
                        prcLBuffer * 100f,
                        prcCompose * 100f,
                        prcDraw2D * 100f));

                    if (deferred_gbuffer > 0)
                    {
                        float prcPass1 = (float)deferred_gbufferInit / (float)deferred_gbuffer;
                        float prcPass2 = (float)deferred_gbufferDraw / (float)deferred_gbuffer;
                        float prcPass3 = (float)deferred_gbufferResolve / (float)deferred_gbuffer;

                        Counters.SetStatistics("Scene.Draw.deferred_gbuffer PRC", string.Format(
                            "GBuffer = {0:000000}; Init {1:00}%; Draw {2:00}%; Resolve {3:00}%",
                            deferred_gbuffer,
                            prcPass1 * 100f,
                            prcPass2 * 100f,
                            prcPass3 * 100f));

                        Counters.SetStatistics("Scene.Draw.deferred_gbuffer CNT", string.Format(
                            "GBuffer = {0:000000}; Init {1:000000}; Draw {2:000000}; Resolve {3:000000}",
                            deferred_gbuffer,
                            deferred_gbufferInit,
                            deferred_gbufferDraw,
                            deferred_gbufferResolve));
                    }

                    if (deferred_lbuffer > 0)
                    {
                        float prcPass1 = (float)deferred_lbufferInit / (float)deferred_lbuffer;
                        float prcPass2 = (float)deferred_lbufferDir / (float)deferred_lbuffer;
                        float prcPass3 = (float)deferred_lbufferPoi / (float)deferred_lbuffer;
                        float prcPass4 = (float)deferred_lbufferSpo / (float)deferred_lbuffer;

                        Counters.SetStatistics("Scene.Draw.deferred_lbuffer PRC", string.Format(
                            "LBuffer = {0:000000}; Init {1:00}%; Directionals {2:00}%; Points {3:00}%; Spots {4:00}%",
                            deferred_lbuffer,
                            prcPass1 * 100f,
                            prcPass2 * 100f,
                            prcPass3 * 100f,
                            prcPass4 * 100f));

                        Counters.SetStatistics("Scene.Draw.deferred_lbuffer CNT", string.Format(
                            "LBuffer = {0:000000}; Init {1:000000}; Directionals {2:000000}; Points {3:000000}; Spots {4:000000}",
                            deferred_lbuffer,
                            deferred_lbufferInit,
                            deferred_lbufferDir,
                            deferred_lbufferPoi,
                            deferred_lbufferSpo));
                    }

                    if (deferred_compose > 0)
                    {
                        float prcPass1 = (float)deferred_composeInit / (float)deferred_compose;
                        float prcPass2 = (float)deferred_composeDraw / (float)deferred_compose;

                        Counters.SetStatistics("Scene.Draw.deferred_compose PRC", string.Format(
                            "Compose = {0:000000}; Init {1:00}%; Draw {2:00}%",
                            deferred_compose,
                            prcPass1 * 100f,
                            prcPass2 * 100f));

                        Counters.SetStatistics("Scene.Draw.deferred_compose CNT", string.Format(
                            "Compose = {0:000000}; Init {1:000000}; Draw {2:000000}",
                            deferred_compose,
                            deferred_composeInit,
                            deferred_composeDraw));
                    }
                }

                long other = total - (totalShadowMap + totalDeferred);

                float prcSM = (float)totalShadowMap / (float)total;
                float prcDR = (float)totalDeferred / (float)total;
                float prcOther = (float)other / (float)total;

                Counters.SetStatistics("Scene.Draw", string.Format(
                    "TOTAL = {0:000000}; Shadows {1:00}%; Deferred {2:00}%; Other {3:00}%;",
                    total,
                    prcSM * 100f,
                    prcDR * 100f,
                    prcOther * 100f));
#endif
            }
        }
        /// <summary>
        /// Gets renderer resources
        /// </summary>
        /// <param name="result">Resource type</param>
        /// <returns>Returns renderer specified resource, if renderer produces that resource.</returns>
        public virtual ShaderResourceView GetResource(SceneRendererResultEnum result)
        {
            if (result == SceneRendererResultEnum.ShadowMapStatic) return this.ShadowMapStatic;
            if (result == SceneRendererResultEnum.ShadowMapDynamic) return this.ShadowMapDynamic;
            if (result == SceneRendererResultEnum.LightMap) return this.LightMap;

            if (this.GeometryMap != null && this.GeometryMap.Length > 0)
            {
                if (result == SceneRendererResultEnum.ColorMap) return this.GeometryMap.Length > 0 ? this.GeometryMap[0] : null;
                if (result == SceneRendererResultEnum.NormalMap) return this.GeometryMap.Length > 1 ? this.GeometryMap[1] : null;
                if (result == SceneRendererResultEnum.DepthMap) return this.GeometryMap.Length > 2 ? this.GeometryMap[2] : null;
            }

            return null;
        }

        /// <summary>
        /// Updates renderer parameters
        /// </summary>
        private void UpdateRectangleAndView()
        {
            this.Width = this.Game.Form.RenderWidth;
            this.Height = this.Game.Form.RenderHeight;

            this.viewport = new Viewport(0, 0, this.Width, this.Height, 0, 1.0f);

            this.ViewProjection = Sprite.CreateViewOrthoProjection(this.Width, this.Height);

            if (this.lightGeometry == null)
            {
                this.lightGeometry = new[]
                {
                    new LightGeometry(),
                    new LightGeometry(),
                    new LightGeometry(),
                };
            }

            this.UpdateDirectionalLightGeometry(ref this.lightGeometry[0]);
            this.UpdatePointLightGeometry(ref this.lightGeometry[1]);
            this.UpdateSpotLightGeometry(ref this.lightGeometry[2]);
        }
        /// <summary>
        /// Update directional light buffer
        /// </summary>
        /// <param name="geometry">Geometry</param>
        private void UpdateDirectionalLightGeometry(ref LightGeometry geometry)
        {
            Vector3[] cv;
            Vector2[] cuv;
            uint[] ci;
            GeometryUtil.CreateScreen(
                Game.Form,
                out cv,
                out cuv,
                out ci);

            VertexPositionTexture[] vertices = VertexPositionTexture.Generate(cv, cuv);

            if (geometry.VertexBuffer == null)
            {
                geometry.VertexBuffer = Game.Graphics.Device.CreateVertexBufferWrite(vertices);
                geometry.VertexBufferBinding = new VertexBufferBinding(geometry.VertexBuffer, vertices[0].Stride, 0);
            }
            else
            {
                this.Game.Graphics.DeviceContext.WriteBuffer(geometry.VertexBuffer, vertices);
            }

            if (geometry.IndexBuffer == null)
            {
                geometry.IndexBuffer = Game.Graphics.Device.CreateIndexBufferImmutable(ci);
            }

            geometry.IndexCount = ci.Length;
        }
        /// <summary>
        /// Update point light buffer
        /// </summary>
        /// <param name="geometry">Geometry</param>
        private void UpdatePointLightGeometry(ref LightGeometry geometry)
        {
            Vector3[] cv;
            uint[] ci;
            GeometryUtil.CreateSphere(
                1, 12, 12,
                out cv,
                out ci);

            VertexPosition[] vertices = VertexPosition.Generate(cv);

            if (geometry.VertexBuffer == null)
            {
                geometry.VertexBuffer = Game.Graphics.Device.CreateVertexBufferWrite(vertices);
                geometry.VertexBufferBinding = new VertexBufferBinding(geometry.VertexBuffer, vertices[0].Stride, 0);
            }
            else
            {
                this.Game.Graphics.DeviceContext.WriteBuffer(geometry.VertexBuffer, vertices);
            }

            if (geometry.IndexBuffer == null)
            {
                geometry.IndexBuffer = Game.Graphics.Device.CreateIndexBufferImmutable(ci);
            }

            geometry.IndexCount = ci.Length;
        }
        /// <summary>
        /// Update spot light buffer
        /// </summary>
        /// <param name="geometry">Geometry</param>
        private void UpdateSpotLightGeometry(ref LightGeometry geometry)
        {
            Vector3[] cv;
            uint[] ci;
            GeometryUtil.CreateSphere(
                1, 12, 12,
                out cv,
                out ci);

            VertexPosition[] vertices = VertexPosition.Generate(cv);

            if (geometry.VertexBuffer == null)
            {
                geometry.VertexBuffer = Game.Graphics.Device.CreateVertexBufferWrite(vertices);
                geometry.VertexBufferBinding = new VertexBufferBinding(geometry.VertexBuffer, vertices[0].Stride, 0);
            }
            else
            {
                this.Game.Graphics.DeviceContext.WriteBuffer(geometry.VertexBuffer, vertices);
            }

            if (geometry.IndexBuffer == null)
            {
                geometry.IndexBuffer = Game.Graphics.Device.CreateIndexBufferImmutable(ci);
            }

            geometry.IndexCount = ci.Length;
        }
        /// <summary>
        /// Binds graphics for shadow mapping pass
        /// </summary>
        private void BindShadowMap(DepthStencilView dsv)
        {
            //Set shadow mapper viewport
            this.Game.Graphics.SetViewport(this.shadowMapper.Viewport);

            //Set shadow map depth map without render target
            this.Game.Graphics.SetRenderTarget(
                null,
                false,
                Color.Transparent,
                dsv,
                true,
                DepthStencilClearFlags.Depth);
        }
        /// <summary>
        /// Binds graphics for g-buffer pass
        /// </summary>
        private void BindGBuffer()
        {
            //Set local viewport
            this.Game.Graphics.SetViewport(this.viewport);

            //Set g-buffer render targets
            this.Game.Graphics.SetRenderTargets(
                this.geometryBuffer.RenderTargets, true, Color.Black,
                this.Game.Graphics.DefaultDepthStencil, true);
        }
        /// <summary>
        /// Binds graphics for light acummulation pass
        /// </summary>
        private void BindLights()
        {
            //Set local viewport
            this.Game.Graphics.SetViewport(this.viewport);

            //Set light buffer to draw lights
            this.Game.Graphics.SetRenderTarget(
                this.lightBuffer.RenderTarget, true, Color.Black,
                this.Game.Graphics.DefaultDepthStencil, false);
        }
        /// <summary>
        /// Binds graphics for results pass
        /// </summary>
        private void BindResult()
        {
            //Restore backbuffer as render target and clear it
            this.Game.Graphics.SetDefaultViewport();
            this.Game.Graphics.SetDefaultRenderTarget(false);
        }
        /// <summary>
        /// Draw lights
        /// </summary>
        /// <param name="context">Drawing context</param>
        private void DrawLights(DrawContext context)
        {
#if DEBUG
            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            #region Initialization
#if DEBUG
            Stopwatch swPrepare = Stopwatch.StartNew();
#endif
            var deviceContext = this.Game.Graphics.DeviceContext;
            var effect = DrawerPool.EffectDeferred;

            var directionalLights = context.Lights.GetVisibleDirectionalLights();
            var spotLights = context.Lights.GetVisibleSpotLights();
            var pointLights = context.Lights.GetVisiblePointLights();

            effect.UpdatePerFrame(
                context.World,
                this.ViewProjection,
                context.EyePosition,
                directionalLights.Length,
                context.Lights.FogStart,
                context.Lights.FogRange,
                context.Lights.FogColor,
                this.GeometryMap[0],
                this.GeometryMap[1],
                this.GeometryMap[2],
                context.ShadowMaps,
                context.ShadowMapStatic,
                context.ShadowMapDynamic);

            this.Game.Graphics.SetDepthStencilRDZDisabled();
            this.Game.Graphics.SetBlendDeferredLighting();
#if DEBUG
            swPrepare.Stop();
#endif
            #endregion

            #region Directional Lights
#if DEBUG
            Stopwatch swDirectional = Stopwatch.StartNew();
#endif
            if (directionalLights != null && directionalLights.Length > 0)
            {
                var effectTechnique = effect.DeferredDirectionalLight;
                var geometry = this.lightGeometry[0];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                Counters.IAInputLayoutSets++;
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                Counters.IAVertexBuffersSets++;
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;

                for (int i = 0; i < directionalLights.Length; i++)
                {
                    effect.UpdatePerLight(directionalLights[i], context.FromLightViewProjection);

                    for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                    {
                        effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                        deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                    }
                }
            }
#if DEBUG
            swDirectional.Stop();
#endif
            #endregion

            #region Spot Lights
#if DEBUG
            Stopwatch swSpot = Stopwatch.StartNew();
#endif
            if (spotLights != null && spotLights.Length > 0)
            {
                var effectTechnique = effect.DeferredSpotLight;
                var geometry = this.lightGeometry[2];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                Counters.IAInputLayoutSets++;
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                Counters.IAVertexBuffersSets++;
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;

                this.Game.Graphics.SetRasterizerCullFrontFace();
                this.Game.Graphics.SetDepthStencilDeferredLightingDrawing();

                for (int i = 0; i < spotLights.Length; i++)
                {
                    var light = spotLights[i];

                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        context.World,
                        context.ViewProjection);

                    for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                    {
                        effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                        deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                    }
                }
            }
#if DEBUG
            swSpot.Stop();
#endif
            #endregion

            #region Point Lights
#if DEBUG
            Stopwatch swPoint = Stopwatch.StartNew();
#endif
            if (pointLights != null && pointLights.Length > 0)
            {
                var effectTechnique = effect.DeferredPointLight;
                var geometry = this.lightGeometry[1];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                Counters.IAInputLayoutSets++;
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                Counters.IAVertexBuffersSets++;
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;

                this.Game.Graphics.SetRasterizerCullFrontFace();
                this.Game.Graphics.SetDepthStencilDeferredLightingDrawing();

                for (int i = 0; i < pointLights.Length; i++)
                {
                    var light = pointLights[i];

                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        context.World * light.Transform,
                        context.ViewProjection);

                    for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                    {
                        effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                        deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                        Counters.DrawCallsPerFrame++;
                        Counters.InstancesPerFrame++;
                    }
                }
            }
#if DEBUG
            swPoint.Stop();
#endif
            #endregion
#if DEBUG
            swTotal.Stop();
#endif
#if DEBUG
            long total = swPrepare.ElapsedTicks + swDirectional.ElapsedTicks + swPoint.ElapsedTicks + swSpot.ElapsedTicks;
            if (total > 0)
            {
                float prcPrepare = (float)swPrepare.ElapsedTicks / (float)total;
                float prcDirectional = (float)swDirectional.ElapsedTicks / (float)total;
                float prcPoint = (float)swPoint.ElapsedTicks / (float)total;
                float prcSpot = (float)swSpot.ElapsedTicks / (float)total;
                float prcWasted = (float)(swTotal.ElapsedTicks - total) / (float)total;

                Counters.SetStatistics("DeferredRenderer.DrawLights", string.Format(
                    "{0:000000}; Init {1:00}%; Directional {2:00}%; Point {3:00}%; Spot {4:00}%; Other {5:00}%",
                    swTotal.ElapsedTicks,
                    prcPrepare * 100f,
                    prcDirectional * 100f,
                    prcPoint * 100f,
                    prcSpot * 100f,
                    prcWasted * 100f));
            }

            float perDirectionalLight = 0f;
            float perPointLight = 0f;
            float perSpotLight = 0f;

            if (directionalLights != null && directionalLights.Length > 0)
            {
                long totalDirectional = swDirectional.ElapsedTicks;
                if (totalDirectional > 0)
                {
                    perDirectionalLight = (float)totalDirectional / (float)directionalLights.Length;
                }
            }

            if (pointLights != null && pointLights.Length > 0)
            {
                long totalPoint = swPoint.ElapsedTicks;
                if (totalPoint > 0)
                {
                    perPointLight = (float)totalPoint / (float)pointLights.Length;
                }
            }

            if (spotLights != null && spotLights.Length > 0)
            {
                long totalSpot = swSpot.ElapsedTicks;
                if (totalSpot > 0)
                {
                    perSpotLight = (float)totalSpot / (float)spotLights.Length;
                }
            }

            Counters.SetStatistics("DeferredRenderer.DrawLights.Types", string.Format(
                "Directional {0:000000}; Point {1:000000}; Spot {2:000000}",
                perDirectionalLight,
                perPointLight,
                perSpotLight));

            Counters.SetStatistics("DEFERRED_LIGHTING", new[]
            {
                swPrepare.ElapsedTicks,
                swDirectional.ElapsedTicks,
                swPoint.ElapsedTicks,
                swSpot.ElapsedTicks,
            });
#endif
        }
        /// <summary>
        /// Draw result
        /// </summary>
        /// <param name="context">Drawing context</param>
        private void DrawResult(DrawContext context)
        {
#if DEBUG
            long total = 0;
            long init = 0;
            long draw = 0;

            Stopwatch swTotal = Stopwatch.StartNew();
#endif
            if (this.GeometryMap != null && this.LightMap != null)
            {
#if DEBUG
                Stopwatch swInit = Stopwatch.StartNew();
#endif
                var effect = DrawerPool.EffectDeferred;
                var effectTechnique = effect.DeferredCombineLights;

                effect.UpdateComposer(
                    context.World,
                    this.ViewProjection,
                    context.EyePosition,
                    this.GeometryMap[2],
                    this.LightMap);

                var deviceContext = this.Game.Graphics.DeviceContext;
                var geometry = this.lightGeometry[0];

                deviceContext.InputAssembler.InputLayout = effect.GetInputLayout(effectTechnique);
                Counters.IAInputLayoutSets++;
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Counters.IAPrimitiveTopologySets++;
                deviceContext.InputAssembler.SetVertexBuffers(0, geometry.VertexBufferBinding);
                Counters.IAVertexBuffersSets++;
                deviceContext.InputAssembler.SetIndexBuffer(geometry.IndexBuffer, Format.R32_UInt, 0);
                Counters.IAIndexBufferSets++;

                this.Game.Graphics.SetDepthStencilZDisabled();
                this.Game.Graphics.SetRasterizerDefault();
                this.Game.Graphics.SetBlendDefault();
#if DEBUG
                swInit.Stop();

                init = swInit.ElapsedTicks;
#endif
#if DEBUG
                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                {
                    effectTechnique.GetPassByIndex(p).Apply(deviceContext, 0);

                    deviceContext.DrawIndexed(geometry.IndexCount, 0, 0);

                    Counters.DrawCallsPerFrame++;
                    Counters.InstancesPerFrame++;
                }
#if DEBUG
                swDraw.Stop();

                draw = swDraw.ElapsedTicks;
#endif
            }
#if DEBUG
            swTotal.Stop();

            total = swTotal.ElapsedTicks;
#endif
#if DEBUG
            Counters.SetStatistics("DEFERRED_COMPOSITION", new[]
            {
                init,
                draw,
            });
#endif
        }
        /// <summary>
        /// Draw components for shadow mapping
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="components">Components</param>
        private void DrawShadowsComponents(GameTime gameTime, DrawContext context, List<Drawable> components)
        {
            var toDraw = components.FindAll(c => !c.Cull);
            if (toDraw.Count > 0)
            {
                toDraw.ForEach((c) =>
                {
                    this.Game.Graphics.SetRasterizerCullFrontFace();

                    if (c.EnableDepthStencil) this.Game.Graphics.SetDepthStencilZEnabled();
                    else this.Game.Graphics.SetDepthStencilZDisabled();

                    if (c.EnableAlphaBlending) this.Game.Graphics.SetBlendDeferredComposerTransparent();
                    else this.Game.Graphics.SetBlendDeferredComposer();

                    c.Draw(context);
                });
            }
        }
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="components">Components</param>
        private void DrawResultComponents(GameTime gameTime, DrawContext context, List<Drawable> components)
        {
            var toDraw = components.FindAll(c => !c.Cull);
            if (toDraw.Count > 0)
            {
                toDraw.ForEach((c) =>
                {
                    this.Game.Graphics.SetRasterizerDefault();

                    if (c.EnableDepthStencil) this.Game.Graphics.SetDepthStencilZEnabled();
                    else this.Game.Graphics.SetDepthStencilZDisabled();

                    if (c.EnableAlphaBlending) this.Game.Graphics.SetBlendDeferredComposerTransparent();
                    else this.Game.Graphics.SetBlendDeferredComposer();

                    c.Draw(context);
                });
            }
        }
    }
}
