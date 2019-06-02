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
        private readonly EngineEffectVariable hemiLightVar = null;
        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private readonly EngineEffectVariable dirLightsVar = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private readonly EngineEffectVariable pointLightsVar = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private readonly EngineEffectVariable spotLightsVar = null;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lightCountVar = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private readonly EngineEffectVariableVector eyePositionWorldVar = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar fogStartVar = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar fogRangeVar = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector fogColorVar = null;
        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationOffsetVar = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialIndexVar = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureIndexVar = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorDiffuseVar = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorSpecularVar = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapVar = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture normalMapVar = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture specularMapVar = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationPaletteWidthVar = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private readonly EngineEffectVariableTexture animationPaletteVar = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialPaletteWidthVar = null;
        /// <summary>
        /// Material palette
        /// </summary>
        private readonly EngineEffectVariableTexture materialPaletteVar = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lodVar = null;
        /// <summary>
        /// Sampler for diffuse maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerDiffuseVar = null;
        /// <summary>
        /// Sampler for normal maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerNormalVar = null;
        /// <summary>
        /// Sampler for specular maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerSpecularVar = null;
        /// <summary>
        /// Directional shadow map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapDirectionalVar = null;
        /// <summary>
        /// Point light shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapPointVar = null;
        /// <summary>
        /// Spot light shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapSpotVar = null;

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
        protected BufferLightHemispheric HemiLight
        {
            get
            {
                return this.hemiLightVar.GetValue<BufferLightHemispheric>();
            }
            set
            {
                this.hemiLightVar.SetValue(value);
            }
        }
        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferLightDirectional[] DirLights
        {
            get
            {
                return this.dirLightsVar.GetValue<BufferLightDirectional>(BufferLightDirectional.MAX);
            }
            set
            {
                this.dirLightsVar.SetValue(value, BufferLightDirectional.MAX);
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferLightPoint[] PointLights
        {
            get
            {
                return this.pointLightsVar.GetValue<BufferLightPoint>(BufferLightPoint.MAX);
            }
            set
            {
                this.pointLightsVar.SetValue(value, BufferLightPoint.MAX);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferLightSpot[] SpotLights
        {
            get
            {
                return this.spotLightsVar.GetValue<BufferLightSpot>(BufferLightSpot.MAX);
            }
            set
            {
                this.spotLightsVar.SetValue(value, BufferLightSpot.MAX);
            }
        }
        /// <summary>
        /// Light count
        /// </summary>
        protected int[] LightCount
        {
            get
            {
                var v = this.lightCountVar.GetVector<Int3>();

                return new int[] { v.X, v.Y, v.Z };
            }
            set
            {
                var v = new Int3(value[0], value[1], value[2]);

                this.lightCountVar.Set(v);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                return this.eyePositionWorldVar.GetVector<Vector3>();
            }
            set
            {
                this.eyePositionWorldVar.Set(value);
            }
        }
        /// <summary>
        /// Fog start distance
        /// </summary>
        protected float FogStart
        {
            get
            {
                return this.fogStartVar.GetFloat();
            }
            set
            {
                this.fogStartVar.Set(value);
            }
        }
        /// <summary>
        /// Fog range distance
        /// </summary>
        protected float FogRange
        {
            get
            {
                return this.fogRangeVar.GetFloat();
            }
            set
            {
                this.fogRangeVar.Set(value);
            }
        }
        /// <summary>
        /// Fog color
        /// </summary>
        protected Color4 FogColor
        {
            get
            {
                return this.fogColorVar.GetVector<Color4>();
            }
            set
            {
                this.fogColorVar.Set(value);
            }
        }
        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return this.worldVar.GetMatrix();
            }
            set
            {
                this.worldVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjectionVar.GetMatrix();
            }
            set
            {
                this.worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Animation data
        /// </summary>
        protected uint AnimationOffset
        {
            get
            {
                return this.animationOffsetVar.GetUInt();
            }
            set
            {
                this.animationOffsetVar.Set(value);
            }
        }
        /// <summary>
        /// Material index
        /// </summary>
        protected uint MaterialIndex
        {
            get
            {
                return this.materialIndexVar.GetUInt();
            }
            set
            {
                this.materialIndexVar.Set(value);
            }
        }
        /// <summary>
        /// Texture index
        /// </summary>
        protected uint TextureIndex
        {
            get
            {
                return this.textureIndexVar.GetUInt();
            }
            set
            {
                this.textureIndexVar.Set(value);
            }
        }
        /// <summary>
        /// Use diffuse map color
        /// </summary>
        protected bool UseColorDiffuse
        {
            get
            {
                return this.useColorDiffuseVar.GetBool();
            }
            set
            {
                this.useColorDiffuseVar.Set(value);
            }
        }
        /// <summary>
        /// Use specular map color
        /// </summary>
        protected bool UseColorSpecular
        {
            get
            {
                return this.useColorSpecularVar.GetBool();
            }
            set
            {
                this.useColorSpecularVar.Set(value);
            }
        }
        /// <summary>
        /// Diffuse map
        /// </summary>
        protected EngineShaderResourceView DiffuseMap
        {
            get
            {
                return this.diffuseMapVar.GetResource();
            }
            set
            {
                if (this.currentDiffuseMap != value)
                {
                    this.diffuseMapVar.SetResource(value);

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
                return this.normalMapVar.GetResource();
            }
            set
            {
                if (this.currentNormalMap != value)
                {
                    this.normalMapVar.SetResource(value);

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
                return this.specularMapVar.GetResource();
            }
            set
            {
                if (this.currentSpecularMap != value)
                {
                    this.specularMapVar.SetResource(value);

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
                return this.animationPaletteWidthVar.GetUInt();
            }
            set
            {
                this.animationPaletteWidthVar.Set(value);
            }
        }
        /// <summary>
        /// Animation palette
        /// </summary>
        protected EngineShaderResourceView AnimationPalette
        {
            get
            {
                return this.animationPaletteVar.GetResource();
            }
            set
            {
                if (this.currentAnimationPalette != value)
                {
                    this.animationPaletteVar.SetResource(value);

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
                return this.materialPaletteWidthVar.GetUInt();
            }
            set
            {
                this.materialPaletteWidthVar.Set(value);
            }
        }
        /// <summary>
        /// Material palette
        /// </summary>
        protected EngineShaderResourceView MaterialPalette
        {
            get
            {
                return this.materialPaletteVar.GetResource();
            }
            set
            {
                if (this.currentMaterialPalette != value)
                {
                    this.materialPaletteVar.SetResource(value);

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
                return this.lodVar.GetVector<Vector3>();
            }
            set
            {
                this.lodVar.Set(value);
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

                    this.samplerDiffuseVar.SetValue(0, sampler);
                    this.samplerNormalVar.SetValue(0, sampler);
                    this.samplerSpecularVar.SetValue(0, sampler);
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
                return this.shadowMapDirectionalVar.GetResource();
            }
            set
            {
                if (this.currentShadowMapDirectional != value)
                {
                    this.shadowMapDirectionalVar.SetResource(value);

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
                return this.shadowMapPointVar.GetResource();
            }
            set
            {
                if (this.currentShadowMapPoint != value)
                {
                    this.shadowMapPointVar.SetResource(value);

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
                return this.shadowMapSpotVar.GetResource();
            }
            set
            {
                if (this.currentShadowMapSpot != value)
                {
                    this.shadowMapSpotVar.SetResource(value);

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
            this.animationPaletteWidthVar = this.Effect.GetVariableScalar("gAnimationPaletteWidth");
            this.animationPaletteVar = this.Effect.GetVariableTexture("gAnimationPalette");
            this.materialPaletteWidthVar = this.Effect.GetVariableScalar("gMaterialPaletteWidth");
            this.materialPaletteVar = this.Effect.GetVariableTexture("gMaterialPalette");
            this.lodVar = this.Effect.GetVariableVector("gLOD");

            //Per frame
            this.worldVar = this.Effect.GetVariableMatrix("gVSWorld");
            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gVSWorldViewProjection");
            this.eyePositionWorldVar = this.Effect.GetVariableVector("gPSEyePositionWorld");
            this.hemiLightVar = this.Effect.GetVariable("gPSHemiLight");
            this.dirLightsVar = this.Effect.GetVariable("gPSDirLights");
            this.pointLightsVar = this.Effect.GetVariable("gPSPointLights");
            this.spotLightsVar = this.Effect.GetVariable("gPSSpotLights");
            this.lightCountVar = this.Effect.GetVariableVector("gPSLightCount");
            this.fogStartVar = this.Effect.GetVariableScalar("gPSFogStart");
            this.fogRangeVar = this.Effect.GetVariableScalar("gPSFogRange");
            this.fogColorVar = this.Effect.GetVariableVector("gPSFogColor");
            this.shadowMapDirectionalVar = this.Effect.GetVariableTexture("gPSShadowMapDir");
            this.shadowMapPointVar = this.Effect.GetVariableTexture("gPSShadowMapPoint");
            this.shadowMapSpotVar = this.Effect.GetVariableTexture("gPSShadowMapSpot");

            //Per object
            this.useColorDiffuseVar = this.Effect.GetVariableScalar("gPSUseColorDiffuse");
            this.useColorSpecularVar = this.Effect.GetVariableScalar("gPSUseColorSpecular");
            this.diffuseMapVar = this.Effect.GetVariableTexture("gPSDiffuseMapArray");
            this.normalMapVar = this.Effect.GetVariableTexture("gPSNormalMapArray");
            this.specularMapVar = this.Effect.GetVariableTexture("gPSSpecularMapArray");

            //Per instance
            this.animationOffsetVar = this.Effect.GetVariableScalar("gVSAnimationOffset");
            this.materialIndexVar = this.Effect.GetVariableScalar("gPSMaterialIndex");
            this.textureIndexVar = this.Effect.GetVariableScalar("gPSTextureIndex");

            //Samplers
            this.samplerDiffuseVar = this.Effect.GetVariableSampler("SamplerDiffuse");
            this.samplerSpecularVar = this.Effect.GetVariableSampler("SamplerSpecular");
            this.samplerNormalVar = this.Effect.GetVariableSampler("SamplerNormal");

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
            if (!instanced)
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return this.PositionColor;
                    case VertexTypes.PositionTexture:
                        return this.PositionTexture;
                    case VertexTypes.PositionNormalColor:
                        return this.PositionNormalColor;
                    case VertexTypes.PositionNormalTexture:
                        return this.PositionNormalTexture;
                    case VertexTypes.PositionNormalTextureTangent:
                        return this.PositionNormalTextureTangent;
                    case VertexTypes.PositionColorSkinned:
                        return this.PositionColorSkinned;
                    case VertexTypes.PositionTextureSkinned:
                        return this.PositionTextureSkinned;
                    case VertexTypes.PositionNormalColorSkinned:
                        return this.PositionNormalColorSkinned;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return this.PositionNormalTextureSkinned;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return this.PositionNormalTextureTangentSkinned;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect: {0}", vertexType));
                }
            }
            else
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return this.InstancingPositionColor;
                    case VertexTypes.PositionTexture:
                        return this.InstancingPositionTexture;
                    case VertexTypes.PositionNormalColor:
                        return this.InstancingPositionNormalColor;
                    case VertexTypes.PositionNormalTexture:
                        return this.InstancingPositionNormalTexture;
                    case VertexTypes.PositionNormalTextureTangent:
                        return this.InstancingPositionNormalTextureTangent;
                    case VertexTypes.PositionColorSkinned:
                        return this.InstancingPositionColorSkinned;
                    case VertexTypes.PositionTextureSkinned:
                        return this.InstancingPositionTextureSkinned;
                    case VertexTypes.PositionNormalColorSkinned:
                        return this.InstancingPositionNormalColorSkinned;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return this.InstancingPositionNormalTextureSkinned;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return this.InstancingPositionNormalTextureTangentSkinned;
                    default:
                        throw new EngineException(string.Format("Bad instanced vertex type for effect: {0}", vertexType));
                }
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

            var bHemiLight = BufferLightHemispheric.Default;
            var bDirLights = new BufferLightDirectional[BufferLightDirectional.MAX];
            var bPointLights = new BufferLightPoint[BufferLightPoint.MAX];
            var bSpotLights = new BufferLightSpot[BufferLightSpot.MAX];
            var lCount = new[] { 0, 0, 0 };

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

                var hemi = lights.GetVisibleHemisphericLight();
                if (hemi != null)
                {
                    bHemiLight = new BufferLightHemispheric(hemi);
                }

                var dir = lights.GetVisibleDirectionalLights();
                for (int i = 0; i < Math.Min(dir.Length, BufferLightDirectional.MAX); i++)
                {
                    bDirLights[i] = new BufferLightDirectional(dir[i]);
                }

                var point = lights.GetVisiblePointLights();
                for (int i = 0; i < Math.Min(point.Length, BufferLightPoint.MAX); i++)
                {
                    bPointLights[i] = new BufferLightPoint(point[i]);
                }

                var spot = lights.GetVisibleSpotLights();
                for (int i = 0; i < Math.Min(spot.Length, BufferLightSpot.MAX); i++)
                {
                    bSpotLights[i] = new BufferLightSpot(spot[i]);
                }

                lCount[0] = Math.Min(dir.Length, BufferLightDirectional.MAX);
                lCount[1] = Math.Min(point.Length, BufferLightPoint.MAX);
                lCount[2] = Math.Min(spot.Length, BufferLightSpot.MAX);

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.ShadowMapDirectional = shadowMapDirectional?.Texture;
                this.ShadowMapPoint = shadowMapPoint?.Texture;
                this.ShadowMapSpot = shadowMapSpot?.Texture;
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
