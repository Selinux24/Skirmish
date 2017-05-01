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
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using InputLayout = SharpDX.Direct3D11.InputLayout;

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
        public static int BufferSlot = 15;

        private const int ShadowMapSize = 1024 * 4;

        /// <summary>
        /// Light geometry
        /// </summary>
        struct LightGeometry
        {
            /// <summary>
            /// Geometry offset
            /// </summary>
            public int Offset;
            /// <summary>
            /// Index count
            /// </summary>
            public int IndexCount;
        }

        /// <summary>
        /// View port
        /// </summary>
        private Viewport viewport;
        /// <summary>
        /// High definition shadow mapper
        /// </summary>
        private ShadowMap shadowMapperHigh = null;
        /// <summary>
        /// Low definition shadow mapper
        /// </summary>
        private ShadowMap shadowMapperLow = null;
        /// <summary>
        /// Geometry buffer
        /// </summary>
        private RenderTarget geometryBuffer = null;
        /// <summary>
        /// Light buffer
        /// </summary>
        private RenderTarget lightBuffer = null;
        /// <summary>
        /// Window vertex buffer
        /// </summary>
        private Buffer lightGeometryVertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding lightGeometryVertexBufferBinding;
        /// <summary>
        /// Window index buffer
        /// </summary>
        private Buffer lightGeometryIndexBuffer;
        /// <summary>
        /// Screen geometry
        /// </summary>
        private LightGeometry screenGeometry;
        /// <summary>
        /// Point light geometry
        /// </summary>
        private LightGeometry pointLightGeometry;
        /// <summary>
        /// Spot ligth geometry
        /// </summary>
        private LightGeometry spotLightGeometry;
        /// <summary>
        /// Input layout for directional lights
        /// </summary>
        private InputLayout dirLightInputLayout;
        /// <summary>
        /// Input layout for point lights
        /// </summary>
        private InputLayout pointLightInputLayout;
        /// <summary>
        /// Input layout for spot ligths
        /// </summary>
        private InputLayout spotLightInputLayout;
        /// <summary>
        /// Input layout for result light map
        /// </summary>
        private InputLayout combineLightsInputLayout;

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
        /// Low definition shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapLow
        {
            get
            {
                if (this.shadowMapperLow != null)
                {
                    return this.shadowMapperLow.Texture;
                }

                return null;
            }
        }
        /// <summary>
        /// High definition shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapHigh
        {
            get
            {
                if (this.shadowMapperHigh != null)
                {
                    return this.shadowMapperHigh.Texture;
                }

                return null;
            }
        }
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
        protected ShaderResourceView[] LightMap
        {
            get
            {
                if (this.lightBuffer != null)
                {
                    return this.lightBuffer.Textures;
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

            this.shadowMapperLow = new ShadowMap(game, ShadowMapSize, ShadowMapSize);
            this.shadowMapperHigh = new ShadowMap(game, ShadowMapSize, ShadowMapSize);

            this.geometryBuffer = new RenderTarget(game, Format.R32G32B32A32_Float, 3);
            this.lightBuffer = new RenderTarget(game, Format.R32G32B32A32_Float, 1);

            this.dirLightInputLayout = new InputLayout(
                game.Graphics.Device,
                DrawerPool.EffectDeferredComposer.DeferredDirectionalLight.GetPassByIndex(0).Description.Signature,
                VertexPosition.Input(BufferSlot));

            this.pointLightInputLayout = new InputLayout(
                game.Graphics.Device,
                DrawerPool.EffectDeferredComposer.DeferredPointLight.GetPassByIndex(0).Description.Signature,
                VertexPosition.Input(BufferSlot));

            this.spotLightInputLayout = new InputLayout(
                game.Graphics.Device,
                DrawerPool.EffectDeferredComposer.DeferredSpotLight.GetPassByIndex(0).Description.Signature,
                VertexPosition.Input(BufferSlot));

            this.combineLightsInputLayout = new InputLayout(
                game.Graphics.Device,
                DrawerPool.EffectDeferredComposer.DeferredCombineLights.GetPassByIndex(0).Description.Signature,
                VertexPosition.Input(BufferSlot));

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
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public virtual void Dispose()
        {
            Helper.Dispose(this.shadowMapperLow);
            Helper.Dispose(this.shadowMapperHigh);
            Helper.Dispose(this.geometryBuffer);
            Helper.Dispose(this.lightBuffer);
            Helper.Dispose(this.lightGeometryVertexBuffer);
            Helper.Dispose(this.lightGeometryIndexBuffer);

            Helper.Dispose(this.dirLightInputLayout);
            Helper.Dispose(this.pointLightInputLayout);
            Helper.Dispose(this.spotLightInputLayout);
            Helper.Dispose(this.combineLightsInputLayout);
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
            this.UpdateContext.EyeDirection = scene.Camera.Direction;
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
                    this.DrawContext.ViewProjection = this.UpdateContext.ViewProjection;
                    this.DrawContext.Frustum = this.UpdateContext.Frustum;
                    this.DrawContext.EyePosition = this.UpdateContext.EyePosition;
                    this.DrawContext.EyeTarget = this.UpdateContext.EyeDirection;
                    //Initialize context data from scene
                    this.DrawContext.Lights = scene.Lights;
                    this.DrawContext.ShadowMaps = 0;
                    this.DrawContext.ShadowMapLow = null;
                    this.DrawContext.ShadowMapHigh = null;
                    this.DrawContext.FromLightViewProjectionLow = Matrix.Identity;
                    this.DrawContext.FromLightViewProjectionHigh = Matrix.Identity;
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
                        Vector3 lightPosition;
                        Vector3 lightDirection;
                        ShadowMap.SetLight(shadowCastingLights[0], scene.Lights.FarLightsDistance, out lightPosition, out lightDirection);

                        ShadowMapFlags flags = ShadowMapFlags.None;

                        this.DrawShadowsContext.GameTime = gameTime;
                        this.DrawShadowsContext.World = this.UpdateContext.World;
                        this.DrawShadowsContext.EyePosition = lightPosition;
                        this.DrawShadowsContext.EyeTarget = lightDirection;
#if DEBUG
                        swShadowsPreparation.Stop();

                        shadowMap_cull = 0;
                        shadowMap_draw = 0;

                        shadowMap_start = swShadowsPreparation.ElapsedTicks;
#endif
                        #endregion

                        #region Shadow map

                        //Draw components if drop shadow (opaque)
                        var shadowObjs = scene.Components.FindAll(c => c.CastShadow == true);
                        if (shadowObjs.Count > 0)
                        {
                            #region Cull

#if DEBUG
                            Stopwatch swCull = Stopwatch.StartNew();
#endif
                            var cntType = this.DrawContext.Frustum.Contains(new BoundingSphere(this.DrawContext.EyePosition, scene.Lights.ShadowLDDistance));

                            shadowObjs.ForEach(o => o.SetCulling(cntType == ContainmentType.Disjoint));
#if DEBUG
                            swCull.Stop();

                            shadowMap_cull += swCull.ElapsedTicks;
#endif
                            #endregion

                            if (shadowObjs.Exists(o => o.Cull == false))
                            {
                                flags |= ShadowMapFlags.LowDefinition;

                                #region Draw

#if DEBUG
                                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                                var fromLightVP = this.shadowMapperLow.GetFromLightViewProjection(
                                    lightPosition,
                                    this.DrawContext.EyePosition,
                                    scene.Lights.ShadowLDDistance);

                                this.DrawShadowsContext.ViewProjection = fromLightVP;

                                this.BindShadowMap(this.shadowMapperLow.Viewport, this.shadowMapperLow.DepthMap);
                                this.DrawShadowsComponents(gameTime, this.DrawShadowsContext, shadowObjs);

                                this.DrawContext.ShadowMapLow = this.shadowMapperLow.Texture;
                                this.DrawContext.FromLightViewProjectionLow = fromLightVP;
#if DEBUG
                                swDraw.Stop();

                                shadowMap_draw += swDraw.ElapsedTicks;
#endif
                                #endregion
                            }

                            #region Cull
#if DEBUG
                            swCull = Stopwatch.StartNew();
#endif
                            cntType = this.DrawContext.Frustum.Contains(new BoundingSphere(this.DrawContext.EyePosition, scene.Lights.ShadowHDDistance));

                            shadowObjs.ForEach(o => o.SetCulling(cntType == ContainmentType.Disjoint));
#if DEBUG
                            swCull.Stop();

                            shadowMap_cull += swCull.ElapsedTicks;
#endif
                            #endregion

                            if (shadowObjs.Exists(o => o.Cull == false))
                            {
                                flags |= ShadowMapFlags.HighDefinition;

                                #region Draw
#if DEBUG
                                Stopwatch swDraw = Stopwatch.StartNew();
#endif
                                Matrix fromLightVP = this.shadowMapperHigh.GetFromLightViewProjection(
                                    lightPosition,
                                    this.DrawContext.EyePosition,
                                    scene.Lights.ShadowHDDistance);

                                this.DrawShadowsContext.ViewProjection = fromLightVP;

                                this.BindShadowMap(this.shadowMapperHigh.Viewport, this.shadowMapperHigh.DepthMap);
                                this.DrawShadowsComponents(gameTime, this.DrawShadowsContext, shadowObjs);

                                this.DrawContext.ShadowMapHigh = this.shadowMapperHigh.Texture;
                                this.DrawContext.FromLightViewProjectionHigh = fromLightVP;
#if DEBUG
                                swDraw.Stop();

                                shadowMap_draw += swDraw.ElapsedTicks;
#endif

                                #endregion
                            }
                        }

                        #endregion

                        //Set shadow map flags to drawing context
                        this.DrawContext.ShadowMaps = (int)flags;
                    }

                    #endregion

                    #region Deferred rendering

                    //Render to G-Buffer only opaque objects
                    var deferredEnabledComponents = visibleComponents.FindAll(c => c.DeferredEnabled);
                    if (deferredEnabledComponents.Count > 0)
                    {
                        #region Cull
#if DEBUG
                        Stopwatch swCull = Stopwatch.StartNew();
#endif
                        bool draw = false;
                        if (scene.PerformFrustumCulling)
                        {
                            //Frustum culling
                            draw = scene.CullTest(this.DrawContext.Frustum, deferredEnabledComponents);
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
                            this.DrawResultComponents(gameTime, this.DrawContext, deferredEnabledComponents, true);
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

                    #endregion

                    #region Forward rendering

                    //Render to screen deferred disabled components
                    var deferredDisabledComponents = visibleComponents.FindAll(c => !c.DeferredEnabled);
                    if (deferredDisabledComponents.Count > 0)
                    {
                        #region Draw deferred disabled components
#if DEBUG
                        Stopwatch swDraw = Stopwatch.StartNew();
#endif
                        //Set forward mode
                        this.DrawContext.DrawerMode = DrawerModesEnum.Forward;

                        //Draw scene
                        this.DrawResultComponents(gameTime, this.DrawContext, deferredDisabledComponents, false);

                        //Set deferred mode
                        this.DrawContext.DrawerMode = DrawerModesEnum.Deferred;
#if DEBUG
                        swDraw.Stop();

                        deferred_draw2D = swDraw.ElapsedTicks;
#endif
                        #endregion
                    }

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
            if (result == SceneRendererResultEnum.ShadowMapStatic) return this.ShadowMapLow;
            if (result == SceneRendererResultEnum.ShadowMapDynamic) return this.ShadowMapHigh;
            if (result == SceneRendererResultEnum.LightMap) return this.LightMap[0];

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

            List<VertexPosition> verts = new List<VertexPosition>();
            List<uint> indx = new List<uint>();

            {
                Vector3[] cv;
                uint[] indices;
                GeometryUtil.CreateScreen(
                    Game.Form,
                    out cv,
                    out indices);
                var vertices = new VertexPosition[cv.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new VertexPosition() { Position = cv[i] };
                }

                this.screenGeometry.Offset = indx.Count;
                this.screenGeometry.IndexCount = indices.Length;

                verts.AddRange(vertices);
                indx.AddRange(indices);
            }

            {
                Vector3[] cv;
                uint[] indices;
                GeometryUtil.CreateSphere(
                    1, 16, 16,
                    out cv,
                    out indices);
                var vertices = new VertexPosition[cv.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new VertexPosition() { Position = cv[i] };
                }

                this.pointLightGeometry.Offset = indx.Count;
                this.pointLightGeometry.IndexCount = indices.Length;

                //Sum offsets
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] += (uint)verts.Count;
                }

                verts.AddRange(vertices);
                indx.AddRange(indices);
            }

            {
                Vector3[] cv;
                uint[] indices;
                GeometryUtil.CreateSphere(
                    1, 16, 16,
                    out cv,
                    out indices);
                var vertices = new VertexPosition[cv.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new VertexPosition() { Position = cv[i] };
                }

                this.spotLightGeometry.Offset = indx.Count;
                this.spotLightGeometry.IndexCount = indices.Length;

                //Sum offsets
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] += (uint)verts.Count;
                }

                verts.AddRange(vertices);
                indx.AddRange(indices);
            }

            if (this.lightGeometryVertexBuffer == null)
            {
                this.lightGeometryVertexBuffer = this.Game.Graphics.Device.CreateVertexBufferWrite("Deferred Redenderer Light Geometry", verts.ToArray());
                this.lightGeometryVertexBufferBinding = new VertexBufferBinding(this.lightGeometryVertexBuffer, verts[0].GetStride(), 0);
            }
            else
            {
                this.Game.Graphics.DeviceContext.WriteDiscardBuffer(this.lightGeometryVertexBuffer, verts.ToArray());
            }

            if (this.lightGeometryIndexBuffer == null)
            {
                this.lightGeometryIndexBuffer = this.Game.Graphics.Device.CreateIndexBufferWrite("Deferred Redenderer Light Geometry", indx.ToArray());
            }
            else
            {
                this.Game.Graphics.DeviceContext.WriteDiscardBuffer(this.lightGeometryIndexBuffer, indx.ToArray());
            }
        }
        /// <summary>
        /// Binds graphics for shadow mapping pass
        /// </summary>
        /// <param name="viewport">Viewport</param>
        /// <param name="dsv">Deph stencil buffer</param>
        private void BindShadowMap(Viewport viewport, DepthStencilView dsv)
        {
            //Set shadow mapper viewport
            this.Game.Graphics.SetViewport(viewport);

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
                this.geometryBuffer.Targets, true, Color.Black,
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
            this.Game.Graphics.SetRenderTargets(
                this.lightBuffer.Targets, true, Color.Black,
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
            Counters.MaxInstancesPerFrame += context.Lights.DirectionalLights.Length;
            Counters.MaxInstancesPerFrame += context.Lights.PointLights.Length;
            Counters.MaxInstancesPerFrame += context.Lights.SpotLights.Length;

            #region Initialization
#if DEBUG
            Stopwatch swPrepare = Stopwatch.StartNew();
#endif
            var effect = DrawerPool.EffectDeferredComposer;

            var directionalLights = context.Lights.GetVisibleDirectionalLights();
            var spotLights = context.Lights.GetVisibleSpotLights();
            var pointLights = context.Lights.GetVisiblePointLights();

            effect.UpdatePerFrame(
                context.World,
                this.ViewProjection,
                context.EyePosition,
                this.GeometryMap[0],
                this.GeometryMap[1],
                this.GeometryMap[2]);

            this.Game.Graphics.SetDepthStencilRDZDisabled();
            this.Game.Graphics.SetBlendDeferredLighting();

            this.Game.Graphics.IAPrimitiveTopology = PrimitiveTopology.TriangleList;
            this.Game.Graphics.IASetVertexBuffers(BufferSlot, this.lightGeometryVertexBufferBinding);
            this.Game.Graphics.IASetIndexBuffer(this.lightGeometryIndexBuffer, Format.R32_UInt, 0);

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

                this.Game.Graphics.IAInputLayout = this.dirLightInputLayout;
                Counters.IAInputLayoutSets++;

                for (int i = 0; i < directionalLights.Length; i++)
                {
                    effect.UpdatePerLight(
                        directionalLights[i],
                        context.FromLightViewProjectionLow,
                        context.FromLightViewProjectionHigh,
                        context.ShadowMaps,
                        context.ShadowMapLow,
                        context.ShadowMapHigh);

                    for (int p = 0; p < effectTechnique.Description.PassCount; p++)
                    {
                        effectTechnique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                        this.Game.Graphics.DeviceContext.DrawIndexed(
                            this.screenGeometry.IndexCount,
                            this.screenGeometry.Offset,
                            0);

                        Counters.DrawCallsPerFrame++;
                    }
                }
            }
#if DEBUG
            swDirectional.Stop();
#endif
            #endregion

            #region Point Lights
#if DEBUG
            Stopwatch swPoint = Stopwatch.StartNew();
#endif
            if (pointLights != null && pointLights.Length > 0)
            {
                var geometry = this.pointLightGeometry;

                this.Game.Graphics.IAInputLayout = this.pointLightInputLayout;
                Counters.IAInputLayoutSets++;

                for (int i = 0; i < pointLights.Length; i++)
                {
                    var light = pointLights[i];

                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        context.World * light.Local,
                        context.ViewProjection);

                    this.Game.Graphics.SetRasterizerStencilPass();
                    this.Game.Graphics.SetDepthStencilVolumeMarking();
                    this.Game.Graphics.ClearDepthStencilBuffer(this.Game.Graphics.DefaultDepthStencil, DepthStencilClearFlags.Stencil);
                    this.DrawSingleLight(geometry, effect, effect.DeferredPointStencil);

                    this.Game.Graphics.SetRasterizerLightingPass();
                    this.Game.Graphics.SetDepthStencilVolumeDrawing(0);
                    this.Game.Graphics.DeviceContext.OutputMerger.DepthStencilReference = 0;
                    this.DrawSingleLight(geometry, effect, effect.DeferredPointLight);
                }
            }
