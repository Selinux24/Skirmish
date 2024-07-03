using SharpDX;
using System.Collections.Generic;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Cylinder collider
    /// </summary>
    /// <remarks>
    /// Height-aligned with y-axis
    /// </remarks>
    public class CylinderCollider : Collider
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
        public CylinderCollider(float radius, float height) : base()
        {
            Radius = radius;

            float hh = height * 0.5f;
            BaseHeight = -hh;
            CapHeight = hh;

            var extents = new Vector3(radius, hh, radius);
            boundingBox = new BoundingBox(-extents, extents);
            boundingSphere = BoundingSphere.FromBox(boundingBox);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }

        /// <summary>
        /// Gets the four rectangle points, resulting from the cylinder projection on the specified normal
        /// </summary>
        /// <param name="normal">Projection normal</param>
        /// <param name="transform">Use rigid body transform matrix</param>
        public IEnumerable<Vector3> GetProjectionPoints(Vector3 normal, bool transform = false)
        {
            var bse = new Vector3(0, BaseHeight, 0);
            var cap = new Vector3(0, CapHeight, 0);

            if (!transform || !HasTransform)
            {
                return [bse, cap];
            }

            var trn = RigidBody.Transform;
            bse = Vector3.TransformCoordinate(bse, trn);
            cap = Vector3.TransformCoordinate(cap, trn);
            var cNorm = Vector3.Normalize(cap - bse);
            var dir = Vector3.Cross(normal, cNorm);

            if (MathUtil.IsZero(dir.Length()))
            {
                return [bse, cap];
            }

            dir = Vector3.Normalize(Vector3.Cross(dir, cNorm)) * Radius;
            var bse1 = bse + dir;
            var bse2 = bse - dir;
            var cap1 = cap + dir;
            var cap2 = cap - dir;

            return [bse1, bse2, cap1, cap2];
        }

        /// <inheritdoc/>
        public override Vector3 Support(Vector3 dir)
        {
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            var dir_xz = new Vector3(dir.X, 0, dir.Z);
            var result = Vector3.Normalize(dir_xz) * Radius;
            result.Y = dir.Y > 0 ? CapHeight : BaseHeight;

            return Vector3.TransformNormal(result, RotationScale) + Position; //convert support to world space
        }
    }
}
