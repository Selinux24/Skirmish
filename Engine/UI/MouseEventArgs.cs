using SharpDX;
using System;

namespace Engine.UI
{
    /// <summary>
    /// Mouse event arguments
    /// </summary>
    public class MouseEventArgs : EventArgs
    {
        /// <summary>
        /// Pointer position
        /// </summary>
        public Point PointerPosition { get; set; }
        /// <summary>
        /// Mouse buttons
        /// </summary>
        public MouseButtons Buttons { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseEventArgs() : base()
        {

        }
    }
}
