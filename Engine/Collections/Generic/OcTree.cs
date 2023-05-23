using SharpDX;
using System.Collections.Generic;

namespace Engine.Collections.Generic
{
    /// <summary>
    /// OcTree
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class OcTree<T>
    {
        /// <summary>
        /// Root node
        /// </summary>
        private readonly OcTreeNode<T> root;
        /// <summary>
        /// Result list
        /// </summary>
        private readonly List<T> results;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="boundary">Global boundary</param>
        /// <param name="itemsPerNode">Maximum items per node</param>
        public OcTree(BoundingBox boundary, int itemsPerNode)
        {
            root = new OcTreeNode<T>(boundary, itemsPerNode, true);
            results = new();
        }

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

            root.Query(queryBoundary, results);

            return results.AsReadOnly();
        }
        /// <summary>
        /// Clears the OcTree
        /// </summary>
        public void Clear()
        {
            root.Clear();
        }
    }
}
