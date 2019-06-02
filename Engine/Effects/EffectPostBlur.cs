using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Blur effect
    /// </summary>
    public class EffectPostBlur : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Blur = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Blur direction effect variable
        /// </summary>
        private readonly EngineEffectVariableVector blurDirectionVar = null;
        /// <summary>
        /// Texture size effect variable
        /// </summary>
        private readonly EngineEffectVariableVector textureSizeVar = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapVar = null;

        /// <summary>
        /// Current diffuse map
        /// </summary>
        private EngineShaderResourceView currentDiffuseMap = null;

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
        /// Blur direction
        /// </summary>
        protected Vector2 BlurDirection
        {
            get
            {
                return this.blurDirectionVar.GetVector<Vector2>();
            }
            set
            {
                this.blurDirectionVar.Set(value);
            }
        }
        /// <summary>
        /// Texture size
        /// </summary>
        protected Vector2 TextureSize
        {
            get
            {
                return this.textureSizeVar.GetVector<Vector2>();
            }
            set
            {
                this.textureSizeVar.Set(value);
            }
        }
        /// <summary>
        /// Diffuse map
        /// </summary>
        protected EngineShaderResourceView DiffuseMap
        {
            get
            {
                return this.diffuseMapVar.GetResource();
            }
            set
            {
                if (this.currentDiffuseMap != value)
                {
                    this.diffuseMapVar.SetResource(value);

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
        public EffectPostBlur(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.Blur = this.Effect.GetTechniqueByName("Blur");

            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.blurDirectionVar = this.Effect.GetVariableVector("gBlurDirection");
            this.textureSizeVar = this.Effect.GetVariableVector("gTextureSize");
            this.diffuseMapVar = this.Effect.GetVariableTexture("gDiffuseMap");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="direction">Blur direction</param>
        /// <param name="size">Texture size</param>
        /// <param name="diffuseMap">DiffuseMap</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector2 direction,
            Vector2 size,
            EngineShaderResourceView diffuseMap)
        {
            this.WorldViewProjection = world * viewProjection;
            this.BlurDirection = direction;
            this.TextureSize = size;
            this.DiffuseMap = diffuseMap;
        }
    }
}
