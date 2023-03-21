using SharpDX;
using System.Collections.Generic;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Capsule: Height-aligned with y-axis
    /// </summary>
    public class CapsuleCollider : Collider
    {
        /// <summary>
        /// Capsule radius
        /// </summary>
        public float Radius { get; set; } = 0;
        /// <summary>
        /// Base height
        /// </summary>
        public float BaseHeight { get; set; } = 0;
        /// <summary>
        /// Cap height
        /// </summary>
        public float CapHeight { get; set; } = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        public CapsuleCollider(float radius, float height) : base()
        {
            Radius = radius;

            float hh = (height - (radius * 2f)) * 0.5f;
            BaseHeight = -hh;
            CapHeight = hh;

            var extents = new Vector3(radius, height * 0.5f, radius);
            boundingBox = new BoundingBox(-extents, extents);
            boundingSphere = BoundingSphere.FromBox(boundingBox);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }

        /// <summary>
        /// Gets the capsule axis segment points
        /// </summary>
        /// <param name="transform">Use rigid body transform matrix</param>
        public IEnumerable<Vector3> GetPoints(bool transform = false)
        {
            var bse = new Vector3(0, BaseHeight, 0);
            var cap = new Vector3(0, CapHeight, 0);

            if (!transform || !HasTransform)
            {
                return new[] { bse, cap };
            }

            var trn = RigidBody.Transform;
            bse = Vector3.TransformCoordinate(bse, trn);
            cap = Vector3.TransformCoordinate(cap, trn);

            return new[] { bse, cap };
        }

        /// <inheritdoc/>
        public override Vector3 Support(Vector3 dir)
        {
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            Vector3 result = Vector3.Normalize(dir) * Radius;
            result.Y += dir.Y > 0 ? CapHeight : BaseHeight;

            return Vector3.TransformNormal(result, RotationScale) + Position; //convert support to world space
        }
    }
}
