using SharpDX;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    /// <summary>
    /// User interface panel
    /// </summary>
    public class UIPanel : UIControl, IScrollable
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
        /// Vertical scroll offset (in pixels)
        /// </summary>
        private float verticalScrollOffset = 0;
        /// <summary>
        /// Vertical scroll position (from 0 to 1)
        /// </summary>
        private float verticalScrollPosition = 0;
        /// <summary>
        /// Horizontal scroll offset (in pixels)
        /// </summary>
        private float horizontalScrollOffset = 0;
        /// <summary>
        /// Horizontal scroll position (from 0 to 1)
        /// </summary>
        private float horizontalScrollPosition = 0;

        /// <inheritdoc/>
        public ScrollModes Scroll { get; set; }
        /// <inheritdoc/>
        public float ScrollbarSize { get; set; }
        /// <inheritdoc/>
        public float VerticalScrollOffset
        {
            get
            {
                return verticalScrollOffset;
            }
            set
            {
                verticalScrollOffset = MathUtil.Clamp(value, 0f, this.GetMaximumVerticalOffset());
                verticalScrollPosition = this.ConvertVerticalOffsetToPosition(verticalScrollOffset);
            }
        }
        /// <inheritdoc/>
        public float HorizontalScrollOffset
        {
            get
            {
                return horizontalScrollOffset;
            }
            set
            {
                horizontalScrollOffset = MathUtil.Clamp(value, 0f, this.GetMaximumHorizontalOffset());
                horizontalScrollPosition = this.ConvertHorizontalOffsetToPosition(horizontalScrollOffset);
            }
        }
        /// <inheritdoc/>
        public float VerticalScrollPosition
        {
            get
            {
                return verticalScrollPosition;
            }
            set
            {
                verticalScrollPosition = MathUtil.Clamp(value, 0f, 1f);
                verticalScrollOffset = this.ConvertVerticalPositionToOffset(verticalScrollPosition);
            }
        }
        /// <inheritdoc/>
        public float HorizontalScrollPosition
        {
            get
            {
                return horizontalScrollPosition;
            }
            set
            {
                horizontalScrollPosition = MathUtil.Clamp(value, 0f, 1f);
                horizontalScrollOffset = this.ConvertHorizontalPositionToOffset(horizontalScrollPosition);
            }
        }

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

        /// <inheritdoc/>
        public void ScrollUp(float amount)
        {
            VerticalScrollOffset -= amount * Game.GameTime.ElapsedSeconds;
            VerticalScrollOffset = Math.Max(0, VerticalScrollOffset);
        }
        /// <inheritdoc/>
        public void ScrollDown(float amount)
        {
            float maxOffset = this.GetMaximumVerticalOffset();

            VerticalScrollOffset += amount * Game.GameTime.ElapsedSeconds;
            VerticalScrollOffset = Math.Min(maxOffset, VerticalScrollOffset);
        }
        /// <inheritdoc/>
        public void ScrollLeft(float amount)
        {
            float maxOffset = this.GetMaximumHorizontalOffset();

            HorizontalScrollOffset += amount * Game.GameTime.ElapsedSeconds;
            HorizontalScrollOffset = Math.Min(maxOffset, HorizontalScrollOffset);
        }
        /// <inheritdoc/>
        public void ScrollRight(float amount)
        {
            HorizontalScrollOffset -= amount * Game.GameTime.ElapsedSeconds;
            HorizontalScrollOffset = Math.Max(0, HorizontalScrollOffset);
        }

        /// <inheritdoc/>
        public override RectangleF GetControlArea()
        {
            RectangleF rect = RectangleF.Empty;

            foreach (var item in Children)
            {
                if (rect == RectangleF.Empty)
                {
                    rect = item.GetControlArea();

                    continue;
                }

                rect = RectangleF.Union(rect, item.GetControlArea());
            }

            return rect;
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
