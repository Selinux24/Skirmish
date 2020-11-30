using System;

namespace Engine
{
    [Flags]
    public enum MouseButtons
    {
        /// <summary>
        /// None
        /// </summary>
        None = System.Windows.Forms.MouseButtons.None,
        /// <summary>
        /// Left mouse button
        /// </summary>
        Left = System.Windows.Forms.MouseButtons.Left,
        /// <summary>
        /// Right mouse button
        /// </summary>
        Right = System.Windows.Forms.MouseButtons.Right,
        /// <summary>
        /// Middle mouse button
        /// </summary>
        Middle = System.Windows.Forms.MouseButtons.Middle,
        /// <summary>
        /// XButton1
        /// </summary>
        XButton1 = System.Windows.Forms.MouseButtons.XButton1,
        /// <summary>
        /// XButton2
        /// </summary>
        XButton2 = System.Windows.Forms.MouseButtons.XButton2,
    }
}
