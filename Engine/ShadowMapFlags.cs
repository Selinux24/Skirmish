using System;

namespace Engine
{
    /// <summary>
    /// Flags
    /// </summary>
    [Flags]
    public enum ShadowMapFlags : uint
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Directional shadow map
        /// </summary>
        One = 1,
        /// <summary>
        /// Omnidirectional shadow map
        /// </summary>
        Two = 2,

        Three = 4,
    }
}
