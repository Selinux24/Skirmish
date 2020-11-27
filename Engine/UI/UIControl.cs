using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// User interface control
    /// </summary>
    public abstract class UIControl : Drawable, IUIControl, IScreenFitted
    {
        /// <summary>
        /// Next update order
        /// </summary>
        private static int UpdateOrderSeed = 0;
        /// <summary>
        /// Gets the next order
        /// </summary>
        /// <returns>Returns the next order</returns>
        private static int GetNextUpdateOrder()
        {
            return ++UpdateOrderSeed;
        }

        /// <summary>
        /// Mouse over event
        /// </summary>
        public event EventHandler MouseOver;
        /// <summary>
        /// Mouse enter event
        /// </summary>
        public event EventHandler MouseEnter;
        /// <summary>
        /// Mouse leave event
        /// </summary>
        public event EventHandler MouseLeave;
        /// <summary>
        /// Mouse pressed
        /// </summary>
        public event EventHandler Pressed;
        /// <summary>
        /// Mouse just pressed
        /// </summary>
        public event EventHandler JustPressed;
        /// <summary>
        /// Mouse just released
        /// </summary>
        public event EventHandler JustReleased;

        /// <summary>
        /// Evaluates input over the specified scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="capturedControl">Returns the control wich captures the mouse event</param>
        public static UIControl EvaluateInput(Scene scene)
        {
            var input = scene.Game.Input;

            var sortedControls = scene.GetComponents()
                .OfType<UIControl>()
                .OrderBy(c => c.updateOrder)
                .ToList();

            sortedControls.ForEach(c => InitControlState(input.MousePosition, c));

            var mouseOverCtrls = sortedControls.Where(c => IsEvaluable(c)).ToList();
            if (!mouseOverCtrls.Any())
            {
                return null;
            }

            var topMostParent = mouseOverCtrls.Last();

            bool mouseCaptured = EvaluateInputControl(scene, topMostParent, mouseOverCtrls);
            if (mouseCaptured)
            {
                mouseOverCtrls.ForEach(c => c.prevIsMouseOver = false);
                topMostParent.prevIsMouseOver = true;

                return topMostParent;
            }

            return null;
        }
        /// <summary>
        /// Evaluates input over the specified scene control
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="ctrl">Controls</param>
        /// <param name="mouseOverCtrls">Mouse over controls</param>
        /// <returns>Returns true if any control fires a mouse click event</returns>
        private static bool EvaluateInputControl(Scene scene, UIControl ctrl, List<UIControl> mouseOverCtrls)
        {
            var input = scene.Game.Input;

            bool mouseCaptured = false;
            UIControl topChildren = ctrl;
            while (topChildren != null)
            {
                topChildren.FireMouseOverEvent();
                if (!topChildren.prevIsMouseOver)
                {
                    topChildren.FireMouseEnterEvent();
                }

                mouseOverCtrls.ForEach(c => c.prevIsMouseOver = false);
                topChildren.prevIsMouseOver = true;

                if (input.LeftMouseButtonPressed)
                {
                    topChildren.IsPressed = true;
                    topChildren.FirePressedEvent();

                    mouseCaptured = true;
                }

                if (input.LeftMouseButtonJustPressed)
                {
                    topChildren.IsJustPressed = true;
                    topChildren.FireJustPressedEvent();

                    mouseCaptured = true;
                }

                if (input.LeftMouseButtonJustReleased)
                {
                    topChildren.IsJustReleased = true;
                    topChildren.FireJustReleasedEvent();

                    mouseCaptured = true;
                }

                mouseOverCtrls = topChildren.Children.Where(c => IsEvaluable(c)).ToList();
                topChildren = mouseOverCtrls.LastOrDefault();
            }

            return mouseCaptured;
        }
        /// <summary>
        /// Clears mouse over property in the hierarchy
        /// </summary>
        /// <param name="ctrl">Control</param>
        private static void ClearPrevMouseOver(UIControl ctrl)
        {
            ctrl.prevIsMouseOver = false;

            ctrl.Children.ToList().ForEach(c => ClearPrevMouseOver(c));
        }
        /// <summary>
        /// Gets whether the specified UI control is event-evaluable or not
        /// </summary>
        /// <param name="ctrl">UI control</param>
        /// <returns>Returns true if the control is evaluable for UI events</returns>
        private static bool IsEvaluable(UIControl ctrl)
        {
            if (ctrl == null)
            {
                return false;
            }

            return ctrl.Active && ctrl.Visible && ctrl.EventsEnabled && ctrl.IsMouseOver;
        }
        /// <summary>
        /// Initializes the UI state
        /// </summary>
        /// <param name="mousePosition">Mouse position</param>
        /// <param name="ctrl">Control</param>
        private static void InitControlState(Point mousePosition, UIControl ctrl)
        {
            if (!ctrl.Active || !ctrl.Visible)
            {
                ctrl.prevIsMouseOver = false;
                ctrl.IsMouseOver = false;

                ctrl.IsPressed = false;
                ctrl.IsJustPressed = false;
                ctrl.IsJustReleased = false;
            }
            else
            {
                ctrl.IsMouseOver = ctrl.Contains(mousePosition);
                if (!ctrl.IsMouseOver)
                {
                    if (ctrl.prevIsMouseOver)
                    {
                        ctrl.FireMouseLeaveEvent();
                    }

                    ctrl.prevIsMouseOver = false;
                }

                ctrl.IsPressed = false;
                ctrl.IsJustPressed = false;
                ctrl.IsJustReleased = false;
            }

            if (!ctrl.Children.Any())
            {
                return;
            }

            foreach (var child in ctrl.Children)
            {
                InitControlState(mousePosition, child);
            }
        }

        /// <summary>
        /// Update order value
        /// </summary>
        private readonly int updateOrder;
        /// <summary>
        /// Children collection
        /// </summary>
        private readonly List<UIControl> children = new List<UIControl>();
        /// <summary>
        /// Top position
        /// </summary>
        private float top;
        /// <summary>
        /// Left position
        /// </summary>
        private float left;
        /// <summary>
        /// Width
        /// </summary>
        private float width;
        /// <summary>
        /// Height
        /// </summary>
        private float height;
        /// <summary>
        /// Spacing
        /// </summary>
        private Spacing spacing;
        /// <summary>
        /// Padding
        /// </summary>
        private Padding padding;
        /// <summary>
        /// Anchor
        /// </summary>
        private Anchors anchor;
        /// <summary>
        /// Scale value
        /// </summary>
        private float scale = 1f;
        /// <summary>
        /// Rotation value
        /// </summary>
        private float rotation = 0f;
        /// <summary>
        /// Rotation and scale pivot anchor
        /// </summary>
        private PivotAnchors pivotAnchor = PivotAnchors.Default;
        /// <summary>
        /// Base color
        /// </summary>
        private Color4 baseColor;
        /// <summary>
        /// Tint color
        /// </summary>
        private Color4 tintColor;
        /// <summary>
        /// Alpha color component
        /// </summary>
        private float alpha = 1f;
        /// <summary>
        /// Maintain proportion with parent size
        /// </summary>
        private bool fitWithParent = false;
        /// <summary>
        /// Indicates whether the mouse was previously pressed or not
        /// </summary>
        private bool prevIsMouseOver = false;

        /// <summary>
        /// Manipulator
        /// </summary>
        protected Manipulator2D Manipulator { get; private set; }
        /// <summary>
        /// Update internals flag
        /// </summary>
        protected bool UpdateInternals = false;

        /// <summary>
        /// Parent control
        /// </summary>
        /// <remarks>When a control has a parent, all the position, size, scale and rotation parameters, are relative to it.</remarks>
        public UIControl Parent { get; protected set; }
        /// <summary>
        /// Root control
        /// </summary>
        public UIControl Root
        {
            get
            {
                return Parent?.Root ?? Parent;
            }
        }
        /// <summary>
        /// Gets whether the control has a parent or not
        /// </summary>
        public bool HasParent { get { return Parent != null; } }
        /// <summary>
        /// Gets whether the control is the root control
        /// </summary>
        public bool IsRoot { get { return Root == null; } }
        /// <summary>
        /// Children collection
        /// </summary>
        public IEnumerable<UIControl> Children
        {
            get
            {
                return children.ToArray();
            }
        }

        /// <inheritdoc/>
        public override bool Active
        {
            get
            {
                return base.Active;
            }
            set
            {
                base.Active = value;

                children.ForEach(c => c.Active = value);
            }
        }
        /// <inheritdoc/>
        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;

                children.ForEach(c => c.Visible = value);
            }
        }

        /// <summary>
        /// Gets or sets whether the control is enabled for event processing
        /// </summary>
        public virtual bool EventsEnabled { get; set; } = true;
        /// <summary>
        /// Gets whether the mouse is over the button rectangle or not
        /// </summary>
        public virtual bool IsMouseOver { get; protected set; }
        /// <summary>
        /// Gets whether the control is pressed or not
        /// </summary>
        public virtual bool IsPressed { get; protected set; }
        /// <summary>
        /// Gets whether the control is just pressed or not
        /// </summary>
        public virtual bool IsJustPressed { get; protected set; }
        /// <summary>
        /// Gets whether the control is just released or not
        /// </summary>
        public virtual bool IsJustReleased { get; protected set; }

        /// <summary>
        /// Gets or sets the width
        /// </summary>
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                if (width != value)
                {
                    width = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the height
        /// </summary>
        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                if (height != value)
                {
                    height = value;

                    UpdateInternals = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the local scale
        /// </summary>
        public float Scale
        {
            get
            {
                return scale;
            }
            set
            {
                if (scale != value)
                {
                    scale = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the absolute scale
        /// </summary>
        public float AbsoluteScale
        {
            get
            {
                return (Parent?.AbsoluteScale ?? 1) * Scale;
            }
        }
        /// <summary>
        /// Gets or sets the local rotation
        /// </summary>
        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                float v = value % MathUtil.TwoPi;
                if (rotation != v)
                {
                    rotation = v;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the absolute rotation
        /// </summary>
        public float AbsoluteRotation
        {
            get
            {
                return (Parent?.AbsoluteRotation ?? 0) + Rotation;
            }
        }
        /// <summary>
        /// Gets or sets the rotation and scale pivot anchor
        /// </summary>
        public PivotAnchors PivotAnchor
        {
            get
            {
                return pivotAnchor;
            }
            set
            {
                if (pivotAnchor != value)
                {
                    pivotAnchor = value;

                    UpdateInternals = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the (local) left coordinate value from parent or the screen origin
        /// </summary>
        public float Left
        {
            get
            {
                return left;
            }
            set
            {
                if (left != value)
                {
                    left = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets the (absolute) left coordinate value the screen origin
        /// </summary>
        public float AbsoluteLeft
        {
            get
            {
                return (Parent?.AbsoluteLeft ?? 0) + Left;
            }
        }
        /// <summary>
        /// Gets or sets the (local) top coordinate value from parent or the screen origin
        /// </summary>
        public float Top
        {
            get
            {
                return top;
            }
            set
            {
                if (top != value)
                {
                    top = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets the (absolute) top coordinate value from the screen origin
        /// </summary>
        public float AbsoluteTop
        {
            get
            {
                return (Parent?.AbsoluteTop ?? 0) + Top;
            }
        }

        /// <summary>
        /// Gets the control's rectangle local coordinates
        /// </summary>
        public RectangleF LocalRectangle
        {
            get
            {
                return new RectangleF(0, 0, Width, Height);
            }
        }
        /// <summary>
        /// Gets the control's rectangle absolute coordinates from screen origin
        /// </summary>
        public RectangleF AbsoluteRectangle
        {
            get
            {
                return new RectangleF(AbsoluteLeft, AbsoluteTop, Width, Height);
            }
        }
        /// <summary>
        /// Gets the control's rectangle coordinates relative to inmediate parent control position
        /// </summary>
        public RectangleF RelativeToParentRectangle
        {
            get
            {
                return new RectangleF(Left, Top, Width, Height);
            }
        }
        /// <summary>
        /// Gets the control's rectangle coordinates relative to root control position
        /// </summary>
        public RectangleF RelativeToRootRectangle
        {
            get
            {
                var rect = AbsoluteRectangle;

                if (Root != null)
                {
                    return new RectangleF
                    {
                        Left = rect.Left - Root.Left,
                        Top = rect.Top - Root.Top,
                        Width = Width,
                        Height = Height,
                    };
                }

                return rect;
            }
        }

        /// <summary>
        /// Gets the control's local center coordinates
        /// </summary>
        public Vector2 LocalCenter
        {
            get
            {
                return new Vector2(Width * 0.5f, Height * 0.5f);
            }
        }
        /// <summary>
        /// Gets the control's absolute center coordinates
        /// </summary>
        public Vector2 AbsoluteCenter
        {
            get
            {
                return AbsoluteRectangle.Center;
            }
        }

        /// <summary>
        /// Spacing
        /// </summary>
        public virtual Spacing Spacing
        {
            get
            {
                return spacing;
            }
            set
            {
                if (spacing != value)
                {
                    spacing = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Padding
        /// </summary>
        public virtual Padding Padding
        {
            get
            {
                return padding;
            }
            set
            {
                if (padding != value)
                {
                    padding = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Indicates whether the control has to maintain proportion with parent size
        /// </summary>
        public virtual bool FitWithParent
        {
            get
            {
                return fitWithParent;
            }
            set
            {
                if (fitWithParent != value)
                {
                    fitWithParent = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Anchor
        /// </summary>
        public virtual Anchors Anchor
        {
            get
            {
                return anchor;
            }
            set
            {
                if (anchor != value)
                {
                    anchor = value;

                    UpdateInternals = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the base color
        /// </summary>
        public virtual Color4 BaseColor
        {
            get
            {
                return baseColor;
            }
            set
            {
                if (baseColor != value)
                {
                    baseColor = value;
                }

                children.ForEach(c => c.BaseColor = value);
            }
        }
        /// <summary>
        /// Gets or sets the tint color
        /// </summary>
        public virtual Color4 TintColor
        {
            get
            {
                return tintColor;
            }
            set
            {
                if (tintColor != value)
                {
                    tintColor = value;
                }

                children.ForEach(c => c.TintColor = value);
            }
        }
        /// <summary>
        /// Alpha color component
        /// </summary>
        public virtual float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                if (alpha != value)
                {
                    alpha = value;
                }

                children.ForEach(c => c.Alpha = value);
            }
        }

        /// <summary>
        /// Tooltip text
        /// </summary>
        public string TooltipText { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Button description</param>
        protected UIControl(string name, Scene scene, UIControlDescription description)
            : base(name, scene, description)
        {
            updateOrder = GetNextUpdateOrder();

            Manipulator = new Manipulator2D(Game);

            padding = description.Padding;
            spacing = description.Spacing;
            anchor = description.Anchor;
            fitWithParent = description.FitParent;

            if (fitWithParent)
            {
                top = 0;
                left = 0;
                width = Parent?.width ?? Game.Form.RenderWidth;
                height = Parent?.height ?? Game.Form.RenderHeight;
            }
            else
            {
                top = description.Top;
                left = description.Left;
                width = description.Width;
                height = description.Height;
            }

            baseColor = description.BaseColor;
            tintColor = description.TintColor;

            EventsEnabled = description.EventsEnabled;

            UpdateInternals = true;
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                children.ForEach(c => c.Dispose());
                children.Clear();
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (!Active)
            {
                return;
            }

            if (UpdateInternals)
            {
                UpdateInternalState();

                UpdateInternals = false;
            }

            if (children.Any())
            {
                children.ForEach(c => c.Update(context));
            }
        }
        /// <summary>
        /// Updates the internal transform
        /// </summary>
        protected virtual void UpdateInternalState()
        {
            if (!fitWithParent || Parent == null)
            {
                UpdateAnchorPositions();

                var rect = GetRenderArea(true);
                Vector2 size = new Vector2(rect.Width, rect.Height);
                Vector2 pos = new Vector2(rect.Left, rect.Top);
                Vector2 sca = Vector2.One * AbsoluteScale;
                float rot = AbsoluteRotation;

                Manipulator.SetSize(size);
                Manipulator.SetScale(sca);
                Manipulator.SetRotation(rot);
                Manipulator.SetPosition(pos);

                Vector2? parentPos = GetTransformationPivot();

                Manipulator.Update2D(parentPos);
            }

            if (children.Any())
            {
                children.ForEach(c => c.UpdateInternals = true);
            }
        }
        /// <summary>
        /// Updates the control position, based on the specified anchor value
        /// </summary>
        protected virtual void UpdateAnchorPositions()
        {
            if (anchor == Anchors.None)
            {
                return;
            }

            var areaRect = Root?.AbsoluteRectangle ?? Game.Form.RenderRectangle;

            if (anchor.HasFlag(Anchors.VerticalCenter))
            {
                top = areaRect.Center.Y - (Height * 0.5f);
            }

            if (anchor.HasFlag(Anchors.Top))
            {
                top = areaRect.Top;
            }

            if (anchor.HasFlag(Anchors.Bottom))
            {
                top = areaRect.Bottom - Height;
            }

            if (anchor.HasFlag(Anchors.HorizontalCenter))
            {
                left = areaRect.Center.X - (Width * 0.5f);
            }

            if (anchor.HasFlag(Anchors.Left))
            {
                left = areaRect.Left;
            }

            if (anchor.HasFlag(Anchors.Right))
            {
                left = areaRect.Right - Width;
            }
        }
        /// <summary>
        /// Gets the rotation and scaling absolute pivot point
        /// </summary>
        /// <returns>Returns the control pivot point</returns>
        public virtual Vector2? GetTransformationPivot()
        {
            if (IsRoot)
            {
                return null;
            }

            RectangleF rect = AbsoluteRectangle;
            if (PivotAnchor.HasFlag(PivotAnchors.Root)) rect = Root.AbsoluteRectangle;
            if (PivotAnchor.HasFlag(PivotAnchors.Parent)) rect = Parent.AbsoluteRectangle;

            Vector2 result;

            switch (PivotAnchor)
            {
                case PivotAnchors.TopLeft:
                    result = rect.TopLeft;
                    break;
                case PivotAnchors.TopRight:
                    result = rect.TopRight;
                    break;
                case PivotAnchors.BottomLeft:
                    result = rect.BottomLeft;
                    break;
                case PivotAnchors.BottomRight:
                    result = rect.BottomRight;
                    break;
                default:
                    result = rect.Center;
                    break;
            }

            return result;
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            base.Draw(context);

            if (!Visible)
            {
                return;
            }

            if (children.Any())
            {
                children.ForEach(c => c.Draw(context));
            }
        }

        /// <summary>
        /// Gets the current control's transform matrix
        /// </summary>
        /// <returns>Returns the transform matrix</returns>
        /// <remarks>If the control is parent-fitted, returns the parent's transform</remarks>
        public Matrix GetTransform()
        {
            return fitWithParent && HasParent ? Parent.GetTransform() : Manipulator.LocalTransform;
        }

        /// <summary>
        /// Fires on mouse over event
        /// </summary>
        protected void FireMouseOverEvent()
        {
            MouseOver?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires on mouse enter event
        /// </summary>
        protected void FireMouseEnterEvent()
        {
            MouseEnter?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires on mouse leave event
        /// </summary>
        protected void FireMouseLeaveEvent()
        {
            MouseLeave?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires on pressed event
        /// </summary>
        protected void FirePressedEvent()
        {
            Pressed?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires on just pressed event
        /// </summary>
        protected void FireJustPressedEvent()
        {
            JustPressed?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires on just released event
        /// </summary>
        protected void FireJustReleasedEvent()
        {
            JustReleased?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            UpdateInternals = true;

            children.ForEach(c => c.Resize());
        }

        /// <summary>
        /// Adds a child to the children collection
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="fitToParent">Fit control to parent</param>
        public void AddChild(UIControl ctrl, bool fitToParent = true)
        {
            if (ctrl == null)
            {
                return;
            }

            if (ctrl == this)
            {
                return;
            }

            ctrl.Parent = this;
            ctrl.FitWithParent = fitToParent;
            ctrl.PivotAnchor = PivotAnchors.Default;

            if (!children.Contains(ctrl))
            {
                children.Add(ctrl);
            }
        }
        /// <summary>
        /// Adds a children list to the children collection
        /// </summary>
        /// <param name="controls">Control list</param>
        /// <param name="fitToParent">Fit control to parent</param>
        public void AddChildren(IEnumerable<UIControl> controls, bool fitToParent = true)
        {
            if (!controls.Any())
            {
                return;
            }

            foreach (var ctrl in controls)
            {
                AddChild(ctrl, fitToParent);
            }
        }
        /// <summary>
        /// Removes a child from the children collection
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="dispose">Removes from collection and disposes</param>
        public void RemoveChild(UIControl ctrl, bool dispose = false)
        {
            if (ctrl == null)
            {
                return;
            }

            if (children.Contains(ctrl))
            {
                ctrl.Parent = null;
                ctrl.FitWithParent = false;

                children.Remove(ctrl);

                if (dispose) ctrl.Dispose();
            }
        }
        /// <summary>
        /// Removes a children list from the children collection
        /// </summary>
        /// <param name="controls">Control list</param>
        /// <param name="dispose">Removes from collection and disposes</param>
        public void RemoveChildren(IEnumerable<UIControl> controls, bool dispose = false)
        {
            if (!controls.Any())
            {
                return;
            }

            foreach (var ctrl in controls)
            {
                RemoveChild(ctrl, dispose);
            }
        }
        /// <summary>
        /// Inserts a child at the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="ctrl">Control</param>
        /// <param name="fitToParent">Fit control to parent</param>
        public void InsertChild(int index, UIControl ctrl, bool fitToParent = true)
        {
            if (ctrl == null)
            {
                return;
            }

            if (ctrl == this)
            {
                return;
            }

            ctrl.Parent = this;
            ctrl.FitWithParent = fitToParent;

            if (!children.Contains(ctrl))
            {
                children.Insert(index, ctrl);
            }
        }

        /// <summary>
        /// Gets whether the control contains the point or not
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Returns true if the point is contained into the control rectangle</returns>
        public bool Contains(Point point)
        {
            var rect = GetRenderArea(false);

            return rect.Scale(AbsoluteScale).Contains(point.X, point.Y);
        }

        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        public void MoveLeft(GameTime gameTime, float distance = 1f)
        {
            Left -= (int)(1f * distance * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        public void MoveRight(GameTime gameTime, float distance = 1f)
        {
            Left += (int)(1f * distance * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        public void MoveUp(GameTime gameTime, float distance = 1f)
        {
            Top -= (int)(1f * distance * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        public void MoveDown(GameTime gameTime, float distance = 1f)
        {
            Top += (int)(1f * distance * gameTime.ElapsedSeconds);
        }

        /// <summary>
        /// Sets the control left-top position
        /// </summary>
        /// <param name="x">Position X Component</param>
        /// <param name="y">Position Y Component</param>
        /// <remarks>Setting the position invalidates centering properties</remarks>
        public virtual void SetPosition(float x, float y)
        {
            left = x;
            top = y;
            anchor = Anchors.None;

            UpdateInternals = true;
        }
        /// <summary>
        /// Sets the control left-top position
        /// </summary>
        /// <param name="position">Position</param>
        /// <remarks>Setting the position invalidates centering properties</remarks>
        public virtual void SetPosition(Vector2 position)
        {
            SetPosition(position.X, position.Y);
        }
        /// <summary>
        /// Sets the control rectangle area
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        /// <remarks>Adjust the control left-top position and with and height properties</remarks>
        public virtual void SetRectangle(RectangleF rectangle)
        {
            left = rectangle.Left;
            top = rectangle.Top;
            width = rectangle.Width;
            height = rectangle.Height;
            anchor = Anchors.None;

            UpdateInternals = true;
        }

        /// <summary>
        /// Gets the render area in absolute coordinates from screen origin
        /// </summary>
        /// <param name="applyPadding">Apply the padding to the resulting reactangle, if any.</param>
        /// <returns>Returns the render area</returns>
        public virtual RectangleF GetRenderArea(bool applyPadding)
        {
            RectangleF renderArea;

            if (fitWithParent && Parent != null)
            {
                renderArea = Parent.GetRenderArea(true);
            }
            else
            {
                renderArea = AbsoluteRectangle;
            }

            return applyPadding ? Padding.Apply(renderArea) : renderArea;
        }
    }
}
