using SharpDX;
using System;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDefaultBasic : Drawer, IGeometryDrawer
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
        /// Hemispheric light effect variable
        /// </summary>
        private readonly EngineEffectVariable hemiLight = null;
        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private readonly EngineEffectVariable dirLights = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private readonly EngineEffectVariable pointLights = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private readonly EngineEffectVariable spotLights = null;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lightCount = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private readonly EngineEffectVariableVector eyePositionWorld = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector fogColor = null;
        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationOffset = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialIndex = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureIndex = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorDiffuse = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorSpecular = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMap = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture specularMap = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationPaletteWidth = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private readonly EngineEffectVariableTexture animationPalette = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialPaletteWidth = null;
        /// <summary>
        /// Material palette
        /// </summary>
        private readonly EngineEffectVariableTexture materialPalette = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lod = null;
        /// <summary>
        /// Sampler for diffuse maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerDiffuse = null;
        /// <summary>
        /// Sampler for normal maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerNormal = null;
        /// <summary>
        /// Sampler for specular maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerSpecular = null;
        /// <summary>
        /// Directional shadow map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapDirectional = null;
        /// <summary>
        /// Point light shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapPoint = null;
        /// <summary>
        /// Spot light shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapSpot = null;

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
        /// Current directional shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapDirectional = null;
        /// <summary>
        /// Current point light shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapPoint = null;
        /// <summary>
        /// Current spot light shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapSpot = null;

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
        /// Hemispheric lights
        /// </summary>
        protected BufferHemisphericLight HemiLight
        {
            get
            {
                return this.hemiLight.GetValue<BufferHemisphericLight>();
            }
            set
            {
                this.hemiLight.SetValue(value);
            }
        }
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
        /// Directional shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapDirectional
        {
            get
            {
                return this.shadowMapDirectional.GetResource();
            }
            set
            {
                if (this.currentShadowMapDirectional != value)
                {
                    this.shadowMapDirectional.SetResource(value);

                    this.currentShadowMapDirectional = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Point light shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapPoint
        {
            get
            {
                return this.shadowMapPoint.GetResource();
            }
            set
            {
                if (this.currentShadowMapPoint != value)
                {
                    this.shadowMapPoint.SetResource(value);

                    this.currentShadowMapPoint = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Spot light shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapSpot
        {
            get
            {
                return this.shadowMapSpot.GetResource();
            }
            set
            {
                if (this.currentShadowMapSpot != value)
                {
                    this.shadowMapSpot.SetResource(value);

                    this.currentShadowMapSpot = value;

                    Counters.TextureUpdates++;
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
            this.eyePositionWorld = this.Effect.GetVariableVector("gPSEyePositionWorld");
            this.hemiLight = this.Effect.GetVariable("gPSHemiLight");
            this.dirLights = this.Effect.GetVariable("gPSDirLights");
            this.pointLights = this.Effect.GetVariable("gPSPointLights");
            this.spotLights = this.Effect.GetVariable("gPSSpotLights");
            this.lightCount = this.Effect.GetVariableVector("gPSLightCount");
            this.fogStart = this.Effect.GetVariableScalar("gPSFogStart");
            this.fogRange = this.Effect.GetVariableScalar("gPSFogRange");
            this.fogColor = this.Effect.GetVariableVector("gPSFogColor");
            this.shadowMapDirectional = this.Effect.GetVariableTexture("gPSShadowMapDir");
            this.shadowMapPoint = this.Effect.GetVariableTexture("gPSShadowMapPoint");
            this.shadowMapSpot = this.Effect.GetVariableTexture("gPSShadowMapSpot");

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
        /// Destructor
        /// </summary>
        ~EffectDefaultBasic()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.samplerPoint != null)
                {
                    this.samplerPoint.Dispose();
                    this.samplerPoint = null;
                }
                if (this.samplerLinear != null)
                {
                    this.samplerLinear.Dispose();
                    this.samplerLinear = null;
                }
                if (this.samplerAnisotropic != null)
                {
                    this.samplerAnisotropic.Dispose();
                    this.samplerAnisotropic = null;
                }
            }

            base.Dispose(disposing);
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
        /// <param name="context">Context</param>
        public void UpdatePerFrameBasic(
            Matrix world,
            DrawContext context)
        {
            this.UpdatePerFrame(
                world,
                context.ViewProjection,
                Vector3.Zero,
                null,
                null,
                null,
                null);
        }
        /// <summary>
        /// Update per frame full data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="context">Context</param>
        public void UpdatePerFrameFull(
            Matrix world,
            DrawContext context)
        {
            UpdatePerFrame(
                world,
                context.ViewProjection,
                context.EyePosition,
                context.Lights,
                context.ShadowMapDirectional,
                context.ShadowMapPoint,
                context.ShadowMapSpot);
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animationOffset">Animation index</param>
        /// <param name="material">Material</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="useAnisotropic">Use anisotropic filtering</param>
        public void UpdatePerObject(
            uint animationOffset,
            MeshMaterial material,
            uint textureIndex,
            bool useAnisotropic)
        {
            if (material != null)
            {
                this.DiffuseMap = material.DiffuseTexture;
                this.NormalMap = material.NormalMap;
                this.SpecularMap = material.SpecularTexture;
                this.UseColorDiffuse = material.DiffuseTexture != null;
                this.UseColorSpecular = material.SpecularTexture != null;
                this.MaterialIndex = material.ResourceIndex;
            }
            else
            {
                this.DiffuseMap = null;
                this.NormalMap = null;
                this.SpecularMap = null;
                this.UseColorDiffuse = false;
                this.UseColorSpecular = false;
                this.MaterialIndex = 0;
            }

            this.TextureIndex = textureIndex;
            this.Anisotropic = useAnisotropic;

            this.AnimationOffset = animationOffset;
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="lights">Scene ligths</param>
        /// <param name="shadowMapDirectional">Low definition shadow map</param>
        /// <param name="shadowMapPoint">Point light shadow map</param>
        /// <param name="shadowMapSpot">Spot light shadow map</param>
        private void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            SceneLights lights,
            IShadowMap shadowMapDirectional,
            IShadowMap shadowMapPoint,
            IShadowMap shadowMapSpot)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;

            var bHemiLight = BufferHemisphericLight.Default;
            var bDirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
            var bPointLights = new BufferPointLight[BufferPointLight.MAX];
            var bSpotLights = new BufferSpotLight[BufferSpotLight.MAX];
            var lCount = new[] { 0, 0, 0 };

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

                var hemiLight = lights.GetVisibleHemisphericLight();
                if (hemiLight != null)
                {
                    bHemiLight = new BufferHemisphericLight(hemiLight);
                }

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

                if (shadowMapDirectional != null)
                {
                    this.ShadowMapDirectional = shadowMapDirectional.Texture;
                }
                if (shadowMapPoint != null)
                {
                    this.ShadowMapPoint = shadowMapPoint.Texture;
                }
                if (shadowMapSpot != null)
                {
                    this.ShadowMapSpot = shadowMapSpot.Texture;
                }
            }
            else
            {
                this.EyePositionWorld = Vector3.Zero;

                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.ShadowMapDirectional = null;
                this.ShadowMapPoint = null;
                this.ShadowMapSpot = null;
            }

            this.HemiLight = bHemiLight;
            this.DirLights = bDirLights;
            this.PointLights = bPointLights;
            this.SpotLights = bSpotLights;
            this.LightCount = lCount;
        }
    }
}
