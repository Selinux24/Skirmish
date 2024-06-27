using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Sprite scroll bar
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class UIScrollBar(Scene scene, string id, string name) : UIControl<UIScrollBarDescription>(scene, id, name)
    {
        /// <summary>
        /// Bar sprite
        /// </summary>
        private Sprite spriteBar = null;
        /// <summary>
        /// Base color
        /// </summary>
        private Color4 baseColor;

        /// <summary>
        /// Scroll mode
        /// </summary>
        public ScrollModes ScrollMode { get; set; }
        /// <summary>
        /// Gets or sets the marker size
        /// </summary>
        public float MarkerSize { get; set; }
        /// <summary>
        /// Gets or sets the marker position
        /// </summary>
        public float MarkerPosition { get; set; }
        /// <summary>
        /// Gets or sets the progress color
        /// </summary>
        public Color4 MarkerColor
        {
            get
            {
                return spriteBar.Color2;
            }
            set
            {
                spriteBar.Color2 = value;
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

                spriteBar.Color1 = baseColor;
                spriteBar.Color3 = baseColor;
            }
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(UIScrollBarDescription description)
        {
            await base.ReadAssets(description);

            ScrollMode = Description.ScrollMode;
            MarkerSize = Description.MarkerSize;
            MarkerPosition = 0;

            spriteBar = await CreateScrollSprite();
            AddChild(spriteBar, true);
        }
        private async Task<Sprite> CreateScrollSprite()
        {
            var desc = new SpriteDescription()
            {
                Color1 = Description.BaseColor,
                Color2 = Description.MarkerColor,
                Color3 = Description.BaseColor,
                DrawDirection = Description.ScrollMode == ScrollModes.Vertical ? SpriteDrawDirections.Vertical : SpriteDrawDirections.Horizontal,
                EventsEnabled = false,
            };

            return await Scene.CreateComponent<Sprite, SpriteDescription>(
                $"{Id}.Scroll",
                $"{Name}.Scroll",
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

            float ctrlSize = ScrollMode == ScrollModes.Vertical ? Height : Width;
            float t = ctrlSize - MarkerSize;
            float p = MarkerPosition * t / ctrlSize;
            float size = MarkerSize / ctrlSize;

            spriteBar.SetPercentage(p, p + size);
        }
    }
}
