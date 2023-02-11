using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Collision sphere
    /// </summary>
    public class CollisionSphere : CollisionPrimitive
    {
        /// <summary>
        /// Sphere radius
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="radius">Sphere radius</param>
        public CollisionSphere(float radius) : base()
        {
            Radius = radius;

            boundingSphere = new BoundingSphere(Vector3.Zero, radius);
            boundingBox = BoundingBox.FromSphere(boundingSphere);
            orientedBoundingBox = new OrientedBoundingBox(boundingBox);
        }
    }
}