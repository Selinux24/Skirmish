
namespace Engine.BuiltIn.Components.Ground
{
    /// <summary>
    /// Quadtree description
    /// </summary>
    public class QuadtreeDescription
    {
        /// <summary>
        /// Creates a quadtree description
        /// </summary>
        /// <param name="depth">Depth</param>
        public static QuadtreeDescription Default(int depth)
        {
            return new()
            {
                MaximumDepth = depth,
            };
        }

        /// <summary>
        /// Maximum depth
        /// </summary>
        public int MaximumDepth { get; set; }
    }
}
