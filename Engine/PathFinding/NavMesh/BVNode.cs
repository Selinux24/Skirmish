using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// The data stored in a bounding volume node.
    /// </summary>
    public struct BVNode
    {
        /// <summary>
        /// The bounding box of the node.
        /// </summary>
        public BoundingBoxi Bounds;
        /// <summary>
        /// The index of this node in a <see cref="BVTree"/>.
        /// </summary>
        public int Index;

        /// <summary>
        /// An <see cref="IComparer{T}"/> implementation that only compares two <see cref="BVNode"/>s on the X axis.
        /// </summary>
        public class CompareX : IComparer<BVNode>
        {
            /// <summary>
            /// Compares two nodes's bounds on the X axis.
            /// </summary>
            /// <param name="x">A node.</param>
            /// <param name="y">Another node.</param>
            /// <returns>A negative value if a is less than b; 0 if they are equal; a positive value of a is greater than b.</returns>
            public int Compare(BVNode x, BVNode y)
            {
                if (x.Bounds.Min.X < y.Bounds.Min.X)
                    return -1;

                if (x.Bounds.Min.X > y.Bounds.Min.X)
                    return 1;

                return 0;
            }
        }
        /// <summary>
        /// An <see cref="IComparer{T}"/> implementation that only compares two <see cref="BVNode"/>s on the Y axis.
        /// </summary>
        public class CompareY : IComparer<BVNode>
        {
            /// <summary>
            /// Compares two nodes's bounds on the Y axis.
            /// </summary>
            /// <param name="x">A node.</param>
            /// <param name="y">Another node.</param>
            /// <returns>A negative value if a is less than b; 0 if they are equal; a positive value of a is greater than b.</returns>
            public int Compare(BVNode x, BVNode y)
            {
                if (x.Bounds.Min.Y < y.Bounds.Min.Y)
                    return -1;

                if (x.Bounds.Min.Y > y.Bounds.Min.Y)
                    return 1;

                return 0;
            }
        }
        /// <summary>
        /// An <see cref="IComparer{T}"/> implementation that only compares two <see cref="BVNode"/>s on the Z axis.
        /// </summary>
        public class CompareZ : IComparer<BVNode>
        {
            /// <summary>
            /// Compares two nodes's bounds on the Z axis.
            /// </summary>
            /// <param name="x">A node.</param>
            /// <param name="y">Another node.</param>
            /// <returns>A negative value if a is less than b; 0 if they are equal; a positive value of a is greater than b.</returns>
            public int Compare(BVNode x, BVNode y)
            {
                if (x.Bounds.Min.Z < y.Bounds.Min.Z)
                    return -1;

                if (x.Bounds.Min.Z > y.Bounds.Min.Z)
                    return 1;

                return 0;
            }
        }
    }
}
