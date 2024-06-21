using SharpDX;

namespace Engine.BuiltIn.Components.Flares
{
    /// <summary>
    /// Flare description
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="distance">Distance from light source along light ray</param>
    /// <param name="scale">Relative scale</param>
    /// <param name="color">Color</param>
    /// <param name="texture">Texture name</param>
    public class LensFlareArtifact(float distance, float scale, Color color, string texture)
    {
        /// <summary>
        /// Distance from light source along light ray
        /// </summary>
        public float Distance { get; set; } = distance;
        /// <summary>
        /// Relative scale
        /// </summary>
        public float Scale { get; set; } = scale;
        /// <summary>
        /// Color
        /// </summary>
        public Color Color { get; set; } = color;
        /// <summary>
        /// Texture
        /// </summary>
        public string Texture { get; set; } = texture;
    }
}
