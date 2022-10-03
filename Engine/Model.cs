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

            int count = 0;
            foreach (var mesh in DrawingData.Meshes)
            {
                count += DrawShadowMesh(context, mesh.Key, mesh.Value);
            }
        }
        /// <summary>
        /// Draws a mesh shadow
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="meshDict">Mesh dictionary</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawShadowMesh(DrawContextShadows context, string meshName, Dictionary<string, Mesh> meshDict)
        {
            Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}.");

            int count = 0;

            var localTransform = GetTransformByName(meshName);

            foreach (string materialName in meshDict.Keys)
            {
                var mesh = meshDict[materialName];
                if (!mesh.Ready)
                {
                    Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}.{materialName} discard => Ready {mesh.Ready}");
                    continue;
                }

                var material = DrawingData.Materials[materialName];

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
                drawer.UpdateMesh(meshState);

                var materialState = new BuiltInDrawerMaterialState
                {
                    Material = material,
                    UseAnisotropic = false,
                    TextureIndex = TextureIndex,
                    TintColor = Color4.White,
                };
                drawer.UpdateMaterial(materialState);

                Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}.{materialName}.");
                if (drawer.Draw(BufferManager, new[] { mesh }))
                {
                    count += mesh.Count;
                }
            }

            return count;
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

            int count = 0;
            foreach (var mesh in DrawingData.Meshes)
            {
                count += DrawMesh(context, mesh.Key, mesh.Value);
            }

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += count;
        }
        /// <summary>
        /// Draws a mesh
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="meshDict">Mesh dictionary</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawMesh(DrawContext context, string meshName, Dictionary<string, Mesh> meshDict)
        {
            Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(DrawMesh)}: {meshName}.");

            int count = 0;

            var localTransform = GetTransformByName(meshName);

            foreach (string materialName in meshDict.Keys)
            {
                var mesh = meshDict[materialName];
                if (!mesh.Ready)
                {
                    Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName} discard => Ready {mesh.Ready}");
                    continue;
                }

                var material = DrawingData.Materials[materialName];

                bool draw = context.ValidateDraw(BlendMode, material.Material.IsTransparent);
                if (!draw)
                {
                    Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName} discard => BlendMode {BlendMode}");
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
                drawer.UpdateMesh(meshState);

                var materialState = new BuiltInDrawerMaterialState
                {
                    Material = material,
                    UseAnisotropic = UseAnisotropicFiltering,
                    TextureIndex = TextureIndex,
                    TintColor = TintColor,
                };
                drawer.UpdateMaterial(materialState);

                Logger.WriteTrace(this, $"{nameof(Model)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName}.");
                if (drawer.Draw(BufferManager, new[] { mesh }))
                {
                    count += mesh.Count;
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
            Logger.WriteTrace(this, $"{nameof(Model)} {Name} => LOD: {LevelOfDetail}; InvalidateCache");

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
            if (geometryType != GeometryTypes.Object && DrawingData?.HullMesh?.Any() == true)
            {
                return Triangle.Transform(DrawingData.HullMesh, Manipulator.LocalTransform);
            }

            return GetTriangles();
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

            var mesh = GetGeometry(GeometryTypes.Hull);

            return Intersection.SphereIntersectsMesh(sphere, mesh, out result);
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
                return (IntersectionVolumeMesh)GetGeometry(GeometryTypes.Hull).ToArray();
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
}
