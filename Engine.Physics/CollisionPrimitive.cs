using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Collision primitive
    /// </summary>
    public abstract class CollisionPrimitive : ICollisionPrimitive
    {
        /// <inheritdoc/>
        public IRigidBody RigidBody { get; protected set; }
        /// <inheritdoc/>
        public virtual BoundingBox AABB { get; protected set; }
        /// <inheritdoc/>
        public virtual BoundingSphere SPH { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rigidBody">Rigid body</param>
        protected CollisionPrimitive(IRigidBody rigidBody)
        {
            RigidBody = rigidBody ?? throw new ArgumentNullException(nameof(rigidBody), $"Collision primitive mast have a rigid body.");
        }

        /// <inheritdoc/>
        public void SetInitialState(Vector3 position, Quaternion orientation)
        {
            RigidBody.SetInitialState(position, orientation);
        }
    }
}
