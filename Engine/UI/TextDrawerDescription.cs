using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Text drawer description
    /// </summary>
    public class TextDrawerDescription : SceneObjectDescription
    {
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="font">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="textColor">Text color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription Generate(string font, int size, Color textColor)
        {
            return Generate(font, size, FontMapStyles.Regular, textColor, Color.Transparent);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="font">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <param name="textColor">Text color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription Generate(string font, int size, FontMapStyles style, Color textColor)
        {
            return Generate(font, size, style, textColor, Color.Transparent);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="font">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="textColor">Text color</param>
        /// <param name="shadowColor">Shadow color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription Generate(string font, int size, Color textColor, Color shadowColor)
        {
            return Generate(font, size, FontMapStyles.Regular, textColor, shadowColor);
        }
        /// <summary>
        /// Generates a new description
        /// </summary>
        /// <param name="font">Font name</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <param name="textColor">Text color</param>
        /// <param name="shadowColor">Shadow color</param>
        /// <returns>Returns the new generated description</returns>
        public static TextDrawerDescription Generate(string font, int size, FontMapStyles style, Color textColor, Color shadowColor)
        {
            return new TextDrawerDescription()
            {
                Name = string.Format("TextBox {0} {1} {2}", font, size, style),
                Font = font,
                FontSize = size,
                Style = style,
                TextColor = textColor,
                ShadowColor = shadowColor,
            };
        }

        /// <summary>
        /// Font name
        /// </summary>
        public string Font { get; set; }
        /// <summary>
        /// Font size
        /// </summary>
        public int FontSize { get; set; }
        /// <summary>
        /// Font style
        /// </summary>
        public FontMapStyles Style { get; set; }
        /// <summary>
        /// Text color
        /// </summary>
        public Color4 TextColor { get; set; }
        /// <summary>
        /// Shadow color
        /// </summary>
        public Color4 ShadowColor { get; set; }
        /// <summary>
        /// Shadow position delta
        /// </summary>
        public Vector2 ShadowDelta { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TextDrawerDescription()
            : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;

            this.Style = FontMapStyles.Regular;
            this.ShadowDelta = new Vector2(1, -1);
        }
    }
}
