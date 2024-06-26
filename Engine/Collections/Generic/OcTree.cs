using SharpDX;
using System.Collections.Generic;

namespace Engine.Collections.Generic
{
    /// <summary>
    /// OcTree
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="boundary">Global boundary</param>
    /// <param name="itemsPerNode">Maximum items per node</param>
    public class OcTree<T>(BoundingBox boundary, int itemsPerNode)
    {
        /// <summary>
        /// Root node
        /// </summary>
        private readonly OcTreeNode<T> root = new(boundary, itemsPerNode, true);
        /// <summary>
        /// Result list
        /// </summary>
        private readonly List<T> results = [];

        /// <summary>
        /// Inserts a item in the structure
        /// </summary>
        /// <param name="itemBoundary">Item boundary</param>
        /// <param name="item">Item</param>
        public void Insert(ICullingVolume itemBoundary, T item)
        {
            root.Insert(itemBoundary, item);
        }
        /// <summary>
        /// Returns all items intersecting with the specified boundary
        /// </summary>
        /// <param name="queryBoundary">Boundary to query</param>
        /// <returns>Returns a list of items</returns>
        public IEnumerable<T> Query(ICullingVolume queryBoundary)
        {
            results.Clear();

            OcTreeNode<T>.Query(root, queryBoundary, results);

            return results.AsReadOnly();
        }
        /// <summary>
        /// Clears the OcTree
        /// </summary>
        public void Clear()
        {
            root.Clear();
        }

        /// <summary>
        /// Gets the number of items in the OcTree
        /// </summary>
        public int CountItems()
        {
            results.Clear();

            OcTreeNode<T>.Query(root, (IntersectionVolumeAxisAlignedBox)root.Boundary, results);

            return results.Count;
        }
    }
}
