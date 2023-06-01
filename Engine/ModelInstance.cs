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

            if (dependences.Any(i => i < -1 || i > dependences.Length - 1))
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
            SetLOD(context.EyePosition);

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
            var part = modelParts.Find(p => p.Name == name);
            if (part != null)
            {
                return part.Manipulator.FinalTransform;
            }
            else
            {
                return Manipulator.FinalTransform;
            }
        }
        /// <inheritdoc/>
        public ModelPart GetModelPartByName(string name)
        {
            return modelParts.FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// Invalidates the internal cache
        /// </summary>
        public void InvalidateCache()
        {
            Logger.WriteTrace(this, $"{nameof(ModelInstance)} {model.Name}.{Id} => LOD: {LevelOfDetail}; InvalidateCache");

            boundsHelper.Invalidate();
            geometryHelper.Invalidate();
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
        public IEnumerable<Triangle> GetPickingHull(PickingHullTypes geometryType)
        {
            var drawingData = model.GetDrawingData(model.GetLODMinimum());
            if (geometryType != PickingHullTypes.Object && drawingData?.HullMesh?.Any() == true)
            {
                //Transforms the volume mesh
                return Triangle.Transform(drawingData.HullMesh, Manipulator.LocalTransform);
            }

            //Returns the actual triangles (yet transformed)
            return GetTriangles();
        }

        /// <inheritdoc/>
        public virtual bool Cull(ICullingVolume volume, out float distance)
        {
            return boundsHelper.Cull(Manipulator, model.CullingVolumeType, volume, out distance);
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
                Manipulator.FinalTransform);
        }

        /// <inheritdoc/>
        public bool Intersects(IntersectionVolumeSphere sphere, out PickingResult<Triangle> result)
        {
            var bsph = GetBoundingSphere();
            if (bsph.Intersects(sphere))
            {
                var mesh = GetPickingHull(PickingHullTypes.Hull);
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
            else if (detectionMode == IntersectDetectionMode.Sphere)
            {
                return (IntersectionVolumeSphere)GetBoundingSphere();
            }
            else
            {
                return (IntersectionVolumeMesh)GetPickingHull(PickingHullTypes.Hull).ToArray();
            }
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
            return $"Id: {Id}; LOD: {LevelOfDetail}; Active: {Active}; Visible: {Visible}";
        }
    }
}
