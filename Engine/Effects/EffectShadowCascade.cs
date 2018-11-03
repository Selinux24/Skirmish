using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Shadows generation effect
    /// </summary>
    public class EffectShadowCascade : Drawer, IShadowMapDrawer
    {
        #region Technique variables

        /// <summary>
        /// Spot shadows
        /// </summary>
        protected readonly EngineEffectTechnique SpotShadowGen = null;
        /// <summary>
        /// Point shadows
        /// </summary>
        protected readonly EngineEffectTechnique PointShadowGen = null;
        /// <summary>
        /// Cascaded shadows
        /// </summary>
        protected readonly EngineEffectTechnique CascadedShadowMapsGen = null;
        protected readonly EngineEffectTechnique CascadedShadowMapsGenI = null;
        protected readonly EngineEffectTechnique CascadedShadowMapsGenSkinned = null;
        protected readonly EngineEffectTechnique CascadedShadowMapsGenSkinnedI = null;

        #endregion

        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationPaletteWidth = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private readonly EngineEffectVariableTexture animationPalette = null;
        /// <summary>
        /// Cascade view*projection matrix array effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationOffset = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMap = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureIndex = null;

        /// <summary>
        /// Current animation palette
        /// </summary>
        private EngineShaderResourceView currentAnimationPalette = null;
        /// <summary>
        /// Current diffuse map
        /// </summary>
        private EngineShaderResourceView currentDiffuseMap = null;

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
        /// Cascade view*projection matrix array
        /// </summary>
        protected Matrix[] WorldViewProjection
        {
            get
            {
                return this.worldViewProjection.GetMatrixArray(3);
            }
            set
            {
                if (value == null)
                {
                    this.worldViewProjection.SetMatrix(new Matrix[3]);
                }
                else
                {
                    this.worldViewProjection.SetMatrix(value);
                }
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
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadowCascade(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.CascadedShadowMapsGen = this.Effect.GetTechniqueByName("CascadedShadowMapsGen");
            this.CascadedShadowMapsGenI = this.Effect.GetTechniqueByName("CascadedShadowMapsGenI");
            this.CascadedShadowMapsGenSkinned = this.Effect.GetTechniqueByName("CascadedShadowMapsGenSkinned");
            this.CascadedShadowMapsGenSkinnedI = this.Effect.GetTechniqueByName("CascadedShadowMapsGenSkinnedI");

            this.animationPaletteWidth = this.Effect.GetVariableScalar("gAnimationPaletteWidth");
            this.animationPalette = this.Effect.GetVariableTexture("gAnimationPalette");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gGSWorldViewProjection");
            this.animationOffset = this.Effect.GetVariableScalar("gVSAnimationOffset");
            this.diffuseMap = this.Effect.GetVariableTexture("gPSDiffuseMapArray");
            this.textureIndex = this.Effect.GetVariableScalar("gPSTextureIndex");
        }

        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="transparent">Use transparent textures</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        public EngineEffectTechnique GetTechnique(
            VertexTypes vertexType,
            bool instanced,
            bool transparent)
        {
            if (!instanced)
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return this.CascadedShadowMapsGen;
                    case VertexTypes.PositionColorSkinned:
                        return this.CascadedShadowMapsGenSkinned;

                    case VertexTypes.PositionTexture:
                        return this.CascadedShadowMapsGen;
                    case VertexTypes.PositionTextureSkinned:
                        return this.CascadedShadowMapsGenSkinned;

                    case VertexTypes.PositionNormalColor:
                        return this.CascadedShadowMapsGen;
                    case VertexTypes.PositionNormalColorSkinned:
                        return this.CascadedShadowMapsGenSkinned;

                    case VertexTypes.PositionNormalTexture:
                        return this.CascadedShadowMapsGen;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return this.CascadedShadowMapsGenSkinned;

                    case VertexTypes.PositionNormalTextureTangent:
                        return this.CascadedShadowMapsGen;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return this.CascadedShadowMapsGenSkinned;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Transparent: {2}", vertexType, instanced, transparent));
                }
            }
            else
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return this.CascadedShadowMapsGenI;
                    case VertexTypes.PositionColorSkinned:
                        return this.CascadedShadowMapsGenSkinnedI;

                    case VertexTypes.PositionTexture:
                        return this.CascadedShadowMapsGenI;
                    case VertexTypes.PositionTextureSkinned:
                        return this.CascadedShadowMapsGenSkinnedI;

                    case VertexTypes.PositionNormalColor:
                        return this.CascadedShadowMapsGenI;
                    case VertexTypes.PositionNormalColorSkinned:
                        return this.CascadedShadowMapsGenSkinnedI;

                    case VertexTypes.PositionNormalTexture:
                        return this.CascadedShadowMapsGenI;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return this.CascadedShadowMapsGenSkinnedI;

                    case VertexTypes.PositionNormalTextureTangent:
                        return this.CascadedShadowMapsGenI;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return this.CascadedShadowMapsGenSkinnedI;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Transparent: {2}", vertexType, instanced, transparent));
                }
            }
        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWith">Animation palette texture width</param>
        public void UpdateGlobals(
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {
            this.AnimationPalette = animationPalette;
            this.AnimationPaletteWidth = animationPaletteWidth;
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="context">Context</param>
        public void UpdatePerFrame(
            Matrix world,
            DrawContextShadows context)
        {
            var viewProjection = context.ShadowMap.FromLightViewProjectionArray;

            if (viewProjection != null && viewProjection.Length > 0)
            {
                if (viewProjection.Length != 3)
                {
                    throw new EngineException("The matrix array must have a length of 3");
                }

                var m = new Matrix[viewProjection.Length];
                for (int i = 0; i < viewProjection.Length; i++)
                {
                    m[i] = world * viewProjection[i];
                }

                this.WorldViewProjection = m;
            }
            else
            {
                this.WorldViewProjection = null;
            }
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animationOffset">Animation index</param>
        /// <param name="material">Material</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObject(
            uint animationOffset,
            MeshMaterial material,
            uint textureIndex)
        {
            this.AnimationOffset = animationOffset;
            this.DiffuseMap = material?.DiffuseTexture;
            this.TextureIndex = textureIndex;
        }
    }
}
