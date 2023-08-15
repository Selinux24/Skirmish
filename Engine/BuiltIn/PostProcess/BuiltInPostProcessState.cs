using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn.PostProcess
{
    /// <summary>
    /// Built-in post process drawer state
    /// </summary>
    public class BuiltInPostProcessState
    {
        /// <summary>
        /// Max active effects
        /// </summary>
        private const int MaxEffects = 8;

        /// <summary>
        /// Gets the empty effect
        /// </summary>
        public static BuiltInPostProcessState Empty
        {
            get
            {
                return new BuiltInPostProcessState();
            }
        }

        /// <summary>
        /// Effect list
        /// </summary>
        private readonly List<BuiltInPostProcessEffects> effects = new();

        /// <summary>
        /// Gets the effect list
        /// </summary>
        public IEnumerable<BuiltInPostProcessEffects> Effects
        {
            get
            {
                return effects.ToArray();
            }
        }

        /// <summary>
        /// Gray scale intensity
        /// </summary>
        public float GrayscaleIntensity { get; set; }

        /// <summary>
        /// Sepia intensity
        /// </summary>
        public float SepiaIntensity { get; set; }

        /// <summary>
        /// Grain intensity
        /// </summary>
        public float GrainIntensity { get; set; }

        /// <summary>
        /// Blur intensity
        /// </summary>
        public float BlurIntensity { get; set; }
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
        /// Vignette intensity
        /// </summary>
        public float VignetteIntensity { get; set; }
        /// <summary>
        /// Vignette outer
        /// </summary>
        public float VignetteOuter { get; set; }
        /// <summary>
        /// Vignette inner
        /// </summary>
        public float VignetteInner { get; set; }

        /// <summary>
        /// Blur & Vignette intensity
        /// </summary>
        public float BlurVignetteIntensity { get; set; }
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
        /// Bloom force
        /// </summary>
        public float BloomForce { get; set; }
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
        /// Tone mapping intensity
        /// </summary>
        public float ToneMappingIntensity { get; set; }
        /// <summary>
        /// Tone mapping tone
        /// </summary>
        public BuiltInToneMappingTones ToneMappingTone { get; set; }
        /// <summary>
        /// Gets whether the state has configured effects or not
        /// </summary>
        public bool Ready
        {
            get
            {
                return Effects?.Any() == true;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPostProcessState()
        {

        }

        /// <summary>
        /// Gets the effect list
        /// </summary>
        public IEnumerable<BuiltInPostProcessEffects> GetEffects()
        {
            return effects.Where(e => e != BuiltInPostProcessEffects.None).ToArray();
        }

        /// <summary>
        /// Adds an effect to the effect collection
        /// </summary>
        /// <param name="effect">Effects</param>
        private bool AddEffect(BuiltInPostProcessEffects effect)
        {
            if (effects.Count >= MaxEffects)
            {
                return false;
            }

            if (effects.Contains(effect))
            {
                return true;
            }

            effects.Add(effect);

            return true;
        }

        /// <summary>
        /// Adds the gray scale effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddGrayScale(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Grayscale))
            {
                return;
            }

            GrayscaleIntensity = intensity;
        }
        /// <summary>
        /// Adds the sepia effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddSepia(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Sepia))
            {
                return;
            }

            SepiaIntensity = intensity;
        }
        /// <summary>
        /// Adds the grain effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddGrain(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Grain))
            {
                return;
            }

            GrainIntensity = intensity;
        }
        /// <summary>
        /// Adds the blur effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddBlur(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Blur))
            {
                return;
            }

            BlurIntensity = intensity;
            BlurDirections = 16;
            BlurQuality = 3;
            BlurSize = 4;
        }
        /// <summary>
        /// Adds the strong blur effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddBlurStrong(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Blur))
            {
                return;
            }

            BlurIntensity = intensity;
            BlurDirections = 16;
            BlurQuality = 3;
            BlurSize = 8;
        }
        /// <summary>
        /// Adds the vignette effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddVignette(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Vignette))
            {
                return;
            }

            VignetteIntensity = intensity;
            VignetteOuter = 1f;
            VignetteInner = 0.05f;
        }
        /// <summary>
        /// Adds the thin vignette effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddVignetteThin(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Vignette))
            {
                return;
            }

            VignetteIntensity = intensity;
            VignetteOuter = 1f;
            VignetteInner = 0.66f;
        }
        /// <summary>
        /// Adds the strong vignette effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddVignetteStrong(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Vignette))
            {
                return;
            }

            VignetteIntensity = intensity;
            VignetteOuter = 0.5f;
            VignetteInner = 0.1f;
        }
        /// <summary>
        /// Adds the blur & vignette effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddBlurVignette(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.BlurVignette))
            {
                return;
            }

            BlurVignetteIntensity = intensity;
            BlurVignetteDirections = 16;
            BlurVignetteQuality = 3;
            BlurVignetteSize = 4;
            BlurVignetteOuter = 1f;
            BlurVignetteInner = 0.05f;
        }
        /// <summary>
        /// Adds the strong blur & vignette effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddBlurVignetteStrong(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.BlurVignette))
            {
                return;
            }

            BlurVignetteIntensity = intensity;
            BlurVignetteDirections = 16;
            BlurVignetteQuality = 3;
            BlurVignetteSize = 8;
            BlurVignetteOuter = 1f;
            BlurVignetteInner = 0.05f;
        }
        /// <summary>
        /// Adds the bloom effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddBloom(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Bloom))
            {
                return;
            }

            BloomIntensity = intensity;
            BloomForce = 0.25f;
            BloomDirections = 16;
            BloomQuality = 3;
            BloomSize = 4;
        }
        /// <summary>
        /// Adds the low bloom effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddBloomLow(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Bloom))
            {
                return;
            }

            BloomIntensity = intensity;
            BloomForce = 0.15f;
            BloomDirections = 16;
            BloomQuality = 3;
            BloomSize = 4;
        }
        /// <summary>
        /// Adds the high bloom effect
        /// </summary>
        /// <param name="intensity">Intensity</param>
        public void AddBloomHigh(float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.Bloom))
            {
                return;
            }

            BloomIntensity = intensity;
            BloomForce = 0.35f;
            BloomDirections = 16;
            BloomQuality = 3;
            BloomSize = 4;
        }
        /// <summary>
        /// Adds the tone-mapping effect
        /// </summary>
        /// <param name="tone">Tone-mapping</param>
        /// <param name="intensity">Intensity</param>
        public void AddToneMapping(BuiltInToneMappingTones tone, float intensity = 1f)
        {
            if (!AddEffect(BuiltInPostProcessEffects.ToneMapping))
            {
                return;
            }

            ToneMappingIntensity = intensity;
            ToneMappingTone = tone;
        }

        /// <summary>
        /// Removes the specified effect type
        /// </summary>
        /// <param name="effect">Effect</param>
        public void RemoveEffect(BuiltInPostProcessEffects effect)
        {
            if (!effects.Contains(effect))
            {
                return;
            }

            effects.Remove(effect);
        }
        /// <summary>
        /// Removes the gray scale effect
        /// </summary>
        public void RemoveGrayscale()
        {
            RemoveEffect(BuiltInPostProcessEffects.Grayscale);
        }
        /// <summary>
        /// Removes the speia effect
        /// </summary>
        public void RemoveSepia()
        {
            RemoveEffect(BuiltInPostProcessEffects.Sepia);
        }
        /// <summary>
        /// Removes the grain effect
        /// </summary>
        public void RemoveGrain()
        {
            RemoveEffect(BuiltInPostProcessEffects.Grain);
        }
        /// <summary>
        /// Removes the blur effect
        /// </summary>
        public void RemoveBlur()
        {
            RemoveEffect(BuiltInPostProcessEffects.Blur);
        }
        /// <summary>
        /// Removes the vignette effect
        /// </summary>
        public void RemoveVignette()
        {
            RemoveEffect(BuiltInPostProcessEffects.Vignette);
        }
        /// <summary>
        /// Removes the blur & vignette effect
        /// </summary>
        public void RemoveBlurVignette()
        {
            RemoveEffect(BuiltInPostProcessEffects.BlurVignette);
        }
        /// <summary>
        /// Removes the bloom effect
        /// </summary>
        public void RemoveBloom()
        {
            RemoveEffect(BuiltInPostProcessEffects.Bloom);
        }
        /// <summary>
        /// Removes the tone-mapping effect
        /// </summary>
        public void RemoveToneMapping()
        {
            RemoveEffect(BuiltInPostProcessEffects.ToneMapping);
        }
    }
}
