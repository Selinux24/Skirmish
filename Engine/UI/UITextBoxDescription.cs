
namespace Engine.UI
{
    /// <summary>
    /// UITextBox description
    /// </summary>
    public class UITextBoxDescription : UITextAreaDescription
    {
        /// <summary>
        /// Gets the default text box description
        /// </summary>
        /// <param name="text">Text</param>
        public static new UITextBoxDescription Default(string text = null)
        {
            return new UITextBoxDescription()
            {
                Text = text,
            };
        }
        /// <summary>
        /// Gets the default text box description from a font description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="text">Text</param>
        public static new UITextBoxDescription Default(TextDrawerDescription font, string text = null)
        {
            return new UITextBoxDescription
            {
                Font = font,
                Text = text,
            };
        }
        /// <summary>
        /// Gets the default text box description from a font family name
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="size">Font size</param>
        /// <param name="fontStyle">Font style</param>
        public static new UITextBoxDescription DefaultFromFamily(string fontFamilyName, int size, FontMapStyles fontStyle = FontMapStyles.Regular)
        {
            return new UITextBoxDescription
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
        /// Gets the default text box description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static new UITextBoxDescription DefaultFromFile(string fileName, int size, bool lineAdjust = false, FontMapStyles fontStyle = FontMapStyles.Regular)
        {
            return new UITextBoxDescription()
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
        /// Gets the default text box description from a font map 
        /// </summary>
        /// <param name="fontImageFileName">Font image file name</param>
        /// <param name="fontMapFileName">Font map file name</param>
        public static new UITextBoxDescription DefaultFromMap(string fontImageFileName, string fontMapFileName)
        {
            return new UITextBoxDescription
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
        /// Background
        /// </summary>
        public SpriteDescription Background { get; set; } = SpriteDescription.Default(UIConfiguration.TextBackgroundColor);
        /// <summary>
        /// Cursor character
        /// </summary>
        public char Cursor { get; set; } = '|';
        /// <summary>
        /// Number of spaces to represent the Tab key
        /// </summary>
        public int TabSpaces { get; set; } = 4;
        /// <summary>
        /// Maximum text size
        /// </summary>
        public int Size { get; set; } = 0;
        /// <summary>
        /// Enables multi line text
        /// </summary>
        public bool MultiLine { get; set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public UITextBoxDescription() : base()
        {
            Font = UIConfiguration.MonospacedFont;
        }
    }
}
