
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
        /// <param name="text">Text</param>
        public static UITextAreaDescription Default(string text = null)
        {
            return new UITextAreaDescription()
            {
                Font = TextDrawerDescription.Default(),
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
        public static UITextAreaDescription FromFamily(string fontFamilyName, int size, FontMapStyles fontStyle = FontMapStyles.Regular)
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
        public static UITextAreaDescription FromFile(string fileName, int size, bool lineAdjust = false, FontMapStyles fontStyle = FontMapStyles.Regular)
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
        /// Gets or sets whether the area must grow or shrinks with the text value
        /// </summary>
        public bool AdjustAreaWithText { get; set; } = true;

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
        public UITextAreaDescription()
            : base()
        {

        }
    }
}
