
namespace Engine
{
    /// <summary>
    /// Scene object description
    /// </summary>
    public class SceneObjectDescription
    {
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public bool CastShadow { get; set; } = false;
        /// <summary>
        /// Can be renderer by the deferred renderer
        /// </summary>
        public bool DeferredEnabled { get; set; } = true;
        /// <summary>
        /// Uses depth info
        /// </summary>
        public bool DepthEnabled { get; set; } = true;
        /// <summary>
        /// Blend mode
        /// </summary>
        public BlendModes BlendMode { get; set; } = BlendModes.Default;
        /// <summary>
        /// Use spheric volume for culling by default
        /// </summary>
        public bool SphericVolume { get; set; } = true;
    }
}
