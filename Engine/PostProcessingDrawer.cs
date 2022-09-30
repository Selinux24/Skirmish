using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.PostProcess;
    using Engine.Common;
    using Engine.PostProcessing;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Post-processing drawer class
    /// </summary>
    public class PostProcessingDrawer : IDisposable
    {
        /// <summary>
        /// Render helper geometry buffer slot
        /// </summary>
        public static int BufferSlot { get; set; } = 0;

        /// <summary>
        /// Graphics class
        /// </summary>
        private readonly Graphics graphics;
        /// <summary>
        /// Window vertex buffer
        /// </summary>
        private Buffer vertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding vertexBufferBinding;
        /// <summary>
        /// Window index buffer
        /// </summary>
        private Buffer indexBuffer;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount;
        /// <summary>
        /// Layout dictionary
        /// </summary>
        private Dictionary<EngineEffectTechnique, InputLayout> layouts = new Dictionary<EngineEffectTechnique, InputLayout>();

        /// <summary>
        /// Constructor
        /// </summary>
        public PostProcessingDrawer(Graphics graphics)
        {
            this.graphics = graphics;

            InitializeBuffers();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PostProcessingDrawer()
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
                vertexBuffer?.Dispose();
                vertexBuffer = null;
                indexBuffer?.Dispose();
                indexBuffer = null;

                foreach (var layout in layouts?.Values)
                {
                    layout?.Dispose();
                }
                layouts?.Clear();
                layouts = null;
            }
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        private void InitializeBuffers()
        {
            var screen = GeometryUtil.CreateScreen((int)graphics.Viewport.Width, (int)graphics.Viewport.Height);

            indexCount = screen.Indices.Count();
            var vertices = VertexPositionTexture.Generate(screen.Vertices, screen.Uvs);

            if (vertexBuffer == null)
            {
                vertexBuffer = graphics.CreateVertexBuffer("Post processing vertex buffer", vertices, true);
                vertexBufferBinding = new VertexBufferBinding(vertexBuffer, vertices.First().GetStride(), 0);
            }
            else
            {
                graphics.WriteDiscardBuffer(vertexBuffer, vertices);
            }

            if (indexBuffer == null)
            {
                indexBuffer = graphics.CreateIndexBuffer("Post processing index buffer", screen.Indices, true);
            }
            else
            {
                graphics.WriteDiscardBuffer(indexBuffer, screen.Indices);
            }
        }

        /// <summary>
        /// Updates the effect parameters
        /// </summary>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        public IBuiltInDrawer UpdateEffectCombine(EngineShaderResourceView texture1, EngineShaderResourceView texture2)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInCombine>();

            drawer.Update(texture1, texture2);

            return drawer;
        }
        /// <summary>
        /// Updates the effect parameters
        /// </summary>
        /// <param name="texture">Source texture</param>
        /// <param name="parameters">Parameters</param>
        public IBuiltInDrawer UpdateEffectParameters(EngineShaderResourceView texture, IDrawerPostProcessParams parameters)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInPostProcess>();

            drawer.UpdatePass(new BuiltInPostProcessState
            {
                RenderTargetTexture = texture,
                EffectIntensity = parameters.EffectIntensity,
            });

            return drawer;
        }
        /// <summary>
        /// Binds the result box input layout to the input assembler
        /// </summary>
        public void Bind()
        {
            graphics.IAPrimitiveTopology = Topology.TriangleList;
            graphics.IASetVertexBuffers(BufferSlot, vertexBufferBinding);
            graphics.IASetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
        }
        /// <summary>
        /// Draws the resulting light composition
        /// </summary>
        public void Draw()
        {
            graphics.DrawIndexed(indexCount, 0, 0);
        }
        /// <summary>
        /// Updates the internal buffers according to the new render dimension
        /// </summary>
        public void Resize()
        {
            InitializeBuffers();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(PostProcessingDrawer)}";
        }
    }
}
