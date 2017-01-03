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
    public class EffectDefaultBasic : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EffectTechnique PositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique PositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EffectTechnique PositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique PositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture technique
        /// </summary>
        protected readonly EffectTechnique PositionTexture = null;
        /// <summary>
        /// Position texture using red channer as gray-scale technique
        /// </summary>
        protected readonly EffectTechnique PositionTextureRED = null;
        /// <summary>
        /// Position texture using green channer as gray-scale technique
        /// </summary>
        protected readonly EffectTechnique PositionTextureGREEN = null;
        /// <summary>
        /// Position texture using blue channer as gray-scale technique
        /// </summary>
        protected readonly EffectTechnique PositionTextureBLUE = null;
        /// <summary>
        /// Position texture using alpha channer as gray-scale technique
        /// </summary>
        protected readonly EffectTechnique PositionTextureALPHA = null;
        /// <summary>
        /// Position texture without alpha channel
        /// </summary>
        protected readonly EffectTechnique PositionTextureNOALPHA = null;
        /// <summary>
        /// Position texture skinned technique
        /// </summary>
        protected readonly EffectTechnique PositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture technique
        /// </summary>
        protected readonly EffectTechnique PositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned technique
        /// </summary>
        protected readonly EffectTechnique PositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture with normal mapping technique
        /// </summary>
        protected readonly EffectTechnique PositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture skinned with normal mapping technique
        /// </summary>
        protected readonly EffectTechnique PositionNormalTextureTangentSkinned = null;
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionTexture = null;
        /// <summary>
        /// Position texture using red channer as gray-scale technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionTextureRED = null;
        /// <summary>
        /// Position texture using green channer as gray-scale technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionTextureGREEN = null;
        /// <summary>
        /// Position texture using blue channer as gray-scale technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionTextureBLUE = null;
        /// <summary>
        /// Position texture using alpha channer as gray-scale technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionTextureALPHA = null;
        /// <summary>
        /// Position texture without alpha channel
        /// </summary>
        protected readonly EffectTechnique InstancingPositionTextureNOALPHA = null;
        /// <summary>
        /// Position texture skinned technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture with normal mapping technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture skinned with normal mapping technique
        /// </summary>
        protected readonly EffectTechnique InstancingPositionNormalTextureTangentSkinned = null;

        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private EffectVariable dirLights = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private EffectVariable pointLights = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EffectVariable spotLights = null;
        /// <summary>
        /// Global ambient light effect variable;
        /// </summary>
        private EffectScalarVariable globalAmbient;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private EffectVectorVariable lightCount = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EffectVectorVariable eyePositionWorld = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private EffectScalarVariable fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private EffectScalarVariable fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private EffectVectorVariable fogColor = null;
        /// <summary>
        /// Shadow maps flag effect variable
        /// </summary>
        private EffectScalarVariable shadowMaps = null;
        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// From light View * Projection transform
        /// </summary>
        private EffectMatrixVariable fromLightViewProjection = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private EffectVectorVariable animationData = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private EffectScalarVariable materialIndex = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private EffectScalarVariable textureIndex = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private EffectScalarVariable useColorDiffuse = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private EffectScalarVariable useColorSpecular = null;
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
        /// Static shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMapStatic = null;
        /// <summary>
        /// Dynamic shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMapDynamic = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private EffectScalarVariable animationPaletteWidth = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private EffectShaderResourceVariable animationPalette = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private EffectScalarVariable materialPaletteWidth = null;
        /// <summary>
        /// Material palette
        /// </summary>
        private EffectShaderResourceVariable materialPalette = null;

        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferDirectionalLight[] DirLights
        {
            get
            {
                using (DataStream ds = this.dirLights.GetRawValue(default(BufferDirectionalLight).Stride * BufferDirectionalLight.MAX))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferDirectionalLight>(BufferDirectionalLight.MAX);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferDirectionalLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.dirLights.SetRawValue(ds, default(BufferDirectionalLight).Stride * BufferDirectionalLight.MAX);
                }
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferPointLight[] PointLights
        {
            get
            {
                using (DataStream ds = this.pointLights.GetRawValue(default(BufferPointLight).Stride * BufferPointLight.MAX))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferPointLight>(BufferPointLight.MAX);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferPointLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.pointLights.SetRawValue(ds, default(BufferPointLight).Stride * BufferPointLight.MAX);
                }
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferSpotLight[] SpotLights
        {
            get
            {
                using (DataStream ds = this.spotLights.GetRawValue(default(BufferSpotLight).Stride * BufferSpotLight.MAX))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferSpotLight>(BufferSpotLight.MAX);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferSpotLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.spotLights.SetRawValue(ds, default(BufferSpotLight).Stride * BufferSpotLight.MAX);
                }
            }
        }
        /// <summary>
        /// Global almbient light intensity
        /// </summary>
        protected float GlobalAmbient
        {
            get
            {
                return this.globalAmbient.GetFloat();
            }
            set
            {
                this.globalAmbient.Set(value);
            }
        }
        /// <summary>
        /// Light count
        /// </summary>
        protected int[] LightCount
        {
            get
            {
                Int4 v = this.lightCount.GetIntVector();

                return new int[] { v.X, v.Y, v.Z };
            }
            set
            {
                Int4 v4 = new Int4(value[0], value[1], value[2], 0);

                this.lightCount.Set(v4);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                Vector4 v = this.eyePositionWorld.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.eyePositionWorld.Set(v4);
            }
        }
        /// <summary>
        /// Fog start distance
        /// </summary>
        protected float FogStart
        {
            get
            {
                return this.fogStart.GetFloat();
            }
            set
            {
                this.fogStart.Set(value);
            }
        }
        /// <summary>
        /// Fog range distance
        /// </summary>
        protected float FogRange
        {
            get
            {
                return this.fogRange.GetFloat();
            }
            set
            {
                this.fogRange.Set(value);
            }
        }
        /// <summary>
        /// Fog color
        /// </summary>
        protected Color4 FogColor
        {
            get
            {
                return new Color4(this.fogColor.GetFloatVector());
            }
            set
            {
                this.fogColor.Set(value);
            }
        }
        /// <summary>
        /// Shadow maps flag
        /// </summary>
        protected int ShadowMaps
        {
            get
            {
                return this.shadowMaps.GetInt();
            }
            set
            {
                this.shadowMaps.Set(value);
            }
        }
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
        /// From light View * Projection transform
        /// </summary>
        protected Matrix FromLightViewProjection
        {
            get
            {
                return this.fromLightViewProjection.GetMatrix();
            }
            set
            {
                this.fromLightViewProjection.SetMatrix(value);
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
        /// Material index
        /// </summary>
        protected uint MaterialIndex
        {
            get
            {
                return (uint)this.materialIndex.GetFloat();
            }
            set
            {
                this.materialIndex.Set((float)value);
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
        /// Use diffuse map color
        /// </summary>
        protected bool UseColorDiffuse
        {
            get
            {
                return this.useColorDiffuse.GetBool();
            }
            set
            {
                this.useColorDiffuse.Set(value);
            }
        }
        /// <summary>
        /// Use specular map color
        /// </summary>
        protected bool UseColorSpecular
        {
            get
            {
                return this.useColorSpecular.GetBool();
            }
            set
            {
                this.useColorSpecular.Set(value);
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
        /// Static shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapStatic
        {
            get
            {
                return this.shadowMapStatic.GetResource();
            }
            set
            {
                this.shadowMapStatic.SetResource(value);
            }
        }
        /// <summary>
        /// Dynamic shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapDynamic
        {
            get
            {
                return this.shadowMapDynamic.GetResource();
            }
            set
            {
                this.shadowMapDynamic.SetResource(value);
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
        /// Material palette width
        /// </summary>
        protected uint MaterialPaletteWidth
        {
            get
            {
                return (uint)this.materialPaletteWidth.GetFloat();
            }
            set
            {
                this.materialPaletteWidth.Set((float)value);
            }
        }
        /// <summary>
        /// Material palette
        /// </summary>
        protected ShaderResourceView MaterialPalette
        {
            get
            {
                return this.materialPalette.GetResource();
            }
            set
            {
                this.materialPalette.SetResource(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultBasic(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.PositionColor = this.Effect.GetTechniqueByName("PositionColor");
            this.PositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinned");
            this.PositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColor");
            this.PositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinned");
            this.PositionTexture = this.Effect.GetTechniqueByName("PositionTexture");
            this.PositionTextureNOALPHA = this.Effect.GetTechniqueByName("PositionTextureNOALPHA");
            this.PositionTextureRED = this.Effect.GetTechniqueByName("PositionTextureRED");
            this.PositionTextureGREEN = this.Effect.GetTechniqueByName("PositionTextureGREEN");
            this.PositionTextureBLUE = this.Effect.GetTechniqueByName("PositionTextureBLUE");
            this.PositionTextureALPHA = this.Effect.GetTechniqueByName("PositionTextureALPHA");
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
            this.InstancingPositionTextureNOALPHA = this.Effect.GetTechniqueByName("PositionTextureNOALPHAI");
            this.InstancingPositionTextureRED = this.Effect.GetTechniqueByName("PositionTextureREDI");
            this.InstancingPositionTextureGREEN = this.Effect.GetTechniqueByName("PositionTextureGREENI");
            this.InstancingPositionTextureBLUE = this.Effect.GetTechniqueByName("PositionTextureBLUEI");
            this.InstancingPositionTextureALPHA = this.Effect.GetTechniqueByName("PositionTextureALPHAI");
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
            this.AddInputLayout(this.PositionTextureNOALPHA, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureRED, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureGREEN, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureBLUE, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.PositionTextureALPHA, VertexPositionTexture.GetInput());
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
            this.AddInputLayout(this.InstancingPositionTextureNOALPHA, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureRED, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureGREEN, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureBLUE, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureALPHA, VertexPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionTextureSkinned, VertexSkinnedPositionTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTexture, VertexPositionNormalTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTextureSkinned, VertexSkinnedPositionNormalTexture.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTextureTangent, VertexPositionNormalTextureTangent.GetInput().Merge(VertexInstancingData.GetInput()));
            this.AddInputLayout(this.InstancingPositionNormalTextureTangentSkinned, VertexSkinnedPositionNormalTextureTangent.GetInput().Merge(VertexInstancingData.GetInput()));

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.fromLightViewProjection = this.Effect.GetVariableByName("gLightViewProjection").AsMatrix();
            this.animationData = this.Effect.GetVariableByName("gAnimationData").AsVector();
            this.materialIndex = this.Effect.GetVariableByName("gMaterialIndex").AsScalar();
            this.textureIndex = this.Effect.GetVariableByName("gTextureIndex").AsScalar();
            this.useColorDiffuse = this.Effect.GetVariableByName("gUseColorDiffuse").AsScalar();
            this.useColorSpecular = this.Effect.GetVariableByName("gUseColorSpecular").AsScalar();
            this.dirLights = this.Effect.GetVariableByName("gDirLights");
            this.pointLights = this.Effect.GetVariableByName("gPointLights");
            this.spotLights = this.Effect.GetVariableByName("gSpotLights");
            this.globalAmbient = this.Effect.GetVariableByName("gGlobalAmbient").AsScalar();
            this.lightCount = this.Effect.GetVariableByName("gLightCount").AsVector();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.shadowMaps = this.Effect.GetVariableByName("gShadows").AsScalar();
            this.diffuseMap = this.Effect.GetVariableByName("gDiffuseMapArray").AsShaderResource();
            this.normalMap = this.Effect.GetVariableByName("gNormalMapArray").AsShaderResource();
            this.specularMap = this.Effect.GetVariableByName("gSpecularMapArray").AsShaderResource();
            this.shadowMapStatic = this.Effect.GetVariableByName("gShadowMapStatic").AsShaderResource();
            this.shadowMapDynamic = this.Effect.GetVariableByName("gShadowMapDynamic").AsShaderResource();
            this.animationPaletteWidth = this.Effect.GetVariableByName("gAnimationPaletteWidth").AsScalar();
            this.animationPalette = this.Effect.GetVariableByName("gAnimationPalette").AsShaderResource();
            this.materialPaletteWidth = this.Effect.GetVariableByName("gMaterialPaletteWidth").AsScalar();
            this.materialPalette = this.Effect.GetVariableByName("gMaterialPalette").AsShaderResource();
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
                if (mode == DrawerModesEnum.Forward)
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
                    throw new Exception(string.Format("Bad mode for effect: {0}", mode));
                }
            }
            else
            {
                throw new Exception(string.Format("Bad stage for effect: {0}", stage));
            }
        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWith">Animation palette texture width</param>
        public void UpdateGlobals(
            ShaderResourceView materialPalette,
            uint materialPaletteWidth,
            ShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {
            this.MaterialPalette = materialPalette;
            this.MaterialPaletteWidth = materialPaletteWidth;

            this.AnimationPalette = animationPalette;
            this.AnimationPaletteWidth = animationPaletteWidth;
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            this.UpdatePerFrame(world, viewProjection, Vector3.Zero, null, 0, null, null, Matrix.Identity);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="lights">Scene ligths</param>
        /// <param name="shadowMaps">Shadow map flags</param>
        /// <param name="shadowMapStatic">Static shadow map texture</param>
        /// <param name="shadowMapDynamic">Dynamic shadow map texture</param>
        /// <param name="fromLightViewProjection">From light View * Projection transform</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            SceneLights lights,
            int shadowMaps,
            ShaderResourceView shadowMapStatic,
            ShaderResourceView shadowMapDynamic,
            Matrix fromLightViewProjection)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;

            var globalAmbient = 0f;
            var bDirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
            var bPointLights = new BufferPointLight[BufferPointLight.MAX];
            var bSpotLights = new BufferSpotLight[BufferSpotLight.MAX];
            var lCount = new[] { 0, 0, 0 };

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

                globalAmbient = lights.GlobalAmbientLight;

                var dirLights = lights.GetVisibleDirectionalLights();
                for (int i = 0; i < Math.Min(dirLights.Length, BufferDirectionalLight.MAX); i++)
                {
                    bDirLights[i] = new BufferDirectionalLight(dirLights[i]);
                }

                var pointLights = lights.GetVisiblePointLights();
                for (int i = 0; i < Math.Min(pointLights.Length, BufferPointLight.MAX); i++)
                {
                    bPointLights[i] = new BufferPointLight(pointLights[i]);
                }

                var spotLights = lights.GetVisibleSpotLights();
                for (int i = 0; i < Math.Min(spotLights.Length, BufferSpotLight.MAX); i++)
                {
                    bSpotLights[i] = new BufferSpotLight(spotLights[i]);
                }

                lCount[0] = dirLights.Length;
                lCount[1] = pointLights.Length;
                lCount[2] = spotLights.Length;

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.FromLightViewProjection = fromLightViewProjection;
                this.ShadowMapStatic = shadowMapStatic;
                this.ShadowMapDynamic = shadowMapDynamic;
                this.ShadowMaps = shadowMaps;
            }
            else
            {
                this.EyePositionWorld = Vector3.Zero;

                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.FromLightViewProjection = Matrix.Identity;
                this.ShadowMapStatic = null;
                this.ShadowMapDynamic = null;
                this.ShadowMaps = 0;
            }

            this.GlobalAmbient = globalAmbient;
            this.DirLights = bDirLights;
            this.PointLights = bPointLights;
            this.SpotLights = bSpotLights;
            this.LightCount = lCount;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="diffuseMap">Diffuse map</param>
        /// <param name="normalMap">Normal map</param>
        /// <param name="specularMap">Specular map</param>
        /// <param name="materialIndex">Material index</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="animationIndex">Animation index</param>
        public void UpdatePerObject(
            ShaderResourceView diffuseMap,
            ShaderResourceView normalMap,
            ShaderResourceView specularMap,
            uint materialIndex,
            uint textureIndex,
            uint animationIndex)
        {
            this.DiffuseMap = diffuseMap;
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;
            this.UseColorDiffuse = diffuseMap != null;
            this.UseColorSpecular = specularMap != null;
            this.MaterialIndex = materialIndex;
            this.TextureIndex = textureIndex;

            this.AnimationData = new uint[] { 0, animationIndex, 0 };
        }
    }
}
