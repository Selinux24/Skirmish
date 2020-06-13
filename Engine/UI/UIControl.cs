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
        /// Mouse over event
        /// </summary>
        public event EventHandler MouseOver;
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
        /// Draws the sprite vertically centered on the render area
        /// </summary>
        private CenterTargets centerVertically = CenterTargets.None;
        /// <summary>
        /// Draws the sprite horizontally centered on the render area
        /// </summary>
        private CenterTargets centerHorizontally = CenterTargets.None;
        /// <summary>
        /// Scale value
        /// </summary>
        private float scale = 1f;
        /// <summary>
        /// Rotation value
        /// </summary>
        private float rotation = 0f;
        /// <summary>
        /// Color
        /// </summary>
        private Color4 color = Color4.Black;
        /// <summary>
        /// Maintain proportion with window size
        /// </summary>
        private bool fitParent = false;
        /// <summary>
        /// Update internals flag
        /// </summary>
        private bool updateInternals = false;

        /// <summary>
        /// Manipulator
        /// </summary>
        protected Manipulator2D Manipulator { get; private set; }

        /// <summary>
        /// Parent control
        /// </summary>
        /// <remarks>When a control has a parent, all the position, size, scale and rotation parameters, are relative to it.</remarks>
        public UIControl Parent { get; protected set; }
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
        /// Gets whether the mouse is over the button rectangle or not
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return this.Contains(this.Game.Input.MousePosition);
            }
        }
        /// <summary>
        /// Gets whether the control is pressed or not
        /// </summary>
        public bool IsPressed
        {
            get
            {
                return IsMouseOver && this.Game.Input.LeftMouseButtonPressed;
            }
        }
        /// <summary>
        /// Gets whether the control is just pressed or not
        /// </summary>
        public bool IsJustPressed
        {
            get
            {
                return IsMouseOver && this.Game.Input.LeftMouseButtonJustPressed;
            }
        }
        /// <summary>
        /// Gets whether the control is just released or not
        /// </summary>
        public bool IsJustReleased
        {
            get
            {
                return IsMouseOver && this.Game.Input.LeftMouseButtonJustReleased;
            }
        }

        /// <summary>
        /// Gets or sets the left position in the render area
        /// </summary>
        public virtual float Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.left = value;
                this.centerHorizontally = CenterTargets.None;

                this.updateInternals = true;
            }
        }
        /// <summary>
        /// Gets or sets the top position in the render area
        /// </summary>
        public virtual float Top
        {
            get
            {
                return this.top;
            }
            set
            {
                this.top = value;
                this.centerVertically = CenterTargets.None;

                this.updateInternals = true;
            }
        }
        /// <summary>
        /// Gets or sets the width
        /// </summary>
        public virtual float Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;

                this.updateInternals = true;
            }
        }
        /// <summary>
        /// Gets or sets the height
        /// </summary>
        public virtual float Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;

                this.updateInternals = true;
            }
        }
        /// <summary>
        /// Gets or sets the scale
        /// </summary>
        public virtual float Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;

                this.updateInternals = true;
            }
        }
        /// <summary>
        /// Gets or sets the rotation
        /// </summary>
        public virtual float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value % MathUtil.TwoPi;

                this.updateInternals = true;
            }
        }
        /// <summary>
        /// Gets the control's rectangle coordinates in the render area
        /// </summary>
        public virtual RectangleF Rectangle
        {
            get
            {
                return new RectangleF(
                    this.Left,
                    this.Top,
                    this.Width,
                    this.Height);
            }
        }
        /// <summary>
        /// Gets the control's local center coordinates
        /// </summary>
        public virtual Vector2 Center
        {
            get
            {
                return new Vector2(this.width, this.height) * 0.5f;
            }
        }

        /// <summary>
        /// Gets or sets the left position in the render area, taking account the parent control configuration
        /// </summary>
        /// <remarks>Uses the parent position plus the actual relative position</remarks>
        public float AbsoluteLeft
        {
            get
            {
                return (Parent?.AbsoluteLeft ?? 0) + this.Left;
            }
        }
        /// <summary>
        /// Gets or sets the top position in the render area, taking account the parent control configuration
        /// </summary>
        public float AbsoluteTop
        {
            get
            {
                return (Parent?.AbsoluteTop ?? 0) + this.Top;
            }
        }
        /// <summary>
        /// Gets or sets the width, taking account the parent control configuration
        /// </summary>
        public float AbsoluteWidth
        {
            get
            {
                if (this.fitParent)
                {
                    return Parent?.AbsoluteWidth ?? this.Width;
                }
                else
                {
                    return this.Width;
                }
            }
        }
        /// <summary>
        /// Gets or sets the height, taking account the parent control configuration
        /// </summary>
        public float AbsoluteHeight
        {
            get
            {
                if (this.fitParent)
                {
                    return Parent?.AbsoluteHeight ?? this.Height;
                }
                else
                {
                    return this.Height;
                }
            }
        }
        /// <summary>
        /// Gets or sets the scale, taking account the parent control configuration
        /// </summary>
        public float AbsoluteScale
        {
            get
            {
                return (Parent?.AbsoluteScale ?? 1) * this.Scale;
            }
        }
        /// <summary>
        /// Gets or sets the rotation, taking account the parent control configuration
        /// </summary>
        public float AbsoluteRotation
        {
            get
            {
                return (Parent?.AbsoluteRotation ?? 0) + this.Rotation;
            }
        }
        /// <summary>
        /// Gets or sets the parent rectangle
        /// </summary>
        public RectangleF AbsoluteRectangle
        {
            get
            {
                return new RectangleF(
                    this.AbsoluteLeft,
                    this.AbsoluteTop,
                    this.AbsoluteWidth,
                    this.AbsoluteHeight);
            }
        }
        /// <summary>
        /// Gets the parent center coordinates in the render area
        /// </summary>
        public virtual Vector2 AbsoluteCenter
        {
            get
            {
                return AbsoluteRectangle.Center;
            }
        }

        /// <summary>
        /// Gets or sets the first's hierarchy item left position in the render area
        /// </summary>
        public float GrandpaLeft
        {
            get
            {
                return Parent?.GrandpaLeft ?? this.Left;
            }
        }
        /// <summary>
        /// Gets or sets the first's hierarchy item top position in the render area
        /// </summary>
        public float GrandpaTop
        {
            get
            {
                return Parent?.GrandpaTop ?? this.Top;
            }
        }
        /// <summary>
        /// Gets or sets the first's hierarchy item width
        /// </summary>
        public float GrandpaWidth
        {
            get
            {
                return Parent?.GrandpaWidth ?? this.Width;
            }
        }
        /// <summary>
        /// Gets or sets the first's hierarchy item height
        /// </summary>
        public float GrandpaHeight
        {
            get
            {
                return Parent?.GrandpaHeight ?? this.Height;
            }
        }
        /// <summary>
        /// Gets or sets the first's hierarchy item scale
        /// </summary>
        public float GrandpaScale
        {
            get
            {
                return Parent?.GrandpaScale ?? this.Scale;
            }
        }
        /// <summary>
        /// Gets or sets the first's hierarchy item rotation
        /// </summary>
        public float GrandpaRotation
        {
            get
            {
                return Parent?.GrandpaRotation ?? this.Rotation;
            }
        }
        /// <summary>
        /// Gets or sets the first's hierarchy item rectangle
        /// </summary>
        public RectangleF GrandpaRectangle
        {
            get
            {
                return new RectangleF(
                    this.GrandpaLeft,
                    this.GrandpaTop,
                    this.GrandpaWidth,
                    this.GrandpaHeight);
            }
        }
        /// <summary>
        /// Gets the first's hierarchy item center coordinates in the render area
        /// </summary>
        public virtual Vector2 GrandpaCenter
        {
            get
            {
                return GrandpaRectangle.Center;
            }
        }

        /// <summary>
        /// Indicates whether the sprite has to maintain proportion with window size
        /// </summary>
        public virtual bool FitParent
        {
            get
            {
                return this.fitParent;
            }
            set
            {
                this.fitParent = value;

                this.updateInternals = true;
            }
        }
        /// <summary>
        /// Base color
        /// </summary>
        public virtual Color4 Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;

                children.ForEach(c => c.Color = value);
            }
        }
        /// <summary>
        /// Alpha color component
        /// </summary>
        public virtual float Alpha
        {
            get
            {
                return color.Alpha;
            }
            set
            {
                color.Alpha = value;

                children.ForEach(c => c.Alpha = value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Button description</param>
        protected UIControl(Scene scene, UIControlDescription description)
            : base(scene, description)
        {
            this.Manipulator = new Manipulator2D(this.Game);

            this.fitParent = description.FitParent;
            if (fitParent)
            {
                this.top = 0;
                this.left = 0;
                this.width = Parent?.width ?? this.Game.Form.RenderWidth;
                this.height = Parent?.height ?? this.Game.Form.RenderHeight;
            }
            else
            {
                this.top = description.Top;
                this.left = description.Left;
                this.width = description.Width;
                this.height = description.Height;
            }

            this.centerHorizontally = description.CenterHorizontally;
            this.centerVertically = description.CenterVertically;

            this.color = description.Color;

            this.updateInternals = true;
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
            base.Update(context);

            if (!Active)
            {
                return;
            }

            if (this.updateInternals)
            {
                this.UpdateInternals();

                this.updateInternals = false;
            }

            this.UpdateInput();

            if (children.Any())
            {
                children.ForEach(c => c.Update(context));
            }
        }
        /// <summary>
        /// Updates the internal transform
        /// </summary>
        protected virtual void UpdateInternals()
        {
            if (this.centerHorizontally != CenterTargets.None)
            {
                var rect = this.GetCenteringArea(this.centerHorizontally);
                this.left = rect.Center.X - (this.AbsoluteWidth * 0.5f);
            }

            if (this.centerVertically != CenterTargets.None)
            {
                var rect = this.GetCenteringArea(this.centerVertically);
                this.top = rect.Center.Y - (this.AbsoluteHeight * 0.5f);
            }

            Vector2 sca = new Vector2(this.AbsoluteWidth, this.AbsoluteHeight) * AbsoluteScale;
            float rot = this.AbsoluteRotation;
            Vector2 pos = new Vector2(this.AbsoluteLeft, this.AbsoluteTop);

            this.Manipulator.SetScale(sca);
            this.Manipulator.SetRotation(rot);
            this.Manipulator.SetPosition(pos);
            this.Manipulator.Update(this.GrandpaRectangle.Center, this.GrandpaScale);

            if (children.Any())
            {
                children.ForEach(c => c.updateInternals = true);
            }
        }
        /// <summary>
        /// Updates the input state
        /// </summary>
        protected virtual void UpdateInput()
        {
            if (this.IsMouseOver)
            {
                this.FireMouseOverEvent();
            }

            if (this.IsPressed)
            {
                this.FirePressedEvent();
            }

            if (this.IsJustPressed)
            {
                this.FireJustPressedEvent();
            }

            if (this.IsJustReleased)
            {
                this.FireJustReleasedEvent();
            }
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
        /// Fires on mouse over event
        /// </summary>
        protected void FireMouseOverEvent()
        {
            this.MouseOver?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires on pressed event
        /// </summary>
        protected void FirePressedEvent()
        {
            this.Pressed?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires on just pressed event
        /// </summary>
        protected void FireJustPressedEvent()
        {
            this.JustPressed?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires on just released event
        /// </summary>
        protected void FireJustReleasedEvent()
        {
            this.JustReleased?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            children.ForEach(c => c.Resize());
        }
        /// <summary>
        /// Centers vertically the text
        /// </summary>
        /// <param name="target">Center target</param>
        public virtual void CenterVertically(CenterTargets target)
        {
            this.centerVertically = target;

            this.updateInternals = true;
        }
        /// <summary>
        /// Centers horinzontally the text
        /// </summary>
        /// <param name="target">Center target</param>
        public virtual void CenterHorizontally(CenterTargets target)
        {
            this.centerHorizontally = target;

            this.updateInternals = true;
        }

        /// <summary>
        /// Adds a child to the children collection
        /// </summary>
        /// <param name="ctrl">Control</param>
        public void AddChild(UIControl ctrl)
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
            ctrl.HasParent = true;

            if (!children.Contains(ctrl))
            {
                children.Add(ctrl);
            }
        }
        /// <summary>
        /// Adds a children list to the children collection
        /// </summary>
        /// <param name="controls">Control list</param>
        public void AddChildren(IEnumerable<UIControl> controls)
        {
            if (!controls.Any())
            {
                return;
            }

            foreach (var ctrl in controls)
            {
                AddChild(ctrl);
            }
        }
        /// <summary>
        /// Removes a child from the children collection
        /// </summary>
        /// <param name="ctrl">Control</param>
        public void RemoveChild(UIControl ctrl)
        {
            if (ctrl == null)
            {
                return;
            }

            if (children.Contains(ctrl))
            {
                ctrl.Parent = null;
                ctrl.HasParent = false;

                children.Remove(ctrl);
            }
        }
        /// <summary>
        /// Removes a children list from the children collection
        /// </summary>
        /// <param name="controls">Control list</param>
        public void RemoveChildren(IEnumerable<UIControl> controls)
        {
            if (!controls.Any())
            {
                return;
            }

            foreach (var ctrl in controls)
            {
                RemoveChild(ctrl);
            }
        }

        /// <summary>
        /// Gets whether the control contains the point or not
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Returns true if the point is contained into the control rectangle</returns>
        public bool Contains(Point point)
        {
            return AbsoluteRectangle.Contains(point.X, point.Y);
        }

        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        public void MoveLeft(GameTime gameTime, float distance = 1f)
        {
            this.Left -= (int)(1f * distance * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        public void MoveRight(GameTime gameTime, float distance = 1f)
        {
            this.Left += (int)(1f * distance * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        public void MoveUp(GameTime gameTime, float distance = 1f)
        {
            this.Top -= (int)(1f * distance * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="distance">Distance</param>
        public void MoveDown(GameTime gameTime, float distance = 1f)
        {
            this.Top += (int)(1f * distance * gameTime.ElapsedSeconds);
        }

        /// <summary>
        /// Sets the control left-top position
        /// </summary>
        /// <param name="x">Position X Component</param>
        /// <param name="y">Position Y Component</param>
        /// <remarks>Setting the position invalidates centering properties</remarks>
        public virtual void SetPosition(float x, float y)
        {
            this.SetPosition(new Vector2(x, y));
        }
        /// <summary>
        /// Sets the control left-top position
        /// </summary>
        /// <param name="position">Position</param>
        /// <remarks>Setting the position invalidates centering properties</remarks>
        public virtual void SetPosition(Vector2 position)
        {
            this.left = (int)position.X;
            this.top = (int)position.Y;
            this.centerHorizontally = CenterTargets.None;
            this.centerVertically = CenterTargets.None;

            this.updateInternals = true;
        }
        /// <summary>
        /// Sets the control rectangle area
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        /// <remarks>Adjust the control left-top position and with and height properties</remarks>
        public virtual void SetRectangle(RectangleF rectangle)
        {
            this.left = rectangle.X;
            this.top = rectangle.Y;
            this.width = rectangle.Width;
            this.height = rectangle.Height;
            this.centerHorizontally = CenterTargets.None;
            this.centerVertically = CenterTargets.None;

            this.updateInternals = true;
        }

        /// <summary>
        /// Gets the render area
        /// </summary>
        /// <returns>Returns the render area</returns>
        public virtual RectangleF GetRenderArea()
        {
            return this.AbsoluteRectangle;
        }
        /// <summary>
        /// Gets the area used for centering calculation
        /// </summary>
        /// <param name="target">Center target</param>
        /// <returns>Returns the centering area</returns>
        public virtual RectangleF GetCenteringArea(CenterTargets target)
        {
            if (this.Parent != null && target == CenterTargets.Parent)
            {
                return new RectangleF(0, 0, this.Parent.AbsoluteWidth, this.Parent.AbsoluteHeight);
            }

            return this.Game.Form.RenderRectangle;
        }
    }

    /// <summary>
    /// Centering targets
    /// </summary>
    public enum CenterTargets
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Parent
        /// </summary>
        Parent = 1,
        /// <summary>
        /// Screen
        /// </summary>
        Screen = 2,
    }
}
