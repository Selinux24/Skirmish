using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVariable = SharpDX.Direct3D11.EffectVariable;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectBasicGBuffer : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        public readonly EffectTechnique PositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        public readonly EffectTechnique PositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        public readonly EffectTechnique PositionNormalTextureTangentSkinned = null;
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        public readonly EffectTechnique InstancingPositionNormalTextureTangentSkinned = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// Inverse world matrix effect variable
        /// </summary>
        private EffectMatrixVariable worldInverse = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Material effect variable
        /// </summary>
        private EffectVariable material = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private EffectVectorVariable animationData = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private EffectScalarVariable textureIndex = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private EffectShaderResourceVariable diffuseMap = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EffectShaderResourceVariable normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private EffectShaderResourceVariable specularMap = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private EffectScalarVariable animationPaletteWidth = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private EffectShaderResourceVariable animationPalette = null;

        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return this.world.GetMatrix();
            }
            set
            {
                this.world.SetMatrix(value);
            }
        }
        /// <summary>
        /// Inverse world matrix
        /// </summary>
        protected Matrix WorldInverse
        {
            get
            {
                return this.worldInverse.GetMatrix();
            }
            set
            {
                this.worldInverse.SetMatrix(value);
            }
        }
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
        /// Material
        /// </summary>
        protected BufferMaterials Material
        {
            get
            {
                using (DataStream ds = this.material.GetRawValue(default(BufferMaterials).Stride))
                {
                    ds.Position = 0;

                    return ds.Read<BufferMaterials>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferMaterials>(new BufferMaterials[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.material.SetRawValue(ds, default(BufferMaterials).Stride);
                }
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
        protected int TextureIndex
        {
            get
            {
                return (int)this.textureIndex.GetFloat();
            }
            set
            {
                this.textureIndex.Set((float)value);
            }
        }
        /// <summary>
        /// Diffuse map
        /// </summary>
        protected ShaderResourceView DiffuseMap
        {
            get
            {
                return this.diffuseMap.GetResource();
            }
            set
            {
                this.diffuseMap.SetResource(value);
            }
        }
        /// <summary>
        /// Normal map
        /// </summary>
        protected ShaderResourceView NormalMap
        {
            get
            {
                return this.normalMap.GetResource();
            }
            set
            {
                this.normalMap.SetResource(value);
            }
        }
        /// <summary>
        /// Specular map
        /// </summary>
        protected ShaderResourceView SpecularMap
        {
            get
            {
                return this.specularMap.GetResource();
            }
            set
            {
                this.specularMap.SetResource(value);
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
        public EffectBasicGBuffer(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.PositionColor = this.Effect.GetTechniqueByName("PositionColor");
            this.PositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinned");
            this.PositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColor");
            this.PositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinned");
            this.PositionTexture = this.Effect.GetTechniqueByName("PositionTexture");
            this.PositionTextureSkinned = this.Effect.GetTechniqueByName("PositionTextureSkinned");
            this.PositionNormalTexture = this.Effect.GetTechniqueByName("PositionNormalTexture");
            this.PositionNormalTextureSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureSkinned");
            this.PositionNormalTextureTangent = this.Effect.GetTechniqueByName("PositionNormalTextureTangent");
            this.PositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureTangentSkinned");
            this.InstancingPositionColor = this.Effect.GetTechniqueByName("PositionColorI");
            this.InstancingPositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinnedI");
            this.InstancingPositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColorI");
            this.InstancingPositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinnedI");
            this.InstancingPositionTexture = this.Effect.GetTechniqueByName("PositionTextureI");
            this.InstancingPositionTextureSkinned = this.Effect.GetTechniqueByName("PositionTextureSkinnedI");
            this.InstancingPositionNormalTexture = this.Effect.GetTechniqueByName("PositionNormalTextureI");
            this.InstancingPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureSkinnedI");
            this.InstancingPositionNormalTextureTangent = this.Effect.GetTechniqueByName("PositionNormalTextureTangentI");
            this.InstancingPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureTangentSkinnedI");

            this.AddInputLayout(this.PositionColor, VertexPositionColor.GetInput());
            this.AddInputLayout(this.PositionColorSkinned, VertexSkinnedPositionColor.GetInput());
            this.AddInputLayout(this.PositionNormalColor, VertexPositionNormalColor.GetInput());
            this.AddInputLayout(this.PositionNormalColorSkinned, VertexSkinnedPositionNormalColor.GetInput());
            this.AddInputLayout(this.PositionTexture, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureSkinned, VertexSkinnedPositionTexture.GetInput());
            this.AddInputLayout(this.PositionNormalTexture, VertexPositionNormalTexture.GetInput());
            this.AddInputLayout(this.PositionNormalTextureSkinned, VertexSkinnedPositionNormalTexture.GetInput());
            this.AddInputLayout(this.PositionNormalTextureTangent, VertexPositionNormalTextureTangent.GetInput());
            this.AddInputLayout(this.PositionNormalTextureTangentSkinned, VertexSkinnedPositionNormalTextureTangent.GetInput());
            this.AddInputLayout(this.InstancingPositionColor, VertexPositionColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionColorSkinned, VertexSkinnedPositionColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalColor, VertexPositionNormalColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalColorSkinned, VertexSkinnedPositionNormalColor.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTexture, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureSkinned, VertexSkinnedPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTexture, VertexPositionNormalTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTextureSkinned, VertexSkinnedPositionNormalTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTextureTangent, VertexPositionNormalTextureTangent.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTextureTangentSkinned, VertexSkinnedPositionNormalTextureTangent.GetInput().Merge(VertexInstancingData.GetInput()));

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldInverse = this.Effect.GetVariableByName("gWorldInverse").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.material = this.Effect.GetVariableByName("gMaterial");
            this.animationData = this.Effect.GetVariableByName("gAnimationData").AsVector();
            this.textureIndex = this.Effect.GetVariableByName("gTextureIndex").AsScalar();
            this.diffuseMap = this.Effect.GetVariableByName("gDiffuseMapArray").AsShaderResource();
            this.normalMap = this.Effect.GetVariableByName("gNormalMapArray").AsShaderResource();
            this.specularMap = this.Effect.GetVariableByName("gSpecularMapArray").AsShaderResource();
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
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return instanced ? this.InstancingPositionColor : this.PositionColor;
                    case VertexTypes.PositionTexture:
                        return instanced ? this.InstancingPositionTexture : this.PositionTexture;
                    case VertexTypes.PositionNormalColor:
                        return instanced ? this.InstancingPositionNormalColor : this.PositionNormalColor;
                    case VertexTypes.PositionNormalTexture:
                        return instanced ? this.InstancingPositionNormalTexture : this.PositionNormalTexture;
                    case VertexTypes.PositionNormalTextureTangent:
                        return instanced ? this.InstancingPositionNormalTextureTangent : this.PositionNormalTextureTangent;
                    case VertexTypes.PositionColorSkinned:
                        return instanced ? this.InstancingPositionColorSkinned : this.PositionColorSkinned;
                    case VertexTypes.PositionTextureSkinned:
                        return instanced ? this.InstancingPositionTextureSkinned : this.PositionTextureSkinned;
                    case VertexTypes.PositionNormalColorSkinned:
                        return instanced ? this.InstancingPositionNormalColorSkinned : this.PositionNormalColorSkinned;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return instanced ? this.InstancingPositionNormalTextureSkinned : this.PositionNormalTextureSkinned;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return instanced ? this.InstancingPositionNormalTextureTangentSkinned : this.PositionNormalTextureTangentSkinned;
                    default:
                        throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
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
        /// <param name="world">World Matrix</param>
        /// <param name="viewProjection">View * projection</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            this.World = world;
            this.WorldInverse = Matrix.Invert(world);
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
        /// <param name="material">Material</param>
        /// <param name="diffuseMap">Diffuse map</param>
        /// <param name="normalMap">Normal map</param>
        /// <param name="specularMap">Specular map</param>
        /// <param name="animationData">Animation data</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObject(
            Material material,
            ShaderResourceView diffuseMap,
            ShaderResourceView normalMap,
            ShaderResourceView specularMap,
            uint[] animationData,
            int textureIndex)
        {
            this.Material = new BufferMaterials(material);
            this.DiffuseMap = diffuseMap;
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;
            this.AnimationData = animationData != null ? animationData : new uint[3];
            this.TextureIndex = textureIndex;
        }
    }
}
