using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using RenderTargetView = SharpDX.Direct3D11.RenderTargetView;
using RenderTargetViewDescription = SharpDX.Direct3D11.RenderTargetViewDescription;
using ResourceOptionFlags = SharpDX.Direct3D11.ResourceOptionFlags;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using ShaderResourceViewDescription = SharpDX.Direct3D11.ShaderResourceViewDescription;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Texture2DDescription = SharpDX.Direct3D11.Texture2DDescription;

namespace Engine
{
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
        /// <param name="count">Buffer count</param>
        public RenderTarget(Game game, int count)
        {
            this.Game = game;
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
            Format rtFormat = Format.R32G32B32A32_Float;

            this.Textures = new ShaderResourceView[this.BufferCount];
            this.Targets = new RenderTargetView[this.BufferCount];

            for (int i = 0; i < this.BufferCount; i++)
            {
                Texture2DDescription txDesc = new Texture2DDescription()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = rtFormat,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };

                var tex = new Texture2D(
                    this.Game.Graphics.Device,
                    new Texture2DDescription()
                    {
                        Width = width,
                        Height = height,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = rtFormat,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    });

                using (tex)
                {
                    this.Targets[i] = new RenderTargetView(
                        this.Game.Graphics.Device,
                        tex,
                        new RenderTargetViewDescription()
                        {
                            Format = rtFormat,
                            Dimension = SharpDX.Direct3D11.RenderTargetViewDimension.Texture2D,
                            Texture2D = new RenderTargetViewDescription.Texture2DResource()
                            {
                                MipSlice = 0,
                            },
                        });

                    ShaderResourceViewDescription srDesc = new ShaderResourceViewDescription()
                    {
                        Format = rtFormat,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                        {
                            MostDetailedMip = 0,
                            MipLevels = 1,
                        },
                    };

                    this.Textures[i] = new ShaderResourceView(
                        this.Game.Graphics.Device,
                        tex,
                        new ShaderResourceViewDescription()
                        {
                            Format = rtFormat,
                            Dimension = ShaderResourceViewDimension.Texture2D,
                            Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                            {
                                MostDetailedMip = 0,
                                MipLevels = 1,
                            },
                        });
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
