using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    /// <summary>
    /// Chunky triangle mesh
    /// </summary>
    public class ChunkyTriMesh
    {
        /// <summary>
        /// Checks wether the extents overlaps
        /// </summary>
        /// <param name="amin">A minimum rectangle point</param>
        /// <param name="amax">A maximum rectangle point</param>
        /// <param name="bmin">B minimum rectangle point</param>
        /// <param name="bmax">B maximum rectangle point</param>
        /// <returns>Returns true if rectangles overlap</returns>
        private static bool CheckOverlapRect(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax)
        {
            bool overlap =
                !(amin.X > bmax.X || amax.X < bmin.X) &&
                !(amin.Y > bmax.Y || amax.Y < bmin.Y);

            return overlap;
        }

        /// <summary>
        /// X comparer
        /// </summary>
        private static readonly BoundsItemComparerX xComparer = new BoundsItemComparerX();
        /// <summary>
        /// Y comparer
        /// </summary>
        private static readonly BoundsItemComparerY yComparer = new BoundsItemComparerY();

        /// <summary>
        /// Triangle list
        /// </summary>
        public Triangle[] Triangles { get; set; }
        /// <summary>
        /// Maximum number of triangles per chunk
        /// </summary>
        public int MaxTrisPerChunk { get; set; }
        /// <summary>
        /// Chunk nodes
        /// </summary>
        public ChunkyTriMeshNode[] Nodes { get; set; }
        /// <summary>
        /// Node count
        /// </summary>
        public int NNodes { get; set; }
        /// <summary>
        /// Chunk triangle indices
        /// </summary>
        public int[] Tris { get; set; }
        /// <summary>
        /// Triangle index count
        /// </summary>
        public int NTris { get; set; }

        /// <summary>
        /// Builds a chunky mesh
        /// </summary>
        /// <param name="input">Geometry input</param>
        /// <returns>Returns the new chunky mesh</returns>
        public static ChunkyTriMesh Build(InputGeometry input)
        {
            var triangles = input.GetTriangles();
            if (triangles?.Length > 0 && CreateChunkyTriMesh(triangles, 256, out ChunkyTriMesh chunkyMesh))
            {
                return chunkyMesh;
            }

            return null;
        }

        /// <summary>
        /// Creates a new chunky mesh
        /// </summary>
        /// <param name="tris">Triangles</param>
        /// <param name="trisPerChunk">Triangles per chunk</param>
        /// <param name="cm">The resulting chunky mesh</param>
        /// <returns>Returns true if the chunky mesh were correctly created</returns>
        private static bool CreateChunkyTriMesh(Triangle[] tris, int trisPerChunk, out ChunkyTriMesh cm)
        {
            int ntris = tris.Count();
            int nchunks = (ntris + trisPerChunk - 1) / trisPerChunk;

            cm = new ChunkyTriMesh
            {
                Triangles = tris,
                Nodes = new ChunkyTriMeshNode[nchunks * 4],
                Tris = new int[ntris],
                NTris = ntris,
            };

            // Build tree
            BoundsItem[] items = new BoundsItem[ntris];

            for (int i = 0; i < ntris; i++)
            {
                var t = tris[i];

                // Calc triangle XZ bounds.
                var bbox = BoundingBox.FromPoints(t.GetVertices());

                items[i].i = i;
                items[i].bmin = new Vector2(bbox.Minimum.X, bbox.Minimum.Z);
                items[i].bmax = new Vector2(bbox.Maximum.X, bbox.Maximum.Z);
            }

            int curNode = 0;
            int curTri = 0;
            Subdivide(
                items,
                0, ntris, trisPerChunk,
                ref curNode, cm.Nodes, nchunks * 4,
                ref curTri, cm.Tris, tris);

            cm.NNodes = curNode;

            // Calc max tris per node.
            cm.MaxTrisPerChunk = 0;
            for (int i = 0; i < cm.NNodes; ++i)
            {
                var node = cm.Nodes[i];

                bool isLeaf = node.i >= 0;
                if (!isLeaf) continue;
                if (node.n > cm.MaxTrisPerChunk)
                {
                    cm.MaxTrisPerChunk = node.n;
                }
            }

            return true;
        }
        /// <summary>
        /// Subdivide items
        /// </summary>
        /// <param name="items">Item list</param>
        /// <param name="imin">Minimum index</param>
        /// <param name="imax">Maximum index</param>
        /// <param name="trisPerChunk">Triangles per chunk</param>
        /// <param name="curNode">Current node</param>
        /// <param name="nodes">Node list</param>
        /// <param name="maxNodes">Maximum node count</param>
        /// <param name="curTri">Current triangle index</param>
        /// <param name="outTris">Resulting triangle index list</param>
        /// <param name="inTris">Resulting triangle list</param>
        private static void Subdivide(
            BoundsItem[] items, int imin, int imax, int trisPerChunk,
            ref int curNode, ChunkyTriMeshNode[] nodes, int maxNodes,
            ref int curTri, int[] outTris, Triangle[] inTris)
        {
            int inum = imax - imin;
            int icur = curNode;

            if (curNode > maxNodes)
            {
                return;
            }

            int iNode = curNode++;

            CalcExtends(items, imin, imax, out Vector2 bmin, out Vector2 bmax);

            nodes[iNode].bmin = bmin;
            nodes[iNode].bmax = bmax;

            if (inum <= trisPerChunk)
            {
                // Leaf

                // Copy triangles.
                nodes[iNode].i = curTri;
                nodes[iNode].n = inum;

                for (int i = imin; i < imax; ++i)
                {
                    outTris[curTri] = items[i].i;
                    curTri++;
                }
            }
            else
            {
                // Split

                int axis = LongestAxis(bmax.X - bmin.X, bmax.Y - bmin.Y);
                if (axis == 0)
                {
                    // Sort along x-axis
                    Array.Sort(items, imin, inum, xComparer);
                }
                else if (axis == 1)
                {
                    // Sort along y-axis
                    Array.Sort(items, imin, inum, yComparer);
                }

                int isplit = imin + inum / 2;

                // Left
                Subdivide(
                    items, imin, isplit, trisPerChunk,
                    ref curNode, nodes, maxNodes,
                    ref curTri, outTris, inTris);

                // Right
                Subdivide(
                    items, isplit, imax, trisPerChunk,
                    ref curNode, nodes, maxNodes,
                    ref curTri, outTris, inTris);

                int iescape = curNode - icur;

                // Negative index means escape.
                nodes[iNode].i = -iescape;
            }
        }
        /// <summary>
        /// Gets the longest axis
        /// </summary>
        /// <param name="x">X magnitude</param>
        /// <param name="y">Y magnitude</param>
        /// <returns>Returns 0 if X is the longest axis, 1 otherwise</returns>
        private static int LongestAxis(float x, float y)
        {
            return y > x ? 1 : 0;
        }
        /// <summary>
        /// Gets the extents of the specified item list
        /// </summary>
        /// <param name="items">Item list</param>
        /// <param name="imin">Minimum index</param>
        /// <param name="imax">Maximum index</param>
        /// <param name="bmin">Resulting minimum extent</param>
        /// <param name="bmax">Resulting maxumum extent</param>
        private static void CalcExtends(BoundsItem[] items, int imin, int imax, out Vector2 bmin, out Vector2 bmax)
        {
            bmin.X = items[imin].bmin.X;
            bmin.Y = items[imin].bmin.Y;

            bmax.X = items[imin].bmax.X;
            bmax.Y = items[imin].bmax.Y;

            for (int i = imin + 1; i < imax; ++i)
            {
                if (items[i].bmin.X < bmin.X) bmin.X = items[i].bmin.X;
                if (items[i].bmin.Y < bmin.Y) bmin.Y = items[i].bmin.Y;

                if (items[i].bmax.X > bmax.X) bmax.X = items[i].bmax.X;
                if (items[i].bmax.Y > bmax.Y) bmax.Y = items[i].bmax.Y;
            }
        }

        /// <summary>
        /// Gets the triangles in the specified node
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Returns the triangles in the specified node</returns>
        public Triangle[] GetTriangles(int index)
        {
            return GetTriangles(this.Nodes[index]);
        }
        /// <summary>
        /// Gets the triangles in the specified node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the triangles in the specified node</returns>
        public Triangle[] GetTriangles(ChunkyTriMeshNode node)
        {
            if (node.i >= 0)
            {
                int[] indices = new int[node.n];
                Array.Copy(Tris.ToArray(), node.i, indices, 0, node.n);

                Triangle[] res = new Triangle[node.n];
                for (int i = 0; i < node.n; i++)
                {
                    res[i] = Triangles[indices[i]];
                }
                return res;
            }

            return new Triangle[] { };
        }

        /// <summary>
        /// Gets the chunks overlapping the specified rectangle
        /// </summary>
        /// <param name="bmin">Bounding box minimum</param>
        /// <param name="bmax">Bounding box maximum</param>
        /// <returns>Returns a list of indices</returns>
        public IEnumerable<int> GetChunksOverlappingRect(Vector2 bmin, Vector2 bmax)
        {
            List<int> ids = new List<int>();

            // Traverse tree
            int i = 0;
            while (i < NNodes)
            {
                var node = Nodes[i];
                bool overlap = CheckOverlapRect(bmin, bmax, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    ids.Add(i);
                }

                if (overlap || isLeafNode)
                {
                    i++;
                }
                else
                {
                    int escapeIndex = -node.i;
                    i += escapeIndex;
                }
            }

            return ids;
        }
    }
}
