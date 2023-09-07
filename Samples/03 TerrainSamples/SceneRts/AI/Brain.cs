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
    public class Brain
    {
        /// <summary>
        /// Groups dictionary
        /// </summary>
        private readonly Dictionary<int, List<AIAgent>> groups = new();

        /// <summary>
        /// Ground instance
        /// </summary>
        public WalkableScene Scene { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        public Brain(WalkableScene scene)
        {
            Scene = scene;
        }

        /// <summary>
        /// Adds agents to the brain groups
        /// </summary>
        /// <param name="index">Group index</param>
        /// <param name="agent">Agent</param>
        public void AddAgent(int index, AIAgent agent)
        {
            if (!groups.ContainsKey(index))
            {
                groups.Add(index, new List<AIAgent>());
            }

            groups[index].Add(agent);
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

            return targets.ToArray();
        }

        /// <summary>
        /// Updates brain state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            var agents = groups.Values.SelectMany(a => a.ToList()).ToList();
            if (!agents.Any())
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
