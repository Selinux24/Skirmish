using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using RenderTargetView = SharpDX.Direct3D11.RenderTargetView;
using RenderTargetViewDescription = SharpDX.Direct3D11.RenderTargetViewDescription;
using RenderTargetViewDimension = SharpDX.Direct3D11.RenderTargetViewDimension;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using ShaderResourceViewDescription = SharpDX.Direct3D11.ShaderResourceViewDescription;

namespace Engine
{
    using Engine.Helpers;

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
        /// Render target format
        /// </summary>
        public Format RenderTargetFormat { get; protected set; }
        /// <summary>
        /// Buffer count
        /// </summary>
        public int BufferCount { get; protected set; }
        /// <summary>
        /// Buffer textures
        /// </summary>
        public ShaderResourceView[] Textures { get; protected set; }
        /// <summary>
        /// Render targets
        /// </summary>
        public RenderTargetView[] Targets { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="format">Format</param>
        /// <param name="count">Buffer count</param>
        public RenderTarget(Game game, Format format, int count)
        {
            this.Game = game;
            this.RenderTargetFormat = format;
            this.BufferCount = count;

            this.CreateTargets();
        }
        /// <summary>
        /// Release of resources
        /// </summary>
        public void Dispose()
        {
            this.DisposeTargets();
        }
        /// <summary>
        /// Resizes geometry buffer using render form size
        /// </summary>
        public void Resize()
        {
            this.DisposeTargets();
            this.CreateTargets();
        }

        /// <summary>
        /// Creates render targets, depth buffer and viewport
        /// </summary>
        private void CreateTargets()
        {
            int width = this.Game.Form.RenderWidth;
            int height = this.Game.Form.RenderHeight;

            this.Textures = new ShaderResourceView[this.BufferCount];
            this.Targets = new RenderTargetView[this.BufferCount];

            for (int i = 0; i < this.BufferCount; i++)
            {
                var tex = this.Game.Graphics.Device.CreateRenderTargetTexture(this.RenderTargetFormat, width, height);

                var rtvDesc = new RenderTargetViewDescription()
                {
                    Format = this.RenderTargetFormat,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource()
                    {
                        MipSlice = 0,
                    },
                };

                var srvDesc = new ShaderResourceViewDescription()
                {
                    Format = this.RenderTargetFormat,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                    {
                        MostDetailedMip = 0,
                        MipLevels = 1,
                    },
                };

                using (tex)
                {
                    this.Targets[i] = new RenderTargetView(this.Game.Graphics.Device, tex, rtvDesc);

                    this.Textures[i] = new ShaderResourceView(this.Game.Graphics.Device, tex, srvDesc);
                }
            }
        }
        /// <summary>
        /// Disposes all targets and depth buffer
        /// </summary>
        private void DisposeTargets()
        {
            Helper.Dispose(this.Targets);
            Helper.Dispose(this.Textures);
        }
    }
}
