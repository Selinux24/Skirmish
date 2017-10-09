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
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// First layer texture effect variable
        /// </summary>
        private EngineEffectVariableTexture firstTexture = null;
        /// <summary>
        /// Second layer texture effect variable
        /// </summary>
        private EngineEffectVariableTexture secondTexture = null;
        /// <summary>
        /// Brightness
        /// </summary>
        private EngineEffectVariableScalar brightness = null;
        /// <summary>
        /// Clouds fadding distance effect variable
        /// </summary>
        private EngineEffectVariableScalar fadingDistance = null;

        /// <summary>
        /// First layer translation effect variable
        /// </summary>
        private EngineEffectVariableVector firstTranslation = null;
        /// <summary>
        /// Second layer translation effect variable
        /// </summary>
        private EngineEffectVariableVector secondTranslation = null;
        /// <summary>
        /// Clouds translation effect variable
        /// </summary>
        private EngineEffectVariableScalar translation = null;
        /// <summary>
        /// Clouds scale effect variable
        /// </summary>
        private EngineEffectVariableScalar scale = null;

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
                return this.worldViewProjection.GetMatrix();
            }
            set
            {
                this.worldViewProjection.SetMatrix(value);
            }
        }
        /// <summary>
        /// First layer texture
        /// </summary>
        protected EngineShaderResourceView FirstTexture
        {
            get
            {
                return this.firstTexture.GetResource();
            }
            set
            {
                if (this.currentFirstTexture != value)
                {
                    this.firstTexture.SetResource(value);

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
                return this.secondTexture.GetResource();
            }
            set
            {
                if (this.currentSecondTexture != value)
                {
                    this.secondTexture.SetResource(value);

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
                return this.brightness.GetFloat();
            }
            set
            {
                this.brightness.Set(value);
            }
        }
        /// <summary>
        /// Clouds fadding distance
        /// </summary>
        protected float FadingDistance
        {
            get
            {
                return this.fadingDistance.GetFloat();
            }
            set
            {
                this.fadingDistance.Set(value);
            }
        }
        /// <summary>
        /// First layer translation
        /// </summary>
        protected Vector2 FirstTranslation
        {
            get
            {
                var v = this.firstTranslation.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                var v = new Vector4(value.X, value.Y, 0, 0);

                this.firstTranslation.Set(v);
            }
        }
        /// <summary>
        /// Second layer translation
        /// </summary>
        protected Vector2 SecondTranslation
        {
            get
            {
                var v = this.secondTranslation.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                var v = new Vector4(value.X, value.Y, 0, 0);

                this.secondTranslation.Set(v);
            }
        }
        /// <summary>
        /// Clouds translation
        /// </summary>
        protected float Translation
        {
            get
            {
                return this.translation.GetFloat();
            }
            set
            {
                this.translation.Set(value);
            }
        }
        /// <summary>
        /// Clouds scale
        /// </summary>
        protected float Scale
        {
            get
            {
                return this.scale.GetFloat();
            }
            set
            {
                this.scale.Set(value);
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

            this.firstTexture = this.Effect.GetVariableTexture("gCloudTexture1");
            this.secondTexture = this.Effect.GetVariableTexture("gCloudTexture2");

            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.brightness = this.Effect.GetVariableScalar("gBrightness");
            this.fadingDistance = this.Effect.GetVariableScalar("gFadingDistance");

            this.firstTranslation = this.Effect.GetVariableVector("gFirstTranslation");
            this.secondTranslation = this.Effect.GetVariableVector("gSecondTranslation");

            this.translation = this.Effect.GetVariableScalar("gTranslation");
            this.scale = this.Effect.GetVariableScalar("gScale");
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
            throw new EngineException("Use technique variables directly");
        }

        /// <summary>
        /// Update per frame
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="brightness">Brightness</param>
        /// <param name="fadingDistance">FadingDistance</param>
        /// <param name="firstTexture">First texture</param>
        /// <param name="secondTexture">Second texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float brightness,
            float fadingDistance,
            EngineShaderResourceView firstTexture,
            EngineShaderResourceView secondTexture)
        {
            this.WorldViewProjection = world * viewProjection;

            this.Brightness = brightness;
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
