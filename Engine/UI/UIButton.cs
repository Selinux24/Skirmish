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
        /// Gets the caption
        /// </summary>
        public UITextArea Caption { get; } = null;

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
                EventsEnabled = false,
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
                    EventsEnabled = false,
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
                this.Caption = new UITextArea(
                    scene,
                    new UITextAreaDescription
                    {
                        Font = description.Font,
                        EventsEnabled = false,
                    });

                this.AddChild(this.Caption, true);

                this.Caption.Text = description.Text;
            }
        }

        /// <inheritdoc/>
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
