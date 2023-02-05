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
        /// Axis aligned bounding box
        /// </summary>
        BoundingBox AABB { get; }
        /// <summary>
        /// Bounding sphere
        /// </summary>
        BoundingSphere SPH { get; }

        /// <summary>
        /// Sets the initial state of the primitive to the indicated position and orientation
        /// </summary>
        /// <param name="position">Initial position</param>
        /// <param name="orientation">Initial orientation</param>
        void SetInitialState(Vector3 position, Quaternion orientation);
    }
}