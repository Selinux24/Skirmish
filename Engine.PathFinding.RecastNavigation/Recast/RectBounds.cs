using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Rectangle bounds
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public struct RectBounds(int xMin, int yMin, int xMax, int yMax)
    {
        /// <summary>
        /// Minimum
        /// </summary>
        public Vector2Int Min { get; set; } = new(xMin, yMin);
        /// <summary>
        /// Maximum
        /// </summary>
        public Vector2Int Max { get; set; } = new(xMax, yMax);

        /// <summary>
        /// Gets the bounds rectangle
        /// </summary>
        public readonly Rectangle GetRectangle()
        {
            return new Rectangle(Min.X, Min.Y, Max.X - Min.X, Max.Y - Min.Y);
        }
    }
}
