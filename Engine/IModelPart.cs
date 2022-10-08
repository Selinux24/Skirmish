
namespace Engine
{
    /// <summary>
    /// Model part interface
    /// </summary>
    public interface IModelPart
    {
        /// <summary>
        /// Part name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Manipulator
        /// </summary>
        Manipulator3D Manipulator { get; }
    }
}
