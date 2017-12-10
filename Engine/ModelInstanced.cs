using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Instaced model
    /// </summary>
    public class ModelInstanced : ModelBase, IComposed
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
        /// Instances
        /// </summary>
        private int instanceCount = 0;

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
                return Array.FindAll(this.instances, i => i.Visible == true).Length;
            }
        }
        /// <summary>
        /// Maximum number of instances
        /// </summary>
        public int Count
        {
            get
            {
                return this.instanceCount;
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
            if (description.Instances <= 0) throw new ArgumentException(string.Format("Instances parameter must be more than 0: {0}", instances));

            this.instanceCount = description.Instances;

            this.instances = Helper.CreateArray(this.instanceCount, () => new ModelInstance(this));
            this.instancingData = new VertexInstancingData[this.instanceCount];

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
                    if (i.Active) i.Update(context);
                });

                this.instancesTmp = Array.FindAll(this.instances, i => i.LevelOfDetail != LevelOfDetailEnum.None);

                this.SortInstances(context.EyePosition);
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
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.VisibleCount > 0)
            {
                var mode = context.DrawerMode;

                int count = 0;
                int instanceCount = 0;

                Drawer effect = null;
                GetTechniqueDelegate techniqueFn = null;
                if (mode.HasFlag(DrawerModesEnum.Forward))
                {
                    effect = DrawerPool.EffectDefaultBasic;
                    techniqueFn = DrawerPool.EffectDefaultBasic.GetTechnique;
                }
                else if (mode.HasFlag(DrawerModesEnum.Deferred))
                {
                    effect = DrawerPool.EffectDeferredBasic;
                    techniqueFn = DrawerPool.EffectDeferredBasic.GetTechnique;
                }
                else if (mode.HasFlag(DrawerModesEnum.ShadowMap))
                {
                    effect = DrawerPool.EffectShadowBasic;
                    techniqueFn = DrawerPool.EffectShadowBasic.GetTechnique;
                }
                if (effect != null)
                {
                    var graphics = this.Game.Graphics;

                    #region Update instancing data

                    //Process only visible instances
                    if (this.instancesTmp != null && this.instancesTmp.Length > 0)
                    {
                        LevelOfDetailEnum lastLod = LevelOfDetailEnum.None;
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

                        //Writes instancing data
                        if (instanceIndex > 0)
                        {
                            this.BufferManager.WriteInstancingData(this.instancingData);
                        }
                    }

                    #endregion

                    #region Per frame update

                    if (mode.HasFlag(DrawerModesEnum.Forward))
                    {
                        ((EffectDefaultBasic)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection,
                            context.EyePosition,
                            context.Lights,
                            context.ShadowMaps,
                            context.ShadowMapLow,
                            context.ShadowMapHigh,
                            context.FromLightViewProjectionLow,
                            context.FromLightViewProjectionHigh);
                    }
                    else if (mode.HasFlag(DrawerModesEnum.Deferred))
                    {
                        ((EffectDeferredBasic)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }
                    else if (mode.HasFlag(DrawerModesEnum.ShadowMap))
                    {
                        ((EffectShadowBasic)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }

                    #endregion

                    int maxCount = this.MaximumCount >= 0 ?
                        Math.Min(this.MaximumCount, this.Count) :
                        this.Count;

                    //Render by level of detail
                    for (int l = 1; l < (int)LevelOfDetailEnum.Minimum + 1; l *= 2)
                    {
                        if (maxCount > 0)
                        {
                            LevelOfDetailEnum lod = (LevelOfDetailEnum)l;

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
                                                if (mode.HasFlag(DrawerModesEnum.OpaqueOnly) && transparent)
                                                {
                                                    continue;
                                                }
                                                if (mode.HasFlag(DrawerModesEnum.TransparentOnly) && !transparent)
                                                {
                                                    continue;
                                                }

                                                if (!mode.HasFlag(DrawerModesEnum.ShadowMap))
                                                {
                                                    count += mesh.IndexBuffer.Count > 0 ? mesh.IndexBuffer.Count / 3 : mesh.VertexBuffer.Count / 3;
                                                    count *= instanceCount;
                                                }

                                                #region Per object update

                                                var mat = drawingData.Materials[material];

                                                if (mode.HasFlag(DrawerModesEnum.Forward))
                                                {
                                                    ((EffectDefaultBasic)effect).UpdatePerObject(
                                                        this.UseAnisotropicFiltering,
                                                        mat.DiffuseTexture,
                                                        mat.NormalMap,
                                                        mat.SpecularTexture,
                                                        mat.ResourceIndex,
                                                        0,
                                                        0);
                                                }
                                                else if (mode.HasFlag(DrawerModesEnum.Deferred))
                                                {
                                                    ((EffectDeferredBasic)effect).UpdatePerObject(
                                                        this.UseAnisotropicFiltering,
                                                        mat.DiffuseTexture,
                                                        mat.NormalMap,
                                                        mat.SpecularTexture,
                                                        mat.ResourceIndex,
                                                        0,
                                                        0);
                                                }
                                                else if (mode.HasFlag(DrawerModesEnum.ShadowMap))
                                                {
                                                    ((EffectShadowBasic)effect).UpdatePerObject(
                                                        mat.DiffuseTexture,
                                                        0,
                                                        0);
                                                }

                                                #endregion

                                                this.BufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);

                                                var technique = techniqueFn(mesh.VertextType, mesh.Instanced);
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

                if (!mode.HasFlag(DrawerModesEnum.ShadowMap) && count > 0)
                {
                    Counters.InstancesPerFrame += instanceCount;
                    Counters.PrimitivesPerFrame += count;
                }
            }
        }
        /// <summary>
        /// Set instance positions
        /// </summary>
        /// <param name="positions">New positions</param>
        public void SetPositions(Vector3[] positions)
        {
            if (positions != null && positions.Length > 0)
            {
                if (this.instances != null && this.instances.Length > 0)
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

            for (int i = 0; i < this.instanceCount; i++)
            {
                if (this.instances[i] is T)
                {
                    res.Add((T)(object)this.instances[i]);
                }
            }

            return res;
        }
    }
}
