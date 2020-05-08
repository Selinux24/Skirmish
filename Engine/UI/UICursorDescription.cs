using SharpDX;

namespace Engine.UI
{
    /// <summary>
    /// Cursor description
    /// </summary>
    public class UICursorDescription : SpriteDescription
    {
        /// <summary>
        /// Delta to position
        /// </summary>
        public Vector2 Delta { get; set; } = Vector2.Zero;
        /// <summary>
        /// Center cursor sprite
        /// </summary>
        public bool Centered { get; set; } = true;
    }
}
