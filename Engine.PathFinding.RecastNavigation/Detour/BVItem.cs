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
        #region Helpers

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

        public const int MESH_NULL_IDX = -1;

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

        public static int CreateBVTree(NavMeshCreateParams param, out List<BVNode> nodes)
        {
            nodes = new List<BVNode>();

            // Build tree
            float quantFactor = 1 / param.CellSize;
            BVItem[] items = new BVItem[param.PolyCount];
            for (int i = 0; i < param.PolyCount; i++)
            {
                var it = items[i];
                it.I = i;
                // Calc polygon bounds. Use detail meshes if available.
                if (param.DetailMeshes != null)
                {
                    it.CalcDetailBounds(param.DetailMeshes[i], param.DetailVerts, param.BMin, quantFactor);
                }
                else
                {
                    it.CalcPolygonBounds(param.Polys[i], param.Nvp, param.Verts, param.CellSize, param.CellHeight);
                }
                items[i] = it;
            }

            int curNode = 0;
            Subdivide(items, param.PolyCount, 0, param.PolyCount, ref curNode, ref nodes);

            return curNode;
        }
        private static void Subdivide(BVItem[] items, int nitems, int imin, int imax, ref int curNode, ref List<BVNode> nodes)
        {
            int inum = imax - imin;
            int icur = curNode;

            var node = new BVNode();
            nodes.Add(node);
            curNode++;

            if (inum == 1)
            {
                // Leaf
                node.BMin = items[imin].BMin;
                node.BMax = items[imin].BMax;
                node.I = items[imin].I;
            }
            else
            {
                // Split
                CalcExtends(items, imin, imax, out var bmin, out var bmax);
                node.BMin = bmin;
                node.BMax = bmax;

                int axis = LongestAxis(
                    node.BMax.X - node.BMin.X,
                    node.BMax.Y - node.BMin.Y,
                    node.BMax.Z - node.BMin.Z);

                if (axis == 0)
                {
                    // Sort along x-axis
                    Array.Sort(items, imin, inum, XComparer);
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort(items, imin, inum, YComparer);
                }
                else
                {
                    // Sort along z-axis
                    Array.Sort(items, imin, inum, ZComparer);
                }

                int isplit = imin + inum / 2;

                // Left
                Subdivide(items, nitems, imin, isplit, ref curNode, ref nodes);
                // Right
                Subdivide(items, nitems, isplit, imax, ref curNode, ref nodes);

                int iescape = curNode - icur;
                // Negative index means escape.
                node.I = -iescape;
            }
        }
        private static int LongestAxis(int x, int y, int z)
        {
            int axis = 0;
            int maxVal = x;
            if (y > maxVal)
            {
                axis = 1;
                maxVal = y;
            }
            if (z > maxVal)
            {
                axis = 2;
            }
            return axis;
        }
        public static void CalcExtends(BVItem[] items, int imin, int imax, out Int3 bmin, out Int3 bmax)
        {
            bmin.X = items[imin].BMin.X;
            bmin.Y = items[imin].BMin.Y;
            bmin.Z = items[imin].BMin.Z;

            bmax.X = items[imin].BMax.X;
            bmax.Y = items[imin].BMax.Y;
            bmax.Z = items[imin].BMax.Z;

            for (int i = imin + 1; i < imax; ++i)
            {
                var it = items[i];
                if (it.BMin.X < bmin.X) bmin.X = it.BMin.X;
                if (it.BMin.Y < bmin.Y) bmin.Y = it.BMin.Y;
                if (it.BMin.Z < bmin.Z) bmin.Z = it.BMin.Z;

                if (it.BMax.X > bmax.X) bmax.X = it.BMax.X;
                if (it.BMax.Y > bmax.Y) bmax.Y = it.BMax.Y;
                if (it.BMax.Z > bmax.Z) bmax.Z = it.BMax.Z;
            }
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
