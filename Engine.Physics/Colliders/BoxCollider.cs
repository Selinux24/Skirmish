using SharpDX;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Collision box
    /// </summary>
    public class BoxCollider : Collider
    {
        /// <summary>
        /// Box half size extents
        /// </summary>
        public Vector3 Extents { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extents">Bounding box extents</param>
        public BoxCollider(Vector3 extents) : base()
        {
            Extents = extents;

            boundingBox = new BoundingBox(-extents, extents);
            boundingSphere = BoundingSphere.FromBox(boundingBox);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }

        /// <inheritdoc/>
        public override Vector3 Support(Vector3 dir)
        {
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            Vector3 result;
            result.X = dir.X > 0 ? Extents.X : -Extents.X;
            result.Y = dir.Y > 0 ? Extents.Y : -Extents.Y;
            result.Z = dir.Z > 0 ? Extents.Z : -Extents.Z;

            return Vector3.TransformNormal(result, RotationScale) + Position; //convert support to world space
        }
    }
}