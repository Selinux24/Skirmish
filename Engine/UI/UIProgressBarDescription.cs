using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Sprite progress bar description
    /// </summary>
    public class UIProgressBarDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default progress bar description
        /// </summary>
        public static UIProgressBarDescription Default
        {
            get
            {
                return new UIProgressBarDescription();
            }
        }
        /// <summary>
        /// Gets the default progress bar description with text
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="fontLineAdjust">Font line adjust</param>
        public static UIProgressBarDescription WithText(string fontFileName, int fontSize, bool fontLineAdjust = false)
        {
            return new UIProgressBarDescription()
            {
                Font = new TextDrawerDescription
                {
                    FontFileName = fontFileName,
                    FontSize = fontSize,
                    LineAdjust = fontLineAdjust,
                    HorizontalAlign = TextAlign.Center,
                    VerticalAlign = VerticalAlign.Middle,
                },
            };
        }
        /// <summary>
        /// Gets the default progress bar description with text
        /// </summary>
        /// <param name="font">Font description</param>
        public static UIProgressBarDescription WithText(TextDrawerDescription font)
        {
            return new UIProgressBarDescription()
            {
                Font = font,
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
        public TextDrawerDescription Font { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UIProgressBarDescription()
            : base()
        {

        }
    }
}
