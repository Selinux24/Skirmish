using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Sprite button description
    /// </summary>
    public class UIButtonDescription : UIControlDescription, IWithTextDescription
    {
        /// <summary>
        /// Gets the default button description
        /// </summary>
        public static UIButtonDescription Default()
        {
            return new UIButtonDescription();
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="color">Button color</param>
        public static UIButtonDescription Default(Color4 color)
        {
            var blendMode = color.Alpha >= 1f ? BlendModes.Default : BlendModes.DefaultTransparent;

            return new UIButtonDescription()
            {
                ColorReleased = color,
                BlendMode = blendMode,
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
                TextureReleased = textureFileName,
                ColorReleased = Color4.White,
                ColorPressed = Color4.White,
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
                TextureReleased = textureFileName,
                TextureReleasedUVMap = textureRect,
                ColorReleased = Color4.White,
                ColorPressed = Color4.White,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="font">Font description</param>
        public static UIButtonDescription Default(TextDrawerDescription font)
        {
            return new UIButtonDescription()
            {
                Font = font,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="color">Button color</param>
        public static UIButtonDescription Default(TextDrawerDescription font, Color4 color)
        {
            var blendMode = color.Alpha >= 1f ? BlendModes.Default : BlendModes.DefaultTransparent;

            return new UIButtonDescription()
            {
                Font = font,
                ColorReleased = color,
                BlendMode = blendMode,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="textureFileName">Texture file name</param>
        public static UIButtonDescription Default(TextDrawerDescription font, string textureFileName)
        {
            return new UIButtonDescription()
            {
                Font = font,
                TextureReleased = textureFileName,
                ColorReleased = Color4.White,
                ColorPressed = Color4.White,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="textureRect">Texture rectangle</param>
        public static UIButtonDescription Default(TextDrawerDescription font, string textureFileName, Vector4 textureRect)
        {
            return new UIButtonDescription()
            {
                Font = font,
                TextureReleased = textureFileName,
                TextureReleasedUVMap = textureRect,
                ColorReleased = Color4.White,
                ColorPressed = Color4.White,
            };
        }

        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        public static UIButtonDescription DefaultTwoStateButton()
        {
            return new UIButtonDescription()
            {
                TwoStateButton = true,
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
                TwoStateButton = true,

                ColorReleased = releasedColor,

                ColorPressed = pressedColor,

                BlendMode = blendMode,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="releasedTextureFileName">Released texture file name</param>
        /// <param name="pressedTextureFileName">Pressed texture file name</param>
        /// <param name="caption">Caption description</param>
        public static UIButtonDescription DefaultTwoStateButton(string releasedTextureFileName, string pressedTextureFileName)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,

                TextureReleased = releasedTextureFileName,

                TexturePressed = pressedTextureFileName,
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
                TwoStateButton = true,

                TextureReleased = textureFileName,
                TextureReleasedUVMap = releasedTextureRect,
                ColorReleased = Color4.White,

                TexturePressed = textureFileName,
                TexturePressedUVMap = pressedTextureRect,
                ColorPressed = Color4.White,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="font">Font description</param>
        public static UIButtonDescription DefaultTwoStateButton(TextDrawerDescription font)
        {
            return new UIButtonDescription()
            {
                TwoStateButton = true,
                Font = font,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="releasedColor">Released button color</param>
        /// <param name="pressedColor">Pressed button color</param>
        public static UIButtonDescription DefaultTwoStateButton(TextDrawerDescription font, Color4 releasedColor, Color4 pressedColor)
        {
            var blendMode = releasedColor.Alpha >= 1f && pressedColor.Alpha > 1f ? BlendModes.Default : BlendModes.DefaultTransparent;

            return new UIButtonDescription()
            {
                TwoStateButton = true,
                Font = font,

                ColorReleased = releasedColor,

                ColorPressed = pressedColor,

                BlendMode = blendMode,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="releasedTextureFileName">Released texture file name</param>
        /// <param name="pressedTextureFileName">Pressed texture file name</param>
        public static UIButtonDescription DefaultTwoStateButton(TextDrawerDescription font, string releasedTextureFileName, string pressedTextureFileName)
        {
            return new UIButtonDescription()
            {
                Width = 120,
                Height = 50,

                TwoStateButton = true,
                Font = font,

                TextureReleased = releasedTextureFileName,

                TexturePressed = pressedTextureFileName,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="releasedTextureRect">Released texture rectangle</param>
        /// <param name="pressedTextureRect">Pressed texture rectangle</param>
        public static UIButtonDescription DefaultTwoStateButton(TextDrawerDescription font, string textureFileName, Vector4 releasedTextureRect, Vector4 pressedTextureRect)
        {
            return new UIButtonDescription()
            {
                TwoStateButton = true,
                Font = font,

                TextureReleased = textureFileName,
                TextureReleasedUVMap = releasedTextureRect,
                ColorReleased = Color4.White,

                TexturePressed = textureFileName,
                TexturePressedUVMap = pressedTextureRect,
                ColorPressed = Color4.White,
            };
        }

        /// <summary>
        /// Gets the default button description from a font family name
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="size">Font size</param>
        /// <param name="fineSampling">Fine sampling</param>
        public static UIButtonDescription DefaultFromFamily(string fontFamilyName, int size, bool fineSampling = false)
        {
            return new UIButtonDescription()
            {
                Font = TextDrawerDescription.FromFamily(fontFamilyName, size, fineSampling),
            };
        }
        /// <summary>
        /// Gets the default button description from a font family name
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="size">Font size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="fineSampling">Fine sampling</param>
        public static UIButtonDescription DefaultFromFamily(string fontFamilyName, int size, FontMapStyles fontStyle, bool fineSampling = false)
        {
            return new UIButtonDescription()
            {
                Font = TextDrawerDescription.FromFamily(fontFamilyName, size, fontStyle, fineSampling),
            };
        }

        /// <summary>
        /// Gets the default button description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UIButtonDescription DefaultFromFile(string fileName, int size, bool lineAdjust = false)
        {
            return new UIButtonDescription()
            {
                Font = TextDrawerDescription.FromFile(fileName, size, lineAdjust),
            };
        }
        /// <summary>
        /// Gets the default button description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UIButtonDescription DefaultFromFile(string fileName, int size, FontMapStyles fontStyle, bool lineAdjust = false)
        {
            return new UIButtonDescription()
            {
                Font = TextDrawerDescription.FromFile(fileName, size, fontStyle, lineAdjust),
            };
        }

        /// <summary>
        /// Gets the default button description from a font map 
        /// </summary>
        /// <param name="fontImageFileName">Font image file name</param>
        /// <param name="fontMapFileName">Font map file name</param>
        public static UIButtonDescription DefaultFromMap(string fontImageFileName, string fontMapFileName)
        {
            return new UIButtonDescription
            {
                Font = TextDrawerDescription.FromMap(fontImageFileName, fontMapFileName),
            };
        }

        /// <summary>
        /// Gets the default two state button description from a font family name
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="size">Font size</param>
        /// <param name="fineSampling">Fine sampling</param>
        public static UIButtonDescription DefaultTwoStateButtonFromFamily(string fontFamilyName, int size, bool fineSampling = false)
        {
            return new UIButtonDescription()
            {
                TwoStateButton = true,
                Font = TextDrawerDescription.FromFamily(fontFamilyName, size, fineSampling),
            };
        }
        /// <summary>
        /// Gets the default two state button description from a font family name
        /// </summary>
        /// <param name="fontFamilyName">Font family name</param>
        /// <param name="size">Font size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="fineSampling">Fine sampling</param>
        public static UIButtonDescription DefaultTwoStateButtonFromFamily(string fontFamilyName, int size, FontMapStyles fontStyle, bool fineSampling = false)
        {
            return new UIButtonDescription()
            {
                TwoStateButton = true,
                Font = TextDrawerDescription.FromFamily(fontFamilyName, size, fontStyle, fineSampling),
            };
        }

        /// <summary>
        /// Gets the default two state button description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UIButtonDescription DefaultTwoStateButtonFromFile(string fileName, int size, bool lineAdjust = false)
        {
            return new UIButtonDescription()
            {
                TwoStateButton = true,
                Font = TextDrawerDescription.FromFile(fileName, size, lineAdjust),
            };
        }
        /// <summary>
        /// Gets the default two state button description from a font file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="size">Size</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="lineAdjust">Line adjust</param>
        public static UIButtonDescription DefaultTwoStateButtonFromFile(string fileName, int size, FontMapStyles fontStyle, bool lineAdjust = false)
        {
            return new UIButtonDescription()
            {
                TwoStateButton = true,
                Font = TextDrawerDescription.FromFile(fileName, size, fontStyle, lineAdjust),
            };
        }

        /// <summary>
        /// Gets the default two state button description from a font map 
        /// </summary>
        /// <param name="fontImageFileName">Font image file name</param>
        /// <param name="fontMapFileName">Font map file name</param>
        public static UIButtonDescription DefaultTwoStateButtonFromMap(string fontImageFileName, string fontMapFileName)
        {
            return new UIButtonDescription
            {
                TwoStateButton = true,
                Font = TextDrawerDescription.FromMap(fontImageFileName, fontMapFileName),
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
        public Color4 ColorReleased { get; set; } = UIConfiguration.ForeColor;
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
        public Color4 ColorPressed { get; set; } = UIConfiguration.HighlightColor;
        /// <summary>
        /// Texture pressed UV map
        /// </summary>
        public Vector4 TexturePressedUVMap { get; set; } = new Vector4(0, 0, 1, 1);

        /// <summary>
        /// Font description
        /// </summary>
        public TextDrawerDescription Font { get; set; } = UIConfiguration.Font;

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Text fore color
        /// </summary>
        public Color4 TextForeColor { get; set; } = UIConfiguration.TextColor;
        /// <summary>
        /// Text shadow color
        /// </summary>
        public Color4 TextShadowColor { get; set; } = Color.Transparent;
        /// <summary>
        /// Shadow position delta
        /// </summary>
        public Vector2 TextShadowDelta { get; set; } = new Vector2(1, 1);
        /// <summary>
        /// Text horizontal alignement
        /// </summary>
        public TextHorizontalAlign TextHorizontalAlign { get; set; } = TextHorizontalAlign.Center;
        /// <summary>
        /// Text vertical alignement
        /// </summary>
        public TextVerticalAlign TextVerticalAlign { get; set; } = TextVerticalAlign.Middle;

        /// <summary>
        /// Constructor
        /// </summary>
        public UIButtonDescription()
            : base()
        {
            Width = 120;
            Height = 50;
        }
    }
}
