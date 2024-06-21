
namespace Engine.BuiltIn.Drawers.PostProcess
{
    /// <summary>
    /// Post processing effects
    /// </summary>
    public enum BuiltInPostProcessEffects : uint
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Gray scale
        /// </summary>
        Grayscale = 1,
        /// <summary>
        /// Sepia
        /// </summary>
        Sepia = 2,
        /// <summary>
        /// Vignette
        /// </summary>
        Vignette = 3,
        /// <summary>
        /// Blur
        /// </summary>
        Blur = 4,
        /// <summary>
        /// Blur + Vignette
        /// </summary>
        BlurVignette = 5,
        /// <summary>
        /// Bloom
        /// </summary>
        Bloom = 6,
        /// <summary>
        /// Grain
        /// </summary>
        Grain = 7,
        /// <summary>
        /// Tone mapping
        /// </summary>
        ToneMapping = 8,
    }
}
