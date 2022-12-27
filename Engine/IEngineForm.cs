using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Engine form interface
    /// </summary>
    public interface IEngineForm : IDisposable
    {
        /// <summary>
        /// Gets the primary screen size
        /// </summary>
        static Vector2 ScreenSize { get; }

#nullable enable
        /// <summary>
        /// Occurs when the form is resized.
        /// </summary>
        event EventHandler? Resize;
        /// <summary>
        /// Occurs when the form starts the resize process
        /// </summary>
        event EventHandler? ResizeBegin;
        /// <summary>
        /// Occurs when the form ends the resize process
        /// </summary>
        event EventHandler? ResizeEnd;
        /// <summary>
        /// Occurs when the form is activated
        /// </summary>
        event EventHandler? Activated;
        /// <summary>
        /// Occurs when the form is deactivated
        /// </summary>
        event EventHandler? Deactivate;
#nullable disable

        /// <summary>
        /// Gets or sets the form name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets or sets the form's caption text
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// Gets or sets the window handle
        /// </summary>
        nint Handle { get; }
        /// <summary>
        /// Gets or sets whether the form is in full-screen mode or not
        /// </summary>
        bool IsFullscreen { get; set; }
        /// <summary>
        /// Render width
        /// </summary>
        int RenderWidth { get; }
        /// <summary>
        /// Render height
        /// </summary>
        int RenderHeight { get; }
        /// <summary>
        /// Render rectangle
        /// </summary>
        RectangleF RenderRectangle
        {
            get
            {
                return new RectangleF(0, 0, RenderWidth, RenderHeight);
            }
        }
        /// <summary>
        /// Rneder area center
        /// </summary>
        Point RenderCenter { get; }
        /// <summary>
        /// Screen center
        /// </summary>
        Point ScreenCenter { get; }
        /// <summary>
        /// The form is manually resizing
        /// </summary>
        bool Resizing { get; }
        /// <summary>
        /// The form's size just changed
        /// </summary>
        bool SizeUpdated { get; }
        /// <summary>
        /// The form's mode just changed
        /// </summary>
        bool FormModeUpdated { get; }
        /// <summary>
        /// The form is minimized
        /// </summary>
        bool IsMinimized { get; }
        /// <summary>
        /// Mouse over flag
        /// </summary>
        bool MouseIn { get; }
        /// <summary>
        /// Mouse wheel delta
        /// </summary>
        int MouseWheelDelta { get; }
        /// <summary>
        /// Mouse wheel delta timestamp
        /// </summary>
        long MouseWheelDeltaTimestamp { get; }

        /// <summary>
        /// Initializes the form
        /// </summary>
        /// <param name="name">Form name</param>
        /// <param name="screenWidth">Width</param>
        /// <param name="screenHeight">Height</param>
        /// <param name="fullScreen">Full screen</param>
        void Initialize(string name, int screenWidth, int screenHeight, bool fullScreen);
        /// <summary>
        /// Gets the render viewport
        /// </summary>
        /// <returns></returns>
        Viewport GetViewport();
        /// <summary>
        /// Gets the current ortho projection matrix
        /// </summary>
        /// <returns>Returns the current ortho projection matrix</returns>
        Matrix GetOrthoProjectionMatrix();
        /// <summary>
        /// Transform to screen space using the form view ortho projection matrix
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the screen space position</returns>
        /// <remarks>Screen space: Center = (0,0) Left = -X Up = +Y</remarks>
        Vector2 ToScreenSpace(Vector2 position);
        /// <summary>
        /// Render loop callback initializer
        /// </summary>
        /// <param name="renderCallback">Render callback method</param>
        void Render(Action renderCallback);
        /// <summary>
        /// Closes the form
        /// </summary>
        void Close();
        /// <summary>
        ///  Computes the location of the screen point p in client coords.
        /// </summary>
        Point PointToClient(Point p);
        /// <summary>
        ///  Computes the location of the client point p in screen coords.
        /// </summary>
        Point PointToScreen(Point p);
    }
}
