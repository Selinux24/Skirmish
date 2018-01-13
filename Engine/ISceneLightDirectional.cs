using SharpDX;

namespace Engine
{
    /// <summary>
    /// Directional light
    /// </summary>
    public interface ISceneLightDirectional
    {
        /// <summary>
        /// Light direction
        /// </summary>
        Vector3 Direction { get; }
        /// <summary>
        /// Casts shadows
        /// </summary>
        bool CastShadow { get; }
    }
}
