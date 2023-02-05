using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Collision box
    /// </summary>
    public class CollisionBox : CollisionPrimitive
    {
        /// <summary>
        /// Box corners
        /// </summary>
        private static readonly float[,] mults = new float[8, 3] { { 1, 1, 1 }, { -1, 1, 1 }, { 1, -1, 1 }, { -1, -1, 1 }, { 1, 1, -1 }, { -1, 1, -1 }, { 1, -1, -1 }, { -1, -1, -1 } };

        /// <summary>
        /// Bounding sphere radius
        /// </summary>
        private readonly float radius;

        /// <inheritdoc/>
        public override BoundingBox AABB
        {
            get
            {
                return BoundingBox.FromPoints(GetCorners());
            }
        }
        /// <inheritdoc/>
        public override BoundingSphere SPH
        {
            get
            {
                return new BoundingSphere(RigidBody.Position, radius);
            }
        }
        /// <summary>
        /// Gets the oriented bounding box
        /// </summary>
        public OrientedBoundingBox OBB
        {
            get
            {
                return new OrientedBoundingBox(AABB)
                {
                    Transformation = RigidBody.TransformMatrix
                };
            }
        }
        /// <summary>
        /// Box half size extents
        /// </summary>
        public Vector3 HalfSize { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CollisionBox(IRigidBody rigidBody, BoundingBox aabb)
            : this(rigidBody, aabb.Maximum, aabb.Minimum)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public CollisionBox(IRigidBody rigidBody, Vector3 max, Vector3 min)
            : this(rigidBody, (max - min) * 0.5f)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        public CollisionBox(IRigidBody rigidBody, Vector3 halfSize) : base(rigidBody)
        {
            HalfSize = halfSize;
            radius = BoundingSphere.FromPoints(GetCorners()).Radius;
        }

        /// <summary>
        /// Gets the specified corner in world coordinates
        /// </summary>
        /// <param name="index">Corner index</param>
        public Vector3 GetCorner(int index)
        {
            Vector3 result = new Vector3(mults[index, 0], mults[index, 1], mults[index, 2]);
            result = Vector3.Multiply(result, HalfSize);
            result = Vector3.TransformCoordinate(result, RigidBody.TransformMatrix);

            return result;
        }
        /// <summary>
        /// Gets the box corners
        /// </summary>
        public Vector3[] GetCorners()
        {
            Vector3[] corners = new Vector3[8];

            for (int i = 0; i < 8; i++)
            {
                corners[i] = GetCorner(i);
            }

            return corners;
        }

        /// <inheritdoc/>
        public override void SetInitialState(Vector3 position, Quaternion orientation)
        {
            base.SetInitialState(position, orientation);

            float mass = RigidBody.Mass;
            Vector3 squares = Vector3.Multiply(HalfSize, HalfSize);
            var inertiaTensor = Core.CreateFromInertiaTensorCoeffs(
                0.3f * mass * (squares.Y + squares.Z),
                0.3f * mass * (squares.X + squares.Z),
                0.3f * mass * (squares.X + squares.Y));
            RigidBody.SetInertiaTensor(inertiaTensor);
        }
    }
}