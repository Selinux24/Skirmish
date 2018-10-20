
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
        Manipulator3D Manipulator { get; }
    }
}
