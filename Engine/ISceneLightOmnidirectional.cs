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
        /// <summary>
        /// Shadow map index
        /// </summary>
        uint ShadowMapIndex { get; set; }

        /// <summary>
        /// Gets the perspective projection matrix for shadow mapping
        /// </summary>
        /// <returns>Returns the perspective projection matrix</returns>
        Matrix GetProjection();
    }
}
