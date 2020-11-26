using System;

namespace Engine
{
    /// <summary>
    /// Scene object interface
    /// </summary>
    public interface ISceneObject : IDisposable
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Visible
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// Active
        /// </summary>
        bool Active { get; set; }
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        bool CastShadow { get; set; }
        /// <summary>
        /// Gets or sets whether the object is enabled to draw with the deferred renderer
        /// </summary>
        bool DeferredEnabled { get; set; }
        /// <summary>
        /// Uses depth info
        /// </summary>
        bool DepthEnabled { get; set; }
        /// <summary>
        /// Blend mode
        /// </summary>
        BlendModes BlendMode { get; set; }
        /// <summary>
        /// Object usage
        /// </summary>
        SceneObjectUsages Usage { get; set; }
        /// <summary>
        /// Processing layer
        /// </summary>
        int Layer { get; set; }
        /// <summary>
        /// Maximum instance count
        /// </summary>
        int InstanceCount { get; }
        /// <summary>
        /// Gets whether the current object has owner or not
        /// </summary>
        bool HasOwner { get; }
        /// <summary>
        /// Gets or sets the current object's owner
        /// </summary>
        ISceneObject Owner { get; set; }
    }
}
