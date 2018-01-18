using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectShadowPoint : Drawer, IShadowMapDrawer
    {
        #region Technique variables

        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColor = null;
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColorInstanced = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColorSkinned = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColorSkinnedInstanced = null;

        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColor = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColorInstanced = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColorSkinned = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColorSkinnedInstanced = null;

        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTexture = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureInstanced = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureSkinned = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureSkinnedInstanced = null;

        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureInstanced = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureSkinnedInstanced = null;

        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentInstanced = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentSkinned = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentSkinnedInstanced = null;

        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureTransparent = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureTransparentInstanced = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureTransparentSkinned = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureTransparentSkinnedInstanced = null;

        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTransparent = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTransparentInstanced = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTransparentSkinned = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTransparentSkinnedInstanced = null;

        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentTransparent = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentTransparentInstanced = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentTransparentSkinned = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentTransparentSkinnedInstanced = null;

        #endregion

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
            this.ShadowMapPositionColorInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionColorI");
            this.ShadowMapPositionColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionColorSkinned");
            this.ShadowMapPositionColorSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionColorSkinnedI");

            this.ShadowMapPositionNormalColor = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColor");
            this.ShadowMapPositionNormalColorInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorI");
            this.ShadowMapPositionNormalColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinned");
            this.ShadowMapPositionNormalColorSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinnedI");

            this.ShadowMapPositionTexture = this.Effect.GetTechniqueByName("ShadowMapPositionTexture");
            this.ShadowMapPositionTextureInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionTextureI");
            this.ShadowMapPositionTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionTextureSkinned");
            this.ShadowMapPositionTextureSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionTextureSkinnedI");

            this.ShadowMapPositionNormalTexture = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTexture");
            this.ShadowMapPositionNormalTextureInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureI");
            this.ShadowMapPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinned");
            this.ShadowMapPositionNormalTextureSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinnedI");

            this.ShadowMapPositionNormalTextureTangent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangent");
            this.ShadowMapPositionNormalTextureTangentInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentI");
            this.ShadowMapPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinned");
            this.ShadowMapPositionNormalTextureTangentSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinnedI");

            this.ShadowMapPositionTextureTransparent = this.Effect.GetTechniqueByName("ShadowMapPositionTextureTransparent");
            this.ShadowMapPositionTextureTransparentInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionTextureTransparentI");
            this.ShadowMapPositionTextureTransparentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionTextureTransparentSkinned");
            this.ShadowMapPositionTextureTransparentSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionTextureTransparentSkinnedI");

            this.ShadowMapPositionNormalTextureTransparent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparent");
            this.ShadowMapPositionNormalTextureTransparentInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparentI");
            this.ShadowMapPositionNormalTextureTransparentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparentSkinned");
            this.ShadowMapPositionNormalTextureTransparentSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparentSkinnedI");

            this.ShadowMapPositionNormalTextureTangentTransparent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparent");
            this.ShadowMapPositionNormalTextureTangentTransparentInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparentI");
            this.ShadowMapPositionNormalTextureTangentTransparentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparentSkinned");
            this.ShadowMapPositionNormalTextureTangentTransparentSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparentSkinnedI");

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
            if (transparent)
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionTexture:
                        return instanced ? this.ShadowMapPositionTextureTransparentInstanced : this.ShadowMapPositionTextureTransparent;
                    case VertexTypes.PositionTextureSkinned:
                        return instanced ? this.ShadowMapPositionTextureTransparentSkinnedInstanced : this.ShadowMapPositionTextureTransparentSkinned;

                    case VertexTypes.PositionNormalTexture:
                        return instanced ? this.ShadowMapPositionNormalTextureTransparentInstanced : this.ShadowMapPositionNormalTextureTransparent;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return instanced ? this.ShadowMapPositionNormalTextureTransparentSkinnedInstanced : this.ShadowMapPositionNormalTextureTransparentSkinned;

                    case VertexTypes.PositionNormalTextureTangent:
                        return instanced ? this.ShadowMapPositionNormalTextureTangentTransparentInstanced : this.ShadowMapPositionNormalTextureTangentTransparent;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return instanced ? this.ShadowMapPositionNormalTextureTangentTransparentSkinnedInstanced : this.ShadowMapPositionNormalTextureTangentTransparentSkinned;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Transparent: {2}", vertexType, instanced, transparent));
                }
            }
            else
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return instanced ? this.ShadowMapPositionColorInstanced : this.ShadowMapPositionColor;
                    case VertexTypes.PositionColorSkinned:
                        return instanced ? this.ShadowMapPositionColorSkinnedInstanced : this.ShadowMapPositionColorSkinned;

                    case VertexTypes.PositionTexture:
                        return instanced ? this.ShadowMapPositionTextureInstanced : this.ShadowMapPositionTexture;
                    case VertexTypes.PositionTextureSkinned:
                        return instanced ? this.ShadowMapPositionTextureSkinnedInstanced : this.ShadowMapPositionTextureSkinned;

                    case VertexTypes.PositionNormalColor:
                        return instanced ? this.ShadowMapPositionNormalColorInstanced : this.ShadowMapPositionNormalColor;
                    case VertexTypes.PositionNormalColorSkinned:
                        return instanced ? this.ShadowMapPositionNormalColorSkinnedInstanced : this.ShadowMapPositionNormalColorSkinned;

                    case VertexTypes.PositionNormalTexture:
                        return instanced ? this.ShadowMapPositionNormalTextureInstanced : this.ShadowMapPositionNormalTexture;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return instanced ? this.ShadowMapPositionNormalTextureSkinnedInstanced : this.ShadowMapPositionNormalTextureSkinned;

                    case VertexTypes.PositionNormalTextureTangent:
                        return instanced ? this.ShadowMapPositionNormalTextureTangentInstanced : this.ShadowMapPositionNormalTextureTangent;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return instanced ? this.ShadowMapPositionNormalTextureTangentSkinnedInstanced : this.ShadowMapPositionNormalTextureTangentSkinned;
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
        /// <param name="animationOffset">Animation index</param>
        /// <param name="material">Material</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObject(
            uint animationOffset,
            MeshMaterial material,
            uint textureIndex)
        {
            this.AnimationOffset = animationOffset;

            if (material != null)
            {
                this.DiffuseMap = material.DiffuseTexture;
            }
            else
            {
                this.diffuseMap = null;
            }

            this.TextureIndex = textureIndex;
        }
    }
}
