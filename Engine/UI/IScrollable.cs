﻿
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