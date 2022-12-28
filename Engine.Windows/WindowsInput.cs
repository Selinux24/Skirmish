using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Engine.Windows
{
    using Engine.Windows.Helpers;

    /// <summary>
    /// Mouse and keyboard input
    /// </summary>
    public class WindowsInput : IInput
    {
        /// <summary>
        /// Engine render form
        /// </summary>
        private IEngineForm form;
        /// <summary>
        /// First state update flag
        /// </summary>
        private bool firstUpdate = true;
        /// <summary>
        /// Mouse visble flag
        /// </summary>
        private bool visibleMouse = true;
        /// <summary>
        /// Last mouse wheel delta timestamp
        /// </summary>
        private long mouseWheelDeltaTimestamp = 0;

        /// <summary>
        /// Mouse position of last update
        /// </summary>
        private Point lastMousePos;
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

        /// <inheritdoc/>
        public float Elapsed { get; set; } = 0f;
        /// <inheritdoc/>
        public int MouseXDelta { get; private set; }
        /// <inheritdoc/>
        public int MouseYDelta { get; private set; }
        /// <inheritdoc/>
        public int MouseWheelDelta { get; private set; }
        /// <inheritdoc/>
        public int MouseX { get; private set; }
        /// <inheritdoc/>
        public int MouseY { get; private set; }
        /// <inheritdoc/>
        public Point MousePosition { get; private set; }
        /// <inheritdoc/>
        public MouseButtons PressedMouseButtons { get; private set; } = MouseButtons.None;
        /// <inheritdoc/>
        public MouseButtons JustPressedMouseButtons
        {
            get
            {
                return MouseButtonsState & ~lastMouseButtons;
            }
        }
        /// <inheritdoc/>
        public MouseButtons JustReleasedMouseButtons
        {
            get
            {
                return lastMouseButtons & ~MouseButtonsState;
            }
        }
        /// <inheritdoc/>
        public bool ShiftPressed
        {
            get
            {
                return KeyPressed(Keys.LShiftKey) || KeyPressed(Keys.RShiftKey);
            }
        }
        /// <inheritdoc/>
        public bool ControlPressed
        {
            get
            {
                return KeyPressed(Keys.LControlKey) || KeyPressed(Keys.RControlKey);
            }
        }
        /// <inheritdoc/>
        public bool LockMouse { get; set; }
        /// <inheritdoc/>
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
                    WindowsCursor.Show();
                }
                else
                {
                    WindowsCursor.Hide();
                }
            }
        }
        /// <inheritdoc/>
        public MouseButtons MouseButtonsState
        {
            get
            {
                return (MouseButtons)Control.MouseButtons;
            }
        }

        /// <summary>
        /// Get keyboard pressed keys
        /// </summary>
        /// <returns>Return pressed keys collection</returns>
        private static Keys[] GetPressedKeys()
        {
            return NativeMethods.GetPressedKeys();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public WindowsInput()
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~WindowsInput()
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

        /// <inheritdoc/>
        public void SetForm(IEngineForm form)
        {
            this.form = form;
        }

        /// <inheritdoc/>
        public void Update(GameTime gameTime)
        {
            Elapsed += gameTime.ElapsedSeconds;

            if (Elapsed < GameEnvironment.FrameTime)
            {
                return;
            }

            Elapsed -= GameEnvironment.FrameTime;

            if (form == null)
            {
                return;
            }

            if (form.MouseIn)
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
            var mousePos = form.PointToClient(WindowsCursor.ScreenPosition);

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
            lastMouseButtons = PressedMouseButtons;
            PressedMouseButtons = MouseButtonsState;

            MouseWheelDelta = 0;
            if (mouseWheelDeltaTimestamp != form.MouseWheelDeltaTimestamp)
            {
                MouseWheelDelta = form.MouseWheelDelta;
                mouseWheelDeltaTimestamp = form.MouseWheelDeltaTimestamp;
            }
        }

        /// <inheritdoc/>
        public bool KeyJustReleased(Keys key)
        {
            return lastKeyboardKeys.Contains(key) && !currentKeyboardKeys.Contains(key);
        }
        /// <inheritdoc/>
        public bool KeyJustPressed(Keys key)
        {
            return !lastKeyboardKeys.Contains(key) && currentKeyboardKeys.Contains(key);
        }
        /// <inheritdoc/>
        public bool KeyPressed(Keys key)
        {
            return currentKeyboardKeys.Contains(key);
        }
        /// <inheritdoc/>
        public bool MouseButtonJustReleased(MouseButtons button)
        {
            return JustReleasedMouseButtons.HasFlag(button);
        }
        /// <inheritdoc/>
        public bool MouseButtonJustPressed(MouseButtons button)
        {
            return JustPressedMouseButtons.HasFlag(button);
        }
        /// <inheritdoc/>
        public bool MouseButtonPressed(MouseButtons button)
        {
            return PressedMouseButtons.HasFlag(button);
        }
        /// <inheritdoc/>
        public void SetMousePosition(int x, int y)
        {
            SetMousePosition(new Point(x, y));
        }
        /// <inheritdoc/>
        public void SetMousePosition(Point location)
        {
            WindowsCursor.ScreenPosition = location;

            var mousePos = form.PointToClient(WindowsCursor.ScreenPosition);
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

            #endregion

            #region Mouse Wheel

            MouseWheelDelta = 0;

            #endregion

            #region Keyboard keys

            lastKeyboardKeys.Clear();
            currentKeyboardKeys.Clear();

            #endregion
        }
        /// <inheritdoc/>
        public Keys[] GetJustPressedKeys()
        {
            return currentKeyboardKeys.ToArray();
        }
        /// <inheritdoc/>
        public Keys[] GetJustReleasedKeys()
        {
            return lastKeyboardKeys.Where(lk => !currentKeyboardKeys.Contains(lk)).ToArray();
        }

        /// <inheritdoc/>
        public string GetStrokes()
        {
            return NativeMethods.GetStrokes();
        }
    }
}
