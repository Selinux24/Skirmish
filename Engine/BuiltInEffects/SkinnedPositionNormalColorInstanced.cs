using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltInShaders;
    using Engine.Common;
    using Engine.Effects;

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

            pixelShader.SetVSPerFrame(context.EyePosition, context.Lights, context.LevelOfDetail);
            pixelShader.SetDirShadowMap(context.ShadowMapDirectional.Texture);
            pixelShader.SetSpotShadowMap(context.ShadowMapPoint.Texture);
            pixelShader.SetPointShadowMap(context.ShadowMapPoint.Texture);
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
                if (!bufferManager.SetInputAssembler(vertexShader.Shader, mesh.VertexBuffer, mesh.Topology))
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
            if (!bufferManager.SetInputAssembler(vertexShader.Shader, vertexBuffer, topology))
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
                if (!bufferManager.SetInputAssembler(vertexShader.Shader, mesh.VertexBuffer, mesh.Topology))
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
            if (!bufferManager.SetInputAssembler(vertexShader.Shader, vertexBuffer, topology))
            {
                return;
            }

            // Render the primitives.
            Graphics.Draw(drawCount, vertexBuffer.BufferOffset);
        }
    }
}
