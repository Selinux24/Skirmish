using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Sprite progress bar
    /// </summary>
    public sealed class UIProgressBar : UIControl<UIProgressBarDescription>
    {
        /// <summary>
        /// Progress sprite
        /// </summary>
        private Sprite spriteProgress = null;
        /// <summary>
        /// Base color
        /// </summary>
        private Color4 baseColor;
        /// <summary>
        /// Alpha component
        /// </summary>
        private float alpha;

        /// <summary>
        /// Gets the caption
        /// </summary>
        public UITextArea Caption { get; private set; } = null;
        /// <summary>
        /// Gets or sets the progress value
        /// </summary>
        public float ProgressValue { get; set; }
        /// <summary>
        /// Gets or sets the progress color
        /// </summary>
        public Color4 ProgressColor
        {
            get
            {
                return spriteProgress.Color2;
            }
            set
            {
                spriteProgress.Color2 = value;
            }
        }
        /// <inheritdoc/>
        public override Color4 BaseColor
        {
            get
            {
                return baseColor;
            }
            set
            {
                baseColor = value;

                spriteProgress.Color1 = baseColor;
            }
        }
        /// <inheritdoc/>
        public override float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;

                base.Alpha = alpha;

                if (Caption != null)
                {
                    Caption.Alpha = alpha;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public UIProgressBar(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(UIProgressBarDescription description)
        {
            await base.InitializeAssets(description);

            ProgressValue = 0;

            spriteProgress = await CreateSpriteProgress();
            AddChild(spriteProgress, true);

            if (Description.Font != null)
            {
                Caption = await CreateCaption();
                AddChild(Caption, true);
            }
        }
        private async Task<Sprite> CreateSpriteProgress()
        {
            var desc = new SpriteDescription()
            {
                Color1 = Description.ProgressColor,
                Color2 = Description.BaseColor,
                DrawDirection = SpriteDrawDirections.Horizontal,
                EventsEnabled = false,
            };

            return await Scene.CreateComponent<Sprite, SpriteDescription>(
                $"{Id}.Progress",
                $"{Name}.Progress",
                desc);
        }
        private async Task<UITextArea> CreateCaption()
        {
            var desc = new UITextAreaDescription
            {
                Font = Description.Font,
                Text = Description.Text,
                TextForeColor = Description.TextForeColor,
                TextShadowColor = Description.TextShadowColor,
                TextShadowDelta = Description.TextShadowDelta,
                TextHorizontalAlign = Description.TextHorizontalAlign,
                TextVerticalAlign = Description.TextVerticalAlign,
                EventsEnabled = false,
            };

            return await Scene.CreateComponent<UITextArea, UITextAreaDescription>(
                $"{Id}.Caption",
                $"{Name}.Caption",
                desc);
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            spriteProgress.SetPercentage(ProgressValue);
        }
    }
}
