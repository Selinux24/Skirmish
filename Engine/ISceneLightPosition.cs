using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene light with position
    /// </summary>
    public interface ISceneLightPosition
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// Light radius
        /// </summary>
        float Radius { get; set; }
    }
}
