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
        /// <param name="fineSampling">Fine sampling</param>
        public static UITextAreaDescription DefaultFromFamily(string fontFamilyName, int size, bool fineSampling = false)
        {
            return new UITextAreaDescription
            {
                Font = TextDrawerDescription.FromFamily(fontFamilyName, size, fineSampling),
            };
        }
        /// <summary>
        /// Gets the default text area description from a font family name
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="size">Font size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="fineSampling">Fine sampling</param>
        public static UITextAreaDescription DefaultFromFamily(string fontFamilyName, int size, FontMapStyles fontStyle, bool fineSampling = false)
        {
            return new UITextAreaDescription
            {
                Font = TextDrawerDescription.FromFamily(fontFamilyName, size, fontStyle, fineSampling),
            };
        }

        /// <summary>
        /// Gets the default text area description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UITextAreaDescription DefaultFromFile(string fileName, int size, bool lineAdjust = false)
        {
            return new UITextAreaDescription()
            {
                Font = TextDrawerDescription.FromFile(fileName, size, lineAdjust),
            };
        }
        /// <summary>
        /// Gets the default text area description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UITextAreaDescription DefaultFromFile(string fileName, int size, FontMapStyles fontStyle, bool lineAdjust = false)
        {
            return new UITextAreaDescription()
            {
                Font = TextDrawerDescription.FromFile(fileName, size, fontStyle, lineAdjust),
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
                Font = TextDrawerDescription.FromMap(fontImageFileName, fontMapFileName),
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
        public HorizontalAlign TextHorizontalAlign { get; set; } = HorizontalAlign.Left;
        /// <summary>
        /// Text vertical alignement
        /// </summary>
        public VerticalAlign TextVerticalAlign { get; set; } = VerticalAlign.Top;
        /// <summary>
        /// Scroll
        /// </summary>
        public ScrollModes Scroll { get; set; } = ScrollModes.None;
        /// <summary>
        /// Scrollbar size
        /// </summary>
        public float ScrollbarSize { get; set; } = 15;
        /// <summary>
        /// Vertical scroll bar position
        /// </summary>
        public HorizontalAlign ScrollVerticalAlign { get; set; } = HorizontalAlign.Right;
        /// <summary>
        /// Horizontal scroll bar position
        /// </summary>
        public VerticalAlign ScrollHorizontalAlign { get; set; } = VerticalAlign.Bottom;

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
