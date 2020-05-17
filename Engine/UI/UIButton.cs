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
        public readonly Sprite buttonPressed = null;
        /// <summary>
        /// Release sprite button
        /// </summary>
        public readonly Sprite buttonReleased = null;
        /// <summary>
        /// Button text drawer
        /// </summary>
        public TextDrawer textDrawer = null;

        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public override int Top
        {
            get { return base.Top; }
            set
            {
                base.Top = value;

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
                Name = $"{description.Name}.ReleasedButton",
                Color = description.ColorReleased,
                FitParent = true,
            };

            if (!string.IsNullOrEmpty(description.TextureReleased))
            {
                spriteDesc.Textures = new[] { description.TextureReleased };
                spriteDesc.UVMap = description.TextureReleasedUVMap;
            }

            this.buttonReleased = new Sprite(scene, spriteDesc);
            this.AddChild(this.buttonReleased);

            if (description.TwoStateButton)
            {
                var spriteDesc2 = new SpriteDescription()
                {
                    Name = $"{description.Name}.PressedButton",
                    Color = description.ColorPressed,
                    FitParent = true,
                };

                if (!string.IsNullOrEmpty(description.TexturePressed))
                {
                    spriteDesc2.Textures = new[] { description.TexturePressed };
                    spriteDesc2.UVMap = description.TexturePressedUVMap;
                }

                this.buttonPressed = new Sprite(scene, spriteDesc2);
                this.AddChild(this.buttonPressed);
            }

            if (description.TextDescription != null)
            {
                description.TextDescription.Name = description.TextDescription.Name ?? $"{description.Name}.TextButton";

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
                this.textDrawer?.Dispose();
                this.textDrawer = null;
            }
        }

        /// <summary>
        /// Updates state
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!this.Active)
            {
                return;
            }

            if (this.buttonPressed != null)
            {
                this.buttonPressed.Visible = this.Pressed;
                this.buttonReleased.Visible = !this.Pressed;
            }
            else
            {
                this.buttonReleased.Visible = true;
            }

            this.textDrawer?.Update(context);
        }

        /// <summary>
        /// Draws button
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            base.Draw(context);

            if (!this.Visible)
            {
                return;
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

            this.textDrawer?.Resize();
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
