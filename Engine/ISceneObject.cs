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
        /// Processing order
        /// </summary>
        int Order { get; set; }
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
        /// Maximum instance count
        /// </summary>
        int InstanceCount { get; }
        /// <summary>
        /// Gets or sets if the current object has a parent
        /// </summary>
        bool HasParent { get; }
    }
}
