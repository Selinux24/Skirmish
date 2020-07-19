
namespace Engine.UI
{
    /// <summary>
    /// Panel description
    /// </summary>
    public class UITextAreaDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default text area description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UITextAreaDescription FromFile(string fileName, int size, bool lineAdjust = false)
        {
            return new UITextAreaDescription()
            {
                Font = new TextDrawerDescription
                {
                    FontFileName = fileName,
                    FontSize = size,
                    LineAdjust = false,
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
