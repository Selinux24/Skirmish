namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Drawable interface
    /// </summary>
    public interface IDrawable
    {
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
        /// Maximum instance count
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Gets or sets whether the object is static
        /// </summary>
        bool Static { get; set; }
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
        /// Enables transparent blending
        /// </summary>
        bool AlphaEnabled { get; set; }

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        void Draw(DrawContext context);
    }
}
