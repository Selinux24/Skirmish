using Engine.UI;
using System.Numerics;

namespace Engine.BuiltIn.UI
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
            return new();
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="color">Button color</param>
        public static UIButtonDescription Default(Vector4 color)
        {
            return new()
            {
                ColorReleased = color,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="textureFileName">Texture file name</param>
        public static UIButtonDescription Default(string textureFileName)
        {
            return new()
            {
                TextureReleased = textureFileName,
                ColorReleased = Vector4.One,
                ColorPressed = Vector4.One,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="textureRect">Texture rectangle</param>
        public static UIButtonDescription Default(string textureFileName, Vector4 textureRect)
        {
            return new()
            {
                TextureReleased = textureFileName,
                TextureReleasedUVMap = textureRect,
                ColorReleased = Vector4.One,
                ColorPressed = Vector4.One,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="font">Font description</param>
        public static UIButtonDescription Default(FontDescription font)
        {
            return new()
            {
                Font = font,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="color">Button color</param>
        public static UIButtonDescription Default(FontDescription font, Vector4 color)
        {
            return new()
            {
                Font = font,
                ColorReleased = color,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="textureFileName">Texture file name</param>
        public static UIButtonDescription Default(FontDescription font, string textureFileName)
        {
            return new()
            {
                Font = font,
                TextureReleased = textureFileName,
                ColorReleased = Vector4.One,
                ColorPressed = Vector4.One,
            };
        }
        /// <summary>
        /// Gets the default button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="textureFileName">Texture file name</param>
        /// <param name="textureRect">Texture rectangle</param>
        public static UIButtonDescription Default(FontDescription font, string textureFileName, Vector4 textureRect)
        {
            return new()
            {
                Font = font,
                TextureReleased = textureFileName,
                TextureReleasedUVMap = textureRect,
                ColorReleased = Vector4.One,
                ColorPressed = Vector4.One,
            };
        }

        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        public static UIButtonDescription DefaultTwoStateButton()
        {
            return new()
            {
                TwoStateButton = true,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="releasedColor">Released button color</param>
        /// <param name="pressedColor">Pressed button color</param>
        public static UIButtonDescription DefaultTwoStateButton(Vector4 releasedColor, Vector4 pressedColor)
        {
            return new()
            {
                TwoStateButton = true,
                ColorReleased = releasedColor,
                ColorPressed = pressedColor,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="releasedTextureFileName">Released texture file name</param>
        /// <param name="pressedTextureFileName">Pressed texture file name</param>
        public static UIButtonDescription DefaultTwoStateButton(string releasedTextureFileName, string pressedTextureFileName)
        {
            return new()
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
            return new()
            {
                TwoStateButton = true,
                TextureReleased = textureFileName,
                TextureReleasedUVMap = releasedTextureRect,
                ColorReleased = Vector4.One,
                TexturePressed = textureFileName,
                TexturePressedUVMap = pressedTextureRect,
                ColorPressed = Vector4.One,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="font">Font description</param>
        public static UIButtonDescription DefaultTwoStateButton(FontDescription font)
        {
            return new()
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
        public static UIButtonDescription DefaultTwoStateButton(FontDescription font, Vector4 releasedColor, Vector4 pressedColor)
        {
            return new()
            {
                TwoStateButton = true,
                Font = font,
                ColorReleased = releasedColor,
                ColorPressed = pressedColor,
            };
        }
        /// <summary>
        /// Gets the default two state button description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="releasedTextureFileName">Released texture file name</param>
        /// <param name="pressedTextureFileName">Pressed texture file name</param>
        public static UIButtonDescription DefaultTwoStateButton(FontDescription font, string releasedTextureFileName, string pressedTextureFileName)
        {
            return new()
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
        public static UIButtonDescription DefaultTwoStateButton(FontDescription font, string textureFileName, Vector4 releasedTextureRect, Vector4 pressedTextureRect)
        {
            return new()
            {
                TwoStateButton = true,
                Font = font,
                TextureReleased = textureFileName,
                TextureReleasedUVMap = releasedTextureRect,
                ColorReleased = Vector4.One,
                TexturePressed = textureFileName,
                TexturePressedUVMap = pressedTextureRect,
                ColorPressed = Vector4.One,
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
            return new()
            {
                Font = Engine.UI.FontDescription.FromFamily(fontFamilyName, size, fineSampling),
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
            return new()
            {
                Font = FontDescription.FromFamily(fontFamilyName, size, fontStyle, fineSampling),
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
            return new()
            {
                Font = FontDescription.FromFile(fileName, size, lineAdjust),
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
            return new()
            {
                Font = FontDescription.FromFile(fileName, size, fontStyle, lineAdjust),
            };
        }

        /// <summary>
        /// Gets the default button description from a font map 
        /// </summary>
        /// <param name="fontImageFileName">Font image file name</param>
        /// <param name="fontMapFileName">Font map file name</param>
        public static UIButtonDescription DefaultFromMap(string fontImageFileName, string fontMapFileName)
        {
            return new()
            {
                Font = FontDescription.FromMap(fontImageFileName, fontMapFileName),
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
            return new()
            {
                TwoStateButton = true,
                Font = FontDescription.FromFamily(fontFamilyName, size, fineSampling),
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
            return new()
            {
                TwoStateButton = true,
                Font = FontDescription.FromFamily(fontFamilyName, size, fontStyle, fineSampling),
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
            return new()
            {
                TwoStateButton = true,
                Font = FontDescription.FromFile(fileName, size, lineAdjust),
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
            return new()
            {
                TwoStateButton = true,
                Font = FontDescription.FromFile(fileName, size, fontStyle, lineAdjust),
            };
        }

        /// <summary>
        /// Gets the default two state button description from a font map 
        /// </summary>
        /// <param name="fontImageFileName">Font image file name</param>
        /// <param name="fontMapFileName">Font map file name</param>
        public static UIButtonDescription DefaultTwoStateButtonFromMap(string fontImageFileName, string fontMapFileName)
        {
            return new()
            {
                TwoStateButton = true,
                Font = FontDescription.FromMap(fontImageFileName, fontMapFileName),
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
        public Vector4 ColorReleased { get; set; } = UIConfiguration.ForeColor;
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
        public Vector4 ColorPressed { get; set; } = UIConfiguration.HighlightColor;
        /// <summary>
        /// Texture pressed UV map
        /// </summary>
        public Vector4 TexturePressedUVMap { get; set; } = new Vector4(0, 0, 1, 1);

        /// <summary>
        /// Font description
        /// </summary>
        public FontDescription Font { get; set; } = UIConfiguration.Font;

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Text fore color
        /// </summary>
        public Vector4 TextForeColor { get; set; } = UIConfiguration.TextColor;
        /// <summary>
        /// Text shadow color
        /// </summary>
        public Vector4 TextShadowColor { get; set; } = Vector4.Zero;
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
