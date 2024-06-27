
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
        /// Applies the force to the specified object
        /// </summary>
        /// <param name="rigidBody">Rigid body</param>
        void ApplyForce(IRigidBody rigidBody);
        /// <summary>
        /// Updates the force state
        /// </summary>
        /// <param name="time">Elapsed time</param>
        void UpdateForce(float time);
    }
}
