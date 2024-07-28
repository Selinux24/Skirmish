using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// User interface configuration
    /// </summary>
    public static class UIConfiguration
    {
        /// <summary>
        /// Base color
        /// </summary>
        public static Color4 BaseColor { get; set; } = Color.CornflowerBlue;
        /// <summary>
        /// Control fore color
        /// </summary>
        public static Color4 ForeColor { get; set; } = Color.DeepSkyBlue;
        /// <summary>
        /// Control highlight color
        /// </summary>
        public static Color4 HighlightColor { get; set; } = Color.LightBlue;
        /// <summary>
        /// Control shadow color
        /// </summary>
        public static Color4 ShadowColor { get; set; } = Color.DarkBlue;
        /// <summary>
        /// Text color
        /// </summary>
        public static Color4 TextColor { get; set; } = Color.LightGray;
        /// <summary>
        /// Highlight text color
        /// </summary>
        public static Color4 HighlightTextColor { get; set; } = Color.Yellow;
        /// <summary>
        /// Text background color
        /// </summary>
        public static Color4 TextBackgroundColor { get; set; } = new Color4(0, 0, 0, 0.75f);
        /// <summary>
        /// Default Font
        /// </summary>
        public static FontDescription Font
        {
            get
            {
                return FontDescription.Default();
            }
        }
        /// <summary>
        /// Monospaced font
        /// </summary>
        public static FontDescription MonospacedFont
        {
            get
            {
                return FontDescription.FromFamily("Lucida Console");
            }
        }

        /// <summary>
        /// Spacing
        /// </summary>
        public static Spacing Spacing { get; set; } = 0;
        /// <summary>
        /// Padding
        /// </summary>
        public static Padding Padding { get; set; } = 0;
    }
}
