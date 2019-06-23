using SharpDX;

namespace Engine
{
    /// <summary>
    /// Manipulator interface
    /// </summary>
    public interface IManipulator
    {
        /// <summary>
        /// Forward vector
        /// </summary>
        Vector3 Forward { get; }
        /// <summary>
        /// Backward vector
        /// </summary>
        Vector3 Backward { get; }
        /// <summary>
        /// Left vector
        /// </summary>
        Vector3 Left { get; }
        /// <summary>
        /// Right vector
        /// </summary>
        Vector3 Right { get; }
        /// <summary>
        /// Up vector
        /// </summary>
        Vector3 Up { get; }
        /// <summary>
        /// Down vector
        /// </summary>
        Vector3 Down { get; }
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; }
        /// <summary>
        /// Velocity
        /// </summary>
        Vector3 Velocity { get; }
    }
}
