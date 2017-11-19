using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sprite button description
    /// </summary>
    public class SpriteButtonDescription : SceneObjectDescription
    {
        /// <summary>
        /// Two state button
        /// </summary>
        public bool TwoStateButton;

        /// <summary>
        /// Texture to show when released state
        /// </summary>
        public string TextureReleased;
        /// <summary>
        /// Released button color
        /// </summary>
        public Color4 ColorReleased;
        /// <summary>
        /// Texture released UV map
        /// </summary>
        public Vector4 TextureReleasedUVMap;

        /// <summary>
        /// Texture to show when pressed state
        /// </summary>
        public string TexturePressed;
        /// <summary>
        /// Pressed button color
        /// </summary>
        public Color4 ColorPressed;
        /// <summary>
        /// Texture pressed UV map
        /// </summary>
        public Vector4 TexturePressedUVMap;

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
