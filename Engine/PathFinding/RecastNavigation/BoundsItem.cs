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
        public int i;
        /// <summary>
        /// XZ minimum bounds
        /// </summary>
        public Vector2 bmin;
        /// <summary>
        /// XZ maximum bounds
        /// </summary>
        public Vector2 bmax;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Index {0}; Min {1} Max {2}", i, bmin, bmax);
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
            if (x.bmin.X < y.bmin.X) return -1;
            if (x.bmin.X > y.bmin.X) return 1;
            if (x.bmax.X < y.bmax.X) return -1;
            if (x.bmax.X > y.bmax.X) return 1;
            if (x.i < y.i) return -1;
            if (x.i > y.i) return 1;
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
            if (x.bmin.Y < y.bmin.Y) return -1;
            if (x.bmin.Y > y.bmin.Y) return 1;
            if (x.bmax.Y < y.bmax.Y) return -1;
            if (x.bmax.Y > y.bmax.Y) return 1;
            if (x.i < y.i) return -1;
            if (x.i > y.i) return 1;
            return 0;
        }
    }
}
