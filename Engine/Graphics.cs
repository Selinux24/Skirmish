using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Helpers;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Graphics class
    /// </summary>
    public sealed partial class Graphics : IDisposable
    {
        /// <summary>
        /// On resized event
        /// </summary>
        public event EventHandler Resized;

#if DEBUG
        /// <summary>
        /// Debug device
        /// </summary>
        private DeviceDebug deviceDebug = null;
        /// <summary>
        /// Debug information queue
        /// </summary>
        private InfoQueue deviceDebugInfoQueue = null;
#endif

        /// <summary>
        /// Vertical sync enabled
        /// </summary>
        private readonly bool vsyncEnabled = false;
        /// <summary>
        /// Multisample count
        /// </summary>
        private readonly int msCount = 1;
        /// <summary>
        /// Multisample quality
        /// </summary>
        private readonly int msQuality = 0;
        /// <summary>
        /// Graphics device
        /// </summary>
        private Device3 device = null;
        /// <summary>
        /// Graphics inmmediate context
        /// </summary>
        private readonly DeviceContext3 deviceContext = null;
        /// <summary>
        /// Swap chain
        /// </summary>
        private SwapChain4 swapChain = null;

        /// <summary>
        /// Current primitive topology set in input assembler
        /// </summary>
        private Topology currentIAPrimitiveTopology = Topology.Undefined;
        /// <summary>
        /// Current input layout set in input assembler
        /// </summary>
        private InputLayout currentIAInputLayout = null;
        /// <summary>
        /// Current viewport
        /// </summary>
        private IEnumerable<RawViewportF> currentViewports;

        /// <summary>
        /// Back buffer format
        /// </summary>
        private readonly Format bufferFormat = BackBufferFormats.R8G8B8A8_UNorm;
        /// <summary>
        /// Depth buffer format
        /// </summary>
        private readonly Format depthFormat = DepthBufferFormats.D24_UNorm_S8_UInt;

        /// <summary>
        /// Device description
        /// </summary>
        public string DeviceDescription { get; private set; }

        /// <summary>
        /// Screen viewport
        /// </summary>
        public ViewportF Viewport { get; private set; }
        /// <summary>
        /// Gets if the device was created with multi-sampling active
        /// </summary>
        public bool MultiSampled
        {
            get
            {
                return msCount > 1;
            }
        }
        /// <summary>
        /// Current sample description
        /// </summary>
        public SampleDescription CurrentSampleDescription
        {
            get
            {
                return new SampleDescription(msCount, msQuality);
            }
        }
        /// <summary>
        /// Gets or sets the input assembler's primitive topology
        /// </summary>
        public Topology IAPrimitiveTopology
        {
            get
            {
                return currentIAPrimitiveTopology;
            }
            set
            {
                if (currentIAPrimitiveTopology != value)
                {
                    deviceContext.InputAssembler.PrimitiveTopology = (PrimitiveTopology)value;
                    Counters.IAPrimitiveTopologySets++;

                    currentIAPrimitiveTopology = value;
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
                return currentIAInputLayout;
            }
            set
            {
                if (currentIAInputLayout != value)
                {
                    deviceContext.InputAssembler.InputLayout = value;
                    Counters.IAInputLayoutSets++;

                    currentIAInputLayout = value;
                }
            }
        }

        /// <summary>
        /// Finds mode description
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="refreshRate">Refresh date</param>
        /// <param name="mode">Returns found mode description</param>
        private static void FindModeDescription(Device3 device, Format format, int width, int height, int refreshRate, out ModeDescription1 mode)
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

                                mode = displayModeList[0];

                                return;
                            }

                            ModeDescription1 desc = new ModeDescription1()
                            {
                                Width = width,
                                Height = height,
                                Format = format,
                            };
                            output.FindClosestMatchingMode1(
                                ref desc,
                                out mode,
                                device);

                            mode.Width = width;
                            mode.Height = height;

                            return;
                        }
                        catch
                        {
                            // Display mode not found
                        }
                    }
                }
            }

            mode = new ModeDescription1()
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
        public Graphics(IEngineForm form, bool vsyncEnabled = false, int refreshRate = 0, int multiSampling = 0)
        {
            FindModeDescription(
                device,
                bufferFormat,
                form.RenderWidth,
                form.RenderHeight,
                refreshRate,
                out var displayMode);

            this.vsyncEnabled = vsyncEnabled && displayMode.RefreshRate != new Rational(0, 1);

#if DEBUG
            using (var tmpFactory = new Factory2(true))
#else
            using (var tmpFactory = new Factory2())
#endif
            using (var factory = tmpFactory.QueryInterface<Factory5>())
            {
                int adapterIndex = SelectBestAdapter(factory);

                using (var tmpAdapter = factory.GetAdapter1(adapterIndex))
                using (var adapter = tmpAdapter.QueryInterface<Adapter4>())
                {
                    DeviceDescription = string.Format("{0}", adapter.Description2.Description);

                    DeviceCreationFlags creationFlags = DeviceCreationFlags.None;

#if DEBUG
                    creationFlags |= DeviceCreationFlags.Debug;
#endif
                    using (var tmpDevice = new Device(adapter, creationFlags, FeatureLevel.Level_11_1, FeatureLevel.Level_11_0))
                    {
                        device = tmpDevice.QueryInterface<Device3>();
                        device.DebugName = "GraphicsDevice";
                    }
                }

                if (multiSampling != 0 && !CheckMultisample(device, bufferFormat, multiSampling, out msCount, out msQuality))
                {
                    throw new EngineException(string.Format("The specified multisampling value [{0}] is not supported for {1}", multiSampling, bufferFormat));
                }

                var desc = new SwapChainDescription1()
                {
                    BufferCount = 2,
                    Format = displayMode.Format,
                    Width = displayMode.Width,
                    Height = displayMode.Height,
                    Stereo = displayMode.Stereo,
                    SampleDescription = CurrentSampleDescription,
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

                using (var tmpSwapChain = new SwapChain1(factory, device, form.Handle, ref desc, fsdesc))
                {
                    swapChain = tmpSwapChain.QueryInterface<SwapChain4>();
                    swapChain.DebugName = "GraphicsSwapChain";
                }
            }

            deviceContext = device.ImmediateContext3;
            deviceContext.DebugName = "Immediate";

            PrepareDevice(displayMode.Width, displayMode.Height, false);

#if DEBUG
            ConfigureDebugLayer();
#endif

            #region Alt + Enter

            using (var factory = swapChain.GetParent<Factory5>())
            {
                factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);
            }

            form.KeyUp += (sender, eventArgs) =>
            {
                if (eventArgs.Alt && (int)eventArgs.KeyCode == (int)Keys.Enter)
                {
                    swapChain.IsFullScreen = !swapChain.IsFullScreen;
                }
            };

            #endregion
        }
        /// <summary>
        /// Selects the best adapter based in dedicated video memory
        /// </summary>
        /// <param name="factory">Factory</param>
        /// <returns>Returns the best adapter index</returns>
        private int SelectBestAdapter(Factory5 factory)
        {
            int bestIndex = 0;

            int adapterCount = factory.GetAdapterCount1();

            long bestSize = 0;
            for (int i = 0; i < adapterCount; i++)
            {
                using (var adapter = factory.GetAdapter1(i))
                {
                    long size = adapter.Description1.DedicatedVideoMemory;
                    if (size > bestSize)
                    {
                        bestSize = size;
                        bestIndex = i;
                    }
                }
            }

            return bestIndex;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Graphics()
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
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (swapChain?.IsFullScreen == true)
                {
                    swapChain.IsFullScreen = false;
                }

                swapChain?.Dispose();
                swapChain = null;

                DisposeResources();

                device?.Dispose();
                device = null;

#if DEBUG
                deviceDebugInfoQueue?.Dispose();
                deviceDebugInfoQueue = null;

                deviceDebug?.ReportLiveDeviceObjects(ReportingLevel.Detail);
                deviceDebug?.Dispose();
                deviceDebug = null;
#endif
            }
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
                DisposeResources();

                swapChain.ResizeBuffers(2, width, height, bufferFormat, SwapChainFlags.None);
            }

            #region Viewport

            Viewport = new ViewportF()
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

            using (var backBuffer = Resource.FromSwapChain<Resource>(swapChain, 0))
            {
                renderTargetView = new EngineRenderTargetView("DefaultRenderTarget", new RenderTargetView1(device, backBuffer));
            }

            #endregion

            #region Depth Stencil Buffer and View

            depthStencilView = CreateDepthStencilBuffer("DefaultDepthStencil", depthFormat, width, height, true);

            #endregion

            #region Set Defaults

            SetDefaultViewport();
            SetDefaultRenderTarget(true, Color.Transparent, true, true);

            SetDepthStencilWRZEnabled();
            SetRasterizerDefault();
            SetBlendDefault();

            #endregion

            if (resizing)
            {
                //Launch the "resized" event
                Resized?.Invoke(this, new EventArgs());
            }
        }

