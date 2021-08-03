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
        private GridLayout gridLayout;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UIPanel(string id, string name, Scene scene, UIPanelDescription description) :
            base(id, name, scene, description)
        {
            if (description.Background != null)
            {
                background = new Sprite(
                    $"{id}.Background",
                    $"{name}.Background",
                    scene,
                    description.Background);

                AddChild(background);
            }

            SetGridLayout(description.GridLayout);
        }

        /// <inheritdoc/>
        protected override void UpdateInternalState()
        {
            var childs = Children.Where(c => c != background).ToArray();
            if (childs.Any())
            {
                GridLayout.UpdateLayout(childs, gridLayout, AbsoluteRectangle.Size, Padding, Spacing);
            }

            base.UpdateInternalState();
        }

        /// <summary>
        /// Sets the grid layout settings
        /// </summary>
        /// <param name="layout">Grid settings</param>
        public void SetGridLayout(GridLayout layout)
        {
            gridLayout.FitType = layout.FitType;
            gridLayout.Rows = layout.Rows;
            gridLayout.Columns = layout.Columns;
            gridLayout.CellSize = layout.CellSize;
            gridLayout.FitX = layout.FitX;
            gridLayout.FitY = layout.FitY;
        }
        /// <summary>
        /// Gets the grid layout settings
        /// </summary>
        public GridLayout GetGridLayout()
        {
            return new GridLayout
            {
                FitType = gridLayout.FitType,
                Rows = gridLayout.Rows,
                Columns = gridLayout.Columns,
                CellSize = gridLayout.CellSize,
                FitX = gridLayout.FitX,
                FitY = gridLayout.FitY,
            };
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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<UIPanel> AddComponentUIPanel(this Scene scene, string id, string name, UIPanelDescription description, int layer = Scene.LayerUI)
        {
            UIPanel component = null;

            await Task.Run(() =>
            {
                component = new UIPanel(id, name, scene, description);

                scene.AddComponent(component, SceneObjectUsages.UI, layer);
            });

            return component;
        }
    }
}
