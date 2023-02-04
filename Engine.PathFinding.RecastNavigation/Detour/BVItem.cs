using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Bounding volume item.
    /// </summary>
    [Serializable]
    public struct BVItem
    {
        /// <summary>
        /// X axis comparer
        /// </summary>
        public static readonly CompareX XComparer = new CompareX();
        /// <summary>
        /// Y axis comparer
        /// </summary>
        public static readonly CompareY YComparer = new CompareY();
        /// <summary>
        /// Z axis comparer
        /// </summary>
        public static readonly CompareZ ZComparer = new CompareZ();

        /// <summary>
        /// Minimum bounds of the item's AABB. [(x, y, z)]
        /// </summary>
        public Int3 BMin { get; set; }
        /// <summary>
        /// Maximum bounds of the item's AABB. [(x, y, z)]
        /// </summary>
        public Int3 BMax { get; set; }
        /// <summary>
        /// The item's index. (Negative for escape sequence.)
        /// </summary>
        public int I { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BVItem()
        {

        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return $"{nameof(BVItem)} Region Id: {I}; BMin: {BMin}; BMax: {BMax};";
        }

        /// <summary>
        /// An <see cref="IComparer{T}"/> implementation that only compares two <see cref="BoundingVolumeTreeNode"/>s on the X axis.
        /// </summary>
        public class CompareX : IComparer<BVItem>
        {
            /// <summary>
            /// Compares two nodes's bounds on the X axis.
            /// </summary>
            /// <param name="x">A node.</param>
            /// <param name="y">Another node.</param>
            /// <returns>A negative value if a is less than b; 0 if they are equal; a positive value of a is greater than b.</returns>
            public int Compare(BVItem x, BVItem y)
            {
                if (x.BMin.X < y.BMin.X) return -1;
                if (x.BMin.X > y.BMin.X) return 1;
                if (x.BMax.X < y.BMax.X) return -1;
                if (x.BMax.X > y.BMax.X) return 1;
                if (x.I < y.I) return -1;
                if (x.I > y.I) return 1;
                return 0;
            }
        }
        /// <summary>
        /// An <see cref="IComparer{T}"/> implementation that only compares two <see cref="BoundingVolumeTreeNode"/>s on the Y axis.
        /// </summary>
        public class CompareY : IComparer<BVItem>
        {
            /// <summary>
            /// Compares two nodes's bounds on the Y axis.
            /// </summary>
            /// <param name="x">A node.</param>
            /// <param name="y">Another node.</param>
            /// <returns>A negative value if a is less than b; 0 if they are equal; a positive value of a is greater than b.</returns>
            public int Compare(BVItem x, BVItem y)
            {
                if (x.BMin.Y < y.BMin.Y) return -1;
                if (x.BMin.Y > y.BMin.Y) return 1;
                if (x.BMax.Y < y.BMax.Y) return -1;
                if (x.BMax.Y > y.BMax.Y) return 1;
                if (x.I < y.I) return -1;
                if (x.I > y.I) return 1;
                return 0;
            }
        }
        /// <summary>
        /// An <see cref="IComparer{T}"/> implementation that only compares two <see cref="BoundingVolumeTreeNode"/>s on the Z axis.
        /// </summary>
        public class CompareZ : IComparer<BVItem>
        {
            /// <summary>
            /// Compares two nodes's bounds on the Z axis.
            /// </summary>
            /// <param name="x">A node.</param>
            /// <param name="y">Another node.</param>
            /// <returns>A negative value if a is less than b; 0 if they are equal; a positive value of a is greater than b.</returns>
            public int Compare(BVItem x, BVItem y)
            {
                if (x.BMin.Z < y.BMin.Z) return -1;
                if (x.BMin.Z > y.BMin.Z) return 1;
                if (x.BMax.Z < y.BMax.Z) return -1;
                if (x.BMax.Z > y.BMax.Z) return 1;
                if (x.I < y.I) return -1;
                if (x.I > y.I) return 1;
                return 0;
            }
        }
    }
}
