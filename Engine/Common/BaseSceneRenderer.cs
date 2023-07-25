﻿#if DEBUG
using System.Diagnostics;
#endif
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.PostProcess;

    /// <summary>
    /// Base scene renderer
    /// </summary>
    public abstract class BaseSceneRenderer : ISceneRenderer
    {
        /// <summary>
        /// Render targets
        /// </summary>
        protected enum Targets
        {
            /// <summary>
            /// Objects target
            /// </summary>
            /// <remarks>
            /// All scene objects and their post-processing effects are drawn to this target
            /// </remarks>
            Objects,
            /// <summary>
            /// UI target
            /// </summary>
            /// <remarks>
            /// All user interface components and their post-processing effects are drawn to this target
            /// </remarks>
            UI,
            /// <summary>
            /// Results target
            /// </summary>
            Result,
            /// <summary>
            /// Screen
            /// </summary>
            Screen,
        }

        /// <summary>
        /// Post-processing drawer
        /// </summary>
        private readonly IPostProcessingDrawer processingDrawer = null;
        /// <summary>
        /// Scene objects target
        /// </summary>
        private RenderTarget sceneObjectsTarget = null;
        /// <summary>
        /// Scene UI target
        /// </summary>
        private RenderTarget sceneUITarget = null;
        /// <summary>
        /// Scene results target
        /// </summary>
        private RenderTarget sceneResultsTarget = null;
        /// <summary>
        /// Post-processing render target A
        /// </summary>
        private RenderTarget postProcessingTargetA = null;
        /// <summary>
        /// Post-processing render target B
        /// </summary>
        private RenderTarget postProcessingTargetB = null;

        /// <summary>
        /// Deferred context list
        /// </summary>
        private readonly List<EngineDeviceContext> deferredContextList = new();

        /// <summary>
        /// Update globals
        /// </summary>
        private bool updateGlobals = true;
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

#if DEBUG
        /// <summary>
        /// Shadow mapping stats dictionary
        /// </summary>
        private readonly Dictionary<string, double> shadowMappingDict = new();
#endif

        /// <summary>
        /// Shadow map size
        /// </summary>
        protected const int DirectionalShadowMapSize = 1024 * 4;
        /// <summary>
        /// Maximum number of directional shadow maps
        /// </summary>
        protected const int MaxDirectionalShadowMaps = 1;
        /// <summary>
        /// Maximum number of cascade shadow maps per directional light
        /// </summary>
        protected const int MaxDirectionalCascadeShadowMaps = 3;

        /// <summary>
        /// Cubic shadow map size
        /// </summary>
        protected const int CubicShadowMapSize = 1024;
        /// <summary>
        /// Maximum number of cubic shadow maps
        /// </summary>
        protected const int MaxCubicShadows = 16;

        /// <summary>
        /// Spot light shadow map size
        /// </summary>
        protected const int SpotShadowMapSize = 1024;
        /// <summary>
        /// Max spot shadows
        /// </summary>
        protected const int MaxSpotShadows = 16;

        /// <summary>
        /// Cull index for drawing
        /// </summary>
        protected const int CullIndexDrawIndex = 0;
        /// <summary>
        /// Cull index for low definition shadows
        /// </summary>
        protected const int CullIndexShadowMaps = 100;

        /// <summary>
        /// Shadow mapper for directional lights
        /// </summary>
        protected IShadowMap ShadowMapperDirectional { get; private set; }
        /// <summary>
        /// Cube shadow mapper for point lights
        /// </summary>
        protected IShadowMap ShadowMapperPoint { get; private set; }
        /// <summary>
        /// Shadow mapper for spot lights
        /// </summary>
        protected IShadowMap ShadowMapperSpot { get; private set; }

        /// <summary>
        /// Scene
        /// </summary>
        protected Scene Scene;
        /// <summary>
        /// Renderer width
        /// </summary>
        protected int Width;
        /// <summary>
        /// Renderer height
        /// </summary>
        protected int Height;
        /// <summary>
        /// Update context
        /// </summary>
        protected UpdateContext UpdateContext = null;
        /// <summary>
        /// Cull manager
        /// </summary>
        protected SceneCullManager cullManager = null;
        /// <summary>
        /// Directional shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapDirectional
        {
            get
            {
                return ShadowMapperDirectional?.Texture;
            }
        }
        /// <summary>
        /// Point lights shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapPoint
        {
            get
            {
                return ShadowMapperPoint?.Texture;
            }
        }
        /// <summary>
        /// Spot lights shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapSpot
        {
            get
            {
                return ShadowMapperSpot?.Texture;
            }
        }
        /// <summary>
        /// Gets or sets whether the renderer was updated
        /// </summary>
        protected bool Updated { get; set; }

        /// <inheritdoc/>
        public bool PostProcessingEnabled
        {
            get
            {
                if (PostProcessingObjectsEffects?.Ready == true)
                {
                    return true;
                }

                if (PostProcessingUIEffects?.Ready == true)
                {
                    return true;
                }

                if (PostProcessingFinalEffects?.Ready == true)
                {
                    return true;
                }

                return false;
            }
        }
        /// <inheritdoc/>
        public BuiltInPostProcessState PostProcessingObjectsEffects { get; set; } = BuiltInPostProcessState.Empty;
        /// <inheritdoc/>
        public BuiltInPostProcessState PostProcessingUIEffects { get; set; } = BuiltInPostProcessState.Empty;
        /// <inheritdoc/>
        public BuiltInPostProcessState PostProcessingFinalEffects { get; set; } = BuiltInPostProcessState.Empty;

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
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        protected BaseSceneRenderer(Scene scene)
        {
            Scene = scene;

            // Directional shadow mapper
            ShadowMapperDirectional = new ShadowMapCascade(
                scene,
                "Directional Shadow Mapper",
                DirectionalShadowMapSize,
                MaxDirectionalCascadeShadowMaps, MaxDirectionalShadowMaps,
                scene.GameEnvironment.CascadeShadowMapsDistances)
            {
                HighResolutionMap = true
            };

            // Point shadow mapper
            ShadowMapperPoint = new ShadowMapPoint(
                scene,
                "Point Shadow Mapper",
                CubicShadowMapSize, CubicShadowMapSize,
                MaxCubicShadows)
            {
                HighResolutionMap = true
            };

            // Spot shadow mapper
            ShadowMapperSpot = new ShadowMapSpot(
                scene,
                "Spot Shadow Mapper",
                SpotShadowMapSize, SpotShadowMapSize,
                MaxSpotShadows)
            {
                HighResolutionMap = true
            };

            cullManager = new SceneCullManager();

            UpdateContext = new UpdateContext()
            {
                Name = "Primary",
            };

            var targetFormat = SharpDX.DXGI.Format.R32G32B32A32_Float;

            sceneObjectsTarget = new RenderTarget(scene.Game, "SceneObjectsTarget", targetFormat, false, 1);
            sceneUITarget = new RenderTarget(scene.Game, "SceneUITarget", targetFormat, false, 1);
            sceneResultsTarget = new RenderTarget(scene.Game, "SceneResultsTarget", targetFormat, false, 1);

            postProcessingTargetA = new RenderTarget(scene.Game, "PostProcessingTargetA", targetFormat, false, 1);
            postProcessingTargetB = new RenderTarget(scene.Game, "PostProcessingTargetB", targetFormat, false, 1);
            processingDrawer = new PostProcessingDrawer(scene.Game);

            updateMaterialsPalette = true;
            updateAnimationsPalette = true;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BaseSceneRenderer()
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
                ShadowMapperDirectional?.Dispose();
                ShadowMapperDirectional = null;

                ShadowMapperPoint?.Dispose();
                ShadowMapperPoint = null;

                ShadowMapperSpot?.Dispose();
                ShadowMapperSpot = null;

                sceneObjectsTarget?.Dispose();
                sceneObjectsTarget = null;
                sceneUITarget?.Dispose();
                sceneUITarget = null;
                sceneResultsTarget?.Dispose();
                sceneResultsTarget = null;
                postProcessingTargetA?.Dispose();
                postProcessingTargetA = null;
                postProcessingTargetB?.Dispose();
                postProcessingTargetB = null;

                deferredContextList.ForEach(dc => dc.Dispose());
                deferredContextList.Clear();
            }
        }

        /// <inheritdoc/>
        public virtual void Resize()
        {
            sceneObjectsTarget?.Resize();
            sceneUITarget?.Resize();
            sceneResultsTarget?.Resize();
            postProcessingTargetA?.Resize();
            postProcessingTargetB?.Resize();
            processingDrawer?.Resize();
        }
        /// <inheritdoc/>
        public virtual EngineShaderResourceView GetResource(SceneRendererResults result)
        {
            if (result == SceneRendererResults.ShadowMapDirectional) return ShadowMapDirectional;
            if (result == SceneRendererResults.ShadowMapPoint) return ShadowMapPoint;
            if (result == SceneRendererResults.ShadowMapSpot) return ShadowMapSpot;
            return null;
        }

        /// <inheritdoc/>
        public virtual void Update(GameTime gameTime)
        {
            //Updates the update context
            UpdateUpdateContext(gameTime);

            //Cull lights
            Scene.Lights.Cull(UpdateContext.CameraVolume, UpdateContext.EyePosition, Scene.GameEnvironment.LODDistanceLow);

            //Update active components
            var updatables = Scene.Components.Get<IUpdatable>(c => c.Active);
            if (updatables.Any())
            {
                updatables
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(c => c.EarlyUpdate(UpdateContext));

                updatables
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(c => c.Update(UpdateContext));

                updatables
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(c => c.LateUpdate(UpdateContext));
            }

            Updated = true;
        }
        /// <summary>
        /// Updates the update context
        /// </summary>
        /// <param name="gameTime">Game time</param>
        protected virtual void UpdateUpdateContext(GameTime gameTime)
        {
            UpdateContext.GameTime = gameTime;

            Matrix viewProj = Scene.Camera.View * Scene.Camera.Projection;
            UpdateContext.View = Scene.Camera.View;
            UpdateContext.Projection = Scene.Camera.Projection;
            UpdateContext.ViewProjection = viewProj;
            UpdateContext.CameraVolume = new IntersectionVolumeFrustum(viewProj);
            UpdateContext.NearPlaneDistance = Scene.Camera.NearPlaneDistance;
            UpdateContext.FarPlaneDistance = Scene.Camera.FarPlaneDistance;
            UpdateContext.EyePosition = Scene.Camera.Position;
            UpdateContext.EyeDirection = Scene.Camera.Direction;

            UpdateContext.Lights = Scene.Lights;
        }

        /// <summary>
        /// Gets the immediate draw context
        /// </summary>
        protected virtual DrawContext GetImmediateDrawContext(GameTime gameTime, DrawerModes drawMode, IEngineForm form)
        {
            return new DrawContext
            {
                Name = $"Immediate context.",

                GameTime = gameTime,
                DrawerMode = drawMode,
                Form = form,

                //Initialize context data from update context
                ViewProjection = UpdateContext.ViewProjection,
                CameraVolume = UpdateContext.CameraVolume,
                EyePosition = UpdateContext.EyePosition,
                EyeDirection = UpdateContext.EyeDirection,

                //Initialize context data from scene
                Lights = Scene.Lights,
                LevelOfDetail = new Vector3(Scene.GameEnvironment.LODDistanceHigh, Scene.GameEnvironment.LODDistanceMedium, Scene.GameEnvironment.LODDistanceLow),
                ShadowMapDirectional = ShadowMapperDirectional,
                ShadowMapPoint = ShadowMapperPoint,
                ShadowMapSpot = ShadowMapperSpot,

                //Device context
                DeviceContext = Scene.Game.Graphics.ImmediateContext,
            };
        }
        /// <summary>
        /// Gets a deferred draw context
        /// </summary>
        protected virtual DrawContext GetDeferredDrawContext(DrawContext context, int passIndex)
        {
            return new DrawContext
            {
                Name = $"Deferred pass[{passIndex}] context.",

                GameTime = context.GameTime,
                DrawerMode = context.DrawerMode,
                Form = context.Form,

                //Initialize context data from update context
                ViewProjection = UpdateContext.ViewProjection,
                CameraVolume = UpdateContext.CameraVolume,
                EyePosition = UpdateContext.EyePosition,
                EyeDirection = UpdateContext.EyeDirection,

                //Initialize context data from scene
                Lights = Scene.Lights,
                LevelOfDetail = new Vector3(Scene.GameEnvironment.LODDistanceHigh, Scene.GameEnvironment.LODDistanceMedium, Scene.GameEnvironment.LODDistanceLow),
                ShadowMapDirectional = ShadowMapperDirectional,
                ShadowMapPoint = ShadowMapperPoint,
                ShadowMapSpot = ShadowMapperSpot,

                //Device context
                DeviceContext = GetDeferredContext(passIndex),
            };
        }
        /// <summary>
        /// Gets per light draw context
        /// </summary>
        protected virtual DrawContextShadows GetPerLightDrawContext(int passIndex, IShadowMap shadowMapper, ISceneLight light, int lightIndex)
        {
            var dc = GetDeferredContext(passIndex);

            shadowMapper.UpdateFromLightViewProjection(Scene.Camera, light);
            shadowMapper.Bind(dc, lightIndex);

            return new DrawContextShadows()
            {
                Name = $"Per light pass[{passIndex}] context.",

                ViewProjection = shadowMapper.ToShadowMatrix,
                EyePosition = shadowMapper.LightPosition,
                Frustum = Scene.Camera.Frustum,
                ShadowMap = shadowMapper,

                DeviceContext = dc,
            };
        }
        /// <summary>
        /// Creates a deferred context
        /// </summary>
        private EngineDeviceContext GetDeferredContext(int passIndex)
        {
            while (passIndex >= deferredContextList.Count)
            {
                deferredContextList.Add(Scene.Game.Graphics.CreateDeferredContext($"Deferred Context {passIndex}"));
            }

            return deferredContextList[passIndex];
        }

        /// <inheritdoc/>
        public abstract void Draw(GameTime gameTime);

        /// <summary>
        /// Gets opaque components
        /// </summary>
        /// <param name="index">Cull index</param>
        /// <param name="components">Component list</param>
        /// <returns>Returns the opaque components</returns>
        protected virtual List<IDrawable> GetOpaques(int index, IEnumerable<IDrawable> components)
        {
            var opaques = components.Where(c =>
            {
                if (c is null) return false;

                if (!c.BlendMode.HasFlag(BlendModes.Opaque)) return false;

                if (c is ICullable cull)
                {
                    return !cullManager.GetCullValue(index, cull).Culled;
                }

                return true;
            });

            return opaques.ToList();
        }
        /// <summary>
        /// Sorting opaque list comparer
        /// </summary>
        /// <param name="index">Cull index</param>
        /// <param name="c1">First component</param>
        /// <param name="c2">Second component</param>
        /// <returns>Returns sorting order (nearest first)</returns>
        protected virtual int SortOpaques(int index, IDrawable c1, IDrawable c2)
        {
            int res = c1.Layer.CompareTo(c2.Layer);

            if (res == 0)
            {
                res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
            }

            if (res == 0)
            {
                float d1 = float.MaxValue;
                if (c1 is ICullable cull1)
                {
                    d1 = cullManager.GetCullValue(index, cull1).Distance;
                }

                float d2 = float.MaxValue;
                if (c2 is ICullable cull2)
                {
                    d2 = cullManager.GetCullValue(index, cull2).Distance;
                }

                // Nearest first
                res = -d1.CompareTo(d2);
            }

            if (res == 0)
            {
                res = c1.BlendMode.CompareTo(c2.BlendMode);
            }

            return res;
        }
        /// <summary>
        /// Gets transparent components
        /// </summary>
        /// <param name="index">Cull index</param>
        /// <param name="components">Component list</param>
        /// <returns>Returns the transparent components</returns>
        protected virtual List<IDrawable> GetTransparents(int index, IEnumerable<IDrawable> components)
        {
            var transparents = components.Where(c =>
            {
                if (c is null) return false;

                if (!c.BlendMode.HasFlag(BlendModes.Alpha) && !c.BlendMode.HasFlag(BlendModes.Transparent))
                {
                    return false;
                }

                if (c is ICullable cull)
                {
                    return !cullManager.GetCullValue(index, cull).Culled;
                }

                return true;
            });

            return transparents.ToList();
        }
        /// <summary>
        /// Sorting transparent list comparer
        /// </summary>
        /// <param name="index">Cull index</param>
        /// <param name="c1">First component</param>
        /// <param name="c2">Second component</param>
        /// <returns>Returns sorting order (far first)</returns>
        protected virtual int SortTransparents(int index, IDrawable c1, IDrawable c2)
        {
            int res = c1.Layer.CompareTo(c2.Layer);

            if (res == 0)
            {
                res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
            }

            if (res == 0)
            {
                float d1 = float.MaxValue;
                if (c1 is ICullable cull1)
                {
                    d1 = cullManager.GetCullValue(index, cull1).Distance;
                }

                float d2 = float.MaxValue;
                if (c2 is ICullable cull2)
                {
                    d2 = cullManager.GetCullValue(index, cull2).Distance;
                }

                // Far objects first
                res = d1.CompareTo(d2);
            }

            if (res == 0)
            {
                res = c1.BlendMode.CompareTo(c2.BlendMode);
            }

            return res;
        }

        /// <summary>
        /// Updates the global resources state
        /// </summary>
        /// <param name="dc">Device context</param>
        protected void UpdateGlobalState(EngineDeviceContext dc)
        {
            ShadowMapperDirectional?.UpdateGlobals();
            ShadowMapperPoint?.UpdateGlobals();
            ShadowMapperSpot?.UpdateGlobals();

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
                BuiltInShaders.UpdateGlobals(dc, materialPalette, materialPaletteWidth, animationPalette, animationPaletteWidth);

                updateGlobals = false;
            }
        }

        /// <inheritdoc/>
        public virtual void UpdateGlobals(bool updatedEnvironment, bool updatedComponents)
        {
            updateMaterialsPalette = updateMaterialsPalette || updatedComponents;
            updateAnimationsPalette = updateAnimationsPalette || updatedComponents;
            updateGlobals = updateGlobals || updatedEnvironment;
        }
        /// <summary>
        /// Updates the materials palette
        /// </summary>
        public virtual void UpdateMaterialPalette()
        {
            updateMaterialsPalette = true;
        }
        /// <summary>
        /// Updates the global material palette
        /// </summary>
        /// <param name="materialPalette">Material palette</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        private void UpdateMaterialPalette(out EngineShaderResourceView materialPalette, out uint materialPaletteWidth)
        {
            List<IMeshMaterial> mats = new()
            {
                MeshMaterial.DefaultBlinnPhong,
            };

            var matComponents = Scene.Components.Get<IUseMaterials>().SelectMany(c => c.GetMaterials());
            if (matComponents.Any())
            {
                mats.AddRange(matComponents);
            }

            List<Vector4> values = new();

            for (int i = 0; i < mats.Count; i++)
            {
                var mat = mats[i];
                var matV = mat.Material.Convert().Pack();

                mat.UpdateResource((uint)i, (uint)values.Count, (uint)matV.Length);

                values.AddRange(matV);
            }

            int texWidth = GetTextureSize(values.Count);

            materialPalette = Scene.Game.ResourceManager.CreateGlobalResource("MaterialPalette", values, texWidth);
            materialPaletteWidth = (uint)texWidth;
        }
        /// <summary>
        /// Updates the global animation palette
        /// </summary>
        /// <param name="animationPalette">Animation palette</param>
        /// <param name="animationPaletteWidth">Animation palette width</param>
        private void UpdateAnimationPalette(out EngineShaderResourceView animationPalette, out uint animationPaletteWidth)
        {
            var skData = Scene.Components.Get<IUseSkinningData>(c => c.SkinningData != null)
                .Select(c => c.SkinningData)
                .ToArray();

            List<ISkinningData> addedSks = new();

            List<Vector4> values = new();

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

            animationPalette = Scene.Game.ResourceManager.CreateGlobalResource("AnimationPalette", values.ToArray(), texWidth);
            animationPaletteWidth = (uint)texWidth;
        }

        /// <summary>
        /// Performs the culling test
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="components">Components collection to test</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns>Returns true if the test find components to draw</returns>
        protected virtual bool CullingTest(Scene scene, ICullingVolume volume, IEnumerable<ICullable> components, int cullIndex)
        {
            if (!components.Any())
            {
                return false;
            }

            //Frustum culling
            bool draw = cullManager.Cull(volume, cullIndex, components) || scene.PerformFrustumCulling;

            if (draw)
            {
                var groundVolume = scene.GetSceneVolume();
                if (groundVolume != null)
                {
                    //Ground culling
                    draw = cullManager.Cull(groundVolume, cullIndex, components);
                }
            }

            return draw;
        }

        /// <summary>
        /// Draws an object
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="drawable">Drawable component</param>
        protected virtual bool Draw(DrawContext context, IDrawable drawable)
        {
            Counters.MaxInstancesPerFrame += drawable.InstanceCount;

            var blend = drawable.BlendMode;
            if (drawable.Usage.HasFlag(SceneObjectUsages.UI))
            {
                blend |= BlendModes.PostProcess;
            }

            var dc = context.DeviceContext;

            SetRasterizer(dc);
            SetBlendState(dc, context.DrawerMode, blend);
            SetDepthStencil(dc, drawable.DepthEnabled);

            return drawable.Draw(context);
        }
        /// <summary>
        /// Sets the rasterizer state
        /// </summary>
        /// <param name="dc">Device context</param>
        protected virtual void SetRasterizer(EngineDeviceContext dc)
        {
            dc.SetRasterizerState(Scene.Game.Graphics.GetRasterizerDefault());
        }
        /// <summary>
        /// Sets the blend state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="drawerMode">Draw mode</param>
        /// <param name="blendMode">Blend mode</param>
        protected virtual void SetBlendState(EngineDeviceContext dc, DrawerModes drawerMode, BlendModes blendMode)
        {
            dc.SetBlendState(Scene.Game.Graphics.GetBlendState(blendMode));
        }
        /// <summary>
        /// Sets the depth-stencil buffer state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="enableWrite">Enables the z-buffer writing</param>
        protected virtual void SetDepthStencil(EngineDeviceContext dc, bool enableWrite)
        {
            if (enableWrite)
            {
                dc.SetDepthStencilState(Scene.Game.Graphics.GetDepthStencilWRZEnabled());
            }
            else
            {
                dc.SetDepthStencilState(Scene.Game.Graphics.GetDepthStencilWRZDisabled());
            }
        }

        /// <summary>
        /// Draw shadow maps
        /// </summary>
        /// <param name="context">Drawing context</param>
        protected virtual void DoShadowMapping(DrawContext context, ref int passIndex)
        {
#if DEBUG
            shadowMappingDict.Clear();
#endif

            int cullIndex = CullIndexShadowMaps;

            DoDirectionalShadowMapping(context, ref passIndex, ref cullIndex);

            DoPointShadowMapping(context, ref passIndex, ref cullIndex);

            DoSpotShadowMapping(context, ref passIndex, ref cullIndex);

#if DEBUG
            if (Scene.Game.CollectGameStatus)
            {
                Scene.Game.GameStatus.Add(shadowMappingDict);
            }
#endif
        }
        /// <summary>
        /// Draw directional shadow maps
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="passIndex">Pass index</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoDirectionalShadowMapping(DrawContext context, ref int passIndex, ref int cullIndex)
        {
#if DEBUG
            var gStopwatch = Stopwatch.StartNew();
            var stopwatch = Stopwatch.StartNew();
#endif
            var shadowCastingLights = Scene.Lights.GetDirectionalShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
#if DEBUG
            stopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoDirectionalShadowMapping)} Getting lights", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowCastingLights.Any())
            {
                return;
            }

            //Objects that cast shadows
