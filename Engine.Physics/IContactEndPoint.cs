using SharpDX;

namespace Engine.Physics
{
    /// <summary>
    /// Contact end-point interface
    /// </summary>
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
}
