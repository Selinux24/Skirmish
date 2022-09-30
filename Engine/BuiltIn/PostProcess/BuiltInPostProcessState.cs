using SharpDX;

namespace Engine.BuiltIn.PostProcess
{
    using Engine.Common;

    /// <summary>
    /// Built-in post process drawer state
    /// </summary>
    public struct BuiltInPostProcessState
    {
        /// <summary>
        /// Render target texture
        /// </summary>
        public EngineShaderResourceView RenderTargetTexture { get; set; }

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
        /// Intensity
        /// </summary>
        public float EffectIntensity { get; set; }
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
        /// Bloom intensity
        /// </summary>
        public float BloomIntensity { get; set; }
        /// <summary>
        /// Tone mapping tone
        /// </summary>
        public BuiltInToneMappingTones ToneMappingTone { get; set; }
    }
}
