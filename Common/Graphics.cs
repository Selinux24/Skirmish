using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Windows;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using BlendOperation = SharpDX.Direct3D11.BlendOperation;
using BlendOption = SharpDX.Direct3D11.BlendOption;
using BlendState = SharpDX.Direct3D11.BlendState;
using BlendStateDescription = SharpDX.Direct3D11.BlendStateDescription;
using ColorWriteMaskFlags = SharpDX.Direct3D11.ColorWriteMaskFlags;
using Comparison = SharpDX.Direct3D11.Comparison;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using CullMode = SharpDX.Direct3D11.CullMode;
using DepthStencilClearFlags = SharpDX.Direct3D11.DepthStencilClearFlags;
using DepthStencilOperationDescription = SharpDX.Direct3D11.DepthStencilOperationDescription;
using DepthStencilState = SharpDX.Direct3D11.DepthStencilState;
using DepthStencilStateDescription = SharpDX.Direct3D11.DepthStencilStateDescription;
using DepthStencilView = SharpDX.Direct3D11.DepthStencilView;
using DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using DeviceCreationFlags = SharpDX.Direct3D11.DeviceCreationFlags;
using FillMode = SharpDX.Direct3D11.FillMode;
using RasterizerState = SharpDX.Direct3D11.RasterizerState;
using RasterizerStateDescription = SharpDX.Direct3D11.RasterizerStateDescription;
using RenderTargetView = SharpDX.Direct3D11.RenderTargetView;
using Resource = SharpDX.Direct3D11.Resource;
using ResourceOptionFlags = SharpDX.Direct3D11.ResourceOptionFlags;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using StencilOperation = SharpDX.Direct3D11.StencilOperation;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Texture2DDescription = SharpDX.Direct3D11.Texture2DDescription;

namespace Common
{
    public class Graphics : IDisposable
    {
        private Device device = null;
        private bool vsyncEnabled = false;
        private int msCount = 1;
        private int msQuality = 0;
        private SwapChain swapChain = null;
        private RenderTargetView renderTargetView = null;
        private Texture2D depthStencilBuffer = null;
        private DepthStencilView depthStencilView = null;

        private RasterizerState rasterizerDefault = null;
        private RasterizerState rasterizerWireframe = null;
        private RasterizerState rasterizerNoCull = null;
        private BlendState blendAlphaToCoverage = null;
        private BlendState blendTransparent = null;
        private DepthStencilState depthStencilzBufferEnabled = null;
        private DepthStencilState depthStencilzBufferDisabled = null;

        protected Format BufferFormat = Format.R8G8B8A8_UNorm;
        protected Format DepthFormat = Format.D24_UNorm_S8_UInt;

        public Device Device
        {
            get
            {
                return this.device;
            }
        }
        public DeviceContext DeviceContext
        {
            get
            {
                return this.device.ImmediateContext;
            }
        }
        public bool FullScreen
        {
            get
            {
                return this.swapChain.IsFullScreen;
            }
        }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Graphics(RenderForm form, bool vsync, bool fullscreen, int refreshRate = 0, int multiSampleCount = 1)
        {
            this.vsyncEnabled = vsync;

            ModeDescription displayMode = this.FindModeDescription(form, vsync, fullscreen, this.BufferFormat, refreshRate);

            using (Device tmpDevice = new Device(DriverType.Hardware))
            {
                int quality = tmpDevice.CheckMultisampleQualityLevels(this.BufferFormat, multiSampleCount);
                if (quality > 0)
                {
                    this.msCount = multiSampleCount;
                    this.msQuality = quality - 1;
                }
            }

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.None,
                new[] { FeatureLevel.Level_11_0 },
                new SwapChainDescription()
                {
                    BufferCount = 1,
                    ModeDescription = displayMode,
                    Usage = Usage.RenderTargetOutput,
                    OutputHandle = form.Handle,
                    SampleDescription = new SampleDescription(this.msCount, this.msQuality),
                    IsWindowed = !fullscreen,
                    SwapEffect = SwapEffect.Discard,
                    Flags = SwapChainFlags.None,
                },
                out this.device,
                out this.swapChain);

            this.PrepareDevice(form, false);

            #region Alt + Enter

            using (Factory factory = swapChain.GetParent<Factory>())
            {
                factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);
            }

            form.KeyDown += (sender, eventArgs) =>
            {
                if (eventArgs.Alt && eventArgs.KeyCode == Keys.Enter)
                {
                    swapChain.IsFullScreen = !swapChain.IsFullScreen;
                }
            };

