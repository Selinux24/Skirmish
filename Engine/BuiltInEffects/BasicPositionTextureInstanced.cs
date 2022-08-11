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
    /// Basic position-texture drawer
    /// </summary>
    public class BasicPositionTextureInstanced : IGeometryDrawer2, IDisposable
    {
        /// <summary>
        /// Graphics
        /// </summary>
        private readonly Graphics graphics;

        /// <summary>
        /// Instanced position texture shader
        /// </summary>
        private readonly PositionTextureVsI positionTextureVsI;
        /// <summary>
        /// Skinned instanced position texture shader
        /// </summary>
        private readonly PositionTextureVsSkinnedI positionTextureVsSkinnedI;
        /// <summary>
        /// Position texture pixel shader
        /// </summary>
        private readonly PositionTexturePs positionTexturePs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionTextureVsI">Position texture vertex shader</param>
        /// <param name="positionTextureVsSkinnedI">Skinned position texture vertex shader</param>
        /// <param name="positionTexturePs">Position texture pixel shader</param>
        public BasicPositionTextureInstanced(Graphics graphics, PositionTextureVsI positionTextureVsI, PositionTextureVsSkinnedI positionTextureVsSkinnedI, PositionTexturePs positionTexturePs)
        {
            this.graphics = graphics;

            this.positionTextureVsI = positionTextureVsI;
            this.positionTextureVsSkinnedI = positionTextureVsSkinnedI;
            this.positionTexturePs = positionTexturePs;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionTextureInstanced()
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
            positionTextureVsI.SetVSGlobals(
                materialPalette, materialPaletteWidth);

            positionTextureVsSkinnedI.SetVSGlobals(
                materialPalette, materialPaletteWidth,
                animationPalette, animationPaletteWidth);
        }
        /// <inheritdoc/>
        public void UpdatePerFrame(
            Matrix world,
            DrawContext context)
        {
            positionTextureVsI.SetVSPerFrame(world, context.ViewProjection);
            positionTextureVsSkinnedI.SetVSPerFrame(world, context.ViewProjection);

            positionTexturePs.SetVSPerFrame(context.EyePosition, context.Lights.FogColor, context.Lights.FogStart, context.Lights.FogRange);
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
            graphics.SetVertexShader(positionTextureVsI.Shader);
            graphics.ClearPixelShader();

            positionTextureVsI.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionTextureVsI.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void DrawShadows(BufferManager bufferManager, BufferDescriptor vertexBuffer, int drawCount, Topology topology)
        {
            if (drawCount <= 0)
            {
                return;
            }

            // Set the vertex and clear the pixel shaders that will be used to render this mesh shadow map.
            graphics.SetVertexShader(positionTextureVsI.Shader);
            graphics.ClearPixelShader();

            positionTextureVsI.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(positionTextureVsI.Shader, vertexBuffer, topology))
            {
                return;
            }

            // Render the primitives.
            graphics.Draw(drawCount, vertexBuffer.BufferOffset);
        }

        /// <inheritdoc/>
        public void DrawShadowsSkinned(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh shadow map.
            graphics.SetVertexShader(positionTextureVsSkinnedI.Shader);
            graphics.ClearPixelShader();

            positionTextureVsSkinnedI.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionTextureVsSkinnedI.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void DrawShadowsSkinned(BufferManager bufferManager, BufferDescriptor vertexBuffer, int drawCount, Topology topology)
        {
            if (drawCount <= 0)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh shadow map.
            graphics.SetVertexShader(positionTextureVsSkinnedI.Shader);
            graphics.ClearPixelShader();

            positionTextureVsSkinnedI.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(positionTextureVsSkinnedI.Shader, vertexBuffer, topology))
            {
                return;
            }

            // Render the primitives.
            graphics.Draw(drawCount, vertexBuffer.BufferOffset);
        }

        /// <inheritdoc/>
        public void Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionTextureVsI.Shader);
            graphics.SetPixelShader(positionTexturePs.Shader);

            positionTextureVsI.SetConstantBuffers();
            positionTexturePs.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionTextureVsI.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void Draw(BufferManager bufferManager, BufferDescriptor vertexBuffer, int drawCount, Topology topology)
        {
            if (drawCount <= 0)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionTextureVsI.Shader);
            graphics.SetPixelShader(positionTexturePs.Shader);

            positionTextureVsI.SetConstantBuffers();
            positionTexturePs.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(positionTextureVsI.Shader, vertexBuffer, topology))
            {
                return;
            }

            // Render the primitives.
            graphics.Draw(drawCount, vertexBuffer.BufferOffset);
        }

        /// <inheritdoc/>
        public void DrawSkinned(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionTextureVsSkinnedI.Shader);
            graphics.SetPixelShader(positionTexturePs.Shader);

            positionTextureVsSkinnedI.SetConstantBuffers();
            positionTexturePs.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionTextureVsSkinnedI.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void DrawSkinned(BufferManager bufferManager, BufferDescriptor vertexBuffer, int drawCount, Topology topology)
        {
            if (drawCount <= 0)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionTextureVsSkinnedI.Shader);
            graphics.SetPixelShader(positionTexturePs.Shader);

            positionTextureVsSkinnedI.SetConstantBuffers();
            positionTexturePs.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(positionTextureVsSkinnedI.Shader, vertexBuffer, topology))
            {
                return;
            }

            // Render the primitives.
            graphics.Draw(drawCount, vertexBuffer.BufferOffset);
        }
    }
}
