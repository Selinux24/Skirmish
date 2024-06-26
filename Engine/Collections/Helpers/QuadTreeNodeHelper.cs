using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections.Helpers
{
    /// <summary>
    /// Quad-tree node helper
    /// </summary>
    /// <typeparam name="T">Node type</typeparam>
    static class QuadTreeNodeHelper<T> where T : class, IQuadTreeNode<T>
    {
        /// <summary>
        /// Searches for the neighbor node at top position (from above)
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        public static T FindNeighborNodeAtTop(T node)
        {
            var parent = node?.Parent;

            if (parent == null)
            {
                return null;
            }

            if (node == parent.TopLeftChild)
            {
                return FindNeighborNodeAtTop(parent)?.BottomLeftChild;
            }
            else if (node == parent.TopRightChild)
            {
                return FindNeighborNodeAtTop(parent)?.BottomRightChild;
            }
            else if (node == parent.BottomLeftChild)
            {
                return parent.TopLeftChild;
            }
            else if (node == parent.BottomRightChild)
            {
                return parent.TopRightChild;
            }

            return null;
        }
        /// <summary>
        /// Searches for the neighbor node at bottom position (from above)
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the neighbor node at bottom position if exists.</returns>
        public static T FindNeighborNodeAtBottom(T node)
        {
            var parent = node?.Parent;

            if (parent == null)
            {
                return null;
            }

            if (node == parent.TopLeftChild)
            {
                return parent.BottomLeftChild;
            }
            else if (node == parent.TopRightChild)
            {
                return parent.BottomRightChild;
            }
            else if (node == parent.BottomLeftChild)
            {
                return FindNeighborNodeAtBottom(parent)?.TopLeftChild;
            }
            else if (node == parent.BottomRightChild)
            {
                return FindNeighborNodeAtBottom(parent)?.TopRightChild;
            }

            return null;
        }
        /// <summary>
        /// Searches for the neighbor node at right position(from above)
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the neighbor node at top position if exists.</returns>
        public static T FindNeighborNodeAtRight(T node)
        {
            var parent = node?.Parent;

            if (parent == null)
            {
                return null;
            }

            if (node == parent.TopLeftChild)
            {
                return parent.TopRightChild;
            }
            else if (node == parent.TopRightChild)
            {
                return FindNeighborNodeAtRight(parent)?.TopLeftChild;
            }
            else if (node == parent.BottomLeftChild)
            {
                return parent.BottomRightChild;
            }
            else if (node == parent.BottomRightChild)
            {
                return FindNeighborNodeAtRight(parent)?.BottomLeftChild;
            }

            return null;
        }
        /// <summary>
        /// Searches for the neighbor node at left position (from above)
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the neighbor node at left position if exists.</returns>
        public static T FindNeighborNodeAtLeft(T node)
        {
            var parent = node?.Parent;

            if (parent == null)
            {
                return null;
            }

            if (node == parent.TopLeftChild)
            {
                return FindNeighborNodeAtLeft(parent)?.TopRightChild;
            }
            else if (node == parent.TopRightChild)
            {
                return parent.TopLeftChild;
            }
            else if (node == parent.BottomLeftChild)
            {
                return FindNeighborNodeAtLeft(parent)?.BottomRightChild;
            }
            else if (node == parent.BottomRightChild)
            {
                return parent.BottomLeftChild;
            }

            return null;
        }

        /// <summary>
        /// Get bounding boxes of specified level
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="maxDepth">Maximum depth (if zero there is no limit)</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        public static IEnumerable<BoundingBox> GetBoundingBoxes(T node, int maxDepth = 0)
        {
            if (node == null)
            {
                return [];
            }

            if (maxDepth < 0)
            {
                return [];
            }

            if (node.IsLeaf)
            {
                return [node.BoundingBox];
            }

            if (maxDepth > 0 && node.Level == maxDepth)
            {
                return [node.BoundingBox];
            }

            return node.Children.SelectMany(c => GetBoundingBoxes(c, maxDepth)).ToArray();
        }
        /// <summary>
        /// Gets all leaf nodes
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns all leaf nodes</returns>
        public static IEnumerable<T> GetLeafNodes(T node)
        {
            if (node == null)
            {
                return [];
            }

            if (node.IsLeaf)
            {
                return [node];
            }

            return node.Children.SelectMany(c => GetLeafNodes(c));
        }
        /// <summary>
        /// Gets maximum level value
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns></returns>
        public static int GetMaxLevel(T node)
        {
            if (node == null)
            {
                return -1;
            }

            if (node.IsLeaf)
            {
                return node.Level;
            }

            int level = 0;

            foreach (var child in node.Children)
            {
                int cLevel = GetMaxLevel(child);

                if (cLevel > level) level = cLevel;
            }

            return level;
        }

        /// <summary>
        /// Gets node at position
        /// </summary>
        /// <param name="node">Node to test</param>
        /// <param name="position">Position</param>
        /// <returns>Returns the leaf node which contains the specified position</returns>
        public static T GetNodeAtPosition(T node, Vector3 position)
        {
            if (node == null)
            {
                return null;
            }

            if (node.BoundingBox.Contains(position) == ContainmentType.Disjoint)
            {
                // Early exit
                return null;
            }

            if (node.IsLeaf)
            {
                // Leaf node
                return node;
            }

            foreach (var child in node.Children)
            {
                var nIn = GetNodeAtPosition(child, position);
                if (nIn != null)
                {
                    return nIn;
                }
            }

            return null;
        }
        /// <summary>
        /// Gets the closest node to the specified position
        /// </summary>
        /// <param name="node">Node to test</param>
        /// <param name="position">Position</param>
        /// <returns>Returns the closest leaf node to the specified position</returns>
        public static T GetClosestNodeAtPosition(T node, Vector3 position)
        {
            if (node == null)
            {
                return null;
            }

            T n = null;
            float dist = float.MaxValue;
            foreach (var child in node.Children)
            {
                float d = Vector3.DistanceSquared(position, child.Center);
                if (d < dist)
                {
                    dist = d;
                    n = child;
                }
            }

            if (!n.IsLeaf)
            {
                return GetClosestNodeAtPosition(n, position);
            }

            return n;
        }

        /// <summary>
        /// Gets the leaf nodes contained into the specified bounding volume
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="volume">Bounding volume</param>
        /// <returns>Returns the leaf nodes contained into the bounding volume</returns>
        public static IEnumerable<T> GetNodesInVolume(T node, ICullingVolume volume)
        {
            if (node == null)
            {
                yield break;
            }

            if (volume == null)
            {
                yield break;
            }

            if (volume.Contains(node.BoundingBox) == ContainmentType.Disjoint)
            {
                // Early exit
                yield break;
            }

            if (node.IsLeaf)
            {
                yield return node;
            }

            foreach (var child in node.Children)
            {
                var nodesIn = GetNodesInVolume(child, volume);
                foreach (var nIn in nodesIn)
                {
                    yield return nIn;
                }
            }
        }
    }
}
