using System;

namespace Engine
{
    /// <summary>
    /// Mouse buttons
    /// </summary>
    [Flags]
    public enum MouseButtons
    {
        /// <summary>
        ///  No mouse button was pressed.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        ///  The left mouse button was pressed.
        /// </summary>
        Left = 0x00100000,
        /// <summary>
        ///  The right mouse button was pressed.
        /// </summary>
        Right = 0x00200000,
        /// <summary>
        ///  The middle mouse button was pressed.
        /// </summary>
        Middle = 0x00400000,
        /// <summary>
        /// X button 1
        /// </summary>
        XButton1 = 0x00800000,
        /// <summary>
        /// X button 2
        /// </summary>
        XButton2 = 0x01000000,
    }
}
