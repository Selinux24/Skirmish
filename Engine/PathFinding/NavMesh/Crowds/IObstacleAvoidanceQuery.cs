using SharpDX;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// Obstacle avoidance query interface
    /// </summary>
    public interface IObstacleAvoidanceQuery
    {
        /// <summary>
        /// Adds a new obstacle to the query
        /// </summary>
        /// <param name="obstacle">Obstacle</param>
        void AddObstacle(IObstacle obstacle);
        /// <summary>
        /// Resets the ObstacleAvoidanceQuery's internal data
        /// </summary>
        void Reset();

        /// <summary>
        /// Samples the new velocity
        /// </summary>
        /// <param name="position">Agent position</param>
        /// <param name="radius">Agent radius</param>
        /// <param name="maximumVelocity">Agent maximum velocity</param>
        /// <param name="velocity">Agent current velocity</param>
        /// <param name="desiredVelocity">Agent desired velocity</param>
        /// <param name="newVelocity">Returns the new agent velocity to avoid the query obstacles</param>
        /// <returns>Returns the number of samples used in the query</returns>
        int SampleVelocity(Vector3 position, float radius, float maximumVelocity, Vector3 velocity, Vector3 desiredVelocity, out Vector3 newVelocity);
    }
}