#if DEBUG
            stopwatch.Restart();
#endif
            var shadowObjs = Scene.Components.Get<IDrawable>(c => c.Visible && c.CastShadow.HasFlag(ShadowCastingAlgorihtms.Directional));
#if DEBUG
            stopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoDirectionalShadowMapping)} Getting components", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowObjs.Any())
            {
                return;
            }

            //All objects suitable for culling
            var toCullShadowObjs = shadowObjs.OfType<ICullable>();
            bool allCullingObjects = shadowObjs.Count() == toCullShadowObjs.Count();

            var camVolume = context.CameraVolume;
            var shadowSph = new IntersectionVolumeSphere(camVolume.Position, camVolume.Radius);
            if (!DoShadowCullingTest(toCullShadowObjs, 0, shadowSph, cullIndex, allCullingObjects))
            {
                return;
            }

            int assigned = 0;

            int l = 0;
            foreach (var light in shadowCastingLights)
            {
                light.ClearShadowParameters();

                if (assigned >= MaxDirectionalShadowMaps)
                {
                    continue;
                }

                //Draw shadows
#if DEBUG
                stopwatch.Restart();
#endif
                var drawContext = GetPerLightDrawContext(passIndex, ShadowMapperDirectional, light, assigned * MaxDirectionalCascadeShadowMaps);
                DrawShadowComponents(drawContext, cullIndex, shadowObjs);
#if DEBUG
                stopwatch.Stop();
                shadowMappingDict.Add($"{nameof(DoDirectionalShadowMapping)} {l} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

                //Assign light parameters
                light.ShadowMapIndex = assigned;
                light.ShadowMapCount++;

                passIndex++;

                assigned++;

                cullIndex++;

                l++;
            }

#if DEBUG
            gStopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoDirectionalShadowMapping)} TOTAL", gStopwatch.Elapsed.TotalMilliseconds);
