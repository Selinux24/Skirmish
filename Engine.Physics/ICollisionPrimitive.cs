using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Collision primitive interface
    /// </summary>
    public interface ICollisionPrimitive
    {
        /// <summary>
        /// Rigid body
        /// </summary>
        IRigidBody RigidBody { get; }

        /// <summary>
        /// Bounding sphere
        /// </summary>
        BoundingSphere BoundingSphere { get; }
        /// <summary>
        /// Axis aligned bounding box
        /// </summary>
        BoundingBox BoundingBox { get; }
        /// <summary>
        /// Oriented bounding box
        /// </summary>
        OrientedBoundingBox OrientedBoundingBox { get; }

        /// <summary>
        /// Attachs a body to the collision primitive
        /// </summary>
        /// <param name="body">Rigid body</param>
        void Attach(IRigidBody body);
    }
}