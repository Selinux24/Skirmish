using System;
using System.Drawing;
using System.Windows.Forms;
using SharpDX.DirectInput;

namespace Engine
{
    /// <summary>
    /// Input de teclado y ratón
    /// </summary>
    public class Input : IDisposable
    {
        /// <summary>
        /// Input
        /// </summary>
        private DirectInput input = null;
        /// <summary>
        /// Ratón
        /// </summary>
        private Mouse mouse = null;
        /// <summary>
        /// Teclado
        /// </summary>
        private Keyboard keyboard = null;
        /// <summary>
        /// Estado previo del ratón
        /// </summary>
        private MouseState previousMouseState;
        /// <summary>
        /// Estado actual del ratón
        /// </summary>
        private MouseState currentMouseState;
        /// <summary>
        /// Estado previo del teclado
        /// </summary>
        private KeyboardState previousKeyboardState;
        /// <summary>
        /// Estado actual del teclado
        /// </summary>
        private KeyboardState currentKeyboardState;
        /// <summary>
        /// Engine render form
        /// </summary>
        private EngineForm form;

        /// <summary>
        /// Valor del eje X del ratón
        /// </summary>
        public int MouseX { get { return this.currentMouseState.X; } }
        /// <summary>
        /// Valor del eje Y del ratón
        /// </summary>
        public int MouseY { get { return this.currentMouseState.Y; } }
        /// <summary>
        /// Indica si el botón izquierdo del ratón acaba de ser soltado
        /// </summary>
        public bool LeftMouseButtonJustReleased { get { return this.MouseButtonJustReleased(MouseButtons.LeftButton); } }
        /// <summary>
        /// Indica si el botón izquierdo del ratón acaba de ser presionado
        /// </summary>
        public bool LeftMouseButtonJustPressed { get { return this.MouseButtonJustPressed(MouseButtons.LeftButton); } }
        /// <summary>
        /// Indica si el botón izquierdo del ratón está siendo presionado
        /// </summary>
        public bool LeftMouseButtonPressed { get { return this.MouseButtonPressed(MouseButtons.LeftButton); } }
        /// <summary>
        /// Indica si el botón derecho del ratón acaba de ser soltado
        /// </summary>
        public bool RightMouseButtonJustReleased { get { return this.MouseButtonJustReleased(MouseButtons.RightButton); } }
        /// <summary>
        /// Indica si el botón derecho del ratón acaba de ser presionado
        /// </summary>
        public bool RightMouseButtonJustPressed { get { return this.MouseButtonJustPressed(MouseButtons.RightButton); } }
        /// <summary>
        /// Indica si el botón derecho del ratón está siendo presionado
        /// </summary>
        public bool RightMouseButtonPressed { get { return this.MouseButtonPressed(MouseButtons.RightButton); } }
        /// <summary>
        /// Indica si el botón medio del ratón acaba de ser soltado
        /// </summary>
        public bool MiddleMouseButtonJustReleased { get { return this.MouseButtonJustReleased(MouseButtons.MiddleButton); } }
        /// <summary>
        /// Indica si el botón medio del ratón acaba de ser presionado
        /// </summary>
        public bool MiddleMouseButtonJustPressed { get { return this.MouseButtonJustPressed(MouseButtons.MiddleButton); } }
        /// <summary>
        /// Indica si el botón medio del ratón está siendo presionado
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
        /// Actualiza el estado del componente
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        public void Update()
        {
            this.previousMouseState = this.currentMouseState;
            this.currentMouseState = this.mouse.GetCurrentState();

            this.previousKeyboardState = this.currentKeyboardState;
            this.currentKeyboardState = this.keyboard.GetCurrentState();

            if (this.LockToCenter)
            {
                this.SetMousePosition(this.form.ScreenCenter);
            }
        }

        public void Clear()
        {
            this.previousMouseState = this.currentMouseState = new MouseState();

            this.previousKeyboardState = this.currentKeyboardState = new KeyboardState();
        }
        /// <summary>
        /// Libera los objetos
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
        /// Indica si la tecla especificada acaba de ser soltada
        /// </summary>
        /// <param name="key">Tecla</param>
        public bool KeyJustReleased(Key key)
        {
            if (this.previousKeyboardState != null)
            {
                return this.previousKeyboardState.IsPressed(key) && !this.currentKeyboardState.IsPressed(key);
            }

            return false;
        }
        /// <summary>
        /// Indica si la tecla especificada acaba de ser presionada
        /// </summary>
        /// <param name="key">Tecla</param>
        public bool KeyJustPressed(Key key)
        {
            if (this.previousKeyboardState != null)
            {
                return !this.previousKeyboardState.IsPressed(key) && this.currentKeyboardState.IsPressed(key);
            }

            return false;
        }
        /// <summary>
        /// Indica si la tecla especificada está siendo presionada
        /// </summary>
        /// <param name="key">Tecla</param>
        public bool KeyPressed(Key key)
        {
            return this.currentKeyboardState.IsPressed(key);
        }
        /// <summary>
        /// Indica si el botón del ratón acaba de ser soltado
        /// </summary>
        public bool MouseButtonJustReleased(MouseButtons button)
        {
            if (this.previousMouseState != null)
            {
                return this.previousMouseState.Buttons[(int)button] && !this.currentMouseState.Buttons[(int)button];
            }

            return false;
        }
        /// <summary>
        /// Indica si el botón del ratón acaba de ser presionado
        /// </summary>
        public bool MouseButtonJustPressed(MouseButtons button)
        {
            if (this.previousMouseState != null)
            {
                return !this.previousMouseState.Buttons[(int)button] && this.currentMouseState.Buttons[(int)button];
            }

            return false;
        }
        /// <summary>
        /// Indica si el botón del ratón está siendo presionado
        /// </summary>
        public bool MouseButtonPressed(MouseButtons button)
        {
            return this.currentMouseState.Buttons[(int)button];
        }
        /// <summary>
        /// Muestra el cursor del ratón
        /// </summary>
        public void ShowMouse()
        {
            System.Windows.Forms.Cursor.Show();
        }
        /// <summary>
        /// Oculta el cursor del ratón
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
