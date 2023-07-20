using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Engine.Windows
{
    using Engine.Windows.Helpers;

    /// <summary>
    /// RenderLoop provides a rendering loop infrastructure. See remarks for usage. 
    /// </summary>
    /// <remarks>
    /// Use static <see cref="Run"/>  
    /// method to directly use a renderloop with a render callback or use your own loop:
    /// <code>
    /// control.Show();
    /// using (var loop = new RenderLoop(control))
    /// {
    ///     while (loop.NextFrame())
    ///     {
    ///        // Perform draw operations here.
    ///     }
    /// }
    /// </code>
    /// Note that the main control can be changed at anytime inside the loop.
    /// 
    /// A copy of https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX.Desktop/RenderLoop.cs
    /// </remarks>
    class RenderLoop : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is application idle.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is application idle; otherwise, <c>false</c>.
        /// </value>
        public static bool IsIdle
        {
            get
            {
                return NativeMethods.PeekMessage(out _, IntPtr.Zero, 0, 0, 0) == 0;
            }
        }

        private IntPtr controlHandle;
        private Control control;
        private bool isControlAlive;
        private bool switchControl;

        /// <summary>
        /// Gets or sets the control to associate with the current render loop.
        /// </summary>
        /// <value>The control.</value>
        /// <exception cref="System.InvalidOperationException">Control is already disposed</exception>
        public Control Control
        {
            get
            {
                return control;
            }
            set
            {
                if (control == value) return;

                // Remove any previous control
                if (control != null && !switchControl)
                {
                    isControlAlive = false;
                    control.Disposed -= ControlDisposed;
                    controlHandle = IntPtr.Zero;
                }

                if (value != null && value.IsDisposed)
                {
                    throw new InvalidOperationException("Control is already disposed");
                }

                control = value;
                switchControl = true;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the render loop should use the default <see cref="Application.DoEvents"/> instead of a custom window message loop lightweight for GC. Default is false.
        /// </summary>
        /// <value><c>true</c> if the render loop should use the default <see cref="Application.DoEvents"/> instead of a custom window message loop (default false); otherwise, <c>false</c>.</value>
        /// <remarks>By default, RenderLoop is using a custom window message loop that is more lightweight than <see cref="Application.DoEvents" /> to process windows event message. 
        /// Set this parameter to true to use the default <see cref="Application.DoEvents"/>.</remarks>
        public bool UseApplicationDoEvents { get; set; }

        /// <summary>
        /// Runs the specified main loop in the specified context.
        /// </summary>
        public static void Run(ApplicationContext context, Action renderCallback)
        {
            Run(context.MainForm, renderCallback);
        }
        /// <summary>
        /// Runs the specified main loop for the specified windows form.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="renderCallback">The rendering callback.</param>
        /// <param name="useApplicationDoEvents">if set to <c>true</c> indicating whether the render loop should use the default <see cref="Application.DoEvents"/> instead of a custom window message loop lightweight for GC. Default is false.</param>
        /// <exception cref="System.ArgumentNullException">form
        /// or
        /// renderCallback</exception>
        public static void Run(Control form, Action renderCallback, bool useApplicationDoEvents = false)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (renderCallback == null) throw new ArgumentNullException(nameof(renderCallback));

            form.Show();

            using var renderLoop = new RenderLoop(form) { UseApplicationDoEvents = useApplicationDoEvents };
            while (renderLoop.NextFrame())
            {
                renderCallback();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderLoop"/> class.
        /// </summary>
        public RenderLoop()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderLoop"/> class.
        /// </summary>
        public RenderLoop(Control control)
        {
            Control = control;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~RenderLoop()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Control = null;
            }
        }

        /// <summary>
        /// Calls this method on each frame.
        /// </summary>
        /// <returns><c>true</c> if if the control is still active, <c>false</c> otherwise.</returns>
        /// <exception cref="System.InvalidOperationException">An error occured </exception>
        public bool NextFrame()
        {
            // Setup new control
            // TODO this is not completely thread-safe. We should use a lock to handle this correctly
            if (switchControl && control != null)
            {
                controlHandle = control.Handle;
                control.Disposed += ControlDisposed;
                isControlAlive = true;
                switchControl = false;
            }

            if (!isControlAlive)
            {
                return switchControl;
            }

            if (UseApplicationDoEvents)
            {
                // Revert back to Application.DoEvents in order to support Application.AddMessageFilter
                // Seems that DoEvents is compatible with Mono unlike Application.Run that was not running
                // correctly.
                Application.DoEvents();

                return true;
            }

            var localHandle = controlHandle;
            if (localHandle == IntPtr.Zero)
            {
                return true;
            }

            // Previous code not compatible with Application.AddMessageFilter but faster then DoEvents
            while (NativeMethods.PeekMessage(out var msg, IntPtr.Zero, 0, 0, 0) != 0)
            {
                if (NativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0) == -1)
                {
                    throw new InvalidOperationException($"An error happened in rendering loop while processing windows messages. Error: {Marshal.GetLastWin32Error()}");
                }

                // NCDESTROY event?
                if (msg.msg == 130)
                {
                    isControlAlive = false;
                }

                var message = new Message() { HWnd = msg.handle, LParam = msg.lParam, Msg = (int)msg.msg, WParam = msg.wParam };
                if (!Application.FilterMessage(ref message))
                {
                    _ = NativeMethods.TranslateMessage(ref msg);
                    _ = NativeMethods.DispatchMessage(ref msg);
                }
            }

            return isControlAlive || switchControl;
        }

        private void ControlDisposed(object sender, EventArgs e)
        {
            isControlAlive = false;
        }
    }
}
