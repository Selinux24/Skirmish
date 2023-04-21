using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Rod
    /// </summary>
    public class Rod : IContactGenerator
    {
        /// <summary>
        /// First body
        /// </summary>
        public IRigidBody BodyOne { get; set; }
        /// <summary>
        /// Second body
        /// </summary>
        public IRigidBody BodyTwo { get; set; }
        /// <summary>
        /// Relative position of the connection in the first body
        /// </summary>
        public Vector3 PositionOne { get; set; }
        /// <summary>
        /// Relative position of the connection in the second body
        /// </summary>
        public Vector3 PositionTwo { get; set; }
        /// <summary>
        /// Rod distance
        /// </summary>
        public float Length { get; set; }
        /// <summary>
        /// Tolerance
        /// </summary>
        public float Tolerance { get; set; }
        /// <summary>
        /// World position of the connection in the first body
        /// </summary>
        public Vector3 PositionWorldOne
        {
            get
            {
                return BodyOne?.GetPointInWorldSpace(PositionOne) ?? Vector3.Zero;
            }
        }
        /// <summary>
        /// World position of the connection in the second body
        /// </summary>
        public Vector3 PositionWorldTwo
        {
            get
            {
                return BodyTwo?.GetPointInWorldSpace(PositionTwo) ?? Vector3.Zero;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Rod(IRigidBody a, Vector3 a_pos, IRigidBody b, Vector3 b_pos, float length, float tolerance)
        {
            BodyOne = a;
            BodyTwo = b;

            PositionOne = a_pos;
            PositionTwo = b_pos;

            Length = length;
            Tolerance = tolerance;
        }

        /// <inheritdoc/>
        public bool AddContact(ContactResolver contactData, int limit)
        {
            if (!contactData.HasFreeContacts())
            {
                return false;
            }

            // Find current separation length
            Vector3 positionOneWorld = PositionWorldOne;
            Vector3 positionTwoWorld = PositionWorldTwo;
            float currentLen = Vector3.Distance(positionOneWorld, positionTwoWorld);

            if (MathUtil.NearEqual(Length - currentLen, Tolerance))
            {
                return false;
            }

            var point = (positionOneWorld + positionTwoWorld) * 0.5f;
            var normal = Vector3.Normalize(BodyTwo.Position - BodyOne.Position);
            float penetration = currentLen - Length;

            // The contact normal depends on whether it is necessary to extend or contract to preserve the length
            if (currentLen <= Length)
            {
                normal = -normal;
                penetration = -penetration;
            }

            contactData.AddContact(BodyOne, BodyTwo, point, normal, penetration, 0f, 1f);

            return true;
        }
    }
}
