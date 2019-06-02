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
        /// Current specular map
        /// </summary>
        private EngineShaderResourceView currentSpecularMap = null;
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
        /// Texture resolution
        /// </summary>
        protected float TextureResolution
        {
            get
            {
                return this.textureResolutionVar.GetFloat();
            }
            set
            {
                this.textureResolutionVar.Set(value);
            }
        }
        /// <summary>
        /// Low resolution textures
        /// </summary>
        protected EngineShaderResourceView DiffuseMapLR
        {
            get
            {
                return this.diffuseMapLRVar.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapLR != value)
                {
                    this.diffuseMapLRVar.SetResource(value);

                    this.currentDiffuseMapLR = value;

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
                return this.diffuseMapHRVar.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapHR != value)
                {
                    this.diffuseMapHRVar.SetResource(value);

                    this.currentDiffuseMapHR = value;

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
        /// Scpecular map
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
        /// Color textures for alpha map
        /// </summary>
        protected EngineShaderResourceView ColorTextures
        {
            get
            {
                return this.colorTexturesVar.GetResource();
            }
            set
            {
                if (this.currentColorTextures != value)
                {
                    this.colorTexturesVar.SetResource(value);

                    this.currentColorTextures = value;

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
                return this.alphaMapVar.GetResource();
            }
            set
            {
                if (this.currentAlphaMap != value)
                {
                    this.alphaMapVar.SetResource(value);

                    this.currentAlphaMap = value;

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
                return this.parametersVar.GetVector<Vector4>();
            }
            set
            {
                this.parametersVar.Set(value);
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
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDeferredTerrain(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.TerrainAlphaMapDeferred = this.Effect.GetTechniqueByName("TerrainAlphaMapDeferred");
            this.TerrainSlopesDeferred = this.Effect.GetTechniqueByName("TerrainSlopesDeferred");
            this.TerrainFullDeferred = this.Effect.GetTechniqueByName("TerrainFullDeferred");

            this.worldVar = this.Effect.GetVariableMatrix("gVSWorld");
            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gVSWorldViewProjection");
            this.textureResolutionVar = this.Effect.GetVariableScalar("gVSTextureResolution");

            this.diffuseMapLRVar = this.Effect.GetVariableTexture("gPSDiffuseMapLRArray");
            this.diffuseMapHRVar = this.Effect.GetVariableTexture("gPSDiffuseMapHRArray");
            this.normalMapVar = this.Effect.GetVariableTexture("gPSNormalMapArray");
            this.specularMapVar = this.Effect.GetVariableTexture("gPSSpecularMapArray");
            this.colorTexturesVar = this.Effect.GetVariableTexture("gPSColorTextureArray");
            this.alphaMapVar = this.Effect.GetVariableTexture("gPSAlphaTexture");
            this.parametersVar = this.Effect.GetVariableVector("gPSParams");
            this.materialIndexVar = this.Effect.GetVariableScalar("gPSMaterialIndex");

            //Samplers
            this.samplerDiffuseVar = this.Effect.GetVariableSampler("SamplerDiffuse");
            this.samplerSpecularVar = this.Effect.GetVariableSampler("SamplerSpecular");
            this.samplerNormalVar = this.Effect.GetVariableSampler("SamplerNormal");

            //Initialize states
            this.samplerPoint = EngineSamplerState.Point(graphics);
            this.samplerLinear = EngineSamplerState.Linear(graphics);
            this.samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, 8);
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
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="textureResolution">Texture resolution</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            float textureResolution)
        {
            this.World = Matrix.Identity;
            this.WorldViewProjection = viewProjection;
            this.TextureResolution = textureResolution;
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
            this.MaterialIndex = state.MaterialIndex;

            this.Anisotropic = state.UseAnisotropic;
            this.NormalMap = state.NormalMap;
            this.SpecularMap = state.SpecularMap;

            this.AlphaMap = state.AlphaMap;
            this.ColorTextures = state.ColorTextures;

            this.DiffuseMapLR = state.DiffuseMapLR;
            this.DiffuseMapHR = state.DiffuseMapHR;

            this.Parameters = new Vector4(0, state.Proportion, state.SlopeRanges.X, state.SlopeRanges.Y);
        }
    }
}
