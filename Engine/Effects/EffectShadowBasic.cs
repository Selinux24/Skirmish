using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectShadowBasic : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique ShadowMapPositionNormalTextureTangentSkinned = null;
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingShadowMapPositionNormalTextureTangentSkinned = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private EffectVectorVariable animationData = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private EffectScalarVariable textureIndex = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private EffectScalarVariable animationPaletteWidth = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private EffectShaderResourceVariable animationPalette = null;

        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjection.GetMatrix();
            }
            set
            {
                this.worldViewProjection.SetMatrix(value);
            }
        }
        /// <summary>
        /// Animation data
        /// </summary>
        protected UInt32[] AnimationData
        {
            get
            {
                Int4 v = this.animationData.GetIntVector();

                return new UInt32[] { (uint)v.X, (uint)v.Y, (uint)v.Z };
            }
            set
            {
                Int4 v4 = new Int4((int)value[0], (int)value[1], (int)value[2], 0);

                this.animationData.Set(v4);
            }
        }
        /// <summary>
        /// Texture index
        /// </summary>
        protected uint TextureIndex
        {
            get
            {
                return (uint)this.textureIndex.GetFloat();
            }
            set
            {
                this.textureIndex.Set((float)value);
            }
        }
        /// <summary>
        /// Animation palette width
        /// </summary>
        protected uint AnimationPaletteWidth
        {
            get
            {
                return (uint)this.animationPaletteWidth.GetFloat();
            }
            set
            {
                this.animationPaletteWidth.Set((float)value);
            }
        }
        /// <summary>
        /// Animation palette
        /// </summary>
        protected ShaderResourceView AnimationPalette
        {
            get
            {
                return this.animationPalette.GetResource();
            }
            set
            {
                this.animationPalette.SetResource(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadowBasic(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.ShadowMapPositionColor = this.Effect.GetTechniqueByName("ShadowMapPositionColor");
            this.ShadowMapPositionColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionColorSkinned");
            this.ShadowMapPositionNormalColor = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColor");
            this.ShadowMapPositionNormalColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinned");
            this.ShadowMapPositionTexture = this.Effect.GetTechniqueByName("ShadowMapPositionTexture");
            this.ShadowMapPositionTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionTextureSkinned");
            this.ShadowMapPositionNormalTexture = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTexture");
            this.ShadowMapPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinned");
            this.ShadowMapPositionNormalTextureTangent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangent");
            this.ShadowMapPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinned");
            this.InstancingShadowMapPositionColor = this.Effect.GetTechniqueByName("ShadowMapPositionColorI");
            this.InstancingShadowMapPositionColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionColorSkinnedI");
            this.InstancingShadowMapPositionNormalColor = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorI");
            this.InstancingShadowMapPositionNormalColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinnedI");
            this.InstancingShadowMapPositionTexture = this.Effect.GetTechniqueByName("ShadowMapPositionTextureI");
            this.InstancingShadowMapPositionTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionTextureSkinnedI");
            this.InstancingShadowMapPositionNormalTexture = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureI");
            this.InstancingShadowMapPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinnedI");
            this.InstancingShadowMapPositionNormalTextureTangent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentI");
            this.InstancingShadowMapPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinnedI");

            this.AddInputLayout(this.ShadowMapPositionColor, VertexPositionColor.GetInput());
            this.AddInputLayout(this.ShadowMapPositionColorSkinned, VertexSkinnedPositionColor.GetInput());
            this.AddInputLayout(this.ShadowMapPositionNormalColor, VertexPositionNormalColor.GetInput());
            this.AddInputLayout(this.ShadowMapPositionNormalColorSkinned, VertexSkinnedPositionNormalColor.GetInput());
            this.AddInputLayout(this.ShadowMapPositionTexture, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.ShadowMapPositionTextureSkinned, VertexSkinnedPositionTexture.GetInput());
            this.AddInputLayout(this.ShadowMapPositionNormalTexture, VertexPositionNormalTexture.GetInput());
            this.AddInputLayout(this.ShadowMapPositionNormalTextureSkinned, VertexSkinnedPositionNormalTexture.GetInput());
            this.AddInputLayout(this.ShadowMapPositionNormalTextureTangent, VertexPositionNormalTextureTangent.GetInput());
            this.AddInputLayout(this.ShadowMapPositionNormalTextureTangentSkinned, VertexSkinnedPositionNormalTextureTangent.GetInput());
            this.AddInputLayout(this.InstancingShadowMapPositionColor, VertexPositionColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingShadowMapPositionColorSkinned, VertexSkinnedPositionColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingShadowMapPositionNormalColor, VertexPositionNormalColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingShadowMapPositionNormalColorSkinned, VertexSkinnedPositionNormalColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingShadowMapPositionTexture, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingShadowMapPositionTextureSkinned, VertexSkinnedPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingShadowMapPositionNormalTexture, VertexPositionNormalTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingShadowMapPositionNormalTextureSkinned, VertexSkinnedPositionNormalTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingShadowMapPositionNormalTextureTangent, VertexPositionNormalTextureTangent.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingShadowMapPositionNormalTextureTangentSkinned, VertexSkinnedPositionNormalTextureTangent.GetInput().Merge(VertexInstancingData.GetInput()));

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.animationData = this.Effect.GetVariableByName("gAnimationData").AsVector();
            this.textureIndex = this.Effect.GetVariableByName("gTextureIndex").AsScalar();
            this.animationPaletteWidth = this.Effect.GetVariableByName("gPaletteWidth").AsScalar();
            this.animationPalette = this.Effect.GetVariableByName("gAnimationPalette").AsShaderResource();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            if (stage == DrawingStages.Drawing)
            {
                if (mode == DrawerModesEnum.ShadowMap)
                {
                    switch (vertexType)
                    {
                        case VertexTypes.PositionColor:
                            return instanced ? this.InstancingShadowMapPositionColor : this.ShadowMapPositionColor;
                        case VertexTypes.PositionTexture:
                            return instanced ? this.InstancingShadowMapPositionTexture : this.ShadowMapPositionTexture;
                        case VertexTypes.PositionNormalColor:
                            return instanced ? this.InstancingShadowMapPositionNormalColor : this.ShadowMapPositionNormalColor;
                        case VertexTypes.PositionNormalTexture:
                            return instanced ? this.InstancingShadowMapPositionNormalTexture : this.ShadowMapPositionNormalTexture;
                        case VertexTypes.PositionNormalTextureTangent:
                            return instanced ? this.InstancingShadowMapPositionNormalTextureTangent : this.ShadowMapPositionNormalTextureTangent;
                        case VertexTypes.PositionColorSkinned:
                            return instanced ? this.InstancingShadowMapPositionColorSkinned : this.ShadowMapPositionColorSkinned;
                        case VertexTypes.PositionTextureSkinned:
                            return instanced ? this.InstancingShadowMapPositionTextureSkinned : this.ShadowMapPositionTextureSkinned;
                        case VertexTypes.PositionNormalColorSkinned:
                            return instanced ? this.InstancingShadowMapPositionNormalColorSkinned : this.ShadowMapPositionNormalColorSkinned;
                        case VertexTypes.PositionNormalTextureSkinned:
                            return instanced ? this.InstancingShadowMapPositionNormalTextureSkinned : this.ShadowMapPositionNormalTextureSkinned;
                        case VertexTypes.PositionNormalTextureTangentSkinned:
                            return instanced ? this.InstancingShadowMapPositionNormalTextureTangentSkinned : this.ShadowMapPositionNormalTextureTangentSkinned;
                        default:
                            throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                    }
                }
                else
                {
                    throw new Exception(string.Format("Bad mode for effect: {0}", mode));
                }
            }
            else
            {
                throw new Exception(string.Format("Bad stage for effect: {0}", stage));
            }
        }
   
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            this.WorldViewProjection = world * viewProjection;
        }
        /// <summary>
        /// Update per group data
        /// </summary>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWith">Animation palette texture width</param>
        public void UpdatePerGroup(
            ShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {
            this.AnimationPalette = animationPalette;
            this.AnimationPaletteWidth = animationPaletteWidth;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animationData">Animation data</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObject(
            uint[] animationData,
            uint textureIndex)
        {
            this.AnimationData = animationData != null ? animationData : new uint[3];
            this.TextureIndex = textureIndex;
        }
    }
}
