using SharpDX;

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
        /// Tint color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector tintColorVar = null;
        /// <summary>
        /// First animation offset effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationOffsetVar = null;
        /// <summary>
        /// Second animation offset effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationOffset2Var = null;
        /// <summary>
        /// Animation interpolation value between offsets
        /// </summary>
        private readonly EngineEffectVariableScalar animationInterpolationVar = null;
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
        /// Diffuse map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapVar = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture normalMapVar = null;
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
                return hemiLightVar.GetValue<BufferLightHemispheric>();
            }
            set
            {
                hemiLightVar.SetValue(value);
            }
        }
        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferLightDirectional[] DirLights
        {
            get
            {
                return dirLightsVar.GetValue<BufferLightDirectional>(BufferLightDirectional.MAX);
            }
            set
            {
                dirLightsVar.SetValue(value, BufferLightDirectional.MAX);
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferLightPoint[] PointLights
        {
            get
            {
                return pointLightsVar.GetValue<BufferLightPoint>(BufferLightPoint.MAX);
            }
            set
            {
                pointLightsVar.SetValue(value, BufferLightPoint.MAX);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferLightSpot[] SpotLights
        {
            get
            {
                return spotLightsVar.GetValue<BufferLightSpot>(BufferLightSpot.MAX);
            }
            set
            {
                spotLightsVar.SetValue(value, BufferLightSpot.MAX);
            }
        }
        /// <summary>
        /// Light count
        /// </summary>
        protected int[] LightCount
        {
            get
            {
                var v = lightCountVar.GetVector<Int3>();

                return new int[] { v.X, v.Y, v.Z };
            }
            set
            {
                var v = new Int3(value[0], value[1], value[2]);

                lightCountVar.Set(v);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                return eyePositionWorldVar.GetVector<Vector3>();
            }
            set
            {
                eyePositionWorldVar.Set(value);
            }
        }
        /// <summary>
        /// Fog start distance
        /// </summary>
        protected float FogStart
        {
            get
            {
                return fogStartVar.GetFloat();
            }
            set
            {
                fogStartVar.Set(value);
            }
        }
        /// <summary>
        /// Fog range distance
        /// </summary>
        protected float FogRange
        {
            get
            {
                return fogRangeVar.GetFloat();
            }
            set
            {
                fogRangeVar.Set(value);
            }
        }
        /// <summary>
        /// Fog color
        /// </summary>
        protected Color4 FogColor
        {
            get
            {
                return fogColorVar.GetVector<Color4>();
            }
            set
            {
                fogColorVar.Set(value);
            }
        }
        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return worldVar.GetMatrix();
            }
            set
            {
                worldVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return worldViewProjectionVar.GetMatrix();
            }
            set
            {
                worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Tint color
        /// </summary>
        protected Color4 TintColor
        {
            get
            {
                return tintColorVar.GetVector<Color4>();
            }
            set
            {
                tintColorVar.Set(value);
            }
        }
        /// <summary>
        /// First animation offset
        /// </summary>
        protected uint AnimationOffset
        {
            get
            {
                return animationOffsetVar.GetUInt();
            }
            set
            {
                animationOffsetVar.Set(value);
            }
        }
        /// <summary>
        /// Second animation offset
        /// </summary>
        protected uint AnimationOffset2
        {
            get
            {
                return animationOffset2Var.GetUInt();
            }
            set
            {
                animationOffset2Var.Set(value);
            }
        }
        /// <summary>
        /// Animation interpolation between offsets
        /// </summary>
        protected float AnimationInterpolation
        {
            get
            {
                return animationInterpolationVar.GetFloat();
            }
            set
            {
                animationInterpolationVar.Set(value);
            }
        }
        /// <summary>
        /// Material index
        /// </summary>
        protected uint MaterialIndex
        {
            get
            {
                return materialIndexVar.GetUInt();
            }
            set
            {
                materialIndexVar.Set(value);
            }
        }
        /// <summary>
        /// Texture index
        /// </summary>
        protected uint TextureIndex
        {
            get
            {
                return textureIndexVar.GetUInt();
            }
            set
            {
                textureIndexVar.Set(value);
            }
        }
        /// <summary>
        /// Use diffuse map color
        /// </summary>
        protected bool UseColorDiffuse
        {
            get
            {
                return useColorDiffuseVar.GetBool();
            }
            set
            {
                useColorDiffuseVar.Set(value);
            }
        }
        /// <summary>
        /// Diffuse map
        /// </summary>
        protected EngineShaderResourceView DiffuseMap
        {
            get
            {
                return diffuseMapVar.GetResource();
            }
            set
            {
                if (currentDiffuseMap != value)
                {
                    diffuseMapVar.SetResource(value);

                    currentDiffuseMap = value;

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
                return normalMapVar.GetResource();
            }
            set
            {
                if (currentNormalMap != value)
                {
                    normalMapVar.SetResource(value);

                    currentNormalMap = value;

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
                return animationPaletteWidthVar.GetUInt();
            }
            set
            {
                animationPaletteWidthVar.Set(value);
            }
        }
        /// <summary>
        /// Animation palette
        /// </summary>
        protected EngineShaderResourceView AnimationPalette
        {
            get
            {
                return animationPaletteVar.GetResource();
            }
            set
            {
                if (currentAnimationPalette != value)
                {
                    animationPaletteVar.SetResource(value);

                    currentAnimationPalette = value;

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
                return materialPaletteWidthVar.GetUInt();
            }
            set
            {
                materialPaletteWidthVar.Set(value);
            }
        }
        /// <summary>
        /// Material palette
        /// </summary>
        protected EngineShaderResourceView MaterialPalette
        {
            get
            {
                return materialPaletteVar.GetResource();
            }
            set
            {
                if (currentMaterialPalette != value)
                {
                    materialPaletteVar.SetResource(value);

                    currentMaterialPalette = value;

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
                return lodVar.GetVector<Vector3>();
            }
            set
            {
                lodVar.Set(value);
            }
        }
        /// <summary>
        /// Gets or sets if the effect use anisotropic filtering
        /// </summary>
        public bool Anisotropic
        {
            get
            {
                return anisotropic == true;
            }
            set
            {
                if (anisotropic != value)
                {
                    anisotropic = value;

                    var sampler = anisotropic == true ?
                        samplerAnisotropic.GetSamplerState() :
                        samplerLinear.GetSamplerState();

                    samplerDiffuseVar.SetValue(0, sampler);
                    samplerNormalVar.SetValue(0, sampler);
                    samplerSpecularVar.SetValue(0, sampler);
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
                return shadowMapDirectionalVar.GetResource();
            }
            set
            {
                if (currentShadowMapDirectional != value)
                {
                    shadowMapDirectionalVar.SetResource(value);

                    currentShadowMapDirectional = value;

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
                return shadowMapPointVar.GetResource();
            }
            set
            {
                if (currentShadowMapPoint != value)
                {
                    shadowMapPointVar.SetResource(value);

                    currentShadowMapPoint = value;

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
                return shadowMapSpotVar.GetResource();
            }
            set
            {
                if (currentShadowMapSpot != value)
                {
                    shadowMapSpotVar.SetResource(value);

                    currentShadowMapSpot = value;

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
            PositionColor = Effect.GetTechniqueByName("PositionColor");
            PositionColorSkinned = Effect.GetTechniqueByName("PositionColorSkinned");
            PositionNormalColor = Effect.GetTechniqueByName("PositionNormalColor");
            PositionNormalColorSkinned = Effect.GetTechniqueByName("PositionNormalColorSkinned");
            PositionTexture = Effect.GetTechniqueByName("PositionTexture");
            PositionTextureNOALPHA = Effect.GetTechniqueByName("PositionTextureNOALPHA");
            PositionTextureRED = Effect.GetTechniqueByName("PositionTextureRED");
            PositionTextureGREEN = Effect.GetTechniqueByName("PositionTextureGREEN");
            PositionTextureBLUE = Effect.GetTechniqueByName("PositionTextureBLUE");
            PositionTextureALPHA = Effect.GetTechniqueByName("PositionTextureALPHA");
            PositionTextureSkinned = Effect.GetTechniqueByName("PositionTextureSkinned");
            PositionNormalTexture = Effect.GetTechniqueByName("PositionNormalTexture");
            PositionNormalTextureSkinned = Effect.GetTechniqueByName("PositionNormalTextureSkinned");
            PositionNormalTextureTangent = Effect.GetTechniqueByName("PositionNormalTextureTangent");
            PositionNormalTextureTangentSkinned = Effect.GetTechniqueByName("PositionNormalTextureTangentSkinned");
            InstancingPositionColor = Effect.GetTechniqueByName("PositionColorI");
            InstancingPositionColorSkinned = Effect.GetTechniqueByName("PositionColorSkinnedI");
            InstancingPositionNormalColor = Effect.GetTechniqueByName("PositionNormalColorI");
            InstancingPositionNormalColorSkinned = Effect.GetTechniqueByName("PositionNormalColorSkinnedI");
            InstancingPositionTexture = Effect.GetTechniqueByName("PositionTextureI");
            InstancingPositionTextureNOALPHA = Effect.GetTechniqueByName("PositionTextureNOALPHAI");
            InstancingPositionTextureRED = Effect.GetTechniqueByName("PositionTextureREDI");
            InstancingPositionTextureGREEN = Effect.GetTechniqueByName("PositionTextureGREENI");
            InstancingPositionTextureBLUE = Effect.GetTechniqueByName("PositionTextureBLUEI");
            InstancingPositionTextureALPHA = Effect.GetTechniqueByName("PositionTextureALPHAI");
            InstancingPositionTextureSkinned = Effect.GetTechniqueByName("PositionTextureSkinnedI");
            InstancingPositionNormalTexture = Effect.GetTechniqueByName("PositionNormalTextureI");
            InstancingPositionNormalTextureSkinned = Effect.GetTechniqueByName("PositionNormalTextureSkinnedI");
            InstancingPositionNormalTextureTangent = Effect.GetTechniqueByName("PositionNormalTextureTangentI");
            InstancingPositionNormalTextureTangentSkinned = Effect.GetTechniqueByName("PositionNormalTextureTangentSkinnedI");

            //Globals
            animationPaletteWidthVar = Effect.GetVariableScalar("gAnimationPaletteWidth");
            animationPaletteVar = Effect.GetVariableTexture("gAnimationPalette");
            materialPaletteWidthVar = Effect.GetVariableScalar("gMaterialPaletteWidth");
            materialPaletteVar = Effect.GetVariableTexture("gMaterialPalette");
            lodVar = Effect.GetVariableVector("gLOD");

            //Per frame
            worldVar = Effect.GetVariableMatrix("gVSWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gVSWorldViewProjection");
            eyePositionWorldVar = Effect.GetVariableVector("gPSEyePositionWorld");
            hemiLightVar = Effect.GetVariable("gPSHemiLight");
            dirLightsVar = Effect.GetVariable("gPSDirLights");
            pointLightsVar = Effect.GetVariable("gPSPointLights");
            spotLightsVar = Effect.GetVariable("gPSSpotLights");
            lightCountVar = Effect.GetVariableVector("gPSLightCount");
            fogStartVar = Effect.GetVariableScalar("gPSFogStart");
            fogRangeVar = Effect.GetVariableScalar("gPSFogRange");
            fogColorVar = Effect.GetVariableVector("gPSFogColor");
            shadowMapDirectionalVar = Effect.GetVariableTexture("gPSShadowMapDir");
            shadowMapPointVar = Effect.GetVariableTexture("gPSShadowMapPoint");
            shadowMapSpotVar = Effect.GetVariableTexture("gPSShadowMapSpot");

            //Per object
            useColorDiffuseVar = Effect.GetVariableScalar("gPSUseColorDiffuse");
            diffuseMapVar = Effect.GetVariableTexture("gPSDiffuseMapArray");
            normalMapVar = Effect.GetVariableTexture("gPSNormalMapArray");

            //Per instance
            tintColorVar = Effect.GetVariableVector("gVSTintColor");
            animationOffsetVar = Effect.GetVariableScalar("gVSAnimationOffset");
            animationOffset2Var = Effect.GetVariableScalar("gVSAnimationOffset2");
            animationInterpolationVar = Effect.GetVariableScalar("gVSAnimationInterpolation");
            materialIndexVar = Effect.GetVariableScalar("gPSMaterialIndex");
            textureIndexVar = Effect.GetVariableScalar("gPSTextureIndex");

            //Samplers
            samplerDiffuseVar = Effect.GetVariableSampler("SamplerDiffuse");
            samplerSpecularVar = Effect.GetVariableSampler("SamplerSpecular");
            samplerNormalVar = Effect.GetVariableSampler("SamplerNormal");

            //Initialize states
            samplerPoint = EngineSamplerState.Point(graphics);
            samplerLinear = EngineSamplerState.Linear(graphics);
            samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, 4);
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
                samplerPoint?.Dispose();
                samplerPoint = null;

                samplerLinear?.Dispose();
                samplerLinear = null;

                samplerAnisotropic?.Dispose();
                samplerAnisotropic = null;
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
                        return PositionColor;
                    case VertexTypes.PositionTexture:
                        return PositionTexture;
                    case VertexTypes.PositionNormalColor:
                        return PositionNormalColor;
                    case VertexTypes.PositionNormalTexture:
                        return PositionNormalTexture;
                    case VertexTypes.PositionNormalTextureTangent:
                        return PositionNormalTextureTangent;
                    case VertexTypes.PositionColorSkinned:
                        return PositionColorSkinned;
                    case VertexTypes.PositionTextureSkinned:
                        return PositionTextureSkinned;
                    case VertexTypes.PositionNormalColorSkinned:
                        return PositionNormalColorSkinned;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return PositionNormalTextureSkinned;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return PositionNormalTextureTangentSkinned;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect: {0}", vertexType));
                }
            }
            else
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return InstancingPositionColor;
                    case VertexTypes.PositionTexture:
                        return InstancingPositionTexture;
                    case VertexTypes.PositionNormalColor:
                        return InstancingPositionNormalColor;
                    case VertexTypes.PositionNormalTexture:
                        return InstancingPositionNormalTexture;
                    case VertexTypes.PositionNormalTextureTangent:
                        return InstancingPositionNormalTextureTangent;
                    case VertexTypes.PositionColorSkinned:
                        return InstancingPositionColorSkinned;
                    case VertexTypes.PositionTextureSkinned:
                        return InstancingPositionTextureSkinned;
                    case VertexTypes.PositionNormalColorSkinned:
                        return InstancingPositionNormalColorSkinned;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return InstancingPositionNormalTextureSkinned;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return InstancingPositionNormalTextureTangentSkinned;
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
            MaterialPalette = materialPalette;
            MaterialPaletteWidth = materialPaletteWidth;

            AnimationPalette = animationPalette;
            AnimationPaletteWidth = animationPaletteWidth;

            LOD = new Vector3(lod1, lod2, lod3);
        }
        /// <inheritdoc/>
        public void UpdatePerFrameBasic(
            Matrix world,
            DrawContext context)
        {
            UpdatePerFrame(
                world,
                context.ViewProjection,
                Vector3.Zero,
                null,
                null,
                null,
                null);
        }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public void UpdatePerObject()
        {
            UpdatePerObject(AnimationDrawInfo.Empty, MaterialDrawInfo.Empty, 0, Color4.White);
        }
        /// <inheritdoc/>
        public void UpdatePerObject(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex,
            Color4 tintColor)
        {
            AnimationOffset = animation.Offset1;
            AnimationOffset2 = animation.Offset2;
            AnimationInterpolation = animation.InterpolationAmount;

            DiffuseMap = material.Material?.DiffuseTexture;
            NormalMap = material.Material?.NormalMap;
            UseColorDiffuse = material.Material?.DiffuseTexture != null;
            MaterialIndex = material.Material?.ResourceIndex ?? 0;
            Anisotropic = material.UseAnisotropic;

            TextureIndex = textureIndex;
            TintColor = tintColor;
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
            World = world;
            WorldViewProjection = world * viewProjection;

            if (lights != null)
            {
                EyePositionWorld = eyePositionWorld;

                HemiLight = BufferLightHemispheric.Build(lights.GetVisibleHemisphericLight());
                DirLights = BufferLightDirectional.Build(lights.GetVisibleDirectionalLights(), out int dirLength);
                PointLights = BufferLightPoint.Build(lights.GetVisiblePointLights(), out int pointLength);
                SpotLights = BufferLightSpot.Build(lights.GetVisibleSpotLights(), out int spotLength);
                LightCount = new[] { dirLength, pointLength, spotLength };

                FogStart = lights.FogStart;
                FogRange = lights.FogRange;
                FogColor = lights.FogColor;

                ShadowMapDirectional = shadowMapDirectional?.Texture;
                ShadowMapPoint = shadowMapPoint?.Texture;
                ShadowMapSpot = shadowMapSpot?.Texture;
            }
            else
            {
                EyePositionWorld = Vector3.Zero;

                HemiLight = BufferLightHemispheric.Default;
                DirLights = BufferLightDirectional.Default;
                PointLights = BufferLightPoint.Default;
                SpotLights = BufferLightSpot.Default;
                LightCount = new[] { 0, 0, 0 };

                FogStart = 0;
                FogRange = 0;
                FogColor = Color.Transparent;

                ShadowMapDirectional = null;
                ShadowMapPoint = null;
                ShadowMapSpot = null;
            }
        }
    }
}
