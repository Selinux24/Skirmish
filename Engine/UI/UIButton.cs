using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Sprite button
    /// </summary>
    public class UIButton : UIControl
    {
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
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public override int Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;

                if (buttonPressed != null) buttonPressed.Top = value;
                if (buttonReleased != null) buttonReleased.Top = value;
                if (textDrawer != null) textDrawer.CenterRectangle(this.Rectangle);
            }
        }
        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        public override int Left
        {
            get { return base.Left; }
            set
            {
                base.Left = value;

                if (buttonPressed != null) buttonPressed.Left = value;
                if (buttonReleased != null) buttonReleased.Left = value;
                if (textDrawer != null) textDrawer.CenterRectangle(this.Rectangle);
            }
        }
        /// <summary>
        /// Width
        /// </summary>
        public override int Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;

                if (buttonPressed != null) buttonPressed.Width = value;
                if (buttonReleased != null) buttonReleased.Width = value;
                if (textDrawer != null) textDrawer.CenterRectangle(this.Rectangle);
            }
        }
        /// <summary>
        /// Height
        /// </summary>
        public override int Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;

                if (buttonPressed != null) buttonPressed.Height = value;
                if (buttonReleased != null) buttonReleased.Height = value;
                if (textDrawer != null) textDrawer.CenterRectangle(this.Rectangle);
            }
        }
        /// <summary>
        /// Scale
        /// </summary>
        public override float Scale
        {
            get { return base.Scale; }
            set
            {
                base.Scale = value;

                if (buttonPressed != null) buttonPressed.Scale = value;
                if (buttonReleased != null) buttonReleased.Scale = value;
                if (textDrawer != null) textDrawer.CenterRectangle(this.Rectangle);
            }
        }

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
                    this.textDrawer.CenterRectangle(this.Rectangle);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Button description</param>
        public UIButton(Scene scene, UIButtonDescription description)
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

                this.textDrawer = new TextDrawer(scene, description.TextDescription)
                {
                    Text = description.Text
                };

                this.textDrawer.CenterRectangle(this.Rectangle);
            }
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
            if (!this.Active)
            {
                return;
            }

            this.buttonReleased?.Update(context);
            this.buttonPressed?.Update(context);
            this.textDrawer?.Update(context);
        }

        /// <summary>
        /// Draws button
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (!this.Visible)
            {
                return;
            }

            if (this.Pressed && this.buttonPressed != null)
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
        public override void Resize()
        {
            base.Resize();

            this.buttonReleased?.Resize();
            this.buttonPressed?.Resize();
            this.textDrawer?.Resize();
        }
        /// <summary>
        /// Centers horinzontally the text
        /// </summary>
        public override void CenterHorizontally()
        {
            base.CenterHorizontally();

            buttonPressed?.CenterHorizontally();
            buttonReleased?.CenterHorizontally();
        }
        /// <summary>
        /// Centers vertically the text
        /// </summary>
        public override void CenterVertically()
        {
            base.CenterVertically();

            buttonPressed?.CenterVertically();
            buttonReleased?.CenterVertically();
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
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIButton> AddComponentUIButton(this Scene scene, UIButtonDescription description, int order = 0)
        {
            UIButton component = null;

            await Task.Run(() =>
            {
                component = new UIButton(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }
}
