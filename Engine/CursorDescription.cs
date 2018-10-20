using SharpDX;

namespace Engine
{
    /// <summary>
    /// Cursor description
    /// </summary>
    public class CursorDescription : SpriteDescription
    {
        /// <summary>
        /// Delta to position
        /// </summary>
        public Vector2 Delta { get; set; } = Vector2.Zero;
    }
}
