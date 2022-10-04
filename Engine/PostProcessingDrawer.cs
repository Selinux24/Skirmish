
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
        /// Render helper geometry buffer slot
        /// </summary>
        public static int BufferSlot { get; set; } = 0;

        /// <summary>
        /// Graphics class
        /// </summary>
        private readonly Graphics graphics;
        /// <summary>
        /// Buffer manager
        /// </summary>
        private readonly BufferManager bufferManager;
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public PostProcessingDrawer(Graphics graphics, BufferManager bufferManager)
        {
            this.graphics = graphics;
            this.bufferManager = bufferManager;

            InitializeBuffers();
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        private void InitializeBuffers()
        {
            var screen = GeometryUtil.CreateScreen((int)graphics.Viewport.Width, (int)graphics.Viewport.Height);
            indexBuffer = bufferManager.AddIndexData("Post processing index buffer", false, screen.Indices);
            var vertices = VertexPositionTexture.Generate(screen.Vertices, screen.Uvs);
            vertexBuffer = bufferManager.AddVertexData("Post processing vertex buffer", false, vertices);
        }

        /// <inheritdoc/>
        public IBuiltInDrawer UpdateEffectCombine(EngineShaderResourceView texture1, EngineShaderResourceView texture2)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInCombine>();

            drawer.Update(texture1, texture2);

            return drawer;
        }
        /// <inheritdoc/>
        public IBuiltInDrawer UpdateEffectParameters(BuiltInPostProcessState state)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInPostProcess>();

            drawer.UpdatePass(null, state);

            return drawer;
        }
        /// <inheritdoc/>
        public IBuiltInDrawer UpdateEffect(EngineShaderResourceView sourceTexture, BuiltInPostProcessEffects effect)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInPostProcess>();

            //drawer.UpdatePass(sourceTexture, state)

            return drawer;
        }
        /// <inheritdoc/>
        public void Draw(IBuiltInDrawer drawer)
        {
            drawer.Draw(bufferManager, new DrawOptions
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
