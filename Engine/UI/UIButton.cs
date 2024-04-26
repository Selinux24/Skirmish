using Engine.Common;
using System.Threading.Tasks;

namespace Engine.UI
{
    /// <summary>
    /// Sprite button
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class UIButton(Scene scene, string id, string name) : UIControl<UIButtonDescription>(scene, id, name)
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
        /// Button state
        /// </summary>
        private UIButtonState state = UIButtonState.Released;

        /// <summary>
        /// Gets the caption
        /// </summary>
        public UITextArea Caption { get; private set; } = null;
        /// <summary>
        /// Gets whether this buttons is a two-state button or not
        /// </summary>
        public bool TwoStateButton { get; private set; }
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

        /// <inheritdoc/>
        public override async Task ReadAssets(UIButtonDescription description)
        {
            await base.ReadAssets(description);

            TwoStateButton = Description.TwoStateButton;

            buttonReleased = await CreateButtonReleased();
            AddChild(buttonReleased);

            if (Description.TwoStateButton)
            {
                buttonPressed = await CreateButtonPressed();
                AddChild(buttonPressed);
            }

            Caption = await CreateCaption();
            AddChild(Caption);
        }
        private async Task<Sprite> CreateButtonReleased()
        {
            var spriteDesc = new SpriteDescription()
            {
                ContentPath = Description.ContentPath,
                BaseColor = Description.ColorReleased,
                EventsEnabled = false,
            };

            if (!string.IsNullOrEmpty(Description.TextureReleased))
            {
                spriteDesc.Textures = [Description.TextureReleased];
                spriteDesc.UVMap = Description.TextureReleasedUVMap;
            }

            return await Scene.CreateComponent<Sprite, SpriteDescription>(
                $"{Id}.ReleasedButton",
                $"{Name}.ReleasedButton",
                spriteDesc);
        }
        private async Task<Sprite> CreateButtonPressed()
        {
            var spriteDesc = new SpriteDescription()
            {
                ContentPath = Description.ContentPath,
                BaseColor = Description.ColorPressed,
                EventsEnabled = false,
            };

            if (!string.IsNullOrEmpty(Description.TexturePressed))
            {
                spriteDesc.Textures = [Description.TexturePressed];
                spriteDesc.UVMap = Description.TexturePressedUVMap;
            }

            return await Scene.CreateComponent<Sprite, SpriteDescription>(
                $"{Id}.PressedButton",
                $"{Name}.PressedButton",
                spriteDesc);
        }
        private async Task<UITextArea> CreateCaption()
        {
            return await Scene.CreateComponent<UITextArea, UITextAreaDescription>(
                $"{Id}.Caption",
                $"{Name}.Caption",
                new UITextAreaDescription
                {
                    ContentPath = Description.ContentPath,
                    Font = Description.Font,
                    Text = Description.Text,
                    TextForeColor = Description.TextForeColor,
                    TextShadowColor = Description.TextShadowColor,
                    TextShadowDelta = Description.TextShadowDelta,
                    TextHorizontalAlign = Description.TextHorizontalAlign,
                    TextVerticalAlign = Description.TextVerticalAlign,

                    EventsEnabled = false,
                });
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            bool pressed = PressedState.HasFlag(MouseButtons.Left) || (State == UIButtonState.Pressed);

            if (buttonPressed != null)
            {
                buttonPressed.Visible = pressed;
                buttonReleased.Visible = !pressed;
            }
            else
            {
                buttonReleased.Visible = true;
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
}
