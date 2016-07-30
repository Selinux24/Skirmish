using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A tree of bounding volumes.
    /// </summary>
    public class BoundingVolumeTree
    {
        private static readonly BoundingVolumeTreeNode.CompareX XComparer = new BoundingVolumeTreeNode.CompareX();
        private static readonly BoundingVolumeTreeNode.CompareY YComparer = new BoundingVolumeTreeNode.CompareY();
        private static readonly BoundingVolumeTreeNode.CompareZ ZComparer = new BoundingVolumeTreeNode.CompareZ();

        /// <summary>
        /// Nodes in the tree
        /// </summary>
        private BoundingVolumeTreeNode[] nodes;
        /// <summary>
        /// Gets the number of nodes in the tree.
        /// </summary>
        public int Count
        {
            get
            {
                return nodes.Length;
            }
        }
        /// <summary>
        /// Gets the node at a specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The node at the index.</returns>
        public BoundingVolumeTreeNode this[int index]
        {
            get
            {
                return nodes[index];
            }
        }

        /// <summary>
        /// Calculates the bounding box for a set of bounding boxes.
        /// </summary>
        /// <param name="items">The list of all the bounding boxes.</param>
        /// <param name="minIndex">The first bounding box in the list to get the extends of.</param>
        /// <param name="maxIndex">The last bounding box in the list to get the extends of.</param>
        /// <param name="bounds">The extends of all the bounding boxes.</param>
        private static void CalcExtends(List<BoundingVolumeTreeNode> items, int minIndex, int maxIndex, out BoundingBoxi bounds)
        {
            bounds = items[minIndex].Bounds;

            for (int i = minIndex + 1; i < maxIndex; i++)
            {
                BoundingVolumeTreeNode it = items[i];
                Vertexi.ComponentMin(ref it.Bounds.Min, ref bounds.Min, out bounds.Min);
                Vertexi.ComponentMax(ref it.Bounds.Max, ref bounds.Max, out bounds.Max);
            }
        }
        /// <summary>
        /// Determine whether the bounding x, y, or z axis contains the longest distance 
        /// </summary>
        /// <param name="x">Length of bounding x-axis</param>
        /// <param name="y">Length of bounding y-axis</param>
        /// <param name="z">Length of bounding z-axis</param>
        /// <returns>Returns the a specific axis (x, y, or z)</returns>
        private static int LongestAxis(int x, int y, int z)
        {
            int axis = 0;
            int max = x;

            if (y > max)
            {
                axis = 1;
                max = y;
            }

            if (z > max)
            {
                axis = 2;
            }

            return axis;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingVolumeTree"/> class.
        /// </summary>
        /// <param name="verts">A set of vertices.</param>
        /// <param name="polys">A set of polygons composed of the vertices in <c>verts</c>.</param>
        /// <param name="nvp">The maximum number of vertices per polygon.</param>
        /// <param name="cellSize">The size of a cell.</param>
        /// <param name="cellHeight">The height of a cell.</param>
        public BoundingVolumeTree(Vertexi[] verts, PolyMesh.Polygon[] polys, int nvp, float cellSize, float cellHeight)
        {
            this.nodes = new BoundingVolumeTreeNode[polys.Length * 2];

            var items = new List<BoundingVolumeTreeNode>();

            for (int i = 0; i < polys.Length; i++)
            {
                PolyMesh.Polygon p = polys[i];

                BoundingVolumeTreeNode temp;
                temp.Index = i;
                temp.Bounds.Min = temp.Bounds.Max = verts[p.Vertices[0]];

                for (int j = 1; j < nvp; j++)
                {
                    int vi = p.Vertices[j];
                    if (vi == PolyMesh.NullId)
                    {
                        break;
                    }

                    var v = verts[vi];
                    Vertexi.ComponentMin(ref temp.Bounds.Min, ref v, out temp.Bounds.Min);
                    Vertexi.ComponentMax(ref temp.Bounds.Max, ref v, out temp.Bounds.Max);
                }

                temp.Bounds.Min.Y = (int)Math.Floor((float)temp.Bounds.Min.Y * cellHeight / cellSize);
                temp.Bounds.Max.Y = (int)Math.Ceiling((float)temp.Bounds.Max.Y * cellHeight / cellSize);

                items.Add(temp);
            }

            this.Subdivide(items, 0, items.Count, 0);
        }

        /// <summary>
        /// Subdivides a list of bounding boxes until it is a tree.
        /// </summary>
        /// <param name="items">A list of bounding boxes.</param>
        /// <param name="minIndex">The first index to consider (recursively).</param>
        /// <param name="maxIndex">The last index to consier (recursively).</param>
        /// <param name="curNode">The current node to look at.</param>
        /// <returns>The current node at the end of each method.</returns>
        private int Subdivide(List<BoundingVolumeTreeNode> items, int minIndex, int maxIndex, int curNode)
        {
            int numIndex = maxIndex - minIndex;
            int curIndex = curNode;

            int oldNode = curNode;
            curNode++;

            //Check if the current node is a leaf node
            if (numIndex == 1)
            {
                this.nodes[oldNode] = items[minIndex];
            }
            else
            {
                BoundingBoxi bounds;
                CalcExtends(items, minIndex, maxIndex, out bounds);
                this.nodes[oldNode].Bounds = bounds;

                int axis = LongestAxis((int)(bounds.Max.X - bounds.Min.X), (int)(bounds.Max.Y - bounds.Min.Y), (int)(bounds.Max.Z - bounds.Min.Z));

                switch (axis)
                {
                    case 0:
                        items.Sort(minIndex, numIndex, XComparer);
                        break;
                    case 1:
                        items.Sort(minIndex, numIndex, YComparer);
                        break;
                    case 2:
                        items.Sort(minIndex, numIndex, ZComparer);
                        break;
                    default:
                        break;
                }

                int splitIndex = minIndex + (numIndex / 2);

                curNode = this.Subdivide(items, minIndex, splitIndex, curNode);
                curNode = this.Subdivide(items, splitIndex, maxIndex, curNode);

                int escapeIndex = curNode - curIndex;
                this.nodes[oldNode].Index = -escapeIndex;
            }

            return curNode;
        }
    }
}

