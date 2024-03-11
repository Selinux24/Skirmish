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
        /// Subdivision data
        /// </summary>
        class SubdivideData
        {
            /// <summary>
            /// X comparer
            /// </summary>
            public static readonly BoundsItemComparerX XComparer = new();
            /// <summary>
            /// Y comparer
            /// </summary>
            public static readonly BoundsItemComparerY YComparer = new();

            /// <summary>
            /// Bound items
            /// </summary>
            public BoundsItem[] Items { get; set; }
            /// <summary>
            /// Max nodes
            /// </summary>
            public int MaxNodes { get; set; }
            /// <summary>
            /// Node list
            /// </summary>
            public List<ChunkyTriMeshNode> Nodes { get; set; } = [];
            /// <summary>
            /// Indexed triangle list
            /// </summary>
            public List<int> OutTris { get; set; } = [];
            /// <summary>
            /// Tirangles per chunk
            /// </summary>
            public int TrisPerChunk { get; set; }
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
                if (x.Bounds.Left < y.Bounds.Left) return -1;
                if (x.Bounds.Left > y.Bounds.Left) return 1;
                if (x.Bounds.Right < y.Bounds.Right) return -1;
                if (x.Bounds.Right > y.Bounds.Right) return 1;
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
                if (x.Bounds.Top < y.Bounds.Top) return -1;
                if (x.Bounds.Top > y.Bounds.Top) return 1;
                if (x.Bounds.Bottom < y.Bounds.Bottom) return -1;
                if (x.Bounds.Bottom > y.Bounds.Bottom) return 1;
                if (x.Index < y.Index) return -1;
                if (x.Index > y.Index) return 1;
                return 0;
            }
        }
        /// <summary>
        /// Axis
        /// </summary>
        enum Axis
        {
            /// <summary>
            /// X Axis
            /// </summary>
            X,
            /// <summary>
            /// Y Axis
            /// </summary>
            Y,
        }

        /// <summary>
        /// Creates a new chunky mesh
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <param name="trisPerChunk">Triangles per chunk</param>
        /// <returns>The resulting chunky mesh</returns>
        public static ChunkyTriMesh Build(IEnumerable<Triangle> triangles, int trisPerChunk = 256)
        {
            if (triangles?.Any() != true)
            {
                return null;
            }

            // Build tree
            var items = new List<BoundsItem>(triangles.Count());

            foreach (var t in triangles)
            {
                var bbox = SharpDXExtensions.BoundingBoxFromPoints(t.GetVertices().ToArray());

                items.Add(new BoundsItem
                {
                    Index = items.Count,
                    Bounds = bbox.GetRectangleXZ(),
                });
            }

            int maxNodes = (triangles.Count() + trisPerChunk - 1) / trisPerChunk * 4;

            var data = new SubdivideData
            {
                MaxNodes = maxNodes,
                TrisPerChunk = trisPerChunk,
                Items = [.. items],
            };

            Subdivide(0, items.Count, data);

            return new ChunkyTriMesh
            {
                triangles = triangles.ToArray(),
                nodes = [.. data.Nodes],
                triangleIndices = [.. data.OutTris]
            };
        }
        /// <summary>
        /// Subdivide items
        /// </summary>
        /// <param name="imin">Minimum index</param>
        /// <param name="imax">Maximum index</param>
        /// <param name="data">Mesh data</param>
        private static void Subdivide(int imin, int imax, SubdivideData data)
        {
            int inum = imax - imin;
            int icur = data.Nodes.Count;

            if (data.Nodes.Count > data.MaxNodes)
            {
                return;
            }

            var bounds = CalcExtends(data.Items, imin, imax);

            var node = new ChunkyTriMeshNode
            {
                Bounds = bounds,
            };

            data.Nodes.Add(node);

            if (inum <= data.TrisPerChunk)
            {
                // Leaf

                // Copy triangles.
                node.Index = data.OutTris.Count;
                node.Count = inum;

                for (int i = imin; i < imax; ++i)
                {
                    data.OutTris.Add(data.Items[i].Index);
                }
            }
            else
            {
                // Split
                Axis axis = LongestAxis(bounds);
                if (axis == Axis.X)
                {
                    // Sort along x-axis
                    Array.Sort(data.Items, imin, inum, SubdivideData.XComparer);
                }
                else if (axis == Axis.Y)
                {
                    // Sort along y-axis
                    Array.Sort(data.Items, imin, inum, SubdivideData.YComparer);
                }

                int isplit = imin + inum / 2;

                // Left
                Subdivide(imin, isplit, data);

                // Right
                Subdivide(isplit, imax, data);

                int iescape = data.Nodes.Count - icur;

                // Negative index means escape.
                node.Index = -iescape;
            }
        }
        /// <summary>
        /// Gets the longest axis
        /// </summary>
        /// <param name="bounds">Bounds</param>
        /// <returns>Returns the longest axis</returns>
        private static Axis LongestAxis(RectangleF bounds)
        {
            Vector2 bmin = bounds.TopLeft;
            Vector2 bmax = bounds.BottomRight;

            float x = bmax.X - bmin.X;
            float y = bmax.Y - bmin.Y;

            return y > x ? Axis.Y : Axis.X;
        }
        /// <summary>
        /// Gets the extents of the specified item list
        /// </summary>
        /// <param name="items">Item list</param>
        /// <param name="imin">Minimum index</param>
        /// <param name="imax">Maximum index</param>
        /// <returns>Returns the items bounds</returns>
        private static RectangleF CalcExtends(BoundsItem[] items, int imin, int imax)
        {
            var bounds = items[imin].Bounds;

            for (int i = imin + 1; i < imax; ++i)
            {
                bounds = RectangleF.Union(bounds, items[i].Bounds);
            }

            return bounds;
        }

        /// <summary>
        /// Triangle list
        /// </summary>
        private IEnumerable<Triangle> triangles;
        /// <summary>
        /// Chunk triangle indices
        /// </summary>
        private IEnumerable<int> triangleIndices;
        /// <summary>
        /// Chunk nodes
        /// </summary>
        private ChunkyTriMeshNode[] nodes;

        /// <summary>
        /// Gets all the triangles in the mesh
        /// </summary>
        /// <returns>Returns the triangle list</returns>
        public Triangle[] GetTriangles()
        {
            return triangles?.ToArray() ?? [];
        }
        /// <summary>
        /// Gets the triangles in the specified node
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Returns the triangles in the specified node</returns>
        public Triangle[] GetTriangles(int index)
        {
            return GetTriangles(nodes[index]);
        }
        /// <summary>
        /// Gets the triangles in the specified node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the triangles in the specified node</returns>
        public Triangle[] GetTriangles(ChunkyTriMeshNode node)
        {
            if (node.Index < 0)
            {
                return [];
            }

            var tris = GetTriangles();
            if (tris.Length == 0)
            {
                return [];
            }

            var indices = triangleIndices.Skip(node.Index).Take(node.Count);

            var res = new List<Triangle>();

            foreach (var index in indices)
            {
                res.Add(tris[index]);
            }

            return [.. res];
        }

        /// <summary>
        /// Gets the chunks overlapping the specified rectangle
        /// </summary>
        /// <param name="bmin">Bounding box minimum</param>
        /// <param name="bmax">Bounding box maximum</param>
        /// <returns>Returns a list of indices</returns>
        public int[] GetChunksOverlappingRect(Vector2 bmin, Vector2 bmax)
        {
            var ids = new List<int>();

            // Traverse tree
            int i = 0;
            while (i < nodes.Length)
            {
                var node = nodes[i];
                bool overlap = Utils.OverlapRect(bmin, bmax, node.Bounds);
                bool isLeafNode = node.Index >= 0;

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
                    int escapeIndex = -node.Index;
                    i += escapeIndex;
                }
            }

            return [.. ids];
        }
    }
}
