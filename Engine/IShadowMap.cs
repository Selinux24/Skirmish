using SharpDX;
using System;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Shadow map interface
    /// </summary>
    public interface IShadowMap : IDisposable
    {
        /// <summary>
        /// Depth map texture
        /// </summary>
        EngineShaderResourceView Texture { get; }
        /// <summary>
        /// Gets or sets the high resolution map flag (if available)
        /// </summary>
        bool HighResolutionMap { get; set; }
        /// <summary>
        /// Light
        /// </summary>
        ISceneLight Light { get; set; }

        /// <summary>
        /// Binds the shadow map data to graphics
        /// </summary>
        /// <param name="dc">Device context</param>
        void Bind(IEngineDeviceContext dc);
        /// <summary>
        /// Gets the drawer to draw this shadow map
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="useTextureAlpha">Uses alpha channel</param>
        /// <returns>Returns a drawer</returns>
        IBuiltInDrawer GetDrawer(VertexTypes vertexType, bool instanced, bool useTextureAlpha);
    }
}
