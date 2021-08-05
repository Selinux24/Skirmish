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
        None,
        /// <summary>
        /// Vertical scroll
        /// </summary>
        Vertical,
        /// <summary>
        /// Horizontal scroll
        /// </summary>
        Horizontal,
    }
}
