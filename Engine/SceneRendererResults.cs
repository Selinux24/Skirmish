
namespace Engine
{
    /// <summary>
    /// Renderer results enumeration
    /// </summary>
    public enum SceneRendererResults
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
        /// Point light shadow map
        /// </summary>
        ShadowMapPoint,
        /// <summary>
        /// Spot light shadow map
        /// </summary>
        ShadowMapSpot,
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
