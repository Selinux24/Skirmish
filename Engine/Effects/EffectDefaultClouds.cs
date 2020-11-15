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
                return worldViewProjectionVar.GetMatrix();
            }
            set
            {
                worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// First layer texture
        /// </summary>
        protected EngineShaderResourceView FirstTexture
        {
            get
            {
                return firstTextureVar.GetResource();
            }
            set
            {
                if (currentFirstTexture != value)
                {
                    firstTextureVar.SetResource(value);

                    currentFirstTexture = value;

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
                return secondTextureVar.GetResource();
            }
            set
            {
                if (currentSecondTexture != value)
                {
                    secondTextureVar.SetResource(value);

                    currentSecondTexture = value;

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
                return brightnessVar.GetFloat();
            }
            set
            {
                brightnessVar.Set(value);
            }
        }
        /// <summary>
        /// Cloud Color
        /// </summary>
        protected Color3 Color
        {
            get
            {
                return colorVar.GetVector<Color3>();
            }
            set
            {
                colorVar.Set(value);
            }
        }
        /// <summary>
        /// Clouds fadding distance
        /// </summary>
        protected float FadingDistance
        {
            get
            {
                return fadingDistanceVar.GetFloat();
            }
            set
            {
                fadingDistanceVar.Set(value);
            }
        }
        /// <summary>
        /// First layer translation
        /// </summary>
        protected Vector2 FirstTranslation
        {
            get
            {
                return firstTranslationVar.GetVector<Vector2>();
            }
            set
            {
                firstTranslationVar.Set(value);
            }
        }
        /// <summary>
        /// Second layer translation
        /// </summary>
        protected Vector2 SecondTranslation
        {
            get
            {
                return secondTranslationVar.GetVector<Vector2>();
            }
            set
            {
                secondTranslationVar.Set(value);
            }
        }
        /// <summary>
        /// Clouds translation
        /// </summary>
        protected float Translation
        {
            get
            {
                return translationVar.GetFloat();
            }
            set
            {
                translationVar.Set(value);
            }
        }
        /// <summary>
        /// Clouds scale
        /// </summary>
        protected float Scale
        {
            get
            {
                return scaleVar.GetFloat();
            }
            set
            {
                scaleVar.Set(value);
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
            CloudsStatic = Effect.GetTechniqueByName("CloudsStatic");
            CloudsPerturbed = Effect.GetTechniqueByName("CloudsPerturbed");

            firstTextureVar = Effect.GetVariableTexture("gCloudTexture1");
            secondTextureVar = Effect.GetVariableTexture("gCloudTexture2");

            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            brightnessVar = Effect.GetVariableScalar("gBrightness");
            colorVar = Effect.GetVariableVector("gColor");
            fadingDistanceVar = Effect.GetVariableScalar("gFadingDistance");

            firstTranslationVar = Effect.GetVariableVector("gFirstTranslation");
            secondTranslationVar = Effect.GetVariableVector("gSecondTranslation");

            translationVar = Effect.GetVariableScalar("gTranslation");
            scaleVar = Effect.GetVariableScalar("gScale");
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
            Color3 color,
            float fadingDistance,
            EngineShaderResourceView firstTexture,
            EngineShaderResourceView secondTexture)
        {
            WorldViewProjection = world * viewProjection;

            Brightness = brightness;
            Color = color;
            FadingDistance = fadingDistance;

            FirstTexture = firstTexture;
            SecondTexture = secondTexture;
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
            FirstTranslation = firstTranslation;
            SecondTranslation = secondTranslation;
        }
        /// <summary>
        /// Update perturbed clouds
        /// </summary>
        public void UpdatePerFramePerturbed(
            float translation,
            float scale)
        {
            Translation = translation;
            Scale = scale;
        }
    }
}
