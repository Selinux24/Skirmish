
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
        public SpriteTextureChannels Channel { get; set; } = SpriteTextureChannels.All;

        /// <summary>
        /// Constructor
        /// </summary>
        public UITextureRendererDescription()
            : base()
        {

        }
    }
}
