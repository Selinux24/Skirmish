using System.Collections.Generic;
using System.Linq;

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
        /// <param name="fineSampling">Fine sampling</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription Default(bool fineSampling = false)
        {
            return FromFamily(DefaultFontFamily, DefaultSize, FontMapStyles.Regular, fineSampling);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="fineSampling">Fine sampling</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription Default(int size, bool fineSampling = false)
        {
            return FromFamily(DefaultFontFamily, size, FontMapStyles.Regular, fineSampling);
        }

        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFamilyName">Comma separated font family name list</param>
        /// <param name="fineSampling">Fine sampling</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFamily(string fontFamilyName, bool fineSampling = false)
        {
            return FromFamily(fontFamilyName, DefaultSize, FontMapStyles.Regular, fineSampling);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFamilyName">Comma separated font family name list</param>
        /// <param name="size">Size</param>
        /// <param name="fineSampling">Fine sampling</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFamily(string fontFamilyName, int size, bool fineSampling = false)
        {
            return FromFamily(fontFamilyName, size, FontMapStyles.Regular, fineSampling);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="fontFamilyName">Comma separated font family name list</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <param name="fineSampling">Fine sampling</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription FromFamily(string fontFamilyName, int size, FontMapStyles style, bool fineSampling = false)
        {
            return new TextDrawerDescription()
            {
                FontFamily = fontFamilyName,
                FontSize = size,
                Style = style,
                FineSampling = fineSampling,
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
            return FromFile(fontFileName, DefaultSize, FontMapStyles.Regular, lineAdjust);
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
            return FromFile(fontFileName, size, FontMapStyles.Regular, lineAdjust);
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
            return new TextDrawerDescription()
            {
                FontFileName = fontFileName,
                FontSize = size,
                Style = style,
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
            return new TextDrawerDescription()
            {
                FontMapping = new FontMapping
                {
                    ImageFile = imageFileName,
                    MapFile = mapFileName,
                },
                UseTextureColor = true,
            };
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";
        /// <summary>
        /// Font family comma separated name list
        /// </summary>
        /// <remarks>For example: "Verdana, Consolas"</remarks>
        public string FontFamily { get; set; } = DefaultFontFamily;
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
        /// Use the texture color instead of the specified Color
        /// </summary>
        public bool UseTextureColor { get; set; } = false;
        /// <summary>
        /// Perform line adjust
        /// </summary>
        public bool LineAdjust { get; set; } = false;
        /// <summary>
        /// Fine sampling
        /// </summary>
        /// <remarks>
        /// If deactivated, the font will be drawn with a point sampler. Otherwise, a linear sampler will be used.
        /// Deactivate for thin fonts.
        /// </remarks>
        public bool FineSampling { get; set; } = true;
        /// <summary>
        /// Custom key codes to add to the default key code collection
        /// </summary>
        public IEnumerable<char> CustomKeycodes { get; set; } = Enumerable.Empty<char>();

        /// <summary>
        /// Constructor
        /// </summary>
        public TextDrawerDescription()
            : base()
        {
            DeferredEnabled = false;
            DepthEnabled = false;
            BlendMode = BlendModes.Alpha;
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
