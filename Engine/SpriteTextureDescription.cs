
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Minimap description
    /// </summary>
    public class SpriteTextureDescription : DrawableDescription
    {
        /// <summary>
        /// Top position
        /// </summary>
        public int Top;
        /// <summary>
        /// Left position
        /// </summary>
        public int Left;
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Channel color
        /// </summary>
        public SpriteTextureChannelsEnum Channel = SpriteTextureChannelsEnum.All;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteTextureDescription()
            : base()
        {
            this.Static = true;
            this.AlwaysVisible = true;
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.EnableDepthStencil = false;
            this.EnableAlphaBlending = true;
        }
    }
}
