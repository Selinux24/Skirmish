using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDeferredBasic : Drawer, IGeometryDrawer
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
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique PositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
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
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTexture = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionTextureSkinned = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique InstancingPositionNormalTextureTangentSkinned = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
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
        /// Sampler for diffuse maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerDiffuseVar = null;
        /// <summary>
        /// Sampler for normal maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerNormalVar = null;

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
        /// Use anisotropic sampling
        /// </summary>
        private bool? anisotropic = null;

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
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDeferredBasic(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            PositionColor = Effect.GetTechniqueByName("PositionColor");
            PositionColorSkinned = Effect.GetTechniqueByName("PositionColorSkinned");
            PositionNormalColor = Effect.GetTechniqueByName("PositionNormalColor");
            PositionNormalColorSkinned = Effect.GetTechniqueByName("PositionNormalColorSkinned");
            PositionTexture = Effect.GetTechniqueByName("PositionTexture");
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
            InstancingPositionTextureSkinned = Effect.GetTechniqueByName("PositionTextureSkinnedI");
            InstancingPositionNormalTexture = Effect.GetTechniqueByName("PositionNormalTextureI");
            InstancingPositionNormalTextureSkinned = Effect.GetTechniqueByName("PositionNormalTextureSkinnedI");
            InstancingPositionNormalTextureTangent = Effect.GetTechniqueByName("PositionNormalTextureTangentI");
            InstancingPositionNormalTextureTangentSkinned = Effect.GetTechniqueByName("PositionNormalTextureTangentSkinnedI");

            worldVar = Effect.GetVariableMatrix("gWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            animationOffsetVar = Effect.GetVariableScalar("gAnimationOffset");
            animationOffset2Var = Effect.GetVariableScalar("gAnimationOffset2");
            animationInterpolationVar = Effect.GetVariableScalar("gAnimationInterpolation");
            materialIndexVar = Effect.GetVariableScalar("gMaterialIndex");
            textureIndexVar = Effect.GetVariableScalar("gTextureIndex");
            diffuseMapVar = Effect.GetVariableTexture("gDiffuseMapArray");
            normalMapVar = Effect.GetVariableTexture("gNormalMapArray");
            animationPaletteWidthVar = Effect.GetVariableScalar("gAnimationPaletteWidth");
            animationPaletteVar = Effect.GetVariableTexture("gAnimationPalette");

            //Samplers
            samplerDiffuseVar = Effect.GetVariableSampler("SamplerDiffuse");
            samplerNormalVar = Effect.GetVariableSampler("SamplerNormal");

            //Initialize states
            samplerLinear = EngineSamplerState.Linear(graphics);
            samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, 4);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EffectDeferredBasic()
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
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWith">Animation palette texture width</param>
        public void UpdateGlobals(
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {
            AnimationPalette = animationPalette;
            AnimationPaletteWidth = animationPaletteWidth;
        }
        /// <inheritdoc/>
        public void UpdatePerFrameBasic(
            Matrix world,
            DrawContext context)
        {
            World = world;
            WorldViewProjection = world * context.ViewProjection;
        }
        /// <inheritdoc/>
        public void UpdatePerFrameFull(
            Matrix world,
            DrawContext context)
        {
            UpdatePerFrameBasic(world, context);
        }
        /// <inheritdoc/>
        public void UpdatePerObject()
        {
            UpdatePerObject(AnimationDrawInfo.Empty, MaterialDrawInfo.Empty, 0);
        }
        /// <inheritdoc/>
        public void UpdatePerObject(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex)
        {
            AnimationOffset = animation.Offset1;
            AnimationOffset2 = animation.Offset2;
            AnimationInterpolation = animation.InterpolationAmount;

            DiffuseMap = material.Material?.DiffuseTexture;
            NormalMap = material.Material?.NormalMap;
            MaterialIndex = material.Material?.ResourceIndex ?? 0;
            Anisotropic = material.UseAnisotropic;

            TextureIndex = textureIndex;
        }
    }
}
