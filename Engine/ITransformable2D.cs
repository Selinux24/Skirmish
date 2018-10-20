
namespace Engine
{
    /// <summary>
    /// 2D transformable interface
    /// </summary>
    public interface ITransformable2D
    {
        /// <summary>
        /// Gets the manipulator of the instance
        /// </summary>
        Manipulator2D Manipulator { get; }
    }
}
