
namespace Engine.UI
{
    /// <summary>
    /// Minimap description
    /// </summary>
    public class UITextureRendererDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default texture renderer description
        /// </summary>
        public static UITextureRendererDescription Default()
        {
            return new UITextureRendererDescription
            {
                Channel = UITextureRendererChannels.NoAlpha,
            };
        }
        /// <summary>
        /// Gets the default texture renderer description
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
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
