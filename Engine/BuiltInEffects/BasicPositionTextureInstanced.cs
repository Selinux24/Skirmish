using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Basic position-texture instanced drawer
    /// </summary>
    public class BasicPositionTextureInstanced : IGeometryDrawer2
    {
        /// <summary>
        /// Instanced position texture shader
        /// </summary>
        private readonly PositionTextureVsI vertexShader;
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
        /// <param name="positionTextureVsI">Position texture vertex shader</param>
        /// <param name="positionTexturePs">Position texture pixel shader</param>
        public BasicPositionTextureInstanced(Graphics graphics, PositionTextureVsI positionTextureVsI, PositionTexturePs positionTexturePs)
        {
            Graphics = graphics;
            vertexShader = positionTextureVsI;
            pixelShader = positionTexturePs;
        }

        /// <inheritdoc/>
        public void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {

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