#endif
        }
        /// <summary>
        /// Draw point light shadow maps
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoPointShadowMapping(DrawContext context, ref int passIndex, ref int cullIndex)
        {
#if DEBUG
            var gStopwatch = Stopwatch.StartNew();
            var stopwatch = Stopwatch.StartNew();
#endif
            var shadowCastingLights = Scene.Lights.GetPointShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
#if DEBUG
            stopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoPointShadowMapping)} Getting lights", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowCastingLights.Any())
            {
                return;
            }

            //Draw components if drop shadow (opaque)
#if DEBUG
            stopwatch.Restart();
#endif
            var shadowObjs = Scene.Components.Get<IDrawable>(c => c.Visible && c.CastShadow.HasFlag(ShadowCastingAlgorihtms.Point));
#if DEBUG
            stopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoPointShadowMapping)} Getting components", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowObjs.Any())
            {
                return;
            }

            //All objects suitable for culling
            var toCullShadowObjs = shadowObjs.OfType<ICullable>();
            bool allCullingObjects = shadowObjs.Count() == toCullShadowObjs.Count();

            int assigned = 0;

            int l = 0;
            foreach (var light in shadowCastingLights)
            {
                light.ClearShadowParameters();

                if (assigned >= MaxCubicShadows)
                {
                    continue;
                }

                //Cull test
                if (!DoShadowCullingTest(toCullShadowObjs, l, new IntersectionVolumeSphere(light.Position, light.Radius), cullIndex, allCullingObjects))
                {
                    continue;
                }

                //Draw shadows
#if DEBUG
                stopwatch.Restart();
#endif
                var drawShadowsContext = GetPerLightDrawContext(passIndex, ShadowMapperPoint, light, assigned);
                DrawShadowComponents(drawShadowsContext, cullIndex, shadowObjs);
#if DEBUG
                stopwatch.Stop();
                shadowMappingDict.Add($"{nameof(DoPointShadowMapping)} {l} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

                //Assign light parameters
                light.ShadowMapIndex = assigned;

                passIndex++;

                assigned++;

                cullIndex++;

                l++;
            }

#if DEBUG
            gStopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoPointShadowMapping)} TOTAL", gStopwatch.Elapsed.TotalMilliseconds);
