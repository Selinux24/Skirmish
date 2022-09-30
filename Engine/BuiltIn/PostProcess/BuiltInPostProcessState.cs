
namespace Engine.BuiltIn.PostProcess
{
    /// <summary>
    /// Built-in post process drawer state
    /// </summary>
    public struct BuiltInPostProcessState
    {
        public static readonly BuiltInPostProcessState Empty = new BuiltInPostProcessState();

        private static bool SetState(BuiltInPostProcessState sourceState, BuiltInPostProcessEffects effect, float intensity, out BuiltInPostProcessState newState)
        {
            newState = new BuiltInPostProcessState(sourceState);

            if (sourceState.Effect1 == BuiltInPostProcessEffects.None && sourceState.Effect1 != effect)
            {
                newState.Effect1 = effect;
                newState.Effect1Intensity = intensity;
                return true;
            }
            else if (sourceState.Effect2 == BuiltInPostProcessEffects.None && sourceState.Effect2 != effect)
            {
                newState.Effect2 = effect;
                newState.Effect2Intensity = intensity;
                return true;
            }
            else if (sourceState.Effect3 == BuiltInPostProcessEffects.None && sourceState.Effect3 != effect)
            {
                newState.Effect3 = effect;
                newState.Effect3Intensity = intensity;
                return true;
            }
            else if (sourceState.Effect4 == BuiltInPostProcessEffects.None && sourceState.Effect4 != effect)
            {
                newState.Effect4 = effect;
                newState.Effect4Intensity = intensity;
                return true;
            }

            return false;
        }

