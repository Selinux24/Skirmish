using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Body end-point
    /// </summary>
    /// <remarks>Used for connect rigid bodies</remarks>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class BodyEndPoint(IRigidBody body, Vector3 positionLocal) : IContactEndPoint
    {
        /// <inheritdoc/>
        public IRigidBody Body { get; set; } = body ?? throw new ArgumentNullException(nameof(body), $"A body must be specified. For contacts without body, use {nameof(FixedEndPoint)} instead.");
        /// <inheritdoc/>
        public Vector3 BodyPosition { get => Body.Position; }
        /// <inheritdoc/>
        public Vector3 PositionLocal { get; set; } = positionLocal;
        /// <inheritdoc/>
        public Vector3 PositionWorld { get => Body.GetPointInWorldSpace(PositionLocal); }
    }
}
