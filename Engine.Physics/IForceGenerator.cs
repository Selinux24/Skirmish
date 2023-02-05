
namespace Engine.Physics
{
    /// <summary>
    /// Force generator interface
    /// </summary>
    public interface IForceGenerator
    {
        /// <summary>
        /// Calculates the force and applies it to the specified object
        /// </summary>
        /// <param name="rigidBody">Rigid body</param>
        /// <param name="time">Time</param>
        public abstract void UpdateForce(IRigidBody rigidBody, float time);
    }
}
