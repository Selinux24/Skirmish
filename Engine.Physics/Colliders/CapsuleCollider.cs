using SharpDX;

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
        /// <param name="baseHeight">Base height</param>
        /// <param name="capHeight">Cap height</param>
        public CapsuleCollider(float radius, float baseHeight, float capHeight) : base()
        {
            Radius = radius;
            BaseHeight = baseHeight;
            CapHeight = capHeight;
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
