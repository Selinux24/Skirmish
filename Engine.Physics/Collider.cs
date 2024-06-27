using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Collider
    /// </summary>
    public abstract class Collider : ICollider
    {
        private static readonly BoundingSphere EmptyBoundingSphere = new();
        private static readonly BoundingBox EmptyBoundingBox = new();
        private static readonly OrientedBoundingBox EmptyOrientedBoundingBox = new();

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
        public Vector3 Position
        {
            get
            {
                if (!HasTransform)
                {
                    return Vector3.Zero;
                }

                return RigidBody.Position;
            }
        }
        /// <inheritdoc/>
        public Matrix RotationScale
        {
            get
            {
                if (!HasTransform)
                {
                    return Matrix.Identity;
                }

                return Matrix.RotationQuaternion(RigidBody.Rotation);
            }
        }
        /// <inheritdoc/>
        public Matrix RotationScaleInverse
        {
            get
            {
                if (!HasTransform)
                {
                    return Matrix.Identity;
                }

                return Matrix.Invert(RotationScale);
            }
        }
        /// <inheritdoc/>
        public bool HasTransform
        {
            get
            {
                return !(RigidBody?.Transform ?? Matrix.Identity).IsIdentity;
            }
        }

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

                if (!HasTransform)
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

                if (!HasTransform)
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

                if (!HasTransform)
                {
                    return orientedBoundingBox;
                }

                return orientedBoundingBox.SetTransform(RigidBody.Transform);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected Collider()
        {

        }

        /// <inheritdoc/>
        public void Attach(IRigidBody body)
        {
            RigidBody = body ?? throw new ArgumentNullException(nameof(body), $"{nameof(Collider)} must have a rigid body.");

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
        /// <inheritdoc/>
        public abstract Vector3 Support(Vector3 dir);
    }
}
