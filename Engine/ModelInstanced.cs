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
    public class ModelInstanced : BaseModel, IComposed
    {
        /// <summary>
        /// Instancing data per instance
        /// </summary>
        private readonly VertexInstancingData[] instancingData = null;
        /// <summary>
        /// Model instance list
        /// </summary>
        private readonly ModelInstance[] instances = null;
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
        private readonly bool hasIndependantTransforms = false;

        /// <summary>
        /// Gets manipulator per instance list
        /// </summary>
        /// <returns>Gets manipulator per instance list</returns>
        public ModelInstance this[int index]
        {
            get
            {
                return this.instances[index];
            }
        }
        /// <summary>
        /// Gets visible instance count
        /// </summary>
        public int VisibleCount
        {
            get
            {
                return this.Visible ? Array.FindAll(this.instances, i => i.Visible).Length : 0;
            }
        }
        /// <summary>
        /// Gets or sets the maximum number of instances to draw
        /// </summary>
        public int MaximumCount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public ModelInstanced(Scene scene, ModelInstancedDescription description)
            : base(scene, description)
        {
            if (description.Instances <= 0)
            {
                throw new ArgumentException($"Instances parameter must be more than 0: {description.Instances}");
            }

            this.InstanceCount = description.Instances;

            this.instances = Helper.CreateArray(this.InstanceCount, () => new ModelInstance(this, description));
            this.instancingData = new VertexInstancingData[this.InstanceCount];

            this.MaximumCount = -1;

            this.hasIndependantTransforms = (description.TransformDependences?.Any() == true);
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (this.instances.Any())
            {
                Array.ForEach(this.instances, i =>
                {
                    if (i.Active)
                    {
                        i.Update(context);
                        i.SetLOD(context.EyePosition);
                    }
                });

                this.instancesTmp = Array.FindAll(this.instances, i => i.Visible && i.LevelOfDetail != LevelOfDetail.None);
            }

            //Process only visible instances
            this.UpdateInstancingData(context);
        }
        /// <summary>
        /// Updates instancing data buffer
        /// </summary>
        /// <param name="context">Update context</param>
        private void UpdateInstancingData(UpdateContext context)
        {
            if (!this.instancesTmp.Any())
            {
                return;
            }

            this.SortInstances(context.EyePosition);

            LevelOfDetail lastLod = LevelOfDetail.None;
            DrawingData drawingData = null;
            int instanceIndex = 0;

            for (int i = 0; i < this.instancesTmp.Length; i++)
            {
                var current = this.instancesTmp[i];
                if (current == null)
                {
                    continue;
                }

                if (lastLod != current.LevelOfDetail)
                {
                    lastLod = current.LevelOfDetail;
                    drawingData = this.GetDrawingData(lastLod);
                }

                uint animationOffset = 0;

                if (drawingData?.SkinningData != null)
                {
                    if (current.AnimationController.Playing)
                    {
                        current.InvalidateCache();
                    }

                    current.AnimationController.Update(context.GameTime.ElapsedSeconds, drawingData.SkinningData);
                    animationOffset = current.AnimationController.GetAnimationOffset(drawingData.SkinningData);
                }

                this.instancingData[instanceIndex].Local = current.Manipulator.LocalTransform;
                this.instancingData[instanceIndex].TextureIndex = current.TextureIndex;
                this.instancingData[instanceIndex].AnimationOffset = animationOffset;

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

            for (int i = 0; i < this.instancesTmp.Length; i++)
            {
                var current = this.instancesTmp[i];
                if (current == null)
                {
                    continue;
                }

                var currentTransform = this.instancingData[instanceIndex].Local;
                var localTransform = current.GetTransformByName(meshName);
                if (currentTransform != localTransform)
                {
                    this.instancingData[instanceIndex].Local = localTransform;

                    hasDataToWrite = true;
                }

                instanceIndex++;
            }

            if (hasDataToWrite)
            {
                this.BufferManager.WriteInstancingData(this.InstancingBuffer, this.instancingData);
            }
        }
        /// <summary>
        /// Updates the instances order
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private void SortInstances(Vector3 eyePosition)
        {
            //Sort by LOD, distance and id
            Array.Sort(this.instancesTmp, (i1, i2) =>
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
            return this.MaximumCount >= 0 ?
                Math.Min(this.MaximumCount, this.InstanceCount) :
                this.InstanceCount;
        }
        /// <inheritdoc/>
        public override void DrawShadows(DrawContextShadows context)
        {
            if (!this.Visible)
            {
                return;
            }

            if (!this.InstancingBuffer.Ready)
            {
                return;
            }

            if (this.hasDataToWrite)
            {
                Logger.WriteDebug($"{this.Name} - DrawShadows WriteInstancingData: BufferDescriptionIndex {InstancingBuffer.BufferDescriptionIndex} BufferOffset {InstancingBuffer.BufferOffset}");
                this.BufferManager.WriteInstancingData(this.InstancingBuffer, this.instancingData);
            }

            var effect = context.ShadowMap.GetEffect();
            if (effect == null)
            {
                return;
            }

            this.DrawShadows(context, effect);
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

            int maxCount = this.GetMaxCount();

            //Render by level of detail
            for (int l = 1; l < (int)LevelOfDetail.Minimum + 1; l *= 2)
            {
                if (maxCount <= 0)
                {
                    break;
                }

                LevelOfDetail lod = (LevelOfDetail)l;

                //Get instances in this LOD
                var lodInstances = Array.FindAll(this.instancesTmp, i => i != null && i.LevelOfDetail == lod);
                if (lodInstances.Length <= 0)
                {
                    continue;
                }

                var drawingData = this.GetDrawingData(lod);
                if (drawingData == null)
                {
                    continue;
                }

                var index = Array.IndexOf(this.instancesTmp, lodInstances[0]) + this.InstancingBuffer.BufferOffset;
                var length = Math.Min(maxCount, lodInstances.Length);
                if (length <= 0)
                {
                    continue;
                }

                maxCount -= length;
                instanceCount += length;

                foreach (string meshName in drawingData.Meshes.Keys)
                {
                    this.UpdateIndependantTransforms(meshName);

                    count += this.DrawShadowMesh(effect, drawingData, meshName, index, length);
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
            int count = 0;

            var graphics = this.Game.Graphics;

            var meshDict = drawingData.Meshes[meshName];

            foreach (string materialName in meshDict.Keys)
            {
                var mesh = meshDict[materialName];
                if (!mesh.Ready)
                {
                    continue;
                }

                var material = drawingData.Materials[materialName];

                count += mesh.Count;

                effect.UpdatePerObject(0, material, 0);

                this.BufferManager.SetIndexBuffer(mesh.IndexBuffer);

                var technique = effect.GetTechnique(mesh.VertextType, true, material.Material.IsTransparent);
                this.BufferManager.SetInputAssembler(technique, mesh.VertexBuffer, mesh.Topology);

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    mesh.Draw(graphics, index, length);
                }
            }

            return count;
        }
        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!this.Visible)
            {
                return;
            }

            if (!this.InstancingBuffer.Ready)
            {
                return;
            }

            if (this.hasDataToWrite)
            {
                Logger.WriteDebug($"{this.Name} - Draw WriteInstancingData: BufferDescriptionIndex {InstancingBuffer.BufferDescriptionIndex} BufferOffset {InstancingBuffer.BufferOffset} {context.DrawerMode}");
                this.BufferManager.WriteInstancingData(this.InstancingBuffer, this.instancingData);
            }

            var effect = this.GetEffect(context.DrawerMode);
            if (effect == null)
            {
                return;
            }

            this.Draw(context, effect);
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

            int maxCount = this.GetMaxCount();

            //Render by level of detail
            for (int l = 1; l < (int)LevelOfDetail.Minimum + 1; l *= 2)
            {
                if (maxCount <= 0)
                {
                    break;
                }

                LevelOfDetail lod = (LevelOfDetail)l;

                //Get instances in this LOD
                var lodInstances = Array.FindAll(this.instancesTmp, i => i != null && i.LevelOfDetail == lod);
                if (lodInstances.Length <= 0)
                {
                    continue;
                }

                var drawingData = this.GetDrawingData(lod);
                if (drawingData == null)
                {
                    continue;
                }

                var index = Array.IndexOf(this.instancesTmp, lodInstances[0]) + this.InstancingBuffer.BufferOffset;
                var length = Math.Min(maxCount, lodInstances.Length);
                if (length <= 0)
                {
                    continue;
                }

                maxCount -= length;
                instanceCount += length;

                foreach (string meshName in drawingData.Meshes.Keys)
                {
                    this.UpdateIndependantTransforms(meshName);

                    count += this.DrawMesh(context, effect, drawingData, meshName, index, length);
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
            int count = 0;

            var graphics = this.Game.Graphics;

            var meshDict = drawingData.Meshes[meshName];

            foreach (string materialName in meshDict.Keys)
            {
                var mesh = meshDict[materialName];
                if (!mesh.Ready)
                {
                    continue;
                }

                var material = drawingData.Materials[materialName];

                bool draw = context.ValidateDraw(this.BlendMode, material.Material.IsTransparent);
                if (!draw)
                {
                    continue;
                }

                count += mesh.Count;

                effect.UpdatePerObject(0, material, 0, this.UseAnisotropicFiltering);

                this.BufferManager.SetIndexBuffer(mesh.IndexBuffer);

                var technique = effect.GetTechnique(mesh.VertextType, true);
                this.BufferManager.SetInputAssembler(technique, mesh.VertexBuffer, mesh.Topology);

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

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
            if (positions?.Any() == true && this.instances?.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    var instance = this.instances[i];

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
            if (transforms?.Any() == true && this.instances?.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    var instance = this.instances[i];

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
            return new ReadOnlyCollection<ModelInstance>(this.instances);
        }
        /// <summary>
        /// Gets all components
        /// </summary>
        /// <returns>Returns a collection of components</returns>
        public IEnumerable<T> GetComponents<T>()
        {
            return new ReadOnlyCollection<T>(this.instances.Where(i => i.Visible).OfType<T>().ToArray());
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If any instance is inside the volume, returns zero</param>
        /// <returns>Returns true if all of the instances were outside of the frustum</returns>
        public override bool Cull(IIntersectionVolume volume, out float distance)
        {
            distance = float.MaxValue;

            if (this.instancesTmp?.Length > 0)
            {
                var item = this.instancesTmp.FirstOrDefault(i =>
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
    }

    /// <summary>
    /// Instanced model extensions
    /// </summary>
    public static class ModelInstancedExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<ModelInstanced> AddComponentModelInstanced(this Scene scene, ModelInstancedDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            ModelInstanced component = null;

            await Task.Run(() =>
            {
                component = new ModelInstanced(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
