using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Basic position-normal-texture drawer
    /// </summary>
    public class BasicPositionNormalTexture : IGeometryDrawer2
    {
        /// <summary>
        /// Position normal texture shader
        /// </summary>
        private readonly PositionNormalTextureVs vertexShader;
        /// <summary>
        /// Position normal texture pixel shader
        /// </summary>
        private readonly PositionNormalTexturePs pixelShader;

        /// <summary>
        /// Graphics
        /// </summary>
        protected readonly Graphics Graphics;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionNormalTextureVs">Position normal texture vertex shader</param>
        /// <param name="positionNormalTexturePs">Position normal texture pixel shader</param>
        public BasicPositionNormalTexture(Graphics graphics, PositionNormalTextureVs positionNormalTextureVs, PositionNormalTexturePs positionNormalTexturePs)
        {
            Graphics = graphics;
            vertexShader = positionNormalTextureVs;
            pixelShader = positionNormalTexturePs;
        }

        /// <inheritdoc/>
        public void Update(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex,
            Color4 tintColor)
        {
            vertexShader.SetCBPerInstance(
                tintColor,
                material.Material?.ResourceIndex ?? 0);

            pixelShader.SetDiffuseMap(material.Material?.DiffuseTexture);
        }

        /// <inheritdoc/>
        public void DrawShadows(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and clear the pixel shaders that will be used to render this mesh shadow map.
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

            // Set the vertex and clear the pixel shaders that will be used to render this mesh shadow map.
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
