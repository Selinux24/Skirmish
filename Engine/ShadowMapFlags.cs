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
        /// Low definition shadow map
        /// </summary>
        LowDefinition = 1,
        /// <summary>
        /// High definition shadow map
        /// </summary>
        HighDefinition = 2,
    }
}
