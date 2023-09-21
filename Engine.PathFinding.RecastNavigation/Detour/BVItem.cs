using Engine.PathFinding.RecastNavigation.Recast;
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

        public const int MESH_NULL_IDX = -1;

        /// <summary>
        /// X axis comparer
        /// </summary>
        public static readonly CompareX XComparer = new();
        /// <summary>
        /// Y axis comparer
        /// </summary>
        public static readonly CompareY YComparer = new();
        /// <summary>
        /// Z axis comparer
        /// </summary>
        public static readonly CompareZ ZComparer = new();

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

        private static void GetMinMaxBounds(Vector3[] vectors, int vb, int ndv, out Vector3 bmin, out Vector3 bmax)
        {
            bmin = vectors[vb];
            bmax = vectors[vb];
            for (int j = 1; j < ndv; j++)
            {
                bmin = Vector3.Min(bmin, vectors[vb + j]);
                bmax = Vector3.Max(bmax, vectors[vb + j]);
            }
        }

        public void CalcDetailBounds(PolyMeshDetailIndices dm, Vector3[] detailVerts, Vector3 bMin, float quantFactor)
        {
            int vb = dm.VertBase;
            int ndv = dm.VertCount;
            GetMinMaxBounds(detailVerts, vb, ndv, out var bmin, out var bmax);

            // BV-tree uses cs for all dimensions
            BMin = new Int3
            {
                X = MathUtil.Clamp((int)((bmin.X - bMin.X) * quantFactor), 0, int.MaxValue),
                Y = MathUtil.Clamp((int)((bmin.Y - bMin.Y) * quantFactor), 0, int.MaxValue),
                Z = MathUtil.Clamp((int)((bmin.Z - bMin.Z) * quantFactor), 0, int.MaxValue)
            };

            BMax = new Int3
            {
                X = MathUtil.Clamp((int)((bmax.X - bMin.X) * quantFactor), 0, int.MaxValue),
                Y = MathUtil.Clamp((int)((bmax.Y - bMin.Y) * quantFactor), 0, int.MaxValue),
                Z = MathUtil.Clamp((int)((bmax.Z - bMin.Z) * quantFactor), 0, int.MaxValue)
            };
        }
        public void CalcPolygonBounds(IndexedPolygon p, int nvp, Int3[] verts, float ch, float cs)
        {
            var itBMin = verts[p[0]];
            var itBMax = verts[p[0]];

            for (int j = 1; j < nvp; ++j)
            {
                if (p[j] == MESH_NULL_IDX) break;
                var x = verts[p[j]].X;
                var y = verts[p[j]].Y;
                var z = verts[p[j]].Z;

                if (x < BMin.X) itBMin.X = x;
                if (y < BMin.Y) itBMin.Y = y;
                if (z < BMin.Z) itBMin.Z = z;

                if (x > BMax.X) itBMax.X = x;
                if (y > BMax.Y) itBMax.Y = y;
                if (z > BMax.Z) itBMax.Z = z;
            }
            // Remap y
            itBMin.Y = (int)Math.Floor(BMin.Y * ch / cs);
            itBMax.Y = (int)Math.Ceiling(BMax.Y * ch / cs);

            BMin = itBMin;
            BMax = itBMax;
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"{nameof(BVItem)} Region Id: {I}; BMin: {BMin}; BMax: {BMax};";
        }
    }
}
