using SharpDX;
using SharpDX.Windows;
using System;
using System.Windows.Forms;
using static SharpDX.Windows.RenderLoop;

namespace Engine.Windows
{
    using Engine.Windows.Properties;

    /// <summary>
    /// Engine render form
    /// </summary>
    public class WindowsEngineForm : RenderForm, IEngineForm
    {
        /// <summary>
        /// Intialization internal flag
        /// </summary>
        private readonly bool initialized = false;
        /// <summary>
        /// Previous window state
        /// </summary>
        private FormWindowState lastWindowState = FormWindowState.Normal;

        /// <inheritdoc/>
        public int RenderWidth { get; private set; }
        /// <inheritdoc/>
        public int RenderHeight { get; private set; }
        /// <inheritdoc/>
        public RectangleF RenderRectangle
        {
            get
            {
                return new RectangleF(0, 0, RenderWidth, RenderHeight);
            }
        }
        /// <inheritdoc/>
        public Point RenderCenter { get; private set; }
        /// <inheritdoc/>
        public Point ScreenCenter { get; private set; }
        /// <inheritdoc/>
        public bool Resizing { get; private set; }
        /// <inheritdoc/>
        public bool SizeUpdated { get; private set; }
        /// <inheritdoc/>
        public bool FormModeUpdated
        {
            get
            {
                return lastWindowState != WindowState;
            }
        }
        /// <inheritdoc/>
        public bool IsMinimized
        {
            get
            {
                return WindowState == FormWindowState.Minimized;
            }
        }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        public WindowsEngineForm()
            : base()
        {
            InitializeComponent();

            initialized = true;
        }
        /// <summary>
        /// Initializes the form
        /// </summary>
        /// <param name="name">Form name</param>
        /// <param name="screenWidth">Width</param>
        /// <param name="screenHeight">Height</param>
        /// <param name="fullScreen">Full screen</param>
        public void Initialize(string name, int screenWidth, int screenHeight, bool fullScreen)
        {
            Name = name;

            base.IsFullscreen = fullScreen;
            AllowUserResizing = !fullScreen;

            Size = new System.Drawing.Size(screenWidth, screenHeight);

            UpdateSizes(fullScreen);
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

            ResumeLayout(false);
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == System.Windows.Forms.Keys.F10)
            {
                // Do what you want with the F10 key
                e.SuppressKeyPress = true;
            }

            base.OnKeyDown(e);
        }
        /// <inheritdoc/>
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);

            if (initialized)
            {
                UpdateSizes(IsFullscreen);
            }
        }
        /// <inheritdoc/>
        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);

            Resizing = true;
        }
        /// <inheritdoc/>
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            Resizing = false;
        }
        /// <inheritdoc/>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            lastWindowState = WindowState;
        }

        /// <inheritdoc/>
        public Viewport GetViewport()
        {
            return new Viewport(0, 0, RenderWidth, RenderHeight, 0, 1.0f);
        }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public Vector2 ToScreenSpace(Vector2 position)
        {
            var screenSpace = position - RenderRectangle.Center;

            screenSpace.Y *= -1f;

            return screenSpace;
        }
        /// <inheritdoc/>
        public Point PointToClient(Point p)
        {
            var drRes = PointToClient(new System.Drawing.Point(p.X, p.Y));

            return new Point(drRes.X, drRes.Y);
        }
        /// <inheritdoc/>
        public Point PointToScreen(Point p)
        {
            var drRes = PointToScreen(new System.Drawing.Point(p.X, p.Y));

            return new Point(drRes.X, drRes.Y);
        }

        /// <inheritdoc/>
        public void RenderLoop(Action renderCallback)
        {
            RenderCallback r = new RenderCallback(renderCallback);

            Run(this, r);
        }
    }
}
