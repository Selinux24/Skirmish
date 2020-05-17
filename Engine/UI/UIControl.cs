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
        /// Click event
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Children collection
        /// </summary>
        private readonly List<UIControl> children = new List<UIControl>();
        /// <summary>
        /// Internal pressed button flag
        /// </summary>
        private bool pressed = false;
        /// <summary>
        /// Top position
        /// </summary>
        private int top;
        /// <summary>
        /// Left position
        /// </summary>
        private int left;
        /// <summary>
        /// Width
        /// </summary>
        private int width;
        /// <summary>
        /// Height
        /// </summary>
        private int height;
        /// <summary>
        /// Draws the sprite vertically centered on screen
        /// </summary>
        private bool centerVertically = false;
        /// <summary>
        /// Draws the sprite horizontally centered on screen
        /// </summary>
        private bool centerHorizontally = false;
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
        /// Parent control
        /// </summary>
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

        /// <summary>
        /// Active
        /// </summary>
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
        /// <summary>
        /// Visible
        /// </summary>
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
        /// Gets or sets if mouse button is pressed
        /// </summary>
        public bool Pressed
        {
            get
            {
                return this.pressed;
            }
            set
            {
                this.JustPressed = false;
                this.JustReleased = false;

                if (value && !this.pressed)
                {
                    this.JustPressed = true;
                }
                else if (!value && this.pressed)
                {
                    this.JustReleased = true;
                }

                this.pressed = value;

                children.ForEach(c => c.Pressed = value);
            }
        }
        /// <summary>
        /// Gest whether the button is just pressed
        /// </summary>
        public bool JustPressed { get; private set; }
        /// <summary>
        /// Gest whether the button is just released
        /// </summary>
        public bool JustReleased { get; private set; }
        /// <summary>
        /// Gets or sets whether the mouse is over the button rectangle
        /// </summary>
        public bool MouseOver { get; set; }

        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        public virtual int Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.left = value;
                this.centerHorizontally = false;

                UpdateInternals();
            }
        }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public virtual int Top
        {
            get
            {
                return this.top;
            }
            set
            {
                this.top = value;
                this.centerVertically = false;

                UpdateInternals();
            }
        }
        /// <summary>
        /// Width
        /// </summary>
        public virtual int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;

                UpdateInternals();
            }
        }
        /// <summary>
        /// Height
        /// </summary>
        public virtual int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;

                UpdateInternals();
            }
        }
        /// <summary>
        /// Scale
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

                UpdateInternals();
            }
        }
        /// <summary>
        /// Rotation
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

                UpdateInternals();
            }
        }

        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        protected int AbsoluteLeft
        {
            get
            {
                return (Parent?.AbsoluteLeft ?? 0) + this.left;
            }
        }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        protected int AbsoluteTop
        {
            get
            {
                return (Parent?.AbsoluteTop ?? 0) + this.top;
            }
        }
        /// <summary>
        /// Width
        /// </summary>
        protected int AbsoluteWidth
        {
            get
            {
                if (this.fitParent)
                {
                    return Parent?.AbsoluteWidth ?? this.width;
                }
                else
                {
                    return this.width;
                }
            }
        }
        /// <summary>
        /// Height
        /// </summary>
        protected int AbsoluteHeight
        {
            get
            {
                if (this.fitParent)
                {
                    return Parent?.AbsoluteHeight ?? this.height;
                }
                else
                {
                    return this.height;
                }
            }
        }
        /// <summary>
        /// Scale
        /// </summary>
        protected float AbsoluteScale
        {
            get
            {
                return (Parent?.AbsoluteScale ?? 1) * this.scale;
            }
        }
        /// <summary>
        /// Rotation
        /// </summary>
        protected float AbsoluteRotation
        {
            get
            {
                return (Parent?.AbsoluteRotation ?? 0) + this.rotation;
            }
        }

        protected int GrandpaLeft
        {
            get
            {
                return Parent?.GrandpaLeft ?? this.Left;
            }
        }

        protected int GrandpaTop
        {
            get
            {
                return Parent?.GrandpaTop ?? this.Top;
            }
        }
        /// <summary>
        /// Width
        /// </summary>
        protected int GrandpaWidth
        {
            get
            {
                return Parent?.GrandpaWidth ?? this.width;
            }
        }
        /// <summary>
        /// Height
        /// </summary>
        protected int GrandpaHeight
        {
            get
            {
                return Parent?.GrandpaHeight ?? this.height;
            }
        }
        /// <summary>
        /// Scale
        /// </summary>
        protected float GrandpaScale
        {
            get
            {
                return Parent?.GrandpaScale ?? this.scale;
            }
        }

        /// <summary>
        /// Absolute center
        /// </summary>
        public virtual Vector2 AbsoluteCenter
        {
            get
            {
                return RelativeCenter + new Vector2(left, top);
            }
        }
        /// <summary>
        /// Relative center
        /// </summary>
        public virtual Vector2 RelativeCenter
        {
            get
            {
                return new Vector2(this.width, this.height) * 0.5f;
            }
        }
        /// <summary>
        /// Sprite rectangle
        /// </summary>
        public virtual Rectangle Rectangle
        {
            get
            {
                return new Rectangle(
                    (int)(this.left - (this.width * 0.5f)),
                    (int)(this.top - (this.height * 0.5f)),
                    (int)(this.width * scale),
                    (int)(this.height * scale));
            }
        }
        /// <summary>
        /// Indicates whether the sprite has to maintain proportion with window size
        /// </summary>
        public virtual bool FitParent
        {
            get
            {
                return fitParent;
            }
            set
            {
                fitParent = value;

                children.ForEach(c => c.FitParent = value);
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
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator { get; private set; }

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

            UpdateInternals();
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                children.ForEach(c => c.Dispose());
                children.Clear();
            }
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            this.UpdateInternals();

            if (children.Any())
            {
                children.ForEach(c => c.Update(context));
            }
        }

        /// <summary>
        /// Draw components
        /// </summary>
        /// <param name="context">Draw context</param>
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
        /// Fires on-click event
        /// </summary>
        public void FireOnClickEvent()
        {
            this.OnClick(new EventArgs());
        }
        /// <summary>
        /// Launch on-click event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnClick(EventArgs e)
        {
            this.Click?.Invoke(this, e);
        }

        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {

        }
        /// <summary>
        /// Centers vertically the text
        /// </summary>
        public virtual void CenterVertically()
        {
            this.centerVertically = true;

            this.UpdateInternals();
        }
        /// <summary>
        /// Centers horinzontally the text
        /// </summary>
        public virtual void CenterHorizontally()
        {
            this.centerHorizontally = true;

            this.UpdateInternals();
        }
        /// <summary>
        /// Updates the internal transform
        /// </summary>
        protected virtual void UpdateInternals()
        {
            var type = this.GetType();

            if (this.centerHorizontally)
            {
                this.left = 0;
            }

            if (this.centerVertically)
            {
                this.top = 0;
            }

            Vector2 sca = new Vector2(this.AbsoluteWidth, this.AbsoluteHeight) * AbsoluteScale;
            float rot = this.AbsoluteRotation;
            Vector2 pos = new Vector2(this.AbsoluteLeft, this.AbsoluteTop);
            Vector2 rotCenter = new Vector2(this.GrandpaLeft, this.GrandpaTop);

            this.Manipulator.SetScale(sca);
            this.Manipulator.SetRotation(rot);
            this.Manipulator.SetPosition(pos);
            this.Manipulator.Update(rotCenter, this.GrandpaScale);

            if (children.Any())
            {
                children.ForEach(c => c.UpdateInternals());
            }
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

            ctrl.Parent = this;

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
    }
}
