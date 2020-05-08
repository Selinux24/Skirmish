
namespace Engine.UI
{
    /// <summary>
    /// Minimap description
    /// </summary>
    public class UITextureRendererDescription : UIControlDescription
    {
        /// <summary>
        /// Channel color
        /// </summary>
        public UITextureRendererChannels Channel { get; set; } = UITextureRendererChannels.All;

        /// <summary>
        /// Constructor
        /// </summary>
        public UITextureRendererDescription()
            : base()
        {

        }
    }
}
