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
        /// Button state
        /// </summary>
        private UIButtonState state = UIButtonState.Released;

        /// <summary>
        /// Gets the caption
        /// </summary>
        public UITextArea Caption { get; } = null;
        /// <summary>
        /// Gets whether this buttons is a two-state button or not
        /// </summary>
        public bool TwoStateButton { get; }
        /// <summary>
        /// Gets or sets the button state
        /// </summary>
        public UIButtonState State
        {
            get
            {
                return state;
            }
            set
            {
                if (state != value)
                {
                    state = value;

                    UpdateInternals = true;
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
            TwoStateButton = description.TwoStateButton;

            var spriteDesc = new SpriteDescription()
            {
                Name = $"{description.Name}.ReleasedButton",
                BaseColor = description.ColorReleased,
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
                    BaseColor = description.ColorPressed,
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

            if (description.Caption != null)
            {
                description.Caption.EventsEnabled = false;

                this.Caption = new UITextArea(scene, description.Caption);

                this.AddChild(this.Caption, true);
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

            bool pressed = IsPressed || (State == UIButtonState.Pressed);

            if (this.buttonPressed != null)
            {
                this.buttonPressed.Visible = pressed;
                this.buttonReleased.Visible = !pressed;
            }
            else
            {
                this.buttonReleased.Visible = true;
            }
        }
    }

    /// <summary>
    /// UI button state
    /// </summary>
    public enum UIButtonState
    {
        /// <summary>
        /// Pressed
        /// </summary>
        Pressed,
        /// <summary>
        /// Released
        /// </summary>
        Released,
    }

    /// <summary>
    /// UIButton extensions
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
