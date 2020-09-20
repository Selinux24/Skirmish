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
        /// Spacing
        /// </summary>
        private Spacing spacing;
        /// <summary>
        /// Padding
        /// </summary>
        private Padding padding;
        /// <summary>
        /// Fit type
        /// </summary>
        private GridFitTypes fitType;
        /// <summary>
        /// Number of rows
        /// </summary>
        private int rows;
        /// <summary>
        /// Number of columns
        /// </summary>
        private int columns;
        /// <summary>
        /// Fixed cell size
        /// </summary>
        private Vector2 cellSize;
        /// <summary>
        /// Fit the x component
        /// </summary>
        private bool fitX;
        /// <summary>
        /// Fit the y component
        /// </summary>
        private bool fitY;

        /// <summary>
        /// Spacing
        /// </summary>
        public Spacing Spacing
        {
            get
            {
                return spacing;
            }
            set
            {
                if (spacing != value)
                {
                    spacing = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Padding
        /// </summary>
        public Padding Padding
        {
            get
            {
                return padding;
            }
            set
            {
                if (padding != value)
                {
                    padding = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Fit type
        /// </summary>
        public GridFitTypes FitType
        {
            get
            {
                return fitType;
            }
            set
            {
                if (fitType != value)
                {
                    fitType = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Number of rows
        /// </summary>
        public int Rows
        {
            get
            {
                return rows;
            }
            set
            {
                if (rows != value)
                {
                    rows = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Number of columns
        /// </summary>
        public int Columns
        {
            get
            {
                return columns;
            }
            set
            {
                if (columns != value)
                {
                    columns = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Fixed cell size
        /// </summary>
        public Vector2 CellSize
        {
            get
            {
                return cellSize;
            }
            set
            {
                if (cellSize != value)
                {
                    cellSize = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Fit the x component
        /// </summary>
        public bool FitX
        {
            get
            {
                return fitX;
            }
            set
            {
                if (fitX != value)
                {
                    fitX = value;

                    UpdateInternals = true;
                }
            }
        }
        /// <summary>
        /// Fit the y component
        /// </summary>
        public bool FitY
        {
            get
            {
                return fitY;
            }
            set
            {
                if (fitY != value)
                {
                    fitY = value;

                    UpdateInternals = true;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public UIPanel(Scene scene, UIPanelDescription description) : base(scene, description)
        {
            Padding = description.Padding;
            Spacing = description.Spacing;

            if (description.Background != null)
            {
                background = new Sprite(scene, description.Background)
                {
                    Name = $"{description.Name}.Background",
                    FitWithParent = true,
                };

                this.AddChild(background);
            }

            SetGridLayout(description.GridLayout);
        }

        /// <inheritdoc/>
        protected override void UpdateInternalState()
        {
            var childs = Children.Where(c => c != background).ToArray();
            if (childs.Any())
            {
                GridLayout.UpdateLayout(this, childs, GetGridLayout());
            }

            base.UpdateInternalState();
        }

        /// <summary>
        /// Sets the grid layout settings
        /// </summary>
        /// <param name="gridLayout">Grid settings</param>
        public void SetGridLayout(GridLayout gridLayout)
        {
            FitType = gridLayout.FitType;
            Rows = gridLayout.Rows;
            Columns = gridLayout.Columns;
            CellSize = gridLayout.CellSize;
            FitX = gridLayout.FitX;
            FitY = gridLayout.FitY;
        }
        /// <summary>
        /// Gets the grid layout settings
        /// </summary>
        public GridLayout GetGridLayout()
        {
            return new GridLayout
            {
                FitType = FitType,
                Rows = Rows,
                Columns = Columns,
                FitX = FitX,
                FitY = FitY,
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
