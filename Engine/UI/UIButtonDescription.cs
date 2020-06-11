using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Sprite button description
    /// </summary>
    public class UIButtonDescription : UIControlDescription
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
        /// Button text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Text description
        /// </summary>
        public UITextAreaDescription TextDescription { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UIButtonDescription()
            : base()
        {
            this.TwoStateButton = false;
            this.ColorReleased = new Color4(1f, 1f, 1f, 1f);
            this.ColorPressed = new Color4(1f, 1f, 1f, 1f);
            this.TextureReleasedUVMap = new Vector4(0, 0, 1, 1);
            this.TexturePressedUVMap = new Vector4(0, 0, 1, 1);
        }
    }
}
