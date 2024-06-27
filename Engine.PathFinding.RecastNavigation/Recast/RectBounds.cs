using SharpDX;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Rectangle bounds
    /// </summary>
    public struct RectBounds
    {
        /// <summary>
        /// Minimum
        /// </summary>
        public Vector2Int Min { get; set; }
        /// <summary>
        /// Maximum
        /// </summary>
        public Vector2Int Max { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RectBounds(int xMin, int yMin, int xMax, int yMax)
        {
            Min = new(xMin, yMin);
            Max = new(xMax, yMax);
        }

        /// <summary>
        /// Gets the bounds rectangle
        /// </summary>
        public readonly Rectangle GetRectangle()
        {
            return new Rectangle(Min.X, Min.Y, Max.X - Min.X, Max.Y - Min.Y);
        }
    }
}
