
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
        public BlendModes BlendMode { get; set; } = BlendModes.Opaque;
        /// <summary>
        /// Culling volume by default
        /// </summary>
        public CullingVolumeTypes CullingVolumeType { get; set; } = CullingVolumeTypes.SphericVolume;
        /// <summary>
        /// Collider type
        /// </summary>
        public ColliderTypes ColliderType { get; set; } = ColliderTypes.None;
        /// <summary>
        /// Picking hull
        /// </summary>
        public PickingHullTypes PickingHull { get; set; } = PickingHullTypes.Default;
        /// <summary>
        /// Path finding hull
        /// </summary>
        public PickingHullTypes PathFindingHull { get; set; } = PickingHullTypes.None;
    }
}
