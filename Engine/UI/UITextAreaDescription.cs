
namespace Engine.UI
{
    /// <summary>
    /// Panel description
    /// </summary>
    public class UITextAreaDescription : UIControlDescription
    {
        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }
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
        /// Text description
        /// </summary>
        public TextDrawerDescription TextDescription { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UITextAreaDescription()
            : base()
        {

        }
    }
}
