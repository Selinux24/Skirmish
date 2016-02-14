using SharpDX;

namespace Engine.PathFinding
{
    public abstract class Graph : IGraph
    {
        /// <summary>
        /// Graph node list
        /// </summary>
        public IGraphNode[] Nodes { get; set; }
        /// <summary>
        /// Gets node wich contains specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Returns the node wich contains the specified point if exists</returns>
        public IGraphNode FindNode(Vector3 point)
        {
            float minDistance = float.MaxValue;
            IGraphNode bestNode = null;

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                float distance;
                if (this.Nodes[i].Contains(point, out distance))
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestNode = this.Nodes[i];
                    }
                }
            }

            return bestNode;
        }
    }
}
