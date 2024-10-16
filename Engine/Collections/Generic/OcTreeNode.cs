﻿using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Collections.Generic
{
    /// <summary>
    /// OcTree node
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="boundary">Node boundary</param>
    /// <param name="itemsPerNode">Maximum items per node</param>
    /// <param name="root">Is root node</param>
    public class OcTreeNode<T>(BoundingBox boundary, int itemsPerNode, bool root)
    {
        /// <summary>
        /// Node boundary
        /// </summary>
        public BoundingBox Boundary { get; private set; } = boundary;
        /// <summary>
        /// Items per node
        /// </summary>
        private readonly int itemsPerNode = itemsPerNode;
        /// <summary>
        /// Is the root node
        /// </summary>
        private readonly bool root = root;
        /// <summary>
        /// Node items array
        /// </summary>
        private readonly (ICullingVolume volume, T item)[] items = root ? [] : new (ICullingVolume, T)[itemsPerNode];
        /// <summary>
        /// Number of stored items in the array
        /// </summary>
        private int storedItems = 0;

        /// <summary>
        /// Top left front node
        /// </summary>
        public OcTreeNode<T> TopLeftFront { get; private set; }
        /// <summary>
        /// Top left back node
        /// </summary>
        public OcTreeNode<T> TopLeftBack { get; private set; }
        /// <summary>
        /// Top right front node
        /// </summary>
        public OcTreeNode<T> TopRightFront { get; private set; }
        /// <summary>
        /// Top right back node
        /// </summary>
        public OcTreeNode<T> TopRightBack { get; private set; }
        /// <summary>
        /// Bottom left front node
        /// </summary>
        public OcTreeNode<T> BottomLeftFront { get; private set; }
        /// <summary>
        /// Bottom left back node
        /// </summary>
        public OcTreeNode<T> BottomLeftBack { get; private set; }
        /// <summary>
        /// Bottom right front node
        /// </summary>
        public OcTreeNode<T> BottomRightFront { get; private set; }
        /// <summary>
        /// Bottom right back node
        /// </summary>
        public OcTreeNode<T> BottomRightBack { get; private set; }

        /// <summary>
        /// Inserts a item in the node
        /// </summary>
        /// <param name="itemBoundary">Item boundary</param>
        /// <param name="item">Item</param>
        public void Insert(ICullingVolume itemBoundary, T item)
        {
            if (itemBoundary.Contains(Boundary) == ContainmentType.Disjoint)
            {
                return;
            }

            if (!root && storedItems < items.Length)
            {
                items[storedItems++] = (itemBoundary, item);

                return;
            }

            int index = -1;

            foreach (var childBoundary in Boundary.SubdivideOctree())
            {
                index++;

                var child = GetNode(index);
                if (child == null)
                {
                    if (itemBoundary.Contains(childBoundary) == ContainmentType.Disjoint)
                    {
                        continue;
                    }

                    child = new OcTreeNode<T>(childBoundary, itemsPerNode, false);

                    SetNode(index, child);
                }

                child.Insert(itemBoundary, item);
            }
        }
        /// <summary>
        /// Gets the node by index
        /// </summary>
        /// <param name="index">Node index</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception launched when the specified index is out of the 0..7 range.</exception>
        /// <remarks>
        /// 0 => TopLeftFront
        /// 1 => TopLeftBack
        /// 2 => TopRightFront
        /// 3 => TopRightBack
        /// 4 => BottomLeftFront
        /// 5 => BottomLeftBack
        /// 6 => BottomRightFront
        /// 7 => BottomRightBack
        /// </remarks>
        private OcTreeNode<T> GetNode(int index)
        {
            return index switch
            {
                0 => TopLeftFront,
                1 => TopLeftBack,
                2 => TopRightFront,
                3 => TopRightBack,
                4 => BottomLeftFront,
                5 => BottomLeftBack,
                6 => BottomRightFront,
                7 => BottomRightBack,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
        /// <summary>
        /// Sets the specified node in the specified index location
        /// </summary>
        /// <param name="index">Node index</param>
        /// <param name="node">Node</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception launched when the specified index is out of the 0..7 range.</exception>
        /// <remarks>
        /// 0 => TopLeftFront
        /// 1 => TopLeftBack
        /// 2 => TopRightFront
        /// 3 => TopRightBack
        /// 4 => BottomLeftFront
        /// 5 => BottomLeftBack
        /// 6 => BottomRightFront
        /// 7 => BottomRightBack
        /// </remarks>
        private void SetNode(int index, OcTreeNode<T> node)
        {
            switch (index)
            {
                case 0:
                    TopLeftFront = node;
                    break;
                case 1:
                    TopLeftBack = node;
                    break;
                case 2:
                    TopRightFront = node;
                    break;
                case 3:
                    TopRightBack = node;
                    break;
                case 4:
                    BottomLeftFront = node;
                    break;
                case 5:
                    BottomLeftBack = node;
                    break;
                case 6:
                    BottomRightFront = node;
                    break;
                case 7:
                    BottomRightBack = node;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Returns all items intersecting with the specified boundary
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="queryBoundary">Boundary to query</param>
        /// <param name="results">Results list</param>
        public static void Query(OcTreeNode<T> node, ICullingVolume queryBoundary, List<T> results)
        {
            if (node == null)
            {
                return;
            }

            if (queryBoundary == null)
            {
                return;
            }

            if (results == null)
            {
                return;
            }

            var containmentType = queryBoundary.Contains(node.Boundary);
            if (containmentType == ContainmentType.Disjoint)
            {
                // The query not contains the current node boundary. Exit
                return;
            }

            if (containmentType == ContainmentType.Contains)
            {
                // The query contains the current node boundary. Return all items without any query
                GetItems(node, results);
            }

            // Query items
            QueryItems(node, queryBoundary, results);
        }
        /// <summary>
        /// Fills the results list with all items
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="results">Results list</param>
        private static void GetItems(OcTreeNode<T> node, List<T> results)
        {
            if (node == null)
            {
                return;
            }

            for (int i = 0; i < node.storedItems; i++)
            {
                if (results.Contains(node.items[i].item))
                {
                    // Item already in the list
                    continue;
                }

                results.Add(node.items[i].item);
            }

            for (int i = 0; i < 8; i++)
            {
                GetItems(node.GetNode(i), results);
            }
        }
        /// <summary>
        /// Fills the results list with all items contained in the specified boundary
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="queryBoundary">Query boundary</param>
        /// <param name="results">Results list</param>
        private static void QueryItems(OcTreeNode<T> node, ICullingVolume queryBoundary, List<T> results)
        {
            if (node == null)
            {
                return;
            }

            for (int i = 0; i < node.storedItems; i++)
            {
                if (results.Contains(node.items[i].item))
                {
                    // Item already in the list
                    continue;
                }

                if (IntersectionHelper.Intersects(queryBoundary, node.items[i].volume))
                {
                    results.Add(node.items[i].item);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                Query(node.GetNode(i), queryBoundary, results);
            }
        }

        /// <summary>
        /// Clears the node
        /// </summary>
        public void Clear()
        {
            storedItems = 0;

            for (int i = 0; i < 8; i++)
            {
                GetNode(i)?.Clear();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return root ? "Root" : $"{storedItems} items.";
        }
    }
}
