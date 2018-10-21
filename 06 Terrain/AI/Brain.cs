using Engine;
using System.Collections.Generic;

namespace Terrain.AI
{
    /// <summary>
    /// Brain controller
    /// </summary>
    public class Brain
    {
        /// <summary>
        /// Groups dictionary
        /// </summary>
        private readonly Dictionary<int, List<AIAgent>> groups = new Dictionary<int, List<AIAgent>>();

        /// <summary>
        /// Ground instance
        /// </summary>
        public Scene Scene { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        public Brain(Scene scene)
        {
            this.Scene = scene;
        }

        /// <summary>
        /// Adds agents to the brain groups
        /// </summary>
        /// <param name="index">Group index</param>
        /// <param name="agent">Agent</param>
        public void AddAgent(int index, AIAgent agent)
        {
            if (!this.groups.ContainsKey(index))
            {
                this.groups.Add(index, new List<AIAgent>());
            }

            this.groups[index].Add(agent);
        }
        /// <summary>
        /// Gets available targets for agent
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns all available targets for agent, based on group indexes</returns>
        public AIAgent[] GetTargetsForAgent(AIAgent agent)
        {
            List<AIAgent> targets = new List<AIAgent>();

            foreach (var key in this.groups.Keys)
            {
                if (!this.groups[key].Contains(agent))
                {
                    targets.AddRange(this.groups[key]);
                }
            }

            return targets.ToArray();
        }
    }
}
