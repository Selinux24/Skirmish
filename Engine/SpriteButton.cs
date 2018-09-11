using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sprite button
    /// </summary>
    public class SpriteButton : Drawable, IControl, IScreenFitted
    {
        /// <summary>
        /// Click event
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Pressed sprite button
        /// </summary>
        private Sprite buttonPressed = null;
        /// <summary>
        /// Release sprite button
        /// </summary>
        private Sprite buttonReleased = null;
        /// <summary>
        /// Button text drawer
        /// </summary>
        private TextDrawer text = null;
        /// <summary>
        /// Internal pressed button flag
        /// </summary>
        private bool pressed = false;

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
        public int Top { get; set; }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public int Left { get; set; }
        /// <summary>
        /// Button width
        /// </summary>
        public int Width { get { return this.buttonReleased.Width; } }
        /// <summary>
        /// Button height
        /// </summary>
        public int Height { get { return this.buttonReleased.Height; } }
        /// <summary>
        /// Gets or sets the button text
        /// </summary>
        public string Text
        {
            get
            {
                if (this.text != null)
                {
                    return this.text.Text;
                }

                return null;
            }
            set
            {
                if (this.text != null)
                {
                    this.text.Text = value;
                }
            }
        }
        /// <summary>
        /// Gets bounding rectangle of button
        /// </summary>
        /// <remarks>Bounding rectangle without text</remarks>
        public Rectangle Rectangle { get { return this.buttonReleased.Rectangle; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Button description</param>
        public SpriteButton(Scene scene, SpriteButtonDescription description)
            : base(scene, description)
        {
            var spriteDesc = new SpriteDescription()
            {
                Width = description.Width,
                Height = description.Height,
                Color = description.ColorReleased,
                FitScreen = false,
            };

            if (!string.IsNullOrEmpty(description.TextureReleased))
            {
                spriteDesc.Textures = new[] { description.TextureReleased };
                spriteDesc.UVMap = description.TextureReleasedUVMap;
            }

            this.buttonReleased = new Sprite(scene, spriteDesc);

            if (description.TwoStateButton)
            {
                var spriteDesc2 = new SpriteDescription()
                {
                    Width = description.Width,
                    Height = description.Height,
                    Color = description.ColorPressed,
                    FitScreen = false,
                };

                if (!string.IsNullOrEmpty(description.TexturePressed))
                {
                    spriteDesc2.Textures = new[] { description.TexturePressed };
                    spriteDesc2.UVMap = description.TexturePressedUVMap;
                }

                this.buttonPressed = new Sprite(scene, spriteDesc2);
            }

            if (description.TextDescription != null)
            {
                this.text = new TextDrawer(
                    scene,
                    description.TextDescription);
            }

            this.Left = description.Left;
            this.Top = description.Top;
            this.Text = description.Text;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SpriteButton()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Releases used resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.buttonReleased != null)
                {
                    this.buttonReleased.Dispose();
                    this.buttonReleased = null;
                }

                if (this.buttonPressed != null)
                {
                    this.buttonPressed.Dispose();
                    this.buttonPressed = null;
                }

                if (this.text != null)
                {
                    this.text.Dispose();
                    this.text = null;
                }
            }
        }
        /// <summary>
        /// Updates state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.buttonReleased.Left = this.Left;
            this.buttonReleased.Top = this.Top;

            if (this.buttonPressed != null)
            {
                this.buttonPressed.Left = this.Left;
                this.buttonPressed.Top = this.Top;
            }

            if (!string.IsNullOrEmpty(this.Text))
            {
                //Center text
                float leftmove = ((float)this.Width * 0.5f) - ((float)this.text.Width * 0.5f);
                float topmove = ((float)this.Height * 0.5f) - ((float)this.text.Height * 0.5f);

                this.text.Left = this.Left + (int)leftmove;
                this.text.Top = this.Top + (int)topmove;

                this.text.Update(context);
            }

            this.buttonReleased.Update(context);
            if (this.buttonPressed != null) this.buttonPressed.Update(context);
        }
        /// <summary>
        /// Draws button
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.pressed && this.buttonPressed != null)
            {
                this.buttonPressed.Draw(context);
            }
            else
            {
                this.buttonReleased.Draw(context);
            }

            if (!string.IsNullOrEmpty(this.Text))
            {
                this.text.Draw(context);
            }
        }
        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            this.buttonReleased.Resize();
            if (this.buttonPressed != null) this.buttonPressed.Resize();
            if (this.text != null) this.text.Resize();
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
    }
}
