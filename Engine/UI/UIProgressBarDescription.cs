using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Sprite progress bar description
    /// </summary>
    public class UIProgressBarDescription : UIControlDescription
    {
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
        public TextDrawerDescription Font { get; set; } = new TextDrawerDescription();

        /// <summary>
        /// Constructor
        /// </summary>
        public UIProgressBarDescription()
            : base()
        {

        }
    }
}
