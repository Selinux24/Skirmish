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
        public RigidBody BodyOne { get; set; }
        /// <summary>
        /// Second body
        /// </summary>
        public RigidBody BodyTwo { get; set; }
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
        public Joint(RigidBody a, Vector3 a_pos, RigidBody b, Vector3 b_pos, float error)
        {
            BodyOne = a;
            BodyTwo = b;

            PositionOne = a_pos;
            PositionTwo = b_pos;

            Error = error;
        }

        /// <inheritdoc/>
        public bool AddContact(ContactResolver data, int limit)
        {
            if (!data.HasFreeContacts())
            {
                return false;
            }

            Vector3 positionOneWorld = BodyOne.GetPointInWorldSpace(PositionOne);
            Vector3 positionTwoWorld = BodyTwo.GetPointInWorldSpace(PositionTwo);

            float length = Vector3.Distance(positionTwoWorld, positionOneWorld);

            // Check if it is violated
            if (Math.Abs(length) <= Error)
            {
                return false;
            }

            var normal = Vector3.Normalize(positionTwoWorld - positionOneWorld);
            var point = (positionOneWorld + positionTwoWorld) * 0.5f;
            var penetration = length - Error;

            data.AddContact(BodyOne, BodyTwo, point, normal, penetration);

            return true;
        }
    }
}
