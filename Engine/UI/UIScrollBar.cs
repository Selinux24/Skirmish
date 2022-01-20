using SharpDX;
using System.Threading.Tasks;

namespace Engine.UI
{
    using Engine.Common;

    /// <summary>
    /// Sprite scroll bar
    /// </summary>
    public class UIScrollBar : UIControl
    {
        /// <summary>
        /// Bar sprite
        /// </summary>
        private readonly Sprite spriteBar = null;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Scroll bar description</param>
        public UIScrollBar(string id, string name, Scene scene, UIScrollBarDescription description)
            : base(id, name, scene, description)
        {
            ScrollMode = description.ScrollMode;
            MarkerSize = description.MarkerSize;
            MarkerPosition = 0;

            spriteBar = new Sprite(
                $"{id}.Scroll",
                $"{name}.Scroll",
                scene,
                new SpriteDescription()
                {
                    Color1 = description.BaseColor,
                    Color2 = description.MarkerColor,
                    Color3 = description.BaseColor,
                    DrawDirection = description.ScrollMode == ScrollModes.Vertical ? SpriteDrawDirections.Vertical : SpriteDrawDirections.Horizontal,
                    EventsEnabled = false,
                });

            AddChild(spriteBar, true);
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

    /// <summary>
    /// Scroll bar extensions
    /// </summary>
    public static class UIScrollBarExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIScrollBar> AddComponentUIScrollBar(this Scene scene, string id, string name, UIScrollBarDescription description, int layer = Scene.LayerUI)
        {
            UIScrollBar component = null;

            await Task.Run(() =>
            {
                component = new UIScrollBar(id, name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
