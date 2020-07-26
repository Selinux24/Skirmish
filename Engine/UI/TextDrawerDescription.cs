using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Text drawer description
    /// </summary>
    public class TextDrawerDescription : SceneObjectDescription
    {
        /// <summary>
        /// Default font family name
        /// </summary>
        public const string DefaultFontFamily = "Consolas";
        /// <summary>
        /// Default font size
        /// </summary>
        public const int DefaultSize = 12;

        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription Default()
        {
            return FromFamily(DefaultFontFamily, DefaultSize, FontMapStyles.Regular, Color.White, Color.Transparent);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="size">Size</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription Default(int size)
        {
            return FromFamily(DefaultFontFamily, size, FontMapStyles.Regular, Color.White, Color.Transparent);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="textColor">Text color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription Default(int size, Color textColor)
        {
            return new TextDrawerDescription()
            {
                Name = string.Format("TextBox {0} {1} {2}", DefaultFontFamily, size, FontMapStyles.Regular),
                Font = DefaultFontFamily,
                FontSize = size,
                Style = FontMapStyles.Regular,
                TextColor = textColor,
            };
        }

        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFamilyName">Font name</param>
        /// <param name="size">Size</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFamily(string fontFamilyName, int size)
        {
            return FromFamily(fontFamilyName, size, FontMapStyles.Regular, Color.White, Color.Transparent);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFamilyName">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="textColor">Text color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFamily(string fontFamilyName, int size, Color textColor)
        {
            return FromFamily(fontFamilyName, size, FontMapStyles.Regular, textColor, Color.Transparent);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFamilyName">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <param name="textColor">Text color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFamily(string fontFamilyName, int size, FontMapStyles style, Color textColor)
        {
            return FromFamily(fontFamilyName, size, style, textColor, Color.Transparent);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFamilyName">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="shadowColor">Shadow color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFamily(string fontFamilyName, int size, Color textColor, Color shadowColor)
        {
            return FromFamily(fontFamilyName, size, FontMapStyles.Regular, textColor, shadowColor);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFamilyName">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <param name="textColor">Text color</param>
        /// <param name="shadowColor">Shadow color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFamily(string fontFamilyName, int size, FontMapStyles style, Color textColor, Color shadowColor)
        {
            return new TextDrawerDescription()
            {
                Name = string.Format("TextBox {0} {1} {2}", fontFamilyName, size, style),
                Font = fontFamilyName,
                FontSize = size,
                Style = style,
                TextColor = textColor,
                ShadowColor = shadowColor,
            };
        }

        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="lineAdjust">Line adjust</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFile(string fontFileName, bool lineAdjust = false)
        {
            return FromFile(fontFileName, DefaultSize, FontMapStyles.Regular, Color.White, Color.Transparent, lineAdjust);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="size">Size</param>
        /// <param name="lineAdjust">Line adjust</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFile(string fontFileName, int size, bool lineAdjust = false)
        {
            return FromFile(fontFileName, size, FontMapStyles.Regular, Color.White, Color.Transparent, lineAdjust);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="size">Size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="lineAdjust">Line adjust</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFile(string fontFileName, int size, Color textColor, bool lineAdjust = false)
        {
            return FromFile(fontFileName, size, FontMapStyles.Regular, textColor, Color.Transparent, lineAdjust);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="size">Size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="shadowColor">Shadow color</param>
        /// <param name="lineAdjust">Line adjust</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFile(string fontFileName, int size, Color textColor, Color shadowColor, bool lineAdjust = false)
        {
            return FromFile(fontFileName, size, FontMapStyles.Regular, textColor, shadowColor, lineAdjust);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <param name="lineAdjust">Line adjust</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFile(string fontFileName, int size, FontMapStyles style, bool lineAdjust = false)
        {
            return FromFile(fontFileName, size, style, Color.White, Color.Transparent, lineAdjust);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <param name="textColor">Text color</param>
        /// <param name="lineAdjust">Line adjust</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFile(string fontFileName, int size, FontMapStyles style, Color textColor, bool lineAdjust = false)
        {
            return FromFile(fontFileName, size, style, textColor, Color.Transparent, lineAdjust);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFileName">Font file name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <param name="textColor">Text color</param>
        /// <param name="shadowColor">Shadow color</param>
        /// <param name="lineAdjust">Line adjust</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFile(string fontFileName, int size, FontMapStyles style, Color textColor, Color shadowColor, bool lineAdjust = false)
        {
            return new TextDrawerDescription()
            {
                Name = string.Format("TextBox {0} {1} {2}", fontFileName, size, style),
                FontFileName = fontFileName,
                FontSize = size,
                Style = style,
                TextColor = textColor,
                ShadowColor = shadowColor,
                LineAdjust = lineAdjust,
            };
        }

        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="imageFileName">Font image file name</param>
        /// <param name="mapFileName">Map file name</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromMap(string imageFileName, string mapFileName)
        {
            return FromMap(imageFileName, mapFileName, Color.White, Color.Transparent);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="imageFileName">Font image file name</param>
        /// <param name="mapFileName">Map file name</param>
        /// <param name="textColor">Text color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromMap(string imageFileName, string mapFileName, Color textColor)
        {
            return FromMap(imageFileName, mapFileName, textColor, Color.Transparent);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="imageFileName">Font image file name</param>
        /// <param name="mapFileName">Map file name</param>
        /// <param name="textColor">Text color</param>
        /// <param name="shadowColor">Shadow color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromMap(string imageFileName, string mapFileName, Color textColor, Color shadowColor)
        {
            return new TextDrawerDescription()
            {
                Name = string.Format("TextBox {0}", imageFileName),
                FontMapping = new FontMapping
                {
                    ImageFile = imageFileName,
                    MapFile = mapFileName,
                },
                TextColor = textColor,
                ShadowColor = shadowColor,
            };
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";
        /// <summary>
        /// Font name
        /// </summary>
        public string Font { get; set; } = DefaultFontFamily;
        /// <summary>
        /// Font file name
        /// </summary>
        public string FontFileName { get; set; }
        /// <summary>
        /// Font mapping
        /// </summary>
        public FontMapping FontMapping { get; set; }
        /// <summary>
        /// Font size
        /// </summary>
        public int FontSize { get; set; } = DefaultSize;
        /// <summary>
        /// Font style
        /// </summary>
        public FontMapStyles Style { get; set; } = FontMapStyles.Regular;
        /// <summary>
        /// Text color
        /// </summary>
        public Color4 TextColor { get; set; } = Color.White;
        /// <summary>
        /// Use the texture color instead of the specified Color
        /// </summary>
        public bool UseTextureColor { get; set; } = false;
        /// <summary>
        /// Shadow color
        /// </summary>
        public Color4 ShadowColor { get; set; } = Color.Transparent;
        /// <summary>
        /// Shadow position delta
        /// </summary>
        public Vector2 ShadowDelta { get; set; } = new Vector2(1, 1);
        /// <summary>
        /// Perform line adjust
        /// </summary>
        public bool LineAdjust { get; set; } = false;
        /// <summary>
        /// Horizontal alignement
        /// </summary>
        public TextAlign HorizontalAlign { get; set; } = TextAlign.Left;
        /// <summary>
        /// Vertical alignement
        /// </summary>
        public VerticalAlign VerticalAlign { get; set; } = VerticalAlign.Top;

        /// <summary>
        /// Constructor
        /// </summary>
        public TextDrawerDescription()
            : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.BlendMode = BlendModes.Alpha;
        }
    }

    /// <summary>
    /// Font mapping description
    /// </summary>
    public class FontMapping
    {
        /// <summary>
        /// Image filename
        /// </summary>
        public string ImageFile { get; set; }
        /// <summary>
        /// Map filename
        /// </summary>
        public string MapFile { get; set; }
    }
}
