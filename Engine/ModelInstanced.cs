using SharpDX;
using System;
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
        /// Instancing data per instance
        /// </summary>
        private VertexInstancingData[] instancingData = null;
        /// <summary>
        /// Model instance list
        /// </summary>
        private ModelInstance[] instances = null;
        /// <summary>
        /// Temporal instance list for rendering
        /// </summary>
        private ModelInstance[] instancesTmp = null;
        /// <summary>
        /// Write instancing data to graphics flag
        /// </summary>
        private bool hasDataToWrite = false;
        /// <summary>
        /// Independent transforms flag
        /// </summary>
        private bool hasIndependentTransforms = false;

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
                return instances[index];
            }
        }
        /// <summary>
        /// Gets visible instance count
        /// </summary>
        public int VisibleCount
        {
            get
            {
                return Visible ? Array.FindAll(instances, i => i.Visible).Length : 0;
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

            instances = Helper.CreateArray(InstanceCount, () => new ModelInstance(this, Description));
            instancingData = new VertexInstancingData[InstanceCount];

            MaximumCount = -1;

            hasIndependentTransforms = Description.TransformDependences?.Any() == true;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (instances.Any())
            {
                instances
                    .Where(i => i.Active)
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .ForAll(i => i.Update(context));

                instancesTmp = instances
                    .Where(i => i.Visible && i.LevelOfDetail != LevelOfDetail.None)
                    .ToArray();
            }

            //Process only visible instances
            UpdateInstancingData(context);
        }
        /// <summary>
        /// Updates instancing data buffer
        /// </summary>
        /// <param name="context">Update context</param>
        private void UpdateInstancingData(UpdateContext context)
        {
            if (!instancesTmp.Any())
            {
                return;
            }

            SortInstances(context.EyePosition);

            int instanceIndex = 0;

            for (int i = 0; i < instancesTmp.Length; i++)
            {
                var current = instancesTmp[i];
                if (current == null)
                {
                    continue;
                }

                current.AnimationController.Update(context.GameTime.ElapsedSeconds);

                instancingData[instanceIndex].Local = current.Manipulator.LocalTransform;
                instancingData[instanceIndex].TintColor = current.TintColor;
                instancingData[instanceIndex].TextureIndex = current.TextureIndex;
                instancingData[instanceIndex].MaterialIndex = current.MaterialIndex;
                instancingData[instanceIndex].AnimationOffset = current.AnimationController.AnimationOffset;
                instancingData[instanceIndex].AnimationOffsetB = current.AnimationController.TransitionOffset;
                instancingData[instanceIndex].AnimationInterpolation = current.AnimationController.TransitionInterpolationAmount;

                instanceIndex++;
            }

            //Mark to write instancing data
            hasDataToWrite = instanceIndex > 0;
        }
        /// <summary>
        /// Updates independent transforms
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        private void UpdateIndependentTransforms(string meshName)
        {
            if (!hasIndependentTransforms)
            {
                return;
            }

            int instanceIndex = 0;

            for (int i = 0; i < instancesTmp.Length; i++)
            {
                var current = instancesTmp[i];
                if (current == null)
                {
                    continue;
                }

                var currentTransform = instancingData[instanceIndex].Local;
                var localTransform = current.GetTransformByName(meshName);
                if (currentTransform != localTransform)
                {
                    instancingData[instanceIndex].Local = localTransform;

                    hasDataToWrite = true;
                }

                instanceIndex++;
            }

            if (hasDataToWrite)
            {
                BufferManager.WriteInstancingData(InstancingBuffer, instancingData);
            }
        }
        /// <summary>
        /// Updates the instances order
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private void SortInstances(Vector3 eyePosition)
        {
            //Sort by LOD, distance and id
            Array.Sort(instancesTmp, (i1, i2) =>
            {
                var i = i1.LevelOfDetail.CompareTo(i2.LevelOfDetail);

                if (i == 0)
                {
                    var da = Vector3.DistanceSquared(i1.Manipulator.Position, eyePosition);
                    var db = Vector3.DistanceSquared(i2.Manipulator.Position, eyePosition);
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

            if (hasDataToWrite)
            {
                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadows)} {context.ShadowMap} WriteInstancingData: BufferDescriptionIndex {InstancingBuffer.BufferDescriptionIndex} BufferOffset {InstancingBuffer.BufferOffset}");
                BufferManager.WriteInstancingData(InstancingBuffer, instancingData);
            }

            return DrawShadowsInstances(context);
        }
        /// <summary>
        /// Shadow drawing
        /// </summary>
        /// <param name="context">Context</param>
        private bool DrawShadowsInstances(DrawContextShadows context)
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
                var lodInstances = Array.FindAll(instancesTmp, i => i != null && i.LevelOfDetail == lod);
                if (lodInstances.Length <= 0)
                {
                    continue;
                }

                var drawingData = GetDrawingData(lod);
                if (drawingData == null)
                {
                    continue;
                }

                var startInstanceLocation = Array.IndexOf(instancesTmp, lodInstances[0]) + InstancingBuffer.BufferOffset;
                var instancesToDraw = Math.Min(maxCount, lodInstances.Length);
                if (instancesToDraw <= 0)
                {
                    continue;
                }

                maxCount -= instancesToDraw;
                instanceCount += instancesToDraw;

                foreach (string meshName in drawingData.Meshes.Keys)
                {
                    UpdateIndependentTransforms(meshName);

                    count += DrawShadowMesh(context, drawingData, meshName, instancesToDraw, startInstanceLocation);
                    count *= instanceCount;
                }
            }

            return count > 0;
        }
        /// <summary>
        /// Draws a mesh with a shadow map drawer
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="drawingData">Drawing data</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="instancesToDraw">Instance buffer length</param>
        /// <param name="startInstanceLocation">Instance buffer index</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawShadowMesh(DrawContextShadows context, DrawingData drawingData, string meshName, int instancesToDraw, int startInstanceLocation)
        {
            Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}. Index {startInstanceLocation} Length {instancesToDraw}.");

            int count = 0;

            var meshDict = drawingData.Meshes[meshName];

            foreach (string materialName in meshDict.Keys)
            {
                var mesh = meshDict[materialName];
                if (!mesh.Ready)
                {
                    Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}.{materialName} discard => Ready {mesh.Ready}");
                    continue;
                }

                var material = drawingData.Materials[materialName];

                var drawer = context.ShadowMap?.GetDrawer(mesh.VertextType, true, material.Material.IsTransparent);
                if (drawer == null)
                {
                    continue;
                }

                drawer.UpdateCastingLight(context);

                var materialState = new BuiltInDrawerMaterialState
                {
                    Material = material,
                    UseAnisotropic = false,
                    TextureIndex = 0,
                    TintColor = Color4.White,
                };
                drawer.UpdateMaterial(materialState);

                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}.{materialName} Index {startInstanceLocation} Length {instancesToDraw}.");
                if (drawer.Draw(context.DeviceContext, BufferManager, new[] { mesh }, instancesToDraw, startInstanceLocation))
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

            if (hasDataToWrite)
            {
                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(Draw)} WriteInstancingData: BufferDescriptionIndex {InstancingBuffer.BufferDescriptionIndex} BufferOffset {InstancingBuffer.BufferOffset} {context.DrawerMode}");
                BufferManager.WriteInstancingData(InstancingBuffer, instancingData);
            }

            return DrawInstances(context);
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        private bool DrawInstances(DrawContext context)
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
                var lodInstances = instancesTmp.Where(i => i?.LevelOfDetail == lod);
                if (!lodInstances.Any())
                {
                    continue;
                }

                var drawingData = GetDrawingData(lod);
                if (drawingData == null)
                {
                    continue;
                }

                var startInstanceLocation = Array.IndexOf(instancesTmp, lodInstances.First()) + InstancingBuffer.BufferOffset;
                var instancesToDraw = Math.Min(maxCount, lodInstances.Count());
                if (instancesToDraw <= 0)
                {
                    continue;
                }

                maxCount -= instancesToDraw;
                instanceCount += instancesToDraw;

                foreach (string meshName in drawingData.Meshes.Keys)
                {
                    UpdateIndependentTransforms(meshName);

                    count += DrawMesh(context, drawingData, meshName, instancesToDraw, startInstanceLocation);
                    count *= instanceCount;
                }
            }

            Counters.InstancesPerFrame += instanceCount;
            Counters.PrimitivesPerFrame += count;

            return count > 0;
        }
        /// <summary>
        /// Draws a mesh with a geometry drawer
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="drawingData">Drawing data</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="instancesToDraw">Instance buffer length</param>
        /// <param name="startInstanceLocation">Instance buffer index</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawMesh(DrawContext context, DrawingData drawingData, string meshName, int instancesToDraw, int startInstanceLocation)
        {
            Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}. Index {startInstanceLocation} Length {instancesToDraw}. {context.DrawerMode}");

            int count = 0;

            var meshDict = drawingData.Meshes[meshName];

            foreach (string materialName in meshDict.Keys)
            {
                var mesh = meshDict[materialName];
                if (!mesh.Ready)
                {
                    Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName} discard => Ready {mesh.Ready}");
                    continue;
                }

                var material = drawingData.Materials[materialName];

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

                var materialState = new BuiltInDrawerMaterialState
                {
                    Material = material,
                    UseAnisotropic = UseAnisotropicFiltering,
                    TextureIndex = 0,
                    TintColor = Color4.White,
                };
                drawer.UpdateMaterial(materialState);

                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName} Index {startInstanceLocation} Length {instancesToDraw}");
                if (drawer.Draw(context.DeviceContext, BufferManager, new[] { mesh }, instancesToDraw, startInstanceLocation))
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
            if (positions?.Any() == true && instances?.Length > 0)
            {
                for (int i = 0; i < instances.Length; i++)
                {
                    var instance = instances[i];

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
            if (transforms?.Any() == true && instances?.Length > 0)
            {
                for (int i = 0; i < instances.Length; i++)
                {
                    var instance = instances[i];

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
            return new ReadOnlyCollection<ModelInstance>(instances);
        }
        /// <inheritdoc/>
        public IEnumerable<T> GetComponents<T>()
        {
            return new ReadOnlyCollection<T>(instances.Where(i => i.Visible).OfType<T>().ToArray());
        }

        /// <inheritdoc/>
        public override bool Cull(ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (instancesTmp?.Length > 0)
            {
                var item = Array.Find(instancesTmp, i =>
                {
                    return i.Visible && !i.Cull(volume, out float iDistance);
                });

                if (item != null)
                {
                    distance = 0;
                    return false;
                }
            }

            return true;
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
                Instances = instances.Select(i => i.GetState()).ToArray(),
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
                instances[i].SetState(instanceState);
            }
        }
    }
}
