using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Text area description
    /// </summary>
    public class UITextAreaDescription : UIControlDescription, IWithTextDescription
    {
        /// <summary>
        /// Gets the default text area description
        /// </summary>
        /// <param name="text">Text</param>
        public static UITextAreaDescription Default(string text = null)
        {
            return new UITextAreaDescription()
            {
                Text = text,
            };
        }
        /// <summary>
        /// Gets the default text area description from a font description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="text">Text</param>
        public static UITextAreaDescription Default(TextDrawerDescription font, string text = null)
        {
            return new UITextAreaDescription
            {
                Font = font,
                Text = text,
            };
        }
        /// <summary>
        /// Gets the default text area description from a font family name
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="size">Font size</param>
        /// <param name="fontStyle">Font style</param>
        public static UITextAreaDescription DefaultFromFamily(string fontFamilyName, int size, FontMapStyles fontStyle = FontMapStyles.Regular)
        {
            return new UITextAreaDescription
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
        /// Gets the default text area description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UITextAreaDescription DefaultFromFile(string fileName, int size, bool lineAdjust = false, FontMapStyles fontStyle = FontMapStyles.Regular)
        {
            return new UITextAreaDescription()
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
        /// Gets the default text area description from a font map 
        /// </summary>
        /// <param name="fontImageFileName">Font image file name</param>
        /// <param name="fontMapFileName">Font map file name</param>
        public static UITextAreaDescription DefaultFromMap(string fontImageFileName, string fontMapFileName)
        {
            return new UITextAreaDescription
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
        /// Gets or sets whether the control must grow or shrinks with the text value
        /// </summary>
        public bool GrowControlWithText { get; set; } = true;

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
        public HorizontalTextAlign TextHorizontalAlign { get; set; } = HorizontalTextAlign.Left;
        /// <summary>
        /// Text vertical alignement
        /// </summary>
        public VerticalTextAlign TextVerticalAlign { get; set; } = VerticalTextAlign.Top;
        /// <summary>
        /// Scroll
        /// </summary>
        public ScrollModes Scroll { get; set; } = ScrollModes.None;

        /// <summary>
        /// Constructor
        /// </summary>
        public UITextAreaDescription()
            : base()
        {
            EventsEnabled = false;
        }
    }
}
