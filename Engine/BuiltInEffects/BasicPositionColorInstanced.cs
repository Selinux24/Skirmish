using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltInShaders;
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Basic position-color drawer
    /// </summary>
    public class BasicPositionColorInstanced : IGeometryDrawer2, IDisposable
    {
        /// <summary>
        /// Graphics
        /// </summary>
        private readonly Graphics graphics;

        /// <summary>
        /// Instanced position color shader
        /// </summary>
        private readonly PositionColorVsI positionColorVsI;
        /// <summary>
        /// Skinned instanced position color shader
        /// </summary>
        private readonly PositionColorVsSkinnedI positionColorVsSkinnedI;
        /// <summary>
        /// Position color pixel shader
        /// </summary>
        private readonly PositionColorPs positionColorPs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionColorVsI">Position color vertex shader</param>
        /// <param name="positionColorVsSkinnedI">Skinned position color vertex shader</param>
        /// <param name="positionColorPs">Position color pixel shader</param>
        public BasicPositionColorInstanced(Graphics graphics, PositionColorVsI positionColorVsI, PositionColorVsSkinnedI positionColorVsSkinnedI, PositionColorPs positionColorPs)
        {
            this.graphics = graphics;

            this.positionColorVsI = positionColorVsI;
            this.positionColorVsSkinnedI = positionColorVsSkinnedI;
            this.positionColorPs = positionColorPs;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionColorInstanced()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
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

            }
        }

        /// <inheritdoc/>
        public void UpdateGlobals(
            EngineShaderResourceView materialPalette,
            uint materialPaletteWidth,
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {
            positionColorVsSkinnedI.SetVSGlobals(animationPalette, animationPaletteWidth);

            positionColorPs.SetVSGlobals(materialPalette, materialPaletteWidth);
        }
        /// <inheritdoc/>
        public void UpdatePerFrame(
            Matrix world,
            DrawContext context)
        {
            positionColorVsI.SetVSPerFrame(world, context.ViewProjection);
            positionColorVsSkinnedI.SetVSPerFrame(world, context.ViewProjection);

            positionColorPs.SetVSPerFrame(context.EyePosition, context.Lights.FogColor, context.Lights.FogStart, context.Lights.FogRange);
        }
        /// <inheritdoc/>
        public void UpdatePerObject(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex,
            Color4 tintColor)
        {

        }

        /// <inheritdoc/>
        public void DrawShadows(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and clear the pixel shaders that will be used to render this mesh shadow map.
            graphics.SetVertexShader(positionColorVsI.Shader);
            graphics.ClearPixelShader();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionColorVsI.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void DrawShadowsSkinned(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionColorVsSkinnedI.Shader);
            graphics.ClearPixelShader();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionColorVsSkinnedI.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionColorVsI.Shader);
            graphics.SetPixelShader(positionColorPs.Shader);

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionColorVsI.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void DrawSkinned(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionColorVsSkinnedI.Shader);
            graphics.SetPixelShader(positionColorPs.Shader);

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionColorVsSkinnedI.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
    }
}
