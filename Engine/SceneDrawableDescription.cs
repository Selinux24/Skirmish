
namespace Engine
{
    /// <summary>
    /// Scene object description
    /// </summary>
    public class SceneDrawableDescription
    {
        /// <summary>
        /// The object starts active
        /// </summary>
        public bool StartsActive { get; set; } = true;
        /// <summary>
        /// The object starts visible
        /// </summary>
        public bool StartsVisible { get; set; } = true;
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
