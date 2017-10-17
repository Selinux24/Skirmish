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
        private EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Texture resolution effect variable
        /// </summary>
        private EngineEffectVariableScalar textureResolution = null;
        /// <summary>
        /// Low resolution textures effect variable
        /// </summary>
        private EngineEffectVariableTexture diffuseMapLR = null;
        /// <summary>
        /// High resolution textures effect variable
        /// </summary>
        private EngineEffectVariableTexture diffuseMapHR = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EngineEffectVariableTexture normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private EngineEffectVariableTexture specularMap = null;
        /// <summary>
        /// Color texture array effect variable
        /// </summary>
        private EngineEffectVariableTexture colorTextures = null;
        /// <summary>
        /// Alpha map effect variable
        /// </summary>
        private EngineEffectVariableTexture alphaMap = null;
        /// <summary>
        /// Slope ranges effect variable
        /// </summary>
        private EngineEffectVariableVector parameters = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private EngineEffectVariableScalar materialIndex = null;

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
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EngineEffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            EngineEffectTechnique technique = null;

            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Terrain)
                {
                    if (mode == DrawerModesEnum.Deferred) technique = this.TerrainFullDeferred;
                }
            }

            if (technique == null)
            {
                throw new EngineException(string.Format("Bad vertex type for effect, stage and mode: {0} - {1} - {2}", vertexType, stage, mode));
            }

            return technique;
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="textureResolution">Texture resolution</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float textureResolution)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.TextureResolution = textureResolution;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="materialIndex">Material index</param>
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
            uint materialIndex,
            EngineShaderResourceView normalMap,
            EngineShaderResourceView specularMap,
            bool useAlphaMap,
            EngineShaderResourceView alphaMap,
            EngineShaderResourceView colorTextures,
            bool useSlopes,
            Vector2 slopeRanges,
            EngineShaderResourceView diffuseMapLR,
            EngineShaderResourceView diffuseMapHR,
            float proportion)
        {
            this.MaterialIndex = materialIndex;
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;

            this.AlphaMap = alphaMap;
            this.ColorTextures = colorTextures;

            this.DiffuseMapLR = diffuseMapLR;
            this.DiffuseMapHR = diffuseMapHR;

            this.Parameters = new Vector4(0, proportion, slopeRanges.X, slopeRanges.Y);
        }
    }
}
