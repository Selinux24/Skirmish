using SharpDX;
using System;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// User interface control
    /// </summary>
    public abstract class UIControl : Drawable, IUIControl, IScreenFitted
    {
        /// <summary>
        /// Creates view and orthoprojection from specified size
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns view * orthoprojection matrix</returns>
        public static Matrix CreateViewOrthoProjection(int width, int height)
        {
            Vector3 pos = new Vector3(0, 0, -1);

            Matrix view = Matrix.LookAtLH(
                pos,
                pos + Vector3.ForwardLH,
                Vector3.Up);

            Matrix projection = Matrix.OrthoLH(
                width,
                height,
                0f, 100f);

            return view * projection;
        }

        /// <summary>
        /// Click event
        /// </summary>
        public event EventHandler Click;

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
                    this.left,
                    this.top,
                    (int)(this.width * scale),
                    (int)(this.height * scale));
            }
        }
        /// <summary>
        /// Indicates whether the sprite has to maintain proportion with window size
        /// </summary>
        public virtual bool FitScreen { get; set; }
        /// <summary>
        /// Base color
        /// </summary>
        public Color4 Color { get; set; }
        /// <summary>
        /// Alpha color component
        /// </summary>
        public virtual float Alpha
        {
            get
            {
                return Color.Alpha;
            }
            set
            {
                var col = Color;

                col.Alpha = value;

                Color = col;
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
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator { get; private set; } = new Manipulator2D();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Button description</param>
        protected UIControl(Scene scene, UIControlDescription description)
            : base(scene, description)
        {
            this.FitScreen = description.FitScreen;
            if (FitScreen)
            {
                this.top = 0;
                this.left = 0;
                this.width = this.Game.Form.RenderWidth;
                this.height = this.Game.Form.RenderHeight;
            }
            else
            {
                this.top = description.Top;
                this.left = description.Left;
                this.width = description.Width;
                this.height = description.Height;

                this.centerHorizontally = description.CenterHorizontally;
                this.centerVertically = description.CenterVertically;
            }

            this.Color = description.Color;

            UpdateInternals();
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            this.Manipulator.Update();
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
        private void UpdateInternals()
        {
            Vector2 center = new Vector2(-this.Game.Form.RelativeCenter.X, this.Game.Form.RelativeCenter.Y);

            Vector2 pos = Vector2.Zero;
            if (this.centerHorizontally)
            {
                var relCenterX = this.width * scale * 0.5f;

                this.left = (int)(center.X - relCenterX);
                pos.X = -relCenterX;
            }
            else
            {
                pos.X = this.left + center.X;
            }

            if (this.centerVertically)
            {
                var relCenterY = this.height * scale * 0.5f;

                this.top = (int)(center.Y - relCenterY);
                pos.Y = -relCenterY;
            }
            else
            {
                pos.Y = this.top - center.Y;
            }

            Vector2 sca = new Vector2(this.width, this.height) * scale;

            this.Manipulator.SetScale(sca);
            this.Manipulator.SetRotation(rotation);
            this.Manipulator.SetPosition(pos);
        }
    }
}
