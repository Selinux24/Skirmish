using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sprite description
    /// </summary>
    public class SpriteDescription : DrawableDescription
    {
        /// <summary>
        /// Sprite color
        /// </summary>
        public Color4 Color = Color4.White;
        /// <summary>
        /// Sprite textures
        /// </summary>
        public string[] Textures;
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Fit screen
        /// </summary>
        public bool FitScreen;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteDescription()
            : base()
        {
            this.Static = true;
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
