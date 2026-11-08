
namespace Engine.Collections
{
    /// <summary>
    /// Quadtree generation options
    /// </summary>
    /// <param name="maxDepth">Maximum depth</param>
    /// <param name="separateSubdivisions">Separate subdivisions</param>
    /// <param name="separation">Subdivision separation distance</param>
    public struct QuadTreeOptions(int maxDepth, bool separateSubdivisions = false, float separation = float.Epsilon)
    {
        /// <summary>
        /// Maximum depth
        /// </summary>
        public int MaxDepth { get; set; } = maxDepth;
        /// <summary>
        /// Separate subdivisions
        /// </summary>
        public bool SeparateSubdivisions { get; set; } = separateSubdivisions;
        /// <summary>
        /// Subdivision separation distance
        /// </summary>
        public float Separation { get; set; } = separation;
    }
}
