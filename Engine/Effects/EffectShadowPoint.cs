using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectShadowPoint : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentSkinned = null;
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionColor = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionColorSkinned = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionNormalColor = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionNormalColorSkinned = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingShadowMapPositionNormalTextureTangentSkinned = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private EngineEffectVariableScalar animationOffset = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private EngineEffectVariableScalar textureIndex = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private EngineEffectVariableScalar animationPaletteWidth = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private EngineEffectVariableTexture animationPalette = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private EngineEffectVariableTexture diffuseMap = null;

        /// <summary>
        /// Current animation palette
        /// </summary>
        private EngineShaderResourceView currentAnimationPalette = null;
        /// <summary>
        /// Current diffuse map
        /// </summary>
        private EngineShaderResourceView currentDiffuseMap = null;

        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix[] WorldViewProjection
        {
            get
            {
                return this.worldViewProjection.GetMatrixArray(6);
            }
            set
            {
                if (value == null)
                {
                    this.worldViewProjection.SetMatrix(new Matrix[6]);
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
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadowPoint(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.ShadowMapPositionColor = this.Effect.GetTechniqueByName("ShadowMapPositionColor");
            this.ShadowMapPositionColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionColorSkinned");
            this.ShadowMapPositionNormalColor = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColor");
            this.ShadowMapPositionNormalColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinned");
            this.ShadowMapPositionTexture = this.Effect.GetTechniqueByName("ShadowMapPositionTexture");
            this.ShadowMapPositionTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionTextureSkinned");
            this.ShadowMapPositionNormalTexture = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTexture");
            this.ShadowMapPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinned");
            this.ShadowMapPositionNormalTextureTangent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangent");
            this.ShadowMapPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinned");
            this.InstancingShadowMapPositionColor = this.Effect.GetTechniqueByName("ShadowMapPositionColorI");
            this.InstancingShadowMapPositionColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionColorSkinnedI");
            this.InstancingShadowMapPositionNormalColor = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorI");
            this.InstancingShadowMapPositionNormalColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinnedI");
            this.InstancingShadowMapPositionTexture = this.Effect.GetTechniqueByName("ShadowMapPositionTextureI");
            this.InstancingShadowMapPositionTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionTextureSkinnedI");
            this.InstancingShadowMapPositionNormalTexture = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureI");
            this.InstancingShadowMapPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinnedI");
            this.InstancingShadowMapPositionNormalTextureTangent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentI");
            this.InstancingShadowMapPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinnedI");

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
        /// <returns>Returns the technique to process the specified vertex type</returns>
        public EngineEffectTechnique GetTechnique(
            VertexTypes vertexType,
            bool instanced)
        {
            switch (vertexType)
            {
                case VertexTypes.PositionColor:
                    return instanced ? this.InstancingShadowMapPositionColor : this.ShadowMapPositionColor;
                case VertexTypes.PositionTexture:
                    return instanced ? this.InstancingShadowMapPositionTexture : this.ShadowMapPositionTexture;
                case VertexTypes.PositionNormalColor:
                    return instanced ? this.InstancingShadowMapPositionNormalColor : this.ShadowMapPositionNormalColor;
                case VertexTypes.PositionNormalTexture:
                    return instanced ? this.InstancingShadowMapPositionNormalTexture : this.ShadowMapPositionNormalTexture;
                case VertexTypes.PositionNormalTextureTangent:
                    return instanced ? this.InstancingShadowMapPositionNormalTextureTangent : this.ShadowMapPositionNormalTextureTangent;
                case VertexTypes.PositionColorSkinned:
                    return instanced ? this.InstancingShadowMapPositionColorSkinned : this.ShadowMapPositionColorSkinned;
                case VertexTypes.PositionTextureSkinned:
                    return instanced ? this.InstancingShadowMapPositionTextureSkinned : this.ShadowMapPositionTextureSkinned;
                case VertexTypes.PositionNormalColorSkinned:
                    return instanced ? this.InstancingShadowMapPositionNormalColorSkinned : this.ShadowMapPositionNormalColorSkinned;
                case VertexTypes.PositionNormalTextureSkinned:
                    return instanced ? this.InstancingShadowMapPositionNormalTextureSkinned : this.ShadowMapPositionNormalTextureSkinned;
                case VertexTypes.PositionNormalTextureTangentSkinned:
                    return instanced ? this.InstancingShadowMapPositionNormalTextureTangentSkinned : this.ShadowMapPositionNormalTextureTangentSkinned;
                default:
                    throw new EngineException(string.Format("Bad vertex type for effect: {0}", vertexType));
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
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix[] viewProjection)
        {
            if (viewProjection != null && viewProjection.Length > 0)
            {
                if (viewProjection.Length != 6)
                {
                    throw new EngineException("The matrix array must have a length of 6");
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
        /// <param name="diffuseMap">Diffuse map</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="animationOffset">Animation index</param>
        public void UpdatePerObject(
            EngineShaderResourceView diffuseMap,
            uint textureIndex,
            uint animationOffset)
        {
            this.DiffuseMap = diffuseMap;
            this.TextureIndex = textureIndex;

            this.AnimationOffset = animationOffset;
        }
    }
}
