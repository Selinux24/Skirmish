using SharpDX;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.UI
{
    /// <summary>
    /// User interface panel
    /// </summary>
    public sealed class UIPanel : UIControl<UIPanelDescription>, IScrollable
    {
        /// <summary>
        /// Background
        /// </summary>
        private Sprite background;
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
        public ScrollVerticalAlign ScrollVerticalAlign { get; set; }
        /// <inheritdoc/>
        public float ScrollVerticalOffset
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
        public float ScrollVerticalPosition
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
        public ScrollHorizontalAlign ScrollHorizontalAlign { get; set; }
        /// <inheritdoc/>
        public float ScrollHorizontalOffset
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
        public float ScrollHorizontalPosition
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
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public UIPanel(Scene scene, string id, string name) :
            base(scene, id, name)
        {

        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(UIPanelDescription description)
        {
            await base.InitializeAssets(description);

            if (Description.Background != null)
            {
                background = await CreateBackground();
                AddChild(background);
            }

            SetGridLayout(Description.GridLayout);
        }
        private async Task<Sprite> CreateBackground()
        {
            return await Scene.CreateComponent<Sprite, SpriteDescription>(
                $"{Id}.Background",
                $"{Name}.Background",
                Description.Background);
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
            ScrollVerticalOffset -= amount * Game.GameTime.ElapsedSeconds;
            ScrollVerticalOffset = Math.Max(0, ScrollVerticalOffset);
        }
        /// <inheritdoc/>
        public void ScrollDown(float amount)
        {
            float maxOffset = this.GetMaximumVerticalOffset();

            ScrollVerticalOffset += amount * Game.GameTime.ElapsedSeconds;
            ScrollVerticalOffset = Math.Min(maxOffset, ScrollVerticalOffset);
        }
        /// <inheritdoc/>
        public void ScrollLeft(float amount)
        {
            float maxOffset = this.GetMaximumHorizontalOffset();

            ScrollHorizontalOffset += amount * Game.GameTime.ElapsedSeconds;
            ScrollHorizontalOffset = Math.Min(maxOffset, ScrollHorizontalOffset);
        }
        /// <inheritdoc/>
        public void ScrollRight(float amount)
        {
            ScrollHorizontalOffset -= amount * Game.GameTime.ElapsedSeconds;
            ScrollHorizontalOffset = Math.Max(0, ScrollHorizontalOffset);
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
}
