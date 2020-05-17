using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Control interface
    /// </summary>
    public interface IUIControl
    {
        /// <summary>
        /// Click event
        /// </summary>
        event EventHandler Click;

        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        int Left { get; set; }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        int Top { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        int Height { get; set; }
        /// <summary>
        /// Control rectangle
        /// </summary>
        Rectangle Rectangle { get; }
        /// <summary>
        /// Absolute center
        /// </summary>
        Vector2 AbsoluteCenter { get; }
        /// <summary>
        /// Relative center
        /// </summary>
        Vector2 RelativeCenter { get; }
        /// <summary>
        /// Indicates whether the sprite has to maintain proportion with window size
        /// </summary>
        bool FitParent { get; set; }
        /// <summary>
        /// Base color
        /// </summary>
        Color4 Color { get; set; }
        /// <summary>
        /// Alpha color component
        /// </summary>
        float Alpha { get; set; }
        /// <summary>
        /// Scale
        /// </summary>
        float Scale { get; set; }
        /// <summary>
        /// Rotation
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Mouser is over control
        /// </summary>
        bool MouseOver { get; set; }
        /// <summary>
        /// Control is pressed
        /// </summary>
        bool Pressed { get; set; }
        /// <summary>
        /// Control is just pressed
        /// </summary>
        bool JustPressed { get; }
        /// <summary>
        /// Control is just released
        /// </summary>
        bool JustReleased { get; }

        /// <summary>
        /// Fires on-click event
        /// </summary>
        void FireOnClickEvent();
    }
}
