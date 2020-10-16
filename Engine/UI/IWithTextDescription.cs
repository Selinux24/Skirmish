using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// With text description interface
    /// </summary>
    public interface IWithTextDescription
    {
        /// <summary>
        /// Font description
        /// </summary>
        TextDrawerDescription Font { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// Text fore color
        /// </summary>
        Color4 TextForeColor { get; set; }
        /// <summary>
        /// Text shadow color
        /// </summary>
        Color4 TextShadowColor { get; set; }
        /// <summary>
        /// Shadow position delta
        /// </summary>
        Vector2 TextShadowDelta { get; set; }
        /// <summary>
        /// Text horizontal alignement
        /// </summary>
        HorizontalTextAlign TextHorizontalAlign { get; set; }
        /// <summary>
        /// Text vertical alignement
        /// </summary>
        VerticalTextAlign TextVerticalAlign { get; set; }
    }
}
