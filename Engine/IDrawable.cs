
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Drawable interface
    /// </summary>
    public interface IDrawable : ISceneObject
    {
        /// <summary>
        /// Visible
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// Gets or sets whether the object cast shadows or not
        /// </summary>
        ShadowCastingAlgorihtms CastShadow { get; }
        /// <summary>
        /// Gets or sets whether the object is enabled to draw with the deferred renderer
        /// </summary>
        bool DeferredEnabled { get; }
        /// <summary>
        /// Uses depth info
        /// </summary>
        bool DepthEnabled { get; }
        /// <summary>
        /// Blend mode
        /// </summary>
        BlendModes BlendMode { get; }
        /// <summary>
        /// Maximum instance count
        /// </summary>
        int InstanceCount { get; }

        /// <summary>
        /// Draw shadows
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Returns true if the draw calls the device</returns>
        bool DrawShadows(DrawContextShadows context);
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Returns true if the draw calls the device</returns>
        bool Draw(DrawContext context);
    }
}
