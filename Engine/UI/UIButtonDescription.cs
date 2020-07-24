using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Sprite button description
    /// </summary>
    public class UIButtonDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="color">Button color</param>
        public static UIButtonDescription Default(Color4 color)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                ColorReleased = color,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="color">Button color</param>
        /// <param name="font">Font description</param>
        public static UIButtonDescription Default(Color4 color, TextDrawerDescription font)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                ColorReleased = color,

                Font = font,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="textureRect">Texture rectangle</param>
        public static UIButtonDescription Default(string textureFileName, Vector4 textureRect)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TextureReleased = textureFileName,
                TextureReleasedUVMap = textureRect,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="textureRect">Texture rectangle</param>
        /// <param name="font">Font description</param>
        public static UIButtonDescription Default(string textureFileName, Vector4 textureRect, TextDrawerDescription font)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TextureReleased = textureFileName,
                TextureReleasedUVMap = textureRect,

                Font = font,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="releasedColor">Released button color</param>
        /// <param name="pressedColor">Pressed button color</param>
        public static UIButtonDescription DefaultTwoStateButton(Color4 releasedColor, Color4 pressedColor)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,

                ColorReleased = releasedColor,

                ColorPressed = pressedColor,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="releasedColor">Released button color</param>
        /// <param name="pressedColor">Pressed button color</param>
        /// <param name="font">Font description</param>
        public static UIButtonDescription DefaultTwoStateButton(Color4 releasedColor, Color4 pressedColor, TextDrawerDescription font)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,

                ColorReleased = releasedColor,

                ColorPressed = pressedColor,

                Font = font,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="releasedTextureRect">Released texture rectangle</param>
        /// <param name="pressedTextureRect">Pressed texture rectangle</param>
        public static UIButtonDescription DefaultTwoStateButton(string textureFileName, Vector4 releasedTextureRect, Vector4 pressedTextureRect)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,

                TextureReleased = textureFileName,
                TextureReleasedUVMap = releasedTextureRect,

                TexturePressed = textureFileName,
                TexturePressedUVMap = pressedTextureRect,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="releasedTextureRect">Released texture rectangle</param>
        /// <param name="pressedTextureRect">Pressed texture rectangle</param>
        /// <param name="font">Font description</param>
        public static UIButtonDescription DefaultTwoStateButton(string textureFileName, Vector4 releasedTextureRect, Vector4 pressedTextureRect, TextDrawerDescription font)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,

                TextureReleased = textureFileName,
                TextureReleasedUVMap = releasedTextureRect,

                TexturePressed = textureFileName,
                TexturePressedUVMap = pressedTextureRect,

                Font = font,
            };
        }

        /// <summary>
        /// Two state button
        /// </summary>
        public bool TwoStateButton { get; set; } = false;

        /// <summary>
        /// Texture to show when released state
        /// </summary>
        public string TextureReleased { get; set; }
        /// <summary>
        /// Released button color
        /// </summary>
        public Color4 ColorReleased { get; set; } = new Color4(1f, 1f, 1f, 1f);
        /// <summary>
        /// Texture released UV map
        /// </summary>
        public Vector4 TextureReleasedUVMap { get; set; } = new Vector4(0, 0, 1, 1);

        /// <summary>
        /// Texture to show when pressed state
        /// </summary>
        public string TexturePressed { get; set; }
        /// <summary>
        /// Pressed button color
        /// </summary>
        public Color4 ColorPressed { get; set; } = new Color4(1f, 1f, 1f, 1f);
        /// <summary>
        /// Texture pressed UV map
        /// </summary>
        public Vector4 TexturePressedUVMap { get; set; } = new Vector4(0, 0, 1, 1);

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Font description
        /// </summary>
        public TextDrawerDescription Font { get; set; } = new TextDrawerDescription();

        /// <summary>
        /// Constructor
        /// </summary>
        public UIButtonDescription()
            : base()
        {

        }
    }
}
