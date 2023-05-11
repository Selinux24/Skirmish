using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections.Generic
{
    /// <summary>
    /// OcTree node
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class OcTreeNode<T>
    {
        /// <summary>
        /// Node boundary
        /// </summary>
        private readonly BoundingBox boundary;
        /// <summary>
        /// Items per node
        /// </summary>
        private readonly int itemsPerNode;
        /// <summary>
        /// Is the root node
        /// </summary>
        private readonly bool root;
        /// <summary>
        /// Node items array
        /// </summary>
        private readonly (ICullingVolume volume, T item)[] items;
        /// <summary>
        /// Number of stored items in the array
        /// </summary>
        private int storedItems;

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
        /// Constructor
        /// </summary>
        /// <param name="boundary">Node boundary</param>
        /// <param name="itemsPerNode">Maximum items per node</param>
        /// <param name="root">Is root node</param>
        public OcTreeNode(BoundingBox boundary, int itemsPerNode, bool root)
        {
            this.boundary = boundary;
            this.itemsPerNode = itemsPerNode;
            this.root = root;
            items = root ? Array.Empty<(ICullingVolume volume, T item)>() : new (ICullingVolume, T)[itemsPerNode];
            storedItems = 0;
        }

        /// <summary>
        /// Gets the actual items in the node
        /// </summary>
        public IEnumerable<T> GetItems()
        {
            return items.Take(storedItems).Select(i => i.item).ToArray();
        }

        /// <summary>
        /// Inserts a item in the node
        /// </summary>
        /// <param name="itemBoundary">Item boundary</param>
        /// <param name="item">Item</param>
        public void Insert(ICullingVolume itemBoundary, T item)
        {
            if (itemBoundary.Contains(boundary) == ContainmentType.Disjoint)
            {
                return;
            }

            if (!root && storedItems < items.Length)
            {
                items[storedItems++] = (itemBoundary, item);

                return;
            }

            int index = -1;

            foreach (var childBoundary in boundary.Octree())
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
        /// <param name="queryBoundary">Boundary to query</param>
        /// <returns>Returns a list of items</returns>
        public IEnumerable<T> Query(ICullingVolume queryBoundary)
        {
            if (queryBoundary.Contains(boundary) == ContainmentType.Disjoint)
            {
                return Enumerable.Empty<T>();
            }

            List<T> results = new List<T>();

            for (int i = 0; i < storedItems; i++)
            {
                if (!IntersectionHelper.Intersects(queryBoundary, items[i].volume))
                {
                    continue;
                }

                results.Add(items[i].item);
            }

            for (int i = 0; i < 8; i++)
            {
                var child = GetNode(i);
                if (child == null)
                {
                    continue;
                }

                var colliders = child.Query(queryBoundary);
                if (!colliders.Any())
                {
                    continue;
                }

                foreach (var collider in colliders)
                {
                    if (results.Contains(collider))
                    {
                        continue;
                    }

                    results.Add(collider);
                }
            }

            return results.ToArray();
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
