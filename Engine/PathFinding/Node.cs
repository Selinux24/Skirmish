using Engine.Geometry;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    /// <summary>
    /// Every polygon becomes a Node, which contains a position and cost.
    /// </summary>
    public class Node : IValueWithCost
    {
        public Vector3 Pos;
        public float cost;
        public float total;
        public int ParentIdx = 30; //index to parent node
        public NodeFlags Flags = 0; //node flags 0/open/closed
        public PolyId Id; //polygon ref the node corresponds to

        //TODO should make more generic or move to Pathfinding namespace

        public float Cost
        {
            get
            {
                return total;
            }
        }
    }
    /// <summary>
    /// Determine which list the node is in.
    /// </summary>
    [Flags]
    public enum NodeFlags
    {
        /// <summary>
        /// Open list contains nodes to examine.
        /// </summary>
        Open = 0x01,

        /// <summary>
        /// Closed list stores path.
        /// </summary>
        Closed = 0x02
    }
    /// <summary>
    /// An interface that defines a class containing a cost associated with the instance.
    /// Used in <see cref="PriorityQueue{T}"/>
    /// </summary>
    public interface IValueWithCost
    {
        /// <summary>
        /// Gets the cost of this instance.
        /// </summary>
        float Cost { get; }
    }
}
