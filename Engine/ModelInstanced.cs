using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

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
                return Array.FindAll(this.instances, i => i.Visible).Length;
            }
        }
        /// <summary>
        /// Maximum number of instances
        /// </summary>
        public int Count { get; } = 0;
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
            if (description.Instances <= 0) throw new ArgumentException(string.Format("Instances parameter must be more than 0: {0}", instances));

            this.Count = description.Instances;

            this.instances = Helper.CreateArray(this.Count, () => new ModelInstance(this));
            this.instancingData = new VertexInstancingData[this.Count];

            this.MaximumCount = -1;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (this.instances != null && this.instances.Length > 0)
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

            #region Update instancing data

            //Process only visible instances
            if (this.instancesTmp != null && this.instancesTmp.Length > 0)
            {
                this.SortInstances(context.EyePosition);

                LevelOfDetail lastLod = LevelOfDetail.None;
                DrawingData drawingData = null;
                int instanceIndex = 0;

                for (int i = 0; i < this.instancesTmp.Length; i++)
                {
                    var current = this.instancesTmp[i];
                    if (current != null)
                    {
                        if (lastLod != current.LevelOfDetail)
                        {
                            lastLod = current.LevelOfDetail;
                            drawingData = this.GetDrawingData(lastLod);
                        }

                        this.instancingData[instanceIndex].Local = current.Manipulator.LocalTransform;
                        this.instancingData[instanceIndex].TextureIndex = current.TextureIndex;

                        if (drawingData != null && drawingData.SkinningData != null)
                        {
                            current.AnimationController.Update(context.GameTime.ElapsedSeconds, drawingData.SkinningData);

                            this.instancingData[instanceIndex].AnimationOffset = current.AnimationController.GetAnimationOffset(drawingData.SkinningData);

                            current.InvalidateCache();
                        }

                        instanceIndex++;
                    }
                }

                //Mark to write instancing data
                hasDataToWrite = instanceIndex > 0;
            }

            #endregion
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
        /// Shadow Drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void DrawShadows(DrawContextShadows context)
        {
            if (this.hasDataToWrite)
            {
                this.BufferManager.WriteInstancingData(this.instancingData);
            }

            if (this.VisibleCount > 0)
            {
                var effect = context.ShadowMap.GetEffect();
                if (effect != null)
                {
                    var graphics = this.Game.Graphics;

                    effect.UpdatePerFrame(Matrix.Identity, context);

                    int maxCount = this.MaximumCount >= 0 ?
                    Math.Min(this.MaximumCount, this.Count) :
                    this.Count;

                    if (maxCount > 0)
                    {
                        //Get instances
                        var tmp = this.instancesTmp;
                        if (tmp?.Length > 0)
                        {
                            //Render minimum LOD available
                            var drawingData = GetFirstDrawingData(LevelOfDetail.Minimum);
                            {
                                var index = Array.IndexOf(this.instancesTmp, tmp[0]);
                                var length = Math.Min(maxCount, tmp.Length);

                                if (length > 0)
                                {
                                    foreach (string meshName in drawingData.Meshes.Keys)
                                    {
                                        var dictionary = drawingData.Meshes[meshName];

                                        foreach (string material in dictionary.Keys)
                                        {
                                            var mesh = dictionary[material];

                                            var mat = drawingData.Materials[material];

                                            effect.UpdatePerObject(0, mat, 0);

                                            this.BufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);

                                            var technique = effect.GetTechnique(mesh.VertextType, mesh.Instanced, mesh.Transparent);
                                            this.BufferManager.SetInputAssembler(technique, mesh.VertexBuffer.Slot, mesh.Topology);

                                            for (int p = 0; p < technique.PassCount; p++)
                                            {
                                                graphics.EffectPassApply(technique, p, 0);

                                                mesh.Draw(graphics, index, length);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.hasDataToWrite)
            {
                this.BufferManager.WriteInstancingData(this.instancingData);
            }

            if (this.VisibleCount > 0)
            {
                var mode = context.DrawerMode;

                int count = 0;
                int instanceCount = 0;

                IGeometryDrawer effect = null;

                if (mode.HasFlag(DrawerModes.Forward))
                {
                    effect = DrawerPool.EffectDefaultBasic;
                }
                else if (mode.HasFlag(DrawerModes.Deferred))
                {
                    effect = DrawerPool.EffectDeferredBasic;
                }

                if (effect != null)
                {
                    var graphics = this.Game.Graphics;

                    effect.UpdatePerFrameFull(Matrix.Identity, context);

                    int maxCount = this.MaximumCount >= 0 ?
                        Math.Min(this.MaximumCount, this.Count) :
                        this.Count;

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
                        if (lodInstances != null && lodInstances.Length > 0)
                        {
                            var drawingData = this.GetDrawingData(lod);
                            if (drawingData != null)
                            {
                                var index = Array.IndexOf(this.instancesTmp, lodInstances[0]);
                                var length = Math.Min(maxCount, lodInstances.Length);
                                maxCount -= length;

                                if (length > 0)
                                {
                                    instanceCount += length;

                                    foreach (string meshName in drawingData.Meshes.Keys)
                                    {
                                        var dictionary = drawingData.Meshes[meshName];

                                        foreach (string material in dictionary.Keys)
                                        {
                                            var mesh = dictionary[material];

                                            bool transparent = mesh.Transparent && this.Description.AlphaEnabled;
                                            if (mode.HasFlag(DrawerModes.OpaqueOnly) && transparent)
                                            {
                                                continue;
                                            }
                                            if (mode.HasFlag(DrawerModes.TransparentOnly) && !transparent)
                                            {
                                                continue;
                                            }

                                            count += mesh.IndexBuffer.Count > 0 ? mesh.IndexBuffer.Count / 3 : mesh.VertexBuffer.Count / 3;
                                            count *= instanceCount;

                                            effect.UpdatePerObject(
                                                0,
                                                drawingData.Materials[material],
                                                0,
                                                this.UseAnisotropicFiltering);

                                            this.BufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);

                                            var technique = effect.GetTechnique(mesh.VertextType, mesh.Instanced);
                                            this.BufferManager.SetInputAssembler(technique, mesh.VertexBuffer.Slot, mesh.Topology);

                                            for (int p = 0; p < technique.PassCount; p++)
                                            {
                                                graphics.EffectPassApply(technique, p, 0);

                                                mesh.Draw(graphics, index, length);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                Counters.InstancesPerFrame += instanceCount;
                Counters.PrimitivesPerFrame += count;
            }
        }

        /// <summary>
        /// Set instance positions
        /// </summary>
        /// <param name="positions">New positions</param>
        public void SetPositions(Vector3[] positions)
        {
            if (positions?.Length > 0 && this.instances?.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    if (i < positions.Length)
                    {
                        this.instances[i].Manipulator.SetPosition(positions[i], true);
                        this.instances[i].Active = true;
                        this.instances[i].Visible = true;
                    }
                    else
                    {
                        this.instances[i].Active = false;
                        this.instances[i].Visible = false;
                    }
                }
            }
        }
        /// <summary>
        /// Sets instance transforms
        /// </summary>
        /// <param name="transforms">Transform matrix list</param>
        public void SetTransforms(Matrix[] transforms)
        {
            if (transforms?.Length > 0 && this.instances?.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    if (i < transforms.Length)
                    {
                        this.instances[i].Manipulator.SetTransform(transforms[i]);
                        this.instances[i].Active = true;
                        this.instances[i].Visible = true;
                    }
                    else
                    {
                        this.instances[i].Active = false;
                        this.instances[i].Visible = false;
                    }
                }
            }
        }
        /// <summary>
        /// Gets the instance list
        /// </summary>
        /// <returns>Returns an array with the instance list</returns>
        public ModelInstance[] GetInstances()
        {
            return this.instances;
        }
        /// <summary>
        /// Gets all components
        /// </summary>
        /// <returns>Returns a collection of components</returns>
        public IEnumerable<T> GetComponents<T>()
        {
            List<T> res = new List<T>();

            for (int i = 0; i < this.Count; i++)
            {
                if (this.instances[i].Visible && this.instances[i] is T)
                {
                    res.Add((T)(object)this.instances[i]);
                }
            }

            return res;
        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If any instance is inside the volume, returns zero</param>
        /// <returns>Returns true if all of the instances were outside of the frustum</returns>
        public override bool Cull(ICullingVolume volume, out float distance)
        {
            if (this.instancesTmp != null && this.instancesTmp.Length > 0)
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

            distance = float.MaxValue;
            return true;
        }
    }
}
