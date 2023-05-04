
namespace Engine.Physics
{
    /// <summary>
    /// Force generator interface
    /// </summary>
    /// <remarks>
    /// A force that affects two bodies in the physics simulator
    /// </remarks>
    public interface IForceGenerator
    {
        /// <summary>
        /// Force source end-point
        /// </summary>
        IContactEndPoint Source { get; }
        /// <summary>
        /// Force target end-point
        /// </summary>
        IContactEndPoint Target { get; }

        /// <summary>
        /// Gets whether the force is active or not
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Calculates the force and applies it to the objects
        /// </summary>
        /// <param name="time">Time</param>
        void UpdateForce(float time);
    }
}
