using Engine.Common;
using System;

namespace Engine
{
    /// <summary>
    /// Shadow map interface
    /// </summary>
    public interface IShadowMap : IDisposable
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Light source
        /// </summary>
        ISceneLight LightSource { get; set; }
        /// <summary>
        /// Light cull index
        /// </summary>
        int CullIndex { get; set; }
        /// <summary>
        /// Depth map texture
        /// </summary>
        EngineShaderResourceView DepthMapTexture { get; }
        /// <summary>
        /// Gets or sets the high resolution map flag (if available)
        /// </summary>
        bool HighResolutionMap { get; set; }

        /// <summary>
        /// Binds the shadow map data to graphics
        /// </summary>
        /// <param name="dc">Device context</param>
        void Bind(IEngineDeviceContext dc);

        /// <summary>
        /// Gets the drawer to draw the specified vertex mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="useTextureAlpha">Uses alpha channel</param>
        /// <returns>Returns a drawer</returns>
        IDrawer GetDrawer(IMesh mesh, bool instanced, bool useTextureAlpha);
        /// <summary>
        /// Gets the drawer to draw the specified vertex data type
        /// </summary>
        /// <typeparam name="T">Vertex data type</typeparam>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="useTextureAlpha">Uses alpha channel</param>
        /// <returns>Returns a drawer</returns>
        IDrawer GetDrawer<T>(bool instanced, bool useTextureAlpha) where T : struct, IVertexData;
    }
}
