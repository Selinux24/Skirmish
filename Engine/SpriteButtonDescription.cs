using SharpDX;

namespace Engine
{
    /// <summary>
    /// Sprite button description
    /// </summary>
    public class SpriteButtonDescription : DrawableDescription
    {
        /// <summary>
        /// Texture to show when released state
        /// </summary>
        public string TextureReleased;
        /// <summary>
        /// Texture to show when pressed state
        /// </summary>
        public string TexturePressed;
        /// <summary>
        /// Left position
        /// </summary>
        public int Left;
        /// <summary>
        /// Top position
        /// </summary>
        public int Top;
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Button text
        /// </summary>
        public string Text;
        /// <summary>
        /// Text description
        /// </summary>
        public TextDrawerDescription TextDescription = new TextDrawerDescription();

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteButtonDescription()
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
