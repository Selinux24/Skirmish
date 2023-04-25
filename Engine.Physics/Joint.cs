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

    public interface IContactEndPoint
    {
        /// <summary>
        /// Body
        /// </summary>
        IRigidBody Body { get; }
        /// <summary>
        /// Gets the body position
        /// </summary>
        Vector3 BodyPosition { get; }
        /// <summary>
        /// Relative position of the connection in the body
        /// </summary>
        Vector3 PositionLocal { get; }
        /// <summary>
        /// World position of the connection in the body
        /// </summary>
        Vector3 PositionWorld { get; }
    }

    /// <summary>
    /// Body end-point
    /// </summary>
    /// <remarks>Used for connect rigid bodies</remarks>
    public class BodyEndPoint : IContactEndPoint
    {
        /// <inheritdoc/>
        public IRigidBody Body { get; set; }
        /// <inheritdoc/>
        public Vector3 BodyPosition { get => Body.Position; }
        /// <inheritdoc/>
        public Vector3 PositionLocal { get; set; }
        /// <inheritdoc/>
        public Vector3 PositionWorld { get => Body.GetPointInWorldSpace(PositionLocal); }

        /// <summary>
        /// Constructor
        /// </summary>
        public BodyEndPoint(IRigidBody body, Vector3 positionLocal)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body), $"A body must be specified. For contacts without body, use {nameof(FixedEndPoint)} instead.");
            PositionLocal = positionLocal;
        }
    }

    /// <summary>
    /// Fixed end-point
    /// </summary>
    /// <remarks>Used for connect a rigid body with a fixed world position</remarks>
    public class FixedEndPoint : IContactEndPoint
    {
        /// <inheritdoc/>
        public IRigidBody Body { get => null; }
        /// <inheritdoc/>
        public Vector3 BodyPosition { get => PositionWorld; }
        /// <inheritdoc/>
        public Vector3 PositionLocal { get => PositionWorld; }
        /// <inheritdoc/>
        public Vector3 PositionWorld { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FixedEndPoint(Vector3 positionWorld)
        {
            PositionWorld = positionWorld;
        }
    }
}
