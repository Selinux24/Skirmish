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
        /// XZ minimum bounds
        /// </summary>
        public Vector2 BMin;
        /// <summary>
        /// XZ maximum bounds
        /// </summary>
        public Vector2 BMax;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Index {0}; Min {1} Max {2}", Index, BMin, BMax);
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
            if (x.BMin.X < y.BMin.X) return -1;
            if (x.BMin.X > y.BMin.X) return 1;
            if (x.BMax.X < y.BMax.X) return -1;
            if (x.BMax.X > y.BMax.X) return 1;
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
            if (x.BMin.Y < y.BMin.Y) return -1;
            if (x.BMin.Y > y.BMin.Y) return 1;
            if (x.BMax.Y < y.BMax.Y) return -1;
            if (x.BMax.Y > y.BMax.Y) return 1;
            if (x.Index < y.Index) return -1;
            if (x.Index > y.Index) return 1;
            return 0;
        }
    }
}
