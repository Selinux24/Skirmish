using System;

namespace Engine
{
    /// <summary>
    /// A bounding rectangle represeted by integers.
    /// </summary>
    [Serializable]
    public struct BoundingRectangleInt : IEquatable<BoundingRectangleInt>
    {
        /// <summary>
        /// The minimum of the bounding box.
        /// </summary>
        public Vector2Int Min { get; set; }
        /// <summary>
        /// The maximum of the bounding box.
        /// </summary>
        public Vector2Int Max { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BBox2i"/> struct.
        /// </summary>
        /// <param name="min">A minimum bound.</param>
        /// <param name="max">A maximum bound.</param>
        public BoundingRectangleInt(Vector2Int min, Vector2Int max)
        {
            Min = min;
            Max = max;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BBox2i"/> struct.
        /// </summary>
        /// <param name="minX">The minimum X bound.</param>
        /// <param name="minY">The minimum Y bound.</param>
        /// <param name="maxX">The maximum X bound.</param>
        /// <param name="maxY">The maximum Y bound.</param>
        public BoundingRectangleInt(int minX, int minY, int maxX, int maxY)
        {
            Min = new Vector2Int(minX, minY);
            Max = new Vector2Int(maxX, maxY);
        }

        /// <inheritdoc/>
        public static bool operator ==(BoundingRectangleInt left, BoundingRectangleInt right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(BoundingRectangleInt left, BoundingRectangleInt right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            var objV = obj as BoundingRectangleInt?;
            if (objV != null)
            {
                return Equals(objV);
            }

            return false;
        }
        /// <inheritdoc/>
        public readonly bool Equals(BoundingRectangleInt other)
        {
            return 
                Min == other.Min && 
                Max == other.Max;
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Min: {Min}; Max: {Max};";
        }
    }
}
