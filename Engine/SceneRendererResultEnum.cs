
namespace Engine
{
    /// <summary>
    /// Renderer results enumeration
    /// </summary>
    public enum SceneRendererResultEnum
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Directional shadow map
        /// </summary>
        ShadowMapDirectional,
        /// <summary>
        /// Omnidirectional shadow map
        /// </summary>
        ShadowMapOmnidirectional,
        /// <summary>
        /// Light map
        /// </summary>
        LightMap,
        /// <summary>
        /// Color map
        /// </summary>
        ColorMap,
        /// <summary>
        /// Normal map
        /// </summary>
        NormalMap,
        /// <summary>
        /// Depth map
        /// </summary>
        DepthMap,
        /// <summary>
        /// Other
        /// </summary>
        Other,
    }
}
