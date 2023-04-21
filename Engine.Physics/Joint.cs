using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Joint between two bodies
    /// </summary>
    public class Joint : IContactGenerator
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
        /// Maximum joint distance
        /// </summary>
        public float Error { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a"></param>
        /// <param name="a_pos"></param>
        /// <param name="b"></param>
        /// <param name="b_pos"></param>
        /// <param name="error"></param>
        public Joint(IRigidBody a, Vector3 a_pos, IRigidBody b, Vector3 b_pos, float error)
        {
            BodyOne = a;
            BodyTwo = b;

            PositionOne = a_pos;
            PositionTwo = b_pos;

            Error = error;
        }

        /// <inheritdoc/>
        public bool AddContact(ContactResolver contactData, int limit)
        {
            if (!contactData.HasFreeContacts())
            {
                return false;
            }

            var positionOneWorld = BodyOne.GetPointInWorldSpace(PositionOne);
            var positionTwoWorld = BodyTwo.GetPointInWorldSpace(PositionTwo);

            float distance = Vector3.Distance(positionTwoWorld, positionOneWorld);
            if (Math.Abs(distance) <= Error)
            {
                // Valid joint
                return false;
            }

            // Adjust bodies
            var normal = Vector3.Normalize(positionTwoWorld - positionOneWorld);
            var point = (positionOneWorld + positionTwoWorld) * 0.5f;
            var penetration = distance - Error;

            contactData.AddContact(BodyOne, BodyTwo, point, normal, penetration, 0f, 1f);

            return true;
        }
    }
}
