using SharpDX;
using System.Collections.Generic;

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
        IRigidBody RigidBody { get; }
        /// <summary>
        /// Collider list
        /// </summary>
        IEnumerable<ICollider> Colliders { get; }

        /// <summary>
        /// Gets whether the specified object is close enough to the current object that a possible collision exists 
        /// </summary>
        /// <param name="obj">Physics object</param>
        bool BroadPhaseTest(IPhysicsObject obj);
        /// <summary>
        /// Gets the bounding sphere for the broad phase test
        /// </summary>
        ICullingVolume GetBroadPhaseBounds();
        /// <summary>
        /// Gets the candidate collider list for the specified object
        /// </summary>
        /// <param name="obj">Physics object</param>
        IEnumerable<ICollider> GetBroadPhaseColliders(IPhysicsObject obj);

        /// <summary>
        /// Updates the object state
        /// </summary>
        void Update();
        /// <summary>
        /// Resets the object state
        /// </summary>
        /// <param name="transform">Transform</param>
        void Reset(Matrix transform);
        /// <summary>
        /// Resets the object state
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        void Reset(Vector3 position, Quaternion rotation);
    }
}
