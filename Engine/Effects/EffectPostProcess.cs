﻿using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

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
        /// Blur drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Blur = null;
        /// <summary>
        /// Vignette drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Vignette = null;
        /// <summary>
        /// Bloom drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Bloom = null;
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
        /// Bloom blur size ring effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar bloomBlurSizeVar = null;

        /// <summary>
        /// Tone mapping tone
        /// </summary>
        private readonly EngineEffectVariableScalar toneMappingToneVar = null;

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
        /// Bloom blur size
        /// </summary>
        protected float BloomBlurSize
        {
            get
            {
                return bloomBlurSizeVar.GetFloat();
            }
            set
            {
                bloomBlurSizeVar.Set(value);
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
            Blur = Effect.GetTechniqueByName("Blur");
            Vignette = Effect.GetTechniqueByName("Vignette");
            Bloom = Effect.GetTechniqueByName("Bloom");
            ToneMapping = Effect.GetTechniqueByName("ToneMapping");

            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            textureSizeVar = Effect.GetVariableVector("gTextureSize");
            diffuseMapVar = Effect.GetVariableTexture("gDiffuseMap");

            blurDirectionsVar = Effect.GetVariableScalar("gBlurDirections");
            blurQualityVar = Effect.GetVariableScalar("gBlurQuality");
            blurSizeVar = Effect.GetVariableScalar("gBlurSize");

            vignetteOuterVar = Effect.GetVariableScalar("gVignetteOuter");
            vignetteInnerVar = Effect.GetVariableScalar("gVignetteInner");

            bloomIntensityVar = Effect.GetVariableScalar("gBloomIntensity");
            bloomBlurSizeVar = Effect.GetVariableScalar("gBloomBlurSize");

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
                case PostProcessingEffects.Blur:
                    technique = DrawerPool.EffectPostProcess.Blur;
                    break;
                case PostProcessingEffects.Vignette:
                    technique = DrawerPool.EffectPostProcess.Vignette;
                    break;
                case PostProcessingEffects.Bloom:
                    technique = DrawerPool.EffectPostProcess.Bloom;
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
        /// <param name="texture">Texture</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            Vector2 viewportSize,
            EngineShaderResourceView texture)
        {
            WorldViewProjection = viewProjection;
            TextureSize = viewportSize;
            DiffuseMap = texture;
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

            if (parameters is PostProcessBlurParams blur)
            {
                BlurDirections = blur.Directions;
                BlurQuality = blur.Quality;
                BlurSize = blur.Size;
            }
            else if (parameters is PostProcessVignetteParams vignette)
            {
                VignetteOuter = vignette.Outer;
                VignetteInner = vignette.Inner;
            }
            else if (parameters is PostProcessBloomParams bloom)
            {
                BloomIntensity = bloom.Intensity;
                BloomBlurSize = bloom.BlurSize;
            }
            else if (parameters is PostProcessToneMappingParams toneMapping)
            {
                ToneMappingTone = (uint)toneMapping.Tone;
            }
        }
    }
}