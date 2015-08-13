﻿using System;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using DepthStencilView = SharpDX.Direct3D11.DepthStencilView;
using DepthStencilViewDescription = SharpDX.Direct3D11.DepthStencilViewDescription;
using DepthStencilViewDimension = SharpDX.Direct3D11.DepthStencilViewDimension;
using DepthStencilViewFlags = SharpDX.Direct3D11.DepthStencilViewFlags;
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
    /// Light buffer
    /// </summary>
    public class LightBuffer : IDisposable
    {
        /// <summary>
        /// Game class
        /// </summary>
        protected Game Game { get; private set; }

        /// <summary>
        /// Buffer texture
        /// </summary>
        public ShaderResourceView Texture { get; protected set; }
        /// <summary>
        /// Render target
        /// </summary>
        public RenderTargetView RenderTarget { get; protected set; }
        /// <summary>
        /// Depth map
        /// </summary>
        public DepthStencilView DepthMap { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public LightBuffer(Game game)
        {
            this.Game = game;

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
            Format dbFormat = Format.D24_UNorm_S8_UInt;

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
                this.RenderTarget = new RenderTargetView(
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

                this.Texture = new ShaderResourceView(
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

            Texture2D depthMap = new Texture2D(
                this.Game.Graphics.Device,
                new Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = dbFormat,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });

            using (depthMap)
            {
                this.DepthMap = new DepthStencilView(
                    this.Game.Graphics.Device,
                    depthMap,
                    new DepthStencilViewDescription
                    {
                        Flags = DepthStencilViewFlags.None,
                        Format = dbFormat,
                        Dimension = DepthStencilViewDimension.Texture2D,
                        Texture2D = new DepthStencilViewDescription.Texture2DResource()
                        {
                            MipSlice = 0,
                        },
                    });
            }
        }
        /// <summary>
        /// Disposes all targets and depth buffer
        /// </summary>
        private void DisposeTargets()
        {
            if (this.RenderTarget != null)
            {
                this.RenderTarget.Dispose();
                this.RenderTarget = null;
            }

            if (this.Texture != null)
            {
                this.Texture.Dispose();
                this.Texture = null;
            }

            if (this.DepthMap != null)
            {
                this.DepthMap.Dispose();
                this.DepthMap = null;
            }
        }
    }
}
