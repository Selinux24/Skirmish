using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    /// <summary>
    /// User interface panel
    /// </summary>
    public class UIPanel : UIControl
    {
        /// <summary>
        /// Background
        /// </summary>
        private readonly Sprite background;
        /// <summary>
        /// Grid layout
        /// </summary>
        private GridLayout gridLayout = GridLayout.Default;

        /// <summary>
        /// Gets or sets the grid layout
        /// </summary>
        public GridLayout GridLayout
        {
            get
            {
                return gridLayout;
            }
            set
            {
                gridLayout = value;

                UpdateInternals = true;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UIPanel(Scene scene, UIPanelDescription description) : base(scene, description)
        {
            if (description.Background != null)
            {
                background = new Sprite(scene, description.Background)
                {
                    Name = $"{description.Name}.Background",
                    FitParent = true,
                };

                this.AddChild(background);
            }

            gridLayout = description.GridLayout;
        }

        /// <inheritdoc/>
        protected override void UpdateInternalState()
        {
            var childs = Children.ToArray().Where(c => c != background);

            GridLayout.UpdateLayout(this, childs, GridLayout);

            base.UpdateInternalState();
        }
    }

    /// <summary>
    /// UI Panel extensions
    /// </summary>
    public static class UIPanelExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIPanel> AddComponentUIPanel(this Scene scene, UIPanelDescription description, int order = 0)
        {
            UIPanel component = null;

            await Task.Run(() =>
            {
                component = new UIPanel(scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, order);
            });

            return component;
        }
    }
}
