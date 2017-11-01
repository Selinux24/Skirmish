using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

                this.instancesTmp = Array.FindAll(this.instances, i => i.Visible && i.LevelOfDetail != LevelOfDetailEnum.None);

                this.UpdateInstances(context);
            }
        }
        /// <summary>
        /// Updates the instances order
        /// </summary>
        /// <param name="context">Context</param>
        private async void UpdateInstances(UpdateContext context)
        {
            await Task.Run(() =>
            {
                //Sort by LOD
                Array.Sort(this.instancesTmp, (i1, i2) =>
                {
                    var i = i1.LevelOfDetail.CompareTo(i2.LevelOfDetail);

                    if (i == 0)
                    {
                        var da = Vector3.DistanceSquared(i1.Manipulator.Position, context.EyePosition);
                        var db = Vector3.DistanceSquared(i2.Manipulator.Position, context.EyePosition);
                        i = da.CompareTo(db);
                    }

                    if (i == 0)
                    {
                        i = i1.Id.CompareTo(i2.Id);
                    }

                    if (this.Description.AlphaEnabled)
                    {
                        return -i;
                    }
                    else
                    {
                        return i;
                    }
                });
            });
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            var graphics = this.Game.Graphics;

            int count = 0;
            int instanceCount = 0;

            if (this.VisibleCount > 0)
            {
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

                Drawer effect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectDefaultBasic;
                else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectDeferredBasic;
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectShadowBasic;

                if (effect != null)
                {
                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
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
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        ((EffectDeferredBasic)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        ((EffectShadowBasic)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }

                    #endregion

                    //Render by level of detail
                    for (int l = 1; l < (int)LevelOfDetailEnum.Minimum + 1; l *= 2)
                    {
                        LevelOfDetailEnum lod = (LevelOfDetailEnum)l;

                        //Get instances in this LOD
                        var ins = Array.FindAll(this.instancesTmp, i => i != null && i.LevelOfDetail == lod);
                        if (ins != null && ins.Length > 0)
                        {
                            var drawingData = this.GetDrawingData(lod);
                            if (drawingData != null)
                            {
                                var index = Array.IndexOf(this.instancesTmp, ins[0]);
                                var length = ins.Length;

                                instanceCount += length;

                                foreach (string meshName in drawingData.Meshes.Keys)
                                {
                                    var dictionary = drawingData.Meshes[meshName];

                                    foreach (string material in dictionary.Keys)
                                    {
                                        #region Per object update

                                        var mat = drawingData.Materials[material];

                                        if (context.DrawerMode == DrawerModesEnum.Forward)
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
                                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
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
                                        else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                                        {
                                            ((EffectShadowBasic)effect).UpdatePerObject(
                                                mat.DiffuseTexture,
                                                0,
                                                0);
                                        }

                                        #endregion

                                        var mesh = dictionary[material];
                                        this.BufferManager.SetIndexBuffer(mesh.IndexBuffer.Slot);

                                        var technique = effect.GetTechnique(mesh.VertextType, mesh.Instanced, DrawingStages.Drawing, context.DrawerMode);
                                        this.BufferManager.SetInputAssembler(technique, mesh.VertexBuffer.Slot, mesh.Topology);

                                        count += mesh.IndexBuffer.Count > 0 ? mesh.IndexBuffer.Count / 3 : mesh.VertexBuffer.Count / 3;
                                        count *= instanceCount;

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

            if (context.DrawerMode != DrawerModesEnum.ShadowMap)
            {
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
