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
    /// Skinned position-color instanced drawer
    /// </summary>
    public class SkinnedPositionColorInstanced : IGeometryDrawer2
    {
        /// <summary>
        /// Skinned instanced position color shader
        /// </summary>
        private readonly SkinnedPositionColorVsI vertexShader;
        /// <summary>
        /// Position color pixel shader
        /// </summary>
        private readonly PositionColorPs pixelShader;

        /// <summary>
        /// Graphics
        /// </summary>
        protected readonly Graphics Graphics;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionColorVsSkinnedI">Skinned position color vertex shader</param>
        /// <param name="positionColorPs">Position color pixel shader</param>
        public SkinnedPositionColorInstanced(Graphics graphics, SkinnedPositionColorVsI positionColorVsSkinnedI, PositionColorPs positionColorPs)
        {
            Graphics = graphics;
            vertexShader = positionColorVsSkinnedI;
            pixelShader = positionColorPs;
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
                if (!bufferManager.SetInputAssembler(vertexShader.Shader, mesh.VertexBuffer, mesh.Topology, true))
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
            if (!bufferManager.SetInputAssembler(vertexShader.Shader, vertexBuffer, topology, true))
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
                if (!bufferManager.SetInputAssembler(vertexShader.Shader, mesh.VertexBuffer, mesh.Topology, true))
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
            if (!bufferManager.SetInputAssembler(vertexShader.Shader, vertexBuffer, topology, true))
            {
                return;
            }

            // Render the primitives.
            Graphics.Draw(drawCount, vertexBuffer.BufferOffset);
        }
    }
}
