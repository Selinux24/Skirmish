using System;
using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sprite button
    /// </summary>
    public class SpriteButton : Drawable, IControl
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
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Button description</param>
        public SpriteButton(Game game, Scene3D scene, SpriteButtonDescription description)
            : base(game, scene)
        {
            this.button = new Sprite(
                game,
                scene,
                new SpriteDescription()
                {
                    Textures = new[] { description.TextureReleased, description.TexturePressed },
                    Width = description.Width,
                    Height = description.Height,
                    FitScreen = false,
                });

            if (!string.IsNullOrEmpty(description.Font))
            {
                this.text = new TextDrawer(game, scene, description.Font, description.FontSize, description.TextColor, description.TextShadowColor);
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
        /// <param name="gameTime">Game time</param>
        public override void Update(GameTime gameTime)
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

                this.text.Update(gameTime);
            }

            this.button.Update(gameTime);
        }
        /// <summary>
        /// Draws button
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Draw(GameTime gameTime)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                this.text.Draw(gameTime);
            }

            this.button.Draw(gameTime);
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

    /// <summary>
    /// Sprite button description
    /// </summary>
    public struct SpriteButtonDescription
    {
        /// <summary>
        /// Texture to show when released state
        /// </summary>
        public string TextureReleased;
        /// <summary>
        /// Texture to show when pressed state
        /// </summary>
        public string TexturePressed;
        /// <summary>
        /// Left position
        /// </summary>
        public int Left;
        /// <summary>
        /// Top position
        /// </summary>
        public int Top;
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Font name
        /// </summary>
        public string Font;
        /// <summary>
        /// Font size
        /// </summary>
        public int FontSize;
        /// <summary>
        /// Text color
        /// </summary>
        public Color TextColor;
        /// <summary>
        /// Text shadow color
        /// </summary>
        /// <remarks>Set to transparent color if no shadow must be drawn</remarks>
        public Color TextShadowColor;
        /// <summary>
        /// Button text
        /// </summary>
        public string Text;
    }
}
