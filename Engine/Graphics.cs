using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
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
using DepthStencilViewDescription = SharpDX.Direct3D11.DepthStencilViewDescription;
using DepthStencilViewDimension = SharpDX.Direct3D11.DepthStencilViewDimension;
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

namespace Engine
{
    using Engine.Helpers;

    /// <summary>
    /// Graphics class
    /// </summary>
    public class Graphics : IDisposable
    {
        /// <summary>
        /// On resized event
        /// </summary>
        public event EventHandler Resized;

        /// <summary>
        /// Vertical sync enabled
        /// </summary>
        private bool vsyncEnabled = false;
        /// <summary>
        /// Multisample count
        /// </summary>
        private int msCount = 1;
        /// <summary>
        /// Multisample quality
        /// </summary>
        private int msQuality = 0;
        /// <summary>
        /// Swap chain
        /// </summary>
        private SwapChain swapChain = null;
        /// <summary>
        /// Render target view
        /// </summary>
        private RenderTargetView renderTargetView = null;
        /// <summary>
        /// Depth stencil buffer
        /// </summary>
        private Texture2D depthStencilBuffer = null;
        /// <summary>
        /// Depth stencil view
        /// </summary>
        private DepthStencilView depthStencilView = null;

        /// <summary>
        /// Current depth-stencil state
        /// </summary>
        private DepthStencilState currentDepthStencilState = null;
        /// <summary>
        /// Current blend state
        /// </summary>
        private BlendState currentBlendState = null;
        /// <summary>
        /// Current rasterizer state
        /// </summary>
        private RasterizerState currentRasterizerState = null;

        /// <summary>
        /// Depth stencil state with z-buffer enabled
        /// </summary>
        private DepthStencilState depthStencilzBufferEnabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer disabled
        /// </summary>
        private DepthStencilState depthStencilzBufferDisabled = null;
        /// <summary>
        /// No depth stencil
        /// </summary>
        private DepthStencilState depthStencilNone = null;
        /// <summary>
        /// Blend state for alpha blending
        /// </summary>
        private BlendState blendAlphaToCoverage = null;
        /// <summary>
        /// Blend state for transparent blending
        /// </summary>
        private BlendState blendTransparent = null;
        /// <summary>
        /// Blend state for additive blending
        /// </summary>
        private BlendState blendAdditive = null;
        /// <summary>
        /// Default rasterizer
        /// </summary>
        private RasterizerState rasterizerDefault = null;
        /// <summary>
        /// Wireframe rasterizer
        /// </summary>
        private RasterizerState rasterizerWireframe = null;
        /// <summary>
        /// No-cull rasterizer
        /// </summary>
        private RasterizerState rasterizerNoCull = null;
        /// <summary>
        /// Sets cull counter-clockwise face rasterizer
        /// </summary>
        private RasterizerState rasterizerCullCounterClockwiseFace = null;

        /// <summary>
        /// Back buffer format
        /// </summary>
        protected Format BufferFormat = Texture2DFormats.R8G8B8A8_UNorm;
        /// <summary>
        /// Depth buffer format
        /// </summary>
        protected Format DepthFormat = BackBufferFormats.D24_UNorm_S8_UInt;

        /// <summary>
        /// Graphics device
        /// </summary>
        public Device Device { get; private set; }
        /// <summary>
        /// Graphics inmmediate context
        /// </summary>
        public DeviceContext DeviceContext { get; private set; }
        /// <summary>
        /// Device description
        /// </summary>
        public readonly string DeviceDescription = null;
        /// <summary>
        /// Screen viewport
        /// </summary>
        public ViewportF Viewport { get; private set; }

