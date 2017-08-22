using SharpDX;

namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// Obstacle interface
    /// </summary>
    public interface IObstacle
    {
        /// <summary>
        /// Updates the obstacle against a position at a specified desired velocity
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="desiredVelocity">Desired velocity</param>
        void Update(Vector3 position, Vector3 desiredVelocity);
        /// <summary>
        /// Process sample
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="vcand">Candidate velocity</param>
        /// <param name="vel">Velocity</param>
        /// <param name="tmin">Distance to intersection with obstacle</param>
        /// <param name="side">Intersected side</param>
        /// <returns>Returns true if velocity change needed</returns>
        bool ProcessSample(Vector3 position, float radius, Vector3 vcand, Vector3 vel, out float tmin, out float side);
    }
}
