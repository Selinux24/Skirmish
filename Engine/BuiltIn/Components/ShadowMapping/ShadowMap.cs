using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Drawers.Shadows;
using Engine.BuiltIn.Primitives;
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
        public IDrawer GetDrawer(VertexTypes vertexType, bool instanced, bool useTextureAlpha)
        {
            if (useTextureAlpha && VertexTypesHelper.IsTextured(vertexType))
            {
                return GetTransparentDrawer(vertexType, instanced);
            }
            else
            {
                return GetOpaqueDrawer(vertexType, instanced);
            }
        }
        /// <summary>
        /// Gets the opaque drawer for the vertex type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="instanced">Instanced</param>
        private static IDrawer GetOpaqueDrawer(VertexTypes vertexType, bool instanced)
        {
            bool skinned = VertexTypesHelper.IsSkinned(vertexType);

            if (instanced)
            {
                if (skinned)
                {
                    return BuiltInShaders.GetDrawer<BuiltInPositionSkinnedInstanced>();
                }
                else
                {
                    return BuiltInShaders.GetDrawer<BuiltInPositionInstanced>();
                }
            }
            else
            {
                if (skinned)
                {
                    return BuiltInShaders.GetDrawer<BuiltInPositionSkinned>();
                }
                else
                {
                    return BuiltInShaders.GetDrawer<BuiltInPosition>();
                }
            }
        }
        /// <summary>
        /// Gets the transparent drawer for the vertex type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="instanced">Instanced</param>
        private static IDrawer GetTransparentDrawer(VertexTypes vertexType, bool instanced)
        {
            bool skinned = VertexTypesHelper.IsSkinned(vertexType);

            if (instanced)
            {
                if (skinned)
                {
                    return BuiltInShaders.GetDrawer<BuiltInTransparentPositionSkinnedInstanced>();
                }
                else
                {
                    return BuiltInShaders.GetDrawer<BuiltInTransparentPositionInstanced>();
                }
            }
            else
            {
                if (skinned)
                {
                    return BuiltInShaders.GetDrawer<BuiltInTransparentPositionSkinned>();
                }
                else
                {
                    return BuiltInShaders.GetDrawer<BuiltInTransparentPosition>();
                }
            }
        }
    }
}
