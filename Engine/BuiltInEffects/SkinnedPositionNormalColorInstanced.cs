using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-color instanced drawer
    /// </summary>
    public class SkinnedPositionNormalColorInstanced : IGeometryDrawer2
    {
        /// <summary>
        /// Skinned instanced position normal color shader
        /// </summary>
        private readonly SkinnedPositionNormalColorVsI vertexShader;
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
        /// <param name="positionNormalColorVsSkinnedI">Skinned position normal color vertex shader</param>
        /// <param name="positionNormalColorPs">Position normal color pixel shader</param>
        public SkinnedPositionNormalColorInstanced(Graphics graphics, SkinnedPositionNormalColorVsI positionNormalColorVsSkinnedI, PositionNormalColorPs positionNormalColorPs)
        {
            Graphics = graphics;
            vertexShader = positionNormalColorVsSkinnedI;
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