            #endregion
        }
        public ModeDescription FindModeDescription(RenderForm form, bool vSync, bool fullScreen, Format format, int refreshRate = 0)
        {
            int width;
            int height;
            if (fullScreen)
            {
                width = form.Size.Width;
                height = form.Size.Height;
            }
            else
            {
                width = form.ClientSize.Width;
                height = form.ClientSize.Height;
            }

            using (Factory factory = new Factory())
            {
                using (Adapter adapter = factory.GetAdapter(0))
                {
                    using (Output adapterOutput = adapter.GetOutput(0))
                    {
                        ModeDescription[] displayModeList = adapterOutput.GetDisplayModeList(
                            format,
                            DisplayModeEnumerationFlags.Interlaced);

                        ModeDescription[] displayModes = Array.FindAll(displayModeList, d =>
                            d.Format == format &&
                            d.Width == width &&
                            d.Height == height &&
                            (refreshRate == 0 || (d.RefreshRate.Numerator / d.RefreshRate.Denominator) + 1 == refreshRate));

                        if (displayModes.Length > 0)
                        {
                            ModeDescription result = displayModes[0];

                            if (!vSync) result.RefreshRate = new Rational(0, 1);

                            return result;
                        }
                    }
                }
            }

            return new ModeDescription()
            {
                Width = form.ClientSize.Width,
                Height = form.ClientSize.Height,
                Format = format,
                RefreshRate = new Rational(0, 1),
                ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
                Scaling = DisplayModeScaling.Unspecified,
            };
        }
        public void PrepareDevice(RenderForm form, bool resizing)
        {
            #region Liberar recursos

            if (this.renderTargetView != null)
            {
                this.renderTargetView.Dispose();
            }

            if (this.depthStencilBuffer != null)
            {
                this.depthStencilBuffer.Dispose();
            }

            if (this.depthStencilView != null)
            {
                this.depthStencilView.Dispose();
            }

            if (this.depthStencilzBufferEnabled != null)
            {
                this.depthStencilzBufferEnabled.Dispose();
            }

            if (this.depthStencilzBufferDisabled != null)
            {
                this.depthStencilzBufferDisabled.Dispose();
            }

            if (this.rasterizerDefault != null)
            {
                this.rasterizerDefault.Dispose();
            }

            if (this.rasterizerWireframe != null)
            {
                this.rasterizerWireframe.Dispose();
            }

            if (this.rasterizerNoCull != null)
            {
                this.rasterizerNoCull.Dispose();
            }

            if (this.blendAlphaToCoverage != null)
            {
                this.blendAlphaToCoverage.Dispose();
            }

            if (this.blendTransparent != null)
            {
                this.blendTransparent.Dispose();
            }

            #endregion

            if (resizing)
            {
                this.swapChain.ResizeBuffers(2, 0, 0, Format.Unknown, SwapChainFlags.AllowModeSwitch);
            }

            if (this.swapChain.IsFullScreen)
            {
                this.Width = form.Size.Width;
                this.Height = form.Size.Height;
            }
            else
            {
                this.Width = form.ClientSize.Width;
                this.Height = form.ClientSize.Height;
            }

            #region RenderTarget

            using (Resource resource = Resource.FromSwapChain<Texture2D>(swapChain, 0))
            {
                this.renderTargetView = new RenderTargetView(this.Device, resource);
            }

            #endregion

            #region DepthBuffers

            this.depthStencilBuffer = new Texture2D(
                this.device,
                new Texture2DDescription()
                {
                    Width = this.Width,
                    Height = this.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = this.DepthFormat,
                    SampleDescription = new SampleDescription(this.msCount, this.msQuality),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                });

            this.depthStencilView = new DepthStencilView(
                this.device,
                this.depthStencilBuffer);

            this.depthStencilzBufferEnabled = new DepthStencilState(
                this.device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less,

                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,

                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },

                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },
                });

            this.depthStencilzBufferDisabled = new DepthStencilState(
                this.device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less,

                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,

                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },

                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },
                });

            #endregion

            #region Rasterizers

            this.rasterizerDefault = new RasterizerState(
                this.device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = true,
                    IsMultisampleEnabled = true,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            this.rasterizerWireframe = new RasterizerState(
                this.device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Wireframe,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = true,
                    IsMultisampleEnabled = true,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            this.rasterizerNoCull = new RasterizerState(
                this.device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = true,
                    IsMultisampleEnabled = true,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            #endregion

            #region Blends

            BlendStateDescription alphaToCoverageDesc = new BlendStateDescription();
            alphaToCoverageDesc.AlphaToCoverageEnable = true;
            alphaToCoverageDesc.IndependentBlendEnable = false;
            alphaToCoverageDesc.RenderTarget[0].IsBlendEnabled = false;
            alphaToCoverageDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            this.blendAlphaToCoverage = new BlendState(
                device,
                alphaToCoverageDesc);

            BlendStateDescription transparentDesc = new BlendStateDescription();
            alphaToCoverageDesc.AlphaToCoverageEnable = false;
            alphaToCoverageDesc.IndependentBlendEnable = false;
            alphaToCoverageDesc.RenderTarget[0].IsBlendEnabled = true;
            alphaToCoverageDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            alphaToCoverageDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            alphaToCoverageDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            alphaToCoverageDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            alphaToCoverageDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            alphaToCoverageDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            alphaToCoverageDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            this.blendTransparent = new BlendState(
                device,
                transparentDesc);

            #endregion

            #region ViewPort

            this.device.ImmediateContext.Rasterizer.SetViewport(new ViewportF()
            {
                X = 0,
                Y = 0,
                Width = this.Width,
                Height = this.Height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            });

            #endregion

            this.Device.ImmediateContext.Rasterizer.State = this.rasterizerDefault;
            this.Device.ImmediateContext.OutputMerger.BlendState = this.blendAlphaToCoverage;
            this.Device.ImmediateContext.OutputMerger.SetDepthStencilState(this.depthStencilzBufferEnabled);
            this.Device.ImmediateContext.OutputMerger.SetTargets(this.depthStencilView, this.renderTargetView);
        }
        public void Begin()
        {
            this.SetDefaultRasterizer();
            this.SetBlendAlphaToCoverage();
            this.EnableZBuffer();

            this.DeviceContext.ClearRenderTargetView(
                this.renderTargetView, 
                GameEnvironment.Background);

            this.DeviceContext.ClearDepthStencilView(
                this.depthStencilView,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f,
                0);
        }
        public void End()
        {
            if (this.vsyncEnabled)
            {
                this.swapChain.Present(1, 0);
            }
            else
            {
                this.swapChain.Present(0, 0);
            }
        }
        public void EnableZBuffer()
        {
            this.Device.ImmediateContext.OutputMerger.SetDepthStencilState(this.depthStencilzBufferEnabled);
        }
        public void DisableZBuffer()
        {
            this.Device.ImmediateContext.OutputMerger.SetDepthStencilState(this.depthStencilzBufferDisabled);
        }
        public void SetDefaultRasterizer()
        {
            this.Device.ImmediateContext.Rasterizer.State = this.rasterizerDefault;
        }
        public void SetWireframeRasterizer()
        {
            this.Device.ImmediateContext.Rasterizer.State = this.rasterizerWireframe;
        }
        public void SetNoCullRasterizer()
        {
            this.Device.ImmediateContext.Rasterizer.State = this.rasterizerNoCull;
        }
        public void SetBlendAlphaToCoverage()
        {
            this.Device.ImmediateContext.OutputMerger.BlendState = this.blendAlphaToCoverage;
        }
        public void SetBlendTransparent()
        {
            this.Device.ImmediateContext.OutputMerger.BlendState = this.blendTransparent;
        }
        public void Dispose()
        {
            if (this.swapChain != null)
            {
                this.swapChain.SetFullscreenState(false, null);
            }

            if (this.rasterizerDefault != null)
            {
                this.rasterizerDefault.Dispose();
            }

            if (this.rasterizerWireframe != null)
            {
                this.rasterizerWireframe.Dispose();
            }

            if (this.rasterizerNoCull != null)
            {
                this.rasterizerNoCull.Dispose();
            }

            if (this.blendAlphaToCoverage != null)
            {
                this.blendAlphaToCoverage.Dispose();
            }

            if (this.blendTransparent != null)
            {
                this.blendTransparent.Dispose();
            }

            if (this.depthStencilView != null)
            {
                this.depthStencilView.Dispose();
                this.depthStencilView = null;
            }

            if (this.depthStencilzBufferEnabled != null)
            {
                this.depthStencilzBufferEnabled.Dispose();
                this.depthStencilzBufferEnabled = null;
            }

            if (this.depthStencilzBufferDisabled != null)
            {
                this.depthStencilzBufferDisabled.Dispose();
                this.depthStencilzBufferDisabled = null;
            }

            if (this.renderTargetView != null)
            {
                this.renderTargetView.Dispose();
                this.renderTargetView = null;
            }

            if (this.depthStencilBuffer != null)
            {
                this.depthStencilBuffer.Dispose();
                this.depthStencilBuffer = null;
            }

            if (this.device != null)
            {
                this.device.Dispose();
                this.device = null;
            }

            if (this.swapChain != null)
            {
                this.swapChain.Dispose();
                this.swapChain = null;
            }
        }
    }
}
