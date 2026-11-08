using Engine.Common;
using Engine.Helpers;
using Engine.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Debug;
using Vortice.DXGI;

namespace Engine
{
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
        /// Sync interval
        /// </summary>
        private readonly uint syncInterval = 0;
        /// <summary>
        /// Multisample count
        /// </summary>
        private readonly uint msCount = 1;
        /// <summary>
        /// Multisample quality
        /// </summary>
        private readonly uint msQuality = 0;
        /// <summary>
        /// Graphics factory
        /// </summary>
        private IDXGIFactory2 factory = null;
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
        /// Color space
        /// </summary>
        private ColorSpaceType colorSpace = ColorSpaceType.RgbFullG22NoneP709;

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
            ArgumentNullException.ThrowIfNull(form);

            syncInterval = vsyncEnabled ? 1u : 0u;

#if DEBUG
            factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(true);
#else
            factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false);
#endif
            if (factory == null) throw new InvalidOperationException($"Cannot detect {nameof(IDXGIFactory2)}.");
            factory.DebugName = "Graphics Factory6";

            using (var adapter = GetHardwareAdapter())
            {
                DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
                if (D3D11.SdkLayersAvailable())
                {
                    creationFlags |= DeviceCreationFlags.Debug;
                }
#endif
                if (D3D11.D3D11CreateDevice(
                    adapter,
                    DriverType.Unknown,
                    creationFlags,
                    [FeatureLevel.Level_11_1, FeatureLevel.Level_11_0],
                    out ID3D11Device tmpDevice, out FeatureLevel _, out ID3D11DeviceContext tempContext).Failure)
                {
                    // If the initialization fails, fall back to the WARP device.
                    // For more information on WARP, see:
                    // http://go.microsoft.com/fwlink/?LinkId=286690
                    D3D11.D3D11CreateDevice(
                        IntPtr.Zero,
                        DriverType.Warp,
                        creationFlags,
                        [FeatureLevel.Level_11_1, FeatureLevel.Level_11_0],
                        out tmpDevice, out FeatureLevel _, out tempContext).CheckError();
                }

                device = tmpDevice.QueryInterface<ID3D11Device3>();
                device.DebugName = "Graphics Device";
            }

            if (multiSampling != 0 && !CheckMultisample(device, bufferFormat, multiSampling, out msCount, out msQuality))
            {
                throw new EngineException($"The specified multisampling value [{multiSampling}] is not supported for {bufferFormat}");
            }

            var scdesc = new SwapChainDescription1()
            {
                Width = form.RenderWidth,
                Height = form.RenderHeight,
                Format = bufferFormat,
                BufferCount = 2,
                BufferUsage = Usage.RenderTargetOutput,
                SampleDescription = CurrentSampleDescription,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = AlphaMode.Ignore,
            };

            var fsdesc = new SwapChainFullscreenDescription()
            {
                Windowed = !form.IsFullscreen,
            };

            using var tmpSwapChain = factory.CreateSwapChainForHwnd(device, form.Handle, scdesc, fsdesc);
            swapChain = tmpSwapChain.QueryInterface<IDXGISwapChain4>() ?? throw new InvalidOperationException($"Cannot create swap chain {nameof(IDXGISwapChain4)}.");
            swapChain.DebugName = "Graphics Swap Chain";

            UpdateColorSpace(form);

            immediateContext = new("Immediate Context", true, -1, device.ImmediateContext3);

            PrepareDevice(form.RenderWidth, form.RenderHeight, false);

            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);

#if DEBUG
            ConfigureDebugLayer();
