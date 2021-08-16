using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Scroll bar description
    /// </summary>
    public class UIScrollBarDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default scroll bar description
        /// </summary>
        /// <param name="scrollMode">Scroll mode</param>
        public static UIScrollBarDescription Default(ScrollModes scrollMode = ScrollModes.Vertical)
        {
            return new UIScrollBarDescription
            {
                ScrollMode = scrollMode,
            };
        }
        /// <summary>
        /// Gets the default scroll bar description
        /// </summary>
        /// <param name="baseColor">Base color</param>
        /// <param name="markerColor">Marker color</param>
        /// <param name="markerSize">Marker size</param>
        /// <param name="scrollMode">Scroll mode</param>
        public static UIScrollBarDescription Default(Color4 baseColor, Color4 markerColor, float markerSize, ScrollModes scrollMode = ScrollModes.Vertical)
        {
            return new UIScrollBarDescription()
            {
                BaseColor = baseColor,
                MarkerColor = markerColor,
                MarkerSize = markerSize,
                ScrollMode = scrollMode,
            };
        }

        /// <summary>
        /// Scroll marker color
        /// </summary>
        public Color4 MarkerColor { get; set; } = UIConfiguration.HighlightColor;
        /// <summary>
        /// Scroll marker size
        /// </summary>
        public float MarkerSize { get; set; } = 15f;
        /// <summary>
        /// Scroll mode
        /// </summary>
        public ScrollModes ScrollMode { get; set; } = ScrollModes.Vertical;

        /// <summary>
        /// Constructor
        /// </summary>
        public UIScrollBarDescription()
            : base()
        {
            EventsEnabled = false;
        }
    }
}
