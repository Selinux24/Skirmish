using SharpDX;

namespace Engine
{
    /// <summary>
    /// Sprite button description
    /// </summary>
    public class SpriteButtonDescription : SceneObjectDescription
    {
        /// <summary>
        /// Two state button
        /// </summary>
        public bool TwoStateButton { get; set; }

        /// <summary>
        /// Texture to show when released state
        /// </summary>
        public string TextureReleased { get; set; }
        /// <summary>
        /// Released button color
        /// </summary>
        public Color4 ColorReleased { get; set; }
        /// <summary>
        /// Texture released UV map
        /// </summary>
        public Vector4 TextureReleasedUVMap { get; set; }

        /// <summary>
        /// Texture to show when pressed state
        /// </summary>
        public string TexturePressed { get; set; }
        /// <summary>
        /// Pressed button color
        /// </summary>
        public Color4 ColorPressed { get; set; }
        /// <summary>
        /// Texture pressed UV map
        /// </summary>
        public Vector4 TexturePressedUVMap { get; set; }

        /// <summary>
        /// Left position
        /// </summary>
        public int Left { get; set; }
        /// <summary>
        /// Top position
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Button text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Text description
        /// </summary>
        public TextDrawerDescription TextDescription { get; set; } = new TextDrawerDescription();

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteButtonDescription()
            : base()
        {
            this.Static = true;
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;

            this.TwoStateButton = false;
            this.ColorReleased = new Color4(1f, 1f, 1f, 1f);
            this.ColorPressed = new Color4(1f, 1f, 1f, 1f);
        }
    }
}
