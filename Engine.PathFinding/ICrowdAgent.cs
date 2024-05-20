using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Crowd agent interface
    /// </summary>
    public interface ICrowdAgent
    {
        /// <summary>
        /// True if the agent is active, false if the agent is in an unused slot in the agent pool.
        /// </summary>
        bool Active { get; set; }
        /// <summary>
        /// The current agent position. [(x, y, z)]
        /// </summary>
        Vector3 NPos { get; set; }

        /// <summary>
        /// Submits a new move request for the specified agent.
        /// </summary>
        /// <param name="r">The position's polygon reference.</param>
        /// <param name="pos">The position within the polygon.</param>
        /// <returns>True if the request was successfully submitted.</returns>
        bool RequestMoveTarget(int r, Vector3 pos);
        /// <summary>
        /// Submits a new move request for the specified agent.
        /// </summary>
        /// <param name="vel">The movement velocity. [(x, y, z)]</param>
        /// <returns>True if the request was successfully submitted.</returns>
        bool RequestMoveVelocity(Vector3 vel);
        /// <summary>
        /// Resets any request for the specified agent.
        /// </summary>
        /// <returns>True if the request was successfully reseted.</returns>
        bool ResetMoveTarget();
    }
}
