using Engine.BuiltIn.Primitives;
using Engine.Common;

namespace Engine.BuiltIn.Drawers.PostProcess
{
    /// <summary>
    /// Post-processing drawer class
    /// </summary>
    public class BuiltInPostProcessingDrawer : IPostProcessingDrawer<BuiltInPostProcessState>
    {
        /// <summary>
        /// Game instance
        /// </summary>
        private readonly Game game;
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
        /// <summary>
        /// Target combine drawer
        /// </summary>
        private BuiltInCombine builtInCombine = null;
        /// <summary>
        /// Post-process effect drawer
        /// </summary>
        private BuiltInPostProcess builtInPostProcess = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInPostProcessingDrawer(Game game)
        {
            this.game = game;

            InitializeBuffers();
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        private void InitializeBuffers()
        {
            var graphics = game.Graphics;
            var bufferManager = game.BufferManager;

            var screen = GeometryUtil.CreateScreen((int)graphics.Viewport.Width, (int)graphics.Viewport.Height);
            var vertices = VertexPositionTexture.Generate(screen.Vertices, screen.Uvs);

            indexBuffer = bufferManager.AddIndexData("Post processing index buffer", false, screen.Indices);
            vertexBuffer = bufferManager.AddVertexData("Post processing vertex buffer", false, vertices);

            bufferManager.CreateBuffers(nameof(BuiltInPostProcessingDrawer));
        }

        /// <inheritdoc/>
        public void Draw(IEngineDeviceContext dc, EngineShaderResourceView sourceTexture, int effect, BuiltInPostProcessState state)
        {
            builtInPostProcess ??= BuiltInShaders.GetDrawer<BuiltInPostProcess>(false);
            if (state != null) builtInPostProcess.UpdatePass(dc, state);
            builtInPostProcess.UpdateEffect(dc, sourceTexture, (BuiltInPostProcessEffects)effect);

            builtInPostProcess.Draw(dc, new DrawOptions
            {
                Topology = Topology.TriangleList,
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
            });
        }
        /// <inheritdoc/>
        public void Combine(IEngineDeviceContext dc, EngineShaderResourceView texture1, EngineShaderResourceView texture2)
        {
            builtInCombine ??= BuiltInShaders.GetDrawer<BuiltInCombine>(false);
            builtInCombine.Update(texture1, texture2);

            builtInCombine.Draw(dc, new DrawOptions
            {
                Topology = Topology.TriangleList,
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
            });
        }
        /// <inheritdoc/>
        public void Resize()
        {
            InitializeBuffers();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(BuiltInPostProcessingDrawer)}";
        }
    }
}
