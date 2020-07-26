
namespace Engine.UI
{
    /// <summary>
    /// Panel description
    /// </summary>
    public class UITextAreaDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default text area description
        /// </summary>
        public static UITextAreaDescription Default()
        {
            return new UITextAreaDescription();
        }
        /// <summary>
        /// Gets the default text area description
        /// </summary>
        public static UITextAreaDescription DefaultTextCentered()
        {
            return new UITextAreaDescription
            {
                Font = new TextDrawerDescription
                {
                    HorizontalAlign = TextAlign.Center,
                    VerticalAlign = VerticalAlign.Middle,
                },
            };
        }
        /// <summary>
        /// Gets the default text area description from a font description
        /// </summary>
        /// <param name="font">Font description</param>
        public static UITextAreaDescription Default(TextDrawerDescription font)
        {
            return new UITextAreaDescription
            {
                Font = font,
            };
        }

        /// <summary>
        /// Gets the default text area description from a font family name
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="size">Font size</param>
        /// <param name="fontStyle">Font style</param>
        public static UITextAreaDescription FromFamily(string fontFamilyName, int size, FontMapStyles fontStyle = FontMapStyles.Regular)
        {
            return new UITextAreaDescription
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
        /// Gets the default text area description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UITextAreaDescription FromFile(string fileName, int size, bool lineAdjust = false, FontMapStyles fontStyle = FontMapStyles.Regular)
        {
            return new UITextAreaDescription()
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
        /// Gets the default text area description from a font map 
        /// </summary>
        /// <param name="fontImageFileName">Font image file name</param>
        /// <param name="fontMapFileName">Font map file name</param>
        public static UITextAreaDescription FromMap(string fontImageFileName, string fontMapFileName)
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
                    HorizontalAlign = TextAlign.Center,
                    VerticalAlign = VerticalAlign.Middle,
                    UseTextureColor = true,
                },
            };
        }

        /// <summary>
        /// Left margin
        /// </summary>
        public float MarginLeft { get; set; }
        /// <summary>
        /// Top margin
        /// </summary>
        public float MarginTop { get; set; }
        /// <summary>
        /// Right margin
        /// </summary>
        public float MarginRight { get; set; }
        /// <summary>
        /// Bottom margin
        /// </summary>
        public float MarginBottom { get; set; }

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
        public UITextAreaDescription()
            : base()
        {

        }
    }
}
