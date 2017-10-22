using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Engine
{
    using Engine.Common;
    using Engine.Helpers;
    using SharpDX.Direct3D11;

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
        /// Graphics device
        /// </summary>
        private Device3 device = null;
        /// <summary>
        /// Graphics inmmediate context
        /// </summary>
        private DeviceContext3 deviceContext = null;
        /// <summary>
        /// Swap chain
        /// </summary>
        private SwapChain4 swapChain = null;
        /// <summary>
        /// Render target view
        /// </summary>
        private EngineRenderTargetView renderTargetView = null;
        /// <summary>
        /// Depth stencil view
        /// </summary>
        private EngineDepthStencilView depthStencilView = null;

        /// <summary>
        /// Current depth-stencil state
        /// </summary>
        private DepthStencilState currentDepthStencilState = null;
        /// <summary>
        /// Current depth-stencil state reference value
        /// </summary>
        private int currentDepthStencilStateRef = 0;
        /// <summary>
        /// Current blend state
        /// </summary>
        private BlendState1 currentBlendState = null;
        /// <summary>
        /// Current rasterizer state
        /// </summary>
        private RasterizerState2 currentRasterizerState = null;

        /// <summary>
        /// Depth stencil state with z-buffer enabled for write
        /// </summary>
        private DepthStencilState depthStencilzBufferEnabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer disabled for write
        /// </summary>
        private DepthStencilState depthStencilzBufferDisabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer enabled for read
        /// </summary>
        private DepthStencilState depthStencilRDzBufferEnabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer disabled for read
        /// </summary>
        private DepthStencilState depthStencilRDzBufferDisabled = null;
        /// <summary>
        /// No depth, no stencil
        /// </summary>
        private DepthStencilState depthStencilNone = null;
        /// <summary>
        /// Depth stencil state for volume marking
        /// </summary>
        private DepthStencilState depthStencilVolumeMarking = null;
        /// <summary>
        /// Depth stencil state for volume drawing
        /// </summary>
        private DepthStencilState depthStencilVolumeDrawing = null;

        /// <summary>
        /// Default blend state
        /// </summary>
        private BlendState1 blendDefault = null;
        /// <summary>
        /// Default alpha blend state
        /// </summary>
        private BlendState1 blendDefaultAlpha = null;
        /// <summary>
        /// Blend state for transparent blending
        /// </summary>
        private BlendState1 blendTransparent = null;
        /// <summary>
        /// Additive blend state
        /// </summary>
        private BlendState1 blendAdditive = null;
        /// <summary>
        /// Blend state for deferred lighting blending
        /// </summary>
        private BlendState1 blendDeferredLighting = null;
        /// <summary>
        /// Blend state for defered composer blending
        /// </summary>
        private BlendState1 blendDeferredComposer = null;
        /// <summary>
        /// Blend state for transparent defered composer blending
        /// </summary>
        private BlendState1 blendDeferredComposerTransparent = null;

        /// <summary>
        /// Default rasterizer
        /// </summary>
        private RasterizerState2 rasterizerDefault = null;
        /// <summary>
        /// Wireframe rasterizer
        /// </summary>
        private RasterizerState2 rasterizerWireframe = null;
        /// <summary>
        /// No-cull rasterizer
        /// </summary>
        private RasterizerState2 rasterizerNoCull = null;
        /// <summary>
        /// Cull counter-clockwise face rasterizer
        /// </summary>
        private RasterizerState2 rasterizerCullFrontFace = null;
        /// <summary>
        /// Stencil pass rasterizer (No Cull, No depth limit)
        /// </summary>
        private RasterizerState2 rasterizerStencilPass = null;
        /// <summary>
        /// Lighting pass rasterizer (Cull Front faces, No depth limit)
        /// </summary>
        private RasterizerState2 rasterizerLightingPass = null;

        /// <summary>
        /// Current vertex buffer first slot
        /// </summary>
        private int currentVertexBufferFirstSlot = -1;
        /// <summary>
        /// Current vertex buffer bindings
        /// </summary>
        private VertexBufferBinding[] currentVertexBufferBindings = null;
        /// <summary>
        /// Current index buffer reference
        /// </summary>
        private Buffer currentIndexBufferRef = null;
        /// <summary>
        /// Current index buffer format
        /// </summary>
        private Format currentIndexFormat = Format.Unknown;
        /// <summary>
        /// Current index buffer offset
        /// </summary>
        private int currentIndexOffset = -1;
        /// <summary>
        /// Current primitive topology set in input assembler
        /// </summary>
        private PrimitiveTopology currentIAPrimitiveTopology = PrimitiveTopology.Undefined;
        /// <summary>
        /// Current input layout set in input assembler
        /// </summary>
        private InputLayout currentIAInputLayout = null;

        /// <summary>
        /// Back buffer format
        /// </summary>
        protected Format BufferFormat = BackBufferFormats.R8G8B8A8_UNorm;
        /// <summary>
        /// Depth buffer format
        /// </summary>
        protected Format DepthFormat = DepthBufferFormats.D24_UNorm_S8_UInt;

        /// <summary>
        /// Device description
        /// </summary>
        public string DeviceDescription { get; private set; }

        /// <summary>
        /// Screen viewport
        /// </summary>
        public ViewportF Viewport { get; private set; }
        /// <summary>
        /// Gets the default render target
        /// </summary>
        public EngineRenderTargetView DefaultRenderTarget
        {
            get
            {
                return this.renderTargetView;
            }
        }
        /// <summary>
        /// Gets the default depth stencil buffer
        /// </summary>
        public EngineDepthStencilView DefaultDepthStencil
        {
            get
            {
                return this.depthStencilView;
            }
        }
        /// <summary>
        /// Gets if the device was created with multi-sampling active
        /// </summary>
        public bool MultiSampled
        {
            get
            {
                return this.msCount > 1;
            }
        }
        /// <summary>
        /// Current sample description
        /// </summary>
        public SampleDescription CurrentSampleDescription
        {
            get
            {
                return new SampleDescription(this.msCount, this.msQuality);
            }
        }

        /// <summary>
        /// Finds mode description
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="fullScreen">True for full screen modes</param>
        /// <param name="refreshRate">Refresh date</param>
        /// <returns>Returns found mode description</returns>
        private static ModeDescription1 FindModeDescription(Device3 device, Format format, int width, int height, bool fullScreen, int refreshRate)
        {
#if DEBUG
            using (var tmpFactory = new Factory2(true))
#else
            using (var tmpFactory = new Factory2())
#endif
            using (var factory = tmpFactory.QueryInterface<Factory5>())
            {
                using (var tmpAdapter = factory.GetAdapter1(0))
                using (var adapter = tmpAdapter.QueryInterface<Adapter4>())
                {
                    using (var tmpOutput = adapter.GetOutput(0))
                    using (var output = tmpOutput.QueryInterface<Output6>())
                    {
                        try
                        {
                            var displayModeList = output.GetDisplayModeList1(
                                format,
                                DisplayModeEnumerationFlags.Interlaced);

                            displayModeList = Array.FindAll(displayModeList, d => d.Width == width && d.Height == height);
                            if (displayModeList.Length > 0)
                            {
                                if (refreshRate > 0)
                                {
                                    Array.Sort(displayModeList, (d1, d2) =>
                                    {
                                        float f1 = (float)d1.RefreshRate.Numerator / (float)d1.RefreshRate.Denominator;
                                        float f2 = (float)d2.RefreshRate.Numerator / (float)d2.RefreshRate.Denominator;

                                        f1 = Math.Abs(refreshRate - f1);
                                        f2 = Math.Abs(refreshRate - f2);

                                        return f1.CompareTo(f2);
                                    });
                                }
                                else
                                {
                                    Array.Sort(displayModeList, (d1, d2) =>
                                    {
                                        float f1 = (float)d1.RefreshRate.Numerator / (float)d1.RefreshRate.Denominator;
                                        float f2 = (float)d2.RefreshRate.Numerator / (float)d2.RefreshRate.Denominator;

                                        return f2.CompareTo(f1);
                                    });
                                }

                                return displayModeList[0];
                            }
                        }
                        catch
                        {

                        }

                        try
                        {
                            ModeDescription1 result;
                            ModeDescription1 desc = new ModeDescription1()
                            {
                                Width = width,
                                Height = height,
                                Format = format,
                            };
                            output.FindClosestMatchingMode1(
                                ref desc,
                                out result,
                                device);

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

            return new ModeDescription1()
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
        /// Checks the multi-sample specified count
        /// </summary>
        /// <param name="device">Temporary device</param>
        /// <param name="format">Format</param>
        /// <param name="multiSampling">Multi-sample count</param>
        /// <param name="sampleCount">Sample count</param>
        /// <param name="maxQualityLevel">Maximum quality level</param>
        /// <returns>Returns true y the device supports MS for the specified format</returns>
        private static bool CheckMultisample(Device3 device, Format format, int multiSampling, out int sampleCount, out int maxQualityLevel)
        {
            sampleCount = 1;
            maxQualityLevel = 0;
            int maxQuality = device.CheckMultisampleQualityLevels1(format, multiSampling, CheckMultisampleQualityLevelsFlags.None);
            if (maxQuality > 0)
            {
                sampleCount = multiSampling;
                maxQualityLevel = maxQuality - 1;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="form">Game form</param>
        /// <param name="vsyncEnabled">Vertical sync enabled</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multisampling</param>
        public Graphics(EngineForm form, bool vsyncEnabled = false, int refreshRate = 0, int multiSampling = 0)
        {
            var displayMode = FindModeDescription(
                this.device,
                this.BufferFormat,
                form.RenderWidth,
                form.RenderHeight,
                form.IsFullscreen,
                refreshRate);

            this.vsyncEnabled = vsyncEnabled && displayMode.RefreshRate != new Rational(0, 1);

#if DEBUG
            using (var tmpFactory = new Factory2(true))
#else
            using (var tmpFactory = new Factory2())
#endif
            using (var factory = tmpFactory.QueryInterface<Factory5>())
            {
                using (var tmpAdapter = factory.GetAdapter1(0))
                using (var adapter = tmpAdapter.QueryInterface<Adapter4>())
                {
                    this.DeviceDescription = string.Format("{0}", adapter.Description2.Description);

                    DeviceCreationFlags creationFlags = DeviceCreationFlags.None;

#if DEBUG
                    creationFlags |= DeviceCreationFlags.Debug;
#endif
                    using (var tmpDevice = new Device(adapter, creationFlags, FeatureLevel.Level_11_1, FeatureLevel.Level_11_0))
                    {
                        this.device = tmpDevice.QueryInterface<Device3>();
                    }
                }

                if (multiSampling != 0)
                {
                    CheckMultisample(this.device, this.BufferFormat, multiSampling, out this.msCount, out this.msQuality);
                }

                var desc = new SwapChainDescription1()
                {
                    BufferCount = 2,
                    Format = displayMode.Format,
                    Width = displayMode.Width,
                    Height = displayMode.Height,
                    Stereo = displayMode.Stereo,
                    SampleDescription = this.CurrentSampleDescription,
                    AlphaMode = AlphaMode.Ignore,
                    Scaling = Scaling.Stretch,
                    Usage = Usage.RenderTargetOutput,
                    SwapEffect = SwapEffect.Sequential,
                    Flags = SwapChainFlags.None,
                };
                var fsdesc = new SwapChainFullScreenDescription()
                {
                    RefreshRate = displayMode.RefreshRate,
                    Scaling = displayMode.Scaling,
                    ScanlineOrdering = displayMode.ScanlineOrdering,
                    Windowed = !form.IsFullscreen,
                };

                using (var tmpSwapChain = new SwapChain1(factory, this.device, form.Handle, ref desc, fsdesc))
                {
                    this.swapChain = tmpSwapChain.QueryInterface<SwapChain4>();
                }
            }

            this.deviceContext = this.device.ImmediateContext3;

            this.PrepareDevice(displayMode.Width, displayMode.Height, false);

            #region Alt + Enter

            using (var factory = this.swapChain.GetParent<Factory5>())
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

            #region Render Target

            using (var backBuffer = Resource.FromSwapChain<Resource>(this.swapChain, 0))
            {
                this.renderTargetView = new EngineRenderTargetView(new RenderTargetView1(this.device, backBuffer));
            }

            #endregion

            #region Depth Stencil Buffer and View

            this.CreateDepthStencil(this.DepthFormat, width, height, out this.depthStencilView);

            #endregion

            #region Depth Stencil States

            #region Z-buffer enabled for write depth-stencil state

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

            #endregion

            #region Z-buffer disabled for write depth-stencil state

            this.depthStencilzBufferDisabled = new DepthStencilState(
                this.device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Never,

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

            #region Z-buffer enabled for read depth-stencil state

            this.depthStencilRDzBufferEnabled = new DepthStencilState(
                this.device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Less,
                });

            #endregion

            #region Z-buffer disabled for read depth-stencil state

            this.depthStencilRDzBufferDisabled = new DepthStencilState(
                this.device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Never,
                });

            #endregion

            #region No depth, no stencil state

            this.depthStencilNone = new DepthStencilState(
                this.device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Never,

                    IsStencilEnabled = false,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,
                });

            #endregion

            #region Depth-stencil state for volume marking (Value != 0 if object is inside of the current drawing volume)

            this.depthStencilVolumeMarking = new DepthStencilState(
                this.device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Less,

                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,

                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },

                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },
                });

            #endregion

            #region Depth-stencil state for volume drawing (Process pixels if stencil value != stencil reference)

            this.depthStencilVolumeDrawing = new DepthStencilState(
                this.device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Never,

                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0x00,

                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Keep,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.NotEqual,
                    },

                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Keep,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.NotEqual,
                    },
                });

            #endregion

            #endregion

            #region Rasterizer States

            //Default rasterizer state
            this.rasterizerDefault = new RasterizerState2(
                this.device,
                new RasterizerStateDescription2()
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

            //Wireframe rasterizer state
            this.rasterizerWireframe = new RasterizerState2(
                this.device,
                new RasterizerStateDescription2()
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

            //No cull rasterizer state
            this.rasterizerNoCull = new RasterizerState2(
                this.device,
                new RasterizerStateDescription2()
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

            //Counter clockwise cull rasterizer state
            this.rasterizerCullFrontFace = new RasterizerState2(
                this.device,
                new RasterizerStateDescription2()
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

            //Stencil pass rasterizer state
            this.rasterizerStencilPass = new RasterizerState2(
                this.device,
                new RasterizerStateDescription2()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = false,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            //Counter clockwise cull rasterizer state
            this.rasterizerLightingPass = new RasterizerState2(
                this.device,
                new RasterizerStateDescription2()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = true,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = false,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            #endregion

            #region Blend States

            #region Default blend state (No alpha)
            {
                BlendStateDescription1 desc = new BlendStateDescription1();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationBlend = BlendOption.Zero;

                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                this.blendDefault = new BlendState1(this.device, desc);
            }
            #endregion

            #region Alpha blend state
            {
                BlendStateDescription1 desc = new BlendStateDescription1();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                this.blendDefaultAlpha = new BlendState1(this.device, desc);
            }
            #endregion

            #region Transparent blend state
            {
                BlendStateDescription1 desc = new BlendStateDescription1();
                desc.AlphaToCoverageEnable = true;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                this.blendTransparent = new BlendState1(this.device, desc);
            }
            #endregion

            #region Additive blend state
            {
                BlendStateDescription1 desc = new BlendStateDescription1();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTarget[0].DestinationBlend = BlendOption.One;

                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                this.blendAdditive = new BlendState1(this.device, desc);
            }
            #endregion

            #region Deferred composer blend state (no alpha)
            {
                BlendStateDescription1 desc = new BlendStateDescription1();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = true;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                desc.RenderTarget[1].IsBlendEnabled = true;
                desc.RenderTarget[1].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[1].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[1].SourceBlend = BlendOption.One;
                desc.RenderTarget[1].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[1].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[1].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[1].DestinationAlphaBlend = BlendOption.Zero;

                desc.RenderTarget[2].IsBlendEnabled = true;
                desc.RenderTarget[2].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[2].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[2].SourceBlend = BlendOption.One;
                desc.RenderTarget[2].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[2].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[2].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[2].DestinationAlphaBlend = BlendOption.Zero;

                this.blendDeferredComposer = new BlendState1(this.device, desc);
            }
            #endregion

            #region Deferred composer transparent blend state
            {
                BlendStateDescription1 desc = new BlendStateDescription1();
                desc.AlphaToCoverageEnable = true;
                desc.IndependentBlendEnable = true;

                //Transparent blending only in first buffer
                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                desc.RenderTarget[1].IsBlendEnabled = true;
                desc.RenderTarget[1].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[1].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[1].SourceBlend = BlendOption.One;
                desc.RenderTarget[1].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[1].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[1].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[1].DestinationAlphaBlend = BlendOption.Zero;

                desc.RenderTarget[2].IsBlendEnabled = true;
                desc.RenderTarget[2].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[2].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[2].SourceBlend = BlendOption.One;
                desc.RenderTarget[2].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[2].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[2].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[2].DestinationAlphaBlend = BlendOption.Zero;

                this.blendDeferredComposerTransparent = new BlendState1(this.device, desc);
            }
            #endregion

            #region Deferred lighting blend state
            {
                BlendStateDescription1 desc = new BlendStateDescription1();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationBlend = BlendOption.One;
                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;

                this.blendDeferredLighting = new BlendState1(this.device, desc);
            }
            #endregion

            #endregion

            #region Set Defaults

            this.SetDefaultViewport();
            this.SetDefaultRenderTarget();

            this.SetDepthStencilZEnabled();
            this.SetRasterizerDefault();
            this.SetBlendDefault();

            #endregion

            if (resizing)
            {
                if (this.Resized != null)
                {
                    //Launch the "resized" event
                    this.Resized(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Begin frame
        /// </summary>
        public void Begin()
        {
            this.deviceContext.ClearDepthStencilView(
                this.depthStencilView.GetDepthStencil(),
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f,
                0);

            this.deviceContext.ClearRenderTargetView(
                this.renderTargetView.GetRenderTarget(),
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
        /// Sets the default viewport
        /// </summary>
        public void SetDefaultViewport()
        {
            this.SetViewport(this.Viewport);
        }
        /// <summary>
        /// Sets default render target
        /// </summary>
        /// <param name="clear">Indicates whether the target and stencil buffer must be cleared</param>
        public void SetDefaultRenderTarget(bool clear = true)
        {
            this.SetRenderTargets(this.renderTargetView, clear, GameEnvironment.Background, this.depthStencilView, clear, clear);
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(Viewport viewport)
        {
            this.deviceContext.Rasterizer.SetViewport(viewport);
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(ViewportF viewport)
        {
            this.deviceContext.Rasterizer.SetViewport(viewport);
        }
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="renderTarget">Render target</param>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clearRT">Indicates whether the target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="depthMap">Depth map</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil)
        {
            var dsv = depthMap != null ? depthMap.GetDepthStencil() : null;
            var rtv = renderTargets != null ? renderTargets.GetRenderTargets() : null;
            var rtvCount = renderTargets != null ? renderTargets.Count : 0;

            this.deviceContext.OutputMerger.SetTargets(dsv, rtvCount, rtv);

            if (clearRT && rtv != null && rtvCount > 0)
            {
                for (int i = 0; i < rtvCount; i++)
                {
                    this.deviceContext.ClearRenderTargetView(
                        rtv[i],
                        clearRTColor);
                }
            }

            if ((clearDepth || clearStencil) && dsv != null)
            {
                DepthStencilClearFlags clearDSFlags = 0;
                if (clearDepth) clearDSFlags |= DepthStencilClearFlags.Depth;
                if (clearStencil) clearDSFlags |= DepthStencilClearFlags.Stencil;

                this.deviceContext.ClearDepthStencilView(
                    dsv,
                    clearDSFlags,
                    1.0f, 0);
            }
        }
        /// <summary>
        /// Sets targets for stream output
        /// </summary>
        /// <param name="streamOutBinding">Stream output binding</param>
        public void SetStreamOutputTargets(StreamOutputBufferBinding[] streamOutBinding)
        {
            this.deviceContext.StreamOutput.SetTargets(streamOutBinding);
        }
        /// <summary>
        /// Clear depth / stencil buffer
        /// </summary>
        /// <param name="depthMap">Depth buffer</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        public void ClearDepthStencilBuffer(EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil)
        {
            if ((clearDepth || clearStencil) && depthMap != null)
            {
                DepthStencilClearFlags clearDSFlags = 0;
                if (clearDepth) clearDSFlags |= DepthStencilClearFlags.Depth;
                if (clearStencil) clearDSFlags |= DepthStencilClearFlags.Stencil;

                this.deviceContext.ClearDepthStencilView(
                    depthMap.GetDepthStencil(),
                    clearDSFlags,
                    1.0f, 0);
            }
        }
        /// <summary>
        /// Enables z-buffer for write
        /// </summary>
        public void SetDepthStencilZEnabled()
        {
            this.SetDepthStencilState(this.depthStencilzBufferEnabled);
        }
        /// <summary>
        /// Disables z-buffer for write
        /// </summary>
        public void SetDepthStencilZDisabled()
        {
            this.SetDepthStencilState(this.depthStencilzBufferDisabled);
        }
        /// <summary>
        /// Enables z-buffer for read
        /// </summary>
        public void SetDepthStencilRDZEnabled()
        {
            this.SetDepthStencilState(this.depthStencilRDzBufferEnabled);
        }
        /// <summary>
        /// Disables z-buffer for read
        /// </summary>
        public void SetDepthStencilRDZDisabled()
        {
            this.SetDepthStencilState(this.depthStencilRDzBufferDisabled);
        }
        /// <summary>
        /// Disables depth stencil
        /// </summary>
        public void SetDepthStencilNone()
        {
            this.SetDepthStencilState(this.depthStencilNone);
        }
        /// <summary>
        /// Sets depth stencil for volume marking
        /// </summary>
        public void SetDepthStencilVolumeMarking()
        {
            this.SetDepthStencilState(this.depthStencilVolumeMarking);
        }
        /// <summary>
        /// Sets depth stencil for volume drawing
        /// </summary>
        public void SetDepthStencilVolumeDrawing(int stencilRef)
        {
            this.SetDepthStencilState(this.depthStencilVolumeDrawing, stencilRef);
            this.deviceContext.OutputMerger.DepthStencilReference = stencilRef;
        }
        /// <summary>
        /// Sets default blend state
        /// </summary>
        public void SetBlendDefault()
        {
            this.SetBlendState(this.blendDefault, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets default alpha blend state
        /// </summary>
        public void SetBlendDefaultAlpha()
        {
            this.SetBlendState(this.blendDefaultAlpha, Color.Transparent, -1);
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
        /// Sets deferred composer blend state
        /// </summary>
        public void SetBlendDeferredComposer()
        {
            this.SetBlendState(this.blendDeferredComposer, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets transparent deferred composer blend state
        /// </summary>
        public void SetBlendDeferredComposerTransparent()
        {
            this.SetBlendState(this.blendDeferredComposerTransparent, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets deferred lighting blend state
        /// </summary>
        public void SetBlendDeferredLighting()
        {
            this.SetBlendState(this.blendDeferredLighting, Color.Transparent, -1);
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
            this.SetRasterizerState(this.rasterizerCullFrontFace);
        }
        /// <summary>
        /// Sets stencil pass rasterizer
        /// </summary>
        public void SetRasterizerStencilPass()
        {
            this.SetRasterizerState(this.rasterizerStencilPass);
        }
        /// <summary>
        /// Stes lighting pass rasterizer
        /// </summary>
        public void SetRasterizerLightingPass()
        {
            this.SetRasterizerState(this.rasterizerLightingPass);
        }
        /// <summary>
        /// Bind an array of vertex buffers to the input-assembler stage.
        /// </summary>
        /// <param name="firstSlot">The first input slot for binding</param>
        /// <param name="vertexBufferBindings">A reference to an array of VertexBufferBinding</param>
        public void IASetVertexBuffers(int firstSlot, params VertexBufferBinding[] vertexBufferBindings)
        {
            if (this.currentVertexBufferFirstSlot != firstSlot || this.currentVertexBufferBindings != vertexBufferBindings)
            {
                this.deviceContext.InputAssembler.SetVertexBuffers(firstSlot, vertexBufferBindings);
                Counters.IAVertexBuffersSets++;

                this.currentVertexBufferFirstSlot = firstSlot;
                this.currentVertexBufferBindings = vertexBufferBindings;
            }
        }
        /// <summary>
        /// Bind an index buffer to the input-assembler stage.
        /// </summary>
        /// <param name="indexBufferRef">A reference to an Buffer object</param>
        /// <param name="format">A SharpDX.DXGI.Format that specifies the format of the data in the index buffer</param>
        /// <param name="offset">Offset (in bytes) from the start of the index buffer to the first index to use</param>
        public void IASetIndexBuffer(Buffer indexBufferRef, Format format, int offset)
        {
            if (this.currentIndexBufferRef != indexBufferRef || this.currentIndexFormat != format || this.currentIndexOffset != offset)
            {
                this.deviceContext.InputAssembler.SetIndexBuffer(indexBufferRef, format, offset);
                Counters.IAIndexBufferSets++;

                this.currentIndexBufferRef = indexBufferRef;
                this.currentIndexFormat = format;
                this.currentIndexOffset = offset;
            }
        }
        /// <summary>
        /// Gets or sets the input assembler's primitive topology
        /// </summary>
        public PrimitiveTopology IAPrimitiveTopology
        {
            get
            {
                return this.currentIAPrimitiveTopology;
            }
            set
            {
                if (this.currentIAPrimitiveTopology != value)
                {
                    this.deviceContext.InputAssembler.PrimitiveTopology = value;
                    Counters.IAPrimitiveTopologySets++;

                    this.currentIAPrimitiveTopology = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the input assembler's input layout
        /// </summary>
        public InputLayout IAInputLayout
        {
            get
            {
                return this.currentIAInputLayout;
            }
            set
            {
                if (this.currentIAInputLayout != value)
                {
                    this.deviceContext.InputAssembler.InputLayout = value;
                    Counters.IAInputLayoutSets++;

                    this.currentIAInputLayout = value;
                }
            }
        }
        /// <summary>
        /// Dispose created resources
        /// </summary>
        public void Dispose()
        {
            if (this.swapChain != null)
            {
                if (this.swapChain.IsFullScreen) this.swapChain.IsFullScreen = false;

                Helper.Dispose(this.swapChain);
            }

            this.DisposeResources();

            Helper.Dispose(this.device);
        }

        /// <summary>
        /// Sets depth stencil state
        /// </summary>
        /// <param name="state">Depth stencil state</param>
        private void SetDepthStencilState(DepthStencilState state, int stencilRef = 0)
        {
            if (this.currentDepthStencilState != state || this.currentDepthStencilStateRef != stencilRef)
            {
                this.device.ImmediateContext.OutputMerger.SetDepthStencilState(state, stencilRef);

                this.currentDepthStencilState = state;
                this.currentDepthStencilStateRef = stencilRef;

                Counters.DepthStencilStateChanges++;
            }
        }
        /// <summary>
        /// Stes blend state
        /// </summary>
        /// <param name="state">Blend state</param>
        /// <param name="blendFactor">Blend factor</param>
        /// <param name="sampleMask">Sample mask</param>
        private void SetBlendState(BlendState1 state, Color4? blendFactor = null, int sampleMask = -1)
        {
            if (this.currentBlendState != state)
            {
                this.device.ImmediateContext.OutputMerger.SetBlendState(state, blendFactor, sampleMask);

                this.currentBlendState = state;

                Counters.BlendStateChanges++;
            }
        }
        /// <summary>
        /// Sets rasterizer state
        /// </summary>
        /// <param name="state">Rasterizer state</param>
        private void SetRasterizerState(RasterizerState2 state)
        {
            if (this.currentRasterizerState != state)
            {
                this.device.ImmediateContext.Rasterizer.State = state;

                this.currentRasterizerState = state;

                Counters.RasterizerStateChanges++;
            }
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        private void DisposeResources()
        {
            Helper.Dispose(this.renderTargetView);
            Helper.Dispose(this.depthStencilView);

            Helper.Dispose(this.depthStencilzBufferEnabled);
            Helper.Dispose(this.depthStencilzBufferDisabled);
            Helper.Dispose(this.depthStencilRDzBufferEnabled);
            Helper.Dispose(this.depthStencilRDzBufferDisabled);
            Helper.Dispose(this.depthStencilNone);
            Helper.Dispose(this.depthStencilVolumeMarking);
            Helper.Dispose(this.depthStencilVolumeDrawing);

            Helper.Dispose(this.rasterizerDefault);
            Helper.Dispose(this.rasterizerWireframe);
            Helper.Dispose(this.rasterizerNoCull);
            Helper.Dispose(this.rasterizerCullFrontFace);
            Helper.Dispose(this.rasterizerStencilPass);
            Helper.Dispose(this.rasterizerLightingPass);

            Helper.Dispose(this.blendDefault);
            Helper.Dispose(this.blendDefaultAlpha);
            Helper.Dispose(this.blendTransparent);
            Helper.Dispose(this.blendAdditive);
            Helper.Dispose(this.blendDeferredLighting);
            Helper.Dispose(this.blendDeferredComposer);
            Helper.Dispose(this.blendDeferredComposerTransparent);
        }

        /// <summary>
        /// Creates a resource view from a texture description
        /// </summary>
        /// <param name="description">Texture description</param>
        /// <returns>Returns the new shader resource view</returns>
        private ShaderResourceView1 CreateResource(TextureData description)
        {
            bool mipAutogen = false;

            if (description.MipMaps == 1)
            {
                var fmtSupport = this.device.CheckFormatSupport(description.Format);
                mipAutogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);
            }

            if (mipAutogen)
            {
                using (var texture = this.CreateTexture2D(description.Width, description.Height, description.Format, 1, mipAutogen))
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = texture.Description.Format,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                        {
                            MipLevels = -1,
                        },
                    };

                    var result = new ShaderResourceView1(this.device, texture, desc);

                    this.deviceContext.UpdateSubresource(description.GetDataBox(0, 0), texture, 0);

                    this.deviceContext.GenerateMips(result);

                    return result;
                }
            }
            else
            {
                var width = description.Width;
                var height = description.Height;
                var format = description.Format;
                var mipMaps = description.MipMaps;
                var arraySize = description.ArraySize;
                var data = description.GetDataBoxes(0);

                using (var texture = this.CreateTexture2D(width, height, format, mipMaps, arraySize, data))
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                        {
                            MipLevels = mipMaps,
                        },
                    };

                    return new ShaderResourceView1(this.device, texture, desc);
                }
            }
        }
        /// <summary>
        /// Creates a resource view from a texture description list
        /// </summary>
        /// <param name="descriptions">Texture description list</param>
        /// <returns>Returns the new shader resource view</returns>
        private ShaderResourceView1 CreateResource(TextureData[] descriptions)
        {
            var description = descriptions[0];

            bool mipAutogen = false;

            if (description.MipMaps == 1)
            {
                var fmtSupport = this.device.CheckFormatSupport(description.Format);
                mipAutogen = fmtSupport.HasFlag(FormatSupport.MipAutogen);
            }

            if (mipAutogen)
            {
                using (var textureArray = this.CreateTexture2D(description.Width, description.Height, description.Format, descriptions.Length, mipAutogen))
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = description.Format,
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Texture2DArray = new ShaderResourceViewDescription1.Texture2DArrayResource1()
                        {
                            ArraySize = descriptions.Length,
                            MipLevels = -1,
                        },
                    };

                    var result = new ShaderResourceView1(this.device, textureArray, desc);

                    for (int i = 0; i < descriptions.Length; i++)
                    {
                        int mipSize;
                        var index = textureArray.CalculateSubResourceIndex(0, i, out mipSize);

                        this.deviceContext.UpdateSubresource(descriptions[i].GetDataBox(0, 0), textureArray, index);
                    }

                    this.deviceContext.GenerateMips(result);

                    return result;
                }
            }
            else
            {
                var width = description.Width;
                var height = description.Height;
                var format = description.Format;
                var mipMaps = description.MipMaps;
                var arraySize = description.ArraySize;
                var data = description.GetDataBoxes();

                using (var textureArray = this.CreateTexture2D(width, height, format, mipMaps, arraySize, data))
                {
                    var desc = new ShaderResourceViewDescription1()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Texture2DArray = new ShaderResourceViewDescription1.Texture2DArrayResource1()
                        {
                            ArraySize = arraySize,
                            MipLevels = mipMaps,
                        },
                    };

                    return new ShaderResourceView1(this.device, textureArray, desc);
                }
            }
        }
        /// <summary>
        /// Creates an empty Texture2D
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="arraySize">Size</param>
        /// <param name="generateMips">Generate mips for the texture</param>
        /// <returns>Returns the Texture2D</returns>
        private Texture2D1 CreateTexture2D(int width, int height, Format format, int arraySize, bool generateMips)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = (generateMips) ? BindFlags.ShaderResource | BindFlags.RenderTarget : BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                MipLevels = (generateMips) ? 0 : 1,
                OptionFlags = (generateMips) ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(this.device, description);
        }
        /// <summary>
        /// Creates a Texture2D
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="format">Format</param>
        /// <param name="data"></param>
        /// <returns>Returns the Texture2D</returns>
        private Texture2D1 CreateTexture2D(int width, int height, Format format, int mipMaps, int arraySize, DataBox[] data)
        {
            var description = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = arraySize,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                MipLevels = mipMaps,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            return new Texture2D1(this.device, description, data);
        }

        /// <summary>
        /// Loads a texture from memory in the graphics device
        /// </summary>
        /// <param name="buffer">Data buffer</param>
        /// <returns>Returns the resource view</returns>
        internal EngineShaderResourceView LoadTexture(byte[] buffer)
        {
            try
            {
                Counters.Textures++;

                using (var resource = TextureData.ReadTexture(buffer))
                {
                    return new EngineShaderResourceView(CreateResource(resource));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from byte array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <returns>Returns the resource view</returns>
        internal EngineShaderResourceView LoadTexture(string filename)
        {
            try
            {
                Counters.Textures++;

                using (var resource = TextureData.ReadTexture(filename))
                {
                    return new EngineShaderResourceView(CreateResource(resource));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture from file in the graphics device
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the resource view</returns>
        internal EngineShaderResourceView LoadTexture(MemoryStream stream)
        {
            try
            {
                Counters.Textures++;

                using (var resource = TextureData.ReadTexture(stream))
                {
                    return new EngineShaderResourceView(CreateResource(resource));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="filenames">Path file collection</param>
        /// <returns>Returns the resource view</returns>
        internal EngineShaderResourceView LoadTextureArray(string[] filenames)
        {
            try
            {
                Counters.Textures++;

                var textureList = TextureData.ReadTexture(filenames);

                var resource = this.CreateResource(textureList);

                Helper.Dispose(textureList);

                return new EngineShaderResourceView(resource);
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from file array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a texture array from a file collection in the graphics device
        /// </summary>
        /// <param name="streams">Stream collection</param>
        /// <returns>Returns the resource view</returns>
        internal EngineShaderResourceView LoadTextureArray(MemoryStream[] streams)
        {
            try
            {
                Counters.Textures++;

                var textureList = TextureData.ReadTexture(streams);

                var resource = this.CreateResource(textureList);

                Helper.Dispose(textureList);

                return new EngineShaderResourceView(resource);
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTexture from stream array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a cube texture from file in the graphics device
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="format">Format</param>
        /// <param name="faceSize">Face size</param>
        /// <returns>Returns the resource view</returns>
        internal EngineShaderResourceView LoadTextureCube(string filename, Format format, int faceSize)
        {
            try
            {
                Counters.Textures++;

                using (var cubeTex = new Texture2D1(
                    this.device,
                    new Texture2DDescription1()
                    {
                        Width = faceSize,
                        Height = faceSize,
                        MipLevels = 0,
                        ArraySize = 6,
                        SampleDescription = new SampleDescription(1, 0),
                        Format = format,
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
                    }))
                {
                    return new EngineShaderResourceView(new ShaderResourceView1(this.device, cubeTex));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTextureCube from filename Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Loads a cube texture from file in the graphics device
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="format">Format</param>
        /// <param name="faceSize">Face size</param>
        /// <returns>Returns the resource view</returns>
        internal EngineShaderResourceView LoadTextureCube(MemoryStream stream, Format format, int faceSize)
        {
            try
            {
                Counters.Textures++;

                using (var cubeTex = new Texture2D1(
                    this.device,
                    new Texture2DDescription1()
                    {
                        Width = faceSize,
                        Height = faceSize,
                        MipLevels = 0,
                        ArraySize = 6,
                        SampleDescription = new SampleDescription(1, 0),
                        Format = format,
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
                    }))
                {
                    return new EngineShaderResourceView(new ShaderResourceView1(this.device, cubeTex));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("LoadTextureCube from stream Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <param name="size">Texture size</param>
        /// <param name="values">Color values</param>
        /// <returns>Returns created texture</returns>
        internal EngineShaderResourceView CreateTexture1D(int size, Vector4[] values)
        {
            try
            {
                Counters.Textures++;

                using (var str = DataStream.Create(values, false, false))
                {
                    using (var randTex = new Texture1D(
                        this.device,
                        new Texture1DDescription()
                        {
                            Format = Format.R32G32B32A32_Float,
                            Width = size,
                            ArraySize = 1,
                            MipLevels = 1,
                            Usage = ResourceUsage.Immutable,
                            BindFlags = BindFlags.ShaderResource,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                        },
                        str))
                    {
                        return new EngineShaderResourceView(new ShaderResourceView1(this.device, randTex));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateTexture1D from value array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a texture filled with specified values
        /// </summary>
        /// <param name="size">Texture size</param>
        /// <param name="values">Color values</param>
        /// <returns>Returns created texture</returns>
        internal EngineShaderResourceView CreateTexture2D(int size, Vector4[] values)
        {
            try
            {
                Counters.Textures++;

                var tmp = new Vector4[size * size];
                Array.Copy(values, tmp, values.Length);

                using (var str = DataStream.Create(tmp, false, false))
                {
                    var dBox = new DataBox(str.DataPointer, size * (int)FormatHelper.SizeOfInBytes(Format.R32G32B32A32_Float), 0);

                    using (var texture = new Texture2D1(
                        this.device,
                        new Texture2DDescription1()
                        {
                            Format = Format.R32G32B32A32_Float,
                            Width = size,
                            Height = size,
                            ArraySize = 1,
                            MipLevels = 1,
                            SampleDescription = new SampleDescription(1, 0),
                            Usage = ResourceUsage.Immutable,
                            BindFlags = BindFlags.ShaderResource,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                        },
                        new[] { dBox }))
                    {
                        return new EngineShaderResourceView(new ShaderResourceView1(this.device, texture));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateTexture2D from value array Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a random 1D texture
        /// </summary>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns created texture</returns>
        internal EngineShaderResourceView CreateRandomTexture(int size, float min, float max, int seed = 0)
        {
            try
            {
                Counters.Textures++;

                Random rnd = new Random(seed);

                var randomValues = new List<Vector4>();
                for (int i = 0; i < size; i++)
                {
                    randomValues.Add(rnd.NextVector4(new Vector4(min), new Vector4(max)));
                }

                return this.CreateTexture1D(size, randomValues.ToArray());
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRandomTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a new render tarjet and his texture
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="rtv">Render target</param>
        /// <param name="srv">Texture</param>
        internal void CreateRenderTargetTexture(Format format, int width, int height, out EngineRenderTargetView rtv, out EngineShaderResourceView srv)
        {
            try
            {
                Counters.Textures++;

                using (var texture = new Texture2D1(
                    this.device,
                    new Texture2DDescription1()
                    {
                        Width = width,
                        Height = height,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = format,
                        SampleDescription = this.CurrentSampleDescription,
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    }))
                {
                    rtv = new EngineRenderTargetView(new RenderTargetView1(this.device, texture));
                    srv = new EngineShaderResourceView(new ShaderResourceView1(this.device, texture));
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRenderTargetTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a new multiple render target and his textures
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="size">Render target list size</param>
        /// <param name="rtv">Render target</param>
        /// <param name="srv">Textures</param>
        internal void CreateRenderTargetTexture(Format format, int width, int height, int size, out EngineRenderTargetView rtv, out EngineShaderResourceView[] srv)
        {
            try
            {
                Counters.Textures++;

                rtv = new EngineRenderTargetView();
                srv = new EngineShaderResourceView[size];

                for (int i = 0; i < size; i++)
                {
                    using (var texture = new Texture2D1(
                        this.device,
                        new Texture2DDescription1()
                        {
                            Width = width,
                            Height = height,
                            MipLevels = 1,
                            ArraySize = 1,
                            Format = format,
                            SampleDescription = this.CurrentSampleDescription,
                            Usage = ResourceUsage.Default,
                            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None
                        }))
                    {
                        rtv.Add(new RenderTargetView1(this.device, texture));
                        srv[i] = new EngineShaderResourceView(new ShaderResourceView1(this.device, texture));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRenderTargetTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a set of texture and depth stencil view for shadow mapping
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="dsv">Resulting Depth Stencil View</param>
        /// <param name="srv">Resulting Shader Resource View</param>
        internal void CreateShadowMapTextures(Format format, int width, int height, out EngineDepthStencilView dsv, out EngineShaderResourceView srv)
        {
            var depthMap = new Texture2D1(
                this.device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    SampleDescription = this.CurrentSampleDescription,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });

            using (depthMap)
            {
                var dsDimension = this.MultiSampled ?
                    DepthStencilViewDimension.Texture2DMultisampled :
                    DepthStencilViewDimension.Texture2D;

                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = dsDimension,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0,
                    },
                    Texture2DMS = new DepthStencilViewDescription.Texture2DMultisampledResource()
                    {

                    },
                };

                var rvDimension = this.MultiSampled ?
                    ShaderResourceViewDimension.Texture2DMultisampled :
                    ShaderResourceViewDimension.Texture2D;

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = rvDimension,
                    Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                    Texture2DMS = new ShaderResourceViewDescription.Texture2DMultisampledResource()
                    {

                    },
                };

                dsv = new EngineDepthStencilView(new DepthStencilView(this.device, depthMap, dsDescription));
                srv = new EngineShaderResourceView(new ShaderResourceView1(this.device, depthMap, rvDescription));
            }
        }
        /// <summary>
        /// Create depth stencil view
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="dsv">Resulting depth stencil view</param>
        internal void CreateDepthStencil(Format format, int width, int height, out EngineDepthStencilView dsv)
        {
            using (var dsb = new Texture2D1(
                this.device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    SampleDescription = this.CurrentSampleDescription,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                }))
            {
                var description = new DepthStencilViewDescription()
                {
                    Format = format,
                    Dimension = this.MultiSampled ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0
                    },
                };

                dsv = new EngineDepthStencilView(new DepthStencilView(this.device, dsb, description));
            }
        }
        /// <summary>
        /// Creates a new Input Layout for a Shader
        /// </summary>
        /// <param name="shaderBytecode">Byte code</param>
        /// <param name="elements">Input elements</param>
        /// <returns>Returns a new Input Layout</returns>
        internal InputLayout CreateInputLayout(ShaderBytecode shaderBytecode, InputElement[] elements)
        {
            return new InputLayout(this.device, shaderBytecode, elements);
        }

        /// <summary>
        /// Loads vertex shader from file
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Retuns vertex shader description</returns>
        internal EngineVertexShader LoadVertexShader(
            string filename,
            string entryPoint,
            InputElement[] input,
            string profile)
        {
            return LoadVertexShader(
                File.ReadAllBytes(filename),
                entryPoint,
                input,
                profile);
        }
        /// <summary>
        /// Loads vertex shader from file
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Retuns vertex shader description</returns>
        internal EngineVertexShader LoadVertexShader(
            string filename,
            string entryPoint,
            InputElement[] input,
            string profile,
            out string compilationErrors)
        {
            return LoadVertexShader(
                File.ReadAllBytes(filename),
                entryPoint,
                input,
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads vertex shader from byte code
        /// </summary>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Retuns vertex shader description</returns>
        internal EngineVertexShader LoadVertexShader(
            byte[] byteCode,
            string entryPoint,
            InputElement[] input,
            string profile)
        {
            string compilationErrors;
            return LoadVertexShader(
                byteCode,
                entryPoint,
                input,
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads vertex shader from byte code
        /// </summary>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Retuns vertex shader description</returns>
        internal EngineVertexShader LoadVertexShader(
            byte[] byteCode,
            string entryPoint,
            InputElement[] input,
            string profile,
            out string compilationErrors)
        {
            compilationErrors = null;
            using (ShaderIncludeManager includeManager = new ShaderIncludeManager())
            using (CompilationResult cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                InputLayout layout = new InputLayout(
                    this.device,
                    ShaderSignature.GetInputSignature(cmpResult.Bytecode),
                    input);

                VertexShader vertexShader = new VertexShader(
                    this.device,
                    cmpResult.Bytecode);

                return new EngineVertexShader(vertexShader, layout);
            }
        }
        /// <summary>
        /// Loads a pixel shader from file
        /// </summary>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns pixel shader description</returns>
        internal EnginePixelShader LoadPixelShader(
            string filename,
            string entryPoint,
            string profile)
        {
            string compilationErrors;
            return LoadPixelShader(
                File.ReadAllBytes(filename),
                entryPoint,
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a pixel shader from file
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns pixel shader description</returns>
        internal EnginePixelShader LoadPixelShader(
            string filename,
            string entryPoint,
            string profile,
            out string compilationErrors)
        {
            return LoadPixelShader(
                File.ReadAllBytes(filename),
                entryPoint,
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a pixel shader from byte code
        /// </summary>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns pixel shader description</returns>
        internal EnginePixelShader LoadPixelShader(
            byte[] byteCode,
            string entryPoint,
            string profile)
        {
            string compilationErrors;
            return LoadPixelShader(
                byteCode,
                entryPoint,
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a pixel shader from byte code
        /// </summary>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns pixel shader description</returns>
        internal EnginePixelShader LoadPixelShader(
            byte[] byteCode,
            string entryPoint,
            string profile,
            out string compilationErrors)
        {
            compilationErrors = null;

            using (ShaderIncludeManager includeManager = new ShaderIncludeManager())
            using (CompilationResult cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new EnginePixelShader(new PixelShader(this.device, cmpResult.Bytecode));
            }
        }

        /// <summary>
        /// Loads an effect from byte code
        /// </summary>
        /// <param name="bytes">Byte code</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns loaded effect</returns>
        internal EngineEffect CompileEffect(byte[] bytes, string profile)
        {
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                bytes,
                null,
                profile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                var effect = new Effect(
                    this.device,
                    cmpResult.Bytecode.Data,
                    EffectFlags.None);

                return new EngineEffect(effect);
            }
        }
        /// <summary>
        /// Loads an effect from pre-compiled file
        /// </summary>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded effect</returns>
        internal EngineEffect LoadEffect(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var effectCode = ShaderBytecode.FromStream(ms))
                {
                    var effect = new Effect(
                        this.device,
                        effectCode.Data,
                        EffectFlags.None);

                    return new EngineEffect(effect);
                }
            }
        }
        /// <summary>
        /// Apply effect pass
        /// </summary>
        /// <param name="technique"></param>
        /// <param name="index"></param>
        /// <param name="flags"></param>
        internal void EffectPassApply(EngineEffectTechnique technique, int index, int flags)
        {
            technique.GetPass(index).Apply(this.deviceContext, flags);
        }

        /// <summary>
        /// Creates an index buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        /// <param name="dynamic">Dynamic or Inmutable buffers</param>
        internal Buffer CreateIndexBuffer<T>(string name, T[] data, bool dynamic)
            where T : struct
        {
            return CreateBuffer<T>(
                name,
                data,
                dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags.IndexBuffer,
                dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
        }
        /// <summary>
        /// Creates a vertex buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data to write in the buffer</param>
        /// <param name="dynamic">Dynamic or Inmutable</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateVertexBuffer<T>(string name, T[] data, bool dynamic)
            where T : struct
        {
            return CreateBuffer<T>(
                name,
                data,
                dynamic ? ResourceUsage.Dynamic : ResourceUsage.Immutable,
                BindFlags.VertexBuffer,
                dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None);
        }
        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="length">Buffer length</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer</returns>
        internal Buffer CreateBuffer<T>(string name, int length, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * length;

            Counters.RegBuffer(typeof(T), name, (int)usage, (int)binding, sizeInBytes, length);

            var description = new BufferDescription()
            {
                Usage = usage,
                SizeInBytes = sizeInBytes,
                BindFlags = binding,
                CpuAccessFlags = access,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            return new Buffer(this.device, description);
        }
        /// <summary>
        /// Creates a buffer for the specified data type
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="name">Buffer name</param>
        /// <param name="data">Data</param>
        /// <param name="usage">Resource usage</param>
        /// <param name="binding">Binding</param>
        /// <param name="access">Cpu access</param>
        /// <returns>Returns created buffer initialized with the specified data</returns>
        internal Buffer CreateBuffer<T>(string name, T[] data, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * data.Length;

            Counters.RegBuffer(typeof(T), name, (int)usage, (int)binding, sizeInBytes, data.Length);

            using (var dstr = new DataStream(sizeInBytes, true, true))
            {
                dstr.WriteRange(data);
                dstr.Position = 0;

                var description = new BufferDescription()
                {
                    Usage = usage,
                    SizeInBytes = sizeInBytes,
                    BindFlags = binding,
                    CpuAccessFlags = access,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0,
                };

                return new Buffer(this.device, dstr, description);
            }
        }

        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal void WriteDiscardBuffer<T>(Buffer buffer, params T[] data)
            where T : struct
        {
            WriteDiscardBuffer<T>(buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal void WriteNoOverwriteBuffer<T>(Buffer buffer, params T[] data)
            where T : struct
        {
            WriteNoOverwriteBuffer<T>(buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        internal void WriteDiscardBuffer<T>(Buffer buffer, long offset, params T[] data)
            where T : struct
        {
            Counters.BufferWrites++;

            if (data != null && data.Length > 0)
            {
                DataStream stream;
                this.deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                using (stream)
                {
                    stream.Position = Marshal.SizeOf(default(T)) * offset;
                    stream.WriteRange(data);
                }
                this.deviceContext.UnmapSubresource(buffer, 0);
            }
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        internal void WriteNoOverwriteBuffer<T>(Buffer buffer, long offset, params T[] data)
            where T : struct
        {
            Counters.BufferWrites++;

            if (data != null && data.Length > 0)
            {
                DataStream stream;
                this.deviceContext.MapSubresource(buffer, MapMode.WriteNoOverwrite, MapFlags.None, out stream);
                using (stream)
                {
                    stream.Position = Marshal.SizeOf(default(T)) * offset;
                    stream.WriteRange(data);
                }
                this.deviceContext.UnmapSubresource(buffer, 0);
            }
        }

        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        internal T[] ReadBuffer<T>(Buffer buffer, int length)
            where T : struct
        {
            return ReadBuffer<T>(buffer, 0, length);
        }
        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset to read</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        internal T[] ReadBuffer<T>(Buffer buffer, long offset, int length)
            where T : struct
        {
            Counters.BufferReads++;

            T[] data = new T[length];

            DataStream stream;
            this.deviceContext.MapSubresource(buffer, MapMode.Read, MapFlags.None, out stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;

                for (int i = 0; i < length; i++)
                {
                    data[i] = stream.Read<T>();
                }
            }
            this.deviceContext.UnmapSubresource(buffer, 0);

            return data;
        }

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="vertexCount">Vertex count</param>
        /// <param name="startVertexLocation">Start vertex location</param>
        internal void Draw(int vertexCount, int startVertexLocation)
        {
            this.deviceContext.Draw(vertexCount, startVertexLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw indexed
        /// </summary>
        /// <param name="indexCount">Index count</param>
        /// <param name="startIndexLocation">Start vertex location</param>
        /// <param name="baseVertexLocation">Base vertex location</param>
        internal void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            this.deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw instanced
        /// </summary>
        /// <param name="vertexCountPerInstance">Vertex count per instance</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="startVertexLocation">Start vertex location</param>
        /// <param name="startInstanceLocation">Start instance count</param>
        internal void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
        {
            this.deviceContext.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw indexed instanced
        /// </summary>
        /// <param name="indexCountPerInstance">Index count per instance</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="startIndexLocation">Start index location</param>
        /// <param name="baseVertexLocation">Base vertex location</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        internal void DrawIndexedInstanced(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
            this.deviceContext.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw auto
        /// </summary>
        internal void DrawAuto()
        {
            this.deviceContext.DrawAuto();

            Counters.DrawCallsPerFrame++;
        }
    }
}
