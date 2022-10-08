using System;

namespace Engine.UI
{
    /// <summary>
    /// UIControl pivot anchors
    /// </summary>
    [Flags]
    public enum PivotAnchors
    {
        /// <summary>
        /// None specified
        /// </summary>
        None = 0,
        /// <summary>
        /// Pivot over the center
        /// </summary>
        Center = 1,
        /// <summary>
        /// Pivot over the top left corner
        /// </summary>
        TopLeft = 2,
        /// <summary>
        /// Pivot over the top right corner
        /// </summary>
        TopRight = 4,
        /// <summary>
        /// Pivot over the bottom left corner
        /// </summary>
        BottomLeft = 8,
        /// <summary>
        /// Pivot over the bottom right corner
        /// </summary>
        BottomRight = 16,
        /// <summary>
        /// Use the parent as pivot control
        /// </summary>
        Parent = 32,
        /// <summary>
        /// Use the root as pivot control
        /// </summary>
        Root = 64,
        /// <summary>
        /// Default pivot anchor - Use the root's center
        /// </summary>
        Default = Root | Center,
    }
}
