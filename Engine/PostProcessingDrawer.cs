﻿
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

            bufferManager.CreateBuffers();
        }

        /// <inheritdoc/>
        public IBuiltInDrawer UpdateEffectCombine(EngineDeviceContext dc, EngineShaderResourceView texture1, EngineShaderResourceView texture2)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInCombine>();

            drawer.Update(texture1, texture2);

            return drawer;
        }
        /// <inheritdoc/>
        public IBuiltInDrawer UpdateEffectParameters(EngineDeviceContext dc, BuiltInPostProcessState state)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInPostProcess>();

            drawer.UpdatePass(dc, state);

            return drawer;
        }
        /// <inheritdoc/>
        public IBuiltInDrawer UpdateEffect(EngineDeviceContext dc, EngineShaderResourceView sourceTexture, BuiltInPostProcessEffects effect)
        {
            var drawer = BuiltInShaders.GetDrawer<BuiltInPostProcess>();

            drawer.UpdateEffect(dc, sourceTexture, effect);

            return drawer;
        }
        /// <inheritdoc/>
        public void Draw(EngineDeviceContext dc, IBuiltInDrawer drawer)
        {
            var bufferManager = game.BufferManager;

            drawer.Draw(dc, bufferManager, new DrawOptions
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
