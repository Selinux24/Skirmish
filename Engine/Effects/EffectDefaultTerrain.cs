using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDefaultTerrain : Drawer
    {
        /// <summary>
        /// Forward with alpha map drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainAlphaMapForward = null;
        /// <summary>
        /// Forward with slopes drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainSlopesForward = null;
        /// <summary>
        /// Forward full drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainFullForward = null;

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
        /// Texture resolution effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureResolutionVar = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorDiffuseVar = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorSpecularVar = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialIndexVar = null;
        /// <summary>
        /// Low resolution textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapLRVar = null;
        /// <summary>
        /// High resolution textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapHRVar = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture normalMapVar = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture specularMapVar = null;
        /// <summary>
        /// Color texture array effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture colorTexturesVar = null;
        /// <summary>
        /// Alpha map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture alphaMapVar = null;
        /// <summary>
        /// Slope ranges effect variable
        /// </summary>
        private readonly EngineEffectVariableVector parametersVar = null;
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
        /// Albedo effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar albedoVar = null;

        /// <summary>
        /// Current diffuse map (Low resolution)
        /// </summary>
        private EngineShaderResourceView currentDiffuseMapLR = null;
        /// <summary>
        /// Current normal map (High resolution)
        /// </summary>
        private EngineShaderResourceView currentDiffuseMapHR = null;
        /// <summary>
        /// Current normal map
        /// </summary>
        private EngineShaderResourceView currentNormalMap = null;
        /// <summary>
        /// Current specular map
        /// </summary>
        private EngineShaderResourceView currentSpecularMap = null;
        /// <summary>
        /// Current color texure array
        /// </summary>
        private EngineShaderResourceView currentColorTextures = null;
        /// <summary>
        /// Current alpha map
        /// </summary>
        private EngineShaderResourceView currentAlphaMap = null;
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
        /// Texture resolution
        /// </summary>
        protected float TextureResolution
        {
            get
            {
                return textureResolutionVar.GetFloat();
            }
            set
            {
                textureResolutionVar.Set(value);
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
        /// Use specular map color
        /// </summary>
        protected bool UseColorSpecular
        {
            get
            {
                return useColorSpecularVar.GetBool();
            }
            set
            {
                useColorSpecularVar.Set(value);
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
        /// Low resolution textures
        /// </summary>
        protected EngineShaderResourceView DiffuseMapLR
        {
            get
            {
                return diffuseMapLRVar.GetResource();
            }
            set
            {
                if (currentDiffuseMapLR != value)
                {
                    diffuseMapLRVar.SetResource(value);

                    currentDiffuseMapLR = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// High resolution textures
        /// </summary>
        protected EngineShaderResourceView DiffuseMapHR
        {
            get
            {
                return diffuseMapHRVar.GetResource();
            }
            set
            {
                if (currentDiffuseMapHR != value)
                {
                    diffuseMapHRVar.SetResource(value);

                    currentDiffuseMapHR = value;

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
        /// Scpecular map
        /// </summary>
        protected EngineShaderResourceView SpecularMap
        {
            get
            {
                return specularMapVar.GetResource();
            }
            set
            {
                if (currentSpecularMap != value)
                {
                    specularMapVar.SetResource(value);

                    currentSpecularMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Color textures for alpha map
        /// </summary>
        protected EngineShaderResourceView ColorTextures
        {
            get
            {
                return colorTexturesVar.GetResource();
            }
            set
            {
                if (currentColorTextures != value)
                {
                    colorTexturesVar.SetResource(value);

                    currentColorTextures = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Alpha map
        /// </summary>
        protected EngineShaderResourceView AlphaMap
        {
            get
            {
                return alphaMapVar.GetResource();
            }
            set
            {
                if (currentAlphaMap != value)
                {
                    alphaMapVar.SetResource(value);

                    currentAlphaMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Slope ranges
        /// </summary>
        protected Vector4 Parameters
        {
            get
            {
                return parametersVar.GetVector<Vector4>();
            }
            set
            {
                parametersVar.Set(value);
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
        /// Albedo
        /// </summary>
        protected float Albedo
        {
            get
            {
                return albedoVar.GetFloat();
            }
            set
            {
                albedoVar.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultTerrain(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            TerrainAlphaMapForward = Effect.GetTechniqueByName("TerrainAlphaMapForward");
            TerrainSlopesForward = Effect.GetTechniqueByName("TerrainSlopesForward");
            TerrainFullForward = Effect.GetTechniqueByName("TerrainFullForward");

            //Globals
            materialPaletteWidthVar = Effect.GetVariableScalar("gMaterialPaletteWidth");
            materialPaletteVar = Effect.GetVariableTexture("gMaterialPalette");
            lodVar = Effect.GetVariableVector("gLOD");

            //Per frame
            worldVar = Effect.GetVariableMatrix("gVSWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gVSWorldViewProjection");
            textureResolutionVar = Effect.GetVariableScalar("gVSTextureResolution");

            eyePositionWorldVar = Effect.GetVariableVector("gPSEyePositionWorld");
            lightCountVar = Effect.GetVariableVector("gPSLightCount");
            fogColorVar = Effect.GetVariableVector("gPSFogColor");
            fogStartVar = Effect.GetVariableScalar("gPSFogStart");
            fogRangeVar = Effect.GetVariableScalar("gPSFogRange");
            hemiLightVar = Effect.GetVariable("gPSHemiLight");
            dirLightsVar = Effect.GetVariable("gPSDirLights");
            pointLightsVar = Effect.GetVariable("gPSPointLights");
            spotLightsVar = Effect.GetVariable("gPSSpotLights");
            shadowMapDirectionalVar = Effect.GetVariableTexture("gPSShadowMapDir");
            shadowMapPointVar = Effect.GetVariableTexture("gPSShadowMapPoint");
            shadowMapSpotVar = Effect.GetVariableTexture("gPSShadowMapSpot");
            albedoVar = Effect.GetVariableScalar("gPSAlbedo");

            //Per object
            parametersVar = Effect.GetVariableVector("gPSParams");
            useColorDiffuseVar = Effect.GetVariableScalar("gPSUseColorDiffuse");
            useColorSpecularVar = Effect.GetVariableScalar("gPSUseColorSpecular");
            materialIndexVar = Effect.GetVariableScalar("gPSMaterialIndex");
            normalMapVar = Effect.GetVariableTexture("gPSNormalMapArray");
            specularMapVar = Effect.GetVariableTexture("gPSSpecularMapArray");
            colorTexturesVar = Effect.GetVariableTexture("gPSColorTextureArray");
            alphaMapVar = Effect.GetVariableTexture("gPSAlphaTexture");
            diffuseMapLRVar = Effect.GetVariableTexture("gPSDiffuseMapLRArray");
            diffuseMapHRVar = Effect.GetVariableTexture("gPSDiffuseMapHRArray");

            //Samplers
            samplerDiffuseVar = Effect.GetVariableSampler("SamplerDiffuse");
            samplerSpecularVar = Effect.GetVariableSampler("SamplerSpecular");
            samplerNormalVar = Effect.GetVariableSampler("SamplerNormal");

            //Initialize states
            samplerPoint = EngineSamplerState.Point(graphics);
            samplerLinear = EngineSamplerState.Linear(graphics);
            samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, 8);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EffectDefaultTerrain()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose
        /// </summary>
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
        /// Update effect globals
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        /// <param name="lod1">High level of detail maximum distance</param>
        /// <param name="lod2">Medium level of detail maximum distance</param>
        /// <param name="lod3">Low level of detail maximum distance</param>
        public void UpdateGlobals(
            EngineShaderResourceView materialPalette,
            uint materialPaletteWidth,
            float lod1,
            float lod2,
            float lod3)
        {
            MaterialPalette = materialPalette;
            MaterialPaletteWidth = materialPaletteWidth;

            LOD = new Vector3(lod1, lod2, lod3);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="textureResolution">Texture resolution</param>
        /// <param name="context">Drawing context</param>
        public void UpdatePerFrame(
            float textureResolution,
            DrawContext context)
        {
            World = Matrix.Identity;
            WorldViewProjection = context.ViewProjection;
            TextureResolution = textureResolution;

            var lights = context.Lights;
            if (lights != null)
            {
                EyePositionWorld = context.EyePosition;

                HemiLight = BufferLightHemispheric.Build(lights.GetVisibleHemisphericLight());
                DirLights = BufferLightDirectional.Build(lights.GetVisibleDirectionalLights(), out int dirLength);
                PointLights = BufferLightPoint.Build(lights.GetVisiblePointLights(), out int pointLength);
                SpotLights = BufferLightSpot.Build(lights.GetVisibleSpotLights(), out int spotLength);
                LightCount = new[] { dirLength, pointLength, spotLength };
                Albedo = lights.Albedo;

                FogStart = lights.FogStart;
                FogRange = lights.FogRange;
                FogColor = lights.FogColor;

                ShadowMapDirectional = context.ShadowMapDirectional?.Texture;
                ShadowMapPoint = context.ShadowMapPoint?.Texture;
                ShadowMapSpot = context.ShadowMapSpot?.Texture;
            }
            else
            {
                EyePositionWorld = Vector3.Zero;

                HemiLight = BufferLightHemispheric.Default;
                DirLights = BufferLightDirectional.Default;
                PointLights = BufferLightPoint.Default;
                SpotLights = BufferLightSpot.Default;
                LightCount = new[] { 0, 0, 0 };
                Albedo = 1;

                FogStart = 0;
                FogRange = 0;
                FogColor = Color.Transparent;

                ShadowMapDirectional = null;
                ShadowMapPoint = null;
                ShadowMapSpot = null;
            }
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="state">State</param>
        public void UpdatePerObject(
            EffectTerrainState state)
        {
            Anisotropic = state.UseAnisotropic;

            NormalMap = state.NormalMap;
            SpecularMap = state.SpecularMap;
            UseColorSpecular = state.SpecularMap != null;

            AlphaMap = state.AlphaMap;
            ColorTextures = state.ColorTextures;
            UseColorDiffuse = ColorTextures != null;

            DiffuseMapLR = state.DiffuseMapLR;
            DiffuseMapHR = state.DiffuseMapHR;

            Parameters = new Vector4(0, state.Proportion, state.SlopeRanges.X, state.SlopeRanges.Y);

            MaterialIndex = state.MaterialIndex;
        }
    }
}
