using Engine.Collections;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// Path optimization queue
    /// </summary>
    class PathOptimizationQueue : FixedArray<Agent>
    {
        private const int TopologyOptimizationMaximumAgents = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public PathOptimizationQueue() : base(TopologyOptimizationMaximumAgents)
        {

        }

        /// <summary>
        /// Add the CrowdAgent to the optimization queue
        /// </summary>
        /// <param name="agent">Agent to add</param>
        /// <returns>An updated agent count</returns>
        public bool AddToOptQueue(Agent agent)
        {
            //insert neighbor based on greatest time
            if (this.Count == 0 || agent.TopologyOptTime <= this.Last.TopologyOptTime)
            {
                return this.Add(agent);
            }
            else
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (agent.TopologyOptTime >= this[i].TopologyOptTime)
                    {
                        this.InsertAt(i, agent);
                        break;
                    }
                }

                return true;
            }
        }
    }
}
