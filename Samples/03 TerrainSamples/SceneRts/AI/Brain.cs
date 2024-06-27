using Engine;
using Engine.PathFinding;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TerrainSamples.SceneRts.AI
{
    /// <summary>
    /// Brain controller
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    public class Brain(WalkableScene scene)
    {
        /// <summary>
        /// Groups dictionary
        /// </summary>
        private readonly Dictionary<int, List<AIAgent>> groups = [];

        /// <summary>
        /// Ground instance
        /// </summary>
        public WalkableScene Scene { get; set; } = scene;

        /// <summary>
        /// Adds agents to the brain groups
        /// </summary>
        /// <param name="index">Group index</param>
        /// <param name="agent">Agent</param>
        public void AddAgent(int index, AIAgent agent)
        {
            if (!groups.TryGetValue(index, out var group))
            {
                group = [];
                groups.Add(index, group);
            }

            group.Add(agent);
        }
        /// <summary>
        /// Gets available targets for agent
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns all available targets for agent, based on group indexes</returns>
        public AIAgent[] GetTargetsForAgent(AIAgent agent)
        {
            var targets = new List<AIAgent>();

            foreach (var key in groups.Keys)
            {
                if (!groups[key].Contains(agent))
                {
                    targets.AddRange(groups[key]);
                }
            }

            return [.. targets];
        }

        /// <summary>
        /// Updates brain state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(IGameTime gameTime)
        {
            var agents = groups.Values.SelectMany(a => a.ToList()).ToList();
            if (agents.Count == 0)
            {
                return;
            }

            Parallel.ForEach(agents, a =>
            {
                a.Update(gameTime);
            });
        }
    }
}
