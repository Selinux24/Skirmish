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
        private readonly EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Texture resolution effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureResolution = null;
        /// <summary>
        /// Low resolution textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapLR = null;
        /// <summary>
        /// High resolution textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapHR = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture specularMap = null;
        /// <summary>
        /// Color texture array effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture colorTextures = null;
        /// <summary>
        /// Alpha map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture alphaMap = null;
        /// <summary>
        /// Slope ranges effect variable
        /// </summary>
        private readonly EngineEffectVariableVector parameters = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialIndex = null;
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
        /// Texture resolution
        /// </summary>
        protected float TextureResolution
        {
            get
            {
                return this.textureResolution.GetFloat();
            }
            set
            {
                this.textureResolution.Set(value);
            }
        }
        /// <summary>
        /// Low resolution textures
        /// </summary>
        protected EngineShaderResourceView DiffuseMapLR
        {
            get
            {
                return this.diffuseMapLR.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapLR != value)
                {
                    this.diffuseMapLR.SetResource(value);

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
                return this.diffuseMapHR.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapHR != value)
                {
                    this.diffuseMapHR.SetResource(value);

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
        /// Scpecular map
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
        /// Color textures for alpha map
        /// </summary>
        protected EngineShaderResourceView ColorTextures
        {
            get
            {
                return this.colorTextures.GetResource();
            }
            set
            {
                if (this.currentColorTextures != value)
                {
                    this.colorTextures.SetResource(value);

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
                return this.alphaMap.GetResource();
            }
            set
            {
                if (this.currentAlphaMap != value)
                {
                    this.alphaMap.SetResource(value);

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
                return this.parameters.GetVector<Vector4>();
            }
            set
            {
                this.parameters.Set(value);
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
        public EffectDeferredTerrain(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.TerrainAlphaMapDeferred = this.Effect.GetTechniqueByName("TerrainAlphaMapDeferred");
            this.TerrainSlopesDeferred = this.Effect.GetTechniqueByName("TerrainSlopesDeferred");
            this.TerrainFullDeferred = this.Effect.GetTechniqueByName("TerrainFullDeferred");

            this.world = this.Effect.GetVariableMatrix("gVSWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gVSWorldViewProjection");
            this.textureResolution = this.Effect.GetVariableScalar("gVSTextureResolution");

            this.diffuseMapLR = this.Effect.GetVariableTexture("gPSDiffuseMapLRArray");
            this.diffuseMapHR = this.Effect.GetVariableTexture("gPSDiffuseMapHRArray");
            this.normalMap = this.Effect.GetVariableTexture("gPSNormalMapArray");
            this.specularMap = this.Effect.GetVariableTexture("gPSSpecularMapArray");
            this.colorTextures = this.Effect.GetVariableTexture("gPSColorTextureArray");
            this.alphaMap = this.Effect.GetVariableTexture("gPSAlphaTexture");
            this.parameters = this.Effect.GetVariableVector("gPSParams");
            this.materialIndex = this.Effect.GetVariableScalar("gPSMaterialIndex");

            //Samplers
            this.samplerDiffuse = this.Effect.GetVariableSampler("SamplerDiffuse");
            this.samplerSpecular = this.Effect.GetVariableSampler("SamplerSpecular");
            this.samplerNormal = this.Effect.GetVariableSampler("SamplerNormal");

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
