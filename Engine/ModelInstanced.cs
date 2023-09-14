using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Instaced model
    /// </summary>
    public class ModelInstanced : BaseModel<ModelInstancedDescription>, IComposed, IHasGameState
    {
        /// <summary>
        /// Model instance list
        /// </summary>
        private ModelInstance[] instancesAll = null;
        /// <summary>
        /// Visible instance list
        /// </summary>
        private ModelInstance[] instancesVisible = null;
        /// <summary>
        /// Independent transforms flag
        /// </summary>
        private bool hasIndependentTransforms = false;
        /// <summary>
        /// Culling instance dictionary by cull index
        /// </summary>
        private readonly ConcurrentDictionary<int, ModelInstance[]> cullInstances = new();

        /// <summary>
        /// Instancing buffer
        /// </summary>
        protected BufferDescriptor InstancingBuffer { get; private set; } = null;

        /// <summary>
        /// Gets manipulator per instance list
        /// </summary>
        /// <returns>Gets manipulator per instance list</returns>
        public ModelInstance this[int index]
        {
            get
            {
                return instancesAll[index];
            }
        }
        /// <summary>
        /// Gets visible instance count
        /// </summary>
        public int VisibleCount
        {
            get
            {
                return Visible ? Array.FindAll(instancesAll, i => i.Visible).Length : 0;
            }
        }
        /// <summary>
        /// Gets or sets the maximum number of instances to draw
        /// </summary>
        public int MaximumCount { get; set; }
        /// <inheritdoc/>
        /// <remarks>Always uses the highest level of detail drawing data.</remarks>
        public override ISkinningData SkinningData
        {
            get
            {
                return GetDrawingData(LevelOfDetail.High)?.SkinningData;
            }
        }

        /// <summary>
        /// Prepare the instancing buffer data
        /// </summary>
        /// <param name="instances">Instace list</param>
        /// <param name="pointOfView">Point of view</param>
        /// <returns>Returns the instancing data for the instancing buffer</returns>
        private static VertexInstancingData[] PrepareInstancingBuffer(ModelInstance[] instances, Vector3 pointOfView)
        {
            if (!instances.Any())
            {
                return Array.Empty<VertexInstancingData>();
            }

            if (instances.Length > 1)
            {
                SortInstances(instances, pointOfView);
            }

            return instances
                .Where(i => i != null)
                .Select(i => new VertexInstancingData
                {
                    Local = i.Manipulator.LocalTransform,
                    TintColor = i.TintColor,
                    TextureIndex = i.TextureIndex,
                    MaterialIndex = i.MaterialIndex,
                    AnimationOffset = i.AnimationController.AnimationOffset,
                    AnimationOffsetB = i.AnimationController.TransitionOffset,
                    AnimationInterpolation = i.AnimationController.TransitionInterpolationAmount,
                })
                .ToArray();
        }
        /// <summary>
        /// Updates the instances order
        /// </summary>
        /// <param name="instances">Instace list</param>
        /// <param name="pointOfView">Point of view</param>
        private static void SortInstances(ModelInstance[] instances, Vector3 pointOfView)
        {
            //Sort by LOD, distance and id
            Array.Sort(instances, (i1, i2) =>
            {
                var i = i1.LevelOfDetail.CompareTo(i2.LevelOfDetail);

                if (i == 0)
                {
                    var da = Vector3.DistanceSquared(i1.Manipulator.Position, pointOfView);
                    var db = Vector3.DistanceSquared(i2.Manipulator.Position, pointOfView);
                    i = da.CompareTo(db);
                }

                if (i == 0)
                {
                    i = i1.Id.CompareTo(i2.Id);
                }

                return i;
            });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public ModelInstanced(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ModelInstanced()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BufferManager.RemoveInstancingData(InstancingBuffer);
                InstancingBuffer = null;
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(ModelInstancedDescription description)
        {
            await base.InitializeAssets(description);

            if (Description.Instances <= 0)
            {
                throw new ArgumentException($"Instances parameter must be more than 0: {Description.Instances}");
            }

            InstancingBuffer = BufferManager.AddInstancingData($"{Name}.Instances", true, Description.Instances);

            await InitializeGeometry(description, InstancingBuffer);

            InstanceCount = Description.Instances;

            instancesAll = Helper.CreateArray(InstanceCount, () => new ModelInstance(this, Description));

            MaximumCount = -1;

            hasIndependentTransforms = Description.TransformDependences?.Any() == true;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (!instancesAll.Any())
            {
                return;
            }

            // Store visible instances
            instancesVisible = instancesAll
                .Where(i => i.Visible && i.LevelOfDetail != LevelOfDetail.None)
                .ToArray();

            // Update each active instance
            instancesAll
                .Where(i => i.Active)
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .ForAll(i => i.Update(context));
        }
        /// <summary>
        /// Updates independent transforms
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="instanceList">Instance list</param>
        /// <param name="instancingData">Instancing data</param>
        private void UpdateIndependentTransforms(IEngineDeviceContext dc, string meshName, ModelInstance[] instanceList, VertexInstancingData[] instancingData)
        {
            if (!hasIndependentTransforms)
            {
                return;
            }

            int instanceIndex = 0;

            for (int i = 0; i < instanceList.Length; i++)
            {
                var current = instanceList[i];
                if (current == null)
                {
                    continue;
                }

                var currentTransform = instancingData[instanceIndex].Local;
                var localTransform = current.GetTransformByName(meshName);
                if (currentTransform != localTransform)
                {
                    instancingData[instanceIndex].Local = localTransform;
                }

                instanceIndex++;
            }

            BufferManager.WriteInstancingData(dc, InstancingBuffer, instancingData);
        }
        /// <summary>
        /// Gets the limit of instances to draw
        /// </summary>
        /// <returns>Returns the number of maximum instances to draw</returns>
        private int GetMaxCount()
        {
            return MaximumCount >= 0 ? Math.Min(MaximumCount, InstanceCount) : InstanceCount;
        }

        /// <inheritdoc/>
        public override bool DrawShadows(DrawContextShadows context)
        {
            if (!Visible)
            {
                return false;
            }

            if (!InstancingBuffer.Ready)
            {
                return false;
            }

            if (!cullInstances.TryGetValue(context.ShadowMap.CullIndex, out var cullInstanceList))
            {
                return false;
            }
            var cullInstanceData = PrepareInstancingBuffer(cullInstanceList, context.ShadowMap.LightSource.Position);
            Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadows)} {context.ShadowMap} WriteInstancingData: BufferDescriptionIndex {InstancingBuffer.BufferDescriptionIndex} BufferOffset {InstancingBuffer.BufferOffset}");
            BufferManager.WriteInstancingData(context.DeviceContext, InstancingBuffer, cullInstanceData);
            return DrawShadowsInstances(context, cullInstanceList, cullInstanceData);
        }
        /// <summary>
        /// Shadow drawing
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="cullInstanceList">Cull instance list</param>
        /// <param name="cullInstancingData">Cull instances data</param>
        private bool DrawShadowsInstances(DrawContextShadows context, ModelInstance[] cullInstanceList, VertexInstancingData[] cullInstancingData)
        {
            int count = 0;
            int instanceCount = 0;

            int maxCount = GetMaxCount();

            //Render by level of detail
            for (int l = 1; l < (int)LevelOfDetail.Minimum + 1; l *= 2)
            {
                if (maxCount <= 0)
                {
                    break;
                }

                LevelOfDetail lod = (LevelOfDetail)l;

                //Get instances in this LOD
                var lodInstances = Array.FindAll(cullInstanceList, i => i != null && i.LevelOfDetail == lod);
                if (lodInstances.Length <= 0)
                {
                    continue;
                }

                var drawingData = GetDrawingData(lod);
                if (drawingData == null)
                {
                    continue;
                }

                var startInstanceLocation = Array.IndexOf(cullInstanceList, lodInstances[0]) + InstancingBuffer.BufferOffset;
                var instancesToDraw = Math.Min(maxCount, lodInstances.Length);
                if (instancesToDraw <= 0)
                {
                    continue;
                }

                maxCount -= instancesToDraw;
                instanceCount += instancesToDraw;

                int dCount = DrawShadowMesh(context, drawingData, cullInstanceList, cullInstancingData, instancesToDraw, startInstanceLocation);
                dCount *= instancesToDraw;

                count += dCount;
            }

            return count > 0;
        }
        /// <summary>
        /// Draws a mesh with a shadow map drawer
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="drawingData">Drawing data</param>
        /// <param name="cullInstanceList">Cull instance list</param>
        /// <param name="cullInstancingData">Cull instances data</param>
        /// <param name="instancesToDraw">Instance buffer length</param>
        /// <param name="startInstanceLocation">Instance buffer index</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawShadowMesh(DrawContextShadows context, DrawingData drawingData, ModelInstance[] cullInstanceList, VertexInstancingData[] cullInstancingData, int instancesToDraw, int startInstanceLocation)
        {
            int count = 0;

            var dc = context.DeviceContext;

            foreach (var meshMaterial in drawingData.IterateMaterials())
            {
                string materialName = meshMaterial.MaterialName;
                var material = meshMaterial.Material;
                string meshName = meshMaterial.MeshName;
                var mesh = meshMaterial.Mesh;

                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}. Index {startInstanceLocation} Length {instancesToDraw}.");

                var drawer = context.ShadowMap?.GetDrawer(mesh.VertextType, true, material.Material.IsTransparent);
                if (drawer == null)
                {
                    continue;
                }

                UpdateIndependentTransforms(dc, meshName, cullInstanceList, cullInstancingData);

                drawer.UpdateCastingLight(context);

                var materialState = new BuiltInDrawerMaterialState
                {
                    Material = material,
                    UseAnisotropic = false,
                    TextureIndex = 0,
                    TintColor = Color4.White,
                };
                drawer.UpdateMaterial(dc, materialState);

                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}.{materialName} Index {startInstanceLocation} Length {instancesToDraw}.");
                if (drawer.Draw(dc, BufferManager, new[] { mesh }, instancesToDraw, startInstanceLocation))
                {
                    count += mesh.Count;
                }
            }

            return count;
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (!InstancingBuffer.Ready)
            {
                return false;
            }

            if (!cullInstances.TryGetValue(0, out var cullInstanceList))
            {
                return false;
            }
            var cullInstanceData = PrepareInstancingBuffer(cullInstanceList, context.Camera.Position);
            Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(Draw)} WriteInstancingData: BufferDescriptionIndex {InstancingBuffer.BufferDescriptionIndex} BufferOffset {InstancingBuffer.BufferOffset} {context.DrawerMode}");
            BufferManager.WriteInstancingData(context.DeviceContext, InstancingBuffer, cullInstanceData);
            return DrawInstances(context, cullInstanceList, cullInstanceData);
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="cullInstanceList">Cull instance list</param>
        /// <param name="cullInstancingData">Cull instances data</param>
        private bool DrawInstances(DrawContext context, ModelInstance[] cullInstanceList, VertexInstancingData[] cullInstancingData)
        {
            int count = 0;
            int instanceCount = 0;

            int maxCount = GetMaxCount();

            //Render by level of detail
            for (int l = 1; l < (int)LevelOfDetail.Minimum + 1; l *= 2)
            {
                if (maxCount <= 0)
                {
                    break;
                }

                LevelOfDetail lod = (LevelOfDetail)l;

                //Get instances in this LOD
                var lodInstances = cullInstanceList.Where(i => i?.LevelOfDetail == lod);
                if (!lodInstances.Any())
                {
                    continue;
                }

                var drawingData = GetDrawingData(lod);
                if (drawingData == null)
                {
                    continue;
                }

                var startInstanceLocation = Array.IndexOf(cullInstanceList, lodInstances.First()) + InstancingBuffer.BufferOffset;
                var instancesToDraw = Math.Min(maxCount, lodInstances.Count());
                if (instancesToDraw <= 0)
                {
                    continue;
                }

                maxCount -= instancesToDraw;
                instanceCount += instancesToDraw;

                int dCount = DrawMesh(context, drawingData, cullInstanceList, cullInstancingData, instancesToDraw, startInstanceLocation);
                dCount *= instancesToDraw;

                count += dCount;
            }

            return count > 0;
        }
        /// <summary>
        /// Draws a mesh with a geometry drawer
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="drawingData">Drawing data</param>
        /// <param name="cullInstanceList">Cull instance list</param>
        /// <param name="cullInstancingData">Cull instances data</param>
        /// <param name="instancesToDraw">Instance buffer length</param>
        /// <param name="startInstanceLocation">Instance buffer index</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawMesh(DrawContext context, DrawingData drawingData, ModelInstance[] cullInstanceList, VertexInstancingData[] cullInstancingData, int instancesToDraw, int startInstanceLocation)
        {
            int count = 0;

            var dc = context.DeviceContext;

            foreach (var meshMaterial in drawingData.IterateMaterials())
            {
                string materialName = meshMaterial.MaterialName;
                var material = meshMaterial.Material;
                string meshName = meshMaterial.MeshName;
                var mesh = meshMaterial.Mesh;

                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}. Index {startInstanceLocation} Length {instancesToDraw}. {context.DrawerMode}");

                bool draw = context.ValidateDraw(BlendMode, material.Material.IsTransparent);
                if (!draw)
                {
                    Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName} discard => BlendMode {BlendMode}");
                    continue;
                }

                var drawer = GetDrawer(context.DrawerMode, mesh.VertextType, true);
                if (drawer == null)
                {
                    continue;
                }

                UpdateIndependentTransforms(dc, meshName, cullInstanceList, cullInstancingData);

                var materialState = new BuiltInDrawerMaterialState
                {
                    Material = material,
                    UseAnisotropic = UseAnisotropicFiltering,
                    TextureIndex = 0,
                    TintColor = Color4.White,
                };
                drawer.UpdateMaterial(dc, materialState);

                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName} Index {startInstanceLocation} Length {instancesToDraw}");
                if (drawer.Draw(dc, BufferManager, new[] { mesh }, instancesToDraw, startInstanceLocation))
                {
                    count += mesh.Count;
                }
            }

            return count;
        }

        /// <summary>
        /// Set instance positions
        /// </summary>
        /// <param name="positions">New positions</param>
        public void SetPositions(IEnumerable<Vector3> positions)
        {
            if (positions?.Any() == true && instancesAll?.Length > 0)
            {
                for (int i = 0; i < instancesAll.Length; i++)
                {
                    var instance = instancesAll[i];

                    if (i < positions.Count())
                    {
                        instance.Manipulator.SetPosition(positions.ElementAt(i), true);
                        instance.Active = true;
                        instance.Visible = true;
                    }
                    else
                    {
                        instance.Active = false;
                        instance.Visible = false;
                    }
                }
            }
        }
        /// <summary>
        /// Sets instance transforms
        /// </summary>
        /// <param name="transforms">Transform matrix list</param>
        public void SetTransforms(IEnumerable<Matrix> transforms)
        {
            if (transforms?.Any() == true && instancesAll?.Length > 0)
            {
                for (int i = 0; i < instancesAll.Length; i++)
                {
                    var instance = instancesAll[i];

                    if (i < transforms.Count())
                    {
                        instance.Manipulator.SetTransform(transforms.ElementAt(i));
                        instance.Active = true;
                        instance.Visible = true;
                    }
                    else
                    {
                        instance.Active = false;
                        instance.Visible = false;
                    }
                }
            }
        }
        /// <summary>
        /// Gets the instance list
        /// </summary>
        /// <returns>Returns an array with the instance list</returns>
        public IEnumerable<ModelInstance> GetInstances()
        {
            return new ReadOnlyCollection<ModelInstance>(instancesAll);
        }
        /// <inheritdoc/>
        public IEnumerable<T> GetComponents<T>()
        {
            return new ReadOnlyCollection<T>(instancesAll.Where(i => i.Visible).OfType<T>().ToArray());
        }

        /// <summary>
        /// Gets the instances bounding sphere list
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        public IEnumerable<BoundingSphere> GetBoundingSpheres(bool refresh = false)
        {
            foreach (var instance in GetInstances())
            {
                yield return instance.GetBoundingSphere(refresh);
            }
        }
        /// <summary>
        /// Gets the instances bounding box list
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        public IEnumerable<BoundingBox> GetBoundingBoxes(bool refresh = false)
        {
            foreach (var instance in GetInstances())
            {
                yield return instance.GetBoundingBox(refresh);
            }
        }
        /// <summary>
        /// Gets the instances oriented bounding box list
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        public IEnumerable<OrientedBoundingBox> GetOrientedBoundingBoxes(bool refresh = false)
        {
            foreach (var instance in GetInstances())
            {
                yield return instance.GetOrientedBoundingBox(refresh);
            }
        }

        /// <inheritdoc/>
        public override bool Cull(int cullIndex, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            // Copy array
            var tmp = instancesVisible?.ToArray() ?? Array.Empty<ModelInstance>();

            if (tmp.Length <= 0)
            {
                // Culled
                return true;
            }

            var items = tmp
                .Where(i => i.Visible)
                .Select(i =>
                {
                    var cull = i.Cull(cullIndex, volume, out float iDistance);

                    return new { Instance = i, Cull = cull, Distance = iDistance };
                })
                .Where(i => !i.Cull)
                .OrderBy(i => i.Distance)
                .ToArray();

            if (!items.Any())
            {
                // Culled
                return true;
            }

            // Store selection
            var data = items.Select(i => i.Instance).ToArray();
            cullInstances.AddOrUpdate(cullIndex, data, (k, v) => data);

            // Visible
            distance = items[0].Distance;
            return false;
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new ModelInstancedState
            {
                Name = Name,
                Active = Active,
                Visible = Visible,
                Usage = Usage,
                Layer = Layer,
                OwnerId = Owner?.Name,

                MaximumCount = MaximumCount,
                Instances = instancesAll.Select(i => i.GetState()).ToArray(),
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not ModelInstancedState modelInstancedState)
            {
                return;
            }

            Name = modelInstancedState.Name;
            Active = modelInstancedState.Active;
            Visible = modelInstancedState.Visible;
            Usage = modelInstancedState.Usage;
            Layer = modelInstancedState.Layer;
            Owner = Scene.Components.ById(modelInstancedState.OwnerId);
            MaximumCount = modelInstancedState.MaximumCount;
            for (int i = 0; i < modelInstancedState.Instances.Count(); i++)
            {
                var instanceState = modelInstancedState.Instances.ElementAt(i);
                instancesAll[i].SetState(instanceState);
            }
        }
    }
}
