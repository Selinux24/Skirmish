using SharpDX;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Collision sphere
    /// </summary>
    public class SphereCollider : Collider
    {
        /// <summary>
        /// Sphere radius
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="radius">Sphere radius</param>
        public SphereCollider(float radius) : base()
        {
            Radius = radius;

            boundingSphere = new BoundingSphere(Vector3.Zero, radius);
            boundingBox = BoundingBox.FromSphere(boundingSphere);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }

        /// <inheritdoc/>
        public override Vector3 Support(Vector3 dir)
        {
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            Vector3 result = Vector3.Normalize(dir) * Radius;

            return Vector3.TransformNormal(result, RotationScale) + Position; //convert support to world space
        }
    }
}