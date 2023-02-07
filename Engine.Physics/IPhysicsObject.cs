
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
    }
}
