using Engine.UI;
using SharpDX;

namespace Engine.BuiltIn.UI
{
    /// <summary>
    /// Checkbox description
    /// </summary>
    public class UICheckboxDescription : UIControlDescription
    {
        /// <summary>
        /// Gets the default checkbox description
        /// </summary>
        public static UICheckboxDescription Default()
        {
            return new();
        }
        /// <summary>
        /// Gets the default checkbox description
        /// </summary>
        /// <param name="colorOn">State on color</param>
        /// <param name="colorOff">State off color</param>
        public static UICheckboxDescription Default(Color4 colorOn, Color4 colorOff)
        {
            return new()
            {
                StateOnColor = colorOn,
                StateOffColor = colorOff,
            };
        }
        /// <summary>
        /// Gets the default checkbox description
        /// </summary>
        /// <param name="stateOnTextureFileName">State on texture file name</param>
        /// <param name="stateOffTextureFileName">State off texture file name</param>
        public static UICheckboxDescription Default(string stateOnTextureFileName, string stateOffTextureFileName)
        {
            return new()
            {
                StateOnTexture = stateOnTextureFileName,
                StateOffTexture = stateOffTextureFileName,
                StateOnColor = Color4.White,
                StateOffColor = Color4.White,
            };
        }
        /// <summary>
        /// Gets the default checkbox description
        /// </summary>
        /// <param name="stateOnTextureFileName">State on texture file name</param>
        /// <param name="stateOnTextureRect">State on texture rectangle</param>
        /// <param name="stateOffTextureFileName">State off texture file name</param>
        /// <param name="stateOffTextureRect">State off texture rectangle</param>
        public static UICheckboxDescription Default(string stateOnTextureFileName, Vector4 stateOnTextureRect, string stateOffTextureFileName, Vector4 stateOffTextureRect)
        {
            return new()
            {
                StateOnTexture = stateOnTextureFileName,
                StateOffTexture = stateOffTextureFileName,
                StateOnTextureUVMap = stateOnTextureRect,
                StateOffTextureUVMap = stateOffTextureRect,
                StateOnColor = Color4.White,
                StateOffColor = Color4.White,
            };
        }
        /// <summary>
        /// Gets the default checkbox description
        /// </summary>
        /// <param name="font">Font description</param>
        public static UICheckboxDescription Default(TextDrawerDescription font)
        {
            return new()
            {
                Font = font,
            };
        }
        /// <summary>
        /// Gets the default checkbox description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="colorOn">State on color</param>
        /// <param name="colorOff">State off color</param>
        public static UICheckboxDescription Default(TextDrawerDescription font, Color4 colorOn, Color4 colorOff)
        {
            return new()
            {
                Font = font,
                StateOnColor = colorOn,
                StateOffColor = colorOff,
            };
        }
        /// <summary>
        /// Gets the default checkbox description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="stateOnTextureFileName">State on texture file name</param>
        /// <param name="stateOffTextureFileName">State off texture file name</param>
        public static UICheckboxDescription Default(TextDrawerDescription font, string stateOnTextureFileName, string stateOffTextureFileName)
        {
            return new()
            {
                Font = font,
                StateOnTexture = stateOnTextureFileName,
                StateOffTexture = stateOffTextureFileName,
                StateOnColor = Color4.White,
                StateOffColor = Color4.White,
            };
        }
        /// <summary>
        /// Gets the default checkbox description
        /// </summary>
        /// <param name="font">Font description</param>
        /// <param name="stateOnTextureFileName">State on texture file name</param>
        /// <param name="stateOnTextureRect">State on texture rectangle</param>
        /// <param name="stateOffTextureFileName">State off texture file name</param>
        /// <param name="stateOffTextureRect">State off texture rectangle</param>
        public static UICheckboxDescription Default(TextDrawerDescription font, string stateOnTextureFileName, Vector4 stateOnTextureRect, string stateOffTextureFileName, Vector4 stateOffTextureRect)
        {
            return new()
            {
                Font = font,
                StateOnTexture = stateOnTextureFileName,
                StateOffTexture = stateOffTextureFileName,
                StateOnTextureUVMap = stateOnTextureRect,
                StateOffTextureUVMap = stateOffTextureRect,
                StateOnColor = Color4.White,
                StateOffColor = Color4.White,
            };
        }

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
        public TextHorizontalAlign TextHorizontalAlign { get; set; } = TextHorizontalAlign.Left;
        /// <summary>
        /// Text vertical alignement
        /// </summary>
        public TextVerticalAlign TextVerticalAlign { get; set; } = TextVerticalAlign.Middle;

        /// <summary>
        /// Texture to show when state on
        /// </summary>
        public string StateOnTexture { get; set; }
        /// <summary>
        /// State on button color
        /// </summary>
        public Color4 StateOnColor { get; set; } = UIConfiguration.HighlightColor;
        /// <summary>
        /// State on texture UV map
        /// </summary>
        public Vector4 StateOnTextureUVMap { get; set; } = new Vector4(0, 0, 1, 1);

        /// <summary>
        /// Texture to show when state off
        /// </summary>
        public string StateOffTexture { get; set; }
        /// <summary>
        /// State off button color
        /// </summary>
        public Color4 StateOffColor { get; set; } = UIConfiguration.BaseColor;
        /// <summary>
        /// State off texture UV map
        /// </summary>
        public Vector4 StateOffTextureUVMap { get; set; } = new Vector4(0, 0, 1, 1);
    }
}
