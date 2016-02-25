using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    [Serializable]
    public abstract class GraphNode : IGraphNode
    {
        protected List<IGraphNode> ConnectedNodes = new List<IGraphNode>();

        /// <summary>
        /// Gets the connected node list
        /// </summary>
        public IGraphNode[] Connections
        {
            get
            {
                return this.ConnectedNodes.ToArray();
            }
        }
        /// <summary>
        /// Gets a connected node by index
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Returns the connected node by index</returns>
        public IGraphNode this[int index]
        {
            get
            {
                return this.ConnectedNodes[index];
            }
        }

        /// <summary>
        /// Node state
        /// </summary>
        public GraphNodeStates State { get; set; }
        /// <summary>
        /// Node passing cost
        /// </summary>
        public float Cost { get; set; }
        /// <summary>
        /// Center position
        /// </summary>
        public Vector3 Center { get; protected set; }

        /// <summary>
        /// Gets whether this node contains specified point
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Returns whether this node contains specified point</returns>
        public abstract bool Contains(Vector3 point, out float distance);

        public abstract Vector3[] GetPoints();
    }
}
