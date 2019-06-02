using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Clouds effect
    /// </summary>
    public class EffectDefaultClouds : Drawer
    {
        /// <summary>
        /// Default clouds technique
        /// </summary>
        public readonly EngineEffectTechnique CloudsStatic = null;
        /// <summary>
        /// Perturbed clouds technique
        /// </summary>
        public readonly EngineEffectTechnique CloudsPerturbed = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// First layer texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture firstTextureVar = null;
        /// <summary>
        /// Second layer texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture secondTextureVar = null;
        /// <summary>
        /// Brightness
        /// </summary>
        private readonly EngineEffectVariableScalar brightnessVar = null;
        /// <summary>
        /// Clouds fadding distance effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar fadingDistanceVar = null;
        /// <summary>
        /// Cloud color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector colorVar = null;

        /// <summary>
        /// First layer translation effect variable
        /// </summary>
        private readonly EngineEffectVariableVector firstTranslationVar = null;
        /// <summary>
        /// Second layer translation effect variable
        /// </summary>
        private readonly EngineEffectVariableVector secondTranslationVar = null;
        /// <summary>
        /// Clouds translation effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar translationVar = null;
        /// <summary>
        /// Clouds scale effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar scaleVar = null;

        /// <summary>
        /// Current first texture
        /// </summary>
        private EngineShaderResourceView currentFirstTexture = null;
        /// <summary>
        /// Current second texture
        /// </summary>
        private EngineShaderResourceView currentSecondTexture = null;

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
        /// First layer texture
        /// </summary>
        protected EngineShaderResourceView FirstTexture
        {
            get
            {
                return this.firstTextureVar.GetResource();
            }
            set
            {
                if (this.currentFirstTexture != value)
                {
                    this.firstTextureVar.SetResource(value);

                    this.currentFirstTexture = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Second layer texture
        /// </summary>
        protected EngineShaderResourceView SecondTexture
        {
            get
            {
                return this.secondTextureVar.GetResource();
            }
            set
            {
                if (this.currentSecondTexture != value)
                {
                    this.secondTextureVar.SetResource(value);

                    this.currentSecondTexture = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Brightness
        /// </summary>
        protected float Brightness
        {
            get
            {
                return this.brightnessVar.GetFloat();
            }
            set
            {
                this.brightnessVar.Set(value);
            }
        }
        /// <summary>
        /// Cloud Color
        /// </summary>
        protected Color3 Color
        {
            get
            {
                return this.colorVar.GetVector<Color3>();
            }
            set
            {
                this.colorVar.Set(value);
            }
        }
        /// <summary>
        /// Clouds fadding distance
        /// </summary>
        protected float FadingDistance
        {
            get
            {
                return this.fadingDistanceVar.GetFloat();
            }
            set
            {
                this.fadingDistanceVar.Set(value);
            }
        }
        /// <summary>
        /// First layer translation
        /// </summary>
        protected Vector2 FirstTranslation
        {
            get
            {
                return this.firstTranslationVar.GetVector<Vector2>();
            }
            set
            {
                this.firstTranslationVar.Set(value);
            }
        }
        /// <summary>
        /// Second layer translation
        /// </summary>
        protected Vector2 SecondTranslation
        {
            get
            {
                return this.secondTranslationVar.GetVector<Vector2>();
            }
            set
            {
                this.secondTranslationVar.Set(value);
            }
        }
        /// <summary>
        /// Clouds translation
        /// </summary>
        protected float Translation
        {
            get
            {
                return this.translationVar.GetFloat();
            }
            set
            {
                this.translationVar.Set(value);
            }
        }
        /// <summary>
        /// Clouds scale
        /// </summary>
        protected float Scale
        {
            get
            {
                return this.scaleVar.GetFloat();
            }
            set
            {
                this.scaleVar.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultClouds(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.CloudsStatic = this.Effect.GetTechniqueByName("CloudsStatic");
            this.CloudsPerturbed = this.Effect.GetTechniqueByName("CloudsPerturbed");

            this.firstTextureVar = this.Effect.GetVariableTexture("gCloudTexture1");
            this.secondTextureVar = this.Effect.GetVariableTexture("gCloudTexture2");

            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.brightnessVar = this.Effect.GetVariableScalar("gBrightness");
            this.colorVar = this.Effect.GetVariableVector("gColor");
            this.fadingDistanceVar = this.Effect.GetVariableScalar("gFadingDistance");

            this.firstTranslationVar = this.Effect.GetVariableVector("gFirstTranslation");
            this.secondTranslationVar = this.Effect.GetVariableVector("gSecondTranslation");

            this.translationVar = this.Effect.GetVariableScalar("gTranslation");
            this.scaleVar = this.Effect.GetVariableScalar("gScale");
        }

        /// <summary>
        /// Update per frame
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="brightness">Brightness</param>
        /// <param name="color">Cloud color</param>
        /// <param name="fadingDistance">FadingDistance</param>
        /// <param name="firstTexture">First texture</param>
        /// <param name="secondTexture">Second texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float brightness,
            Color4 color,
            float fadingDistance,
            EngineShaderResourceView firstTexture,
            EngineShaderResourceView secondTexture)
        {
            this.WorldViewProjection = world * viewProjection;

            this.Brightness = brightness;
            this.Color = color.RGB();
            this.FadingDistance = fadingDistance;

            this.FirstTexture = firstTexture;
            this.SecondTexture = secondTexture;
        }
        /// <summary>
        /// Update static clouds
        /// </summary>
        /// <param name="firstTranslation">First layer translation</param>
        /// <param name="secondTranslation">Second layer translation</param>
        public void UpdatePerFrameStatic(
            Vector2 firstTranslation,
            Vector2 secondTranslation)
        {
            this.FirstTranslation = firstTranslation;
            this.SecondTranslation = secondTranslation;
        }
        /// <summary>
        /// Update perturbed clouds
        /// </summary>
        public void UpdatePerFramePerturbed(
            float translation,
            float scale)
        {
            this.Translation = translation;
            this.Scale = scale;
        }
    }
}
