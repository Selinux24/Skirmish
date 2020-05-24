using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Control interface
    /// </summary>
    public interface IUIControl
    {
        /// <summary>
        /// Mouse over event
        /// </summary>
        event EventHandler MouseOver;
        /// <summary>
        /// Mouse pressed
        /// </summary>
        event EventHandler Pressed;
        /// <summary>
        /// Mouse just pressed
        /// </summary>
        event EventHandler JustPressed;
        /// <summary>
        /// Mouse just released
        /// </summary>
        event EventHandler JustReleased;

        /// <summary>
        /// Gets whether the mouse is over the button rectangle or not
        /// </summary>
        bool IsMouseOver { get; }
        /// <summary>
        /// Gets whether the control is pressed or not
        /// </summary>
        bool IsPressed { get; }
        /// <summary>
        /// Gets whether the control is just pressed or not
        /// </summary>
        bool IsJustPressed { get; }
        /// <summary>
        /// Gets whether the control is just released or not
        /// </summary>
        bool IsJustReleased { get; }

        /// <summary>
        /// Gets or sets text left position in the render area
        /// </summary>
        int Left { get; set; }
        /// <summary>
        /// Gets or sets text top position in the render area
        /// </summary>
        int Top { get; set; }
        /// <summary>
        /// Gets or sets the width
        /// </summary>
        int Width { get; set; }
        /// <summary>
        /// Gets or sets the height
        /// </summary>
        int Height { get; set; }
        /// <summary>
        /// Gets or sets the scale
        /// </summary>
        float Scale { get; set; }
        /// <summary>
        /// Gets or sets the rotation
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Gets the control's rectangle coordinates in the render area
        /// </summary>
        Rectangle Rectangle { get; }
        /// <summary>
        /// Gets the control's center coordinates in the render area
        /// </summary>
        Vector2 AbsoluteCenter { get; }
        /// <summary>
        /// Gets the control's local center coordinates
        /// </summary>
        Vector2 RelativeCenter { get; }

        /// <summary>
        /// Indicates whether the sprite has to maintain proportion with window size
        /// </summary>
        bool FitParent { get; set; }
        /// <summary>
        /// Base color
        /// </summary>
        Color4 Color { get; set; }
        /// <summary>
        /// Alpha color component
        /// </summary>
        float Alpha { get; set; }

        /// <summary>
        /// Gets whether the control contains the point or not
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Returns true if the point is contained into the control rectangle</returns>
        bool Contains(Point point);

        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        void MoveLeft(GameTime gameTime, float distance = 1f);
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        void MoveRight(GameTime gameTime, float distance = 1f);
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        void MoveUp(GameTime gameTime, float distance = 1f);
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        void MoveDown(GameTime gameTime, float distance = 1f);

        /// <summary>
        /// Sets the control left-top position
        /// </summary>
        /// <param name="position">Position</param>
        /// <remarks>Setting the position invalidates centering properties</remarks>
        void SetPosition(Vector2 position);
    }
}
