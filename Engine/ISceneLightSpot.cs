using SharpDX;

namespace Engine
{
    /// <summary>
    /// Spot light
    /// </summary>
    public interface ISceneLightSpot : ISceneLight
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; }
        /// <summary>
        /// Light direction
        /// </summary>
        Vector3 Direction { get; }
        /// <summary>
        /// Light radius
        /// </summary>
        float Radius { get; }
        /// <summary>
        /// Cone angle in radians
        /// </summary>
        float AngleRadians { get; }

        /// <summary>
        /// Shadow map index
        /// </summary>
        uint ShadowMapCount { get; set; }
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        Matrix[] FromLightVP { get; set; }
    }
}
