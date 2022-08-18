using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Basic position-normal-color instanced drawer
    /// </summary>
    public class BasicPositionNormalColorInstanced : IGeometryDrawer2
    {
        /// <summary>
        /// Instanced position normal color shader
        /// </summary>
        private readonly PositionNormalColorVsI vertexShader;
        /// <summary>
        /// Position normal color pixel shader
        /// </summary>
        private readonly PositionNormalColorPs pixelShader;

        /// <summary>
        /// Graphics
        /// </summary>
        protected readonly Graphics Graphics;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionNormalColorVsI">Position color vertex shader</param>
        /// <param name="positionColorVsSkinnedI">Skinned position normal color vertex shader</param>
        /// <param name="positionNormalColorPs">Position normal color pixel shader</param>
        public BasicPositionNormalColorInstanced(Graphics graphics, PositionNormalColorVsI positionNormalColorVsI, PositionNormalColorPs positionNormalColorPs)
        {
            Graphics = graphics;
            vertexShader = positionNormalColorVsI;
            pixelShader = positionNormalColorPs;
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
