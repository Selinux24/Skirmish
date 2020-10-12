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
                    HorizontalAlign = HorizontalTextAlign.Center,
                    VerticalAlign = VerticalTextAlign.Middle,
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
                    HorizontalAlign = HorizontalTextAlign.Center,
                    VerticalAlign = VerticalTextAlign.Middle,
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
                    HorizontalAlign = HorizontalTextAlign.Center,
                    VerticalAlign = VerticalTextAlign.Middle,
                    UseTextureColor = true,
                },
            };
        }

        /// <summary>
        /// Progress color
        /// </summary>
        public Color4 ProgressColor { get; set; } = new Color4(0f, 1f, 0f, 1f);

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
            var font = TextDrawerDescription.Default();
            font.HorizontalAlign = HorizontalTextAlign.Center;
            font.VerticalAlign = VerticalTextAlign.Middle;

            Font = font;
        }
    }
}
