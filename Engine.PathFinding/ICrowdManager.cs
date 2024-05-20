using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Crowd manager interface
    /// </summary>
    /// <typeparam name="TCrowd">Crowd type</typeparam>
    /// <typeparam name="TAgent">Agent type</typeparam>
    /// <typeparam name="TCrowdAgent">Crowd agent type</typeparam>
    /// <typeparam name="TCrowdAgentParams">Crowd agent parameters</typeparam>
    public interface ICrowdManager<TCrowd, TAgent, TCrowdAgent, TCrowdAgentParams>
        where TCrowd : ICrowd<TAgent, TCrowdAgent, TCrowdAgentParams>
        where TAgent : AgentType
        where TCrowdAgent : ICrowdAgent
        where TCrowdAgentParams : ICrowdAgentParameters
    {
        /// <summary>
        /// Adds a new crowd
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Returns the new crowd</returns>
        TCrowd AddCrowd<TSettings>(TSettings settings) where TSettings : ICrowdParameters<TAgent>;
        /// <summary>
        /// Request move all agents in the crowd
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="agent">Agent type</param>
        /// <param name="p">Destination position</param>
        void RequestMoveCrowd(TCrowd crowd, TAgent agent, Vector3 p);
        /// <summary>
        /// Request move a single crowd agent
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="crowdAgent">Agent</param>
        /// <param name="agent">Agent type</param>
        /// <param name="p">Destination position</param>
        void RequestMoveAgent(TCrowd crowd, TCrowdAgent crowdAgent, TAgent agent, Vector3 p);
        /// <summary>
        /// Updates crowd state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void UpdateCrowds(IGameTime gameTime);
    }
}
