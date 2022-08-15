using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltInShaders;
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Skinned position-texture drawer
    /// </summary>
    public class SkinnedPositionTexture : IGeometryDrawer2
    {
        /// <summary>
        /// Skinned position texture shader
        /// </summary>
        private readonly SkinnedPositionTextureVs vertexShader;
        /// <summary>
        /// Position texture pixel shader
        /// </summary>
        private readonly PositionTexturePs pixelShader;

        /// <summary>
        /// Graphics
        /// </summary>
        protected readonly Graphics Graphics;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionTextureVs">Position texture vertex shader</param>
        /// <param name="positionTextureVsSkinned">Skinned position texture vertex shader</param>
        /// <param name="positionTexturePs">Position texture pixel shader</param>
        public SkinnedPositionTexture(Graphics graphics, SkinnedPositionTextureVs positionTextureVsSkinned, PositionTexturePs positionTexturePs)
        {
            Graphics = graphics;
            vertexShader = positionTextureVsSkinned;
            pixelShader = positionTexturePs;
        }

        /// <inheritdoc/>
        public void UpdateGlobals(
            EngineShaderResourceView materialPalette,
            uint materialPaletteWidth,
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {
            vertexShader.SetVSGlobals(
                materialPalette, materialPaletteWidth,
                animationPalette, animationPaletteWidth);
        }
        /// <inheritdoc/>
        public void UpdatePerFrame(
            Matrix world,
            DrawContext context)
        {
            vertexShader.SetVSPerFrame(world, context.ViewProjection);

            pixelShader.SetVSPerFrame(context.EyePosition, context.Lights.FogColor, context.Lights.FogStart, context.Lights.FogRange);
        }
        /// <inheritdoc/>
        public void UpdatePerObject(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex,
            Color4 tintColor)
        {
            vertexShader.SetVSPerInstance(
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

            // Set the vertex and pixel shaders that will be used to render this mesh shadow map.
            Graphics.SetVertexShader(vertexShader.Shader);
            Graphics.ClearPixelShader();

            vertexShader.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(vertexShader.Shader, mesh.VertexBuffer, mesh.Topology, false))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(Graphics);
            }
        }
        /// <inheritdoc/>
        public void DrawShadows(BufferManager bufferManager, BufferDescriptor vertexBuffer, int drawCount, Topology topology)
        {
            if (drawCount <= 0)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh shadow map.
            Graphics.SetVertexShader(vertexShader.Shader);
            Graphics.ClearPixelShader();

            vertexShader.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(vertexShader.Shader, vertexBuffer, topology, false))
            {
                return;
            }

            // Render the primitives.
            Graphics.Draw(drawCount, vertexBuffer.BufferOffset);
        }

        /// <inheritdoc/>
        public void Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            Graphics.SetVertexShader(vertexShader.Shader);
            Graphics.SetPixelShader(pixelShader.Shader);

            vertexShader.SetConstantBuffers();
            pixelShader.SetConstantBuffers();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(vertexShader.Shader, mesh.VertexBuffer, mesh.Topology, false))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(Graphics);
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
            Graphics.SetVertexShader(vertexShader.Shader);
            Graphics.SetPixelShader(pixelShader.Shader);

            vertexShader.SetConstantBuffers();
            pixelShader.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(vertexShader.Shader, vertexBuffer, topology, false))
            {
                return;
            }

            // Render the primitives.
            Graphics.Draw(drawCount, vertexBuffer.BufferOffset);
        }
    }
}
