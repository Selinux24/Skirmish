using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene point light without direction
    /// </summary>
    public interface ISceneLightPoint : ISceneLight
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; }
        /// <summary>
        /// Light radius
        /// </summary>
        float Radius { get; }
    }
}
