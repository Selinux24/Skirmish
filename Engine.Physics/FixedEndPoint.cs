using SharpDX;

namespace Engine.Physics
{
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
