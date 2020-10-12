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
            var blendMode = color.Alpha >= 1f ? BlendModes.Default : BlendModes.DefaultTransparent;

            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                ColorReleased = color,
                BlendMode = blendMode,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="color">Button color</param>
        /// <param name="caption">Caption description</param>
        public static UIButtonDescription Default(Color4 color, UITextAreaDescription caption)
        {
            var blendMode = color.Alpha >= 1f ? BlendModes.Default : BlendModes.DefaultTransparent;

            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                ColorReleased = color,
                BlendMode = blendMode,

                Caption = caption,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="textureFileName">Texture file name</param>
        public static UIButtonDescription Default(string textureFileName)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TextureReleased = textureFileName,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="caption">Caption description</param>
        public static UIButtonDescription Default(string textureFileName, UITextAreaDescription caption)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TextureReleased = textureFileName,
           
                Caption = caption,
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
        /// <param name="caption">Caption description</param>
        public static UIButtonDescription Default(string textureFileName, Vector4 textureRect, UITextAreaDescription caption)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TextureReleased = textureFileName,
                TextureReleasedUVMap = textureRect,

                Caption = caption,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="releasedColor">Released button color</param>
        /// <param name="pressedColor">Pressed button color</param>
        public static UIButtonDescription DefaultTwoStateButton(Color4 releasedColor, Color4 pressedColor)
        {
            var blendMode = releasedColor.Alpha >= 1f && pressedColor.Alpha > 1f ? BlendModes.Default : BlendModes.DefaultTransparent;

            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,

                ColorReleased = releasedColor,

                ColorPressed = pressedColor,

                BlendMode = blendMode,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="releasedColor">Released button color</param>
        /// <param name="pressedColor">Pressed button color</param>
        /// <param name="caption">Caption description</param>
        public static UIButtonDescription DefaultTwoStateButton(Color4 releasedColor, Color4 pressedColor, UITextAreaDescription caption)
        {
            var blendMode = releasedColor.Alpha >= 1f && pressedColor.Alpha > 1f ? BlendModes.Default : BlendModes.DefaultTransparent;

            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,

                ColorReleased = releasedColor,

                ColorPressed = pressedColor,

                BlendMode = blendMode,

                Caption = caption,
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
        /// <param name="caption">Caption description</param>
        public static UIButtonDescription DefaultTwoStateButton(string textureFileName, Vector4 releasedTextureRect, Vector4 pressedTextureRect, UITextAreaDescription caption)
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

                Caption = caption,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="releasedTextureFileName">Released texture file name</param>
        /// <param name="pressedTextureFileName">Pressed texture file name</param>
        /// <param name="caption">Caption description</param>
        public static UIButtonDescription DefaultTwoStateButton(string releasedTextureFileName, string pressedTextureFileName, UITextAreaDescription caption)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,

                TextureReleased = releasedTextureFileName,

                TexturePressed = pressedTextureFileName,

                Caption = caption,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="releasedTextureFileName">Released texture file name</param>
        /// <param name="releasedTextureRect">Released texture rectangle</param>
        /// <param name="pressedTextureFileName">Pressed texture file name</param>
        /// <param name="pressedTextureRect">Pressed texture rectangle</param>
        /// <param name="caption">Caption description</param>
        public static UIButtonDescription DefaultTwoStateButton(string releasedTextureFileName, Vector4 releasedTextureRect, string pressedTextureFileName, Vector4 pressedTextureRect, UITextAreaDescription caption)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,

                TextureReleased = releasedTextureFileName,
                TextureReleasedUVMap = releasedTextureRect,

                TexturePressed = pressedTextureFileName,
                TexturePressedUVMap = pressedTextureRect,

                Caption = caption,
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
        public Color4 ColorReleased { get; set; } = new Color4(1, 1, 1, 1);
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
        public Color4 ColorPressed { get; set; } = new Color4(1, 1, 1, 1);
        /// <summary>
        /// Texture pressed UV map
        /// </summary>
        public Vector4 TexturePressedUVMap { get; set; } = new Vector4(0, 0, 1, 1);

        /// <summary>
        /// Caption description
        /// </summary>
        public UITextAreaDescription Caption { get; set; } = UITextAreaDescription.Default();

        /// <summary>
        /// Constructor
        /// </summary>
        public UIButtonDescription()
            : base()
        {

        }
    }
}