        public BuiltInPostProcessState AddGrayScale()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Grayscale, 1, out var newState))
            {
                return this;
            }

            return newState;
        }

        public BuiltInPostProcessState AddSepia()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Sepia, 1, out var newState))
            {
                return this;
            }

            return newState;
        }

        public BuiltInPostProcessState AddGrain()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Grain, 1, out var newState))
            {
                return this;
            }

            return newState;
        }

        public BuiltInPostProcessState AddBlur()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Blur, 1, out var newState))
            {
                return this;
            }

            newState.BlurDirections = 16;
            newState.BlurQuality = 3;
            newState.BlurSize = 4;

            return newState;
        }
        public BuiltInPostProcessState AddBlurStrong()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Blur, 1, out var newState))
            {
                return this;
            }

            newState.BlurDirections = 16;
            newState.BlurQuality = 3;
            newState.BlurSize = 8;

            return newState;
        }

        public BuiltInPostProcessState AddVignette()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Vignette, 1, out var newState))
            {
                return this;
            }

            newState.VignetteOuter = 1f;
            newState.VignetteInner = 0.05f;

            return newState;
        }
        public BuiltInPostProcessState AddVignetteThin()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Vignette, 1, out var newState))
            {
                return this;
            }

            newState.VignetteOuter = 1f;
            newState.VignetteInner = 0.66f;

            return newState;
        }
        public BuiltInPostProcessState AddVignetteStrong()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Vignette, 1, out var newState))
            {
                return this;
            }

            newState.VignetteOuter = 0.5f;
            newState.VignetteInner = 0.1f;

            return newState;
        }

        public BuiltInPostProcessState AddBlurVignette()
        {
            if (!SetState(this, BuiltInPostProcessEffects.BlurVignette, 1, out var newState))
            {
                return this;
            }

            newState.BlurVignetteDirections = 16;
            newState.BlurVignetteQuality = 3;
            newState.BlurVignetteSize = 4;
            newState.BlurVignetteOuter = 1f;
            newState.BlurVignetteInner = 0.05f;

            return newState;
        }
        public BuiltInPostProcessState AddBlurVignetteStrong()
        {
            if (!SetState(this, BuiltInPostProcessEffects.BlurVignette, 1, out var newState))
            {
                return this;
            }

            newState.BlurVignetteDirections = 16;
            newState.BlurVignetteQuality = 3;
            newState.BlurVignetteSize = 8;
            newState.BlurVignetteOuter = 1f;
            newState.BlurVignetteInner = 0.05f;

            return newState;
        }

        public BuiltInPostProcessState AddBloom()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Bloom, 1, out var newState))
            {
                return this;
            }

            newState.BloomIntensity = 0.25f;
            newState.BloomDirections = 16;
            newState.BloomQuality = 3;
            newState.BloomSize = 4;

            return newState;
        }
        public BuiltInPostProcessState AddBloomLow()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Bloom, 1, out var newState))
            {
                return this;
            }

            newState.BloomIntensity = 0.15f;
            newState.BloomDirections = 16;
            newState.BloomQuality = 3;
            newState.BloomSize = 4;

            return newState;
        }
        public BuiltInPostProcessState AddBloomHigh()
        {
            if (!SetState(this, BuiltInPostProcessEffects.Bloom, 1, out var newState))
            {
                return this;
            }

            newState.BloomIntensity = 0.35f;
            newState.BloomDirections = 16;
            newState.BloomQuality = 3;
            newState.BloomSize = 4;

            return newState;
        }

        public BuiltInPostProcessState AddToneMapping(BuiltInToneMappingTones tone)
        {
            if (!SetState(this, BuiltInPostProcessEffects.ToneMapping, 1, out var newState))
            {
                return this;
            }

            newState.ToneMappingTone = tone;

            return newState;
        }

        /// <summary>
        /// First effect
        /// </summary>
        public BuiltInPostProcessEffects Effect1 { get; set; }
        /// <summary>
        /// Second effect
        /// </summary>
        public BuiltInPostProcessEffects Effect2 { get; set; }
        /// <summary>
        /// Third effect
        /// </summary>
        public BuiltInPostProcessEffects Effect3 { get; set; }
        /// <summary>
        /// Fourth effect
        /// </summary>
        public BuiltInPostProcessEffects Effect4 { get; set; }

        /// <summary>
        /// Fourth 1 effect intensity
        /// </summary>
        public float Effect1Intensity { get; set; }
        /// <summary>
        /// Fourth 2 effect intensity
        /// </summary>
        public float Effect2Intensity { get; set; }
        /// <summary>
        /// Fourth 3 effect intensity
        /// </summary>
        public float Effect3Intensity { get; set; }
        /// <summary>
        /// Fourth 4 effect intensity
        /// </summary>
        public float Effect4Intensity { get; set; }

        /// <summary>
        /// Blur directions
        /// </summary>
        public float BlurDirections { get; set; }
        /// <summary>
        /// Blur quality
        /// </summary>
        public float BlurQuality { get; set; }
        /// <summary>
        /// Blur size
        /// </summary>
        public float BlurSize { get; set; }

        /// <summary>
        /// Vignette outer
        /// </summary>
        public float VignetteOuter { get; set; }
        /// <summary>
        /// Vignette inner
        /// </summary>
        public float VignetteInner { get; set; }

        /// <summary>
        /// Blur & Vignette directions
        /// </summary>
        public float BlurVignetteDirections { get; set; }
        /// <summary>
        /// Blur & Vignette quality
        /// </summary>
        public float BlurVignetteQuality { get; set; }
        /// <summary>
        /// Blur & Vignette size
        /// </summary>
        public float BlurVignetteSize { get; set; }
        /// <summary>
        /// Blur & Vignette outer
        /// </summary>
        public float BlurVignetteOuter { get; set; }
        /// <summary>
        /// Blur & Vignette inner
        /// </summary>
        public float BlurVignetteInner { get; set; }

        /// <summary>
        /// Bloom intensity
        /// </summary>
        public float BloomIntensity { get; set; }
        /// <summary>
        /// Bloom directions
        /// </summary>
        public float BloomDirections { get; set; }
        /// <summary>
        /// Bloom quality
        /// </summary>
        public float BloomQuality { get; set; }
        /// <summary>
        /// Bloom size
        /// </summary>
        public float BloomSize { get; set; }

        /// <summary>
        /// Tone mapping tone
        /// </summary>
        public BuiltInToneMappingTones ToneMappingTone { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">State</param>
        public BuiltInPostProcessState(BuiltInPostProcessState state)
        {
            Effect1 = state.Effect1;
            Effect2 = state.Effect2;
            Effect3 = state.Effect3;
            Effect4 = state.Effect4;

            Effect1Intensity = state.Effect1Intensity;
            Effect2Intensity = state.Effect2Intensity;
            Effect3Intensity = state.Effect3Intensity;
            Effect4Intensity = state.Effect4Intensity;

            BlurDirections = state.BlurDirections;
            BlurQuality = state.BlurQuality;
            BlurSize = state.BlurSize;

            VignetteOuter = state.VignetteOuter;
            VignetteInner = state.VignetteInner;

            BlurVignetteDirections = state.BlurVignetteDirections;
            BlurVignetteQuality = state.BlurVignetteQuality;
            BlurVignetteSize = state.BlurVignetteSize;
            BlurVignetteOuter = state.BlurVignetteOuter;
            BlurVignetteInner = state.BlurVignetteInner;

            BloomIntensity = state.BloomIntensity;
            BloomDirections = state.BloomDirections;
            BloomQuality = state.BloomQuality;
            BloomSize = state.BloomSize;

            ToneMappingTone = state.ToneMappingTone;
        }
    }
}
