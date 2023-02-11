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
        public Vector3 HalfSize { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="aabb">Axis aligned bounding box</param>
        public CollisionBox(BoundingBox aabb)
            : this(aabb.GetExtents())
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="halfSize">Bounding box half sizes</param>
        public CollisionBox(Vector3 halfSize) : base()
        {
            HalfSize = halfSize;

            boundingBox = new BoundingBox(-halfSize, halfSize);
            boundingSphere = BoundingSphere.FromBox(boundingBox);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }
    }
}