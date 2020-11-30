using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Progress bar description
    /// </summary>
    public class UIProgressBarDescription : UIControlDescription, IWithTextDescription
    {
        /// <summary>
        /// Gets the default progress bar description
        /// </summary>
        public static UIProgressBarDescription Default()
        {
            return new UIProgressBarDescription();
        }
        /// <summary>
        /// Gets the default progress bar description
        /// </summary>
        /// <param name="baseColor">Base color</param>
        /// <param name="progressColor">Progress color</param>
        public static UIProgressBarDescription Default(Color4 baseColor, Color4 progressColor)
        {
            return new UIProgressBarDescription()
            {
                BaseColor = baseColor,
                ProgressColor = progressColor,
            };
        }
        /// <summary>
        /// Gets the default progress bar description from a font family name
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="size">Font size</param>
        /// <param name="fontStyle">Font style</param>
        public static UIProgressBarDescription DefaultFromFamily(string fontFamilyName, int size, FontMapStyles fontStyle = FontMapStyles.Regular)
        {
            return new UIProgressBarDescription()
            {
                Font = new TextDrawerDescription()
                {
                    FontFamily = fontFamilyName,
                    FontSize = size,
                    Style = fontStyle,
                },
            };
        }
        /// <summary>
        /// Gets the default progress bar description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UIProgressBarDescription DefaultFromFile(string fileName, int size, bool lineAdjust = false, FontMapStyles fontStyle = FontMapStyles.Regular)
        {
            return new UIProgressBarDescription()
            {
                Font = new TextDrawerDescription
                {
                    FontFileName = fileName,
                    FontSize = size,
                    LineAdjust = lineAdjust,
                    Style = fontStyle,
                },
            };
        }
        /// <summary>
        /// Gets the default progress bar description from a font map 
        /// </summary>
        /// <param name="fontImageFileName">Font image file name</param>
        /// <param name="fontMapFileName">Font map file name</param>
        public static UIProgressBarDescription DefaultFromMap(string fontImageFileName, string fontMapFileName)
        {
            return new UIProgressBarDescription
            {
                Font = new TextDrawerDescription()
                {
                    FontMapping = new FontMapping
                    {
                        ImageFile = fontImageFileName,
                        MapFile = fontMapFileName,
                    },
                    UseTextureColor = true,
                },
            };
        }

        /// <summary>
        /// Progress color
        /// </summary>
        public Color4 ProgressColor { get; set; } = UIConfiguration.HighlightColor;

        /// <summary>
        /// Font description
        /// </summary>
        public TextDrawerDescription Font { get; set; } = UIConfiguration.Font;

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Text fore color
        /// </summary>
        public Color4 TextForeColor { get; set; } = UIConfiguration.TextColor;
        /// <summary>
        /// Text shadow color
        /// </summary>
        public Color4 TextShadowColor { get; set; } = Color.Transparent;
        /// <summary>
        /// Shadow position delta
        /// </summary>
        public Vector2 TextShadowDelta { get; set; } = new Vector2(1, 1);
        /// <summary>
        /// Text horizontal alignement
        /// </summary>
        public HorizontalTextAlign TextHorizontalAlign { get; set; } = HorizontalTextAlign.Center;
        /// <summary>
        /// Text vertical alignement
        /// </summary>
        public VerticalTextAlign TextVerticalAlign { get; set; } = VerticalTextAlign.Middle;

        /// <summary>
        /// Constructor
        /// </summary>
        public UIProgressBarDescription()
            : base()
        {
            EventsEnabled = false;
        }
    }
}
