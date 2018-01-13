using SharpDX;
using System;

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
        protected readonly EngineEffectTechnique PositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTexture = null;
        /// <summary>
        /// Position texture using red channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureRED = null;
        /// <summary>
        /// Position texture using green channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureGREEN = null;
        /// <summary>
        /// Position texture using blue channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureBLUE = null;
        /// <summary>
        /// Position texture using alpha channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureALPHA = null;
        /// <summary>
        /// Position texture without alpha channel
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureNOALPHA = null;
        /// <summary>
        /// Position texture skinned technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture with normal mapping technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture skinned with normal mapping technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTextureTangentSkinned = null;
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTexture = null;
        /// <summary>
        /// Position texture using red channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTextureRED = null;
        /// <summary>
        /// Position texture using green channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTextureGREEN = null;
        /// <summary>
        /// Position texture using blue channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTextureBLUE = null;
        /// <summary>
        /// Position texture using alpha channer as gray-scale technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTextureALPHA = null;
        /// <summary>
        /// Position texture without alpha channel
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTextureNOALPHA = null;
        /// <summary>
        /// Position texture skinned technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture with normal mapping technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture skinned with normal mapping technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTextureTangentSkinned = null;

        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private EngineEffectVariable dirLights = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private EngineEffectVariable pointLights = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EngineEffectVariable spotLights = null;
        /// <summary>
        /// Global ambient light effect variable;
        /// </summary>
        private EngineEffectVariableScalar globalAmbient;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private EngineEffectVariableVector lightCount = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EngineEffectVariableVector eyePositionWorld = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private EngineEffectVariableScalar fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private EngineEffectVariableScalar fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private EngineEffectVariableVector fogColor = null;
        /// <summary>
        /// Shadow maps flag effect variable
        /// </summary>
        private EngineEffectVariableScalar shadowMaps = null;
        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Low definition map from light View * Projection transform
        /// </summary>
        private EngineEffectVariableMatrix fromLightViewProjectionLD = null;
        /// <summary>
        /// High definition map from light View * Projection transform
        /// </summary>
        private EngineEffectVariableMatrix fromLightViewProjectionHD = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private EngineEffectVariableScalar animationOffset = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private EngineEffectVariableScalar materialIndex = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private EngineEffectVariableScalar textureIndex = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private EngineEffectVariableScalar useColorDiffuse = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private EngineEffectVariableScalar useColorSpecular = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private EngineEffectVariableTexture diffuseMap = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EngineEffectVariableTexture normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private EngineEffectVariableTexture specularMap = null;
        /// <summary>
        /// Low definition shadow map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapLD = null;
        /// <summary>
        /// High definition shadow map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapHD = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private EngineEffectVariableScalar animationPaletteWidth = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private EngineEffectVariableTexture animationPalette = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private EngineEffectVariableScalar materialPaletteWidth = null;
        /// <summary>
        /// Material palette
        /// </summary>
        private EngineEffectVariableTexture materialPalette = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private EngineEffectVariableVector lod = null;
        /// <summary>
        /// Sampler for diffuse maps
        /// </summary>
        private EngineEffectVariableSampler samplerDiffuse = null;
        /// <summary>
        /// Sampler for normal maps
        /// </summary>
        private EngineEffectVariableSampler samplerNormal = null;
        /// <summary>
        /// Sampler for specular maps
        /// </summary>
        private EngineEffectVariableSampler samplerSpecular = null;

        /// <summary>
        /// Current diffuse map
        /// </summary>
        private EngineShaderResourceView currentDiffuseMap = null;
        /// <summary>
        /// Current normal map
        /// </summary>
        private EngineShaderResourceView currentNormalMap = null;
        /// <summary>
        /// Current specular map
        /// </summary>
        private EngineShaderResourceView currentSpecularMap = null;
        /// <summary>
        /// Current low definition shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapLD = null;
        /// <summary>
        /// Current high definition shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapHD = null;
        /// <summary>
        /// Current animation palette
        /// </summary>
        private EngineShaderResourceView currentAnimationPalette = null;
        /// <summary>
        /// Current material palette
        /// </summary>
        private EngineShaderResourceView currentMaterialPalette = null;
        /// <summary>
        /// Use anisotropic sampling
        /// </summary>
        private bool? anisotropic = null;

        /// <summary>
        /// Sampler point
        /// </summary>
        private EngineSamplerState samplerPoint = null;
        /// <summary>
        /// Sampler linear
        /// </summary>
        private EngineSamplerState samplerLinear = null;
        /// <summary>
        /// Sampler anisotropic
        /// </summary>
        private EngineSamplerState samplerAnisotropic = null;

        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferDirectionalLight[] DirLights
        {
            get
            {
                return this.dirLights.GetValue<BufferDirectionalLight>(BufferDirectionalLight.MAX);
            }
            set
            {
                this.dirLights.SetValue(value, BufferDirectionalLight.MAX);
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferPointLight[] PointLights
        {
            get
            {
                return this.pointLights.GetValue<BufferPointLight>(BufferPointLight.MAX);
            }
            set
            {
                this.pointLights.SetValue(value, BufferPointLight.MAX);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferSpotLight[] SpotLights
        {
            get
            {
                return this.spotLights.GetValue<BufferSpotLight>(BufferSpotLight.MAX);
            }
            set
            {
                this.spotLights.SetValue(value, BufferSpotLight.MAX);
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
                var v = this.lightCount.GetVector<Int3>();

                return new int[] { v.X, v.Y, v.Z };
            }
            set
            {
                var v = new Int3(value[0], value[1], value[2]);

                this.lightCount.Set(v);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                return this.eyePositionWorld.GetVector<Vector3>();
            }
            set
            {
                this.eyePositionWorld.Set(value);
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
                return this.fogColor.GetVector<Color4>();
            }
            set
            {
                this.fogColor.Set(value);
            }
        }
        /// <summary>
        /// Shadow maps flag
        /// </summary>
        protected uint ShadowMaps
        {
            get
            {
                return this.shadowMaps.GetUInt();
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
        /// Low definition map from light View * Projection transform
        /// </summary>
        protected Matrix FromLightViewProjectionLD
        {
            get
            {
                return this.fromLightViewProjectionLD.GetMatrix();
            }
            set
            {
                this.fromLightViewProjectionLD.SetMatrix(value);
            }
        }
        /// <summary>
        /// High definition map from light View * Projection transform
        /// </summary>
        protected Matrix FromLightViewProjectionHD
        {
            get
            {
                return this.fromLightViewProjectionHD.GetMatrix();
            }
            set
            {
                this.fromLightViewProjectionHD.SetMatrix(value);
            }
        }
        /// <summary>
        /// Animation data
        /// </summary>
        protected uint AnimationOffset
        {
            get
            {
                return this.animationOffset.GetUInt();
            }
            set
            {
                this.animationOffset.Set(value);
            }
        }
        /// <summary>
        /// Material index
        /// </summary>
        protected uint MaterialIndex
        {
            get
            {
                return this.materialIndex.GetUInt();
            }
            set
            {
                this.materialIndex.Set(value);
            }
        }
        /// <summary>
        /// Texture index
        /// </summary>
        protected uint TextureIndex
        {
            get
            {
                return this.textureIndex.GetUInt();
            }
            set
            {
                this.textureIndex.Set(value);
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
        protected EngineShaderResourceView DiffuseMap
        {
            get
            {
                return this.diffuseMap.GetResource();
            }
            set
            {
                if (this.currentDiffuseMap != value)
                {
                    this.diffuseMap.SetResource(value);

                    this.currentDiffuseMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Normal map
        /// </summary>
        protected EngineShaderResourceView NormalMap
        {
            get
            {
                return this.normalMap.GetResource();
            }
            set
            {
                if (this.currentNormalMap != value)
                {
                    this.normalMap.SetResource(value);

                    this.currentNormalMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Specular map
        /// </summary>
        protected EngineShaderResourceView SpecularMap
        {
            get
            {
                return this.specularMap.GetResource();
            }
            set
            {
                if (this.currentSpecularMap != value)
                {
                    this.specularMap.SetResource(value);

                    this.currentSpecularMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Low definition shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapLD
        {
            get
            {
                return this.shadowMapLD.GetResource();
            }
            set
            {
                if (this.currentShadowMapLD != value)
                {
                    this.shadowMapLD.SetResource(value);

                    this.currentShadowMapLD = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// High definition shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapHD
        {
            get
            {
                return this.shadowMapHD.GetResource();
            }
            set
            {
                if (this.currentShadowMapHD != value)
                {
                    this.shadowMapHD.SetResource(value);

                    this.currentShadowMapHD = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Animation palette width
        /// </summary>
        protected uint AnimationPaletteWidth
        {
            get
            {
                return this.animationPaletteWidth.GetUInt();
            }
            set
            {
                this.animationPaletteWidth.Set(value);
            }
        }
        /// <summary>
        /// Animation palette
        /// </summary>
        protected EngineShaderResourceView AnimationPalette
        {
            get
            {
                return this.animationPalette.GetResource();
            }
            set
            {
                if (this.currentAnimationPalette != value)
                {
                    this.animationPalette.SetResource(value);

                    this.currentAnimationPalette = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Material palette width
        /// </summary>
        protected uint MaterialPaletteWidth
        {
            get
            {
                return this.materialPaletteWidth.GetUInt();
            }
            set
            {
                this.materialPaletteWidth.Set(value);
            }
        }
        /// <summary>
        /// Material palette
        /// </summary>
        protected EngineShaderResourceView MaterialPalette
        {
            get
            {
                return this.materialPalette.GetResource();
            }
            set
            {
                if (this.currentMaterialPalette != value)
                {
                    this.materialPalette.SetResource(value);

                    this.currentMaterialPalette = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Level of detail ranges
        /// </summary>
        protected Vector3 LOD
        {
            get
            {
                return this.lod.GetVector<Vector3>();
            }
            set
            {
                this.lod.Set(value);
            }
        }
        /// <summary>
        /// Gets or sets if the effect use anisotropic filtering
        /// </summary>
        public bool Anisotropic
        {
            get
            {
                return this.anisotropic == true;
            }
            set
            {
                if (this.anisotropic != value)
                {
                    this.anisotropic = value;

                    var sampler = this.anisotropic == true ?
                        this.samplerAnisotropic.GetSamplerState() :
                        this.samplerLinear.GetSamplerState();

                    this.samplerDiffuse.SetValue(0, sampler);
                    this.samplerNormal.SetValue(0, sampler);
                    this.samplerSpecular.SetValue(0, sampler);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultBasic(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
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

            //Globals
            this.animationPaletteWidth = this.Effect.GetVariableScalar("gAnimationPaletteWidth");
            this.animationPalette = this.Effect.GetVariableTexture("gAnimationPalette");
            this.materialPaletteWidth = this.Effect.GetVariableScalar("gMaterialPaletteWidth");
            this.materialPalette = this.Effect.GetVariableTexture("gMaterialPalette");
            this.lod = this.Effect.GetVariableVector("gLOD");

            //Per frame
            this.world = this.Effect.GetVariableMatrix("gVSWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gVSWorldViewProjection");
            this.fromLightViewProjectionLD = this.Effect.GetVariableMatrix("gPSLightViewProjectionLD");
            this.fromLightViewProjectionHD = this.Effect.GetVariableMatrix("gPSLightViewProjectionHD");
            this.eyePositionWorld = this.Effect.GetVariableVector("gPSEyePositionWorld");
            this.globalAmbient = this.Effect.GetVariableScalar("gPSGlobalAmbient");
            this.dirLights = this.Effect.GetVariable("gPSDirLights");
            this.pointLights = this.Effect.GetVariable("gPSPointLights");
            this.spotLights = this.Effect.GetVariable("gPSSpotLights");
            this.lightCount = this.Effect.GetVariableVector("gPSLightCount");
            this.fogStart = this.Effect.GetVariableScalar("gPSFogStart");
            this.fogRange = this.Effect.GetVariableScalar("gPSFogRange");
            this.fogColor = this.Effect.GetVariableVector("gPSFogColor");
            this.shadowMaps = this.Effect.GetVariableScalar("gPSShadows");
            this.shadowMapLD = this.Effect.GetVariableTexture("gPSShadowMapLD");
            this.shadowMapHD = this.Effect.GetVariableTexture("gPSShadowMapHD");

            //Per object
            this.useColorDiffuse = this.Effect.GetVariableScalar("gPSUseColorDiffuse");
            this.useColorSpecular = this.Effect.GetVariableScalar("gPSUseColorSpecular");
            this.diffuseMap = this.Effect.GetVariableTexture("gPSDiffuseMapArray");
            this.normalMap = this.Effect.GetVariableTexture("gPSNormalMapArray");
            this.specularMap = this.Effect.GetVariableTexture("gPSSpecularMapArray");

            //Per instance
            this.animationOffset = this.Effect.GetVariableScalar("gVSAnimationOffset");
            this.materialIndex = this.Effect.GetVariableScalar("gPSMaterialIndex");
            this.textureIndex = this.Effect.GetVariableScalar("gPSTextureIndex");

            //Samplers
            this.samplerDiffuse = this.Effect.GetVariableSampler("SamplerDiffuse");
            this.samplerSpecular = this.Effect.GetVariableSampler("SamplerSpecular");
            this.samplerNormal = this.Effect.GetVariableSampler("SamplerNormal");

            //Initialize states
            this.samplerPoint = EngineSamplerState.Point(graphics);
            this.samplerLinear = EngineSamplerState.Linear(graphics);
            this.samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, 4);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.samplerPoint);
            Helper.Dispose(this.samplerLinear);
            Helper.Dispose(this.samplerAnisotropic);

            base.Dispose();
        }

        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        public EngineEffectTechnique GetTechnique(
            VertexTypes vertexType, 
            bool instanced)
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
                    throw new EngineException(string.Format("Bad vertex type for effect: {0}", vertexType));
            }
        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWidth">Animation palette texture width</param>
        /// <param name="lod1">High level of detail maximum distance</param>
        /// <param name="lod2">Medium level of detail maximum distance</param>
        /// <param name="lod3">Low level of detail maximum distance</param>
        public void UpdateGlobals(
            EngineShaderResourceView materialPalette,
            uint materialPaletteWidth,
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth,
            float lod1,
            float lod2,
            float lod3)
        {
            this.MaterialPalette = materialPalette;
            this.MaterialPaletteWidth = materialPaletteWidth;

            this.AnimationPalette = animationPalette;
            this.AnimationPaletteWidth = animationPaletteWidth;

            this.LOD = new Vector3(lod1, lod2, lod3);
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
            this.UpdatePerFrame(world, viewProjection, Vector3.Zero, null, 0, null, null, Matrix.Identity, Matrix.Identity);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="lights">Scene ligths</param>
        /// <param name="shadowMaps">Shadow map flags</param>
        /// <param name="shadowMapLD">Low definition shadow map texture</param>
        /// <param name="shadowMapHD">High definition shadow map texture</param>
        /// <param name="fromLightViewProjectionLD">Low definition map from light View * Projection transform</param>
        /// <param name="fromLightViewProjectionHD">High definition map from light View * Projection transform</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            SceneLights lights,
            ShadowMapFlags shadowMaps,
            EngineShaderResourceView shadowMapLD,
            EngineShaderResourceView shadowMapHD,
            Matrix fromLightViewProjectionLD,
            Matrix fromLightViewProjectionHD)
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

                lCount[0] = Math.Min(dirLights.Length, BufferDirectionalLight.MAX);
                lCount[1] = Math.Min(pointLights.Length, BufferPointLight.MAX);
                lCount[2] = Math.Min(spotLights.Length, BufferSpotLight.MAX);

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.FromLightViewProjectionLD = fromLightViewProjectionLD;
                this.FromLightViewProjectionHD = fromLightViewProjectionHD;
                this.ShadowMapLD = shadowMapLD;
                this.ShadowMapHD = shadowMapHD;
                this.ShadowMaps = (uint)shadowMaps;
            }
            else
            {
                this.EyePositionWorld = Vector3.Zero;

                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.FromLightViewProjectionLD = Matrix.Identity;
                this.FromLightViewProjectionHD = Matrix.Identity;
                this.ShadowMapLD = null;
                this.ShadowMapHD = null;
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
        /// <param name="useAnisotropic">Use anisotropic filtering</param>
        /// <param name="diffuseMap">Diffuse map</param>
        /// <param name="normalMap">Normal map</param>
        /// <param name="specularMap">Specular map</param>
        /// <param name="materialIndex">Material index</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="animationOffset">Animation index</param>
        public void UpdatePerObject(
            bool useAnisotropic,
            EngineShaderResourceView diffuseMap,
            EngineShaderResourceView normalMap,
            EngineShaderResourceView specularMap,
            uint materialIndex,
            uint textureIndex,
            uint animationOffset)
        {
            this.Anisotropic = useAnisotropic;

            this.DiffuseMap = diffuseMap;
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;
            this.UseColorDiffuse = diffuseMap != null;
            this.UseColorSpecular = specularMap != null;
            this.MaterialIndex = materialIndex;
            this.TextureIndex = textureIndex;

            this.AnimationOffset = animationOffset;
        }
    }
}
