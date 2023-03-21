using SharpDX;

namespace Engine.Physics.Colliders
{
    /// <summary>
    /// Collision plane
    /// </summary>
    public class HalfSpaceCollider : Collider
    {
        /// <summary>
        /// Gets the plane
        /// </summary>
        public Plane Plane { get; private set; }
        /// <summary>
        /// Gets the plane normal
        /// </summary>
        public Vector3 Normal { get { return Plane.Normal; } }
        /// <summary>
        /// Gets the plane distance
        /// </summary>
        public float D { get { return Plane.D; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="normal">Plate normal</param>
        /// <param name="d">Plane distance</param>
        public HalfSpaceCollider(Vector3 normal, float d) : this(new Plane(normal, d))
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plane">Plane</param>
        public HalfSpaceCollider(Plane plane) : base()
        {
            Plane = plane;
        }

        /// <summary>
        /// Gets the plane
        /// </summary>
        /// <param name="transform">Use rigid body transform matrix</param>
        public Plane GetPlane(bool transform = false)
        {
            if (!transform || !HasTransform)
            {
                return Plane;
            }

            return Plane.Transform(Plane, RigidBody.Transform);
        }

        /// <inheritdoc/>
        public override Vector3 Support(Vector3 dir)
        {
            dir = Vector3.TransformNormal(dir, RotationScaleInverse); //find support in model space

            Vector3 result = Vector3.Cross(Vector3.Normalize(dir), Normal);

            return Vector3.TransformNormal(result, RotationScale) + Position; //convert support to world space
        }
    }
}