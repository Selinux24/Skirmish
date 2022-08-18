using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-texture drawer
    /// </summary>
    public class SkinnedPositionNormalTexture : IGeometryDrawer2
    {
        /// <summary>
        /// Skinned position normal texture shader
        /// </summary>
        private readonly SkinnedPositionNormalTextureVs vertexShader;
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
        /// <param name="positionNormalTextureVsSkinned">Skinned position normal texture vertex shader</param>
        /// <param name="positionNormalTexturePs">Position normal texture pixel shader</param>
        public SkinnedPositionNormalTexture(Graphics graphics, SkinnedPositionNormalTextureVs positionNormalTextureVsSkinned, PositionNormalTexturePs positionNormalTexturePs)
        {
            Graphics = graphics;
            vertexShader = positionNormalTextureVsSkinned;
            pixelShader = positionNormalTexturePs;
        }

        /// <inheritdoc/>
        public void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            vertexShader.WriteCBPerInstance(material, tintColor, textureIndex, animation);

            pixelShader.SetDiffuseMap(material.Material?.DiffuseTexture);
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
                Graphics.DrawIndexed(
                    options.IndexBuffer.Count,
                    options.IndexBuffer.BufferOffset,
                    options.VertexBuffer.BufferOffset);
            }
            else
            {
                int drawCount = options.DrawCount > 0 ? options.DrawCount : options.VertexBuffer.Count;

                Graphics.Draw(
                    drawCount,
                    options.VertexBuffer.BufferOffset);
            }
        }
    }
}
