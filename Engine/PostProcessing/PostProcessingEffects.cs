﻿
namespace Engine.PostProcessing
{
    /// <summary>
    /// Post processing effects
    /// </summary>
    public enum PostProcessingEffects
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Combines two textures
        /// </summary>
        Combine,
        /// <summary>
        /// Gray scale
        /// </summary>
        Grayscale,
        /// <summary>
        /// Sepia
        /// </summary>
        Sepia,
        /// <summary>
        /// Vignette
        /// </summary>
        Vignette,
        /// <summary>
        /// Blur
        /// </summary>
        Blur,
        /// <summary>
        /// Blur + Vignette
        /// </summary>
        BlurVignette,
        /// <summary>
        /// Bloom
        /// </summary>
        Bloom,
        /// <summary>
        /// Grain
        /// </summary>
        Grain,
        /// <summary>
        /// Tone mapping
        /// </summary>
        ToneMapping,
    }
}