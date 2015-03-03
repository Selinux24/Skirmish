using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using DepthStencilView = SharpDX.Direct3D11.DepthStencilView;
using DepthStencilViewDescription = SharpDX.Direct3D11.DepthStencilViewDescription;
using DepthStencilViewDimension = SharpDX.Direct3D11.DepthStencilViewDimension;
using DepthStencilViewFlags = SharpDX.Direct3D11.DepthStencilViewFlags;
using ResourceOptionFlags = SharpDX.Direct3D11.ResourceOptionFlags;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using ShaderResourceViewDescription = SharpDX.Direct3D11.ShaderResourceViewDescription;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Texture2DDescription = SharpDX.Direct3D11.Texture2DDescription;

namespace Engine
{
    public class ShadowMap : IDisposable
    {
        protected Game Game { get; private set; }
        public readonly int Width;
        public readonly int Height;
        public readonly Viewport Viewport;
        public DepthStencilView DepthMapDSV;
        public ShaderResourceView DepthMapSRV { get; set; }

        public ShadowMap(Game game, int width, int height)
        {
            this.Game = game;

            this.Width = width;
            this.Height = height;

            this.Viewport = new Viewport(0, 0, width, height, 0, 1.0f);

            using (Texture2D depthMap = new Texture2D(
                game.Graphics.Device,
                new Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R24G8_Typeless,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                }))
            {

                this.DepthMapDSV = new DepthStencilView(
                    game.Graphics.Device, 
                    depthMap,
                    new DepthStencilViewDescription
                    {
                        Flags = DepthStencilViewFlags.None,
                        Format = Format.D24_UNorm_S8_UInt,
                        Dimension = DepthStencilViewDimension.Texture2D,
                        Texture2D = new DepthStencilViewDescription.Texture2DResource()
                        {
                            MipSlice = 0,
                        },
                    });

                this.DepthMapSRV = new ShaderResourceView(
                    game.Graphics.Device, 
                    depthMap,
                    new ShaderResourceViewDescription
                    {
                        Format = Format.R24_UNorm_X8_Typeless,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                        {
                            MipLevels = 1,
                            MostDetailedMip = 0
                        },
                    });
            }
        }

        public void Dispose()
        {
            if (this.DepthMapDSV != null)
            {
                this.DepthMapDSV.Dispose();
                this.DepthMapDSV = null;
            }

            if (this.DepthMapSRV != null)
            {
                this.DepthMapSRV.Dispose();
                this.DepthMapSRV = null;
            }
        }
    }
}
