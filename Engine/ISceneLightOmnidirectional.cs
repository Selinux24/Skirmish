using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene light without direction
    /// </summary>
    public interface ISceneLightOmnidirectional
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; }
        /// <summary>
        /// Light radius
        /// </summary>
        float Radius { get; }
        /// <summary>
        /// Casts shadows
        /// </summary>
        bool CastShadow { get; }
    }
}
