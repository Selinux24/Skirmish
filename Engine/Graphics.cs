using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Vortice.Direct3D;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Engine
{
    using Engine.Common;
    using Engine.Helpers;
    using Vortice.Direct3D11;
    using Vortice.Direct3D11.Debug;

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
        private ID3D11Debug deviceDebug = null;
        /// <summary>
        /// Debug information queue
        /// </summary>
        private ID3D11InfoQueue deviceDebugInfoQueue = null;
#endif

        /// <summary>
        /// Vertical sync enabled
        /// </summary>
        private readonly bool vsyncEnabled = false;
        /// <summary>
        /// Multisample count
        /// </summary>
        private readonly uint msCount = 1;
        /// <summary>
        /// Multisample quality
        /// </summary>
        private readonly uint msQuality = 0;
        /// <summary>
        /// Graphics device
        /// </summary>
        private ID3D11Device3 device = null;
        /// <summary>
        /// Immediate context
        /// </summary>
        private readonly EngineDeviceContext immediateContext = null;
        /// <summary>
        /// Swap chain
        /// </summary>
        private IDXGISwapChain4 swapChain = null;
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
        /// Gets the graphics immmediate context
        /// </summary>
        public IEngineDeviceContext ImmediateContext { get => immediateContext; }

        /// <summary>
        /// Screen viewport
        /// </summary>
        public Viewport Viewport { get; private set; }
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
        /// Finds mode description
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="refreshRate">Refresh date</param>
        /// <param name="mode">Returns found mode description</param>
        private static void FindModeDescription(ID3D11Device3 device, Format format, uint width, uint height, uint refreshRate, out ModeDescription1 mode)
        {
#if DEBUG
            using var tmpFactory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(true);
#else
            using var tmpFactory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false);
#endif
            using var factory = tmpFactory.QueryInterface<IDXGIFactory5>();
            factory.EnumAdapters1(0, out IDXGIAdapter1 tmpAdapter);
            using (tmpAdapter)
            {
                using var adapter = tmpAdapter.QueryInterface<IDXGIAdapter4>();
                adapter.EnumOutputs(0, out IDXGIOutput tmpOutput);
                using (tmpOutput)
                {
                    using var output = tmpOutput.QueryInterface<IDXGIOutput6>();
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
                                    float f1 = d1.RefreshRate.Numerator / (float)d1.RefreshRate.Denominator;
                                    float f2 = d2.RefreshRate.Numerator / (float)d2.RefreshRate.Denominator;

                                    f1 = MathF.Abs(refreshRate - f1);
                                    f2 = MathF.Abs(refreshRate - f2);

                                    return f1.CompareTo(f2);
                                });
                            }
                            else
                            {
                                Array.Sort(displayModeList, (d1, d2) =>
                                {
                                    float f1 = d1.RefreshRate.Numerator / (float)d1.RefreshRate.Denominator;
                                    float f2 = d2.RefreshRate.Numerator / (float)d2.RefreshRate.Denominator;

                                    return f2.CompareTo(f1);
                                });
                            }

                            mode = displayModeList[0];

                            return;
                        }

                        mode = new ModeDescription1()
                        {
                            Width = width,
                            Height = height,
                            Format = format,
                            RefreshRate = new Rational(0, 1),
                            Scaling = ModeScaling.Unspecified,
                            ScanlineOrdering = ModeScanlineOrder.Unspecified,
                        };

                        //var desc = new ModeDescription1()
                        //{
                        //    Width = width,
                        //    Height = height,
                        //    Format = format,
                        //};
                        //output.FindClosestMatchingMode1(
                        //    ref desc,
                        //    out mode,
                        //    device);

                        //mode.Width = width;
                        //mode.Height = height;

                        return;
                    }
                    catch
                    {
                        // Display mode not found
                    }
                }
            }

            mode = new ModeDescription1()
            {
                Width = width,
                Height = height,
                Format = format,
                RefreshRate = new Rational(0, 1),
                Scaling = ModeScaling.Unspecified,
                ScanlineOrdering = ModeScanlineOrder.Unspecified,
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
        private static bool CheckMultisample(ID3D11Device3 device, Format format, uint multiSampling, out uint sampleCount, out uint maxQualityLevel)
        {
            sampleCount = 1;
            maxQualityLevel = 0;
            uint maxQuality = device.CheckMultisampleQualityLevels1(format, multiSampling, CheckMultisampleQualityLevelsFlags.None);
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
        public Graphics(IEngineForm form, bool vsyncEnabled = false, uint refreshRate = 0, uint multiSampling = 0)
        {
            FindModeDescription(
                device,
                bufferFormat,
                form.RenderWidth,
                form.RenderHeight,
                refreshRate,
                out var displayMode);

            this.vsyncEnabled = vsyncEnabled && (displayMode.RefreshRate.Numerator != 0 || displayMode.RefreshRate.Denominator != 1);

#if DEBUG
            using (var tmpFactory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(true))
#else
            using (var tmpFactory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false))
#endif
            using (var factory = tmpFactory.QueryInterface<IDXGIFactory5>())
            {
                uint adapterIndex = SelectBestAdapter(factory);

                factory.EnumAdapters1(adapterIndex, out var tmpAdapter);
                using (tmpAdapter)
                using (var adapter = tmpAdapter.QueryInterface<IDXGIAdapter4>())
                {
                    DeviceDescription = adapter.Description2.Description;

                    DeviceCreationFlags creationFlags = DeviceCreationFlags.None;

#if DEBUG
                    creationFlags |= DeviceCreationFlags.Debug;
#endif
                    var res = D3D11.D3D11CreateDevice(
                        adapter,
                        DriverType.Unknown,
                        creationFlags,
                        [FeatureLevel.Level_11_1, FeatureLevel.Level_11_0],
                        out var tmpDevice, out FeatureLevel featureLevel);
                    if (res.Failure)
                    {
                        throw new EngineException(res.ToString());
                    }

                    device = tmpDevice.QueryInterface<ID3D11Device3>();
                    device.DebugName = "Graphics Device";
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
                    BufferUsage = Usage.RenderTargetOutput,
                    SwapEffect = SwapEffect.Sequential,
                    Flags = SwapChainFlags.None,
                };
                var fsdesc = new SwapChainFullscreenDescription()
                {
                    RefreshRate = displayMode.RefreshRate,
                    Scaling = displayMode.Scaling,
                    ScanlineOrdering = displayMode.ScanlineOrdering,
                    Windowed = !form.IsFullscreen,
                };

                using var tmpSwapChain = factory.CreateSwapChainForHwnd(device, form.Handle, desc, fsdesc);
                swapChain = tmpSwapChain.QueryInterface<IDXGISwapChain4>();
                swapChain.DebugName = "Graphics Swap Chain";
            }

            immediateContext = new("Immediate Context", true, -1, device.ImmediateContext3);

            PrepareDevice(displayMode.Width, displayMode.Height, false);

#if DEBUG
            ConfigureDebugLayer();
#endif

            #region Alt + Enter

            using (var factory = swapChain.GetParent<IDXGIFactory5>())
            {
                factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);
            }

            #endregion
        }
        /// <summary>
        /// Selects the best adapter based in dedicated video memory
        /// </summary>
        /// <param name="factory">Factory</param>
        /// <returns>Returns the best adapter index</returns>
        private static uint SelectBestAdapter(IDXGIFactory5 factory)
        {
            uint bestIndex = 0;
            ulong bestSize = 0;
            for (uint i = 0; factory.EnumAdapters1(i, out var adapter).Success; i++)
            {
                using (adapter)
                {
                    ulong size = adapter.Description1.DedicatedVideoMemory;
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
            if (!disposing)
            {
                return;
            }

            if (swapChain?.IsFullscreen == true)
            {
                swapChain.SetFullscreenState(false);
            }

            swapChain?.Dispose();
            swapChain = null;

            DisposeResources();

            device?.Dispose();
            device = null;

#if DEBUG
            deviceDebugInfoQueue?.Dispose();
            deviceDebugInfoQueue = null;

            deviceDebug?.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Detail);
            deviceDebug?.Dispose();
            deviceDebug = null;
#endif
        }

        /// <summary>
        /// Prepare device
        /// </summary>
        /// <param name="width">Render width</param>
        /// <param name="height">Render height</param>
        /// <param name="resizing">Sets whether the render screen is resizing or not</param>
        public void PrepareDevice(uint width, uint height, bool resizing)
        {
            if (resizing)
            {
                DisposeResources();

                swapChain.ResizeBuffers(2, width, height, bufferFormat, SwapChainFlags.None);
            }

            #region Viewport

            Viewport = new Viewport()
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

            swapChain.GetBuffer(0, out ID3D11Resource backBuffer);
            using (backBuffer)
            {
                backBuffer.DebugName = "Default Render Target Surface";

                renderTargetView = new("Default Render Target", device.CreateRenderTargetView1(backBuffer));
            }

            #endregion

            #region Depth Stencil Buffer and View

            depthStencilView = CreateDepthStencilBuffer("Default Depth Stencil", depthFormat, width, height, true);

            #endregion

            #region Set Defaults

            ImmediateContext.SetViewport(Viewport);
            ImmediateContext.SetRenderTargets(DefaultRenderTarget, true, new(0), DefaultDepthStencil, true, true, false);

            ImmediateContext.SetRasterizerState(GetRasterizerDefault());
            ImmediateContext.SetBlendState(GetBlendDefault());
            ImmediateContext.SetDepthStencilState(GetDepthStencilWRZEnabled());

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
            deviceDebug = device.QueryInterface<ID3D11Debug>();
            deviceDebugInfoQueue = deviceDebug.QueryInterface<ID3D11InfoQueue>();

            var severityFilter = new InfoQueueFilter()
            {
                AllowList = new InfoQueueFilterDescription()
                {

                },
                DenyList = new InfoQueueFilterDescription()
                {
                    Severities =
                    [
                        MessageSeverity.Info,
                        MessageSeverity.Message,
                    ],
                }
            };

            var idFilter = new InfoQueueFilter()
            {
                AllowList = new InfoQueueFilterDescription()
                {

                },
                DenyList = new InfoQueueFilterDescription()
                {
                    Ids =
                    [
                        MessageId.DeviceDrawRenderTargetViewNotSet,
                    ],
                }
            };

            deviceDebugInfoQueue.AddStorageFilterEntries(severityFilter);
            deviceDebugInfoQueue.AddStorageFilterEntries(idFilter);
        }
        /// <summary>
        /// Gets the debug info
        /// </summary>
        public string GetDebugInfo()
        {
            var mcount = deviceDebugInfoQueue.NumStoredMessages;
            if (mcount == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new();

            for (uint i = 0; i < mcount; i++)
            {
                var message = deviceDebugInfoQueue.GetMessage(i);
                string desc = message.Description.Replace('\0', ' ');
                string txt = $"({i + 1}){message.Id}[{message.Severity}]-{message.Category}=>{desc}";
                sb.AppendLine(txt);
            }

            deviceDebugInfoQueue.ClearStoredMessages();

            return sb.ToString();
        }
#endif

        /// <summary>
        /// Present frame
        /// </summary>
        public void Present()
        {
            uint syncInterval = vsyncEnabled ? 1u : 0u;
            var res = swapChain.Present(syncInterval, PresentFlags.None);

            if (!res.Success)
            {
                Game.Logger.WriteError(this, $"Error presenting Graphics: Code {res.Code}");
            }
        }

        /// <summary>
        /// Gets the removed device reason
        /// </summary>
        public bool GetRemovedDeviceStatus(out string removedReason)
        {
            var res = device.DeviceRemovedReason;
            if (res.Failure)
            {
                removedReason = res.ToString();

                return true;
            }

            removedReason = null;

            return false;
        }

        /// <summary>
        /// Creates a new deferred context
        /// </summary>
        /// <param name="name">Deferred context name</param>
        /// <param name="passIndex">Pass index</param>
        public IEngineDeviceContext CreateDeferredContext(string name, int passIndex)
        {
            return new EngineDeviceContext(name, false, passIndex, device.CreateDeferredContext3());
        }

        /// <summary>
        /// Creates a new depth stencil state
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <returns>Returns a new depth stencil state</returns>
        public EngineDepthStencilState CreateDepthStencilState(string name, EngineDepthStencilStateDescription description)
        {
            return new(name, device.CreateDepthStencilState((DepthStencilDescription)description));
        }

        /// <summary>
        /// Creates a new blend state
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="blendFactor">Blend factor</param>
        /// <param name="sampleMask">Sample mask</param>
        /// <returns>Returns a new blend state</returns>
        public EngineBlendState CreateBlendState(string name, EngineBlendStateDescription description, Vector4 blendFactor, uint sampleMask)
        {
            return new(
                name,
                device.CreateBlendState1((BlendDescription1)description),
                blendFactor,
                sampleMask);
        }

        /// <summary>
        /// Creates a new rasterizer state
        /// </summary>
        /// <param name="description">Description</param>
        /// <returns>Returns a new rasterizer state</returns>
        public EngineRasterizerState CreateRasterizerState(string name, EngineRasterizerStateDescription description)
        {
            return new(
                name,
                device.CreateRasterizerState2((RasterizerDescription2)description));
        }

        /// <summary>
        /// Creates a new Sampler state
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description">Sampler description</param>
        /// <returns>Returns the new sampler state</returns>
        public EngineSamplerState CreateSamplerState(string name, EngineSamplerStateDescription description)
        {
            return new(
                name,
                device.CreateSamplerState((SamplerDescription)description));
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
        public EngineDepthStencilView CreateDepthStencilBuffer(string name, Format format, uint width, uint height, bool useSamples)
        {
            bool multiSampled = false;
            var sampleDescription = new SampleDescription(1, 0);
            if (useSamples)
            {
                multiSampled = MultiSampled;
                sampleDescription = CurrentSampleDescription;
            }

            var textureDesc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = format,
                SampleDescription = sampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
            };
            using var texture = device.CreateTexture2D1(textureDesc);
            texture.DebugName = $"{name}.TEX";

            var description = new DepthStencilViewDescription()
            {
                Format = format,
                ViewDimension = multiSampled ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                Texture2D = new(),
                Texture2DMS = new(),
            };
            var dsv = device.CreateDepthStencilView(texture, description);

            return new($"{name}.DSV", dsv);
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
        public (EngineRenderTargetView RenderTarget, EngineShaderResourceView ShaderResource) CreateRenderTargetTexture(string name, Format format, uint width, uint height, bool useSamples)
        {
            FrameCounters.Textures++;

            bool multiSampled = useSamples && MultiSampled;

            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = format,
                SampleDescription = useSamples ? CurrentSampleDescription : new(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            };
            var texture = device.CreateTexture2D1(desc);
            texture.DebugName = $"{name}.TEX";

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
                    rtvDesc.ViewDimension = RenderTargetViewDimension.Texture2DMultisampled;
                    rtvDesc.Texture2DMS = new();

                    srvDesc.ViewDimension = ShaderResourceViewDimension.Texture2DMultisampled;
                    srvDesc.Texture2DMS = new();
                }
                else
                {
                    rtvDesc.ViewDimension = RenderTargetViewDimension.Texture2D;
                    rtvDesc.Texture2D = new();

                    srvDesc.ViewDimension = ShaderResourceViewDimension.Texture2D;
                    srvDesc.Texture2D = new() { MipLevels = 1 };
                }

                var rtv1 = device.CreateRenderTargetView1(texture, rtvDesc);
                rtv1.DebugName = $"{name}.RTV";
                var rtv = new EngineRenderTargetView($"{name}.RTV", rtv1);

                var srv1 = device.CreateShaderResourceView1(texture, srvDesc);
                srv1.DebugName = $"{name}.SRV";
                var srv = new EngineShaderResourceView($"{name}.SRV", srv1);

                return (rtv, srv);
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
        public (EngineRenderTargetView RenderTarget, EngineShaderResourceView[] ShaderResources) CreateRenderTargetTexture(string name, Format format, uint width, uint height, uint arraySize, bool useSamples)
        {
            FrameCounters.Textures++;

            var rtv = new EngineRenderTargetView($"{name}.RTV_array({arraySize})");
            var srv = new EngineShaderResourceView[arraySize];

            bool multiSampled = useSamples && MultiSampled;

            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = format,
                SampleDescription = useSamples ? CurrentSampleDescription : new(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            };

            for (int i = 0; i < arraySize; i++)
            {
                var texture = device.CreateTexture2D1(desc);
                texture.DebugName = $"{name}.TEX_array({arraySize})[{i}]";
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
                        rtvDesc.ViewDimension = RenderTargetViewDimension.Texture2DMultisampled;
                        rtvDesc.Texture2DMS = new();

                        srvDesc.ViewDimension = ShaderResourceViewDimension.Texture2DMultisampled;
                        srvDesc.Texture2DMS = new();
                    }
                    else
                    {
                        rtvDesc.ViewDimension = RenderTargetViewDimension.Texture2D;
                        rtvDesc.Texture2D = new();

                        srvDesc.ViewDimension = ShaderResourceViewDimension.Texture2D;
                        srvDesc.Texture2D = new() { MipLevels = 1 };
                    }

                    string rtvName = $"{name}.RTV_array({arraySize})[{i}]";
                    string srvName = $"{name}.SRV_array({arraySize})[{i}]";

                    var rtv1 = device.CreateRenderTargetView1(texture, rtvDesc);
                    rtv1.DebugName = rtvName;
                    rtv.Add(rtv1);

                    var srv1 = device.CreateShaderResourceView1(texture, srvDesc);
                    srv1.DebugName = srvName;
                    srv[i] = new(srvName, srv1);
                }
            }

            return (rtv, srv);
        }

        /// <summary>
        /// Creates a set of texture and depth stencil view for shadow mapping
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns the depth stencil buffer and its texture</returns>
        public (EngineDepthStencilView DepthStencil, EngineShaderResourceView ShaderResource) CreateShadowMapTextures(string name, uint width, uint height)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R24G8_Typeless,
                SampleDescription = new(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            };
            var depthMap = device.CreateTexture2D1(desc);
            depthMap.DebugName = $"{name}.TEX";
            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    ViewDimension = DepthStencilViewDimension.Texture2D,
                    Texture2D = new()
                    {
                        MipSlice = 0,
                    },
                };
                var dsv = new EngineDepthStencilView($"{name}.DSV", device.CreateDepthStencilView(depthMap, dsDescription));

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    ViewDimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV", device.CreateShaderResourceView1(depthMap, rvDescription));

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
        public (EngineDepthStencilView DepthStencil, EngineShaderResourceView ShaderResource) CreateShadowMapTextures(string name, uint width, uint height, uint mapCount)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = mapCount,
                Format = Format.R24G8_Typeless,
                SampleDescription = new(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None
            };
            var depthMap = device.CreateTexture2D1(desc);
            depthMap.DebugName = $"{name}.TEX_array({mapCount})";
            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    ViewDimension = DepthStencilViewDimension.Texture2DArray,
                    Texture2DArray = new()
                    {
                        ArraySize = mapCount,
                        FirstArraySlice = 0,
                        MipSlice = 0,
                    },
                };
                var dsv = new EngineDepthStencilView($"{name}.DSV_array({mapCount})", device.CreateDepthStencilView(depthMap, dsDescription));

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    ViewDimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV_array({mapCount})", device.CreateShaderResourceView1(depthMap, rvDescription));

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
        public (IEnumerable<EngineDepthStencilView> DepthStencils, EngineShaderResourceView ShaderResource) CreateShadowMapTextureArrays(string name, uint width, uint height, uint mapCount, uint arraySize)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = arraySize * mapCount,
                Format = Format.R24G8_Typeless,
                SampleDescription = new(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.None,
                TextureLayout = TextureLayout.Undefined,
            };
            var depthMap = device.CreateTexture2D1(desc);
            depthMap.DebugName = $"{name}.TEX_array({arraySize},{mapCount})";
            using (depthMap)
            {
                var dsv = new EngineDepthStencilView[arraySize];
                for (uint i = 0; i < arraySize; i++)
                {
                    var dsDescription = new DepthStencilViewDescription
                    {
                        Flags = DepthStencilViewFlags.None,
                        Format = Format.D24_UNorm_S8_UInt,
                        ViewDimension = DepthStencilViewDimension.Texture2DArray,
                        Texture2DArray = new()
                        {
                            ArraySize = mapCount,
                            FirstArraySlice = i,
                            MipSlice = 0,
                        },
                    };
                    dsv[i] = new($"{name}.DSV_array({arraySize},{mapCount})[{i}]", device.CreateDepthStencilView(depthMap, dsDescription));
                }

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    ViewDimension = ShaderResourceViewDimension.Texture2DArray,
                    Texture2DArray = new()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                        ArraySize = arraySize * mapCount,
                        FirstArraySlice = 0,
                        PlaneSlice = 0,
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV_array({arraySize},{mapCount})", device.CreateShaderResourceView1(depthMap, rvDescription));

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
        public (EngineDepthStencilView DepthStencil, EngineShaderResourceView ShaderResource) CreateCubicShadowMapTextures(string name, uint width, uint height)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                ArraySize = 6,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CPUAccessFlags = CpuAccessFlags.None,
                Format = Format.R24G8_Typeless,
                MipLevels = 1,
                MiscFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                TextureLayout = TextureLayout.Undefined,
            };
            var depthMap = device.CreateTexture2D1(desc);
            depthMap.DebugName = $"{name}.TEX_cube";
            using (depthMap)
            {
                var dsDescription = new DepthStencilViewDescription
                {
                    Flags = DepthStencilViewFlags.None,
                    Format = Format.D24_UNorm_S8_UInt,
                    ViewDimension = DepthStencilViewDimension.Texture2DArray,
                    Texture2DArray = new()
                    {
                        ArraySize = 6,
                        FirstArraySlice = 0,
                        MipSlice = 0,
                    },
                };
                var dsv = new EngineDepthStencilView($"{name}.DSV_cube", device.CreateDepthStencilView(depthMap, dsDescription));

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    ViewDimension = ShaderResourceViewDimension.TextureCube,
                    TextureCube = new()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV_cube", device.CreateShaderResourceView1(depthMap, rvDescription));

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
        public (IEnumerable<EngineDepthStencilView> DepthStencils, EngineShaderResourceView ShaderResource) CreateCubicShadowMapTextureArrays(string name, uint width, uint height, uint arraySize)
        {
            var desc = new Texture2DDescription1()
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = arraySize * 6,
                Format = Format.R24G8_Typeless,
                SampleDescription = new(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None,
                MiscFlags = ResourceOptionFlags.TextureCube,
                TextureLayout = TextureLayout.Undefined,
            };
            var depthMap = device.CreateTexture2D1(desc);
            depthMap.DebugName = $"{name}.TEX_cube_array({arraySize})";
            using (depthMap)
            {
                var dsv = new EngineDepthStencilView[arraySize];
                for (uint i = 0; i < arraySize; i++)
                {
                    var dsDescription = new DepthStencilViewDescription
                    {
                        Flags = DepthStencilViewFlags.None,
                        Format = Format.D24_UNorm_S8_UInt,
                        ViewDimension = DepthStencilViewDimension.Texture2DArray,
                        Texture2DArray = new()
                        {
                            ArraySize = 6,
                            FirstArraySlice = i * 6,
                            MipSlice = 0,
                        },
                    };
                    dsv[i] = new($"{name}.DSV_cube_array({arraySize})[{i}]", device.CreateDepthStencilView(depthMap, dsDescription));
                }

                var rvDescription = new ShaderResourceViewDescription1
                {
                    Format = Format.R24_UNorm_X8_Typeless,
                    ViewDimension = ShaderResourceViewDimension.TextureCubeArray,
                    TextureCubeArray = new()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                        NumCubes = arraySize,
                        First2DArrayFace = 0,
                    },
                };
                var srv = new EngineShaderResourceView($"{name}.SRV_cube_array({arraySize})", device.CreateShaderResourceView1(depthMap, rvDescription));

                return (dsv, srv);
            }
        }
    }
}
