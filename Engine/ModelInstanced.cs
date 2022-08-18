using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

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
        /// Temporal instance listo for rendering
        /// </summary>
        private ModelInstance[] instancesTmp = null;
        /// <summary>
        /// Write instancing data to graphics flag
        /// </summary>
        private bool hasDataToWrite = false;
        /// <summary>
        /// Independant transforms flag
        /// </summary>
        private bool hasIndependantTransforms = false;

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

        /// <inheritdoc/>
        public override async Task InitializeAssets(ModelInstancedDescription description)
        {
            await base.InitializeAssets(description);

            if (Description.Instances <= 0)
            {
                throw new ArgumentException($"Instances parameter must be more than 0: {Description.Instances}");
            }

            InstanceCount = Description.Instances;

            instances = Helper.CreateArray(InstanceCount, () => new ModelInstance(this, Description));
            instancingData = new VertexInstancingData[InstanceCount];

            MaximumCount = -1;

            hasIndependantTransforms = Description.TransformDependences?.Any() == true;
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
        /// Updates independant transforms
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        private void UpdateIndependantTransforms(string meshName)
        {
            if (!hasIndependantTransforms)
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
        public override void DrawShadows(DrawContextShadows context)
        {
            if (!Visible)
            {
                return;
            }

            if (!InstancingBuffer.Ready)
            {
                return;
            }

            if (hasDataToWrite)
            {
                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadows)} {context.ShadowMap} WriteInstancingData: BufferDescriptionIndex {InstancingBuffer.BufferDescriptionIndex} BufferOffset {InstancingBuffer.BufferOffset}");
                BufferManager.WriteInstancingData(InstancingBuffer, instancingData);
            }

            var effect = context.ShadowMap.GetEffect();
            if (effect == null)
            {
                return;
            }

            DrawShadows(context, effect);
        }
        /// <summary>
        /// Shadow drawing
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="effect">Drawer</param>
        private void DrawShadows(DrawContextShadows context, IShadowMapDrawer effect)
        {
            int count = 0;
            int instanceCount = 0;

            effect.UpdatePerFrame(Matrix.Identity, context);

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

                var index = Array.IndexOf(instancesTmp, lodInstances[0]) + InstancingBuffer.BufferOffset;
                var length = Math.Min(maxCount, lodInstances.Length);
                if (length <= 0)
                {
                    continue;
                }

                maxCount -= length;
                instanceCount += length;

                foreach (string meshName in drawingData.Meshes.Keys)
                {
                    UpdateIndependantTransforms(meshName);

                    count += DrawShadowMesh(effect, drawingData, meshName, index, length);
                    count *= instanceCount;
                }
            }
        }
        /// <summary>
        /// Draws a mesh with a shadow map drawer
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="drawingData">Drawing data</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="index">Instance buffer index</param>
        /// <param name="length">Instance buffer length</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawShadowMesh(IShadowMapDrawer effect, DrawingData drawingData, string meshName, int index, int length)
        {
            Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}. Index {index} Length {length}.");

            int count = 0;

            var graphics = Game.Graphics;

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

                count += mesh.Count;

                var materialInfo = new MaterialShadowDrawInfo
                {
                    Material = material,
                };

                effect.UpdatePerObject(AnimationShadowDrawInfo.Empty, materialInfo, 0);

                if (!BufferManager.SetIndexBuffer(mesh.IndexBuffer))
                {
                    Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}.{materialName} discard => IndexBuffer not set");
                    continue;
                }

                var technique = effect.GetTechnique(mesh.VertextType, true, material.Material.IsTransparent);
                if (!BufferManager.SetInputAssembler(technique, mesh.VertexBuffer, mesh.Topology))
                {
                    Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}.{materialName} discard => InputAssembler not set");
                    continue;
                }

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawShadowMesh)}: {meshName}.{materialName} Index {index} Length {length}");
                    mesh.Draw(graphics, index, length);
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

            if (!InstancingBuffer.Ready)
            {
                return;
            }

            if (hasDataToWrite)
            {
                Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(Draw)} WriteInstancingData: BufferDescriptionIndex {InstancingBuffer.BufferDescriptionIndex} BufferOffset {InstancingBuffer.BufferOffset} {context.DrawerMode}");
                BufferManager.WriteInstancingData(InstancingBuffer, instancingData);
            }

            var effect = GetEffect(context.DrawerMode);
            if (effect == null)
            {
                return;
            }

            Draw(context, effect);
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="effect">Geometry drawer</param>
        private void Draw(DrawContext context, IGeometryDrawer effect)
        {
            int count = 0;
            int instanceCount = 0;

            effect.UpdatePerFrameFull(Matrix.Identity, context);

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

                var index = Array.IndexOf(instancesTmp, lodInstances.First()) + InstancingBuffer.BufferOffset;
                var length = Math.Min(maxCount, lodInstances.Count());
                if (length <= 0)
                {
                    continue;
                }

                maxCount -= length;
                instanceCount += length;

                foreach (string meshName in drawingData.Meshes.Keys)
                {
                    UpdateIndependantTransforms(meshName);

                    count += DrawMesh(context, effect, drawingData, meshName, index, length);
                    count *= instanceCount;
                }
            }

            Counters.InstancesPerFrame += instanceCount;
            Counters.PrimitivesPerFrame += count;
        }
        /// <summary>
        /// Draws a mesh with a geometry drawer
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="effect">Effect</param>
        /// <param name="drawingData">Drawing data</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="index">Instance buffer index</param>
        /// <param name="length">Instance buffer length</param>
        /// <returns>Returns the number of drawn triangles</returns>
        private int DrawMesh(DrawContext context, IGeometryDrawer effect, DrawingData drawingData, string meshName, int index, int length)
        {
            Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}. Index {index} Length {length}. {context.DrawerMode}");

            int count = 0;

            var graphics = Game.Graphics;

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

                count += mesh.Count;

                var materialInfo = new MaterialDrawInfo
                {
                    Material = material,
                    UseAnisotropic = UseAnisotropicFiltering,
                };

                effect.UpdatePerObject(materialInfo, Color4.White, 0, AnimationDrawInfo.Empty);

                if (!BufferManager.SetIndexBuffer(mesh.IndexBuffer))
                {
                    Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName} discard => IndexBuffer not set");
                    continue;
                }

                var technique = effect.GetTechnique(mesh.VertextType, true);
                if (!BufferManager.SetInputAssembler(technique, mesh.VertexBuffer, mesh.Topology))
                {
                    Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName} discard => InputAssembler not set");
                    continue;
                }

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    Logger.WriteTrace(this, $"{nameof(ModelInstanced)}.{Name} - {nameof(DrawMesh)}: {meshName}.{materialName} Index {index} Length {length}");
                    mesh.Draw(graphics, index, length);
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
        public override bool Cull(IIntersectionVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (instancesTmp?.Length > 0)
            {
                var item = instancesTmp.FirstOrDefault(i =>
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
            if (!(state is ModelInstancedState modelInstancedState))
            {
                return;
            }

            Name = modelInstancedState.Name;
            Active = modelInstancedState.Active;
            Visible = modelInstancedState.Visible;
            Usage = modelInstancedState.Usage;
            Layer = modelInstancedState.Layer;

            if (!string.IsNullOrEmpty(modelInstancedState.OwnerId))
            {
                Owner = Scene.GetComponents().FirstOrDefault(c => c.Id == modelInstancedState.OwnerId);
            }

            MaximumCount = modelInstancedState.MaximumCount;
            for (int i = 0; i < modelInstancedState.Instances.Count(); i++)
            {
                var instanceState = modelInstancedState.Instances.ElementAt(i);
                instances[i].SetState(instanceState);
            }
        }
    }
}
