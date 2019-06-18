using SharpDX;

namespace Engine
{
    /// <summary>
    /// Manipulator interface
    /// </summary>
    public interface IManipulator
    {
        /// <summary>
        /// Gets the local transform
        /// </summary>
        Matrix LocalTransform { get; }
        /// <summary>
        /// Gets the final transform
        /// </summary>
        Matrix FinalTransform { get; }
    }
}
