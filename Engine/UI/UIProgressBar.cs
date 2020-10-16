using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Sprite progress bar
    /// </summary>
    public class UIProgressBar : UIControl
    {
        /// <summary>
        /// Progress sprite
        /// </summary>
        private readonly Sprite spriteProgress = null;
        /// <summary>
        /// Base sprite
        /// </summary>
        private readonly Sprite spriteBase = null;

        /// <summary>
        /// Gets or sets the progress value
        /// </summary>
        public float ProgressValue { get; set; }
        /// <summary>
        /// Gets the caption
        /// </summary>
        public UITextArea Caption { get; } = null;
        /// <inheritdoc/>
        public override float Alpha
        {
            get
            {
                return base.Alpha;
            }
            set
            {
                base.Alpha = value;

                if (Caption != null)
                {
                    Caption.Alpha = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the progress color
        /// </summary>
        public Color4 ProgressColor
        {
            get
            {
                return spriteProgress.BaseColor;
            }
            set
            {
                spriteProgress.BaseColor = value;
            }
        }
        /// <summary>
        /// Gets or sets the back color
        /// </summary>
        public override Color4 BaseColor
        {
            get
            {
                return spriteBase.BaseColor;
            }
            set
            {
                spriteBase.BaseColor = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Button description</param>
        public UIProgressBar(Scene scene, UIProgressBarDescription description)
            : base(scene, description)
        {
            ProgressValue = 0;

            spriteBase = new Sprite(
                scene,
                new SpriteDescription()
                {
                    Name = $"{description.Name}.SpriteBase",
                    BaseColor = description.BaseColor,
                    Width = description.Width,
                    Height = description.Height,
                    EventsEnabled = false,
                });

            AddChild(spriteBase, false);

            spriteProgress = new Sprite(
                scene,
                new SpriteDescription()
                {
                    Name = $"{description.Name}.SpriteProgress",
                    BaseColor = description.ProgressColor,
                    Width = description.Width,
                    Height = description.Height,
                    EventsEnabled = false,
                });

            AddChild(spriteProgress, false);

            if (description.Font != null)
            {
                Caption = new UITextArea(
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

                AddChild(Caption, true);
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            int width = (int)(ProgressValue * Width);

            spriteProgress.Width = width;
            spriteProgress.Height = Height;
            spriteProgress.Left = 0;
            spriteProgress.Top = 0;

            spriteBase.Width = Width - width;
            spriteBase.Height = Height;
            spriteBase.Left = width;
            spriteBase.Top = 0;
        }
    }

    /// <summary>
    /// Progress bar extensions
    /// </summary>
    public static class UIProgressBarExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIProgressBar> AddComponentUIProgressBar(this Scene scene, UIProgressBarDescription description, int order = 0)
        {
            UIProgressBar component = null;

            await Task.Run(() =>
            {
                component = new UIProgressBar(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }
}
