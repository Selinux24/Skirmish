using SharpDX;
using SharpDX.Windows;
using System.Windows.Forms;

namespace Engine
{
    using Engine.Properties;
    using System;

    /// <summary>
    /// Engine render form
    /// </summary>
    public class EngineForm : RenderForm
    {
        /// <summary>
        /// Intialization internal flag
        /// </summary>
        private readonly bool initialized = false;
        private FormWindowState previousState = FormWindowState.Normal;

        /// <summary>
        /// Render width
        /// </summary>
        public int RenderWidth { get; private set; }
        /// <summary>
        /// Render height
        /// </summary>
        public int RenderHeight { get; private set; }
        /// <summary>
        /// Render rectangle
        /// </summary>
        public RectangleF RenderRectangle
        {
            get
            {
                return new RectangleF(0, 0, RenderWidth, RenderHeight);
            }
        }
        /// <summary>
        /// Rneder area center
        /// </summary>
        public Point RenderCenter { get; private set; }
        /// <summary>
        /// Screen center
        /// </summary>
        public Point ScreenCenter { get; private set; }
        /// <summary>
        /// Gets or sets a value indicationg whether the current engine form is in fullscreen
        /// </summary>
        public new bool IsFullscreen
        {
            get
            {
                return base.IsFullscreen;
            }
            set
            {
                base.IsFullscreen = value;
            }
        }

        /// <summary>
        /// Gets the primary screen size
        /// </summary>
        public static Vector2 ScreenSize
        {
            get
            {
                var rect = Screen.PrimaryScreen.Bounds;

                return new Vector2(rect.Width, rect.Height);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Form name</param>
        /// <param name="screenWidth">Width</param>
        /// <param name="screenHeight">Height</param>
        /// <param name="fullScreen">Full screen</param>
        public EngineForm(string name, int screenWidth, int screenHeight, bool fullScreen)
            : base(name)
        {
            base.IsFullscreen = fullScreen;
            AllowUserResizing = !fullScreen;

            Size = new System.Drawing.Size(screenWidth, screenHeight);

            UpdateSizes(fullScreen);

            InitializeComponent();

            initialized = true;
        }
        /// <summary>
        /// Initialize component
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();

            Icon = Resources.engine;
            Name = "EngineForm";
            Text = "Engine Form";
            KeyPreview = true;
            KeyDown += new KeyEventHandler(EngineFormKeyDown);
            ResumeLayout(false);
        }
        /// <summary>
        /// Invalidation override
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);

            if (initialized)
            {
                UpdateSizes(IsFullscreen);
            }
        }
        /// <summary>
        /// Update form sizes
        /// </summary>
        /// <param name="fullScreen">Indicates whether the form is windowed or full screen</param>
        private void UpdateSizes(bool fullScreen)
        {
            SizeUpdated = false;

            if (fullScreen)
            {
                if (RenderWidth != Size.Width)
                {
                    RenderWidth = Size.Width;
                    SizeUpdated = true;
                }
                if (RenderHeight != Size.Height)
                {
                    RenderHeight = Size.Height;
                    SizeUpdated = true;
                }
            }
            else
            {
                if (RenderWidth != ClientSize.Width)
                {
                    RenderWidth = ClientSize.Width;
                    SizeUpdated = true;
                }
                if (RenderHeight != ClientSize.Height)
                {
                    RenderHeight = ClientSize.Height;
                    SizeUpdated = true;
                }
            }

            if (SizeUpdated)
            {
                RenderCenter = new Point(RenderWidth / 2, RenderHeight / 2);
                ScreenCenter = new Point(Location.X + RenderCenter.X, Location.Y + RenderCenter.Y);
            }
        }
        public bool Resizing { get; private set; }
        public bool SizeUpdated { get; private set; }
        public bool FormModeUpdated
        {
            get
            {
                return previousState != WindowState;
            }
        }
        public bool IsMinimized
        {
            get
            {
                return WindowState == FormWindowState.Minimized;
            }
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);

            Resizing = true;
        }
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            Resizing = false;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (previousState != WindowState)
            {
                previousState = WindowState;
            }
        }


        /// <summary>
        /// Gets the render viewport
        /// </summary>
        /// <returns></returns>
        public Viewport GetViewport()
        {
            return new Viewport(0, 0, RenderWidth, RenderHeight, 0, 1.0f);
        }
        /// <summary>
        /// Gets the current ortho projection matrix
        /// </summary>
        /// <returns>Returns the current ortho projection matrix</returns>
        public Matrix GetOrthoProjectionMatrix()
        {
            Matrix view = Matrix.LookAtLH(
                Vector3.Zero,
                Vector3.ForwardLH,
                Vector3.Up);

            Matrix projection = Matrix.OrthoLH(
                RenderWidth,
                RenderHeight,
                0f, 100f);

            return view * projection;
        }
        /// <summary>
        /// Transform to screen space using the form view ortho projection matrix
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the screen space position</returns>
        /// <remarks>Screen space: Center = (0,0) Left = -X Up = +Y</remarks>
        public Vector2 ToScreenSpace(Vector2 position)
        {
            var screenSpace = position - RenderRectangle.Center;

            screenSpace.Y *= -1f;

            return screenSpace;
        }

        /// <summary>
        /// Invalidates control keys
        /// </summary>
        private void EngineFormKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == System.Windows.Forms.Keys.F10)
            {
                // Do what you want with the F10 key
                e.SuppressKeyPress = true;
            }
        }
    }
}
