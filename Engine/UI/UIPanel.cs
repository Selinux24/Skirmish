using Engine.Common;
using SharpDX;
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
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UIPanel(Scene scene, UIPanelDescription description) : base(scene, description)
        {
            if (description.Background != null)
            {
                background = new Sprite(scene, description.Background)
                {
                    Name = $"{description.Name}.Background",
                    FitWithParent = true,
                };

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
                GridLayout.UpdateLayout(childs, gridLayout, new RectangleF(0, 0, AbsoluteWidth, AbsoluteHeight), Padding, Spacing);
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
