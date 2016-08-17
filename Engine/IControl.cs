using System;
using SharpDX;

namespace Engine
{
    /// <summary>
    /// Control interface
    /// </summary>
    public interface IControl
    {
        /// <summary>
        /// Click event
        /// </summary>
        event EventHandler Click;

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
        /// Control rectangle
        /// </summary>
        Rectangle Rectangle { get; }

        /// <summary>
        /// Fire clicked event
        /// </summary>
        void FireOnClickEvent();
    }
}
