﻿using SharpDX;

namespace Engine.Collections
{
    using Engine.Common;

    /// <summary>
    /// Quad tree
    /// </summary>
    public class QuadTree<T> where T : IVertexList
    {
        /// <summary>
        /// Root node
        /// </summary>
        public QuadTreeNode<T> Root { get; private set; }
        /// <summary>
        /// Global bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }
        /// <summary>
        /// Global bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Partitioning items</param>
        /// <param name="maxDepth">Maximum depth</param>
        public QuadTree(T[] items, int maxDepth)
        {
            BoundingBox bbox = GeometryUtil.CreateBoundingBox(items);
            BoundingSphere bsph = GeometryUtil.CreateBoundingSphere(items);

            this.BoundingBox = bbox;
            this.BoundingSphere = bsph;

            this.Root = QuadTreeNode<T>.CreatePartitions(
                this, null,
                bbox, items,
                maxDepth,
                0);

            this.Root.ConnectNodes();
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
        public QuadTreeNode<T>[] GetNodesInVolume(ref BoundingFrustum frustum)
        {
            return this.Root.GetNodesInVolume(ref frustum);
        }
        /// <summary>
        /// Gets all tail nodes
        /// </summary>
        /// <returns>Returns all tais nodel</returns>
        public QuadTreeNode<T>[] GetTailNodes()
        {
            return this.Root.GetTailNodes();
        }
        /// <summary>
        /// Gets the closest node to the specified position
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns the closest node to the specified position</returns>
        public QuadTreeNode<T> FindNode(Vector3 position)
        {
            var node = this.Root.GetNode(position);

            if (node == null)
            {
                //Look for the closest node
                var tailNodes = this.GetTailNodes();

                float dist = float.MaxValue;
                for (int i = 0; i < tailNodes.Length; i++)
                {
                    float d = Vector3.DistanceSquared(position, tailNodes[i].Center);
                    if (d < dist)
                    {
                        dist = d;
                        node = tailNodes[i];
                    }
                }
            }

            return node;
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
