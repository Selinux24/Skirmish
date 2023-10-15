using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Animation;
    using Engine.Common;

    /// <summary>
    /// Model instance
    /// </summary>
    public class ModelInstance : ITransformable3D, IRayPickable<Triangle>, IIntersectable, ICullable, IHasGameState, IModelHasParts<ModelPart>, IUseSkinningData
    {
        /// <summary>
        /// Global id counter
        /// </summary>
        private static int InstanceId = 0;
        /// <summary>
        /// Gets the next instance Id
        /// </summary>
        /// <returns>Returns the next instance Id</returns>
        private static int GetNextInstanceId()
        {
            return ++InstanceId;
        }

        /// <summary>
        /// Model
        /// </summary>
        private readonly BaseModel<ModelInstancedDescription> model = null;
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetail levelOfDetail = LevelOfDetail.High;
        /// <summary>
        /// Volume helper
        /// </summary>
        private readonly BoundsHelper boundsHelper;
        /// <summary>
        /// Geometry helper
        /// </summary>
        private readonly GeometryHelper geometryHelper = new();
        /// <summary>
        /// Model parts collection
        /// </summary>
        private readonly List<ModelPart> modelParts = new();

        /// <summary>
        /// Instance id
        /// </summary>
        public int Id { get; private set; }
        /// <inheritdoc/>
        public Manipulator3D Manipulator { get; private set; }
        /// <summary>
        /// Animation controller
        /// </summary>
        public AnimationController AnimationController { get; private set; }
        /// <summary>
        /// Tint color
        /// </summary>
        public Color4 TintColor { get; set; } = Color4.White;
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; } = 0;
        /// <summary>
        /// Material index
        /// </summary>
        public uint MaterialIndex { get; set; } = 0;
        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; } = true;
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// Instance level of detail
        /// </summary>
        public LevelOfDetail LevelOfDetail
        {
            get
            {
                return levelOfDetail;
            }
            private set
            {
                levelOfDetail = model.GetLODNearest(value);
            }
        }
        /// <summary>
        /// Gets the current instance lights collection
        /// </summary>
        public IEnumerable<ISceneLight> Lights { get; private set; } = Array.Empty<ISceneLight>();
        /// <inheritdoc/>
        public int ModelPartCount
        {
            get
            {
                return modelParts.Count;
            }
        }
        /// <inheritdoc/>
        public ISkinningData SkinningData
        {
            get
            {
                return model.GetDrawingData(levelOfDetail)?.SkinningData;
            }
        }
        /// <summary>
        /// Culling volume for culling test
        /// </summary>
        public CullingVolumeTypes CullingVolumeType
        {
            get
            {
                return model.CullingVolumeType;
            }
        }
        /// <summary>
        /// Collider type for collision tests
        /// </summary>
        public ColliderTypes ColliderType
        {
            get
            {
                return model.ColliderType;
            }
        }
        /// <summary>
        /// Gets or sets the parent path finding hull
        /// </summary>
        public PickingHullTypes PathFindingHull
        {
            get
            {
                return model.PathFindingHull;
            }
            set
            {
                model.PathFindingHull = value;
            }
        }
        /// <summary>
        /// Gets or sets the parent picking hull
        /// </summary>
        public PickingHullTypes PickingHull
        {
            get
            {
                return model.PickingHull;
            }
            set
            {
                model.PickingHull = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="description">Description</param>
        public ModelInstance(BaseModel<ModelInstancedDescription> model, ModelInstancedDescription description)
        {
            Id = GetNextInstanceId();
            this.model = model;

            if (description.TransformDependences?.Any() == true)
            {
                AddModelParts(description.TransformNames, description.TransformDependences);
            }
            else
            {
                Manipulator = new Manipulator3D();
                Manipulator.Updated += ManipulatorUpdated;
            }

            var drawData = model.GetDrawingData(LevelOfDetail.High);
            if (drawData != null)
            {
                SetModelPartsTransforms(drawData);

                Lights = drawData.GetLights();
            }

            AnimationController = new AnimationController(model);
            AnimationController.AnimationOffsetChanged += (s, a) => InvalidateCache();

            boundsHelper = new(GetPoints());
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

            if (Array.Exists(dependences, i => i < -1 || i > dependences.Length - 1))
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

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public virtual void Update(UpdateContext context)
        {
            SetLOD(model.Scene.Camera.Position);

            if (modelParts.Count > 0)
            {
                modelParts.ForEach(p => p.Manipulator.Update(context.GameTime));
            }
            else
            {
                Manipulator.Update(context.GameTime);
            }

            if (Visible && LevelOfDetail != LevelOfDetail.None)
            {
                AnimationController?.Update(context.GameTime.ElapsedSeconds);
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
        public virtual bool Cull(int cullIndex, ICullingVolume volume, out float distance)
        {
            return boundsHelper.Cull(Manipulator, CullingVolumeType, volume, out distance);
        }

        /// <summary>
        /// Set level of detail values
        /// </summary>
        /// <param name="origin">Origin point</param>
        public void SetLOD(Vector3 origin)
        {
            LevelOfDetail = model.Scene.GameEnvironment.GetLOD(
                origin,
                GetBoundingSphere(),
                Manipulator.GlobalTransform);
        }

        /// <summary>
        /// Sets a new manipulator to this instance
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        public void SetManipulator(Manipulator3D manipulator)
        {
            if (manipulator == null)
            {
                Logger.WriteWarning(this, $"ModelInstance Id: {Id} - Sets a null manipulator. Discarded.");

                return;
            }

            if (Manipulator != null)
            {
                Manipulator.Updated -= ManipulatorUpdated;
            }

            Manipulator = manipulator;
            Manipulator.Updated += ManipulatorUpdated;

            InvalidateCache();
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
            var part = GetModelPartByName(name);

            return part?.Manipulator.GlobalTransform ?? Manipulator.GlobalTransform;
        }
        /// <inheritdoc/>
        public ModelPart GetModelPartByName(string name)
        {
            return modelParts.Find(p => p.Name == name);
        }

        /// <summary>
        /// Invalidates the internal cache
        /// </summary>
        public void InvalidateCache()
        {
            Logger.WriteTrace(this, $"{nameof(ModelInstance)} {model.Name}.{Id} => LOD: {LevelOfDetail}; InvalidateCache");

            boundsHelper?.Invalidate();
            geometryHelper?.Invalidate();
        }

        /// <inheritdoc/>
        public bool PickNearest(PickingRay ray, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickNearest(this, ray, out result);
        }
        /// <inheritdoc/>
        public bool PickFirst(PickingRay ray, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickFirst(this, ray, out result);
        }
        /// <inheritdoc/>
        public bool PickAll(PickingRay ray, out IEnumerable<PickingResult<Triangle>> results)
        {
            return RayPickingHelper.PickAll(this, ray, out results);
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
        public IEnumerable<Triangle> GetGeometry(GeometryTypes geometryType)
        {
            var hull = geometryType switch
            {
                GeometryTypes.Picking => PickingHull,
                GeometryTypes.PathFinding => PathFindingHull,
                _ => PickingHullTypes.None,
            };

            if (hull.HasFlag(PickingHullTypes.Coarse))
            {
                return Triangle.ComputeTriangleList(Topology.TriangleList, boundsHelper.GetOrientedBoundingBox(Manipulator));
            }

            if (hull.HasFlag(PickingHullTypes.Hull))
            {
                var drawingData = model.GetDrawingData(model.GetLODMinimum());
                if (drawingData?.HullMesh?.Any() ?? false)
                {
                    return Triangle.Transform(drawingData.HullMesh, Manipulator.LocalTransform);
                }

                return GetTriangles();
            }

            if (hull.HasFlag(PickingHullTypes.Geometry))
            {
                return GetTriangles();
            }

            return Enumerable.Empty<Triangle>();
        }

        /// <inheritdoc/>
        public bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result)
        {
            var bsph = GetBoundingSphere();
            if (bsph.Intersects(sphere))
            {
                var mesh = GetGeometry(GeometryTypes.Picking);
                if (Intersection.SphereIntersectsMesh(sphere, mesh, out var res))
                {
                    result = res;

                    return true;
                }
            }

            result = new PickingResult<Triangle>()
            {
                Distance = float.MaxValue,
            };

            return false;
        }
        /// <inheritdoc/>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectable other, IntersectDetectionMode detectionModeOther)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, other, detectionModeOther);
        }
        /// <inheritdoc/>
        public bool Intersects(IntersectDetectionMode detectionModeThis, ICullingVolume volume)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, volume);
        }
        /// <inheritdoc/>
        public ICullingVolume GetIntersectionVolume(IntersectDetectionMode detectionMode)
        {
            if (detectionMode == IntersectDetectionMode.Box)
            {
                return (IntersectionVolumeAxisAlignedBox)GetBoundingBox();
            }

            if (detectionMode == IntersectDetectionMode.Sphere)
            {
                return (IntersectionVolumeSphere)GetBoundingSphere();
            }

            return (IntersectionVolumeMesh)GetGeometry(GeometryTypes.Picking).ToArray();
        }

        /// <inheritdoc/>
        public IEnumerable<Vector3> GetPoints(bool refresh = false)
        {
            return geometryHelper.GetPoints(
                model.GetDrawingData(model.GetLODMinimum()),
                AnimationController,
                Manipulator,
                refresh);
        }
        /// <inheritdoc/>
        public IEnumerable<Triangle> GetTriangles(bool refresh = false)
        {
            return geometryHelper.GetTriangles(
                model.GetDrawingData(model.GetLODMinimum()),
                AnimationController,
                Manipulator,
                refresh);
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new ModelInstanceState
            {
                InstanceId = Id,
                Active = Active,
                Visible = Visible,
                Manipulator = Manipulator.GetState(),
                AnimationController = AnimationController.GetState(),
                TextureIndex = TextureIndex,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not ModelInstanceState modelInstanceState)
            {
                return;
            }

            Id = modelInstanceState.InstanceId;
            Active = modelInstanceState.Active;
            Visible = modelInstanceState.Visible;
            Manipulator.SetState(modelInstanceState.Manipulator);
            AnimationController.SetState(modelInstanceState.AnimationController);
            TextureIndex = modelInstanceState.TextureIndex;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{model.Id}.{Id}; LOD: {LevelOfDetail}; Active: {Active}; Visible: {Visible}";
        }
    }
}
