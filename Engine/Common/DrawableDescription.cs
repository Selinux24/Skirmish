
namespace Engine.Common
{
    /// <summary>
    /// Drawable description
    /// </summary>
    public abstract class DrawableDescription
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name = null;
        /// <summary>
        /// Is Static
        /// </summary>
        public bool Static = false;
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public bool CastShadow = false;
        /// <summary>
        /// Can be renderer by the deferred renderer
        /// </summary>
        public bool DeferredEnabled = true;
        /// <summary>
        /// Uses depth info
        /// </summary>
        public bool DepthEnabled = true;
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool AlphaEnabled = false;

        /// <summary>
        /// Use spheric volume for culling by default
        /// </summary>
        public bool SphericVolume = true;
    }
}
