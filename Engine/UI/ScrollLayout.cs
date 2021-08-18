
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
    }
}
