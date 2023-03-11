using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Collision primitive
    /// </summary>
    public abstract class CollisionPrimitive : ICollisionPrimitive
    {
        private static readonly BoundingSphere EmptyBoundingSphere = new BoundingSphere();
        private static readonly BoundingBox EmptyBoundingBox = new BoundingBox();
        private static readonly OrientedBoundingBox EmptyOrientedBoundingBox = new OrientedBoundingBox();

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
                if (boundingBox == EmptyBoundingBox)
                {
                    return boundingBox;
                }

                if ((RigidBody?.Transform ?? Matrix.Identity) == Matrix.Identity)
                {
                    return boundingBox;
                }

                return boundingBox.SetTransform(RigidBody.Transform);
            }
        }
        /// <inheritdoc/>
        public BoundingSphere BoundingSphere
        {
            get
            {
                if (boundingSphere == EmptyBoundingSphere)
                {
                    return boundingSphere;
                }

                if ((RigidBody?.Transform ?? Matrix.Identity) == Matrix.Identity)
                {
                    return boundingSphere;
                }

                return boundingSphere.SetTransform(RigidBody.Transform);
            }
        }
        /// <inheritdoc/>
        public OrientedBoundingBox OrientedBoundingBox
        {
            get
            {
                if (orientedBoundingBox == EmptyOrientedBoundingBox)
                {
                    return orientedBoundingBox;
                }

                if ((RigidBody?.Transform ?? Matrix.Identity) == Matrix.Identity)
                {
                    return orientedBoundingBox;
                }

                return orientedBoundingBox.SetTransform(RigidBody.Transform);
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
