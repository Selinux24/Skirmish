using System;

namespace Engine
{
    /// <summary>
    /// Model uses
    /// </summary>
    [Flags]
    public enum AttachedModelUsesEnum : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Full triangle list for picking
        /// </summary>
        FullPicking = 0x1,
        /// <summary>
        /// Coarse list for picking
        /// </summary>
        CoarsePicking = 0x2,
        /// <summary>
        /// Full triangle list for path finding
        /// </summary>
        FullPathFinding = 0x4,
        /// <summary>
        /// Coarse list for path finding
        /// </summary>
        CoarsePathFinding = 0x8,
    }
}