#if DEBUG
        /// <summary>
        /// Configure debug layer messages
        /// </summary>
        private void ConfigureDebugLayer()
        {
            deviceDebug = device.QueryInterface<DeviceDebug>();
            deviceDebugInfoQueue = deviceDebug.QueryInterface<InfoQueue>();

            var severityFilter = new InfoQueueFilter()
            {
                AllowList = new InfoQueueFilterDescription()
                {

                },
                DenyList = new InfoQueueFilterDescription()
                {
                    Severities = new MessageSeverity[]
                    {
                        MessageSeverity.Information,
                        MessageSeverity.Message,
                    },
                }
            };

            var idFilter = new InfoQueueFilter()
            {
                AllowList = new InfoQueueFilterDescription()
                {

                },
                DenyList = new InfoQueueFilterDescription()
                {
                    Ids = new MessageId[]
                    {
                        MessageId.MessageIdDeviceDrawRenderTargetViewNotSet,
                    },
                }
            };

            deviceDebugInfoQueue.AddStorageFilterEntries(severityFilter);
            deviceDebugInfoQueue.AddStorageFilterEntries(idFilter);
        }
#endif

        /// <summary>
        /// Begin frame
        /// </summary>
        public void Begin(Scene scene)
        {
            deviceContext.ClearDepthStencilView(
                depthStencilView.GetDepthStencil(),
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f,
                0);

            deviceContext.ClearRenderTargetView(
                renderTargetView.GetRenderTarget(),
                scene.GameEnvironment.Background);
        }
        /// <summary>
        /// End frame
        /// </summary>
        public void End()
        {
            int syncInterval = vsyncEnabled ? 1 : 0;
            var res = swapChain.Present(syncInterval, PresentFlags.None);

            if (!res.Success)
            {
                Logger.WriteError(this, $"Error presenting Graphics: Code {res.Code}");
            }
        }

        /// <summary>
        /// Sets the default viewport
        /// </summary>
        public void SetDefaultViewport()
        {
            SetViewport(Viewport);
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(Viewport viewport)
        {
            SetViewPorts(new[] { (RawViewportF)viewport });
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(ViewportF viewport)
        {
            SetViewPorts(new[] { (RawViewportF)viewport });
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        public void SetViewports(IEnumerable<Viewport> viewports)
        {
            SetViewPorts(viewports.Select(v => (RawViewportF)v).ToArray());
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        public void SetViewports(IEnumerable<ViewportF> viewports)
        {
            SetViewPorts(viewports.Select(v => (RawViewportF)v).ToArray());
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        private void SetViewPorts(IEnumerable<RawViewportF> viewports)
        {
            if (Helper.CompareEnumerables(currentViewports, viewports))
            {
                return;
            }

            deviceContext.Rasterizer.SetViewports(viewports.ToArray());

            currentViewports = viewports;
        }

        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clearRT">Indicates whether the target must be cleared</param>
        /// <param name="clearRTColor">Render target clear color</param>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor)
        {
            SetRenderTargets(
                renderTargets, clearRT, clearRTColor,
                false);
        }
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clearRT">Indicates whether the target must be cleared</param>
        /// <param name="clearRTColor">Render target clear color</param>
        /// <param name="freeOMResources">Indicates whether the Output merger Shader Resources must be cleared</param>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, bool freeOMResources)
        {
            if (freeOMResources)
            {
                ClearShaderResources();
            }

            var rtv = renderTargets?.GetRenderTargets() ?? Enumerable.Empty<RenderTargetView1>();
            var rtvCount = rtv.Count();

            deviceContext.OutputMerger.SetTargets(null, rtvCount, rtv.ToArray());

            if (clearRT && rtvCount > 0)
            {
                for (int i = 0; i < rtvCount; i++)
                {
                    deviceContext.ClearRenderTargetView(rtv.ElementAt(i), clearRTColor);
                }
            }
        }
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clearRT">Indicates whether the target must be cleared</param>
        /// <param name="clearRTColor">Render target clear color</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, bool clearDepth, bool clearStencil)
        {
            SetRenderTargets(
                renderTargets, clearRT, clearRTColor,
                DefaultDepthStencil, clearDepth, clearStencil,
                false);
        }
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clearRT">Indicates whether the render target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="depthMap">Depth map</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        /// <param name="freeOMResources">Indicates whether the Output merger Shader Resources must be cleared</param>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil, bool freeOMResources)
        {
            if (freeOMResources)
            {
                ClearShaderResources();
            }

            var dsv = depthMap?.GetDepthStencil();
            var rtv = renderTargets?.GetRenderTargets() ?? Enumerable.Empty<RenderTargetView1>();
            var rtvCount = rtv.Count();

            deviceContext.OutputMerger.SetTargets(dsv, 0, new UnorderedAccessView[] { }, new int[] { }, rtv.ToArray());

            if (clearRT && rtvCount > 0)
            {
                for (int i = 0; i < rtvCount; i++)
                {
                    deviceContext.ClearRenderTargetView(rtv.ElementAt(i), clearRTColor);
                }
            }

            ClearDepthStencilBuffer(depthMap, clearDepth, clearStencil);
        }

        /// <summary>
        /// Creates a new depth stencil state
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <returns>Returns a new depth stencil state</returns>
        public EngineDepthStencilState CreateDepthStencilState(string name, EngineDepthStencilStateDescription description)
        {
            return new EngineDepthStencilState(name, new DepthStencilState(device, (DepthStencilStateDescription)description));
        }

        /// <summary>
        /// Creates a new blend state
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="blendFactor">Blend factor</param>
        /// <param name="sampleMask">Sample mask</param>
        /// <returns>Returns a new blend state</returns>
        public EngineBlendState CreateBlendState(string name, EngineBlendStateDescription description, Color4? blendFactor, int sampleMask)
        {
            return new EngineBlendState(name, new BlendState1(device, (BlendStateDescription1)description), blendFactor, sampleMask);
        }

        /// <summary>
        /// Creates a new rasterizer state
        /// </summary>
        /// <param name="description">Description</param>
        /// <returns>Returns a new rasterizer state</returns>
        public EngineRasterizerState CreateRasterizerState(string name, EngineRasterizerStateDescription description)
        {
            return new EngineRasterizerState(name, new RasterizerState2(device, (RasterizerStateDescription2)description));
        }

        /// <summary>
        /// Creates a new Sampler state
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Sampler description</param>
        /// <returns>Returns the new sampler state</returns>
        public EngineSamplerState CreateSamplerState(string name, EngineSamplerStateDescription description)
        {
            return new EngineSamplerState(name, new SamplerState(device, (SamplerStateDescription)description));
        }

        /// <summary>
        /// Create depth stencil buffer view
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="useSamples">Use samples if available</param>
        /// <returns>Returns a depth stencil view</returns>
        public EngineDepthStencilView CreateDepthStencilBuffer(string name, Format format, int width, int height, bool useSamples)
        {
            bool multiSampled = false;
            SampleDescription sampleDescription = new SampleDescription(1, 0);
            if (useSamples)
            {
                multiSampled = MultiSampled;
                sampleDescription = CurrentSampleDescription;
            }

            using (var texture = new Texture2D1(
                device,
                new Texture2DDescription1()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = format,
                    SampleDescription = sampleDescription,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                }))
            {
                var description = new DepthStencilViewDescription()
                {
                    Format = format,
                    Dimension = multiSampled ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {

                    },
                    Texture2DMS = new DepthStencilViewDescription.Texture2DMultisampledResource()
                    {

                    },
                };

                return new EngineDepthStencilView($"{name}.DSV", new DepthStencilView(device, texture, description));
            }
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

                deviceContext.ClearDepthStencilView(
                    depthMap.GetDepthStencil(),
                    clearDSFlags,
                    1.0f, 0);
            }
        }

        /// <summary>
        /// Creates a new render target and his texture
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="useSamples">Use samples if available</param>
        /// <returns>Returns a render target and its texture</returns>
        public (EngineRenderTargetView RenderTarget, EngineShaderResourceView ShaderResource) CreateRenderTargetTexture(string name, Format format, int width, int height, bool useSamples)
        {
            try
            {
                Counters.Textures++;

                bool multiSampled = false;
                SampleDescription sampleDescription = new SampleDescription(1, 0);
                if (useSamples)
                {
                    multiSampled = MultiSampled;
                    sampleDescription = CurrentSampleDescription;
                }

                var rt = CreateRenderTargetTexture(name, format, width, height, multiSampled, sampleDescription);

                var rtv = new EngineRenderTargetView($"{name}.RTV", rt.RenderTarget);
                var srv = new EngineShaderResourceView($"{name}.SRV", rt.ShaderResource);

                return (rtv, srv);
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRenderTargetTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a new multiple render target and his textures
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Render target list size</param>
        /// <param name="useSamples">Use samples if available</param>
        /// <returns>Returns a render target and its textures</returns>
        public (EngineRenderTargetView RenderTarget, IEnumerable<EngineShaderResourceView> ShaderResources) CreateRenderTargetTexture(string name, Format format, int width, int height, int arraySize, bool useSamples)
        {
            try
            {
                Counters.Textures++;

                bool multiSampled = false;
                SampleDescription sampleDescription = new SampleDescription(1, 0);
                if (useSamples)
                {
                    multiSampled = MultiSampled;
                    sampleDescription = CurrentSampleDescription;
                }

                var rtv = new EngineRenderTargetView($"{name}.RTV_array_{arraySize}");
                var srv = new EngineShaderResourceView[arraySize];

                for (int i = 0; i < arraySize; i++)
                {
                    var rt = CreateRenderTargetTexture(name, format, width, height, multiSampled, sampleDescription);

                    rtv.Add(rt.RenderTarget);
                    srv[i] = new EngineShaderResourceView($"{name}.SRV[{i}]", rt.ShaderResource);
                }

                return (rtv, srv);
            }
            catch (Exception ex)
            {
                throw new EngineException("CreateRenderTargetTexture Error. See inner exception for details", ex);
            }
        }
        /// <summary>
        /// Creates a render target texture and a shader resource view for the texture
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="multiSampled">Create a multisampled texture</param>
        /// <param name="sampleDescription">Sample description</param>
        /// <returns>Returns a render target and its texture</returns>
        private (RenderTargetView1 RenderTarget, ShaderResourceView1 ShaderResource) CreateRenderTargetTexture(string name, Format format, int width, int height, bool multiSampled, SampleDescription sampleDescription)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = format,
                SampleDescription = sampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            var texture = new Texture2D1(device, desc)
            {
                DebugName = name
            };
            using (texture)
            {
                var rtvDesc = new RenderTargetViewDescription1()
                {
                    Format = format,
                };
                var srvDesc = new ShaderResourceViewDescription1()
                {
                    Format = format,
                };

                if (multiSampled)
                {
                    rtvDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampled;
                    rtvDesc.Texture2DMS = new RenderTargetViewDescription.Texture2DMultisampledResource();

                    srvDesc.Dimension = ShaderResourceViewDimension.Texture2DMultisampled;
                    srvDesc.Texture2DMS = new ShaderResourceViewDescription.Texture2DMultisampledResource();
                }
                else
                {
                    rtvDesc.Dimension = RenderTargetViewDimension.Texture2D;
                    rtvDesc.Texture2D = new RenderTargetViewDescription1.Texture2DResource();

                    srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                    srvDesc.Texture2D = new ShaderResourceViewDescription1.Texture2DResource1() { MipLevels = 1 };
                }

                var rtv = new RenderTargetView1(device, texture, rtvDesc)
                {
                    DebugName = $"{name}.RTVTexture",
                };
                var srv = new ShaderResourceView1(device, texture, srvDesc)
                {
                    DebugName = $"{name}.SRVTexture",
                };

                return (rtv, srv);
            }
        }

        /// <summary>
        /// Creates a set of texture and depth stencil view for shadow mapping
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns the depth stencil buffer and its texture</returns>
        public (EngineDepthStencilView DepthStencil, EngineShaderResourceView ShaderResource) CreateShadowMapTextures(string name, int width, int height)
        {
            var desc = new Texture2DDescription1()
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
            };

            var depthMap = new Texture2D1(device, desc)
            {
                DebugName = name
            };
            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0,
                    },
                };
                var dsv = new EngineDepthStencilView($"{name}.DSV", new DepthStencilView(device, depthMap, dsDescription));

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV", new ShaderResourceView1(device, depthMap, rvDescription));

                return (dsv, srv);
            }
        }
        /// <summary>
        /// Creates a set of texture and depth stencil view for shadow mapping
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="mapCount">Per stencil view map count</param>
        /// <returns>Returns the depth stencil buffer and its texture</returns>
        public (EngineDepthStencilView DepthStencil, EngineShaderResourceView ShaderResource) CreateShadowMapTextures(string name, int width, int height, int mapCount)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = mapCount,
                Format = Format.R24G8_Typeless,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            var depthMap = new Texture2D1(device, desc)
            {
                DebugName = name
            };
            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = DepthStencilViewDimension.Texture2DArray,
                    Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource()
                    {
                        ArraySize = mapCount,
                        FirstArraySlice = 0,
                        MipSlice = 0,
                    },
                };
                var dsv = new EngineDepthStencilView($"{name}.DSV", new DepthStencilView(device, depthMap, dsDescription));

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription1.Texture2DResource1()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV", new ShaderResourceView1(device, depthMap, rvDescription));

                return (dsv, srv);
            }
        }
        /// <summary>
        /// Creates a set of texture and depth stencil view array for shadow mapping
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="mapCount">Per stencil view map count</param>
        /// <param name="arraySize">Array size</param>
        /// <returns>Returns the depth stencil buffer list and its texture</returns>
        public (IEnumerable<EngineDepthStencilView> DepthStencils, EngineShaderResourceView ShaderResource) CreateShadowMapTextureArrays(string name, int width, int height, int mapCount, int arraySize)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = arraySize * mapCount,
                Format = Format.R24G8_Typeless,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            var depthMap = new Texture2D1(device, desc)
            {
                DebugName = name
            };
            using (depthMap)
            {
                var dsv = new EngineDepthStencilView[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    var dsDescription = new DepthStencilViewDescription
                    {
                        Flags = DepthStencilViewFlags.None,
                        Format = Format.D24_UNorm_S8_UInt,
                        Dimension = DepthStencilViewDimension.Texture2DArray,
                        Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource()
                        {
                            ArraySize = mapCount,
                            FirstArraySlice = i,
                            MipSlice = 0,
                        },
                    };
                    dsv[i] = new EngineDepthStencilView($"{name}_{i}.DSV", new DepthStencilView(device, depthMap, dsDescription));
                }

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.Texture2DArray,
                    Texture2DArray = new ShaderResourceViewDescription1.Texture2DArrayResource1()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                        ArraySize = arraySize * mapCount,
                        FirstArraySlice = 0,
                        PlaneSlice = 0,
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV", new ShaderResourceView1(device, depthMap, rvDescription));

                return (dsv, srv);
            }
        }
        /// <summary>
        /// Creates a cubic texture for shadow mapping
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="width">Face width</param>
        /// <param name="height">Face height</param>
        /// <returns>Returns the depth stencil buffer and its texture</returns>
        public (EngineDepthStencilView DepthStencil, EngineShaderResourceView ShaderResource) CreateCubicShadowMapTextures(string name, int width, int height)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = 6,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R24G8_Typeless,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            var depthMap = new Texture2D1(device, desc)
            {
                DebugName = name
            };
            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = DepthStencilViewDimension.Texture2DArray,
                    Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource()
                    {
                        ArraySize = 6,
                        FirstArraySlice = 0,
                        MipSlice = 0,
                    },
                };
                var dsv = new EngineDepthStencilView($"{name}.DSV", new DepthStencilView(device, depthMap, dsDescription));

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.TextureCube,
                    TextureCube = new ShaderResourceViewDescription.TextureCubeResource()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV", new ShaderResourceView1(device, depthMap, rvDescription));

                return (dsv, srv);
            }
        }
        /// <summary>
        /// Creates a cubic texture array for shadow mapping
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="width">Face width</param>
        /// <param name="height">Face height</param>
        /// <param name="arraySize">Array size</param>
        /// <returns>Returns the depth stencil buffer list and its texture</returns>
        public (IEnumerable<EngineDepthStencilView> DepthStencils, EngineShaderResourceView ShaderResource) CreateCubicShadowMapTextureArrays(string name, int width, int height, int arraySize)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = 6 * arraySize,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R24G8_Typeless,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };

            var depthMap = new Texture2D1(device, desc)
            {
                DebugName = name
            };
            using (depthMap)
            {
                var dsv = new EngineDepthStencilView[arraySize];
                for (int i = 0; i < arraySize; i++)
                {
                    var dsDescription = new DepthStencilViewDescription
                    {
                        Flags = DepthStencilViewFlags.None,
                        Format = Format.D24_UNorm_S8_UInt,
                        Dimension = DepthStencilViewDimension.Texture2DArray,
                        Texture2DArray = new DepthStencilViewDescription.Texture2DArrayResource()
                        {
                            ArraySize = 6,
                            FirstArraySlice = i * 6,
                            MipSlice = 0,
                        },
                    };
                    dsv[i] = new EngineDepthStencilView($"{name}_{i}.DSV", new DepthStencilView(device, depthMap, dsDescription));
                }

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    Dimension = ShaderResourceViewDimension.TextureCubeArray,
                    TextureCubeArray = new ShaderResourceViewDescription.TextureCubeArrayResource()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                        CubeCount = arraySize,
                        First2DArrayFace = 0,
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV", new ShaderResourceView1(device, depthMap, rvDescription));

                return (dsv, srv);
            }
        }

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="vertexCount">Vertex count</param>
        /// <param name="startVertexLocation">Start vertex location</param>
        public void Draw(int vertexCount, int startVertexLocation)
        {
            deviceContext.Draw(vertexCount, startVertexLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw indexed
        /// </summary>
        /// <param name="indexCount">Index count</param>
        /// <param name="startIndexLocation">Start vertex location</param>
        /// <param name="baseVertexLocation">Base vertex location</param>
        public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw instanced
        /// </summary>
        /// <param name="vertexCountPerInstance">Vertex count per instance</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="startVertexLocation">Start vertex location</param>
        /// <param name="startInstanceLocation">Start instance count</param>
        public void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
        {
            deviceContext.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);

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
        public void DrawIndexedInstanced(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
            deviceContext.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <summary>
        /// Draw auto
        /// </summary>
        public void DrawAuto()
        {
            deviceContext.DrawAuto();

            Counters.DrawCallsPerFrame++;
        }
    }
}
