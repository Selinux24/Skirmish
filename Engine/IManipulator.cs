using SharpDX;

namespace Engine
{
    /// <summary>
    /// Manipulator interface
    /// </summary>
    public interface IManipulator
    {
        /// <summary>
        /// Gets the transform by name
        /// </summary>
        /// <param name="name">Transform name</param>
        /// <returns>Returns the transform by name</returns>
        Matrix Transform(string name);
    }
}
