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
        #region Helper classes

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

        #endregion

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

        /// <summary>
        /// Calculates detail bounds
        /// </summary>
        /// <param name="dm">Detail mesh</param>
        /// <param name="detailVerts">Detail vertices</param>
        public void CalcDetailBounds(PolyMeshIndices dm, Vector3[] detailVerts, Vector3 bMin, float quantFactor)
        {
            int vb = dm.VertBase;
            int ndv = dm.VertCount;
            var bbox = Utils.GetMinMaxBounds(detailVerts, vb, ndv);

            // BV-tree uses cs for all dimensions
            BMin = new Int3
            {
                X = MathUtil.Clamp((int)((bbox.Minimum.X - bMin.X) * quantFactor), 0, int.MaxValue),
                Y = MathUtil.Clamp((int)((bbox.Minimum.Y - bMin.Y) * quantFactor), 0, int.MaxValue),
                Z = MathUtil.Clamp((int)((bbox.Minimum.Z - bMin.Z) * quantFactor), 0, int.MaxValue)
            };

            BMax = new Int3
            {
                X = MathUtil.Clamp((int)((bbox.Maximum.X - bMin.X) * quantFactor), 0, int.MaxValue),
                Y = MathUtil.Clamp((int)((bbox.Maximum.Y - bMin.Y) * quantFactor), 0, int.MaxValue),
                Z = MathUtil.Clamp((int)((bbox.Maximum.Z - bMin.Z) * quantFactor), 0, int.MaxValue)
            };
        }
        /// <summary>
        /// Calculates polygon bounds
        /// </summary>
        /// <param name="p">Indexed polygon</param>
        /// <param name="nvp">Number of vertices</param>
        /// <param name="verts">Vertex list</param>
        /// <param name="ch">Cell height</param>
        /// <param name="cs">Cell size</param>
        public void CalcPolygonBounds(IndexedPolygon p, int nvp, Int3[] verts, float ch, float cs)
        {
            int p0 = p.GetVertex(0);
            var v0 = verts[p0];
            var itBMin = v0;
            var itBMax = v0;

            for (int j = 1; j < nvp; ++j)
            {
                if (p.VertexIsNull(j))
                {
                    break;
                }

                int pj = p.GetVertex(j);

                var vj = verts[pj];
                var x = vj.X;
                var y = vj.Y;
                var z = vj.Z;

                if (x < BMin.X) itBMin.X = x;
                if (y < BMin.Y) itBMin.Y = y;
                if (z < BMin.Z) itBMin.Z = z;

                if (x > BMax.X) itBMax.X = x;
                if (y > BMax.Y) itBMax.Y = y;
                if (z > BMax.Z) itBMax.Z = z;
            }

            // Remap y
            itBMin.Y = (int)MathF.Floor(BMin.Y * ch / cs);
            itBMax.Y = (int)MathF.Ceiling(BMax.Y * ch / cs);

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
