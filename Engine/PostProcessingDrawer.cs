using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
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
        /// Post processing drawer
        /// </summary>
        private readonly IDrawerPostProcess drawer;
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
        /// Current drawer
        /// </summary>
        private EngineEffectTechnique currentTechnique;
        /// <summary>
        /// Current input layout
        /// </summary>
        private InputLayout currentInputLayout;
        /// <summary>
        /// Layout dictionary
        /// </summary>
        private Dictionary<EngineEffectTechnique, InputLayout> layouts = new Dictionary<EngineEffectTechnique, InputLayout>();

        /// <summary>
        /// Constructor
        /// </summary>
        public PostProcessingDrawer(Graphics graphics, IDrawerPostProcess drawer)
        {
            this.graphics = graphics;
            this.drawer = drawer;

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
        /// Sets the effect to the post processing helper class
        /// </summary>
        /// <param name="effect">Effect</param>
        private void SetEffect(PostProcessingEffects effect)
        {
            var effectTechnique = drawer.GetTechnique(effect);

            if (currentTechnique == effectTechnique)
            {
                return;
            }

            currentTechnique = effectTechnique;

            if (effectTechnique == null)
            {
                currentInputLayout = null;

                return;
            }

            if (!layouts.ContainsKey(effectTechnique))
            {
                var layout = graphics.CreateInputLayout(effectTechnique.GetSignature(), VertexPositionTexture.Input(BufferSlot));

                layouts.Add(effectTechnique, layout);

                currentInputLayout = layout;
            }
            else
            {
                currentInputLayout = layouts[effectTechnique];
            }
        }
        /// <summary>
        /// Updates the effect parameters
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="texture">Source texture</param>
        public void UpdateEffectEmpty(Scene scene, EngineShaderResourceView texture)
        {
            SetEffect(PostProcessingEffects.None);

            var viewProj = scene.Game.Form.GetOrthoProjectionMatrix();

            drawer.UpdatePerFrameEmpty(
                viewProj,
                texture);
        }
        /// <summary>
        /// Updates the effect parameters
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        public void UpdateEffectCombine(Scene scene, EngineShaderResourceView texture1, EngineShaderResourceView texture2)
        {
            SetEffect(PostProcessingEffects.Combine);

            var viewProj = scene.Game.Form.GetOrthoProjectionMatrix();

            drawer.UpdatePerFrameCombine(
                viewProj,
                texture1,
                texture2);
        }
        /// <summary>
        /// Updates the effect parameters
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="totalSeconds">Total seconds</param>
        /// <param name="texture">Source texture</param>
        /// <param name="effect">Effect</param>
        /// <param name="parameters">Parameters</param>
        public void UpdateEffectParameters(Scene scene, float totalSeconds, EngineShaderResourceView texture, PostProcessingEffects effect, IDrawerPostProcessParams parameters)
        {
            SetEffect(effect);

            var forms = scene.Game.Form;
            var viewProj = forms.GetOrthoProjectionMatrix();
            var screenRect = forms.RenderRectangle;

            drawer.UpdatePerFrame(
                viewProj,
                new Vector2(screenRect.Width, screenRect.Height),
                totalSeconds,
                texture);

            drawer.UpdatePerEffect(parameters);
        }
        /// <summary>
        /// Binds the result box input layout to the input assembler
        /// </summary>
        public void Bind()
        {
            if (currentTechnique == null)
            {
                return;
            }

            graphics.IAPrimitiveTopology = PrimitiveTopology.TriangleList;
            graphics.IASetVertexBuffers(BufferSlot, vertexBufferBinding);
            graphics.IASetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

            graphics.IAInputLayout = currentInputLayout;
        }
        /// <summary>
        /// Draws the resulting light composition
        /// </summary>
        public void Draw()
        {
            if (currentTechnique == null)
            {
                return;
            }

            for (int p = 0; p < currentTechnique.PassCount; p++)
            {
                graphics.EffectPassApply(currentTechnique, p, 0);

                graphics.DrawIndexed(indexCount, 0, 0);
            }
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
