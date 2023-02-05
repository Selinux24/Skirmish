using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Collision plane
    /// </summary>
    public class CollisionPlane : CollisionPrimitive
    {
        /// <summary>
        /// Gets the plane normal
        /// </summary>
        public Vector3 Normal { get; private set; }
        /// <summary>
        /// Gets the plane distance
        /// </summary>
        public float D { get; private set; }
        /// <summary>
        /// Gets the plane
        /// </summary>
        public Plane Plane
        {
            get
            {
                return new Plane(Normal, D);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rigidBody">Rigid body</param>
        /// <param name="plane">Plane</param>
        public CollisionPlane(IRigidBody rigidBody, Plane plane)
            : this(rigidBody, plane.Normal, plane.D)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rigidBody">Rigid body</param>
        /// <param name="normal">Plate normal</param>
        /// <param name="d">Plane distance</param>
        public CollisionPlane(IRigidBody rigidBody, Vector3 normal, float d) : base(rigidBody)
        {
            Normal = normal;
            D = d;
        }
    }
}