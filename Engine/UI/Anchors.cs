using System;

namespace Engine.UI
{
    /// <summary>
    /// UIControl anchors
    /// </summary>
    [Flags]
    public enum Anchors
    {
        /// <summary>
        /// None specified
        /// </summary>
        None = 0,
        /// <summary>
        /// Top
        /// </summary>
        Top = 1,
        /// <summary>
        /// Bottom
        /// </summary>
        Bottom = 2,
        /// <summary>
        /// Left
        /// </summary>
        Left = 4,
        /// <summary>
        /// Right
        /// </summary>
        Right = 8,
        /// <summary>
        /// Horizontal center
        /// </summary>
        HorizontalCenter = 16,
        /// <summary>
        /// Vertical center
        /// </summary>
        VerticalCenter = 32,
        /// <summary>
        /// Top left
        /// </summary>
        TopLeft = Top | Left,
        /// <summary>
        /// Top right
        /// </summary>
        TopRight = Top | Right,
        /// <summary>
        /// Top center
        /// </summary>
        TopCenter = Top | HorizontalCenter,
        /// <summary>
        /// Bottom left
        /// </summary>
        BottomLeft = Bottom | Left,
        /// <summary>
        /// Bottom right
        /// </summary>
        BottomRight = Bottom | Right,
        /// <summary>
        /// Bottom center
        /// </summary>
        BottomCenter = Bottom | HorizontalCenter,
        /// <summary>
        /// Center
        /// </summary>
        Center = HorizontalCenter | VerticalCenter,
        /// <summary>
        /// Center left
        /// </summary>
        CenterLeft = Left | VerticalCenter,
        /// <summary>
        /// Center right
        /// </summary>
        CenterRight = Right | VerticalCenter,
    }
}
