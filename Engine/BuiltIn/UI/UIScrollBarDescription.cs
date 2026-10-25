using Engine.UI;
using System.Numerics;

namespace Engine.BuiltIn.UI
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
            return new()
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
        public static UIScrollBarDescription Default(Vector4 baseColor, Vector4 markerColor, float markerSize, ScrollModes scrollMode = ScrollModes.Vertical)
        {
            return new()
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
        public Vector4 MarkerColor { get; set; } = UIConfiguration.HighlightColor;
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
