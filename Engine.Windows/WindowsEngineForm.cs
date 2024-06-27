using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Engine.Windows
{
    using Engine.Windows.Helpers;
    using Engine.Windows.Properties;
    using SharpDX;
    using SharpDX.Mathematics.Interop;

    /// <summary>
    /// Engine render form
    /// </summary>
    /// <remarks>
    /// An adapted copy of https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX.Desktop/RenderForm.cs
    /// </remarks>
    public class WindowsEngineForm : Form, IEngineForm
    {
        /// <summary>
        /// Intialization internal flag
        /// </summary>
        private readonly bool initialized = false;
        /// <summary>
        /// Previous window state
        /// </summary>
        private FormWindowState lastWindowState = FormWindowState.Normal;

        private const int WM_SIZE = 0x0005;
        private const int SIZE_RESTORED = 0;
        private const int SIZE_MINIMIZED = 1;
        private const int SIZE_MAXIMIZED = 2;
        private const int WM_ACTIVATEAPP = 0x001C;
        private const int WM_POWERBROADCAST = 0x0218;
        private const int WM_MENUCHAR = 0x0120;
        private const int WM_SYSCOMMAND = 0x0112;
        private const uint PBT_APMRESUMESUSPEND = 7;
        private const uint PBT_APMQUERYSUSPEND = 0;
        private const int SC_MONITORPOWER = 0xF170;
        private const int SC_SCREENSAVE = 0xF140;
        private const int WM_DISPLAYCHANGE = 0x007E;
        private const int MNC_CLOSE = 1;
        private Size cachedSize;
        private FormWindowState previousWindowState;
        private bool allowUserResizing;
        private bool isBackgroundFirstDraw;
        private bool isSizeChangedWithoutResizeBegin;

        /// <inheritdoc/>
        public static Vector2 ScreenSize
        {
            get
            {
                var rect = Screen.PrimaryScreen.Bounds;

                return new Vector2(rect.Width, rect.Height);
            }
        }

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
        public int MouseWheelDelta { get; private set; } = 0;
        /// <inheritdoc/>
        public long MouseWheelDeltaTimestamp { get; private set; }
        /// <inheritdoc/>
        public bool MouseIn { get; private set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether this form can be resized by the user. See remarks.
        /// </summary>
        /// <remarks>
        /// This property alters <see cref="Form.FormBorderStyle"/>, 
        /// for <c>true</c> value it is <see cref="FormBorderStyle.Sizable"/>, 
        /// for <c>false</c> - <see cref="FormBorderStyle.FixedSingle"/>.
        /// </remarks>
        /// <value><c>true</c> if this form can be resized by the user (by default); otherwise, <c>false</c>.</value>
        public bool AllowUserResizing
        {
            get
            {
                return allowUserResizing;
            }
            set
            {
                if (allowUserResizing != value)
                {
                    allowUserResizing = value;
                    MaximizeBox = allowUserResizing;
                    FormBorderStyle = GetFormBorderStyle();
                }
            }
        }
        /// <summary>
        /// Gets or sets a value indicationg whether the current render form is in fullscreen mode. See remarks.
        /// </summary>
        /// <remarks>
        /// If Toolkit is used, this property is set automatically,
        /// otherwise user should maintain it himself as it affects the behavior of <see cref="AllowUserResizing"/> property.
        /// </remarks>
        public bool IsFullscreen { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public WindowsEngineForm() : this("Engine")
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public WindowsEngineForm(string text) : base()
        {
            SuspendLayout();

            Icon = Resources.engine;
            Name = text;
            Text = text;
            KeyPreview = true;
            ClientSize = new Size(800, 600);

            ResizeRedraw = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            previousWindowState = FormWindowState.Normal;
            AllowUserResizing = true;

            ResumeLayout(false);

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

            IsFullscreen = fullScreen;
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
        /// Gets the Form Border Style
        /// </summary>
        private FormBorderStyle GetFormBorderStyle()
        {
            if (IsFullscreen)
            {
                return FormBorderStyle.None;
            }

            return allowUserResizing ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
        }

        /// <summary>
        /// Occurs when [app activated].
        /// </summary>
        public event EventHandler<EventArgs> AppActivated;
        /// <summary>
        /// Occurs when [app deactivated].
        /// </summary>
        public event EventHandler<EventArgs> AppDeactivated;
        /// <summary>
        /// Occurs when [monitor changed].
        /// </summary>
        public event EventHandler<EventArgs> MonitorChanged;
        /// <summary>
        /// Occurs when [pause rendering].
        /// </summary>
        public event EventHandler<EventArgs> PauseRendering;
        /// <summary>
        /// Occurs when [resume rendering].
        /// </summary>
        public event EventHandler<EventArgs> ResumeRendering;
        /// <summary>
        /// Occurs when [screensaver].
        /// </summary>
        public event EventHandler<CancelEventArgs> Screensaver;
        /// <summary>
        /// Occurs when [system resume].
        /// </summary>
        public event EventHandler<EventArgs> SystemResume;
        /// <summary>
        /// Occurs when [system suspend].
        /// </summary>
        public event EventHandler<EventArgs> SystemSuspend;
        /// <summary>
        /// Occurs when [user resized].
        /// </summary>
        public event EventHandler<EventArgs> UserResized;

        /// <summary>
        /// Raises the Pause Rendering event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnPauseRendering(EventArgs e)
        {
            PauseRendering?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the Resume Rendering event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnResumeRendering(EventArgs e)
        {
            ResumeRendering?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the User resized event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnUserResized(EventArgs e)
        {
            UserResized?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the Monitor changed event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnMonitorChanged(EventArgs e)
        {
            MonitorChanged?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the On App Activated event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAppActivated(EventArgs e)
        {
            AppActivated?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the App Deactivated event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAppDeactivated(EventArgs e)
        {
            AppDeactivated?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the System Suspend event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnSystemSuspend(EventArgs e)
        {
            SystemSuspend?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the System Resume event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnSystemResume(EventArgs e)
        {
            SystemResume?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the <see cref="E:Screensaver"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void OnScreensaver(CancelEventArgs e)
        {
            Screensaver?.Invoke(this, e);
        }

        /// <inheritdoc/>
        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (!Resizing && (isSizeChangedWithoutResizeBegin || cachedSize != Size))
            {
                isSizeChangedWithoutResizeBegin = false;
                cachedSize = Size;
                OnUserResized(EventArgs.Empty);
            }
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
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            lastWindowState = WindowState;
        }
        /// <inheritdoc/>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            MouseWheelDelta = e.Delta;
            MouseWheelDeltaTimestamp = DateTime.Now.Ticks;
        }
        /// <inheritdoc/>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            MouseIn = true;
        }
        /// <inheritdoc/>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            MouseIn = false;
        }
        /// <inheritdoc/>
        protected override void OnResizeBegin(EventArgs e)
        {
            Resizing = true;

            base.OnResizeBegin(e);
            cachedSize = Size;
            OnPauseRendering(e);
        }
        /// <inheritdoc/>
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            if (Resizing && cachedSize != Size)
            {
                OnUserResized(e);
            }

            Resizing = false;
            OnResumeRendering(e);
        }
        /// <inheritdoc/>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!isBackgroundFirstDraw)
            {
                base.OnPaintBackground(e);
                isBackgroundFirstDraw = true;
            }
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
        protected override void WndProc(ref Message m)
        {
            long wparam = m.WParam.ToInt64();

            switch (m.Msg)
            {
                case WM_SIZE:
                    OnMessageSize(wparam, m.HWnd);
                    break;
                case WM_ACTIVATEAPP:
                    OnMessageActivateApp(wparam);
                    break;
                case WM_POWERBROADCAST:
                    if (OnMessagePowerBroadcast(wparam, ref m))
                    {
                        return;
                    }
                    break;
                case WM_MENUCHAR:
                    m.Result = new IntPtr(MNC_CLOSE << 16);
                    return;
                case WM_SYSCOMMAND:
                    wparam &= 0xFFF0;
                    if (OnMessageSysCommand(wparam, ref m))
                    {
                        return;
                    }
                    break;
                case WM_DISPLAYCHANGE:
                    OnMonitorChanged(EventArgs.Empty);
                    break;
            }

            base.WndProc(ref m);
        }
        private void OnMessageSize(long wparam, nint hwnd)
        {
            if (wparam == SIZE_MINIMIZED)
            {
                previousWindowState = FormWindowState.Minimized;
                OnPauseRendering(EventArgs.Empty);
                return;
            }

            NativeMethods.GetClientRect(hwnd, out RawRectangle rect);
            if (rect.Bottom - rect.Top == 0)
            {
                // Rapidly clicking the task bar to minimize and restore a window
                // can cause a WM_SIZE message with SIZE_RESTORED when 
                // the window has actually become minimized due to rapid change
                // so just ignore this message
                return;
            }

            if (wparam == SIZE_MAXIMIZED)
            {
                if (previousWindowState == FormWindowState.Minimized)
                {
                    OnResumeRendering(EventArgs.Empty);
                }

                previousWindowState = FormWindowState.Maximized;

                OnUserResized(EventArgs.Empty);
                cachedSize = Size;
                return;
            }

            if (wparam != SIZE_RESTORED)
            {
                return;
            }

            if (previousWindowState == FormWindowState.Minimized)
            {
                OnResumeRendering(EventArgs.Empty);
            }

            if (!Resizing && (Size != cachedSize || previousWindowState == FormWindowState.Maximized))
            {
                previousWindowState = FormWindowState.Normal;

                // Only update when cachedSize is != 0
                if (cachedSize != Size.Empty)
                {
                    isSizeChangedWithoutResizeBegin = true;
                }

                return;
            }

            previousWindowState = FormWindowState.Normal;
        }
        private void OnMessageActivateApp(long wparam)
        {
            if (wparam != 0)
                OnAppActivated(EventArgs.Empty);
            else
                OnAppDeactivated(EventArgs.Empty);
        }
        private bool OnMessagePowerBroadcast(long wparam, ref Message m)
        {
            if (wparam == PBT_APMQUERYSUSPEND)
            {
                OnSystemSuspend(EventArgs.Empty);
                m.Result = new IntPtr(1);
                return true;
            }
            else if (wparam == PBT_APMRESUMESUSPEND)
            {
                OnSystemResume(EventArgs.Empty);
                m.Result = new IntPtr(1);
                return true;
            }

            return false;
        }
        private bool OnMessageSysCommand(long wparam, ref Message m)
        {
            if (wparam == SC_MONITORPOWER || wparam == SC_SCREENSAVE)
            {
                var e = new CancelEventArgs();
                OnScreensaver(e);
                if (e.Cancel)
                {
                    m.Result = IntPtr.Zero;
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        protected override bool ProcessDialogKey(System.Windows.Forms.Keys keyData)
        {
            if (keyData == (System.Windows.Forms.Keys.Menu | System.Windows.Forms.Keys.Alt) || keyData == System.Windows.Forms.Keys.F10)
                return true;

            return base.ProcessDialogKey(keyData);
        }

        /// <inheritdoc/>
        public void Render(Action renderCallback)
        {
            RenderLoop.Run(this, renderCallback);
        }
    }
}
