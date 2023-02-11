using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Collision primitive
    /// </summary>
    public abstract class CollisionPrimitive : ICollisionPrimitive
    {
        /// <summary>
        /// Untransformed bounding box
        /// </summary>
        protected BoundingBox boundingBox;
        /// <summary>
        /// Untransformed bounding sphere
        /// </summary>
        protected BoundingSphere boundingSphere;
        /// <summary>
        /// Untransformed oriented bounding box
        /// </summary>
        protected OrientedBoundingBox orientedBoundingBox;

        /// <inheritdoc/>
        public IRigidBody RigidBody { get; private set; }
        /// <inheritdoc/>
        public BoundingBox BoundingBox
        {
            get
            {
                if ((RigidBody?.Position ?? Vector3.Zero) == Vector3.Zero)
                {
                    return boundingBox;
                }

                var position = RigidBody.Position;
                return new BoundingBox(boundingBox.Minimum + position, boundingBox.Maximum + position);
            }
        }
        /// <inheritdoc/>
        public BoundingSphere BoundingSphere
        {
            get
            {
                if ((RigidBody?.Position ?? Vector3.Zero) == Vector3.Zero)
                {
                    return boundingSphere;
                }

                return new BoundingSphere(RigidBody.Position, boundingSphere.Radius);
            }
        }
        /// <inheritdoc/>
        public OrientedBoundingBox OrientedBoundingBox
        {
            get
            {
                if ((RigidBody?.Transform ?? Matrix.Identity) == Matrix.Identity)
                {
                    return orientedBoundingBox;
                }

                var trnObb = orientedBoundingBox;
                trnObb.Transformation = RigidBody.Transform;
                return trnObb;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected CollisionPrimitive()
        {

        }

        /// <inheritdoc/>
        public void Attach(IRigidBody body)
        {
            RigidBody = body ?? throw new ArgumentNullException(nameof(body), $"{nameof(CollisionPrimitive)} must have a rigid body.");

            if (!RigidBody.HasFiniteMass())
            {
                return;
            }

            Vector3 halfSize = boundingBox.GetExtents();
            Vector3 squares = halfSize * halfSize;

            RigidBody.SetIntertiaCoefficients(
                0.3333f * (squares.Y + squares.Z),
                0.3333f * (squares.X + squares.Z),
                0.3333f * (squares.X + squares.Y));
        }
    }
}
