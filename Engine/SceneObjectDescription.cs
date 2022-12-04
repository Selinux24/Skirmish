using System;

namespace Engine
{
    /// <summary>
    /// Scene object description
    /// </summary>
    public class SceneObjectDescription
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
        /// Gets or sets whether the object cast shadows or not
        /// </summary>
        public ShadowCastingAlgorihtms CastShadow { get; set; } = ShadowCastingAlgorihtms.None;
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

    /// <summary>
    /// Shadow casting algorihtms
    /// </summary>
    [Flags]
    public enum ShadowCastingAlgorihtms : uint
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Directional shadow casting
        /// </summary>
        Directional = 1,
        /// <summary>
        /// Spot shadow casting
        /// </summary>
        Spot = 2,
        /// <summary>
        /// Point shadow casting
        /// </summary>
        Point = 4,
    }
}
