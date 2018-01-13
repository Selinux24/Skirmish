using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Shadow map interface
    /// </summary>
    public interface IShadowMap
    {
        /// <summary>
        /// Deph map texture
        /// </summary>
        EngineShaderResourceView Texture { get; }
        /// <summary>
        /// From light view projection
        /// </summary>
        Matrix[] FromLightViewProjectionArray { get; set; }

        /// <summary>
        /// Binds the shadow map data to graphics
        /// </summary>
        /// <param name="graphics">Graphics</param>
        void Bind(Graphics graphics);
    }
}
