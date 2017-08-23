using Engine.Collections;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// Path resolve queue
    /// </summary>
    class PathResolveQueue : FixedArray<Agent>
    {
        private const int PathMaximumAgents = 8;

        /// <summary>
        /// Constructor
        /// </summary>
        public PathResolveQueue() : base(PathMaximumAgents)
        {

        }

        /// <summary>
        /// Add the CrowdAgent to the path queue
        /// </summary>
        /// <param name="agent">Agent to add</param>
        /// <returns>An updated agent count</returns>
        public bool AddToPathQueue(Agent agent)
        {
            //Insert agent based on greatest time
            if (this.Count == 0 || agent.TargetReplanTime <= this.Last.TargetReplanTime)
            {
                //Insert at last position
                return this.Add(agent);
            }
            else
            {
                //Find position
                for (int i = 0; i < this.Count; i++)
                {
                    if (agent.TargetReplanTime >= this[i].TargetReplanTime)
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
