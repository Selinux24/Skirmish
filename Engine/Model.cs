using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Animation;
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Basic Model
    /// </summary>
    public class Model : BaseModel<ModelDescription>, ITransformable3D, IRayPickable<Triangle>, IIntersectable, ICullable, IHasGameState, IModelHasParts<ModelPart>
    {
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetail levelOfDetail = LevelOfDetail.None;
        /// <summary>
        /// Volume helper
        /// </summary>
        private BoundsHelper boundsHelper;
        /// <summary>
        /// Geometry helper
        /// </summary>
        private readonly GeometryHelper geometryHelper = new();
        /// <summary>
        /// Model parts collection
        /// </summary>
        private readonly List<ModelPart> modelParts = new();

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
        /// Tint color
        /// </summary>
        public Color4 TintColor { get; set; } = Color4.White;
        /// <summary>
        /// Animation palette offset
        /// </summary>
        public uint AnimationOffset { get; set; }
        /// <summary>
        /// Transition palette offset
        /// </summary>
        public uint TransitionOffset { get; set; }
        /// <summary>
        /// Transition interpolation value
        /// </summary>
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
                DrawingData ??= GetDrawingData(levelOfDetail);

                return DrawingData?.SkinningData;
            }
        }
        /// <summary>
        /// Gets the current model lights collection
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public Model(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(ModelDescription description)
        {
            await base.InitializeAssets(description);

            await InitializeGeometry(description);

            TextureIndex = Description.TextureIndex;

            if (Description.TransformDependences?.Any() == true)
            {
                AddModelParts(Description.TransformNames, Description.TransformDependences);
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
            AnimationController.AnimationOffsetChanged += (s, a) => InvalidateCache();

            boundsHelper = new(GetPoints());
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            SetLOD(Scene.Camera.Position);

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
        public override bool DrawShadows(DrawContextShadows context)
        {
            if (!Visible)
            {
                return false;
            }

            if (DrawingData == null)
            {
                return false;
            }

            int count = 0;

            var dc = context.DeviceContext;

            foreach (var meshMaterial in DrawingData.IterateMaterials())
            {
                string materialName = meshMaterial.MaterialName;
                var material = meshMaterial.Material;
                string meshName = meshMaterial.MeshName;
                var mesh = meshMaterial.Mesh;

                Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(DrawShadows)}: {meshName}.");

                var localTransform = GetTransformByName(meshName);

                var drawer = context.ShadowMap?.GetDrawer(mesh.VertextType, false, material.Material.IsTransparent);
                if (drawer == null)
                {
                    continue;
                }

                drawer.UpdateCastingLight(context);

                var meshState = new BuiltInDrawerMeshState
                {
                    Local = localTransform,
                    AnimationOffset1 = AnimationOffset,
                    AnimationOffset2 = TransitionOffset,
                    AnimationInterpolationAmount = TransitionInterpolation,
                };
                drawer.UpdateMesh(dc, meshState);

                var materialState = new BuiltInDrawerMaterialState
                {
                    Material = material,
                    UseAnisotropic = false,
                    TextureIndex = TextureIndex,
                    TintColor = Color4.White,
                };
                drawer.UpdateMaterial(dc, materialState);

                Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(DrawShadows)}: {meshName}.{materialName}.");
                if (drawer.Draw(dc, BufferManager, new[] { mesh }))
                {
                    count += mesh.Count;
                }
            }

            return count > 0;
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (DrawingData == null)
            {
                return false;
            }

            int count = 0;

            var dc = context.DeviceContext;

            foreach (var meshMaterial in DrawingData.IterateMaterials())
            {
                string materialName = meshMaterial.MaterialName;
                var material = meshMaterial.Material;
                string meshName = meshMaterial.MeshName;
                var mesh = meshMaterial.Mesh;

                Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(Draw)}: {meshName}.");

                var localTransform = GetTransformByName(meshName);

                bool draw = context.ValidateDraw(BlendMode, material.Material.IsTransparent);
                if (!draw)
                {
                    Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(Draw)}: {meshName}.{materialName} discard => BlendMode {BlendMode}");
                    continue;
                }

                var drawer = GetDrawer(context.DrawerMode, mesh.VertextType, false);
                if (drawer == null)
                {
                    continue;
                }

                var meshState = new BuiltInDrawerMeshState
                {
                    Local = localTransform,
                    AnimationOffset1 = AnimationOffset,
                    AnimationOffset2 = TransitionOffset,
                    AnimationInterpolationAmount = TransitionInterpolation,
                };
                drawer.UpdateMesh(dc, meshState);

                var materialState = new BuiltInDrawerMaterialState
                {
                    Material = material,
                    UseAnisotropic = UseAnisotropicFiltering,
                    TextureIndex = TextureIndex,
                    TintColor = TintColor,
                };
                drawer.UpdateMaterial(dc, materialState);

                Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(Draw)}: {meshName}.{materialName}.");
                if (drawer.Draw(dc, BufferManager, new[] { mesh }))
                {
                    count += mesh.Count;
                }
            }

            return count > 0;
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
                throw new EngineException("Bad transformation dependency indices.");
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
        public override bool Cull(int cullIndex, ICullingVolume volume, out float distance)
        {
            return boundsHelper.Cull(Manipulator, CullingVolumeType, volume, out distance);
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
                Logger.WriteWarning(this, $"Model Name: {Name} - Sets a null manipulator. Discarded.");

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
            Logger.WriteTrace(this, $"{nameof(Model)} {Name} => LOD: {LevelOfDetail}; InvalidateCache");

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
                var drawingData = GetDrawingData(GetLODMinimum());
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
            if (!bsph.Intersects(sphere))
            {
                result = new PickingResult<Triangle>()
                {
                    Distance = float.MaxValue,
                };

                return false;
            }

            var mesh = GetGeometry(GeometryTypes.Picking);

            return Intersection.SphereIntersectsMesh(sphere, mesh, out result);
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
                GetDrawingData(GetLODMinimum()),
                AnimationController,
                Manipulator,
                refresh);
        }
        /// <inheritdoc/>
        public IEnumerable<Triangle> GetTriangles(bool refresh = false)
        {
            return geometryHelper.GetTriangles(
                GetDrawingData(GetLODMinimum()),
                AnimationController,
                Manipulator,
                refresh);
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
            if (state is not ModelState modelState)
            {
                return;
            }

            Name = modelState.Name;
            Active = modelState.Active;
            Visible = modelState.Visible;
            Usage = modelState.Usage;
            Layer = modelState.Layer;
            Owner = Scene.Components.ById(modelState.OwnerId);
            Manipulator?.SetState(modelState.Manipulator);
            AnimationController?.SetState(modelState.AnimationController);
            TextureIndex = modelState.TextureIndex;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Id: {Id}; LOD: {LevelOfDetail}; Active: {Active}; Visible: {Visible}";
        }
    }
}
