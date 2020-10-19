using System;

namespace Engine
{
    /// <summary>
    /// Blending modes
    /// </summary>
    [Flags]
    public enum BlendModes
    {
        /// <summary>
        /// Not specified
        /// </summary>
        None = 0,
        /// <summary>
        /// Opaque
        /// </summary>
        Opaque = 1,
        /// <summary>
        /// Alpha blend
        /// </summary>
        Alpha = 2,
        /// <summary>
        /// Transparent blend
        /// </summary>
        Transparent = 4,
        /// <summary>
        /// Additive blend
        /// </summary>
        Additive = 8,
        /// <summary>
        /// Default blend mode
        /// </summary>
        Default = Opaque | Alpha,
        /// <summary>
        /// Default transparent
        /// </summary>
        DefaultTransparent = Opaque | Transparent,
    }
}
