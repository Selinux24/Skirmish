﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// User interface control
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public abstract class UIControl<T>(Scene scene, string id, string name) : Drawable<T>(scene, id, name), IUIControl, IScreenFitted where T : UIControlDescription
    {
        /// <inheritdoc/>
        public event MouseEventHandler MouseOver;
        /// <inheritdoc/>
        public event MouseEventHandler MouseEnter;
        /// <inheritdoc/>
        public event MouseEventHandler MouseLeave;
        /// <inheritdoc/>
        public event MouseEventHandler MousePressed;
        /// <inheritdoc/>
        public event MouseEventHandler MouseJustPressed;
        /// <inheritdoc/>
        public event MouseEventHandler MouseJustReleased;
        /// <inheritdoc/>
        public event MouseEventHandler MouseClick;
        /// <inheritdoc/>
        public event MouseEventHandler MouseDoubleClick;
        /// <inheritdoc/>
        public event EventHandler SetFocus;
        /// <inheritdoc/>
        public event EventHandler LostFocus;

        /// <summary>
        /// Update order value
        /// </summary>
        private int updateOrder;
        /// <summary>
        /// Children collection
        /// </summary>
        private readonly List<IUIControl> children = [];
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
        /// Last clicked event time
        /// </summary>
        private TimeSpan lastClickedTime = TimeSpan.Zero;
        /// <summary>
        /// Last clicked buttons
        /// </summary>
        private MouseButtons lastClickedButtons = MouseButtons.None;

        /// <summary>
        /// Manipulator
        /// </summary>
        protected Manipulator2D Manipulator { get; private set; }
        /// <summary>
        /// Update internals flag
        /// </summary>
        protected bool UpdateInternals = false;

        /// <inheritdoc/>
        public IUIControl Parent { get; set; }
        /// <inheritdoc/>
        public IUIControl Root
        {
            get
            {
                return Parent?.Root ?? Parent;
            }
        }
        /// <inheritdoc/>
        public bool HasParent { get { return Parent != null; } }
        /// <inheritdoc/>
        public bool IsRoot { get { return Root == null; } }
        /// <inheritdoc/>
        public IEnumerable<IUIControl> Children
        {
            get
            {
                return [.. children];
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

                var childrenArray = children.ToArray();
                foreach (var c in childrenArray)
                {
                    c.Active = value;
                }
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

                var childrenArray = children.ToArray();
                foreach (var c in childrenArray)
                {
                    c.Visible = value;
                }
            }
        }

        /// <inheritdoc/>
        public bool EventsEnabled { get; set; }
        /// <inheritdoc/>
        public bool IsMouseOver { get; private set; }
        /// <inheritdoc/>
        public MouseButtons PressedState { get; private set; } = MouseButtons.None;

        /// <inheritdoc/>
        public virtual float Width
        {
            get
            {
                return width;
            }
            set
            {
                if (!MathUtil.NearEqual(width, value))
                {
                    width = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <inheritdoc/>
        public virtual float Height
        {
            get
            {
                return height;
            }
            set
            {
                if (!MathUtil.NearEqual(height, value))
                {
                    height = value;

                    UpdateInternals = true;
                }
            }
        }

        /// <inheritdoc/>
        public virtual float Scale
        {
            get
            {
                return scale;
            }
            set
            {
                if (!MathUtil.NearEqual(scale, value))
                {
                    scale = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <inheritdoc/>
        public virtual float AbsoluteScale
        {
            get
            {
                return (Parent?.AbsoluteScale ?? 1) * Scale;
            }
        }
        /// <inheritdoc/>
        public virtual float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                float v = value % MathUtil.TwoPi;
                if (!MathUtil.NearEqual(rotation, v))
                {
                    rotation = v;

                    UpdateInternals = true;
                }
            }
        }
        /// <inheritdoc/>
        public virtual float AbsoluteRotation
        {
            get
            {
                return (Parent?.AbsoluteRotation ?? 0) + Rotation;
            }
        }
        /// <inheritdoc/>
        public virtual PivotAnchors PivotAnchor
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

        /// <inheritdoc/>
        public virtual float Left
        {
            get
            {
                return left;
            }
            set
            {
                if (!MathUtil.NearEqual(left, value))
                {
                    left = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <inheritdoc/>
        public virtual float AbsoluteLeft
        {
            get
            {
                return (Parent?.AbsoluteLeft ?? 0) + Left;
            }
        }
        /// <inheritdoc/>
        public virtual float Top
        {
            get
            {
                return top;
            }
            set
            {
                if (!MathUtil.NearEqual(top, value))
                {
                    top = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <inheritdoc/>
        public virtual float AbsoluteTop
        {
            get
            {
                return (Parent?.AbsoluteTop ?? 0) + Top;
            }
        }

        /// <inheritdoc/>
        public virtual RectangleF LocalRectangle
        {
            get
            {
                return new RectangleF(0, 0, Width, Height);
            }
        }
        /// <inheritdoc/>
        public virtual RectangleF AbsoluteRectangle
        {
            get
            {
                return new RectangleF(AbsoluteLeft, AbsoluteTop, Width, Height);
            }
        }
        /// <inheritdoc/>
        public virtual RectangleF RelativeToParentRectangle
        {
            get
            {
                return new RectangleF(Left, Top, Width, Height);
            }
        }
        /// <inheritdoc/>
        public virtual RectangleF RelativeToRootRectangle
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

        /// <inheritdoc/>
        public virtual Vector2 LocalCenter
        {
            get
            {
                return new Vector2(Width * 0.5f, Height * 0.5f);
            }
        }
        /// <inheritdoc/>
        public virtual Vector2 AbsoluteCenter
        {
            get
            {
                return AbsoluteRectangle.Center;
            }
        }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

                var childrenArray = children.ToArray();
                foreach (var c in childrenArray)
                {
                    c.BaseColor = value;
                }
            }
        }
        /// <inheritdoc/>
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

                var childrenArray = children.ToArray();
                foreach (var c in childrenArray)
                {
                    c.TintColor = value;
                }
            }
        }
        /// <inheritdoc/>
        public virtual float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                if (!MathUtil.NearEqual(alpha, value))
                {
                    alpha = value;
                }

                var childrenArray = children.ToArray();
                foreach (var c in childrenArray)
                {
                    c.Alpha = value;
                }
            }
        }

        /// <inheritdoc/>
        public string TooltipText { get; set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var childrenArray = children.ToArray();
                foreach (var c in childrenArray.OfType<IDisposable>())
                {
                    c.Dispose();
                }

                children.Clear();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(T description)
        {
            await base.ReadAssets(description);

            updateOrder = UIControlHelper.GetNextUpdateOrder();

            Manipulator = new Manipulator2D(Game);

            padding = description.Padding;
            spacing = description.Spacing;
            anchor = description.Anchor;
            fitWithParent = description.FitParent;

            if (fitWithParent)
            {
                top = 0;
                left = 0;
                width = Parent?.Width ?? Game.Form.RenderWidth;
                height = Parent?.Height ?? Game.Form.RenderHeight;
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

            var childrenArray = children.ToArray();
            foreach (var c in childrenArray.OfType<IUpdatable>())
            {
                c.Update(context);
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
                var size = new Vector2(rect.Width, rect.Height);
                var pos = new Vector2(rect.Left, rect.Top);
                var sca = Vector2.One * AbsoluteScale;
                float rot = AbsoluteRotation;

                Manipulator.SetSize(size);
                Manipulator.SetScale(sca);
                Manipulator.SetRotation(rot);
                Manipulator.SetPosition(pos);

                var parentPos = GetTransformationPivot();

                Manipulator.Update2D(parentPos);
            }

            var childrenArray = children.ToArray();
            foreach (var c in childrenArray)
            {
                c.Invalidate();
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
        /// <inheritdoc/>
        public int GetUpdateOrder()
        {
            return updateOrder;
        }
        /// <inheritdoc/>
        public virtual void Invalidate()
        {
            UpdateInternals = true;
        }
        /// <inheritdoc/>
        public virtual Vector2? GetTransformationPivot()
        {
            if (IsRoot)
            {
                return null;
            }

            var rect = AbsoluteRectangle;
            if (PivotAnchor.HasFlag(PivotAnchors.Root)) rect = Root.AbsoluteRectangle;
            if (PivotAnchor.HasFlag(PivotAnchors.Parent)) rect = Parent.AbsoluteRectangle;
            return PivotAnchor switch
            {
                PivotAnchors.TopLeft => rect.TopLeft,
                PivotAnchors.TopRight => rect.TopRight,
                PivotAnchors.BottomLeft => rect.BottomLeft,
                PivotAnchors.BottomRight => rect.BottomRight,
                _ => rect.Center,
            };
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            base.Draw(context);

            if (!Visible)
            {
                return false;
            }

            bool drawn = false;

            var childrenArray = children.ToArray();
            foreach (var c in childrenArray.OfType<IDrawable>())
            {
                drawn = c.Draw(context) || drawn;
            }

            return drawn;
        }

        /// <inheritdoc/>
        public virtual Matrix GetTransform()
        {
            return fitWithParent && HasParent ? Parent.GetTransform() : Manipulator.LocalTransform;
        }

        /// <summary>
        /// Fires on mouse over event
        /// </summary>
        /// <param name="pointerPosition">Pointer position</param>
        /// <param name="buttons">Pressed buttons</param>
        protected virtual void FireMouseOverEvent(Point pointerPosition, MouseButtons buttons)
        {
            MouseOver?.Invoke(this, new MouseEventArgs
            {
                PointerPosition = pointerPosition,
                Buttons = buttons,
            });
        }
        /// <summary>
        /// Fires on mouse enter event
        /// </summary>
        /// <param name="pointerPosition">Pointer position</param>
        /// <param name="buttons">Pressed buttons</param>
        protected virtual void FireMouseEnterEvent(Point pointerPosition, MouseButtons buttons)
        {
            MouseEnter?.Invoke(this, new MouseEventArgs
            {
                PointerPosition = pointerPosition,
                Buttons = buttons,
            });
        }
        /// <summary>
        /// Fires on mouse leave event
        /// </summary>
        /// <param name="pointerPosition">Pointer position</param>
        /// <param name="buttons">Pressed buttons</param>
        protected virtual void FireMouseLeaveEvent(Point pointerPosition, MouseButtons buttons)
        {
            MouseLeave?.Invoke(this, new MouseEventArgs
            {
                PointerPosition = pointerPosition,
                Buttons = buttons,
            });
        }
        /// <summary>
        /// Fires on pressed event
        /// </summary>
        /// <param name="pointerPosition">Pointer position</param>
        /// <param name="buttons">Pressed buttons</param>
        protected virtual void FirePressedEvent(Point pointerPosition, MouseButtons buttons)
        {
            MousePressed?.Invoke(this, new MouseEventArgs
            {
                PointerPosition = pointerPosition,
                Buttons = buttons,
            });
        }
        /// <summary>
        /// Fires on just pressed event
        /// </summary>
        /// <param name="pointerPosition">Pointer position</param>
        /// <param name="justPressedButtons">Just pressed buttons</param>
        protected virtual void FireJustPressedEvent(Point pointerPosition, MouseButtons justPressedButtons)
        {
            MouseJustPressed?.Invoke(this, new MouseEventArgs
            {
                PointerPosition = pointerPosition,
                Buttons = justPressedButtons,
            });
        }
        /// <summary>
        /// Fires on just released event
        /// </summary>
        /// <param name="pointerPosition">Pointer position</param>
        /// <param name="justReleasedButtons">Just released buttons</param>
        protected virtual void FireJustReleasedEvent(Point pointerPosition, MouseButtons justReleasedButtons)
        {
            MouseJustReleased?.Invoke(this, new MouseEventArgs
            {
                PointerPosition = pointerPosition,
                Buttons = justReleasedButtons,
            });
        }
        /// <summary>
        /// Fires on click event
        /// </summary>
        /// <param name="pointerPosition">Pointer position</param>
        /// <param name="clickedButtons">Clicked buttons</param>
        protected virtual void FireMouseClickEvent(Point pointerPosition, MouseButtons clickedButtons)
        {
            MouseClick?.Invoke(this, new MouseEventArgs
            {
                PointerPosition = pointerPosition,
                Buttons = clickedButtons,
            });

            if ((Game.GameTime.TotalTime - lastClickedTime).TotalMilliseconds <= GameEnvironment.DoubleClickTime)
            {
                var doubleClickedButtons = lastClickedButtons & clickedButtons;
                if (doubleClickedButtons != MouseButtons.None)
                {
                    FireMouseDoubleClickEvent(pointerPosition, doubleClickedButtons);

                    lastClickedTime = TimeSpan.Zero;
                    lastClickedButtons = MouseButtons.None;

                    return;
                }
            }

            lastClickedTime = Game.GameTime.TotalTime;
            lastClickedButtons = clickedButtons;
        }
        /// <summary>
        /// Fires on double click event
        /// </summary>
        /// <param name="pointerPosition">Pointer position</param>
        /// <param name="doubleClickedButtons">Double clicked buttons</param>
        protected virtual void FireMouseDoubleClickEvent(Point pointerPosition, MouseButtons doubleClickedButtons)
        {
            MouseDoubleClick?.Invoke(this, new MouseEventArgs
            {
                PointerPosition = pointerPosition,
                Buttons = doubleClickedButtons,
            });
        }
        /// <summary>
        /// Fires on set focus event
        /// </summary>
        protected virtual void FireSetFocusEvent()
        {
            SetFocus?.Invoke(this, new EventArgs() { });
        }
        /// <summary>
        /// Fires on lost focus event
        /// </summary>
        protected virtual void FireLostFocusEvent()
        {
            LostFocus?.Invoke(this, new EventArgs() { });
        }

        /// <inheritdoc/>
        public virtual void Resize()
        {
            UpdateInternals = true;

            var childrenArray = children.ToArray();
            foreach (var c in childrenArray)
            {
                c.Resize();
            }
        }

        /// <summary>
        /// Validates the control for adding to the children list
        /// </summary>
        /// <param name="ctrl">Control</param>
        private bool ValidateAddChild(IUIControl ctrl)
        {
            if (ctrl == null)
            {
                return false;
            }

            if (ctrl == this)
            {
                return false;
            }

            if (children.Contains(ctrl))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Validates the control list for adding to the children list
        /// </summary>
        /// <param name="controls">Control list</param>
        private bool ValidateAddChildren(IEnumerable<IUIControl> controls)
        {
            if (!controls.Any())
            {
                return false;
            }

            if (controls.Any(ctrl => !ValidateAddChild(ctrl)))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Validates the control for removing from the children list
        /// </summary>
        /// <param name="ctrl">Control</param>
        private bool ValidateRemoveChild(IUIControl ctrl)
        {
            if (ctrl == null)
            {
                return false;
            }

            if (!children.Contains(ctrl))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Validates the control list for removing from the children list
        /// </summary>
        /// <param name="controls">Control list</param>
        private bool ValidateRemoveChildren(IEnumerable<IUIControl> controls)
        {
            if (!controls.Any())
            {
                return false;
            }

            if (controls.Any(ctrl => !ValidateRemoveChild(ctrl)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds the control to the children collection
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="fitToParent">Fits to parent</param>
        private void AddChildInternal(IUIControl ctrl, bool fitToParent = true)
        {
            ctrl.Parent = this;
            ctrl.FitWithParent = fitToParent;
            ctrl.PivotAnchor = PivotAnchors.Default;

            children.Add(ctrl);
        }
        /// <summary>
        /// Removes the control from the children collection
        /// </summary>
        /// <param name="ctrl">Control</param>
        /// <param name="dispose">Disposes the control, if possible</param>
        private void RemoveChildInternal(IUIControl ctrl, bool dispose = false)
        {
            ctrl.Parent = null;
            ctrl.FitWithParent = false;

            children.Remove(ctrl);

            if (dispose && ctrl is IDisposable ctrlDisposable)
            {
                ctrlDisposable.Dispose();
            }
        }
        /// <summary>
        /// Inserts the control to the children collection at position
        /// </summary>
        /// <param name="index">Position index</param>
        /// <param name="ctrl">Control</param>
        /// <param name="fitToParent">Fits to parent</param>
        private void InsertChildInternal(int index, IUIControl ctrl, bool fitToParent = true)
        {
            ctrl.Parent = this;
            ctrl.FitWithParent = fitToParent;
            ctrl.PivotAnchor = PivotAnchors.Default;

            children.Insert(index, ctrl);
        }

        /// <inheritdoc/>
        public bool AddChild(IUIControl ctrl, bool fitToParent = false)
        {
            if (!ValidateAddChild(ctrl))
            {
                return false;
            }

            AddChildInternal(ctrl, fitToParent);

            UpdateInternalState();

            return true;
        }
        /// <inheritdoc/>
        public bool AddChildren(IEnumerable<IUIControl> controls, bool fitToParent = false)
        {
            if (!ValidateAddChildren(controls))
            {
                return false;
            }

            foreach (var ctrl in controls)
            {
                AddChildInternal(ctrl, fitToParent);
            }

            UpdateInternalState();

            return true;
        }
        /// <inheritdoc/>
        public bool RemoveChild(IUIControl ctrl, bool dispose = false)
        {
            if (!ValidateRemoveChild(ctrl))
            {
                return false;
            }

            RemoveChildInternal(ctrl, dispose);

            UpdateInternalState();

            return true;
        }
        /// <inheritdoc/>
        public bool RemoveChildren(IEnumerable<IUIControl> controls, bool dispose = false)
        {
            if (!ValidateRemoveChildren(controls))
            {
                return false;
            }

            foreach (var ctrl in controls)
            {
                RemoveChildInternal(ctrl, dispose);
            }

            UpdateInternalState();

            return true;
        }
        /// <inheritdoc/>
        public bool InsertChild(int index, IUIControl ctrl, bool fitToParent = false)
        {
            if (index < 0 || index >= children.Count)
            {
                return false;
            }

            if (!ValidateAddChild(ctrl))
            {
                return false;
            }

            InsertChildInternal(index, ctrl, fitToParent);

            UpdateInternalState();

            return true;
        }
        /// <inheritdoc/>
        public bool InsertChildren(int index, IEnumerable<IUIControl> controls, bool fitToParent = false)
        {
            if (index < 0 || index >= children.Count)
            {
                return false;
            }

            if (!ValidateAddChildren(controls))
            {
                return false;
            }

            int i = index;
            foreach (var ctrl in controls)
            {
                InsertChildInternal(i++, ctrl, fitToParent);
            }

            UpdateInternalState();

            return true;
        }

        /// <inheritdoc/>
        public virtual bool Contains(Point point)
        {
            var rect = GetRenderArea(false);

            var contains = rect.Scale(AbsoluteScale).Contains(point.X, point.Y);

            return contains;
        }

        /// <inheritdoc/>
        public virtual void MoveLeft(IGameTime gameTime, float distance = 1f)
        {
            Left -= (int)(1f * distance * gameTime.ElapsedSeconds);
        }
        /// <inheritdoc/>
        public virtual void MoveRight(IGameTime gameTime, float distance = 1f)
        {
            Left += (int)(1f * distance * gameTime.ElapsedSeconds);
        }
        /// <inheritdoc/>
        public virtual void MoveUp(IGameTime gameTime, float distance = 1f)
        {
            Top -= (int)(1f * distance * gameTime.ElapsedSeconds);
        }
        /// <inheritdoc/>
        public virtual void MoveDown(IGameTime gameTime, float distance = 1f)
        {
            Top += (int)(1f * distance * gameTime.ElapsedSeconds);
        }

        /// <inheritdoc/>
        public virtual void SetPosition(float x, float y)
        {
            left = x;
            top = y;
            anchor = Anchors.None;

            UpdateInternals = true;
        }
        /// <inheritdoc/>
        public virtual void SetPosition(Vector2 position)
        {
            SetPosition(position.X, position.Y);
        }
        /// <inheritdoc/>
        public virtual void SetRectangle(RectangleF rectangle)
        {
            left = rectangle.Left;
            top = rectangle.Top;
            width = rectangle.Width;
            height = rectangle.Height;
            anchor = Anchors.None;

            UpdateInternals = true;
        }

        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public virtual RectangleF GetControlArea()
        {
            return GetRenderArea(false);
        }

        /// <inheritdoc/>
        public virtual bool IsEvaluable()
        {
            return Active && Visible;
        }
        /// <inheritdoc/>
        public virtual void InitControlState()
        {
            var input = Game.Input;

            IsMouseOver = Contains(input.MousePosition);

            if (EventsEnabled)
            {
                var justReleasedButtons = PressedState & ~input.MouseButtonsState;

                //Evaluates mouse leave event
                if (!IsMouseOver && prevIsMouseOver)
                {
                    //Update the control pressed state
                    PressedState = input.MouseButtonsState;

                    FireMouseLeaveEvent(input.MousePosition, input.MouseButtonsState);
                    prevIsMouseOver = false;
                }

                //Evaluate the just released event
                if (!IsMouseOver && justReleasedButtons != MouseButtons.None)
                {
                    //Update the control pressed state
                    PressedState = input.MouseButtonsState;

                    FireJustReleasedEvent(input.MousePosition, justReleasedButtons);
                }
            }
            else
            {
                //This flag is only for events evaluation
                prevIsMouseOver = false;

                //Update the control pressed state
                PressedState = MouseButtons.None;
            }

            if (!Children.Any())
            {
                return;
            }

            foreach (var child in Children)
            {
                child.InitControlState();
            }
        }
        /// <inheritdoc/>
        public virtual void EvaluateTopMostControl(out IUIControl topMostControl, out IUIControl focusedControl)
        {
            topMostControl = null;
            focusedControl = null;

            IUIControl topControl = this;
            while (topControl != null)
            {
                if (topControl.EventsEnabled)
                {
                    topMostControl = topControl;

                    topControl.EvaluateEventsEnabledControl(out var focusControl);
                    if (focusControl != null)
                    {
                        focusedControl = focusControl;
                    }
                }

                //Get the new evaluable top most control in the children list
                topControl = topControl.Children.LastOrDefault(c => c.IsEvaluable() && c.IsMouseOver);
            }
        }
        /// <inheritdoc/>
        public virtual void EvaluateEventsEnabledControl(out IUIControl focusedControl)
        {
            focusedControl = null;

            var input = Game.Input;

            var justPressedButtons = input.MouseButtonsState & ~PressedState;
            var justReleasedButtons = PressedState & ~input.MouseButtonsState;

            //Update the control pressed state
            PressedState = input.MouseButtonsState;

            //Mouse is over
            FireMouseOverEvent(input.MousePosition, input.MouseButtonsState);

            //Evaluate mouse enter
            if (!prevIsMouseOver)
            {
                FireMouseEnterEvent(input.MousePosition, input.MouseButtonsState);
            }

            //Only the top most control is considered the mouse-over control
            prevIsMouseOver = true;

            //Evaluate the pressed state
            if (input.MouseButtonsState != MouseButtons.None)
            {
                FirePressedEvent(input.MousePosition, input.MouseButtonsState);
            }

            //Evaluate the just pressed event
            if (justPressedButtons != MouseButtons.None)
            {
                FireJustPressedEvent(input.MousePosition, justPressedButtons);
            }

            //Evaluate the just released event
            if (justReleasedButtons != MouseButtons.None)
            {
                FireJustReleasedEvent(input.MousePosition, justReleasedButtons);
                FireMouseClickEvent(input.MousePosition, justReleasedButtons);

                //Focus changed
                focusedControl = this;
            }
        }
        /// <inheritdoc/>
        public void SetFocusControl()
        {
            Scene.SetFocus(this);

            FireSetFocusEvent();
        }
        /// <inheritdoc/>
        public void SetFocusLost()
        {
            Scene.ClearFocus();

            FireLostFocusEvent();
        }
    }

    /// <summary>
    /// UIControl helper
    /// </summary>
    static class UIControlHelper
    {
        /// <summary>
        /// Next update order
        /// </summary>
        private static int UpdateOrderSeed = 0;
        /// <summary>
        /// Gets the next order
        /// </summary>
        /// <returns>Returns the next order</returns>
        public static int GetNextUpdateOrder()
        {
            return ++UpdateOrderSeed;
        }
    }
}
