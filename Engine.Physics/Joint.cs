using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Joint between two bodies
    /// </summary>
    public class Joint : IContactGenerator
    {
        /// <inheritdoc/>
        public IContactEndPoint One { get; set; }
        /// <inheritdoc/>
        public IContactEndPoint Two { get; set; }
        /// <inheritdoc/>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Maximum joint distance
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Joint(IContactEndPoint one, IContactEndPoint two, float length)
        {
            One = one ?? throw new ArgumentNullException(nameof(one));
            Two = two ?? throw new ArgumentNullException(nameof(two));

            if (one is FixedEndPoint && two is FixedEndPoint)
            {
                throw new ArgumentException("Invalid end-points. No connection between fixed positions permited.", nameof(two));
            }

            Length = length;
        }

        /// <inheritdoc/>
        public bool AddContact(ContactResolver contactData, int limit)
        {
            if (!IsActive)
            {
                return false;
            }

            if (!contactData.HasFreeContacts())
            {
                return false;
            }

            // Find current separation length
            var positionOneWorld = One.PositionWorld;
            var positionTwoWorld = Two.PositionWorld;

            float distance = Vector3.Distance(positionTwoWorld, positionOneWorld);
            if (Math.Abs(distance) <= Length)
            {
                // Valid joint
                return false;
            }

            // Adjust bodies
            var normal = Vector3.Normalize(positionTwoWorld - positionOneWorld);
            var point = (positionOneWorld + positionTwoWorld) * 0.5f;
            float penetration = distance - Length;

            // The contact normal depends on whether it is necessary to extend or contract to preserve the length
            if (distance <= Length)
            {
                normal = -normal;
                penetration = -penetration;
            }

            contactData.AddContact(One.Body, Two.Body, point, normal, penetration, 0f, 1f);

            return true;
        }
    }
}
