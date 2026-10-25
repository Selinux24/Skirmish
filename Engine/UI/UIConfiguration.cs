using System.Numerics;

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
        public static Vector4 BaseColor { get; set; } = System.Drawing.Color.CornflowerBlue.RGBA();
        /// <summary>
        /// Control fore color
        /// </summary>
        public static Vector4 ForeColor { get; set; } = System.Drawing.Color.DeepSkyBlue.RGBA();
        /// <summary>
        /// Control highlight color
        /// </summary>
        public static Vector4 HighlightColor { get; set; } = System.Drawing.Color.LightBlue.RGBA();
        /// <summary>
        /// Control shadow color
        /// </summary>
        public static Vector4 ShadowColor { get; set; } = System.Drawing.Color.DarkBlue.RGBA();
        /// <summary>
        /// Text color
        /// </summary>
        public static Vector4 TextColor { get; set; } = System.Drawing.Color.LightGray.RGBA();
        /// <summary>
        /// Highlight text color
        /// </summary>
        public static Vector4 HighlightTextColor { get; set; } = System.Drawing.Color.Yellow.RGBA();
        /// <summary>
        /// Text background color
        /// </summary>
        public static Vector4 TextBackgroundColor { get; set; } = new Vector4(0, 0, 0, 0.75f);
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
