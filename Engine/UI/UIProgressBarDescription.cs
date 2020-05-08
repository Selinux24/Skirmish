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
        public Color BaseColor { get; set; }
        /// <summary>
        /// Progress color
        /// </summary>
        public Color ProgressColor { get; set; }
        /// <summary>
        /// Button text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Text description
        /// </summary>
        public TextDrawerDescription TextDescription { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UIProgressBarDescription()
            : base()
        {

        }
    }
}