#if DEBUG
            swPoint.Stop();
#endif
            #endregion

            #region Spot Lights
#if DEBUG
            Stopwatch swSpot = Stopwatch.StartNew();
#endif
            if (spotLights != null && spotLights.Length > 0)
            {
                var geometry = this.spotLightGeometry;

                this.Game.Graphics.IAInputLayout = this.spotLightInputLayout;
                Counters.IAInputLayoutSets++;

                for (int i = 0; i < spotLights.Length; i++)
                {
                    var light = spotLights[i];

                    //Draw Pass
                    effect.UpdatePerLight(
                        light,
                        context.World * light.Local,
                        context.ViewProjection);

                    this.Game.Graphics.SetRasterizerStencilPass();
                    this.Game.Graphics.SetDepthStencilVolumeMarking();
                    this.Game.Graphics.ClearDepthStencilBuffer(this.Game.Graphics.DefaultDepthStencil, DepthStencilClearFlags.Stencil);
                    this.DrawSingleLight(geometry, effect, effect.DeferredSpotStencil);

                    this.Game.Graphics.SetRasterizerLightingPass();
                    this.Game.Graphics.SetDepthStencilVolumeDrawing(0);
                    this.DrawSingleLight(geometry, effect, effect.DeferredSpotLight);
                }
            }
