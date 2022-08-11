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
    public class BasicPositionTexture : IGeometryDrawer2, IDisposable
    {
        /// <summary>
        /// Graphics
        /// </summary>
        private readonly Graphics graphics;

        /// <summary>
        /// Position texture shader
        /// </summary>
        private readonly PositionTextureVs positionTextureVs;
        /// <summary>
        /// Skinned position texture shader
        /// </summary>
        private readonly PositionTextureVsSkinned positionTextureVsSkinned;
        /// <summary>
        /// Position texture pixel shader
        /// </summary>
        private readonly PositionTexturePs positionTexturePs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionTextureVs">Position texture vertex shader</param>
        /// <param name="positionTextureVsSkinned">Skinned position texture vertex shader</param>
        /// <param name="positionTexturePs">Position texture pixel shader</param>
        public BasicPositionTexture(Graphics graphics, PositionTextureVs positionTextureVs, PositionTextureVsSkinned positionTextureVsSkinned, PositionTexturePs positionTexturePs)
        {
            this.graphics = graphics;

            this.positionTextureVs = positionTextureVs;
            this.positionTextureVsSkinned = positionTextureVsSkinned;

            this.positionTexturePs = positionTexturePs;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionTexture()
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
            positionTextureVs.SetVSGlobals(
                materialPalette, materialPaletteWidth);

            positionTextureVsSkinned.SetVSGlobals(
                materialPalette, materialPaletteWidth,
                animationPalette, animationPaletteWidth);
        }
        /// <inheritdoc/>
        public void UpdatePerFrame(
            Matrix world,
            DrawContext context)
        {
            positionTextureVs.SetVSPerFrame(world, context.ViewProjection);
            positionTextureVsSkinned.SetVSPerFrame(world, context.ViewProjection);

            positionTexturePs.SetVSPerFrame(context.EyePosition, context.Lights.FogColor, context.Lights.FogStart, context.Lights.FogRange);
        }
        /// <inheritdoc/>
        public void UpdatePerObject(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex,
            Color4 tintColor)
        {
            positionTextureVs.SetVSPerInstance(
                tintColor,
                material.Material?.ResourceIndex ?? 0,
                textureIndex);

            positionTextureVsSkinned.SetVSPerInstance(
                tintColor,
                material.Material?.ResourceIndex ?? 0,
                textureIndex,
                animation.Offset1,
                animation.Offset2,
                animation.InterpolationAmount);
        }

        /// <inheritdoc/>
        public void DrawShadows(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and clear the pixel shaders that will be used to render this mesh shadow map.
            graphics.SetVertexShader(positionTextureVs.Shader);
            graphics.ClearPixelShader();

            positionTextureVs.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionTextureVs.Shader, mesh.VertexBuffer, mesh.Topology))
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
            graphics.SetVertexShader(positionTextureVs.Shader);
            graphics.ClearPixelShader();

            positionTextureVs.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(positionTextureVs.Shader, vertexBuffer, topology))
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
            graphics.SetVertexShader(positionTextureVsSkinned.Shader);
            graphics.ClearPixelShader();

            positionTextureVsSkinned.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionTextureVsSkinned.Shader, mesh.VertexBuffer, mesh.Topology))
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
            graphics.SetVertexShader(positionTextureVsSkinned.Shader);
            graphics.ClearPixelShader();

            positionTextureVsSkinned.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(positionTextureVsSkinned.Shader, vertexBuffer, topology))
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
            graphics.SetVertexShader(positionTextureVs.Shader);
            graphics.SetPixelShader(positionTexturePs.Shader);

            positionTextureVs.SetConstantBuffers();
            positionTexturePs.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionTextureVs.Shader, mesh.VertexBuffer, mesh.Topology))
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
            graphics.SetVertexShader(positionTextureVs.Shader);
            graphics.SetPixelShader(positionTexturePs.Shader);

            positionTextureVs.SetConstantBuffers();
            positionTexturePs.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(positionTextureVs.Shader, vertexBuffer, topology))
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
            graphics.SetVertexShader(positionTextureVsSkinned.Shader);
            graphics.SetPixelShader(positionTexturePs.Shader);

            positionTextureVsSkinned.SetConstantBuffers();
            positionTexturePs.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionTextureVsSkinned.Shader, mesh.VertexBuffer, mesh.Topology))
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
            graphics.SetVertexShader(positionTextureVsSkinned.Shader);
            graphics.SetPixelShader(positionTexturePs.Shader);

            positionTextureVsSkinned.SetConstantBuffers();
            positionTexturePs.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(positionTextureVsSkinned.Shader, vertexBuffer, topology))
            {
                return;
            }

            // Render the primitives.
            graphics.Draw(drawCount, vertexBuffer.BufferOffset);
        }
    }
}
