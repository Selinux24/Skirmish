
namespace Engine.UI
{
    /// <summary>
    /// Scrollable
    /// </summary>
    public interface IScrollable
    {
        /// <summary>
        /// Scroll
        /// </summary>
        ScrollModes Scroll { get; set; }
        /// <summary>
        /// Scroll bar size
        /// </summary>
        float ScrollbarSize { get; set; }
        /// <summary>
        /// Gets or sets the vertical scroll offset
        /// </summary>
        float VerticalScrollOffset { get; set; }
        /// <summary>
        /// Gets or sets the horizontal scroll offset
        /// </summary>
        float HorizontalScrollOffset { get; set; }
        /// <summary>
        /// Vertical Scroll position (0 to 1)
        /// </summary>
        float VerticalScrollPosition { get; set; }
        /// <summary>
        /// Horizontal Scroll position (0 to 1)
        /// </summary>
        float HorizontalScrollPosition { get; set; }

        /// <summary>
        /// Moves scroll up
        /// </summary>
        /// <param name="amount">Amount</param>
        void ScrollUp(float amount);
        /// <summary>
        /// Moves scroll down
        /// </summary>
        /// <param name="amount">Amount</param>
        void ScrollDown(float amount);
        /// <summary>
        /// Moves scroll left
        /// </summary>
        /// <param name="amount">Amount</param>
        void ScrollLeft(float amount);
        /// <summary>
        /// Moves scroll right
        /// </summary>
        /// <param name="amount">Amount</param>
        void ScrollRight(float amount);
    }
}