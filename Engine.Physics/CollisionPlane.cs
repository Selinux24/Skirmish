using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Collision plane
    /// </summary>
    public class CollisionPlane : CollisionPrimitive
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
        public CollisionPlane(Vector3 normal, float d) : this(new Plane(normal, d))
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plane">Plane</param>
        public CollisionPlane(Plane plane) : base()
        {
            Plane = plane;
        }
    }
}