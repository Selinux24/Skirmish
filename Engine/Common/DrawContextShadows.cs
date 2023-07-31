using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Drawing context for shadow mapping
    /// </summary>
    public struct DrawContextShadows
    {
        /// <summary>
        /// Context name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Graphics
        /// </summary>
        public Graphics Graphics { get; set; }
        /// <summary>
        /// View * projection matrix
        /// </summary>
        public Matrix ViewProjection { get; set; }
        /// <summary>
        /// Eye position
        /// </summary>
        public Vector3 EyePosition { get; set; }
        /// <summary>
        /// Bounding frustum
        /// </summary>
        public BoundingFrustum Frustum { get; set; }
        /// <summary>
        /// Shadow map to fill
        /// </summary>
        public IShadowMap ShadowMap { get; set; }

        /// <summary>
        /// Pass context
        /// </summary>
        public PassContext PassContext { get; set; }
        /// <summary>
        /// Device context
        /// </summary>
        public readonly EngineDeviceContext DeviceContext { get => PassContext.DeviceContext; }
    }
}
