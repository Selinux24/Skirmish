
namespace Engine
{
    /// <summary>
    /// Minimap description
    /// </summary>
    public class SpriteTextureDescription : SceneObjectDescription
    {
        /// <summary>
        /// Top position
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// Left position
        /// </summary>
        public int Left { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Channel color
        /// </summary>
        public SpriteTextureChannels Channel { get; set; } = SpriteTextureChannels.All;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteTextureDescription()
            : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
