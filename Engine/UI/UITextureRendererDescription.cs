
namespace Engine.UI
{
    /// <summary>
    /// Minimap description
    /// </summary>
    public class UITextureRendererDescription : UIControlDescription
    {

        public static UITextureRendererDescription Default(float width, float height)
        {
            return new UITextureRendererDescription
            {
                Width = width,
                Height = height,
                Channel = UITextureRendererChannels.NoAlpha,
            };
        }

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
