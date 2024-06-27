using SharpDX;
using System;

namespace Engine.Physics
{
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
}