#endif
        }
        /// <summary>
        /// Gets the hardware adapter
        /// </summary>
        private IDXGIAdapter1 GetHardwareAdapter()
        {
            using var factory6 = factory.QueryInterface<IDXGIFactory6>() ?? throw new InvalidOperationException($"Cannot detect {nameof(IDXGIFactory6)}.");
            factory6.DebugName = "Graphics Factory6";

            for (uint idx = 0; factory6.EnumAdapterByGpuPreference(idx, GpuPreference.HighPerformance, out IDXGIAdapter1 adapter1).Success; idx++)
            {
                if (adapter1 == null) continue;

                var desc = adapter1.Description1;

                if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
                {
                    adapter1.Dispose();
                    continue;
                }

                adapter1.DebugName = $"Graphics Adapter - {desc.Description}";
                return adapter1;
            }

            for (uint idx = 0; factory.EnumAdapters1(idx, out IDXGIAdapter1 adapter1).Success; idx++)
            {
                if (adapter1 == null) continue;

                var desc = adapter1.Description1;

                if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
                {
                    adapter1.Dispose();
                    continue;
                }

                adapter1.DebugName = $"Graphics Adapter - {desc.Description}";
                return adapter1;
            }

            throw new InvalidOperationException("Cannot detect D3D11 adapter.");
        }
        private void UpdateColorSpace(IEngineForm form)
        {
            bool isDisplayHDR10 = false;

            var windowBounds = form.RenderRectangle;

            IDXGIOutput bestOutput = default;
            long bestIntersectArea = -1;

            for (uint idx = 0; factory.EnumAdapters1(idx, out IDXGIAdapter1 adapter).Success; idx++)
            {
                for (uint outIdx = 0; adapter.EnumOutputs(outIdx, out IDXGIOutput output).Success; outIdx++)
                {
                    // Get the rectangle bounds of current output.
                    var r = output.Description.DesktopCoordinates;

                    // Compute the intersection.
                    int intersectArea = RectI.ComputeIntersectionArea(windowBounds, r);
                    if (intersectArea > bestIntersectArea)
                    {
                        bestOutput = output;
                        bestIntersectArea = intersectArea;
                    }
                    else
                    {
                        output?.Dispose();
                    }
                }

                adapter.Dispose();
            }

            using var output6 = bestOutput?.QueryInterfaceOrNull<IDXGIOutput6>();
            if (output6 != null)
            {
                var outputDesc = output6.Description1;
                if (outputDesc.ColorSpace == ColorSpaceType.RgbFullG2084NoneP2020)
                {
                    // Display output is HDR10.
                    isDisplayHDR10 = true;
                }
            }

            if (isDisplayHDR10)
            {
                colorSpace = bufferFormat switch
                {
                    // The application creates the HDR10 signal.
                    Format.R10G10B10A2_UNorm => ColorSpaceType.RgbFullG2084NoneP2020,
                    // The system creates the HDR10 signal, application uses linear values.
                    Format.R16G16B16A16_Float => ColorSpaceType.RgbFullG10NoneP709,
                    _ => ColorSpaceType.RgbFullG22NoneP709,
                };
            }

            using var swapChain3 = swapChain.QueryInterfaceOrNull<IDXGISwapChain3>();
            if (swapChain3 == null) return;

            var colorSpaceSupport = swapChain3.CheckColorSpaceSupport(colorSpace);
            if ((colorSpaceSupport & SwapChainColorSpaceSupportFlags.Present) != SwapChainColorSpaceSupportFlags.None)
            {
                swapChain3.SetColorSpace1(colorSpace);
            }
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
        /// Gets desktop mode description
        /// </summary>
        /// <returns>Returns current desktop mode description</returns>
        public static RectI GetDesktopMode()
        {
            using var factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
            using var factory5 = factory.QueryInterface<IDXGIFactory5>();
            factory5.EnumAdapters1(0, out var adapter);
            using (adapter)
            {
                using var adapter4 = adapter.QueryInterface<IDXGIAdapter4>();
                adapter4.EnumOutputs(0, out var adapterOutput);
                using (adapterOutput)
                {
                    using var adapterOutput6 = adapterOutput.QueryInterface<IDXGIOutput6>();
                    return adapterOutput6.Description1.DesktopCoordinates;
                }
            }
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

            factory?.Dispose();
            factory = null;

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

            Viewport = new()
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
