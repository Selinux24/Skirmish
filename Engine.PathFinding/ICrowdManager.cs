using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Crowd manager interface
    /// </summary>
    /// <typeparam name="TAgent">Agent type</typeparam>
    /// <typeparam name="TCrowdAgent">Crowd agent type</typeparam>
    public interface ICrowdManager<TAgent, TCrowdAgent>
        where TAgent : AgentType
        where TCrowdAgent : ICrowdAgent
    {
        /// <summary>
        /// Adds a new crowd
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="settings">Crowd settings</param>
        void AddCrowd<TCrowd>(TCrowd crowd, CrowdSettings settings) where TCrowd : ICrowd<TAgent, TCrowdAgent>;
        /// <summary>
        /// Request move all agents in the crowd
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="p">Destination position</param>
        void RequestMoveCrowd<TCrowd>(TCrowd crowd, Vector3 p) where TCrowd : ICrowd<TAgent, TCrowdAgent>;
        /// <summary>
        /// Request move a single crowd agent
        /// </summary>
        /// <param name="crowd">Crowd</param>
        /// <param name="crowdAgent">Agent</param>
        /// <param name="p">Destination position</param>
        void RequestMoveAgent<TCrowd>(TCrowd crowd, TCrowdAgent crowdAgent, Vector3 p) where TCrowd : ICrowd<TAgent, TCrowdAgent>;
        /// <summary>
        /// Updates crowd state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void UpdateCrowds(IGameTime gameTime);
    }
}
