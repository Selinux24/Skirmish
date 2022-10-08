using System;

namespace Engine.UI
{
    /// <summary>
    /// Scroll modes
    /// </summary>
    [Flags]
    public enum ScrollModes
    {
        /// <summary>
        /// No scroll
        /// </summary>
        None = 0,
        /// <summary>
        /// Vertical scroll
        /// </summary>
        Vertical = 1,
        /// <summary>
        /// Horizontal scroll
        /// </summary>
        Horizontal = 2,
        /// <summary>
        /// Both scrolls
        /// </summary>
        Both = Vertical | Horizontal,
    }
}
