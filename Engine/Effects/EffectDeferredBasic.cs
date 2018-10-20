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
        private readonly EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationOffset = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialIndex = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureIndex = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMap = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture specularMap = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar animationPaletteWidth = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        private readonly EngineEffectVariableTexture animationPalette = null;
        /// <summary>
        /// Sampler for diffuse maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerDiffuse = null;
        /// <summary>
        /// Sampler for normal maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerNormal = null;

        /// <summary>
        /// Current diffuse map
        /// </summary>
        private EngineShaderResourceView currentDiffuseMap = null;
        /// <summary>
        /// Current normal map
        /// </summary>
        private EngineShaderResourceView currentNormalMap = null;
        /// <summary>
        /// Current specular map
        /// </summary>
        private EngineShaderResourceView currentSpecularMap = null;
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
        /// Specular map
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
            this.PositionColor = this.Effect.GetTechniqueByName("PositionColor");
            this.PositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinned");
            this.PositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColor");
            this.PositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinned");
            this.PositionTexture = this.Effect.GetTechniqueByName("PositionTexture");
            this.PositionTextureSkinned = this.Effect.GetTechniqueByName("PositionTextureSkinned");
            this.PositionNormalTexture = this.Effect.GetTechniqueByName("PositionNormalTexture");
            this.PositionNormalTextureSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureSkinned");
            this.PositionNormalTextureTangent = this.Effect.GetTechniqueByName("PositionNormalTextureTangent");
            this.PositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureTangentSkinned");
            this.InstancingPositionColor = this.Effect.GetTechniqueByName("PositionColorI");
            this.InstancingPositionColorSkinned = this.Effect.GetTechniqueByName("PositionColorSkinnedI");
            this.InstancingPositionNormalColor = this.Effect.GetTechniqueByName("PositionNormalColorI");
            this.InstancingPositionNormalColorSkinned = this.Effect.GetTechniqueByName("PositionNormalColorSkinnedI");
            this.InstancingPositionTexture = this.Effect.GetTechniqueByName("PositionTextureI");
            this.InstancingPositionTextureSkinned = this.Effect.GetTechniqueByName("PositionTextureSkinnedI");
            this.InstancingPositionNormalTexture = this.Effect.GetTechniqueByName("PositionNormalTextureI");
            this.InstancingPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureSkinnedI");
            this.InstancingPositionNormalTextureTangent = this.Effect.GetTechniqueByName("PositionNormalTextureTangentI");
            this.InstancingPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("PositionNormalTextureTangentSkinnedI");

            this.world = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.animationOffset = this.Effect.GetVariableScalar("gAnimationOffset");
            this.materialIndex = this.Effect.GetVariableScalar("gMaterialIndex");
            this.textureIndex = this.Effect.GetVariableScalar("gTextureIndex");
            this.diffuseMap = this.Effect.GetVariableTexture("gDiffuseMapArray");
            this.normalMap = this.Effect.GetVariableTexture("gNormalMapArray");
            this.specularMap = this.Effect.GetVariableTexture("gSpecularMapArray");
            this.animationPaletteWidth = this.Effect.GetVariableScalar("gAnimationPaletteWidth");
            this.animationPalette = this.Effect.GetVariableTexture("gAnimationPalette");

            //Samplers
            this.samplerDiffuse = this.Effect.GetVariableSampler("SamplerDiffuse");
            this.samplerNormal = this.Effect.GetVariableSampler("SamplerNormal");

            //Initialize states
            this.samplerLinear = EngineSamplerState.Linear(graphics);
            this.samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, 4);
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
                    return instanced ? this.InstancingPositionColor : this.PositionColor;
                case VertexTypes.PositionTexture:
                    return instanced ? this.InstancingPositionTexture : this.PositionTexture;
                case VertexTypes.PositionNormalColor:
                    return instanced ? this.InstancingPositionNormalColor : this.PositionNormalColor;
                case VertexTypes.PositionNormalTexture:
                    return instanced ? this.InstancingPositionNormalTexture : this.PositionNormalTexture;
                case VertexTypes.PositionNormalTextureTangent:
                    return instanced ? this.InstancingPositionNormalTextureTangent : this.PositionNormalTextureTangent;
                case VertexTypes.PositionColorSkinned:
                    return instanced ? this.InstancingPositionColorSkinned : this.PositionColorSkinned;
                case VertexTypes.PositionTextureSkinned:
                    return instanced ? this.InstancingPositionTextureSkinned : this.PositionTextureSkinned;
                case VertexTypes.PositionNormalColorSkinned:
                    return instanced ? this.InstancingPositionNormalColorSkinned : this.PositionNormalColorSkinned;
                case VertexTypes.PositionNormalTextureSkinned:
                    return instanced ? this.InstancingPositionNormalTextureSkinned : this.PositionNormalTextureSkinned;
                case VertexTypes.PositionNormalTextureTangentSkinned:
                    return instanced ? this.InstancingPositionNormalTextureTangentSkinned : this.PositionNormalTextureTangentSkinned;
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
        /// <param name="world">World Matrix</param>
        /// <param name="context">Context</param>
        public void UpdatePerFrameBasic(
            Matrix world,
            DrawContext context)
        {
            this.World = world;
            this.WorldViewProjection = world * context.ViewProjection;
        }
        /// <summary>
        /// Update per frame full data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="context">Context</param>
        public void UpdatePerFrameFull(
            Matrix world,
            DrawContext context)
        {
            this.UpdatePerFrameBasic(world, context);
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animationOffset">Animation index</param>
        /// <param name="material">Material</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="useAnisotropic">Use anisotropic filtering</param>
        public void UpdatePerObject(
            uint animationOffset,
            MeshMaterial material,
            uint textureIndex,
            bool useAnisotropic)
        {
            if (material != null)
            {
                this.DiffuseMap = material.DiffuseTexture;
                this.NormalMap = material.NormalMap;
                this.SpecularMap = material.SpecularTexture;
                this.MaterialIndex = material.ResourceIndex;
            }
            else
            {
                this.DiffuseMap = null;
                this.NormalMap = null;
                this.SpecularMap = null;
                this.MaterialIndex = 0;
            }

            this.TextureIndex = textureIndex;
            this.Anisotropic = useAnisotropic;

            this.AnimationOffset = animationOffset;
        }
    }
}
