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
        /// Mouse buttons of last update
        /// </summary>
        private MouseButtons lastMouseButtons = MouseButtons.None;
        /// <summary>
        /// Current mouse buttons
        /// </summary>
        private MouseButtons currentMouseButtons = MouseButtons.None;
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
        /// Gets if left mouse button is just released
        /// </summary>
        public bool LeftMouseButtonJustReleased { get; private set; }
        /// <summary>
        /// Gets if left mouse button is just pressed
        /// </summary>
        public bool LeftMouseButtonJustPressed { get; private set; }
        /// <summary>
        /// Gets if left mouse button is pressed now
        /// </summary>
        public bool LeftMouseButtonPressed { get; private set; }
        /// <summary>
        /// Gets if right mouse button is just released
        /// </summary>
        public bool RightMouseButtonJustReleased { get; private set; }
        /// <summary>
        /// Gets if right mouse button is just pressed
        /// </summary>
        public bool RightMouseButtonJustPressed { get; private set; }
        /// <summary>
        /// Gets if right mouse button is pressed now
        /// </summary>
        public bool RightMouseButtonPressed { get; private set; }
        /// <summary>
        /// Gets if middle mouse button is just released
        /// </summary>
        public bool MiddleMouseButtonJustReleased { get; private set; }
        /// <summary>
        /// Gets if middle mouse button is just pressed
        /// </summary>
        public bool MiddleMouseButtonJustPressed { get; private set; }
        /// <summary>
        /// Gets if middle mouse button is pressed now
        /// </summary>
        public bool MiddleMouseButtonPressed { get; private set; }
        /// <summary>
        /// Gets if X1 mouse button is just released
        /// </summary>
        public bool X1MouseButtonJustReleased { get; private set; }
        /// <summary>
        /// Gets if X1 mouse button is just pressed
        /// </summary>
        public bool X1MouseButtonJustPressed { get; private set; }
        /// <summary>
        /// Gets if X1 mouse button is pressed now
        /// </summary>
        public bool X1MouseButtonPressed { get; private set; }
        /// <summary>
        /// Gets if X2 mouse button is just released
        /// </summary>
        public bool X2MouseButtonJustReleased { get; private set; }
        /// <summary>
        /// Gets if X2 mouse button is just pressed
        /// </summary>
        public bool X2MouseButtonJustPressed { get; private set; }
        /// <summary>
        /// Gets if X2 mouse button is pressed now
        /// </summary>
        public bool X2MouseButtonPressed { get; private set; }
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
            lastMouseButtons = currentMouseButtons;
            currentMouseButtons = MouseButtonsState;

            bool prev;
            bool curr;

            prev = lastMouseButtons.HasFlag(MouseButtons.Left);
            curr = currentMouseButtons.HasFlag(MouseButtons.Left);
            LeftMouseButtonPressed = curr;
            LeftMouseButtonJustPressed = curr && !prev;
            LeftMouseButtonJustReleased = !curr && prev;

            prev = lastMouseButtons.HasFlag(MouseButtons.Right);
            curr = currentMouseButtons.HasFlag(MouseButtons.Right);
            RightMouseButtonPressed = curr;
            RightMouseButtonJustPressed = curr && !prev;
            RightMouseButtonJustReleased = !curr && prev;

            prev = lastMouseButtons.HasFlag(MouseButtons.Middle);
            curr = currentMouseButtons.HasFlag(MouseButtons.Middle);
            MiddleMouseButtonPressed = curr;
            MiddleMouseButtonJustPressed = curr && !prev;
            MiddleMouseButtonJustReleased = !curr && prev;

            prev = lastMouseButtons.HasFlag(MouseButtons.XButton1);
            curr = currentMouseButtons.HasFlag(MouseButtons.XButton1);
            X1MouseButtonPressed = curr;
            X1MouseButtonJustPressed = curr && !prev;
            X1MouseButtonJustReleased = !curr && prev;

            prev = lastMouseButtons.HasFlag(MouseButtons.XButton2);
            curr = currentMouseButtons.HasFlag(MouseButtons.XButton2);
            X2MouseButtonPressed = curr;
            X2MouseButtonJustPressed = curr && !prev;
            X2MouseButtonJustReleased = !curr && prev;

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

            LeftMouseButtonPressed = false;
            LeftMouseButtonJustPressed = false;
            LeftMouseButtonJustReleased = false;

            RightMouseButtonPressed = false;
            RightMouseButtonJustPressed = false;
            RightMouseButtonJustReleased = false;

            MiddleMouseButtonPressed = false;
            MiddleMouseButtonJustPressed = false;
            MiddleMouseButtonJustReleased = false;

            X1MouseButtonPressed = false;
            X1MouseButtonJustPressed = false;
            X1MouseButtonJustReleased = false;

            X2MouseButtonPressed = false;
            X2MouseButtonJustPressed = false;
            X2MouseButtonJustReleased = false;

            lastMouseButtons = MouseButtons.None;
            currentMouseButtons = MouseButtons.None;

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
