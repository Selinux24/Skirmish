using SharpDX;

namespace Engine
{
    /// <summary>
    /// Sprite description
    /// </summary>
    public class SpriteDescription : SceneObjectDescription
    {
        /// <summary>
        /// Sprite color
        /// </summary>
        public Color4 Color { get; set; } = Color4.White;
        /// <summary>
        /// Sprite textures
        /// </summary>
        public string[] Textures { get; set; }
        /// <summary>
        /// UV map
        /// </summary>
        public Vector4? UVMap { get; set; } = null;
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Fit screen
        /// </summary>
        public bool FitScreen { get; set; }
        /// <summary>
        /// Centered
        /// </summary>
        public bool Centered { get; set; } = true;

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
