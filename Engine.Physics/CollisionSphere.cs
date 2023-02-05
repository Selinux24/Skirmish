using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Collision sphere
    /// </summary>
    public class CollisionSphere : CollisionPrimitive
    {
        /// <inheritdoc/>
        public override BoundingBox AABB
        {
            get
            {
                return BoundingBox.FromSphere(SPH);
            }
        }
        /// <inheritdoc/>
        public override BoundingSphere SPH
        {
            get
            {
                return new BoundingSphere(RigidBody.Position, Radius);
            }
        }
        /// <summary>
        /// Sphere radius
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="radius">Radio</param>
        /// <param name="mass">Masa</param>
        public CollisionSphere(IRigidBody rigidBody, float radius) : base(rigidBody)
        {
            Radius = radius;
        }
    }
}