using SharpDX;

namespace Engine.Collections
{
    /// <summary>
    /// Quad tree
    /// </summary>
    public class QuadTree
    {
        /// <summary>
        /// Build quadtree
        /// </summary>
        /// <param name="bbox">Initial bounding box</param>
        /// <param name="maxDepth">Maximum depth level</param>
        /// <returns>Returns generated quadtree</returns>
        public static QuadTree Build(BoundingBox bbox, int maxDepth)
        {
            QuadTree quadTree = new QuadTree()
            {
                BoundingBox = bbox,
            };

            quadTree.Root = QuadTreeNode.CreatePartitions(
                quadTree,
                null,
                bbox,
                maxDepth,
                0);

            quadTree.Root.ConnectNodes();

            return quadTree;
        }

        /// <summary>
        /// Root node
        /// </summary>
        public QuadTreeNode Root { get; private set; }
        /// <summary>
        /// Global bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public QuadTree()
        {

        }

        /// <summary>
        /// Gets bounding boxes of specified depth
        /// </summary>
        /// <param name="maxDepth">Maximum depth (if zero there is no limit)</param>
        /// <returns>Returns bounding boxes of specified depth</returns>
        public BoundingBox[] GetBoundingBoxes(int maxDepth = 0)
        {
            return this.Root.GetBoundingBoxes(maxDepth);
        }
        /// <summary>
        /// Gets the nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns the nodes contained into the frustum</returns>
        public QuadTreeNode[] GetNodesInVolume(ref BoundingFrustum frustum)
        {
            return this.Root.GetNodesInVolume(ref frustum);
        }
        /// <summary>
        /// Gets all tail nodes
        /// </summary>
        /// <returns>Returns all tais nodel</returns>
        public QuadTreeNode[] GetTailNodes()
        {
            return this.Root.GetTailNodes();
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (this.Root != null)
            {
                return string.Format("QuadTree Levels {0}", this.Root.GetMaxLevel() + 1);
            }
            else
            {
                return "QuadTree Empty";
            }
        }
    }
}
