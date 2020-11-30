using SharpDX;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Engine
{
    using Engine.Helpers;

    /// <summary>
    /// Mouse and keyboard input
    /// </summary>
    public class Input : IDisposable
    {
        /// <summary>
        /// Frame time
        /// </summary>
        public const float InputTime = 1f / 60f;

        /// <summary>
        /// Engine render form
        /// </summary>
        private readonly EngineForm form;
        /// <summary>
        /// First state update flag
        /// </summary>
        private bool firstUpdate = true;
        /// <summary>
        /// Mouse over active form flag
        /// </summary>
        private bool mouseIn = true;
        /// <summary>
        /// Mouse visble flag
        /// </summary>
        private bool visibleMouse = true;

        /// <summary>
        /// Mouse position of last update
        /// </summary>
        private Point lastMousePos;
        /// <summary>
        /// Mouse wheel delta of current update
        /// </summary>
        private int mouseWheel;
        /// <summary>
        /// Last mouse buttons state
        /// </summary>
        private MouseButtons lastMouseButtons = MouseButtons.None;
        /// <summary>
        /// Keys of last update
        /// </summary>
        private readonly List<Keys> lastKeyboardKeys = new List<Keys>();
        /// <summary>
        /// Current keys
        /// </summary>
        private readonly List<Keys> currentKeyboardKeys = new List<Keys>();

        /// <summary>
        /// Elapsed time
        /// </summary>
        public float Elapsed { get; set; } = 0f;
        /// <summary>
        /// Mouse X axis value
        /// </summary>
        public int MouseXDelta { get; private set; }
        /// <summary>
        /// Mouse Y axis value
        /// </summary>
        public int MouseYDelta { get; private set; }
        /// <summary>
        /// Mouse wheel value
        /// </summary>
        public int MouseWheelDelta { get; private set; }
        /// <summary>
        /// Absolute Mouse X axis value
        /// </summary>
        public int MouseX { get; private set; }
        /// <summary>
        /// Absolute Mouse Y axis value
        /// </summary>
        public int MouseY { get; private set; }
        /// <summary>
        /// Absolute Mouse position
        /// </summary>
        public Point MousePosition { get; private set; }
        /// <summary>
        /// Current pressed mouse buttons
        /// </summary>
        public MouseButtons PressedMouseButtons { get; private set; } = MouseButtons.None;
        /// <summary>
        /// Current just pressed mouse buttons
        /// </summary>
        public MouseButtons JustPressedMouseButtons { get; private set; } = MouseButtons.None;
        /// <summary>
        /// Current just released mouse buttons
        /// </summary>
        public MouseButtons JustReleasedMouseButtons { get; private set; } = MouseButtons.None;
        /// <summary>
        /// Gets if left or right shift key were pressed now
        /// </summary>
        public bool ShiftPressed
        {
            get
            {
                return KeyPressed(Keys.LShiftKey) || KeyPressed(Keys.RShiftKey);
            }
        }
        /// <summary>
        /// Gets if left or right control key were pressed now
        /// </summary>
        public bool ControlPressed
        {
            get
            {
                return KeyPressed(Keys.LControlKey) || KeyPressed(Keys.RControlKey);
            }
        }
        /// <summary>
        /// Sets mouse on center after update
        /// </summary>
        public bool LockMouse { get; set; }
        /// <summary>
        /// Sets cursor visible
        /// </summary>
        public bool VisibleMouse
        {
            get
            {
                return visibleMouse;
            }
            set
            {
                visibleMouse = value;

                if (visibleMouse)
                {
                    Cursor.Show();
                }
                else
                {
                    Cursor.Hide();
                }
            }
        }
        /// <summary>
        /// Mouse button state
        /// </summary>
        public MouseButtons MouseButtonsState
        {
            get
            {
                return (MouseButtons)Control.MouseButtons;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Input(EngineForm form)
        {
            this.form = form;
            this.form.MouseWheel += new MouseEventHandler(OnMouseWheel);
            this.form.MouseLeave += new EventHandler(OnMouseLeave);
            this.form.MouseEnter += new EventHandler(OnMouseEnter);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Input()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {

        }

        /// <summary>
        /// Updates input state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            Elapsed += gameTime.ElapsedSeconds;

            if (Elapsed < InputTime)
            {
                return;
            }

            Elapsed -= InputTime;

            if (mouseIn)
            {
                if (firstUpdate)
                {
                    SetMousePosition(form.RenderCenter);

                    ClearInputData();

                    firstUpdate = false;
                }
                else
                {
                    UpdateMousePositionState();

                    UpdateMouseButtonsState();

                    UpdateKeyboardState();
                }
            }
            else
            {
                ClearInputData();
            }
        }
        /// <summary>
        /// Updates the keyboard state
        /// </summary>
        private void UpdateKeyboardState()
        {
            lastKeyboardKeys.Clear();
            lastKeyboardKeys.AddRange(currentKeyboardKeys);
            currentKeyboardKeys.Clear();

            Keys[] keyboard = GetPressedKeys();
            if (keyboard.Length > 0)
            {
                currentKeyboardKeys.AddRange(keyboard);
            }
        }
        /// <summary>
        /// Updates the mouse position state
        /// </summary>
        private void UpdateMousePositionState()
        {
            var mousePos = form.PointToClient(Cursor.ScreenPosition);

            MousePosition = new Point(mousePos.X, mousePos.Y);

            MouseXDelta = MousePosition.X - lastMousePos.X;
            MouseYDelta = MousePosition.Y - lastMousePos.Y;

            MouseX = MousePosition.X;
            MouseY = MousePosition.Y;

            lastMousePos = MousePosition;

            if (LockMouse)
            {
                SetMousePosition(form.RenderCenter);
            }
        }
        /// <summary>
        /// Updates the mouse buttons state
        /// </summary>
        private void UpdateMouseButtonsState()
        {
            JustPressedMouseButtons = MouseButtonsState & ~lastMouseButtons;
            JustReleasedMouseButtons = lastMouseButtons & ~MouseButtonsState;

            lastMouseButtons = PressedMouseButtons;
            PressedMouseButtons = MouseButtonsState;

            MouseWheelDelta = mouseWheel;
            mouseWheel = 0;
        }

        /// <summary>
        /// Gets if specified key is just released
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the specified key is just released</returns>
        public bool KeyJustReleased(Keys key)
        {
            return lastKeyboardKeys.Contains(key) && !currentKeyboardKeys.Contains(key);
        }
        /// <summary>
        /// Gets if specified key is just pressed
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the specified key is just pressed</returns>
        public bool KeyJustPressed(Keys key)
        {
            return !lastKeyboardKeys.Contains(key) && currentKeyboardKeys.Contains(key);
        }
        /// <summary>
        /// Gets if specified key is pressed now
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns true if the specified key is pressed now</returns>
        public bool KeyPressed(Keys key)
        {
            return currentKeyboardKeys.Contains(key);
        }
        /// <summary>
        /// Gets if the specified mouse button is just released
        /// </summary>
        /// <param name="button">Mouse button</param>
        /// <returns>Returns true if the specified mouse button is just released</returns>
        public bool MouseButtonJustReleased(MouseButtons button)
        {
            return JustReleasedMouseButtons.HasFlag(button);
        }
        /// <summary>
        /// Gets if the specified mouse button is just pressed
        /// </summary>
        /// <param name="button">Mouse button</param>
        /// <returns>Returns true if the specified mouse button is just pressed</returns>
        public bool MouseButtonJustPressed(MouseButtons button)
        {
            return JustPressedMouseButtons.HasFlag(button);
        }
        /// <summary>
        /// Gets if the specified mouse button is pressed
        /// </summary>
        /// <param name="button">Mouse button</param>
        /// <returns>Returns true if the specified mouse button is pressed</returns>
        public bool MouseButtonPressed(MouseButtons button)
        {
            return PressedMouseButtons.HasFlag(button);
        }
        /// <summary>
        /// Sets mouse position
        /// </summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        public void SetMousePosition(int x, int y)
        {
            SetMousePosition(new Point(x, y));
        }
        /// <summary>
        /// Sets mouse position
        /// </summary>
        /// <param name="location">Position</param>
        public void SetMousePosition(Point location)
        {
            Cursor.ScreenPosition = new System.Drawing.Point(location.X, location.Y);

            var mousePos = form.PointToClient(Cursor.ScreenPosition);
            lastMousePos = new Point(mousePos.X, mousePos.Y);
        }

        /// <summary>
        /// Clear input data variables
        /// </summary>
        private void ClearInputData()
        {
            #region Mouse position

            MouseXDelta = 0;
            MouseYDelta = 0;

            MouseX = lastMousePos.X;
            MouseY = lastMousePos.Y;

            #endregion

            #region Mouse buttons

            lastMouseButtons = MouseButtons.None;
            PressedMouseButtons = MouseButtons.None;
            JustPressedMouseButtons = MouseButtons.None;
            JustReleasedMouseButtons = MouseButtons.None;

            #endregion

            #region Mouse Wheel

            MouseWheelDelta = 0;
            mouseWheel = 0;

            #endregion

            #region Keyboard keys

            lastKeyboardKeys.Clear();
            currentKeyboardKeys.Clear();

            #endregion
        }
        /// <summary>
        /// Get keyboard pressed keys
        /// </summary>
        /// <returns>Return pressed keys collection</returns>
        private Keys[] GetPressedKeys()
        {
            List<Keys> pressedKeys = new List<Keys>();

            byte[] array = new byte[256];
            if (NativeMethods.GetKeyboardState(array))
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if ((array[i] & 0x80) != 0)
                    {
                        //Pressed
                        Keys key = (Keys)i;

                        pressedKeys.Add(key);
                    }
                }
            }

            return pressedKeys.ToArray();
        }

        /// <summary>
        /// When mouse enters form
        /// </summary>
        /// <param name="sender">Form</param>
        /// <param name="e">Event arguments</param>
        private void OnMouseEnter(object sender, EventArgs e)
        {
            mouseIn = true;
        }
        /// <summary>
        /// When mouse leaves form
        /// </summary>
        /// <param name="sender">Form</param>
        /// <param name="e">Event arguments</param>
        private void OnMouseLeave(object sender, EventArgs e)
        {
            mouseIn = false;
        }
        /// <summary>
        /// When mouse wheel moves
        /// </summary>
        /// <param name="sender">Form</param>
        /// <param name="e">Event arguments</param>
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            mouseWheel = e.Delta;
        }
    }
}
