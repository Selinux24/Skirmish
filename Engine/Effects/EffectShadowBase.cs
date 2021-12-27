using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Base shadow effect
    /// </summary>
    public abstract class EffectShadowBase : Drawer, IShadowMapDrawer
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
        protected readonly EngineEffectVariableMatrix WorldViewProjectionVariable = null;
        /// <summary>
        /// First animation offset effect variable
        /// </summary>
        protected readonly EngineEffectVariableScalar AnimationOffsetVariable = null;
        /// <summary>
        /// Second animation offset effect variable
        /// </summary>
        protected readonly EngineEffectVariableScalar AnimationOffset2Variable = null;
        /// <summary>
        /// Animation interpolation value between offsets
        /// </summary>
        protected readonly EngineEffectVariableScalar AnimationInterpolationVariable = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        protected readonly EngineEffectVariableScalar TextureIndexVariable = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        protected readonly EngineEffectVariableScalar AnimationPaletteWidthVariable = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        protected readonly EngineEffectVariableTexture AnimationPaletteVariable = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        protected readonly EngineEffectVariableTexture DiffuseMapVariable = null;

        /// <summary>
        /// Current animation palette
        /// </summary>
        protected EngineShaderResourceView CurrentAnimationPalette = null;
        /// <summary>
        /// Current diffuse map
        /// </summary>
        protected EngineShaderResourceView CurrentDiffuseMap = null;

        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return WorldViewProjectionVariable.GetMatrix();
            }
            set
            {
                WorldViewProjectionVariable.SetMatrix(value);
            }
        }
        /// <summary>
        /// First animation offset
        /// </summary>
        protected uint AnimationOffset
        {
            get
            {
                return AnimationOffsetVariable.GetUInt();
            }
            set
            {
                AnimationOffsetVariable.Set(value);
            }
        }
        /// <summary>
        /// Second animation offset
        /// </summary>
        protected uint AnimationOffset2
        {
            get
            {
                return AnimationOffset2Variable.GetUInt();
            }
            set
            {
                AnimationOffset2Variable.Set(value);
            }
        }
        /// <summary>
        /// Animation interpolation between offsets
        /// </summary>
        protected float AnimationInterpolation
        {
            get
            {
                return AnimationInterpolationVariable.GetFloat();
            }
            set
            {
                AnimationInterpolationVariable.Set(value);
            }
        }
        /// <summary>
        /// Texture index
        /// </summary>
        protected uint TextureIndex
        {
            get
            {
                return TextureIndexVariable.GetUInt();
            }
            set
            {
                TextureIndexVariable.Set(value);
            }
        }
        /// <summary>
        /// Animation palette width
        /// </summary>
        protected uint AnimationPaletteWidth
        {
            get
            {
                return AnimationPaletteWidthVariable.GetUInt();
            }
            set
            {
                AnimationPaletteWidthVariable.Set(value);
            }
        }
        /// <summary>
        /// Animation palette
        /// </summary>
        protected EngineShaderResourceView AnimationPalette
        {
            get
            {
                return AnimationPaletteVariable.GetResource();
            }
            set
            {
                if (CurrentAnimationPalette != value)
                {
                    AnimationPaletteVariable.SetResource(value);

                    CurrentAnimationPalette = value;

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
                return DiffuseMapVariable.GetResource();
            }
            set
            {
                if (CurrentDiffuseMap != value)
                {
                    DiffuseMapVariable.SetResource(value);

                    CurrentDiffuseMap = value;

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
        protected EffectShadowBase(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            ShadowMapPositionColor = Effect.GetTechniqueByName("ShadowMapPositionColor");
            ShadowMapPositionColorInstanced = Effect.GetTechniqueByName("ShadowMapPositionColorI");
            ShadowMapPositionColorSkinned = Effect.GetTechniqueByName("ShadowMapPositionColorSkinned");
            ShadowMapPositionColorSkinnedInstanced = Effect.GetTechniqueByName("ShadowMapPositionColorSkinnedI");

            ShadowMapPositionNormalColor = Effect.GetTechniqueByName("ShadowMapPositionNormalColor");
            ShadowMapPositionNormalColorInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalColorI");
            ShadowMapPositionNormalColorSkinned = Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinned");
            ShadowMapPositionNormalColorSkinnedInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinnedI");

            ShadowMapPositionTexture = Effect.GetTechniqueByName("ShadowMapPositionTexture");
            ShadowMapPositionTextureInstanced = Effect.GetTechniqueByName("ShadowMapPositionTextureI");
            ShadowMapPositionTextureSkinned = Effect.GetTechniqueByName("ShadowMapPositionTextureSkinned");
            ShadowMapPositionTextureSkinnedInstanced = Effect.GetTechniqueByName("ShadowMapPositionTextureSkinnedI");

            ShadowMapPositionNormalTexture = Effect.GetTechniqueByName("ShadowMapPositionNormalTexture");
            ShadowMapPositionNormalTextureInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureI");
            ShadowMapPositionNormalTextureSkinned = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinned");
            ShadowMapPositionNormalTextureSkinnedInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinnedI");

            ShadowMapPositionNormalTextureTangent = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangent");
            ShadowMapPositionNormalTextureTangentInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentI");
            ShadowMapPositionNormalTextureTangentSkinned = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinned");
            ShadowMapPositionNormalTextureTangentSkinnedInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinnedI");

            ShadowMapPositionTextureTransparent = Effect.GetTechniqueByName("ShadowMapPositionTextureTransparent");
            ShadowMapPositionTextureTransparentInstanced = Effect.GetTechniqueByName("ShadowMapPositionTextureTransparentI");
            ShadowMapPositionTextureTransparentSkinned = Effect.GetTechniqueByName("ShadowMapPositionTextureTransparentSkinned");
            ShadowMapPositionTextureTransparentSkinnedInstanced = Effect.GetTechniqueByName("ShadowMapPositionTextureTransparentSkinnedI");

            ShadowMapPositionNormalTextureTransparent = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparent");
            ShadowMapPositionNormalTextureTransparentInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparentI");
            ShadowMapPositionNormalTextureTransparentSkinned = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparentSkinned");
            ShadowMapPositionNormalTextureTransparentSkinnedInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparentSkinnedI");

            ShadowMapPositionNormalTextureTangentTransparent = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparent");
            ShadowMapPositionNormalTextureTangentTransparentInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparentI");
            ShadowMapPositionNormalTextureTangentTransparentSkinned = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparentSkinned");
            ShadowMapPositionNormalTextureTangentTransparentSkinnedInstanced = Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparentSkinnedI");

            AnimationPaletteWidthVariable = Effect.GetVariableScalar("gAnimationPaletteWidth");
            AnimationPaletteVariable = Effect.GetVariableTexture("gAnimationPalette");
            WorldViewProjectionVariable = Effect.GetVariableMatrix("gVSWorldViewProjection");
            AnimationOffsetVariable = Effect.GetVariableScalar("gVSAnimationOffset");
            AnimationOffset2Variable = Effect.GetVariableScalar("gVSAnimationOffset2");
            AnimationInterpolationVariable = Effect.GetVariableScalar("gVSAnimationInterpolation");
            DiffuseMapVariable = Effect.GetVariableTexture("gPSDiffuseMapArray");
            TextureIndexVariable = Effect.GetVariableScalar("gPSTextureIndex");
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
                return GetTechniqueTransparent(vertexType, instanced);
            }
            else
            {
                return GetTechniqueOpaque(vertexType, instanced);
            }
        }
        /// <summary>
        /// Get technique by vertex type for opaque objects
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        private EngineEffectTechnique GetTechniqueOpaque(
            VertexTypes vertexType,
            bool instanced)
        {
            if (!instanced)
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return ShadowMapPositionColor;
                    case VertexTypes.PositionColorSkinned:
                        return ShadowMapPositionColorSkinned;

                    case VertexTypes.PositionTexture:
                        return ShadowMapPositionTexture;
                    case VertexTypes.PositionTextureSkinned:
                        return ShadowMapPositionTextureSkinned;

                    case VertexTypes.PositionNormalColor:
                        return ShadowMapPositionNormalColor;
                    case VertexTypes.PositionNormalColorSkinned:
                        return ShadowMapPositionNormalColorSkinned;

                    case VertexTypes.PositionNormalTexture:
                        return ShadowMapPositionNormalTexture;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return ShadowMapPositionNormalTextureSkinned;

                    case VertexTypes.PositionNormalTextureTangent:
                        return ShadowMapPositionNormalTextureTangent;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return ShadowMapPositionNormalTextureTangentSkinned;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Opaque", vertexType, instanced));
                }
            }
            else
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return ShadowMapPositionColorInstanced;
                    case VertexTypes.PositionColorSkinned:
                        return ShadowMapPositionColorSkinnedInstanced;

                    case VertexTypes.PositionTexture:
                        return ShadowMapPositionTextureInstanced;
                    case VertexTypes.PositionTextureSkinned:
                        return ShadowMapPositionTextureSkinnedInstanced;

                    case VertexTypes.PositionNormalColor:
                        return ShadowMapPositionNormalColorInstanced;
                    case VertexTypes.PositionNormalColorSkinned:
                        return ShadowMapPositionNormalColorSkinnedInstanced;

                    case VertexTypes.PositionNormalTexture:
                        return ShadowMapPositionNormalTextureInstanced;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return ShadowMapPositionNormalTextureSkinnedInstanced;

                    case VertexTypes.PositionNormalTextureTangent:
                        return ShadowMapPositionNormalTextureTangentInstanced;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return ShadowMapPositionNormalTextureTangentSkinnedInstanced;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Opaque", vertexType, instanced));
                }
            }
        }
        /// <summary>
        /// Get technique by vertex type for transparent objects
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        private EngineEffectTechnique GetTechniqueTransparent(
            VertexTypes vertexType,
            bool instanced)
        {
            if (!instanced)
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return ShadowMapPositionColor;
                    case VertexTypes.PositionColorSkinned:
                        return ShadowMapPositionColorSkinned;

                    case VertexTypes.PositionTexture:
                        return ShadowMapPositionTextureTransparent;
                    case VertexTypes.PositionTextureSkinned:
                        return ShadowMapPositionTextureTransparentSkinned;

                    case VertexTypes.PositionNormalColor:
                        return ShadowMapPositionNormalColor;
                    case VertexTypes.PositionNormalColorSkinned:
                        return ShadowMapPositionNormalColorSkinned;

                    case VertexTypes.PositionNormalTexture:
                        return ShadowMapPositionNormalTextureTransparent;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return ShadowMapPositionNormalTextureTransparentSkinned;

                    case VertexTypes.PositionNormalTextureTangent:
                        return ShadowMapPositionNormalTextureTangentTransparent;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return ShadowMapPositionNormalTextureTangentTransparentSkinned;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Transparent", vertexType, instanced));
                }
            }
            else
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return ShadowMapPositionColorInstanced;
                    case VertexTypes.PositionColorSkinned:
                        return ShadowMapPositionColorSkinnedInstanced;

                    case VertexTypes.PositionTexture:
                        return ShadowMapPositionTextureTransparentInstanced;
                    case VertexTypes.PositionTextureSkinned:
                        return ShadowMapPositionTextureTransparentSkinnedInstanced;

                    case VertexTypes.PositionNormalColor:
                        return ShadowMapPositionNormalColorInstanced;
                    case VertexTypes.PositionNormalColorSkinned:
                        return ShadowMapPositionNormalColorSkinnedInstanced;

                    case VertexTypes.PositionNormalTexture:
                        return ShadowMapPositionNormalTextureTransparentInstanced;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return ShadowMapPositionNormalTextureTransparentSkinnedInstanced;

                    case VertexTypes.PositionNormalTextureTangent:
                        return ShadowMapPositionNormalTextureTangentTransparentInstanced;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return ShadowMapPositionNormalTextureTangentTransparentSkinnedInstanced;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Transparent", vertexType, instanced));
                }
            }
        }

        /// <inheritdoc/>
        public abstract void UpdateGlobals(
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth);
        /// <inheritdoc/>
        public abstract void UpdatePerFrame(
            Matrix world,
            DrawContextShadows context);
        /// <inheritdoc/>
        public abstract void UpdatePerObject(
            uint animationOffset,
            uint animationOffset2,
            float animationInterpolation,
            IMeshMaterial material,
            uint textureIndex);
    }
}
