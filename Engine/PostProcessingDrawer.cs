
namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.PostProcess;
    using Engine.Common;

    /// <summary>
    /// Post-processing drawer class
    /// </summary>
    public class PostProcessingDrawer : IPostProcessingDrawer
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
        public PostProcessingDrawer(Game game)
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

            bufferManager.CreateBuffers(nameof(PostProcessingDrawer));
        }

        /// <inheritdoc/>
        public void Draw(IEngineDeviceContext dc, EngineShaderResourceView sourceTexture, BuiltInPostProcessEffects effect, BuiltInPostProcessState state)
        {
            builtInPostProcess ??= BuiltInShaders.GetDrawer<BuiltInPostProcess>(false);
            if (state != null) builtInPostProcess.UpdatePass(dc, state);
            builtInPostProcess.UpdateEffect(dc, sourceTexture, effect);

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
            return $"{nameof(PostProcessingDrawer)}";
        }
    }
}
