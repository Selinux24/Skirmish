using SharpDX;

namespace Engine
{
    /// <summary>
    /// Manipulator interface
    /// </summary>
    public interface ITransform
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
        /// Gets the current velocity
        /// </summary>
        Vector3 Velocity { get; }
        /// <summary>
        /// Gets the position component
        /// </summary>
        Vector3 Position { get; }
        /// <summary>
        /// Gets the scaling component
        /// </summary>
        Vector3 Scaling { get; }
        /// <summary>
        /// Gets the rotation component
        /// </summary>
        Quaternion Rotation { get; }

        /// <summary>
        /// Gets the local transform
        /// </summary>
        Matrix LocalTransform { get; }
        /// <summary>
        /// Gets the global transform
        /// </summary>
        Matrix GlobalTransform { get; }
    }
}