#if DEBUG
            swSpot.Stop();
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
        /// Draws a single light
        /// </summary>
        /// <param name="geometry">Geometry</param>
        /// <param name="effect">Effect</param>
        /// <param name="effectTechnique">Technique</param>
        private void DrawSingleLight(LightGeometry geometry, EffectDeferredComposer effect, EffectTechnique effectTechnique)
        {
            for (int p = 0; p < effectTechnique.Description.PassCount; p++)
            {
                effectTechnique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                this.Game.Graphics.DeviceContext.DrawIndexed(geometry.IndexCount, geometry.Offset, 0);

                Counters.DrawCallsPerFrame++;
            }
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
                var effect = DrawerPool.EffectDeferredComposer;
                var effectTechnique = effect.DeferredCombineLights;

                effect.UpdateComposer(
                    context.World,
                    this.ViewProjection,
                    context.EyePosition,
                    context.Lights.GlobalAmbientLight,
                    context.Lights.FogStart,
                    context.Lights.FogRange,
                    context.Lights.FogColor,
                    this.GeometryMap[2],
                    this.LightMap[0]);

                this.Game.Graphics.IAInputLayout = this.combineLightsInputLayout;
                this.Game.Graphics.IAPrimitiveTopology = PrimitiveTopology.TriangleList;
                this.Game.Graphics.IASetVertexBuffers(BufferSlot, this.lightGeometryVertexBufferBinding);
                this.Game.Graphics.IASetIndexBuffer(this.lightGeometryIndexBuffer, Format.R32_UInt, 0);

                this.Game.Graphics.SetDepthStencilNone();
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
                    effectTechnique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.DrawIndexed(this.screenGeometry.IndexCount, this.screenGeometry.Offset, 0);

                    Counters.DrawCallsPerFrame++;
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
            components.ForEach((c) =>
            {
                if (!c.Cull)
                {
                    this.Game.Graphics.SetRasterizerCullFrontFace();

                    if (c.DepthEnabled) this.Game.Graphics.SetDepthStencilZEnabled();
                    else this.Game.Graphics.SetDepthStencilZDisabled();

                    if (c.AlphaEnabled) this.Game.Graphics.SetBlendTransparent();
                    else this.Game.Graphics.SetBlendDefault();

                    c.Draw(context);
                }
            });
        }
        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        /// <param name="components">Components</param>
        /// <param name="deferred">Deferred drawing</param>
        private void DrawResultComponents(GameTime gameTime, DrawContext context, List<Drawable> components, bool deferred)
        {
            components.ForEach((c) =>
            {
                Counters.MaxInstancesPerFrame += c.MaxInstances;

                if (!c.Cull)
                {
                    this.Game.Graphics.SetRasterizerDefault();

                    if (c.DepthEnabled) this.Game.Graphics.SetDepthStencilZEnabled();
                    else this.Game.Graphics.SetDepthStencilZDisabled();

                    if (deferred)
                    {
                        if (c.AlphaEnabled) this.Game.Graphics.SetBlendDeferredComposerTransparent();
                        else this.Game.Graphics.SetBlendDeferredComposer();
                    }
                    else
                    {
                        if (c.AlphaEnabled) this.Game.Graphics.SetBlendTransparent();
                        else this.Game.Graphics.SetBlendDefault();
                    }

                    c.Draw(context);
                }
            });
        }
    }
}
