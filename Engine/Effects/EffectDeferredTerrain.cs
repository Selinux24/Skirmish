using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDeferredTerrain : Drawer
    {
        /// <summary>
        /// Deferred with alpha map drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainAlphaMapDeferred = null;
        /// <summary>
        /// Deferred with slopes drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainSlopesDeferred = null;
        /// <summary>
        /// Deferred full drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainFullDeferred = null;

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
        /// Material index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialIndexVar = null;
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
        /// Current low resolution diffuse map
        /// </summary>
        private EngineShaderResourceView currentDiffuseMapLR = null;
        /// <summary>
        /// Current hihg resolution diffuse map
        /// </summary>
        private EngineShaderResourceView currentDiffuseMapHR = null;
        /// <summary>
        /// Current normal map
        /// </summary>
        private EngineShaderResourceView currentNormalMap = null;
        /// <summary>
        /// Current color texture array
        /// </summary>
        private EngineShaderResourceView currentColorTextures = null;
        /// <summary>
        /// Current alpha map
        /// </summary>
        private EngineShaderResourceView currentAlphaMap = null;
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
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDeferredTerrain(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            TerrainAlphaMapDeferred = Effect.GetTechniqueByName("TerrainAlphaMapDeferred");
            TerrainSlopesDeferred = Effect.GetTechniqueByName("TerrainSlopesDeferred");
            TerrainFullDeferred = Effect.GetTechniqueByName("TerrainFullDeferred");

            worldVar = Effect.GetVariableMatrix("gVSWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gVSWorldViewProjection");
            textureResolutionVar = Effect.GetVariableScalar("gVSTextureResolution");

            diffuseMapLRVar = Effect.GetVariableTexture("gPSDiffuseMapLRArray");
            diffuseMapHRVar = Effect.GetVariableTexture("gPSDiffuseMapHRArray");
            normalMapVar = Effect.GetVariableTexture("gPSNormalMapArray");
            colorTexturesVar = Effect.GetVariableTexture("gPSColorTextureArray");
            alphaMapVar = Effect.GetVariableTexture("gPSAlphaTexture");
            parametersVar = Effect.GetVariableVector("gPSParams");
            materialIndexVar = Effect.GetVariableScalar("gPSMaterialIndex");

            //Samplers
            samplerDiffuseVar = Effect.GetVariableSampler("SamplerDiffuse");
            samplerSpecularVar = Effect.GetVariableSampler("SamplerSpecular");
            samplerNormalVar = Effect.GetVariableSampler("SamplerNormal");

            //Initialize states
            samplerPoint = EngineSamplerState.Point(graphics, nameof(EffectDeferredTerrain));
            samplerLinear = EngineSamplerState.Linear(graphics, nameof(EffectDeferredTerrain));
            samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, nameof(EffectDeferredTerrain), 8);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EffectDeferredTerrain()
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
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="textureResolution">Texture resolution</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            float textureResolution)
        {
            World = Matrix.Identity;
            WorldViewProjection = viewProjection;
            TextureResolution = textureResolution;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="materialIndex">Material index</param>
        /// <param name="useAnisotropic">Use anisotropic filtering</param>
        /// <param name="normalMap">Normal map</param>
        /// <param name="specularMap">Specular map</param>
        /// <param name="useAlphaMap">Use alpha mapping</param>
        /// <param name="alphaMap">Alpha map</param>
        /// <param name="colorTextures">Color textures</param>
        /// <param name="useSlopes">Use slope texturing</param>
        /// <param name="diffuseMapLR">Low resolution textures</param>
        /// <param name="diffuseMapHR">High resolution textures</param>
        /// <param name="slopeRanges">Slope ranges</param>
        /// <param name="proportion">Lerping proportion</param>
        public void UpdatePerObject(
            EffectTerrainState state)
        {
            MaterialIndex = state.MaterialIndex;

            Anisotropic = state.UseAnisotropic;
            NormalMap = state.NormalMap;

            AlphaMap = state.AlphaMap;
            ColorTextures = state.ColorTextures;

            DiffuseMapLR = state.DiffuseMapLR;
            DiffuseMapHR = state.DiffuseMapHR;

            Parameters = new Vector4(0, state.Proportion, state.SlopeRanges.X, state.SlopeRanges.Y);
        }
    }
}
