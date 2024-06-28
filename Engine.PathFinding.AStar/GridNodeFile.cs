using Engine.Content;
using System;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Grid node file
    /// </summary>
    [Serializable]
    public class GridNodeFile
    {
        /// <summary>
        /// Creates a grid node file from a node
        /// </summary>
        /// <param name="node">Node</param>
        public static GridNodeFile FromNode(GridNode node)
        {
            return new GridNodeFile
            {
                State = node.State,
                TotalCost = node.TotalCost,
                SouthEast = node.SouthEast,
                SouthWest = node.SouthWest,
                NorthEast = node.NorthEast,
                NorthWest = node.NorthWest,
                Center = node.Center,
            };
        }
        /// <summary>
        /// Creates a node from a grid node file
        /// </summary>
        /// <param name="nodefile">Node file</param>
        public static GridNode FromFile(GridNodeFile nodefile)
        {
            return new GridNode(nodefile.NorthEast, nodefile.NorthWest, nodefile.SouthWest, nodefile.SouthEast, nodefile.Center, nodefile.TotalCost)
            {
                State = nodefile.State
            };
        }

        /// <summary>
        /// Node state
        /// </summary>
        public GridNodeStates State { get; set; }
        /// <summary>
        /// Node passing cost
        /// </summary>
        public float TotalCost { get; set; }
        /// <summary>
        /// Center position
        /// </summary>
        public Position3 Center { get; set; }
        /// <summary>
        /// North West point
        /// </summary>
        public Position3 NorthWest { get; set; }
        /// <summary>
        /// North East point
        /// </summary>
        public Position3 NorthEast { get; set; }
        /// <summary>
        /// South West point
        /// </summary>
        public Position3 SouthWest { get; set; }
        /// <summary>
        /// South East point
        /// </summary>
        public Position3 SouthEast { get; set; }
    }
}