        /// <summary>
        /// Gets desktop mode description
        /// </summary>
        /// <returns>Returns current desktop mode description</returns>
        public static OutputDescription GetDesktopMode()
        {
            using (Factory1 factory = new Factory1())
            {
                using (Adapter1 adapter = factory.GetAdapter1(0))
                {
                    using (Output adapterOutput = adapter.GetOutput(0))
                    {
                        return adapterOutput.Description;
                    }
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="form">Game form</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampleCount">Multisample count</param>
        public Graphics(EngineForm form, bool vsyncEnabled = false, int refreshRate = 0, int multiSampleCount = 0)
        {
            Adapter1 adapter = null;
            ModeDescription displayMode = this.FindModeDescription(
                this.BufferFormat,
                form.RenderWidth,
                form.RenderHeight,
                form.IsFullscreen,
                refreshRate,
                out adapter);

            using (adapter)
            {
                this.vsyncEnabled = vsyncEnabled && displayMode.RefreshRate != new Rational(0, 1);

                using (Device tmpDevice = new Device(adapter))
                {
                    int quality = tmpDevice.CheckMultisampleQualityLevels(this.BufferFormat, multiSampleCount);
                    if (quality > 0)
                    {
                        this.msCount = multiSampleCount;
                        this.msQuality = quality - 1;
                    }
                }

                DeviceCreationFlags creationFlags = DeviceCreationFlags.None;

#if DEBUG
                creationFlags |= DeviceCreationFlags.Debug;
#endif

                Device device = null;
                Device.CreateWithSwapChain(
                    adapter,
                    creationFlags,
                    new[] 
                    {
                        FeatureLevel.Level_11_0,
                        FeatureLevel.Level_10_1,
                        FeatureLevel.Level_10_0,
                        /* Windows 10 debugging opts error
                        FeatureLevel.Level_9_3,
                        FeatureLevel.Level_9_2, 
                        FeatureLevel.Level_9_1,
                        */
                    },
                    new SwapChainDescription()
                    {
                        BufferCount = 1,
                        ModeDescription = displayMode,
                        Usage = Usage.RenderTargetOutput,
                        OutputHandle = form.Handle,
                        SampleDescription = new SampleDescription(this.msCount, this.msQuality),
                        IsWindowed = !form.IsFullscreen,
                        SwapEffect = SwapEffect.Discard,
                        Flags = SwapChainFlags.None,
                    },
                    out device,
                    out this.swapChain);

                this.Device = device;
                this.DeviceContext = device.ImmediateContext;
                this.DeviceDescription = string.Format(
                    "{0}",
                    adapter.Description1.Description);
            }

            this.PrepareDevice(displayMode.Width, displayMode.Height, false);

            #region Alt + Enter

            using (Factory factory = this.swapChain.GetParent<Factory>())
            {
                factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);
            }

            form.KeyUp += (sender, eventArgs) =>
            {
                if (eventArgs.Alt && (int)eventArgs.KeyCode == (int)Keys.Enter)
                {
                    this.swapChain.IsFullScreen = !this.swapChain.IsFullScreen;
                }
            };

            #endregion
        }
        /// <summary>
        /// Prepare device
        /// </summary>
        /// <param name="width">Render width</param>
        /// <param name="height">Render height</param>
        /// <param name="resizing">Sets whether the render screen is resizing or not</param>
        public void PrepareDevice(int width, int height, bool resizing)
        {
            if (resizing)
            {
                this.DisposeResources();

                this.swapChain.ResizeBuffers(2, width, height, this.BufferFormat, SwapChainFlags.None);
            }

            #region Render Target

            using (Resource backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0))
            {
                this.renderTargetView = new RenderTargetView(this.Device, backBuffer);
            }

            #endregion

            #region Depth Stencil Buffer and View

            this.depthStencilBuffer = new Texture2D(
                this.Device,
                new Texture2DDescription()
                {
                    Width = width,
                    Height = height,
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
                this.Device,
                this.depthStencilBuffer,
                new DepthStencilViewDescription()
                {
                    Format = this.DepthFormat,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0
                    }
                });

            #endregion

            #region Depth Stencil States

            this.depthStencilzBufferEnabled = new DepthStencilState(
                this.Device,
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
                this.Device,
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

            this.depthStencilNone = new DepthStencilState(
                this.Device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Always,
                });

            #endregion

            #region Rasterizers

            this.rasterizerDefault = new RasterizerState(
                this.Device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            this.rasterizerWireframe = new RasterizerState(
                this.Device,
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
                this.Device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            this.rasterizerCullCounterClockwiseFace = new RasterizerState(
                this.Device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = true,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            #endregion

            #region Blend states

            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = true;
                desc.IndependentBlendEnable = false;
                desc.RenderTarget[0].IsBlendEnabled = false;
                desc.RenderTarget[0].SourceBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                this.blendAlphaToCoverage = new BlendState(this.Device, desc);
            }

            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;
                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                this.blendTransparent = new BlendState(this.Device, desc);
            }

            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;
                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.One;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                this.blendAdditive = new BlendState(this.Device, desc);
            }

            #endregion

            #region Viewport

            this.Viewport = new ViewportF()
            {
                X = 0,
                Y = 0,
                Width = width,
                Height = height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };

            #endregion

            #region Attach to Inmmediate Context

            this.DeviceContext.OutputMerger.SetDepthStencilState(this.depthStencilzBufferEnabled);
            this.DeviceContext.OutputMerger.SetTargets(this.depthStencilView, this.renderTargetView);
            this.DeviceContext.OutputMerger.SetBlendState(this.blendAlphaToCoverage, Color.Transparent, -1);

            this.DeviceContext.Rasterizer.State = this.rasterizerDefault;
            this.DeviceContext.Rasterizer.SetViewport(this.Viewport);

            #endregion

            if (resizing)
            {
                if (this.Resized != null)
                {
                    this.Resized(this, new EventArgs());
                }
            }
        }
        /// <summary>
        /// Begin frame
        /// </summary>
        public void Begin()
        {
            this.DeviceContext.ClearDepthStencilView(
                this.depthStencilView,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f,
                0);

            this.DeviceContext.ClearRenderTargetView(
                this.renderTargetView,
                GameEnvironment.Background);
        }
        /// <summary>
        /// End frame
        /// </summary>
        public void End()
        {
            if (this.vsyncEnabled)
            {
                this.swapChain.Present(1, PresentFlags.None);
            }
            else
            {
                this.swapChain.Present(0, PresentFlags.None);
            }
        }
        /// <summary>
        /// Sets default render target
        /// </summary>
        /// <param name="clear">Indicates whether the target and stencil buffer must be cleared</param>
        public void SetDefaultRenderTarget(bool clear)
        {
            this.SetRenderTarget(this.Viewport, this.depthStencilView, this.renderTargetView, clear, GameEnvironment.Background);
        }
        /// <summary>
        /// Set render target
        /// </summary>
        /// <param name="viewport">Viewport</param>
        /// <param name="depthMap">Depth map</param>
        /// <param name="renderTarget">Render target</param>
        /// <param name="clear">Indicates whether the target and stencil buffer must be cleared</param>
        /// <param name="clearColor">Clear color</param>
        /// <param name="depthClearFlags">Depth cleraring flags</param>
        /// <remarks>By default, depth clearing flags were "Depth" and "Stencil"</remarks>
        public void SetRenderTarget(Viewport viewport, DepthStencilView depthMap, RenderTargetView renderTarget, bool clear, Color4 clearColor, DepthStencilClearFlags depthClearFlags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil)
        {
            this.DeviceContext.Rasterizer.SetViewport(viewport);
            this.DeviceContext.OutputMerger.SetTargets(depthMap, renderTarget);

            if (clear)
            {
                if (renderTarget != null)
                {
                    this.DeviceContext.ClearRenderTargetView(
                        renderTarget,
                        clearColor);
                }

                if (depthMap != null)
                {
                    this.DeviceContext.ClearDepthStencilView(
                        depthMap,
                        depthClearFlags,
                        1.0f, 0);
                }
            }
        }
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="viewport">Viewport</param>
        /// <param name="depthMap">Depth map</param>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clear">Indicates whether the target and stencil buffer must be cleared</param>
        /// <param name="clearColor">Clear color</param>
        /// <param name="depthClearFlags">Depth cleraring flags</param>
        /// <remarks>By default, depth clearing flags were "Depth" and "Stencil"</remarks>
        public void SetRenderTargets(Viewport viewport, DepthStencilView depthMap, RenderTargetView[] renderTargets, bool clear, Color4 clearColor, DepthStencilClearFlags depthClearFlags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil)
        {
            this.DeviceContext.Rasterizer.SetViewport(viewport);
            this.DeviceContext.OutputMerger.SetTargets(depthMap, renderTargets.Length, renderTargets);

            if (clear)
            {
                if (renderTargets != null && renderTargets.Length > 0)
                {
                    for (int i = 0; i < renderTargets.Length; i++)
                    {
                        this.DeviceContext.ClearRenderTargetView(
                            renderTargets[i],
                            clearColor);
                    }
                }

                if (depthMap != null)
                {
                    this.DeviceContext.ClearDepthStencilView(
                        depthMap,
                        depthClearFlags,
                        1.0f, 0);
                }
            }
        }
        /// <summary>
        /// Set render target
        /// </summary>
        /// <param name="viewport">Viewport</param>
        /// <param name="depthMap">Depth map</param>
        /// <param name="renderTarget">Render target</param>
        /// <param name="clear">Indicates whether the target and stencil buffer must be cleared</param>
        /// <param name="clearColor">Clear color</param>
        /// <param name="depthClearFlags">Depth cleraring flags</param>
        /// <remarks>By default, depth clearing flags were "Depth" and "Stencil"</remarks>
        public void SetRenderTarget(ViewportF viewport, DepthStencilView depthMap, RenderTargetView renderTarget, bool clear, Color4 clearColor, DepthStencilClearFlags depthClearFlags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil)
        {
            this.DeviceContext.Rasterizer.SetViewport(viewport);
            this.DeviceContext.OutputMerger.SetTargets(depthMap, renderTarget);

            if (clear)
            {
                if (renderTarget != null)
                {
                    this.DeviceContext.ClearRenderTargetView(
                        renderTarget,
                        clearColor);
                }

                if (depthMap != null)
                {
                    this.DeviceContext.ClearDepthStencilView(
                        depthMap,
                        depthClearFlags,
                        1.0f, 0);
                }
            }
        }
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="viewport">Viewport</param>
        /// <param name="depthMap">Depth map</param>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clear">Indicates whether the target and stencil buffer must be cleared</param>
        /// <param name="clearColor">Clear color</param>
        /// <param name="depthClearFlags">Depth cleraring flags</param>
        /// <remarks>By default, depth clearing flags were "Depth" and "Stencil"</remarks>
        public void SetRenderTargets(ViewportF viewport, DepthStencilView depthMap, RenderTargetView[] renderTargets, bool clear, Color4 clearColor, DepthStencilClearFlags depthClearFlags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil)
        {
            this.DeviceContext.Rasterizer.SetViewport(viewport);
            this.DeviceContext.OutputMerger.SetTargets(depthMap, renderTargets.Length, renderTargets);

            if (clear)
            {
                if (renderTargets != null && renderTargets.Length > 0)
                {
                    for (int i = 0; i < renderTargets.Length; i++)
                    {
                        this.DeviceContext.ClearRenderTargetView(
                            renderTargets[i],
                            clearColor);
                    }
                }

                if (depthMap != null)
                {
                    this.DeviceContext.ClearDepthStencilView(
                        depthMap,
                        depthClearFlags,
                        1.0f, 0);
                }
            }
        }

        /// <summary>
        /// Enables z-buffer
        /// </summary>
        public void SetDepthStencilZEnabled()
        {
            this.SetDepthStencilState(this.depthStencilzBufferEnabled);
        }
        /// <summary>
        /// Disables z-buffer
        /// </summary>
        public void SetDepthStencilZDisabled()
        {
            this.SetDepthStencilState(this.depthStencilzBufferDisabled);
        }
        /// <summary>
        /// Disables depth stencil
        /// </summary>
        public void SetDepthStencilNone()
        {
            this.SetDepthStencilState(this.depthStencilNone);
        }
        /// <summary>
        /// Sets alpha rendering blend state
        /// </summary>
        public void SetBlendAlphaToCoverage()
        {
            this.SetBlendState(this.blendAlphaToCoverage, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets transparent blend state
        /// </summary>
        public void SetBlendTransparent()
        {
            this.SetBlendState(this.blendTransparent, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets additive blend state
        /// </summary>
        public void SetBlendAdditive()
        {
            this.SetBlendState(this.blendAdditive, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets default rasterizer
        /// </summary>
        public void SetRasterizerDefault()
        {
            this.SetRasterizerState(this.rasterizerDefault);
        }
        /// <summary>
        /// Sets wireframe rasterizer
        /// </summary>
        public void SetRasterizerWireframe()
        {
            this.SetRasterizerState(this.rasterizerWireframe);
        }
        /// <summary>
        /// Sets no-cull rasterizer
        /// </summary>
        public void SetRasterizerCullNone()
        {
            this.SetRasterizerState(this.rasterizerNoCull);
        }
        /// <summary>
        /// Sets cull counter-clockwise face rasterizer
        /// </summary>
        public void SetRasterizerCullFrontFace()
        {
            this.SetRasterizerState(this.rasterizerCullCounterClockwiseFace);
        }
        /// <summary>
        /// Dispose created resources
        /// </summary>
        public void Dispose()
        {
            if (this.swapChain != null)
            {
                if (this.swapChain.IsFullScreen) this.swapChain.IsFullScreen = false;
                this.swapChain.Dispose();
                this.swapChain = null;
            }

            this.DisposeResources();

            if (this.Device != null)
            {
                this.Device.Dispose();
                this.Device = null;
            }
        }

        /// <summary>
        /// Sets depth stencil state
        /// </summary>
        /// <param name="state">Depth stencil state</param>
        private void SetDepthStencilState(DepthStencilState state)
        {
            if (this.currentDepthStencilState != state)
            {
                this.Device.ImmediateContext.OutputMerger.SetDepthStencilState(state);

                this.currentDepthStencilState = state;
            }
        }
        /// <summary>
        /// Stes blend state
        /// </summary>
        /// <param name="state">Blend state</param>
        /// <param name="blendFactor">Blend factor</param>
        /// <param name="sampleMask">Sample mask</param>
        private void SetBlendState(BlendState state, Color4? blendFactor = null, int sampleMask = -1)
        {
            if (this.currentBlendState != state)
            {
                this.Device.ImmediateContext.OutputMerger.SetBlendState(state, blendFactor, sampleMask);

                this.currentBlendState = state;
            }
        }
        /// <summary>
        /// Sets rasterizer state
        /// </summary>
        /// <param name="state">Rasterizer state</param>
        private void SetRasterizerState(RasterizerState state)
        {
            if (this.currentRasterizerState != state)
            {
                this.Device.ImmediateContext.Rasterizer.State = state;

                this.currentRasterizerState = state;
            }
        }

        /// <summary>
        /// Finds mode description
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="fullScreen">True for full screen modes</param>
        /// <param name="refreshRate">Refresh date</param>
        /// <param name="adapter">Selected adapter</param>
        /// <returns>Returns found mode description</returns>
        private ModeDescription FindModeDescription(Format format, int width, int height, bool fullScreen, int refreshRate, out Adapter1 adapter)
        {
            adapter = null;

            using (Factory1 factory = new Factory1())
            {
                adapter = factory.GetAdapter1(0);

                using (Adapter1 firstAdapter = factory.GetAdapter1(0))
                {
                    using (Output adapterOutput = firstAdapter.GetOutput(0))
                    {
                        try
                        {
                            ModeDescription[] displayModeList = adapterOutput.GetDisplayModeList(
                                format,
                                DisplayModeEnumerationFlags.Interlaced);

                            ModeDescription[] displayModes = Array.FindAll(displayModeList, d =>
                                d.Width == width &&
                                d.Height == height &&
                                (refreshRate == 0 || (d.RefreshRate.Numerator / d.RefreshRate.Denominator) + 1 == refreshRate));

                            if (displayModes.Length > 0)
                            {
                                return displayModes[0];
                            }
                        }
                        catch
                        {

                        }

                        try
                        {
                            ModeDescription result;
                            adapterOutput.GetClosestMatchingMode(
                                null,
                                new ModeDescription()
                                {
                                    Format = format,
                                    Width = width,
                                    Height = height,
                                },
                                out result);

                            result.Width = width;
                            result.Height = height;

                            return result;
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return new ModeDescription()
            {
                Width = width,
                Height = height,
                Format = format,
                RefreshRate = new Rational(0, 1),
                Scaling = DisplayModeScaling.Unspecified,
                ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
            };
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        private void DisposeResources()
        {
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

            if (this.depthStencilNone != null)
            {
                this.depthStencilNone.Dispose();
                this.depthStencilNone = null;
            }

            if (this.rasterizerDefault != null)
            {
                this.rasterizerDefault.Dispose();
                this.rasterizerDefault = null;
            }

            if (this.rasterizerWireframe != null)
            {
                this.rasterizerWireframe.Dispose();
                this.rasterizerWireframe = null;
            }

            if (this.rasterizerNoCull != null)
            {
                this.rasterizerNoCull.Dispose();
                this.rasterizerNoCull = null;
            }

            if (this.rasterizerCullCounterClockwiseFace != null)
            {
                this.rasterizerCullCounterClockwiseFace.Dispose();
                this.rasterizerCullCounterClockwiseFace = null;
            }

            if (this.blendAlphaToCoverage != null)
            {
                this.blendAlphaToCoverage.Dispose();
                this.blendAlphaToCoverage = null;
            }

            if (this.blendTransparent != null)
            {
                this.blendTransparent.Dispose();
                this.blendTransparent = null;
            }

            if (this.blendAdditive != null)
            {
                this.blendAdditive.Dispose();
                this.blendAdditive = null;
            }
        }
    }
}
