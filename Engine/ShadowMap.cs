using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Shadows;
    using Engine.Common;

    /// <summary>
    /// Shadow map
    /// </summary>
    public abstract class ShadowMap : IShadowMap
    {
        /// <summary>
        /// Scene
        /// </summary>
        protected Scene Scene { get; private set; }
        /// <summary>
        /// Viewport
        /// </summary>
        protected Viewport[] Viewports { get; set; }
        /// <summary>
        /// Depth map
        /// </summary>
        protected IEnumerable<EngineDepthStencilView> DepthMap { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; protected set; }
        /// <inheritdoc/>
        public EngineShaderResourceView Texture { get; protected set; }
        /// <inheritdoc/>
        public Matrix ToShadowMatrix { get; set; } = Matrix.Identity;
        /// <inheritdoc/>
        public Vector3 LightPosition { get; set; } = Vector3.Zero;
        /// <inheritdoc/>
        public Matrix[] FromLightViewProjectionArray { get; set; }
        /// <inheritdoc/>
        public virtual bool HighResolutionMap { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Array size</param>
        protected ShadowMap(Scene scene, string name, int width, int height, int arraySize)
        {
            Scene = scene;
            Name = name;

            Viewports = Helper.CreateArray(arraySize, new Viewport(0, 0, width, height, 0, 1.0f));

            FromLightViewProjectionArray = Helper.CreateArray(arraySize, Matrix.Identity);
        }
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

                Texture?.Dispose();
                Texture = null;
            }
        }

        /// <inheritdoc/>
        public abstract void UpdateFromLightViewProjection(Camera camera, ISceneLight light);
        /// <inheritdoc/>
        public void Bind(Graphics graphics, int index)
        {
            //Set shadow mapper viewport
            graphics.SetViewports(Viewports);

            //Set shadow map depth map without render target
            graphics.SetRenderTargets(
                null, false, Color.Transparent,
                DepthMap.ElementAtOrDefault(index), true, false,
                true);
        }
        /// <inheritdoc/>
        public IBuiltInDrawer GetDrawer(VertexTypes vertexType, bool instanced, bool useTextureAlpha)
        {
            if (useTextureAlpha)
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
        private IBuiltInDrawer GetOpaqueDrawer(VertexTypes vertexType, bool instanced)
        {
            bool skinned = VertexData.IsSkinned(vertexType);

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
        private IBuiltInDrawer GetTransparentDrawer(VertexTypes vertexType, bool instanced)
        {
            bool skinned = VertexData.IsSkinned(vertexType);

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

        /// <inheritdoc/>
        public abstract void UpdateGlobals();
    }
}
