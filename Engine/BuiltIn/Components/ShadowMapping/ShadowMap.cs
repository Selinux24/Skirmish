using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Drawers.Shadows;
using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltIn.Components.ShadowMapping
{
    /// <summary>
    /// Shadow map
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="width">Width</param>
    /// <param name="height">Height</param>
    /// <param name="arraySize">Array size</param>
    public abstract class ShadowMap(Scene scene, string name, int width, int height, int arraySize) : IShadowMap
    {
        /// <summary>
        /// Scene
        /// </summary>
        protected Scene Scene { get; private set; } = scene;
        /// <summary>
        /// Viewport
        /// </summary>
        protected Viewport[] Viewports { get; set; } = Helper.CreateArray(arraySize, new Viewport(0, 0, width, height, 0, 1.0f));
        /// <summary>
        /// Depth map
        /// </summary>
        protected IEnumerable<EngineDepthStencilView> DepthMap { get; set; }

        /// <inheritdoc/>
        public string Name { get; protected set; } = name;
        /// <inheritdoc/>
        public ISceneLight LightSource { get; set; }
        /// <inheritdoc/>
        public int CullIndex { get; set; }
        /// <inheritdoc/>
        public EngineShaderResourceView DepthMapTexture { get; protected set; }
        /// <inheritdoc/>
        public virtual bool HighResolutionMap { get; set; }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowMap()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DepthMap?.Any() == true)
                {
                    for (int i = 0; i < DepthMap.Count(); i++)
                    {
                        DepthMap.ElementAt(i)?.Dispose();
                    }
                }
                DepthMap = null;

                DepthMapTexture?.Dispose();
                DepthMapTexture = null;
            }
        }

        /// <inheritdoc/>
        public void Bind(IEngineDeviceContext dc)
        {
            //Set shadow mapper viewport
            dc.SetViewports(Viewports);

            //Set shadow map depth map without render target
            dc.SetRenderTargets(
                null, false, Color.Transparent,
                DepthMap.ElementAtOrDefault(LightSource?.ShadowMapIndex ?? -1), true, false,
                true);
        }

        /// <inheritdoc/>
        public IDrawer GetDrawer(IMesh mesh, bool instanced, bool useTextureAlpha)
        {
            return GetDrawer(mesh.GetVertexType(), instanced, useTextureAlpha);
        }
        /// <inheritdoc/>
        public IDrawer GetDrawer<T>(bool instanced, bool useTextureAlpha) where T : struct, IVertexData
        {
            return GetDrawer(default(T), instanced, useTextureAlpha);
        }
        /// <summary>
        /// Gets the drawer for the vertex data type
        /// </summary>
        /// <param name="vertexData">Vertex data type</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="useTextureAlpha">Uses alpha channel</param>
        /// <returns>Returns a drawer</returns>
        private static IDrawer GetDrawer(IVertexData vertexData, bool instanced, bool useTextureAlpha)
        {
            bool textured = vertexData.HasChannel(VertexDataChannels.Texture);
            bool skinned = vertexData.HasChannel(VertexDataChannels.BoneIndices);

            if (useTextureAlpha && textured)
            {
                return GetTransparentDrawer(instanced, skinned);
            }

            return GetOpaqueDrawer(instanced, skinned);
        }
        /// <summary>
        /// Gets the opaque drawer for the vertex type
        /// </summary>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="skinned">Skinned</param>
        /// <returns>Returns a drawer</returns>
        private static IDrawer GetOpaqueDrawer(bool instanced, bool skinned)
        {
            if (instanced)
            {
                if (skinned)
                {
                    return BuiltInShaders.GetDrawer<BuiltInPositionSkinnedInstanced>();
                }

                return BuiltInShaders.GetDrawer<BuiltInPositionInstanced>();
            }

            if (skinned)
            {
                return BuiltInShaders.GetDrawer<BuiltInPositionSkinned>();
            }

            return BuiltInShaders.GetDrawer<BuiltInPosition>();
        }
        /// <summary>
        /// Gets the transparent drawer for the vertex type
        /// </summary>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="skinned">Skinned</param>
        /// <returns>Returns a drawer</returns>
        private static IDrawer GetTransparentDrawer(bool instanced, bool skinned)
        {
            if (instanced)
            {
                if (skinned)
                {
                    return BuiltInShaders.GetDrawer<BuiltInTransparentPositionSkinnedInstanced>();
                }

                return BuiltInShaders.GetDrawer<BuiltInTransparentPositionInstanced>();
            }

            if (skinned)
            {
                return BuiltInShaders.GetDrawer<BuiltInTransparentPositionSkinned>();
            }

            return BuiltInShaders.GetDrawer<BuiltInTransparentPosition>();
        }
    }
}
