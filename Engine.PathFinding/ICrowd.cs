using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    /// <summary>
    /// Crowd interface
    /// </summary>
    /// <typeparam name="TAgent">Agent type</typeparam>
    /// <typeparam name="TCrowdAgent">Crowd agent type</typeparam>
    /// <typeparam name="TCrowdAgentParams">Crowd agent parameters</typeparam>
    public interface ICrowd<TAgent, TCrowdAgent, TCrowdAgentParams>
        where TAgent : AgentType
        where TCrowdAgent : ICrowdAgent
        where TCrowdAgentParams : ICrowdAgentParameters
    {
        /// <summary>
        /// Adds a new agent to the crowd.
        /// </summary>
        /// <param name="pos">The requested position of the agent.</param>
        /// <param name="param">The configutation of the agent.</param>
        /// <returns>The new agent.</returns>
        TCrowdAgent AddAgent(Vector3 pos, TCrowdAgentParams param);
        /// <summary>
        /// Removes the agent from the crowd.
        /// </summary>
        /// <param name="ag">Agent to remove</param>
        void RemoveAgent(TCrowdAgent ag);
        /// <summary>
        /// Gets the agents int the agent pool.
        /// </summary>
        /// <returns>The collection of agents.</returns>
        TCrowdAgent[] GetAgents();
        /// <summary>
        /// Gets the active agents int the agent pool.
        /// </summary>
        /// <returns>The collection of active agents.</returns>
        TCrowdAgent[] GetActiveAgents();

        /// <summary>
        /// Updates the steering and positions of all agents.
        /// </summary>
        /// <param name="gameTime">The time to update the simulation. [Limit: > 0]</param>
        void Update(IGameTime gameTime);
    }
}
