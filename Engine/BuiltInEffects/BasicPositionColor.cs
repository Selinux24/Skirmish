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
    /// Basic position-color drawer
    /// </summary>
    public class BasicPositionColor : IGeometryDrawer2, IDisposable
    {
        /// <summary>
        /// Graphics
        /// </summary>
        private readonly Graphics graphics;

        /// <summary>
        /// Position color shader
        /// </summary>
        private readonly PositionColorVs positionColorVs;
        /// <summary>
        /// Skinned position color shader
        /// </summary>
        private readonly PositionColorVsSkinned positionColorVsSkinned;
        /// <summary>
        /// Position color pixel shader
        /// </summary>
        private readonly PositionColorPs positionColorPs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionColorVs">Position color vertex shader</param>
        /// <param name="positionColorVsSkinned">Skinned position color vertex shader</param>
        /// <param name="positionColorPs">Position color pixel shader</param>
        public BasicPositionColor(Graphics graphics, PositionColorVs positionColorVs, PositionColorVsSkinned positionColorVsSkinned, PositionColorPs positionColorPs)
        {
            this.graphics = graphics;

            this.positionColorVs = positionColorVs;
            this.positionColorVsSkinned = positionColorVsSkinned;

            this.positionColorPs = positionColorPs;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionColor()
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
            positionColorVsSkinned.SetVSGlobals(animationPalette, animationPaletteWidth);

            positionColorPs.SetVSGlobals(materialPalette, materialPaletteWidth);
        }
        /// <inheritdoc/>
        public void UpdatePerFrame(
            Matrix world,
            DrawContext context)
        {
            positionColorVs.SetVSPerFrame(world, context.ViewProjection);
            positionColorVsSkinned.SetVSPerFrame(world, context.ViewProjection);

            positionColorPs.SetVSPerFrame(context.EyePosition, context.Lights.FogColor, context.Lights.FogStart, context.Lights.FogRange);
        }
        /// <inheritdoc/>
        public void UpdatePerObject(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex,
            Color4 tintColor)
        {
            positionColorVs.SetVSPerInstance(
                tintColor,
                material.Material?.ResourceIndex ?? 0);

            positionColorVsSkinned.SetVSPerInstance(
                tintColor,
                material.Material?.ResourceIndex ?? 0,
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
            graphics.SetVertexShader(positionColorVs.Shader);
            graphics.ClearPixelShader();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionColorVs.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void DrawShadowsSkinned(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionColorVsSkinned.Shader);
            graphics.ClearPixelShader();

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionColorVsSkinned.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionColorVs.Shader);
            graphics.SetPixelShader(positionColorPs.Shader);

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionColorVs.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
        /// <inheritdoc/>
        public void DrawSkinned(BufferManager bufferManager, IEnumerable<Mesh> meshes)
        {
            if (meshes?.Any() != true)
            {
                return;
            }

            // Set the vertex and pixel shaders that will be used to render this mesh.
            graphics.SetVertexShader(positionColorVsSkinned.Shader);
            graphics.SetPixelShader(positionColorPs.Shader);

            foreach (var mesh in meshes)
            {
                // Set the vertex input layout.
                if (!bufferManager.SetInputAssembler(positionColorVsSkinned.Shader, mesh.VertexBuffer, mesh.Topology))
                {
                    continue;
                }

                // Render the mesh.
                mesh.Draw(graphics);
            }
        }
    }
}