#endif
        }
        /// <summary>
        /// Draw spot light shadow maps
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="cullIndex">Cull index</param>
        protected virtual void DoSpotShadowMapping(DrawContext context, ref int passIndex, ref int cullIndex)
        {
#if DEBUG
            var gStopwatch = Stopwatch.StartNew();
            var stopwatch = Stopwatch.StartNew();
#endif
            var shadowCastingLights = Scene.Lights.GetSpotShadowCastingLights(Scene.GameEnvironment, Scene.Camera.Position);
#if DEBUG
            stopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoSpotShadowMapping)} Getting lights", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowCastingLights.Any())
            {
                return;
            }

            //Draw components if drop shadow (opaque)
#if DEBUG
            stopwatch.Restart();
#endif
            var shadowObjs = Scene.Components.Get<IDrawable>(c => c.Visible && c.CastShadow.HasFlag(ShadowCastingAlgorihtms.Spot));
#if DEBUG
            stopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoSpotShadowMapping)} Getting components", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (!shadowObjs.Any())
            {
                return;
            }

            //All objects suitable for culling
            var toCullShadowObjs = shadowObjs.OfType<ICullable>();
            bool allCullingObjects = shadowObjs.Count() == toCullShadowObjs.Count();

            int assigned = 0;

            int l = 0;
            foreach (var light in shadowCastingLights)
            {
                light.ClearShadowParameters();

                if (assigned >= MaxCubicShadows)
                {
                    continue;
                }

                //Cull test
                if (!DoShadowCullingTest(toCullShadowObjs, l, new IntersectionVolumeSphere(light.Position, light.Radius), cullIndex, allCullingObjects))
                {
                    continue;
                }

                //Draw shadows
#if DEBUG
                stopwatch.Restart();
#endif
                var drawShadowsContext = GetPerLightDrawContext(passIndex, ShadowMapperSpot, light, assigned);
                DrawShadowComponents(drawShadowsContext, cullIndex, shadowObjs);
#if DEBUG
                stopwatch.Stop();
                shadowMappingDict.Add($"{nameof(DoSpotShadowMapping)} {l} - Draw {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

                //Assign light parameters
                light.FromLightVP = drawShadowsContext.ShadowMap.FromLightViewProjectionArray;
                light.ShadowMapIndex = assigned;
                light.ShadowMapCount = 1;

                passIndex++;

                assigned++;

                cullIndex++;

                l++;
            }

#if DEBUG
            gStopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoSpotShadowMapping)} TOTAL", gStopwatch.Elapsed.TotalMilliseconds);
