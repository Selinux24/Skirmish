using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Sprite progress bar description
    /// </summary>
    public class UIProgressBarDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the screen centered progress bar description
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="fontLineAdjust">Font line adjust</param>
        public static UIProgressBarDescription ScreenCentered(string fontFileName, int fontSize, bool fontLineAdjust = false)
        {
            return new UIProgressBarDescription()
            {
                CenterHorizontally = CenterTargets.Screen,
                CenterVertically = CenterTargets.Screen,
                Font = new TextDrawerDescription
                {
                    FontFileName = fontFileName,
                    FontSize = fontSize,
                    LineAdjust = fontLineAdjust,
                },
            };
        }

        /// <summary>
        /// Base color
        /// </summary>
        public Color BaseColor { get; set; } = new Color(0f, 0f, 0f, 0.5f);
        /// <summary>
        /// Progress color
        /// </summary>
        public Color ProgressColor { get; set; } = new Color(0f, 1f, 0f, 1f);

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Font description
        /// </summary>
        public TextDrawerDescription Font { get; set; } = new TextDrawerDescription();

        /// <summary>
        /// Constructor
        /// </summary>
        public UIProgressBarDescription()
            : base()
        {

        }
    }
}
