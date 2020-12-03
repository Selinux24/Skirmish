using SharpDX;

namespace Engine
{
    using Engine.UI;

    /// <summary>
    /// Control interface
    /// </summary>
    public interface IUIControl
    {
        /// <summary>
        /// Mouse over event
        /// </summary>
        event MouseEventHandler MouseOver;
        /// <summary>
        /// Mouse enter event
        /// </summary>
        event MouseEventHandler MouseEnter;
        /// <summary>
        /// Mouse leave event
        /// </summary>
        event MouseEventHandler MouseLeave;
        /// <summary>
        /// Mouse pressed
        /// </summary>
        event MouseEventHandler MousePressed;
        /// <summary>
        /// Mouse just pressed
        /// </summary>
        event MouseEventHandler MouseJustPressed;
        /// <summary>
        /// Mouse just released
        /// </summary>
        event MouseEventHandler MouseJustReleased;
        /// <summary>
        /// Mouse click
        /// </summary>
        event MouseEventHandler MouseClick;
        /// <summary>
        /// Mouse double click
        /// </summary>
        event MouseEventHandler MouseDoubleClick;

        /// <summary>
        /// Gets or sets whether the control is enabled for event processing
        /// </summary>
        bool EventsEnabled { get; set; }
        /// <summary>
        /// Gets whether the mouse is over the button rectangle or not
        /// </summary>
        bool IsMouseOver { get; }
        /// <summary>
        /// Pressed buttons state flags
        /// </summary>
        MouseButtons PressedState { get; }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        float Height { get; set; }
        /// <summary>
        /// Gets or sets the width
        /// </summary>
        float Width { get; set; }

        /// <summary>
        /// Gets or sets the local scale
        /// </summary>
        float Scale { get; set; }
        /// <summary>
        /// Gets or sets the absolute scale
        /// </summary>
        float AbsoluteScale { get; }
        /// <summary>
        /// Gets or sets the local rotation
        /// </summary>
        float Rotation { get; set; }
        /// <summary>
        /// Gets or sets the absolute rotation
        /// </summary>
        float AbsoluteRotation { get; }
        /// <summary>
        /// Gets or sets the rotation and scale pivot anchor
        /// </summary>
        PivotAnchors PivotAnchor { get; set; }

        /// <summary>
        /// Gets or sets the (local) left coordinate value from parent or the screen origin
        /// </summary>
        float Left { get; set; }
        /// <summary>
        /// Gets the (absolute) left coordinate value the screen origin
        /// </summary>
        float AbsoluteLeft { get; }
        /// <summary>
        /// Gets or sets the (local) top coordinate value from parent or the screen origin
        /// </summary>
        float Top { get; set; }
        /// <summary>
        /// Gets the (absolute) top coordinate value from the screen origin
        /// </summary>
        float AbsoluteTop { get; }

        /// <summary>
        /// Gets the control's rectangle local coordinates
        /// </summary>
        RectangleF LocalRectangle { get; }
        /// <summary>
        /// Gets the control's rectangle absolute coordinates from screen origin
        /// </summary>
        RectangleF AbsoluteRectangle { get; }
        /// <summary>
        /// Gets the control's rectangle coordinates relative to inmediate parent control position
        /// </summary>
        RectangleF RelativeToParentRectangle { get; }
        /// <summary>
        /// Gets the control's rectangle coordinates relative to root control position
        /// </summary>
        RectangleF RelativeToRootRectangle { get; }

        /// <summary>
        /// Gets the control's local center coordinates
        /// </summary>
        Vector2 LocalCenter { get; }
        /// <summary>
        /// Gets the control's absolute center coordinates
        /// </summary>
        Vector2 AbsoluteCenter { get; }

        /// <summary>
        /// Spacing
        /// </summary>
        Spacing Spacing { get; set; }
        /// <summary>
        /// Padding
        /// </summary>
        Padding Padding { get; set; }
        /// <summary>
        /// Indicates whether the control has to maintain proportion with parent size
        /// </summary>
        bool FitWithParent { get; set; }
        /// <summary>
        /// Anchor
        /// </summary>
        Anchors Anchor { get; set; }

        /// <summary>
        /// Gets or sets the base color
        /// </summary>
        Color4 BaseColor { get; set; }
        /// <summary>
        /// Gets or sets the tint color
        /// </summary>
        Color4 TintColor { get; set; }
        /// <summary>
        /// Alpha color component
        /// </summary>
        float Alpha { get; set; }

        /// <summary>
        /// Tooltip text
        /// </summary>
        string TooltipText { get; set; }

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
        /// <summary>
        /// Sets the control rectangle area
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        /// <remarks>Adjust the control left-top position and with and height properties</remarks>
        void SetRectangle(RectangleF rectangle);

        /// <summary>
        /// Gets the render area in absolute coordinates from screen origin
        /// </summary>
        /// <param name="applyPadding">Apply the padding to the resulting reactangle, if any.</param>
        /// <returns>Returns the text render area</returns>
        RectangleF GetRenderArea(bool applyPadding);
    }
}
