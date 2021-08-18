using SharpDX;
using System.Linq;

namespace Engine.Effects
{
    /// <summary>
    /// Effect spriteParameters
    /// </summary>
    public struct SpriteEffectParameters
    {
        /// <summary>
        /// First color
        /// </summary>
        public Color4 Color1 { get; set; }
        /// <summary>
        /// Secondo color
        /// </summary>
        public Color4 Color2 { get; set; }
        /// <summary>
        /// Third color
        /// </summary>
        public Color4 Color3 { get; set; }
        /// <summary>
        /// Fourth color
        /// </summary>
        public Color4 Color4 { get; set; }
        /// <summary>
        /// First percentage
        /// </summary>
        public float Percentage1 { get; set; }
        /// <summary>
        /// Second percentage
        /// </summary>
        public float Percentage2 { get; set; }
        /// <summary>
        /// Third percentage
        /// </summary>
        public float Percentage3 { get; set; }
        /// <summary>
        /// Render area in pixels
        /// </summary>
        public RectangleF RenderArea { get; set; }
        /// <summary>
        /// Draw direction
        /// </summary>
        /// <remarks>0 is horizontal, 1 is vertical</remarks>
        public int Direction { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leftColor">Left sprite color</param>
        /// <param name="rightColor">Right sprite color</param>
        /// <param name="percentage">Percentage</param>
        /// <param name="renderArea">Render area in pixels</param>
        /// <param name="direction">Draw direction</param>
        public SpriteEffectParameters(Color4 leftColor, Color4 rightColor, float percentage, int direction, RectangleF renderArea)
        {
            Color1 = leftColor;
            Color2 = rightColor;
            Color3 = Color.Transparent;
            Color4 = Color.Transparent;
            Percentage1 = percentage;
            Percentage2 = 1;
            Percentage3 = 1;
            Direction = direction;
            RenderArea = renderArea;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="colors">Color list</param>
        /// <param name="percentages">Percentage list</param>
        /// <param name="renderArea">Render area in pixels</param>
        /// <param name="direction">Draw direction</param>
        public SpriteEffectParameters(Color4[] colors, float[] percentages, int direction, RectangleF renderArea)
        {
            Color1 = colors?.ElementAtOrDefault(0) ?? Color.Transparent;
            Color2 = colors?.ElementAtOrDefault(1) ?? Color.Transparent;
            Color3 = colors?.ElementAtOrDefault(2) ?? Color.Transparent;
            Color4 = colors?.ElementAtOrDefault(3) ?? Color.Transparent;
            Percentage1 = percentages?.ElementAtOrDefault(0) ?? 1;
            Percentage2 = percentages?.ElementAtOrDefault(1) ?? 1;
            Percentage3 = percentages?.ElementAtOrDefault(2) ?? 1;
            Direction = direction;
            RenderArea = renderArea;
        }
    }
}
