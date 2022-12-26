using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Input interface
    /// </summary>
    public interface IInput : IDisposable
    {
        /// <summary>
        /// Gets if left or right control key were pressed now
        /// </summary>
        bool ControlPressed { get; }
        /// <summary>
        /// Elapsed time
        /// </summary>
        float Elapsed { get; set; }
        /// <summary>
        /// Current just pressed mouse buttons
        /// </summary>
        MouseButtons JustPressedMouseButtons { get; }
        /// <summary>
        /// Current just released mouse buttons
        /// </summary>
        MouseButtons JustReleasedMouseButtons { get; }
        /// <summary>
        /// Sets mouse on center after update
        /// </summary>
        bool LockMouse { get; set; }
        /// <summary>
        /// Mouse button state
        /// </summary>
        MouseButtons MouseButtonsState { get; }
        /// <summary>
        /// Absolute Mouse position
        /// </summary>
        Point MousePosition { get; }
        /// <summary>
        /// Mouse wheel value
        /// </summary>
        int MouseWheelDelta { get; }
        /// <summary>
        /// Absolute Mouse X axis value
        /// </summary>
        int MouseX { get; }
        /// <summary>
        /// Mouse X axis value
        /// </summary>
        int MouseXDelta { get; }
        /// <summary>
        /// Absolute Mouse Y axis value
        /// </summary>
        int MouseY { get; }
        /// <summary>
        /// Mouse Y axis value
        /// </summary>
        int MouseYDelta { get; }
        /// <summary>
        /// Current pressed mouse buttons
        /// </summary>
        MouseButtons PressedMouseButtons { get; }
        /// <summary>
        /// Gets if left or right shift key were pressed now
        /// </summary>
        bool ShiftPressed { get; }
        /// <summary>
        /// Sets cursor visible
        /// </summary>
        bool VisibleMouse { get; set; }

        /// <summary>
        /// Gets the just pressed key list
        /// </summary>
        /// <returns>Returns an array of just pressed keys</returns>
        Keys[] GetJustPressedKeys();
        /// <summary>
        /// Gets the just released key list
        /// </summary>
        /// <returns>Returns an array of just released keys</returns>
        Keys[] GetJustReleasedKeys();
        /// <summary>
        /// Gets if specified key is just pressed
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the specified key is just pressed</returns>
        bool KeyJustPressed(Keys key);
        /// <summary>
        /// Gets if specified key is just released
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the specified key is just released</returns>
        bool KeyJustReleased(Keys key);
        /// <summary>
        /// Gets if specified key is pressed now
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the specified key is pressed now</returns>
        bool KeyPressed(Keys key);
        /// <summary>
        /// Gets if the specified mouse button is just pressed
        /// </summary>
        /// <param name="button">Mouse button</param>
        /// <returns>Returns true if the specified mouse button is just pressed</returns>
        bool MouseButtonJustPressed(MouseButtons button);
        /// <summary>
        /// Gets if the specified mouse button is just released
        /// </summary>
        /// <param name="button">Mouse button</param>
        /// <returns>Returns true if the specified mouse button is just released</returns>
        bool MouseButtonJustReleased(MouseButtons button);
        /// <summary>
        /// Gets if the specified mouse button is pressed
        /// </summary>
        /// <param name="button">Mouse button</param>
        /// <returns>Returns true if the specified mouse button is pressed</returns>
        bool MouseButtonPressed(MouseButtons button);
        /// <summary>
        /// Sets the engine form
        /// </summary>
        /// <param name="form">Engine form</param>
        void SetForm(IEngineForm form);
        /// <summary>
        /// Sets mouse position
        /// </summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        void SetMousePosition(int x, int y);
        /// <summary>
        /// Sets mouse position
        /// </summary>
        /// <param name="location">Position</param>
        void SetMousePosition(Point location);
        /// <summary>
        /// Updates input state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Update(GameTime gameTime);
        /// <summary>
        /// Gets the keyboard key strokes
        /// </summary>
        /// <returns>Returns the stroked key strings</returns>
        string GetStrokes();
    }
}