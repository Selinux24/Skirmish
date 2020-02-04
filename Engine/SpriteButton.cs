using SharpDX;
using System;
using System.Threading.Tasks;

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
        private TextDrawer textDrawer = null;
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
        public int Width { get { return this.buttonReleased?.Width ?? 0; } }
        /// <summary>
        /// Button height
        /// </summary>
        public int Height { get { return this.buttonReleased?.Height ?? 0; } }
        /// <summary>
        /// Gets or sets the button text
        /// </summary>
        public string Text
        {
            get
            {
                return this.textDrawer?.Text;
            }
            set
            {
                if (this.textDrawer != null)
                {
                    this.textDrawer.Text = value;
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
                Name = $"{description.Name}.Button",
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
                    Name = $"{description.Name}.Button2",
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
                description.TextDescription.Name = description.TextDescription.Name ?? $"{description.Name}.TextDrawer";

                this.textDrawer = new TextDrawer(
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

                if (this.textDrawer != null)
                {
                    this.textDrawer.Dispose();
                    this.textDrawer = null;
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
                float leftmove = ((float)this.Width * 0.5f) - ((float)this.textDrawer.Width * 0.5f);
                float topmove = ((float)this.Height * 0.5f) - ((float)this.textDrawer.Height * 0.5f);

                this.textDrawer.Left = this.Left + (int)leftmove;
                this.textDrawer.Top = this.Top + (int)topmove;

                this.textDrawer.Update(context);
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
                this.textDrawer.Draw(context);
            }
        }
        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            this.buttonReleased.Resize();
            if (this.buttonPressed != null) this.buttonPressed.Resize();
            if (this.textDrawer != null) this.textDrawer.Resize();
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

    /// <summary>
    /// Sprite button extensions
    /// </summary>
    public static class SpriteButtonExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<SpriteButton> AddComponentSpriteButton(this Scene scene, SpriteButtonDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            SpriteButton component = null;

            await Task.Run(() =>
            {
                component = new SpriteButton(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
