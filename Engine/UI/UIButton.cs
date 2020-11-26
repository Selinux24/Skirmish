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
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Button description</param>
        public UIButton(string name, Scene scene, UIButtonDescription description)
            : base(name, scene, description)
        {
            TwoStateButton = description.TwoStateButton;

            var spriteDesc = new SpriteDescription()
            {
                BaseColor = description.ColorReleased,
                EventsEnabled = false,
            };

            if (!string.IsNullOrEmpty(description.TextureReleased))
            {
                spriteDesc.Textures = new[] { description.TextureReleased };
                spriteDesc.UVMap = description.TextureReleasedUVMap;
            }

            buttonReleased = new Sprite($"{name}.ReleasedButton", scene, spriteDesc);

            AddChild(buttonReleased);

            if (description.TwoStateButton)
            {
                var spriteDesc2 = new SpriteDescription()
                {
                    BaseColor = description.ColorPressed,
                    EventsEnabled = false,
                };

                if (!string.IsNullOrEmpty(description.TexturePressed))
                {
                    spriteDesc2.Textures = new[] { description.TexturePressed };
                    spriteDesc2.UVMap = description.TexturePressedUVMap;
                }

                buttonPressed = new Sprite($"{name}.PressedButton", scene, spriteDesc2);

                AddChild(buttonPressed);
            }

            Caption = new UITextArea(
                $"{name}.Caption",
                scene,
                new UITextAreaDescription
                {
                    Font = description.Font,
                    Text = description.Text,
                    TextForeColor = description.TextForeColor,
                    TextShadowColor = description.TextShadowColor,
                    TextShadowDelta = description.TextShadowDelta,
                    TextHorizontalAlign = description.TextHorizontalAlign,
                    TextVerticalAlign = description.TextVerticalAlign,

                    EventsEnabled = false,
                });

            AddChild(Caption);
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            bool pressed = IsPressed || (State == UIButtonState.Pressed);

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

    /// <summary>
    /// UIButton extensions
    /// </summary>
    public static class SpriteButtonExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIButton> AddComponentUIButton(this Scene scene, string name, UIButtonDescription description, int layer = Scene.LayerUI)
        {
            UIButton component = null;

            await Task.Run(() =>
            {
                component = new UIButton(name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
