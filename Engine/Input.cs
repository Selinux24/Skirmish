using System;
using System.Drawing;
using System.Windows.Forms;
using SharpDX.DirectInput;

namespace Engine
{
    /// <summary>
    /// Mouse and keyboard input
    /// </summary>
    public class Input : IDisposable
    {
        /// <summary>
        /// Input
        /// </summary>
        private DirectInput input = null;
        /// <summary>
        /// Mouse
        /// </summary>
        private Mouse mouse = null;
        /// <summary>
        /// Keyboard
        /// </summary>
        private Keyboard keyboard = null;
        /// <summary>
        /// Previous mouse state
        /// </summary>
        private MouseState previousMouseState;
        /// <summary>
        /// Current mouse state
        /// </summary>
        private MouseState currentMouseState;
        /// <summary>
        /// Previous keyboard state
        /// </summary>
        private KeyboardState previousKeyboardState;
        /// <summary>
        /// Current keyboard state
        /// </summary>
        private KeyboardState currentKeyboardState;
        /// <summary>
        /// Engine render form
        /// </summary>
        private EngineForm form;

        /// <summary>
        /// Mouse X axis value
        /// </summary>
        public int MouseX { get { return this.currentMouseState.X; } }
        /// <summary>
        /// Mouse Y axis value
        /// </summary>
        public int MouseY { get { return this.currentMouseState.Y; } }
        /// <summary>
        /// Gets if left mouse button is just released
        /// </summary>
        public bool LeftMouseButtonJustReleased { get { return this.MouseButtonJustReleased(MouseButtons.LeftButton); } }
        /// <summary>
        /// Gets if left mouse button is just pressed
        /// </summary>
        public bool LeftMouseButtonJustPressed { get { return this.MouseButtonJustPressed(MouseButtons.LeftButton); } }
        /// <summary>
        /// Gets if left mouse button is pressed now
        /// </summary>
        public bool LeftMouseButtonPressed { get { return this.MouseButtonPressed(MouseButtons.LeftButton); } }
        /// <summary>
        /// Gets if right mouse button is just released
        /// </summary>
        public bool RightMouseButtonJustReleased { get { return this.MouseButtonJustReleased(MouseButtons.RightButton); } }
        /// <summary>
        /// Gets if right mouse button is just pressed
        /// </summary>
        public bool RightMouseButtonJustPressed { get { return this.MouseButtonJustPressed(MouseButtons.RightButton); } }
        /// <summary>
        /// Gets if right mouse button is pressed now
        /// </summary>
        public bool RightMouseButtonPressed { get { return this.MouseButtonPressed(MouseButtons.RightButton); } }
        /// <summary>
        /// Gets if middle mouse button is just released
        /// </summary>
        public bool MiddleMouseButtonJustReleased { get { return this.MouseButtonJustReleased(MouseButtons.MiddleButton); } }
        /// <summary>
        /// Gets if middle mouse button is just pressed
        /// </summary>
        public bool MiddleMouseButtonJustPressed { get { return this.MouseButtonJustPressed(MouseButtons.MiddleButton); } }
        /// <summary>
        /// Gets if middle mouse button is pressed now
        /// </summary>
        public bool MiddleMouseButtonPressed { get { return this.MouseButtonPressed(MouseButtons.MiddleButton); } }
        /// <summary>
        /// Sets mouse on center after update
        /// </summary>
        public bool LockToCenter { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Input(EngineForm form)
        {
            this.form = form;

            this.input = new DirectInput();

            this.mouse = new Mouse(input);
            this.mouse.SetCooperativeLevel(form, CooperativeLevel.NonExclusive | CooperativeLevel.Background);
            this.mouse.Acquire();

            this.keyboard = new Keyboard(input);
            this.keyboard.SetCooperativeLevel(form, CooperativeLevel.NonExclusive | CooperativeLevel.Background);
            this.keyboard.Acquire();
        }
        /// <summary>
        /// Updates input state
        /// </summary>
        public void Update()
        {
            this.previousMouseState = this.currentMouseState;
            this.currentMouseState = this.mouse.GetCurrentState();

            this.previousKeyboardState = this.currentKeyboardState;
            this.currentKeyboardState = this.keyboard.GetCurrentState();

            if (this.LockToCenter)
            {
                this.SetMousePosition(this.form.AbsoluteCenter.X, this.form.AbsoluteCenter.Y);
            }
        }
        /// <summary>
        /// Clear state
        /// </summary>
        public void Clear()
        {
            this.previousMouseState = this.currentMouseState = new MouseState();

            this.previousKeyboardState = this.currentKeyboardState = new KeyboardState();
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (this.mouse != null)
            {
                this.mouse.Dispose();
                this.mouse = null;
            }

            if (this.keyboard != null)
            {
                this.keyboard.Dispose();
                this.keyboard = null;
            }

            if (this.input != null)
            {
                this.input.Dispose();
                this.input = null;
            }
        }
        /// <summary>
        /// Gets if specified key is just released
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the specified key is just released</returns>
        public bool KeyJustReleased(Key key)
        {
            if (this.previousKeyboardState != null)
            {
                return this.previousKeyboardState.IsPressed(key) && !this.currentKeyboardState.IsPressed(key);
            }

            return false;
        }
        /// <summary>
        /// Gets if specified key is just pressed
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the specified key is just pressed</returns>
        public bool KeyJustPressed(Key key)
        {
            if (this.previousKeyboardState != null)
            {
                return !this.previousKeyboardState.IsPressed(key) && this.currentKeyboardState.IsPressed(key);
            }

            return false;
        }
        /// <summary>
        /// Gets if specified key is pressed now
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the specified key is pressed now</returns>
        public bool KeyPressed(Key key)
        {
            return this.currentKeyboardState.IsPressed(key);
        }
        /// <summary>
        /// Gets if specified mouse button is just released
        /// </summary>
        /// <param name="button">Mouse button</param>
        /// <returns>Returns true if the specified mouse button is just released</returns>
        public bool MouseButtonJustReleased(MouseButtons button)
        {
            if (this.previousMouseState != null)
            {
                return this.previousMouseState.Buttons[(int)button] && !this.currentMouseState.Buttons[(int)button];
            }

            return false;
        }
        /// <summary>
        /// Gets if specified mouse button is just pressed
        /// </summary>
        /// <param name="button">Mouse button</param>
        /// <returns>Returns true if the specified mouse button is just pressed</returns>
        public bool MouseButtonJustPressed(MouseButtons button)
        {
            if (this.previousMouseState != null)
            {
                return !this.previousMouseState.Buttons[(int)button] && this.currentMouseState.Buttons[(int)button];
            }

            return false;
        }
        /// <summary>
        /// Gets if specified mouse button is pressed now
        /// </summary>
        /// <param name="button">Mouse button</param>
        /// <returns>Returns true if the specified mouse button is pressed now</returns>
        public bool MouseButtonPressed(MouseButtons button)
        {
            return this.currentMouseState.Buttons[(int)button];
        }
        /// <summary>
        /// Shows mouse cursor
        /// </summary>
        public void ShowMouse()
        {
            System.Windows.Forms.Cursor.Show();
        }
        /// <summary>
        /// Hides mouse cursor
        /// </summary>
        public void HideMouse()
        {
            System.Windows.Forms.Cursor.Hide();
        }
        /// <summary>
        /// Sets mouse position
        /// </summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        public void SetMousePosition(int x, int y)
        {
            this.SetMousePosition(new Point(x, y));
        }
        /// <summary>
        /// Sets mouse position
        /// </summary>
        /// <param name="location">Position</param>
        public void SetMousePosition(Point location)
        {
            Cursor.Position = location;
        }
    }
}
