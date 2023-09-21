using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Detour
{
    /// <summary>
    /// Bounding volume node.
    /// </summary>
    [Serializable]
    public class BVNode
    {
        /// <summary>
        /// Minimum bounds of the node's AABB. [(x, y, z)]
        /// </summary>
        public Int3 BMin { get; set; }
        /// <summary>
        /// Maximum bounds of the node's AABB. [(x, y, z)]
        /// </summary>
        public Int3 BMax { get; set; }
        /// <summary>
        /// The node's index. (Negative for escape sequence.)
        /// </summary>
        public int I { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BVNode()
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
                    Array.Sort(items, imin, inum, BVItem.XComparer);
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort(items, imin, inum, BVItem.YComparer);
                }
                else
                {
                    // Sort along z-axis
                    Array.Sort(items, imin, inum, BVItem.ZComparer);
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
        private static void CalcExtends(BVItem[] items, int imin, int imax, out Int3 bmin, out Int3 bmax)
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(BVNode)} Region Id: {I}; BMin: {BMin}; BMax: {BMax};";
        }
    }
}
