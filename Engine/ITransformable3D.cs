
namespace Engine
{
    /// <summary>
    /// 3D transformable interface
    /// </summary>
    public interface ITransformable3D
    {
        /// <summary>
        /// Gets the manipulator of the instance
        /// </summary>
        IManipulator3D Manipulator { get; }
        /// <summary>
        /// Sets a new manipulator to this instance
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        void SetManipulator(IManipulator3D manipulator);
    }
}
