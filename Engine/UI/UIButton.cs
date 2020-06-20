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
        private readonly Sprite buttonPressed = null;
        /// <summary>
        /// Release sprite button
        /// </summary>
        private readonly Sprite buttonReleased = null;
        /// <summary>
        /// Button text drawer
        /// </summary>
        private readonly UITextArea textArea = null;

        /// <summary>
        /// Gets or sets the button text
        /// </summary>
        public string Text
        {
            get
            {
                return this.textArea?.Text;
            }
            set
            {
                if (this.textArea != null)
                {
                    this.textArea.Text = value;
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
            };

            if (!string.IsNullOrEmpty(description.TextureReleased))
            {
                spriteDesc.Textures = new[] { description.TextureReleased };
                spriteDesc.UVMap = description.TextureReleasedUVMap;
            }

            this.buttonReleased = new Sprite(scene, spriteDesc);
            this.AddChild(this.buttonReleased, true);

            if (description.TwoStateButton)
            {
                var spriteDesc2 = new SpriteDescription()
                {
                    Name = $"{description.Name}.PressedButton",
                    Color = description.ColorPressed,
                };

                if (!string.IsNullOrEmpty(description.TexturePressed))
                {
                    spriteDesc2.Textures = new[] { description.TexturePressed };
                    spriteDesc2.UVMap = description.TexturePressedUVMap;
                }

                this.buttonPressed = new Sprite(scene, spriteDesc2);

                this.AddChild(this.buttonPressed, true);
            }

            if (description.Font != null)
            {
                description.Font.Name = description.Font.Name ?? $"{description.Name}.TextButton";

                var textAreaDesc = new UITextAreaDescription
                {
                    Font = description.Font,
                };

                this.textArea = new UITextArea(scene, textAreaDesc)
                {
                    CenterHorizontally = CenterTargets.Parent,
                    CenterVertically = CenterTargets.Parent
                };

                this.AddChild(this.textArea, true);

                this.Text = description.Text;
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
                this.buttonPressed.Visible = this.IsPressed;
                this.buttonReleased.Visible = !this.IsPressed;
            }
            else
            {
                this.buttonReleased.Visible = true;
            }
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
