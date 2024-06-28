using SharpDX;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Bounds item
    /// </summary>
    struct BoundsItem
    {
        /// <summary>
        /// Item index
        /// </summary>
        public int Index;
        /// <summary>
        /// Bounds
        /// </summary>
        public RectangleF Bounds;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Index {Index}; Min {Bounds.TopLeft}; Max {Bounds.BottomRight};";
        }
    }
}
