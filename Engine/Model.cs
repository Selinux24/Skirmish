﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Basic Model
    /// </summary>
    public class Model : BaseModel, ITransformable3D, IRayPickable<Triangle>, IIntersectable, ICullable, IHasGameState, IModelHasParts<ModelPart>
    {
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetail levelOfDetail = LevelOfDetail.None;
        /// <summary>
        /// Volume helper
        /// </summary>
        private readonly BoundsHelper boundsHelper = new BoundsHelper();
        /// <summary>
        /// Geometry helper
        /// </summary>
        private readonly GeometryHelper geometryHelper = new GeometryHelper();
        /// <summary>
        /// Model parts collection
        /// </summary>
        private readonly List<ModelPart> modelParts = new List<ModelPart>();

        /// <summary>
        /// Current drawing data
        /// </summary>
        protected DrawingData DrawingData { get; private set; }

        /// <inheritdoc/>
        public Manipulator3D Manipulator { get; private set; }
        /// <summary>
        /// Animation controller
        /// </summary>
        public AnimationController AnimationController { get; private set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; }
        /// <summary>
        /// Animation palette offset
        /// </summary>
        public uint AnimationOffset { get; set; }
        public uint TransitionOffset { get; set; }
        public float TransitionInterpolation { get; set; }
        /// <summary>
        /// Level of detail
        /// </summary>
        public LevelOfDetail LevelOfDetail
        {
            get
            {
                return levelOfDetail;
            }
            private set
            {
                if (levelOfDetail != value)
                {
                    levelOfDetail = GetLODNearest(value);
                    DrawingData = GetDrawingData(levelOfDetail);
                }
            }
        }
        /// <inheritdoc/>
        public override ISkinningData SkinningData
        {
            get
            {
                if (DrawingData == null)
                {
                    DrawingData = GetDrawingData(levelOfDetail);
                }

                return DrawingData?.SkinningData;
            }
        }
        /// <summary>
        /// Gets the current model lights collection
        /// </summary>
        public IEnumerable<ISceneLight> Lights { get; private set; } = new ISceneLight[] { };
        /// <inheritdoc/>
        public int ModelPartCount
        {
            get
            {
                return modelParts.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Model(string id, string name, Scene scene, ModelDescription description)
            : base(id, name, scene, description)
        {
            TextureIndex = description.TextureIndex;

            if (description.TransformDependences?.Any() == true)
            {
                AddModelParts(description.TransformNames, description.TransformDependences);
            }
            else
            {
                Manipulator = new Manipulator3D();
                Manipulator.Updated += ManipulatorUpdated;
            }

            var drawData = GetDrawingData(LevelOfDetail.High);
            if (drawData != null)
            {
                SetModelPartsTransforms(drawData);

                Lights = drawData.GetLights();
            }

            AnimationController = new AnimationController(this);
            AnimationController.AnimationOffsetChanged += (s, a) => { InvalidateCache(); };

            boundsHelper.Initialize(GetPoints(true));
        }
        /// <summary>
        /// Add model parts
        /// </summary>
        /// <param name="names">Part names</param>
        /// <param name="dependences">Part dependences</param>
        private void AddModelParts(string[] names, int[] dependences)
        {
            int parents = dependences.Count(i => i == -1);
            if (parents != 1)
            {
                throw new EngineException("Model with transform dependences must have one (and only one) parent mesh identified by -1");
            }

            if (dependences.Any(i => i < -1 || i > dependences.Count() - 1))
            {
                throw new EngineException("Bad transform dependences indices.");
            }

            for (int i = 0; i < names.Length; i++)
            {
                modelParts.Add(new ModelPart(names[i]));
            }

            for (int i = 0; i < names.Length; i++)
            {
                var thisPart = GetModelPartByName(names[i]);
                if (thisPart == null)
                {
                    continue;
                }

                var thisMan = thisPart.Manipulator;
                thisMan.Updated += ManipulatorUpdated;

                var parentIndex = dependences[i];
                if (parentIndex >= 0)
                {
                    var parentPart = GetModelPartByName(names[parentIndex]);

                    thisMan.Parent = parentPart?.Manipulator;
                }
                else
                {
                    Manipulator = thisMan;
                }
            }
        }
        /// <summary>
        /// Sets model part transforms from original meshes
        /// </summary>
        /// <param name="drawData">Drawing data</param>
        private void SetModelPartsTransforms(DrawingData drawData)
        {
            for (int i = 0; i < modelParts.Count; i++)
            {
                var thisName = modelParts[i].Name;

                var mesh = drawData?.GetMeshByName(thisName);
                if (mesh == null)
                {
                    continue;
                }

                var part = modelParts.First(p => p.Name == thisName);
                part.Manipulator.SetTransform(mesh.Transform);
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            SetLOD(context.EyePosition);

            AnimationController.Update(context.GameTime.ElapsedSeconds);
            AnimationOffset = AnimationController.AnimationOffset;
            TransitionOffset = AnimationController.TransitionOffset;
            TransitionInterpolation = AnimationController.TransitionInterpolationAmount;

            if (modelParts.Count > 0)
            {
                modelParts.ForEach(p => p.Manipulator.Update(context.GameTime));
            }
            else
            {
                Manipulator.Update(context.GameTime);
            }

            if (Lights.Any())
            {
                foreach (var light in Lights)
                {
                    light.ParentTransform = Manipulator.LocalTransform;
                }
            }
        }
        /// <inheritdoc/>
        public override void DrawShadows(DrawContextShadows context)
        {
            if (!Visible)
            {
                return;
            }

            if (DrawingData == null)
            {
                return;
            }

            var effect = context.ShadowMap.GetEffect();
            if (effect == null)
            {
                return;
            }

            int count = 0;
            foreach (string meshName in DrawingData.Meshes.Keys)
            {
                count += DrawMeshShadow(context, effect, meshName);
            }
        }
        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (DrawingData == null)
            {
                return;
            }

            var effect = GetEffect(context.DrawerMode);
            if (effect == null)
            {
                return;
            }

            int count = 0;
            foreach (string meshName in DrawingData.Meshes.Keys)
            {
                count += DrawMesh(context, effect, meshName);
            }

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += count;
        }
        /// <summary>
        /// Draws a mesh shadow
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="effect">Effect</param>
        /// <param name="meshName">Mesh name</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawMeshShadow(DrawContextShadows context, IShadowMapDrawer effect, string meshName)
        {
            int count = 0;

            var graphics = Game.Graphics;

            var meshDict = DrawingData.Meshes[meshName];

            var localTransform = GetTransformByName(meshName);

            effect.UpdatePerFrame(localTransform, context);

            foreach (string materialName in meshDict.Keys)
            {
                var mesh = meshDict[materialName];
                if (!mesh.Ready)
                {
                    continue;
                }

                var material = DrawingData.Materials[materialName];

                var animationInfo = new AnimationShadowDrawInfo
                {
                    Offset1 = AnimationOffset,
                    Offset2 = TransitionOffset,
                    InterpolationAmount = TransitionInterpolation,
                };

                var materialInfo = new MaterialShadowDrawInfo
                {
                    Material = material,
                };

                effect.UpdatePerObject(animationInfo, materialInfo, TextureIndex);

                BufferManager.SetIndexBuffer(mesh.IndexBuffer);

                var technique = effect.GetTechnique(mesh.VertextType, false, material.Material.IsTransparent);
                BufferManager.SetInputAssembler(technique, mesh.VertexBuffer, mesh.Topology);

                count += mesh.Count;

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    mesh.Draw(graphics);
                }
            }

            return count;
        }
        /// <summary>
        /// Draws a mesh
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="effect">Effect</param>
        /// <param name="meshName">Mesh name</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawMesh(DrawContext context, IGeometryDrawer effect, string meshName)
        {
            int count = 0;

            var graphics = Game.Graphics;

            var meshDict = DrawingData.Meshes[meshName];

            var localTransform = GetTransformByName(meshName);

            effect.UpdatePerFrameFull(localTransform, context);

            foreach (string materialName in meshDict.Keys)
            {
                var mesh = meshDict[materialName];
                if (!mesh.Ready)
                {
                    continue;
                }

                var material = DrawingData.Materials[materialName];

                bool draw = context.ValidateDraw(BlendMode, material.Material.IsTransparent);
                if (!draw)
                {
                    continue;
                }

                var animationInfo = new AnimationDrawInfo
                {
                    Offset1 = AnimationOffset,
                    Offset2 = TransitionOffset,
                    InterpolationAmount = TransitionInterpolation,
                };

                var materialInfo = new MaterialDrawInfo
                {
                    Material = material,
                    UseAnisotropic = UseAnisotropicFiltering,
                };

                effect.UpdatePerObject(animationInfo, materialInfo, TextureIndex);

                BufferManager.SetIndexBuffer(mesh.IndexBuffer);

                var technique = effect.GetTechnique(mesh.VertextType, false);
                BufferManager.SetInputAssembler(technique, mesh.VertexBuffer, mesh.Topology);

                count += mesh.Count;

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    mesh.Draw(graphics);
                }
            }

            return count;
        }

        /// <inheritdoc/>
        public override bool Cull(IIntersectionVolume volume, out float distance)
        {
            bool cull;
            distance = float.MaxValue;

            if (SphericVolume)
            {
                cull = volume.Contains(GetBoundingSphere()) == ContainmentType.Disjoint;
            }
            else
            {
                cull = volume.Contains(GetBoundingBox()) == ContainmentType.Disjoint;
            }

            if (!cull)
            {
                var eyePosition = volume.Position;

                distance = Vector3.DistanceSquared(Manipulator.Position, eyePosition);
            }

            return cull;
        }
        /// <summary>
        /// Set level of detail values
        /// </summary>
        /// <param name="origin">Origin point</param>
        public void SetLOD(Vector3 origin)
        {
            LevelOfDetail = Scene.GameEnvironment.GetLOD(
                origin,
                GetBoundingSphere(),
                Manipulator.FinalTransform);
        }

        /// <summary>
        /// Sets a new manipulator to this instance
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        public void SetManipulator(Manipulator3D manipulator)
        {
            if (manipulator == null)
            {
                Logger.WriteWarning(this, $"Model Name: {Name} - Sets a null manipulator. Discarded.");

                return;
            }

            if (Manipulator != null)
            {
                Manipulator.Updated -= ManipulatorUpdated;
            }

            Manipulator = manipulator;
            Manipulator.Updated += ManipulatorUpdated;

            boundsHelper.Initialize(GetPoints(true));
        }
        /// <summary>
        /// Occurs when manipulator transform updated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void ManipulatorUpdated(object sender, EventArgs e)
        {
            InvalidateCache();
        }

        /// <inheritdoc/>
        public Matrix GetTransformByName(string name)
        {
            var part = modelParts.FirstOrDefault(p => p.Name == name);
            if (part != null)
            {
                return part.Manipulator.FinalTransform;
            }

            return Manipulator.FinalTransform;
        }
        /// <inheritdoc/>
        public ModelPart GetModelPartByName(string name)
        {
            return modelParts.FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// Invalidates the internal cache
        /// </summary>
        private void InvalidateCache()
        {
            Logger.WriteTrace(this, $"Model Name: {Name}; LOD: {LevelOfDetail}; InvalidateCache");

            boundsHelper.Invalidate();
            geometryHelper.Invalidate();
        }
        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public IEnumerable<Vector3> GetPoints(bool refresh = false)
        {
            return geometryHelper.GetPoints(
                GetDrawingData(GetLODMinimum()),
                AnimationController,
                Manipulator,
                refresh);
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(bool refresh = false)
        {
            return geometryHelper.GetTriangles(
                GetDrawingData(GetLODMinimum()),
                AnimationController,
                Manipulator,
                refresh);
        }

        /// <inheritdoc/>
        public bool PickNearest(Ray ray, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickNearest(this, ray, out result);
        }
        /// <inheritdoc/>
        public bool PickNearest(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickNearest(this, ray, rayPickingParams, out result);
        }
        /// <inheritdoc/>
        public bool PickFirst(Ray ray, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickFirst(this, ray, out result);
        }
        /// <inheritdoc/>
        public bool PickFirst(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickFirst(this, ray, rayPickingParams, out result);
        }
        /// <inheritdoc/>
        public bool PickAll(Ray ray, out IEnumerable<PickingResult<Triangle>> results)
        {
            return RayPickingHelper.PickAll(this, ray, out results);
        }
        /// <inheritdoc/>
        public bool PickAll(Ray ray, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<Triangle>> results)
        {
            return RayPickingHelper.PickAll(this, ray, rayPickingParams, out results);
        }

        /// <inheritdoc/>
        public BoundingSphere GetBoundingSphere(bool refresh = false)
        {
            return boundsHelper.GetBoundingSphere(Manipulator, refresh);
        }
        /// <inheritdoc/>
        public BoundingBox GetBoundingBox(bool refresh = false)
        {
            return boundsHelper.GetBoundingBox(Manipulator, refresh);
        }
        /// <inheritdoc/>
        public OrientedBoundingBox GetOrientedBoundingBox(bool refresh = false)
        {
            return boundsHelper.GetOrientedBoundingBox(Manipulator, refresh);
        }

        /// <inheritdoc/>
        public IEnumerable<Triangle> GetVolume(bool full)
        {
            if (!full && DrawingData?.VolumeMesh?.Any() == true)
            {
                return Triangle.Transform(DrawingData.VolumeMesh, Manipulator.LocalTransform);
            }

            return GetTriangles();
        }

        /// <inheritdoc/>
        public bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result)
        {
            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            var bsph = GetBoundingSphere();
            if (bsph.Intersects(sphere))
            {
                var mesh = GetVolume(false);
                if (Intersection.SphereIntersectsMesh(sphere, mesh, out Triangle tri, out Vector3 position, out float distance))
                {
                    result.Distance = distance;
                    result.Position = position;
                    result.Item = tri;

                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectable other, IntersectDetectionMode detectionModeOther)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, other, detectionModeOther);
        }
        /// <inheritdoc/>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectionVolume volume)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, volume);
        }

        /// <inheritdoc/>
        public IIntersectionVolume GetIntersectionVolume(IntersectDetectionMode detectionMode)
        {
            if (detectionMode == IntersectDetectionMode.Box)
            {
                return (IntersectionVolumeAxisAlignedBox)GetBoundingBox();
            }
            else if (detectionMode == IntersectDetectionMode.Sphere)
            {
                return (IntersectionVolumeSphere)GetBoundingSphere();
            }
            else
            {
                return (IntersectionVolumeMesh)GetVolume(true).ToArray();
            }
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new ModelState
            {
                Name = Name,
                Active = Active,
                Visible = Visible,
                Usage = Usage,
                Layer = Layer,
                OwnerId = Owner?.Name,

                Manipulator = Manipulator.GetState(),
                AnimationController = AnimationController.GetState(),
                TextureIndex = TextureIndex,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (!(state is ModelState modelState))
            {
                return;
            }

            Name = modelState.Name;
            Active = modelState.Active;
            Visible = modelState.Visible;
            Usage = modelState.Usage;
            Layer = modelState.Layer;

            if (!string.IsNullOrEmpty(modelState.OwnerId))
            {
                Owner = Scene.GetComponents().FirstOrDefault(c => c.Id == modelState.OwnerId);
            }

            Manipulator?.SetState(modelState.Manipulator);
            AnimationController?.SetState(modelState.AnimationController);
            TextureIndex = modelState.TextureIndex;
        }
    }

    /// <summary>
    /// Model extensions
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Model> AddComponentModel(this Scene scene, string id, string name, ModelDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = Scene.LayerDefault)
        {
            Model component = null;

            await Task.Run(() =>
            {
                component = new Model(id, name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}
