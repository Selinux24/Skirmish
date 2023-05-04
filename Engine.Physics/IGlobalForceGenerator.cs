
namespace Engine.Physics
{
    /// <summary>
    /// Global force generator interface
    /// </summary>
    /// <remarks>
    /// A force that affects any body in the physics simulator
    /// </remarks>
    public interface IGlobalForceGenerator
    {
        /// <summary>
        /// Gets whether the force is active or not
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Calculates the force and applies it to the specified object
        /// </summary>
        /// <param name="rigidBody">Rigid body</param>
        /// <param name="time">Time</param>
        void UpdateForce(IRigidBody rigidBody, float time);
    }
}
