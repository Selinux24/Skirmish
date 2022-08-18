using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Basic position-color instanced drawer
    /// </summary>
    public class BasicPositionColorInstanced : IGeometryDrawer2
    {
        /// <summary>
        /// Instanced position color shader
        /// </summary>
        private readonly PositionColorVsI vertexShader;
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
        /// <param name="positionColorVsI">Position color vertex shader</param>
        /// <param name="positionColorVsSkinnedI">Skinned position color vertex shader</param>
        /// <param name="positionColorPs">Position color pixel shader</param>
        public BasicPositionColorInstanced(Graphics graphics, PositionColorVsI positionColorVsI, PositionColorPs positionColorPs)
        {
            Graphics = graphics;
            vertexShader = positionColorVsI;
            pixelShader = positionColorPs;
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
        public void Draw(BufferManager bufferManager, DrawOptions options)
        {
            // Set the vertex and pixel shaders that will be used to render this mesh.
            Graphics.SetVertexShader(vertexShader.Shader);
            Graphics.SetPixelShader(pixelShader.Shader);

            vertexShader.SetConstantBuffers();
            pixelShader.SetConstantBuffers();

            // Set the vertex input layout.
            if (!bufferManager.SetInputAssembler(vertexShader.Shader, options.VertexBuffer, options.Topology, options.Instanced))
            {
                return;
            }

            // Render the primitives.
            if (options.Indexed)
            {
                Graphics.DrawIndexedInstanced(
                    options.IndexBuffer.Count,
                    options.InstanceCount,
                    options.IndexBuffer.BufferOffset,
                    options.VertexBuffer.BufferOffset, options.StartInstanceLocation);
            }
            else
            {
                int drawCount = options.DrawCount > 0 ? options.DrawCount : options.VertexBuffer.Count;

                Graphics.DrawInstanced(
                    drawCount,
                    options.InstanceCount,
                    options.VertexBuffer.BufferOffset, options.StartInstanceLocation);
            }
        }
    }
}
