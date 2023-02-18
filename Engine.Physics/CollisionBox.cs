using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Collision box
    /// </summary>
    public class CollisionBox : CollisionPrimitive
    {
        /// <summary>
        /// Box half size extents
        /// </summary>
        public Vector3 Extents { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extents">Bounding box extents</param>
        public CollisionBox(Vector3 extents) : base()
        {
            Extents = extents;

            boundingBox = new BoundingBox(-extents, extents);
            boundingSphere = BoundingSphere.FromBox(boundingBox);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }
    }
}