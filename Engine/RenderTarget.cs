using SharpDX.DXGI;
using System;
using System.Linq;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Render target
    /// </summary>
    public class RenderTarget : IDisposable
    {
        /// <summary>
        /// Game class
        /// </summary>
        protected Game Game { get; private set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Render target format
        /// </summary>
        public Format RenderTargetFormat { get; protected set; }
        /// <summary>
        /// Buffer count
        /// </summary>
        public int BufferCount { get; protected set; }
        /// <summary>
        /// Use samples if available
        /// </summary>
        public bool UseSamples { get; protected set; }
        /// <summary>
        /// Buffer textures
        /// </summary>
        public EngineShaderResourceView[] Textures { get; protected set; }
        /// <summary>
        /// Buffer texture
        /// </summary>
        /// <remarks>Returns the first target texture, if any</remarks>
        public EngineShaderResourceView Texture { get => Textures?.FirstOrDefault(); }
        /// <summary>
        /// Render targets
        /// </summary>
        public EngineRenderTargetView Targets { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Name</param>
        /// <param name="format">Format</param>
        /// <param name="useSamples">Use samples if available</param>
        /// <param name="count">Buffer count</param>
        public RenderTarget(Game game, string name, Format format, bool useSamples, int count)
        {
            Game = game;
            Name = name;
            RenderTargetFormat = format;
            UseSamples = useSamples;
            BufferCount = count;

            CreateTargets();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~RenderTarget()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
                DisposeTargets();
            }
        }

        /// <summary>
        /// Resizes geometry buffer using render form size
        /// </summary>
        public void Resize()
        {
            DisposeTargets();
            CreateTargets();
        }

        /// <summary>
        /// Creates render targets, depth buffer and viewport
        /// </summary>
        private void CreateTargets()
        {
            int width = Game.Form.RenderWidth;
            int height = Game.Form.RenderHeight;

            var rt = Game.Graphics.CreateRenderTargetTexture(
                Name,
                RenderTargetFormat,
                width, height, BufferCount, UseSamples);

            Targets = rt.RenderTarget;
            Textures = rt.ShaderResources;
        }
        /// <summary>
        /// Disposes all targets and depth buffer
        /// </summary>
        private void DisposeTargets()
        {
            Targets?.Dispose();
            Targets = null;

            if (Textures.Length > 0)
            {
                for (int i = 0; i < Textures.Length; i++)
                {
                    Textures[i]?.Dispose();
                }
            }
            Textures = null;
        }
    }
}
