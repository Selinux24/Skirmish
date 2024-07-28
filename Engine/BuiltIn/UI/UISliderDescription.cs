using Engine.UI;
using SharpDX;

namespace Engine.BuiltIn.UI
{
    /// <summary>
    /// Slider description
    /// </summary>
    public class UISliderDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default slider description
        /// </summary>
        public static UISliderDescription Default()
        {
            return Default(1);
        }
        /// <summary>
        /// Gets the default slider description
        /// </summary>
        /// <param name="rangeCount">Number of ranges</param>
        public static UISliderDescription Default(int rangeCount)
        {
            float step = 1f / rangeCount;

            return new()
            {
                RangeCount = rangeCount,

                Height = 20,

                SelectorHeight = 25,
                SelectorWidth = 10,

                BarColors = Helper.CreateArray(rangeCount + 1, UIConfiguration.BaseColor),
                BarRanges = Helper.CreateArray(rangeCount, (i) => (i + 1) * step),

                SelectorColors = Helper.CreateArray(rangeCount, UIConfiguration.HighlightColor),
                SelectorInitialValues = Helper.CreateArray(rangeCount, (i) => i * step)
            };
        }

        /// <summary>
        /// Number of ranges in the slider
        /// </summary>
        public int RangeCount { get; set; } = 1;

        /// <summary>
        /// Bar colors
        /// </summary>
        public Color4[] BarColors { get; set; } = [UIConfiguration.BaseColor, UIConfiguration.BaseColor];
        /// <summary>
        /// Range values array
        /// </summary>
        public float[] BarRanges { get; set; } = [1f];

        /// <summary>
        /// Selector colors
        /// </summary>
        public Color4[] SelectorColors { get; set; } = [UIConfiguration.HighlightColor];
        /// <summary>
        /// Selector height
        /// </summary>
        public float SelectorHeight { get; set; } = 25;
        /// <summary>
        /// Selector width
        /// </summary>
        public float SelectorWidth { get; set; } = 10;
        /// <summary>
        /// Initial selector values
        /// </summary>
        public float[] SelectorInitialValues { get; set; } = [0.5f];

        /// <summary>
        /// Minimum value
        /// </summary>
        public float Minimum { get; set; } = 0f;
        /// <summary>
        /// Maximum value
        /// </summary>
        public float Maximum { get; set; } = 1f;
        /// <summary>
        /// Value step
        /// </summary>
        public float Step { get; set; } = 0.1f;

        /// <summary>
        /// Constructor
        /// </summary>
        public UISliderDescription()
            : base()
        {
            EventsEnabled = false;
        }
    }
}
