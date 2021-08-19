using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Scroll layout helper
    /// </summary>
    public static class ScrollLayout
    {
        /// <summary>
        /// Gets the maximum number of vertical pixels
        /// </summary>
        /// <param name="container">Container control</param>
        /// <returns>Returns the maximum number of vertical pixels</returns>
        public static float GetMaximumVerticalOffset(this IUIControl container)
        {
            return container.GetControlArea().Height - container.GetRenderArea(true).Height;
        }
        /// <summary>
        /// Gets the maximum number of horizontal pixels
        /// </summary>
        /// <param name="container">Container control</param>
        /// <returns>Returns the maximum number of horizontal pixels</returns>
        public static float GetMaximumHorizontalOffset(this IUIControl container)
        {
            return container.GetControlArea().Width - container.GetRenderArea(true).Width;
        }

        /// <summary>
        /// Converts from vertical pixels to a 0 to 1 value
        /// </summary>
        /// <param name="container">Container control</param>
        /// <param name="offset">Vertical pixels</param>
        /// <returns>Returns a value from 0 to 1</returns>
        public static float ConvertVerticalOffsetToPosition(this IUIControl container, float offset)
        {
            float maxOffset = container.GetMaximumVerticalOffset();

            return offset / maxOffset;
        }
        /// <summary>
        /// Converts from vertical position to pixels
        /// </summary>
        /// <param name="container">Container control</param>
        /// <param name="position">0 to 1 position value</param>
        /// <returns>Returns the pixel offset</returns>
        public static float ConvertVerticalPositionToOffset(this IUIControl container, float position)
        {
            float maxOffset = container.GetMaximumVerticalOffset();

            return position * maxOffset;
        }

        /// <summary>
        /// Converts from horizontal pixels to a 0 to 1 value
        /// </summary>
        /// <param name="container">Container control</param>
        /// <param name="offset">Horizontal pixels</param>
        /// <returns>Returns a value from 0 to 1</returns>
        public static float ConvertHorizontalOffsetToPosition(this IUIControl container, float offset)
        {
            float maxOffset = container.GetMaximumHorizontalOffset();

            return offset / maxOffset;
        }
        /// <summary>
        /// Converts from horizontal position to pixels
        /// </summary>
        /// <param name="container">Container control</param>
        /// <param name="position">0 to 1 position value</param>
        /// <returns>Returns the pixel offset</returns>
        public static float ConvertHorizontalPositionToOffset(this IUIControl container, float position)
        {
            float maxOffset = container.GetMaximumHorizontalOffset();

            return position * maxOffset;
        }

        /// <summary>
        /// Gets the layout for a vertical scroll bar control
        /// </summary>
        /// <param name="container">Container control</param>
        /// <param name="size">Scroll bar size</param>
        /// <param name="verticalAlign">Vertical alignment of this scroll bar</param>
        /// <param name="horizontalAlign">Horizontal aligment of the other container scroll bar, if any.</param>
        /// <returns>Returns the layout rectangle for the scroll bar</returns>
        public static RectangleF GetVerticalLayout(this IUIControl container, float size, ScrollVerticalAlign verticalAlign, ScrollHorizontalAlign horizontalAlign)
        {
            var renderArea = container.GetRenderArea(false);

            float heightAdjust = horizontalAlign != ScrollHorizontalAlign.None ? size : 0;

            float left = 0;
            if (verticalAlign == ScrollVerticalAlign.Right)
            {
                left = renderArea.Width - size;
            }

            float top = 0;
            if (horizontalAlign == ScrollHorizontalAlign.Top)
            {
                top = size;
            }

            return new RectangleF
            {
                Left = left,
                Top = top,
                Width = size,
                Height = renderArea.Height - heightAdjust,
            };
        }
        /// <summary>
        /// Gets the layout for a horizontal scroll bar control
        /// </summary>
        /// <param name="container">Container control</param>
        /// <param name="size">Scroll bar size</param>
        /// <param name="verticalAlign">Vertical alignment of the other scroll bar, if any</param>
        /// <param name="horizontalAlign">Horizontal aligment of this scroll bar</param>
        /// <returns>Returns the layout rectangle for the scroll bar</returns>
        public static RectangleF GetHorizontalLayout(this IUIControl container, float size, ScrollVerticalAlign verticalAlign, ScrollHorizontalAlign horizontalAlign)
        {
            var renderArea = container.GetRenderArea(false);

            float widthAdjust = verticalAlign != ScrollVerticalAlign.None ? size : 0;

            float top = 0;
            if (horizontalAlign == ScrollHorizontalAlign.Bottom)
            {
                top = renderArea.Height - size;
            }

            float left = 0;
            if (verticalAlign == ScrollVerticalAlign.Left)
            {
                left = size;
            }

            return new RectangleF
            {
                Left = left,
                Top = top,
                Width = renderArea.Width - widthAdjust,
                Height = size,
            };
        }
    }
}
