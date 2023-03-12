using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Collider interface
    /// </summary>
    /// <remarks>
    /// Some properties were packed here for GJK-EPA collision detection
    /// </remarks>
    public interface ICollider
    {
        /// <summary>
        /// Rigid body
        /// </summary>
        IRigidBody RigidBody { get; }

        /// <summary>
        /// Origin in world space
        /// </summary>
        Vector3 Position { get; }
        /// <summary>
        /// Rotation/scale component of model matrix
        /// </summary>
        Matrix RotationScale { get; }
        /// <summary>
        /// Inverse rotation/scale component of model matrix
        /// </summary>
        Matrix RotationScaleInverse { get; }

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
        /// Attaches a body to the collider
        /// </summary>
        /// <param name="body">Rigid body</param>
        void Attach(IRigidBody body);

        /// <summary>
        /// Gets the support vector of the specified direction
        /// </summary>
        /// <param name="dir">Direction</param>
        Vector3 Support(Vector3 dir);
    }
}