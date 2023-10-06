
namespace Engine
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
            return new QuadtreeDescription
            {
                MaximumDepth = depth,
            };
        }

        /// <summary>
        /// Maximum depth
        /// </summary>
        public int MaximumDepth { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public QuadtreeDescription()
        {
            MaximumDepth = 3;
        }
    }
}
