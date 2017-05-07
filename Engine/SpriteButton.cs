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
        /// Button sprite
        /// </summary>
        private Sprite button = null;
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

                this.button.TextureIndex = this.pressed ? 1 : 0;
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
        public int Width { get { return (int)this.button.Width; } }
        /// <summary>
        /// Button height
        /// </summary>
        public int Height { get { return (int)this.button.Height; } }
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
        public Rectangle Rectangle
        {
            get { return this.button.Rectangle; }
        }
        /// <summary>
        /// Maximum number of instances
        /// </summary>
        public override int Count
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Button description</param>
        public SpriteButton(Game game, BufferManager bufferManager, SpriteButtonDescription description)
            : base(game, bufferManager, description)
        {
            this.button = new Sprite(
                game,
                bufferManager,
                new SpriteDescription()
                {
                    Textures = new[] { description.TextureReleased, description.TexturePressed },
                    Width = description.Width,
                    Height = description.Height,
                    FitScreen = false,
                });

            if (description.TextDescription != null)
            {
                this.text = new TextDrawer(
                    game,
                    bufferManager,
                    description.TextDescription);
            }

            this.Left = description.Left;
            this.Top = description.Top;
            this.Text = description.Text;
        }
        /// <summary>
        /// Releases used resources
        /// </summary>
        public override void Dispose()
        {
            if (this.button != null)
            {
                this.button.Dispose();
                this.button = null;
            }

            if (this.text != null)
            {
                this.text.Dispose();
                this.text = null;
            }
        }
        /// <summary>
        /// Updates state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.button.Left = this.Left;
            this.button.Top = this.Top;

            if (!string.IsNullOrEmpty(this.Text))
            {
                //Center text
                float leftmove = ((float)this.button.Width * 0.5f) - ((float)this.text.Width * 0.5f);
                float topmove = ((float)this.button.Height * 0.5f) - ((float)this.text.Height * 0.5f);

                this.text.Left = this.Left + (int)leftmove;
                this.text.Top = this.Top + (int)topmove;

                this.text.Update(context);
            }

            this.button.Update(context);
        }
        /// <summary>
        /// Draws button
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            this.button.Draw(context);

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
            if (this.button != null) this.button.Resize();
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
            if (this.Click != null)
            {
                this.Click(this, e);
            }
        }
    }
}
