using SharpDX;
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
    public class Model : BaseModel, ITransformable3D, IRayPickable<Triangle>, IIntersectable, ICullable
    {
        /// <summary>
        /// Update point cache flag
        /// </summary>
        private bool updatePoints = true;
        /// <summary>
        /// Update triangle cache flag
        /// </summary>
        private bool updateTriangles = true;
        /// <summary>
        /// Points cache
        /// </summary>
        private Vector3[] positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private Triangle[] triangleCache = null;
        /// <summary>
        /// Coarse bounding sphere
        /// </summary>
        private BoundingSphere? coarseBoundingSphere = null;
        /// <summary>
        /// Bounding sphere
        /// </summary>
        private BoundingSphere? boundingSphere = null;
        /// <summary>
        /// Bounding box
        /// </summary>
        private BoundingBox? boundingBox = null;
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetail levelOfDetail = LevelOfDetail.None;

        /// <summary>
        /// Current drawing data
        /// </summary>
        protected DrawingData DrawingData { get; private set; }
        /// <summary>
        /// Model parts collection
        /// </summary>
        protected List<ModelPart> ModelParts = new List<ModelPart>();
        /// <summary>
        /// Gets if model has volumes
        /// </summary>
        protected bool HasVolumes
        {
            get
            {
                return positionCache?.Any() == true;
            }
        }

        /// <summary>
        /// Model manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }
        /// <summary>
        /// Animation controller
        /// </summary>
        public AnimationController AnimationController { get; set; } = new AnimationController();
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; }
        /// <summary>
        /// Animation palette offset
        /// </summary>
        public uint AnimationOffset { get; set; }
        /// <summary>
        /// Level of detail
        /// </summary>
        public LevelOfDetail LevelOfDetail
        {
            get
            {
                return levelOfDetail;
            }
            set
            {
                levelOfDetail = GetLODNearest(value);
                DrawingData = GetDrawingData(levelOfDetail);
            }
        }
        /// <summary>
        /// Gets the current model lights collection
        /// </summary>
        public IEnumerable<ISceneLight> Lights { get; protected set; } = new ISceneLight[] { };
        /// <summary>
        /// Gets the model part by name
        /// </summary>
        /// <param name="name">Part name</param>
        /// <returns>Returns the model part name</returns>
        public ModelPart this[string name]
        {
            get
            {
                return ModelParts.Find(p => p.Name == name);
            }
        }
        /// <summary>
        /// Gets the model part count
        /// </summary>
        public int ModelPartCount
        {
            get
            {
                return ModelParts.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Model(Scene scene, ModelDescription description)
            : base(scene, description)
        {
            TextureIndex = description.TextureIndex;

            if (description.TransformDependences?.Any() == true)
            {
                var parents = Array.FindAll(description.TransformDependences, i => i == -1);
                if (parents == null || parents.Length != 1)
                {
                    throw new EngineException("Model with transform dependences must have one (and only one) parent mesh identified by -1");
                }

                for (int i = 0; i < description.TransformNames.Length; i++)
                {
                    ModelParts.Add(new ModelPart(description.TransformNames[i]));
                }

                for (int i = 0; i < description.TransformNames.Length; i++)
                {
                    var thisName = description.TransformNames[i];
                    var thisMan = this[thisName].Manipulator;
                    thisMan.Updated += new EventHandler(ManipulatorUpdated);

                    var parentIndex = description.TransformDependences[i];
                    if (parentIndex >= 0)
                    {
                        var parentName = description.TransformNames[parentIndex];

                        thisMan.Parent = this[parentName].Manipulator;
                    }
                    else
                    {
                        Manipulator = thisMan;
                    }
                }
            }
            else
            {
                Manipulator = new Manipulator3D();
                Manipulator.Updated += new EventHandler(ManipulatorUpdated);
            }

            var drawData = GetDrawingData(LevelOfDetail.High);
            Lights = drawData?.Lights.Select(l => l.Clone()).ToArray() ?? new ISceneLight[] { };

            AnimationController.AnimationOffsetChanged += (s, a) => { InvalidateCache(); };
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (DrawingData?.SkinningData != null)
            {
                if (AnimationController.Playing)
                {
                    InvalidateCache();
                }

                AnimationController.Update(context.GameTime.ElapsedSeconds, DrawingData.SkinningData);
                AnimationOffset = AnimationController.AnimationOffset;
            }

            if (ModelParts.Count > 0)
            {
                ModelParts.ForEach(p => p.Manipulator.Update(context.GameTime));
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

                effect.UpdatePerObject(AnimationOffset, material, TextureIndex);

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

                effect.UpdatePerObject(AnimationOffset, material, TextureIndex, UseAnisotropicFiltering);

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

            if (HasVolumes)
            {
                if (coarseBoundingSphere.HasValue)
                {
                    cull = volume.Contains(coarseBoundingSphere.Value) == ContainmentType.Disjoint;
                }
                else if (SphericVolume)
                {
                    cull = volume.Contains(GetBoundingSphere()) == ContainmentType.Disjoint;
                }
                else
                {
                    cull = volume.Contains(GetBoundingBox()) == ContainmentType.Disjoint;
                }
            }
            else
            {
                cull = false;
            }

            if (!cull)
            {
                var eyePosition = volume.Position;

                distance = Vector3.DistanceSquared(Manipulator.Position, eyePosition);

                LevelOfDetail = GameEnvironment.GetLOD(
                    eyePosition,
                    coarseBoundingSphere,
                    Manipulator.LocalTransform);
            }

            return cull;
        }

        /// <summary>
        /// Sets a new manipulator to this instance
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        public void SetManipulator(Manipulator3D manipulator)
        {
            Manipulator.Updated -= ManipulatorUpdated;
            Manipulator = null;

            Manipulator = manipulator;
            Manipulator.Updated += ManipulatorUpdated;
        }
        /// <summary>
        /// Occurs when manipulator transform updated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void ManipulatorUpdated(object sender, EventArgs e)
        {
            InvalidateCache();

            coarseBoundingSphere = GetBoundingSphere();
        }

        /// <summary>
        /// Gets the transform by transform name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Retusn the transform of the specified transform name</returns>
        public Matrix GetTransformByName(string name)
        {
            var part = ModelParts.Find(p => p.Name == name);
            if (part != null)
            {
                return part.Manipulator.FinalTransform;
            }
            else
            {
                return Manipulator.FinalTransform;
            }
        }

        /// <summary>
        /// Invalidates the internal cache
        /// </summary>
        private void InvalidateCache()
        {
            Logger.WriteTrace(this, $"Model InvalidateCache");

            updatePoints = true;
            updateTriangles = true;

            boundingSphere = null;
            boundingBox = null;
        }
        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or position list</returns>
        public IEnumerable<Vector3> GetPoints(bool refresh = false)
        {
            bool update = refresh || updatePoints;

            if (update)
            {
                IEnumerable<Vector3> cache;

                var drawingData = GetDrawingData(GetLODMinimum());
                if (drawingData == null)
                {
                    return new Vector3[] { };
                }

                if (drawingData.SkinningData != null)
                {
                    cache = drawingData.GetPoints(
                        Manipulator.LocalTransform,
                        AnimationController.GetCurrentPose(drawingData.SkinningData),
                        update);
                }
                else
                {
                    cache = drawingData.GetPoints(
                        Manipulator.LocalTransform,
                        update);
                }

                positionCache = cache.ToArray();

                updatePoints = false;
            }

            return positionCache?.ToArray() ?? new Vector3[] { };
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns null or triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(bool refresh = false)
        {
            bool update = refresh || updateTriangles;

            if (update)
            {
                IEnumerable<Triangle> cache;

                var drawingData = GetDrawingData(GetLODMinimum());
                if (drawingData == null)
                {
                    return new Triangle[] { };
                }

                if (drawingData.SkinningData != null)
                {
                    cache = drawingData.GetTriangles(
                        Manipulator.LocalTransform,
                        AnimationController.GetCurrentPose(drawingData.SkinningData),
                        update);
                }
                else
                {
                    cache = drawingData.GetTriangles(
                        Manipulator.LocalTransform,
                        update);
                }

                triangleCache = cache.ToArray();

                updateTriangles = false;
            }

            return triangleCache?.ToArray() ?? new Triangle[] { };
        }
        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere(bool refresh = false)
        {
            if (refresh || boundingSphere == null)
            {
                var points = GetPoints(refresh);
                if (points.Any())
                {
                    boundingSphere = BoundingSphere.FromPoints(points.ToArray());
                }
            }

            return boundingSphere ?? new BoundingSphere(Manipulator.Position, 0f);
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox(bool refresh = false)
        {
            if (refresh || boundingBox == null)
            {
                var points = GetPoints(refresh);
                if (points.Any())
                {
                    boundingBox = BoundingBox.FromPoints(points.ToArray());
                }
            }

            return boundingBox ?? new BoundingBox(Manipulator.Position, Manipulator.Position);
        }

        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickNearest(Ray ray, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickNearest(this, ray, out result);
        }
        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickNearest(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickNearest(this, ray, rayPickingParams, out result);
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickFirst(Ray ray, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickFirst(this, ray, out result);
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickFirst(Ray ray, RayPickingParams rayPickingParams, out PickingResult<Triangle> result)
        {
            return RayPickingHelper.PickFirst(this, ray, rayPickingParams, out result);
        }
        /// <summary>
        /// Get all picking positions of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickAll(Ray ray, out IEnumerable<PickingResult<Triangle>> results)
        {
            return RayPickingHelper.PickAll(this, ray, out results);
        }
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public bool PickAll(Ray ray, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<Triangle>> results)
        {
            return RayPickingHelper.PickAll(this, ray, rayPickingParams, out results);
        }

        /// <summary>
        /// Gets internal volume
        /// </summary>
        /// <param name="full"></param>
        /// <returns>Returns internal volume</returns>
        public IEnumerable<Triangle> GetVolume(bool full)
        {
            if (!full && DrawingData?.VolumeMesh?.Any() == true)
            {
                return Triangle.Transform(DrawingData.VolumeMesh, Manipulator.LocalTransform);
            }

            return GetTriangles(true);
        }

        /// <summary>
        /// Gets whether the sphere intersects with the current object
        /// </summary>
        /// <param name="sphere">Sphere</param>
        /// <param name="result">Picking results</param>
        /// <returns>Returns true if intersects</returns>
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

        /// <summary>
        /// Gets whether the actual object have intersection with the intersectable or not
        /// </summary>
        /// <param name="detectionModeThis">Detection mode for this object</param>
        /// <param name="other">Other intersectable</param>
        /// <param name="detectionModeOther">Detection mode for the other object</param>
        /// <returns>Returns true if have intersection</returns>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectable other, IntersectDetectionMode detectionModeOther)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, other, detectionModeOther);
        }
        /// <summary>
        /// Gets whether the actual object have intersection with the volume or not
        /// </summary>
        /// <param name="detectionModeThis">Detection mode for this object</param>
        /// <param name="volume">Volume</param>
        /// <returns>Returns true if have intersection</returns>
        public bool Intersects(IntersectDetectionMode detectionModeThis, IIntersectionVolume volume)
        {
            return IntersectionHelper.Intersects(this, detectionModeThis, volume);
        }

        /// <summary>
        /// Gets the intersection volume based on the specified detection mode
        /// </summary>
        /// <param name="detectionMode">Detection mode</param>
        /// <returns>Returns an intersection volume</returns>
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
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Model> AddComponentModel(this Scene scene, ModelDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            Model component = null;

            await Task.Run(() =>
            {
                component = new Model(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
