using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Physics object interface
    /// </summary>
    public interface IPhysicsObject
    {
        /// <summary>
        /// Rigid body
        /// </summary>
        IRigidBody Body { get; }
        /// <summary>
        /// Primiteve collider
        /// </summary>
        ICollisionPrimitive Collider { get; }

        /// <summary>
        /// Updates the object state
        /// </summary>
        void Update();
        /// <summary>
        /// Resets the object state
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        void Reset(Vector3 position, Quaternion rotation);
    }
}
