using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.UI;

    /// <summary>
    /// Control interface
    /// </summary>
    public interface IUIControl : IUIEventsEvaluable
    {
        /// <summary>
        /// Active
        /// </summary>
        bool Active { get; set; }
        /// <summary>
        /// Visible
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Parent control
        /// </summary>
        /// <remarks>When a control has a parent, all the position, size, scale and rotation parameters, are relative to it.</remarks>
        IUIControl Parent { get; set; }
        /// <summary>
        /// Root control
        /// </summary>
        IUIControl Root { get; }
        /// <summary>
        /// Gets whether the control has a parent or not
        /// </summary>
        bool HasParent { get; }
        /// <summary>
        /// Gets whether the control is the root control
        /// </summary>
        bool IsRoot { get; }
        /// <summary>
        /// Children collection
        /// </summary>
        IEnumerable<IUIControl> Children { get; }

        /// <summary>
        /// Gets or sets the drawable height
        /// </summary>
        float Height { get; set; }
        /// <summary>
        /// Gets or sets the drawable width
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
        /// Gets the control's rectangle coordinates relative to immediate parent control position
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
        /// Tool-tip text
        /// </summary>
        string TooltipText { get; set; }

        /// <summary>
        /// Gets the rotation and scaling absolute pivot point
        /// </summary>
        /// <returns>Returns the control pivot point</returns>
        Vector2? GetTransformationPivot();
        /// <summary>
        /// Gets the current control's transform matrix
        /// </summary>
        /// <returns>Returns the transform matrix</returns>
        /// <remarks>If the control is parent-fitted, returns the parent's transform</remarks>
        Matrix GetTransform();

        /// <summary>
        /// Resize
        /// </summary>
        void Resize();

        /// <summary>
        /// Adds a child to the children collection
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="fitToParent">Fit control to parent</param>
        bool AddChild(IUIControl ctrl, bool fitToParent = false);
        /// <summary>
        /// Adds a children list to the children collection
        /// </summary>
        /// <param name="controls">Control list</param>
        /// <param name="fitToParent">Fit control to parent</param>
        bool AddChildren(IEnumerable<IUIControl> controls, bool fitToParent = false);
        /// <summary>
        /// Removes a child from the children collection
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="dispose">Removes from collection and disposes</param>
        bool RemoveChild(IUIControl ctrl, bool dispose = false);
        /// <summary>
        /// Removes a children list from the children collection
        /// </summary>
        /// <param name="controls">Control list</param>
        /// <param name="dispose">Removes from collection and disposes</param>
        bool RemoveChildren(IEnumerable<IUIControl> controls, bool dispose = false);
        /// <summary>
        /// Inserts a child at the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="ctrl">Control</param>
        /// <param name="fitToParent">Fit control to parent</param>
        bool InsertChild(int index, IUIControl ctrl, bool fitToParent = false);
        /// <summary>
        /// Inserts a children list at the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="controls">Control list</param>
        /// <param name="fitToParent">Fit control to parent</param>
        bool InsertChildren(int index, IEnumerable<IUIControl> controls, bool fitToParent = false);

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
        void MoveLeft(IGameTime gameTime, float distance = 1f);
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        void MoveRight(IGameTime gameTime, float distance = 1f);
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        void MoveUp(IGameTime gameTime, float distance = 1f);
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        void MoveDown(IGameTime gameTime, float distance = 1f);

        /// <summary>
        /// Sets the control left-top position
        /// </summary>
        /// <param name="x">Position X Component</param>
        /// <param name="y">Position Y Component</param>
        /// <remarks>Setting the position invalidates centering properties</remarks>
        void SetPosition(float x, float y);
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
        /// <param name="applyPadding">Apply the padding to the resulting rectangle, if any.</param>
        /// <returns>Returns the control render area</returns>
        RectangleF GetRenderArea(bool applyPadding);
        /// <summary>
        /// Gets the total control area in absolute coordinates from screen origin
        /// </summary>
        /// <returns>Returns the total control area</returns>
        RectangleF GetControlArea();
    }
}
