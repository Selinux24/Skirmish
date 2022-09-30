
namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.PostProcess;
    using Engine.Common;

    /// <summary>
    /// Post-processing drawer class
    /// </summary>
    public class PostProcessingDrawer
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
        /// <param name="sourceTexture">Source texture</param>
        /// <param name="state">State</param>
        public IBuiltInDrawer UpdateEffectParameters(EngineShaderResourceView sourceTexture, BuiltInPostProcessState state)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInPostProcess>();

            drawer.UpdatePass(sourceTexture, state);

            return drawer;
        }
        /// <summary>
        /// Draws the resulting light composition
        /// </summary>
        /// <param name="drawer">Drawer</param>
        public void Draw(IBuiltInDrawer drawer)
        {
            drawer.Draw(bufferManager, new DrawOptions
            {
                Topology = Topology.TriangleList,
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
            });
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
