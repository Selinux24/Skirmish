using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh2
{
    public struct BVItem
    {
        public static readonly CompareX XComparer = new CompareX();
        public static readonly CompareY YComparer = new CompareY();
        public static readonly CompareZ ZComparer = new CompareZ();

        public Int3 bmin;
        public Int3 bmax;
        public int i;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Region Id: {0}; BMin: {1}; BMax: {2};", this.i, this.bmin, this.bmax);
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
                if (x.bmin.Y < y.bmin.Y) return -1;
                if (x.bmin.Y > y.bmin.Y) return 1;
                if (x.bmax.Y < y.bmax.Y) return -1;
                if (x.bmax.Y > y.bmax.Y) return 1;
                if (x.i < y.i) return -1;
                if (x.i > y.i) return 1;
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
                if (x.bmin.Z < y.bmin.Z) return -1;
                if (x.bmin.Z > y.bmin.Z) return 1;
                if (x.bmax.Z < y.bmax.Z) return -1;
                if (x.bmax.Z > y.bmax.Z) return 1;
                if (x.i < y.i) return -1;
                if (x.i > y.i) return 1;
                return 0;
            }
        }
    }
}
