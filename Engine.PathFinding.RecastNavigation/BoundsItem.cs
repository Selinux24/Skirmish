using SharpDX;
using System.Collections.Generic;

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

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return $"Index {Index}; Min {Bounds.TopLeft}; Max {Bounds.BottomRight};";
        }
    }

    /// <summary>
    /// An <see cref="IComparer{T}"/> implementation that only compares two <see cref="BoundsItem"/>s on the X axis.
    /// </summary>
    class BoundsItemComparerX : IComparer<BoundsItem>
    {
        /// <summary>
        /// Compares two nodes's bounds on the X axis.
        /// </summary>
        /// <param name="x">A node.</param>
        /// <param name="y">Another node.</param>
        /// <returns>A negative value if a is less than b; 0 if they are equal; a positive value of a is greater than b.</returns>
        public int Compare(BoundsItem x, BoundsItem y)
        {
            if (x.Bounds.Left < y.Bounds.Left) return -1;
            if (x.Bounds.Left > y.Bounds.Left) return 1;
            if (x.Bounds.Right < y.Bounds.Right) return -1;
            if (x.Bounds.Right > y.Bounds.Right) return 1;
            if (x.Index < y.Index) return -1;
            if (x.Index > y.Index) return 1;
            return 0;
        }
    }

    /// <summary>
    /// An <see cref="IComparer{T}"/> implementation that only compares two <see cref="BoundsItem"/>s on the Y axis.
    /// </summary>
    class BoundsItemComparerY : IComparer<BoundsItem>
    {
        /// <summary>
        /// Compares two nodes's bounds on the Y axis.
        /// </summary>
        /// <param name="x">A node.</param>
        /// <param name="y">Another node.</param>
        /// <returns>A negative value if a is less than b; 0 if they are equal; a positive value of a is greater than b.</returns>
        public int Compare(BoundsItem x, BoundsItem y)
        {
            if (x.Bounds.Top < y.Bounds.Top) return -1;
            if (x.Bounds.Top > y.Bounds.Top) return 1;
            if (x.Bounds.Bottom < y.Bounds.Bottom) return -1;
            if (x.Bounds.Bottom > y.Bounds.Bottom) return 1;
            if (x.Index < y.Index) return -1;
            if (x.Index > y.Index) return 1;
            return 0;
        }
    }
}
