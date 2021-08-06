
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
        /// Gets or sets the vertical scroll offset
        /// </summary>
        float VerticalScrollOffset { get; set; }
        /// <summary>
        /// Gets or sets the horizontal scroll offset
        /// </summary>
        float HorizontalScrollOffset { get; set; }
    }
}