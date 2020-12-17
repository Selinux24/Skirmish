using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.PostProcessing;

    /// <summary>
    /// Post-process effect drawer
    /// </summary>
    public class EffectPostProcess : Drawer, IDrawerPostProcess
    {
        /// <summary>
        /// Empty drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Empty = null;
        /// <summary>
        /// Gray scale drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Grayscale = null;
        /// <summary>
        /// Sepia drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Sepia = null;
        /// <summary>
        /// Vignette drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Vignette = null;
        /// <summary>
        /// Blur drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Blur = null;
        /// <summary>
        /// Blur + vignette drawing technique
        /// </summary>
        public readonly EngineEffectTechnique BlurVignette = null;
        /// <summary>
        /// Bloom drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Bloom = null;
        /// <summary>
        /// Grain drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Grain = null;
        /// <summary>
        /// Tone mapping technique
        /// </summary>
        public readonly EngineEffectTechnique ToneMapping = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Texture size effect variable
        /// </summary>
        private readonly EngineEffectVariableVector textureSizeVar = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapVar = null;
        /// <summary>
        /// Effect intensity effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar effectIntensityVar = null;
        /// <summary>
        /// Time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar timeVar = null;

        /// <summary>
        /// Blur direcctions effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar blurDirectionsVar = null;
        /// <summary>
        /// Blur quality effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar blurQualityVar = null;
        /// <summary>
        /// Blur size effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar blurSizeVar = null;
        /// <summary>
        /// Vignette outer ring effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar vignetteOuterVar = null;
        /// <summary>
        /// Vignette inner ring effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar vignetteInnerVar = null;
        /// <summary>
        /// Bloom intensity ring effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar bloomIntensityVar = null;
        /// <summary>
        /// Tone mapping tone
        /// </summary>
        private readonly EngineEffectVariableScalar toneMappingToneVar = null;

        /// <summary>
        /// Time
        /// </summary>
        protected float Time
        {
            get
            {
                return timeVar.GetFloat();
            }
            set
            {
                timeVar.Set(value);
            }
        }
        /// <summary>
        /// Effect intensity
        /// </summary>
        protected float EffectIntensity
        {
            get
            {
                return effectIntensityVar.GetFloat();
            }
            set
            {
                effectIntensityVar.Set(value);
            }
        }

        /// <summary>
        /// Blur direction
        /// </summary>
        protected float BlurDirections
        {
            get
            {
                return blurDirectionsVar.GetFloat();
            }
            set
            {
                blurDirectionsVar.Set(value);
            }
        }
        /// <summary>
        /// Blur quality
        /// </summary>
        protected float BlurQuality
        {
            get
            {
                return blurQualityVar.GetFloat();
            }
            set
            {
                blurQualityVar.Set(value);
            }
        }
        /// <summary>
        /// Blur size
        /// </summary>
        protected float BlurSize
        {
            get
            {
                return blurSizeVar.GetFloat();
            }
            set
            {
                blurSizeVar.Set(value);
            }
        }
        /// <summary>
        /// Vignette outer ring
        /// </summary>
        protected float VignetteOuter
        {
            get
            {
                return vignetteOuterVar.GetFloat();
            }
            set
            {
                vignetteOuterVar.Set(value);
            }
        }
        /// <summary>
        /// Vignette inner ring
        /// </summary>
        protected float VignetteInner
        {
            get
            {
                return vignetteInnerVar.GetFloat();
            }
            set
            {
                vignetteInnerVar.Set(value);
            }
        }
        /// <summary>
        /// Bloom intensity
        /// </summary>
        protected float BloomIntensity
        {
            get
            {
                return bloomIntensityVar.GetFloat();
            }
            set
            {
                bloomIntensityVar.Set(value);
            }
        }
        /// <summary>
        /// Tone mapping tone
        /// </summary>
        protected uint ToneMappingTone
        {
            get
            {
                return toneMappingToneVar.GetUInt();
            }
            set
            {
                toneMappingToneVar.Set(value);
            }
        }

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
                return worldViewProjectionVar.GetMatrix();
            }
            set
            {
                worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Texture size
        /// </summary>
        protected Vector2 TextureSize
        {
            get
            {
                return textureSizeVar.GetVector<Vector2>();
            }
            set
            {
                textureSizeVar.Set(value);
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
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectPostProcess(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            Empty = Effect.GetTechniqueByName("Empty");
            Grayscale = Effect.GetTechniqueByName("Grayscale");
            Sepia = Effect.GetTechniqueByName("Sepia");
            Vignette = Effect.GetTechniqueByName("Vignette");
            Blur = Effect.GetTechniqueByName("Blur");
            BlurVignette = Effect.GetTechniqueByName("BlurVignette");
            Bloom = Effect.GetTechniqueByName("Bloom");
            Grain = Effect.GetTechniqueByName("Grain");
            ToneMapping = Effect.GetTechniqueByName("ToneMapping");

            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            textureSizeVar = Effect.GetVariableVector("gTextureSize");
            diffuseMapVar = Effect.GetVariableTexture("gDiffuseMap");
            effectIntensityVar = Effect.GetVariableScalar("gEffectIntensity");
            timeVar = Effect.GetVariableScalar("gTime");

            blurDirectionsVar = Effect.GetVariableScalar("gBlurDirections");
            blurQualityVar = Effect.GetVariableScalar("gBlurQuality");
            blurSizeVar = Effect.GetVariableScalar("gBlurSize");

            vignetteOuterVar = Effect.GetVariableScalar("gVignetteOuter");
            vignetteInnerVar = Effect.GetVariableScalar("gVignetteInner");

            bloomIntensityVar = Effect.GetVariableScalar("gBloomIntensity");

            toneMappingToneVar = Effect.GetVariableScalar("gToneMappingTone");
        }

        /// <summary>
        /// Gets the specified effect technique
        /// </summary>
        /// <param name="effect">Effect enum</param>
        public EngineEffectTechnique GetTechnique(PostProcessingEffects effect)
        {
            EngineEffectTechnique technique;
            switch (effect)
            {
                case PostProcessingEffects.Grayscale:
                    technique = DrawerPool.EffectPostProcess.Grayscale;
                    break;
                case PostProcessingEffects.Sepia:
                    technique = DrawerPool.EffectPostProcess.Sepia;
                    break;
                case PostProcessingEffects.Vignette:
                    technique = DrawerPool.EffectPostProcess.Vignette;
                    break;
                case PostProcessingEffects.Blur:
                    technique = DrawerPool.EffectPostProcess.Blur;
                    break;
                case PostProcessingEffects.BlurVignette:
                    technique = DrawerPool.EffectPostProcess.BlurVignette;
                    break;
                case PostProcessingEffects.Bloom:
                    technique = DrawerPool.EffectPostProcess.Bloom;
                    break;
                case PostProcessingEffects.Grain:
                    technique = DrawerPool.EffectPostProcess.Grain;
                    break;
                case PostProcessingEffects.ToneMapping:
                    technique = DrawerPool.EffectPostProcess.ToneMapping;
                    break;
                default:
                    technique = DrawerPool.EffectPostProcess.Empty;
                    break;
            }

            return technique;
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="viewportSize">Viewport size</param>
        /// <param name="time">Time</param>
        /// <param name="texture">Texture</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            Vector2 viewportSize,
            float time,
            EngineShaderResourceView texture)
        {
            WorldViewProjection = viewProjection;
            TextureSize = viewportSize;
            DiffuseMap = texture;
            Time = time;
        }
        /// <summary>
        /// Update effect parameters
        /// </summary>
        /// <typeparam name="T">Type of parameter</typeparam>
        /// <param name="parameters">Parameters</param>
        public void UpdatePerEffect<T>(T parameters) where T : IDrawerPostProcessParams
        {
            if (parameters == null)
            {
                return;
            }

            EffectIntensity = parameters.EffectIntensity;

            if (parameters is PostProcessVignetteParams vignette)
            {
                VignetteOuter = vignette.Outer;
                VignetteInner = vignette.Inner;
            }
            else if (parameters is PostProcessBlurParams blur)
            {
                BlurDirections = blur.Directions;
                BlurQuality = blur.Quality;
                BlurSize = blur.Size;
            }
            else if (parameters is PostProcessBlurVignetteParams blurVignette)
            {
                BlurDirections = blurVignette.Directions;
                BlurQuality = blurVignette.Quality;
                BlurSize = blurVignette.Size;
                VignetteOuter = blurVignette.Outer;
                VignetteInner = blurVignette.Inner;
            }
            else if (parameters is PostProcessBloomParams bloom)
            {
                BloomIntensity = bloom.Intensity;
                BlurDirections = bloom.Directions;
                BlurQuality = bloom.Quality;
                BlurSize = bloom.Size;
            }
            else if (parameters is PostProcessToneMappingParams toneMapping)
            {
                ToneMappingTone = (uint)toneMapping.Tone;
            }
        }
    }
}
