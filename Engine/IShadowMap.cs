using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Shadow map interface
    /// </summary>
    public interface IShadowMap : IDisposable
    {
        /// <summary>
        /// Deph map texture
        /// </summary>
        EngineShaderResourceView Texture { get; }
        /// <summary>
        /// To shadow view*projection matrix
        /// </summary>
        Matrix ToShadowMatrix { get; set; }
        /// <summary>
        /// Light position
        /// </summary>
        Vector3 LightPosition { get; set; }
        /// <summary>
        /// From light view projection
        /// </summary>
        Matrix[] FromLightViewProjectionArray { get; set; }
        /// <summary>
        /// Gets or sets the high resolution map flag (if available)
        /// </summary>
        bool HighResolutionMap { get; set; }

        /// <summary>
        /// Updates the from light view projection
        /// </summary>
        void UpdateFromLightViewProjection(Camera camera, ISceneLight light);
        /// <summary>
        /// Binds the shadow map data to graphics
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="index">Array index</param>
        void Bind(Graphics graphics, int index);
        /// <summary>
        /// Gets the effect to draw this shadow map
        /// </summary>
        /// <returns>Returns an effect</returns>
        IShadowMapDrawer GetEffect();

        /// <summary>
        /// Update shadow map globals
        /// </summary>
        void UpdateGlobals();
    }
}
