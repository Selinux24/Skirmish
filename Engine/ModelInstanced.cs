using SharpDX;
using System;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Instaced model
    /// </summary>
    public class ModelInstanced : ModelBase
    {
        /// <summary>
        /// Instancing data per instance
        /// </summary>
        private VertexInstancingData[] instancingData = null;
        /// <summary>
        /// Manipulator list per instance
        /// </summary>
        private ModelInstance[] instances = null;

        /// <summary>
        /// Enables z-buffer writting
        /// </summary>
        public bool EnableDepthStencil { get; set; }
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool EnableAlphaBlending { get; set; }
        /// <summary>
        /// Gets manipulator per instance list
        /// </summary>
        /// <returns>Gets manipulator per instance list</returns>
        public ModelInstance[] Instances
        {
            get
            {
                return this.instances;
            }
        }
        /// <summary>
        /// Gets instance count
        /// </summary>
        public int Count
        {
            get
            {
                return this.instances.Length;
            }
        }
        /// <summary>
        /// Gets visible instance count
        /// </summary>
        public int VisibleCount
        {
            get
            {
                return Array.FindAll(this.instances, i => i.Visible == true && i.Cull == false).Length;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        /// <param name="instances">Number of instances</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public ModelInstanced(Game game, ModelContent content, int instances, bool dynamic = false)
            : base(game, content, true, instances, true, true, dynamic)
        {
            this.instancingData = new VertexInstancingData[instances];
            this.instances = Helper.CreateArray(instances, () => new ModelInstance(this));

            this.EnableDepthStencil = true;
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (this.instances != null && this.instances.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    if (this.instances[i].Active)
                    {
                        this.instances[i].Manipulator.Update(context.GameTime);
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
            if (this.Meshes != null && this.VisibleCount > 0)
            {
                Drawer effect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectInstancing;
                else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectInstancingGBuffer;
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectInstancingShadow;

                if (effect != null)
                {
                    if (this.instances != null && this.instances.Length > 0)
                    {
                        int instanceIndex = 0;
                        for (int i = 0; i < this.instances.Length; i++)
                        {
                            if (this.instances[i].Visible && !this.instances[i].Cull)
                            {
                                this.instancingData[instanceIndex].Local = this.instances[i].Manipulator.LocalTransform;
                                this.instancingData[instanceIndex].TextureIndex = this.instances[i].TextureIndex;

                                instanceIndex++;
                            }
                        }
                    }

                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        ((EffectInstancing)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection,
                            context.EyePosition,
                            context.Frustum,
                            context.Lights,
                            context.ShadowMapStatic,
                            context.ShadowMapDynamic,
                            context.FromLightViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        ((EffectInstancingGBuffer)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        ((EffectInstancingShadow)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }

                    #endregion

                    if (this.EnableDepthStencil)
                    {
                        this.Game.Graphics.SetDepthStencilZEnabled();
                    }
                    else
                    {
                        this.Game.Graphics.SetDepthStencilZDisabled();
                    }

                    if (this.EnableAlphaBlending)
                    {
                        this.Game.Graphics.SetBlendAlphaEnabled();
                    }

                    foreach (string meshName in this.Meshes.Keys)
                    {
                        #region Per skinning update

                        if (this.SkinningData != null)
                        {
                            if (context.DrawerMode == DrawerModesEnum.Forward)
                            {
                                ((EffectInstancing)effect).UpdatePerSkinning(this.SkinningData.GetFinalTransforms(meshName));
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectInstancingGBuffer)effect).UpdatePerSkinning(this.SkinningData.GetFinalTransforms(meshName));
                            }
                            else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                            {
                                ((EffectInstancingShadow)effect).UpdatePerSkinning(this.SkinningData.GetFinalTransforms(meshName));
                            }
                        }
                        else
                        {
                            if (context.DrawerMode == DrawerModesEnum.Forward)
                            {
                                ((EffectInstancing)effect).UpdatePerSkinning(null);
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectInstancingGBuffer)effect).UpdatePerSkinning(null);
                            }
                            else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                            {
                                ((EffectInstancingShadow)effect).UpdatePerSkinning(null);
                            }
                        }

                        #endregion

                        MeshMaterialsDictionary dictionary = this.Meshes[meshName];

                        foreach (string material in dictionary.Keys)
                        {
                            MeshInstanced mesh = (MeshInstanced)dictionary[material];
                            MeshMaterial mat = this.Materials[material];

                            #region Per object update

                            var matdata = mat != null ? mat.Material : Material.Default;
                            var texture = mat != null ? mat.DiffuseTexture : null;
                            var normalMap = mat != null ? mat.NormalMap : null;

                            if (context.DrawerMode == DrawerModesEnum.Forward)
                            {
                                ((EffectInstancing)effect).UpdatePerObject(matdata, texture, normalMap);
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectInstancingGBuffer)effect).UpdatePerObject(matdata, texture, normalMap);
                            }

                            #endregion

                            EffectTechnique technique = effect.GetTechnique(mesh.VertextType, DrawingStages.Drawing, context.DrawerMode);

                            mesh.SetInputAssembler(this.DeviceContext, effect.GetInputLayout(technique));

                            mesh.WriteInstancingData(this.DeviceContext, this.instancingData);

                            for (int p = 0; p < technique.Description.PassCount; p++)
                            {
                                technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                                mesh.Draw(this.DeviceContext, this.VisibleCount);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Frustum culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public override void FrustumCulling(BoundingFrustum frustum)
        {
            //Cull was made per instance
            this.Cull = true;

            for (int i = 0; i < this.Instances.Length; i++)
            {
                if (this.Instances[i].Visible)
                {
                    this.Instances[i].FrustumCulling(frustum);

                    if (!this.Instances[i].Cull)
                    {
                        this.Cull = false;
                    }
                }
            }
        }
        /// <summary>
        /// Sets cull value
        /// </summary>
        /// <param name="value">New value</param>
        public override void SetCulling(bool value)
        {
            base.SetCulling(value);

            if (this.instances != null && this.instances.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    this.instances[i].SetCulling(value);
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
                if (this.Instances != null && this.Instances.Length > 0)
                {
                    for (int i = 0; i < this.Instances.Length; i++)
                    {
                        if (i < positions.Length)
                        {
                            this.Instances[i].Manipulator.SetPosition(positions[i], true);
                            this.Instances[i].Active = true;
                            this.Instances[i].Visible = true;
                        }
                        else
                        {
                            this.Instances[i].Active = false;
                            this.Instances[i].Visible = false;
                        }
                    }
                }
            }
        }
    }
}
