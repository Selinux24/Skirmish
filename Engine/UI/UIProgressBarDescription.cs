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
                    Font = fontFamilyName,
                    FontSize = size,
                    HorizontalAlign = TextAlign.Center,
                    VerticalAlign = VerticalAlign.Middle,
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
                    HorizontalAlign = TextAlign.Center,
                    VerticalAlign = VerticalAlign.Middle,
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
                    HorizontalAlign = TextAlign.Center,
                    VerticalAlign = VerticalAlign.Middle,
                    UseTextureColor = true,
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
