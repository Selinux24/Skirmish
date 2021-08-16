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
        /// Gets the caption
        /// </summary>
        public UITextArea Caption { get; } = null;
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
                return spriteProgress.Color1;
            }
            set
            {
                spriteProgress.Color1 = value;
            }
        }
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
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Progress bar description</param>
        public UIProgressBar(string id, string name, Scene scene, UIProgressBarDescription description)
            : base(id, name, scene, description)
        {
            ProgressValue = 0;

            spriteProgress = new Sprite(
                $"{id}.Progress",
                $"{name}.Progress",
                scene,
                new SpriteDescription()
                {
                    Color1 = description.ProgressColor,
                    Color2 = description.BaseColor,
                    DrawDirection = SpriteDrawDirections.Horizontal,
                    EventsEnabled = false,
                });

            AddChild(spriteProgress, true);

            if (description.Font != null)
            {
                Caption = new UITextArea(
                    $"{id}.Caption",
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

            spriteProgress.SetPercentage(ProgressValue);
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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIProgressBar> AddComponentUIProgressBar(this Scene scene, string id, string name, UIProgressBarDescription description, int layer = Scene.LayerUI)
        {
            UIProgressBar component = null;

            await Task.Run(() =>
            {
                component = new UIProgressBar(id, name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