#endif
        }
        /// <summary>
        /// Performs shadow culling testing
        /// </summary>
        /// <param name="components">Component list</param>
        /// <param name="l">Light index</param>
        /// <param name="lightVolume">Light volume</param>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="allCullingObjects">All components were culling components</param>
        /// <returns></returns>
        protected virtual bool DoShadowCullingTest(IEnumerable<ICullable> components, int l, IntersectionVolumeSphere lightVolume, int cullIndex, bool allCullingObjects)
        {
            if (components.Any())
            {
                return true;
            }

#if DEBUG
            var stopwatch = Stopwatch.StartNew();
#endif

            var doShadows = cullManager.Cull(lightVolume, cullIndex, components);

#if DEBUG
            stopwatch.Stop();
            shadowMappingDict.Add($"{nameof(DoShadowCullingTest)} {l} - Cull {cullIndex}", stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (allCullingObjects && !doShadows)
            {
                //All objects suitable for culling but no one pass the culling test
                return false;
            }

            return true;
        }

        /// <summary>
        /// Draw components for shadow mapping
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="index">Culling index</param>
        /// <param name="components">Components to draw</param>
        protected void DrawShadowComponents(DrawContextShadows context, int index, IEnumerable<IDrawable> components)
        {
            var objects = components
                .Where(c => IsVisible(c, index))
                .ToList();

            if (!objects.Any())
            {
                return;
            }

            objects.Sort((c1, c2) => Sort(c1, c2, index));

            objects.ForEach((c) => DrawShadows(context, c));
        }
        /// <summary>
        /// Gets if the specified object is not culled by the cull index
        /// </summary>
        /// <param name="c">Scene object</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns>Returns true if the object is not culled</returns>
        private bool IsVisible(IDrawable c, int cullIndex)
        {
            if (c is ICullable cull)
            {
                return !cullManager.GetCullValue(cullIndex, cull).Culled;
            }

            return true;
        }
        /// <summary>
        /// Sorts an object list by distance to culling point of view
        /// </summary>
        /// <param name="c1">Scene object one</param>
        /// <param name="c2">Scene object two</param>
        /// <param name="cullIndex">Cull index</param>
        /// <returns></returns>
        private int Sort(IDrawable c1, IDrawable c2, int cullIndex)
        {
            int res = c1.DepthEnabled.CompareTo(c2.DepthEnabled);
            if (res == 0)
            {
                float d1 = float.MaxValue;
                if (c1 is ICullable cull1)
                {
                    d1 = cullManager.GetCullValue(cullIndex, cull1).Distance;
                }

                float d2 = float.MaxValue;
                if (c2 is ICullable cull2)
                {
                    d2 = cullManager.GetCullValue(cullIndex, cull2).Distance;
                }

                res = -d1.CompareTo(d2);
            }

            if (res == 0)
            {
                res = c1.Layer.CompareTo(c2.Layer);
            }

            return res;
        }
        /// <summary>
        /// Draws the specified object shadows
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="drawable">Drawable object</param>
        private void DrawShadows(DrawContextShadows context, IDrawable drawable)
        {
            var graphics = Scene.Game.Graphics;
            var dc = context.DeviceContext;

            dc.SetRasterizerState(graphics.GetRasterizerShadowMapping());
            dc.SetDepthStencilState(graphics.GetDepthStencilShadowMapping());

            if (drawable.BlendMode.HasFlag(BlendModes.Alpha) || drawable.BlendMode.HasFlag(BlendModes.Transparent))
            {
                dc.SetBlendState(graphics.GetBlendAlpha());
            }
            else
            {
                dc.SetBlendState(graphics.GetBlendDefault());
            }

            drawable.DrawShadows(context);
        }

        /// <summary>
        /// Binds graphics for results pass
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="target">Target type</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        protected virtual void SetTarget(EngineDeviceContext dc, Targets target, bool clearRT, Color4 clearRTColor, bool clearDepth = false, bool clearStencil = false)
        {
            switch (target)
            {
                case Targets.Screen:
                    BindDefaultTarget(dc, clearRT, clearRTColor);
                    break;
                case Targets.Objects:
                    BindObjectsTarget(dc, clearRT, clearRTColor, clearDepth, clearStencil);
                    break;
                case Targets.UI:
                    BindUITarget(dc, clearRT, clearRTColor);
                    break;
                case Targets.Result:
                    BindResultsTarget(dc, clearRT, clearRTColor);
                    break;
                default:
                    BindDefaultTarget(dc, clearRT, clearRTColor);
                    break;
            }
        }
        /// <summary>
        /// Binds the default render target
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        private void BindDefaultTarget(EngineDeviceContext dc, bool clearRT, Color4 clearRTColor)
        {
            var graphics = Scene.Game.Graphics;

            //Restore back buffer as render target and clear it
            dc.SetRenderTargets(graphics.DefaultRenderTarget, clearRT, clearRTColor);
            dc.SetViewport(graphics.Viewport);
        }
        /// <summary>
        /// Binds the objects render target
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        private void BindObjectsTarget(EngineDeviceContext dc, bool clearRT, Color4 clearRTColor, bool clearDepth, bool clearStencil)
        {
            var graphics = Scene.Game.Graphics;

            dc.SetRenderTargets(sceneObjectsTarget.Targets, clearRT, clearRTColor, graphics.DefaultDepthStencil, clearDepth, clearStencil, false);
            dc.SetViewport(graphics.Viewport);
        }
        /// <summary>
        /// Binds the UI render target
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        private void BindUITarget(EngineDeviceContext dc, bool clearRT, Color4 clearRTColor)
        {
            dc.SetRenderTargets(sceneUITarget.Targets, clearRT, clearRTColor);
            dc.SetViewport(Scene.Game.Graphics.Viewport);
        }
        /// <summary>
        /// Binds the results render target
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        private void BindResultsTarget(EngineDeviceContext dc, bool clearRT, Color4 clearRTColor)
        {
            dc.SetRenderTargets(sceneResultsTarget.Targets, clearRT, clearRTColor);
            dc.SetViewport(Scene.Game.Graphics.Viewport);
        }
        /// <summary>
        /// Binds graphics for post-processing pass
        /// </summary>
        /// <param name="dc">Drawing context</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        private void BindPostProcessingTarget(EngineDeviceContext dc, bool clearRT, Color4 clearRTColor)
        {
            dc.SetRenderTargets(postProcessingTargetA.Targets, clearRT, clearRTColor);

            //Set local viewport
            var viewport = Scene.Game.Form.GetViewport();
            dc.SetViewport(viewport);
        }
        /// <summary>
        /// Toggles post-processing render targets
        /// </summary>
        private void TogglePostProcessingTargets()
        {
            (postProcessingTargetB, postProcessingTargetA) = (postProcessingTargetA, postProcessingTargetB);
        }
        /// <summary>
        /// Validates the post-processing render pass
        /// </summary>
        /// <param name="renderPass">Render pass</param>
        /// <param name="state">Gets the render pass state</param>
        private bool ValidateRenderPass(RenderPass renderPass, out BuiltInPostProcessState state)
        {
            if (renderPass == RenderPass.Objects && PostProcessingObjectsEffects.Ready)
            {
                state = PostProcessingObjectsEffects;

                return true;
            }

            if (renderPass == RenderPass.UI && PostProcessingUIEffects.Ready)
            {
                state = PostProcessingUIEffects;

                return true;
            }

            if (renderPass == RenderPass.Final && PostProcessingFinalEffects.Ready)
            {
                state = PostProcessingFinalEffects;

                return true;
            }

            state = null;

            return false;
        }
        /// <summary>
        /// Does the post-processing draw
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="target">Target to set result</param>
        /// <param name="renderPass">Render pass</param>
        /// <param name="gameTime">Game time</param>
        protected virtual bool DoPostProcessing(DrawContext context, Targets target, RenderPass renderPass)
        {
            if (!ValidateRenderPass(renderPass, out var state))
            {
                return false;
            }

            //Gets the last used target texture
            var texture = GetTargetTextures(target)?.FirstOrDefault();

            var graphics = Scene.Game.Graphics;
            var dc = context.DeviceContext;

            dc.SetRasterizerState(graphics.GetRasterizerCullNone());
            dc.SetDepthStencilState(graphics.GetDepthStencilNone());
            dc.SetBlendState(graphics.GetBlendDefault());

            var drawer = processingDrawer.UpdateEffectParameters(dc, state);
            if (drawer == null)
            {
                return false;
            }

            var activeEffects = state.GetEffects();

            for (int i = 0; i < activeEffects.Count(); i++)
            {
                var effect = activeEffects.ElementAt(i);
                if (effect == BuiltInPostProcessEffects.None)
                {
                    break;
                }

                //Toggles post-processing buffers
                TogglePostProcessingTargets();

                //Use the next buffer as render target
                BindPostProcessingTarget(dc, false, Color.Transparent);

                processingDrawer.UpdateEffect(dc, texture, effect);
                processingDrawer.Draw(dc, drawer);

                //Gets the source texture
                texture = postProcessingTargetA.Textures?.FirstOrDefault();
            }

            //Set the result render target
            SetTarget(dc, target, false, Color.Transparent);

            //Draw the result
            var resultDrawer = processingDrawer.UpdateEffect(dc, texture, BuiltInPostProcessEffects.None);
            processingDrawer.Draw(dc, resultDrawer);

            return true;
        }
        /// <summary>
        /// Gets the target textures
        /// </summary>
        /// <param name="target">Target type</param>
        /// <returns>Returns the target texture list</returns>
        protected virtual IEnumerable<EngineShaderResourceView> GetTargetTextures(Targets target)
        {
            return target switch
            {
                Targets.Screen => Enumerable.Empty<EngineShaderResourceView>(),
                Targets.Objects => sceneObjectsTarget?.Textures,
                Targets.UI => sceneUITarget?.Textures,
                Targets.Result => sceneResultsTarget?.Textures,
                _ => Enumerable.Empty<EngineShaderResourceView>(),
            };
        }
        /// <summary>
        /// Combine the specified targets into the result target
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="target1">Target 1</param>
        /// <param name="target2">Target 2</param>
        /// <param name="resultTarget">Result target</param>
        protected virtual void CombineTargets(DrawContext context, Targets target1, Targets target2, Targets resultTarget)
        {
            var graphics = Scene.Game.Graphics;
            var dc = context.DeviceContext;

            SetTarget(dc, resultTarget, false, Color.Transparent);

            dc.SetDepthStencilState(graphics.GetDepthStencilNone());
            dc.SetRasterizerState(graphics.GetRasterizerDefault());
            dc.SetBlendState(graphics.GetBlendDefault());

            var texture1 = GetTargetTextures(target1)?.FirstOrDefault();
            var texture2 = GetTargetTextures(target2)?.FirstOrDefault();

            var drawer = processingDrawer.UpdateEffectCombine(dc, texture1, texture2);
            processingDrawer.Draw(dc, drawer);
        }
        /// <summary>
        /// Draws the specified target to screen
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="target">Target</param>
        protected virtual void DrawToScreen(DrawContext context, Targets target)
        {
            var graphics = Scene.Game.Graphics;
            var dc = context.DeviceContext;

            SetTarget(dc, Targets.Screen, false, Color.Transparent);

            dc.SetDepthStencilState(graphics.GetDepthStencilNone());
            dc.SetRasterizerState(graphics.GetRasterizerDefault());
            dc.SetBlendState(graphics.GetBlendDefault());

            var texture = GetTargetTextures(target)?.FirstOrDefault();

            var drawer = processingDrawer.UpdateEffect(dc, texture, BuiltInPostProcessEffects.None);
            processingDrawer.Draw(dc, drawer);
        }

        /// <inheritdoc/>
        public void ClearPostProcessingEffects()
        {
            PostProcessingObjectsEffects = BuiltInPostProcessState.Empty;
            PostProcessingUIEffects = BuiltInPostProcessState.Empty;
            PostProcessingFinalEffects = BuiltInPostProcessState.Empty;
        }
    }
}
