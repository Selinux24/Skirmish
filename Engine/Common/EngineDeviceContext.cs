using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.Mathematics.Interop;
    using Format = SharpDX.DXGI.Format;

    /// <summary>
    /// Engine device context
    /// </summary>
    public class EngineDeviceContext : IEngineDeviceContext
    {
        #region Support classes

        /// <summary>
        /// Shader constant buffers state
        /// </summary>
        class ShaderConstantBufferState(EngineDeviceContext deviceContext) : EngineShaderStageState<IEngineConstantBuffer>(deviceContext)
        {
            /// <summary>
            /// Null constant buffers for shader clearing
            /// </summary>
            private static readonly Buffer[] nullBuffers = new Buffer[CommonShaderStage.ConstantBufferApiSlotCount];

            public void SetConstantBuffer(CommonShaderStage shaderStage, int slot, IEngineConstantBuffer buffer)
            {
                Update(slot, buffer);

                var buffers = Resources.Select(r => r?.Buffer.GetBuffer()).ToArray();
                shaderStage.SetConstantBuffers(StartSlot, buffers.Length, buffers);
                DeviceContext.frameCounters.ConstantBufferSets++;
            }

            public void SetConstantBuffers(CommonShaderStage shaderStage, int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
            {
                Update(startSlot, bufferList);

                var buffers = Resources.Select(r => r?.Buffer.GetBuffer()).ToArray();
                shaderStage.SetConstantBuffers(StartSlot, buffers.Length, buffers);
                DeviceContext.frameCounters.ConstantBufferSets++;
            }

            public void Clear(CommonShaderStage shaderStage)
            {
                shaderStage.SetConstantBuffers(StartSlot, nullBuffers.Length - StartSlot, nullBuffers);
                DeviceContext.frameCounters.ConstantBufferClears++;

                Clear();
            }
        }
        /// <summary>
        /// Shader resources state
        /// </summary>
        class ShaderResourceState(EngineDeviceContext deviceContext) : EngineShaderStageState<EngineShaderResourceView>(deviceContext)
        {
            /// <summary>
            /// Null shader resources for shader clearing
            /// </summary>
            private static readonly ShaderResourceView[] nullSrv = new ShaderResourceView[CommonShaderStage.InputResourceSlotCount];

            public void SetShaderResource(CommonShaderStage shaderStage, int slot, EngineShaderResourceView resourceView)
            {
                Update(slot, resourceView);

                var resources = Resources.Select(r => r?.GetResource()).ToArray();
                shaderStage.SetShaderResources(StartSlot, resources.Length, resources);
                DeviceContext.frameCounters.ShaderResourceSets++;
            }

            public void SetShaderResources(CommonShaderStage shaderStage, int startSlot, IEnumerable<EngineShaderResourceView> resourceList)
            {
                Update(startSlot, resourceList);

                var resources = Resources.Select(r => r?.GetResource()).ToArray();
                shaderStage.SetShaderResources(StartSlot, resources.Length, resources);
                DeviceContext.frameCounters.ShaderResourceSets++;
            }

            public void Clear(CommonShaderStage shaderStage)
            {
                shaderStage.SetShaderResources(StartSlot, nullSrv.Length - StartSlot, nullSrv);
                DeviceContext.frameCounters.ShaderResourceClears++;

                Clear();
            }
        }
        /// <summary>
        /// Shader samplers state
        /// </summary>
        class ShaderSamplerState(EngineDeviceContext deviceContext) : EngineShaderStageState<EngineSamplerState>(deviceContext)
        {
            /// <summary>
            /// Null samplers for shader clearing
            /// </summary>
            private static readonly SamplerState[] nullSamplers = new SamplerState[CommonShaderStage.SamplerSlotCount];

            public void SetSampler(CommonShaderStage shaderStage, int slot, EngineSamplerState samplerState)
            {
                Update(slot, samplerState);

                var resources = Resources.Select(r => r?.GetSamplerState()).ToArray();
                shaderStage.SetSamplers(StartSlot, resources.Length, resources);
                DeviceContext.frameCounters.SamplerSets++;
            }

            public void SetSamplers(CommonShaderStage shaderStage, int startSlot, IEnumerable<EngineSamplerState> samplerList)
            {
                Update(startSlot, samplerList);

                var samplers = Resources.Select(r => r?.GetSamplerState()).ToArray();
                shaderStage.SetSamplers(StartSlot, samplers.Length, samplers);
                DeviceContext.frameCounters.SamplerSets++;
            }

            public void Clear(CommonShaderStage shaderStage)
            {
                shaderStage.SetSamplers(StartSlot, nullSamplers.Length - StartSlot, nullSamplers);
                DeviceContext.frameCounters.SamplerClears++;

                Clear();
            }
        }

        #endregion

        /// <summary>
        /// Internal device context
        /// </summary>
        private readonly DeviceContext3 deviceContext = null;
        /// <summary>
        /// Current primitive topology set in input assembler
        /// </summary>
        private Topology currentIAPrimitiveTopology = Topology.Undefined;
        /// <summary>
        /// Current input layout set in input assembler
        /// </summary>
        private EngineInputLayout currentIAInputLayout = null;
        /// <summary>
        /// Current viewport
        /// </summary>
        private RawViewportF[] currentViewports;

        /// <summary>
        /// Current depth-stencil state
        /// </summary>
        private EngineDepthStencilState currentDepthStencilState = null;
        /// <summary>
        /// Current depth-stencil reference
        /// </summary>
        private int currentDepthStencilRef = 0;
        /// <summary>
        /// Current blend state
        /// </summary>
        private EngineBlendState currentBlendState = null;
        /// <summary>
        /// Current rasterizer state
        /// </summary>
        private EngineRasterizerState currentRasterizerState = null;

        /// <summary>
        /// Current vertex shader
        /// </summary>
        private EngineVertexShader currentVertexShader;
        /// <summary>
        /// Current vertex shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentVertexShaderConstantBufferState;
        /// <summary>
        /// Current vertex shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentVertexShaderResourceViewState;
        /// <summary>
        /// Current vertex shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentVertexShaderSamplerState;

        /// <summary>
        /// Current hull shader
        /// </summary>
        private EngineHullShader currentHullShader;
        /// <summary>
        /// Current hull shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentHullShaderConstantBufferState;
        /// <summary>
        /// Current hull shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentHullShaderResourceViewState;
        /// <summary>
        /// Current hull shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentHullShaderSamplerState;

        /// <summary>
        /// Current domain shader
        /// </summary>
        private EngineDomainShader currentDomainShader;
        /// <summary>
        /// Current domain shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentDomainShaderConstantBufferState;
        /// <summary>
        /// Current domain shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentDomainShaderResourceViewState;
        /// <summary>
        /// Current domain shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentDomainShaderSamplerState;

        /// <summary>
        /// Current geometry shader
        /// </summary>
        private EngineGeometryShader currentGeomeryShader;
        /// <summary>
        /// Current geometry shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentGeometryShaderConstantBufferState;
        /// <summary>
        /// Current geometry shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentGeometryShaderResourceViewState;
        /// <summary>
        /// Current geometry shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentGeometryShaderSamplerState;

        /// <summary>
        /// Current stream output bindings
        /// </summary>
        private IEnumerable<EngineStreamOutputBufferBinding> currentStreamOutputBindings;

        /// <summary>
        /// Current pixel shader
        /// </summary>
        private EnginePixelShader currentPixelShader;
        /// <summary>
        /// Current pixel shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentPixelShaderConstantBufferState;
        /// <summary>
        /// Current pixel shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentPixelShaderResourceViewState;
        /// <summary>
        /// Current pixel shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentPixelShaderSamplerState;

        /// <summary>
        /// Current compute shader
        /// </summary>
        private EngineComputeShader currentComputeShader;
        /// <summary>
        /// Current compute shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentComputeShaderConstantBufferState;
        /// <summary>
        /// Current compute shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentComputeShaderResourceViewState;
        /// <summary>
        /// Current compute shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentComputeShaderSamplerState;

        /// <summary>
        /// Current vertex buffer first slot
        /// </summary>
        private int currentVertexBufferFirstSlot = -1;
        /// <summary>
        /// Current vertex buffer bindings
        /// </summary>
        private IEnumerable<EngineVertexBufferBinding> currentVertexBufferBindings = null;
        /// <summary>
        /// Current index buffer reference
        /// </summary>
        private EngineBuffer currentIndexBufferRef = null;
        /// <summary>
        /// Current index buffer format
        /// </summary>
        private Format currentIndexFormat = Format.Unknown;
        /// <summary>
        /// Current index buffer offset
        /// </summary>
        private int currentIndexOffset = -1;

        /// <summary>
        /// Frame counters class
        /// </summary>
        private readonly PassCounters frameCounters = null;

        /// <inheritdoc/>
        public string Name { get; private set; }
        /// <inheritdoc/>
        public bool IsImmediateContext { get; private set; }
        /// <inheritdoc/>
        public int PassIndex { get; private set; }
        /// <inheritdoc/>
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
                    frameCounters.IAPrimitiveTopologySets++;

                    currentIAPrimitiveTopology = value;
                }
            }
        }
        /// <inheritdoc/>
        public EngineInputLayout IAInputLayout
        {
            get
            {
                return currentIAInputLayout;
            }
            set
            {
                if (currentIAInputLayout != value)
                {
                    deviceContext.InputAssembler.InputLayout = value?.GetInputLayout();
                    frameCounters.IAInputLayoutSets++;

                    currentIAInputLayout = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="immediate">Is the immediate context</param>
        /// <param name="passIndex">Pass index</param>
        /// <param name="deviceContext">Device context</param>
        internal EngineDeviceContext(string name, bool immediate, int passIndex, DeviceContext3 deviceContext)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A device context name must be specified.");
            this.deviceContext = deviceContext ?? throw new ArgumentNullException(nameof(deviceContext), "A device context must be specified.");

            IsImmediateContext = immediate;

            PassIndex = passIndex;

            frameCounters = FrameCounters.CreatePassCounters(name, passIndex);

            currentVertexShaderConstantBufferState = new(this);
            currentVertexShaderResourceViewState = new(this);
            currentVertexShaderSamplerState = new(this);

            currentHullShaderConstantBufferState = new(this);
            currentHullShaderResourceViewState = new(this);
            currentHullShaderSamplerState = new(this);

            currentDomainShaderConstantBufferState = new(this);
            currentDomainShaderResourceViewState = new(this);
            currentDomainShaderSamplerState = new(this);

            currentGeometryShaderConstantBufferState = new(this);
            currentGeometryShaderResourceViewState = new(this);
            currentGeometryShaderSamplerState = new(this);

            currentPixelShaderConstantBufferState = new(this);
            currentPixelShaderResourceViewState = new(this);
            currentPixelShaderSamplerState = new(this);

            currentComputeShaderConstantBufferState = new(this);
            currentComputeShaderResourceViewState = new(this);
            currentComputeShaderSamplerState = new(this);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineDeviceContext()
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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                deviceContext?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void ClearState()
        {
            deviceContext.ClearState();
            frameCounters.ContextClears++;

            currentIAPrimitiveTopology = Topology.Undefined;
            currentIAInputLayout = null;
            currentViewports = null;

            currentDepthStencilState = null;
            currentDepthStencilRef = 0;
            currentBlendState = null;
            currentRasterizerState = null;

            currentVertexShader = null;
            currentHullShader = null;
            currentDomainShader = null;
            currentGeomeryShader = null;
            currentStreamOutputBindings = null;
            currentPixelShader = null;
            currentComputeShader = null;

            currentVertexBufferFirstSlot = -1;
            currentVertexBufferBindings = null;
            currentIndexBufferRef = null;
            currentIndexFormat = Format.Unknown;
            currentIndexOffset = -1;
        }

        /// <inheritdoc/>
        public void SetViewport(Viewport viewport)
        {
            SetViewPorts([(RawViewportF)viewport]);
        }
        /// <inheritdoc/>
        public void SetViewport(ViewportF viewport)
        {
            SetViewPorts([(RawViewportF)viewport]);
        }
        /// <inheritdoc/>
        public void SetViewports(Viewport[] viewports)
        {
            SetViewPorts(viewports.Select(v => (RawViewportF)v).ToArray());
        }
        /// <inheritdoc/>
        public void SetViewports(ViewportF[] viewports)
        {
            SetViewPorts(viewports.Select(v => (RawViewportF)v).ToArray());
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        private void SetViewPorts(RawViewportF[] viewports)
        {
            if (Helper.CompareEnumerables(currentViewports, viewports))
            {
                return;
            }

            deviceContext.Rasterizer.SetViewports(viewports);
            frameCounters.ViewportsSets++;

            currentViewports = viewports;
        }

        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, EngineDepthStencilView depthMap)
        {
            deviceContext.OutputMerger.SetTargets(depthMap.GetDepthStencil(), renderTargets.GetRenderTarget());
            frameCounters.RenderTargetSets++;
        }
        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor)
        {
            SetRenderTargets(renderTargets, clearRT, clearRTColor, false);
        }
        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, bool freeOMResources)
        {
            if (freeOMResources)
            {
                ClearShaderResources();
            }

            var rtv = renderTargets?.GetRenderTargets() ?? [];
            var rtvCount = rtv.Count();

            deviceContext.OutputMerger.SetTargets(null, rtvCount, rtv.ToArray());
            frameCounters.RenderTargetSets++;

            if (clearRT && rtvCount > 0)
            {
                for (int i = 0; i < rtvCount; i++)
                {
                    deviceContext.ClearRenderTargetView(rtv.ElementAt(i), clearRTColor);
                    frameCounters.RenderTargetClears++;
                }
            }
        }
        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil, bool freeOMResources)
        {
            if (freeOMResources)
            {
                ClearShaderResources();
            }

            var dsv = depthMap?.GetDepthStencil();
            var rtv = renderTargets?.GetRenderTargets() ?? [];
            var rtvCount = rtv.Count();

            deviceContext.OutputMerger.SetTargets(dsv, 0, [], [], rtv.ToArray());
            frameCounters.RenderTargetSets++;

            if (clearRT && rtvCount > 0)
            {
                for (int i = 0; i < rtvCount; i++)
                {
                    deviceContext.ClearRenderTargetView(rtv.ElementAt(i), clearRTColor);
                    frameCounters.RenderTargetClears++;
                }
            }

            ClearDepthStencilBuffer(depthMap, clearDepth, clearStencil);
        }

        /// <inheritdoc/>
        public void ClearDepthStencilBuffer(EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil)
        {
            if ((clearDepth || clearStencil) && depthMap != null)
            {
                DepthStencilClearFlags clearDSFlags = 0;
                if (clearDepth) clearDSFlags |= DepthStencilClearFlags.Depth;
                if (clearStencil) clearDSFlags |= DepthStencilClearFlags.Stencil;

                deviceContext.ClearDepthStencilView(depthMap.GetDepthStencil(), clearDSFlags, 1.0f, 0);
                frameCounters.DepthStencilClears++;
            }
        }
        /// <inheritdoc/>
        public void SetDepthStencilState(EngineDepthStencilState state, int stencilRef = 0)
        {
            if (currentDepthStencilState == state && currentDepthStencilRef == stencilRef)
            {
                return;
            }

            deviceContext.OutputMerger.SetDepthStencilState(state.GetDepthStencilState(), stencilRef);
            frameCounters.DepthStencilStateChanges++;

            currentDepthStencilState = state;
            currentDepthStencilRef = stencilRef;
        }

        /// <inheritdoc/>
        public void SetRasterizerState(EngineRasterizerState state)
        {
            if (currentRasterizerState == state)
            {
                return;
            }

            deviceContext.Rasterizer.State = state.GetRasterizerState();
            frameCounters.RasterizerStateChanges++;

            currentRasterizerState = state;
        }
        /// <inheritdoc/>
        public void SetBlendState(EngineBlendState state)
        {
            if (currentBlendState == state)
            {
                return;
            }

            deviceContext.OutputMerger.SetBlendState(state.GetBlendState(), state.BlendFactor, state.SampleMask);
            frameCounters.OMBlendStateChanges++;

            currentBlendState = state;
        }

        /// <inheritdoc/>
        public void IASetVertexBuffers(int firstSlot, EngineVertexBufferBinding[] vertexBufferBindings)
        {
            if (currentVertexBufferFirstSlot != firstSlot || !Helper.CompareEnumerables(currentVertexBufferBindings, vertexBufferBindings))
            {
                var vbBindings = vertexBufferBindings?.Select(v => v.GetVertexBufferBinding()).ToArray() ?? [];

                deviceContext.InputAssembler.SetVertexBuffers(firstSlot, vbBindings);
                frameCounters.IAVertexBuffersSets++;

                currentVertexBufferFirstSlot = firstSlot;
                currentVertexBufferBindings = vertexBufferBindings;
            }
        }
        /// <inheritdoc/>
        public void IASetIndexBuffer(EngineBuffer indexBufferRef, Format format, int offset)
        {
            if (currentIndexBufferRef != indexBufferRef || currentIndexFormat != format || currentIndexOffset != offset)
            {
                deviceContext.InputAssembler.SetIndexBuffer(indexBufferRef?.GetBuffer(), format, offset);
                frameCounters.IAIndexBufferSets++;

                currentIndexBufferRef = indexBufferRef;
                currentIndexFormat = format;
                currentIndexOffset = offset;
            }
        }

        /// <inheritdoc/>
        public void ClearShaderResources()
        {
            var vs = deviceContext.VertexShader;
            currentVertexShaderConstantBufferState.Clear(vs);
            currentVertexShaderResourceViewState.Clear(vs);
            currentVertexShaderSamplerState.Clear(vs);

            var hs = deviceContext.HullShader;
            currentHullShaderConstantBufferState.Clear(hs);
            currentHullShaderResourceViewState.Clear(hs);
            currentHullShaderSamplerState.Clear(hs);

            var ds = deviceContext.DomainShader;
            currentDomainShaderConstantBufferState.Clear(ds);
            currentDomainShaderResourceViewState.Clear(ds);
            currentDomainShaderSamplerState.Clear(ds);

            var gs = deviceContext.GeometryShader;
            currentGeometryShaderConstantBufferState.Clear(gs);
            currentGeometryShaderResourceViewState.Clear(gs);
            currentGeometryShaderSamplerState.Clear(gs);

            var ps = deviceContext.PixelShader;
            currentPixelShaderConstantBufferState.Clear(ps);
            currentPixelShaderResourceViewState.Clear(ps);
            currentPixelShaderSamplerState.Clear(ps);

            var cs = deviceContext.ComputeShader;
            currentComputeShaderConstantBufferState.Clear(cs);
            currentComputeShaderResourceViewState.Clear(cs);
            currentComputeShaderSamplerState.Clear(cs);
        }

        /// <inheritdoc/>
        public void SetVertexShader(EngineVertexShader vertexShader)
        {
            if (currentVertexShader == vertexShader)
            {
                return;
            }

            deviceContext.VertexShader.Set(vertexShader?.GetShader());
            frameCounters.VertexShadersSets++;

            currentVertexShader = vertexShader;
        }
        /// <inheritdoc/>
        public void ClearVertexShader()
        {
            SetVertexShader(null);
        }
        /// <inheritdoc/>
        public void SetVertexShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentVertexShaderConstantBufferState.SetConstantBuffer(deviceContext.VertexShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetVertexShaderConstantBuffers(int startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentVertexShaderConstantBufferState.SetConstantBuffers(deviceContext.VertexShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetVertexShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentVertexShaderResourceViewState.SetShaderResource(deviceContext.VertexShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetVertexShaderResourceViews(int startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentVertexShaderResourceViewState.SetShaderResources(deviceContext.VertexShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetVertexShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentVertexShaderSamplerState.SetSampler(deviceContext.VertexShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetVertexShaderSamplers(int startSlot, EngineSamplerState[] samplerStates)
        {
            currentVertexShaderSamplerState.SetSamplers(deviceContext.VertexShader, startSlot, samplerStates);
        }

        /// <inheritdoc/>
        public void SetHullShader(EngineHullShader hullShader)
        {
            if (currentHullShader == hullShader)
            {
                return;
            }

            deviceContext.HullShader.Set(hullShader?.GetShader());
            frameCounters.HullShadersSets++;

            currentHullShader = hullShader;
        }
        /// <inheritdoc/>
        public void ClearHullShader()
        {
            SetHullShader(null);
        }
        /// <inheritdoc/>
        public void SetHullShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentHullShaderConstantBufferState.SetConstantBuffer(deviceContext.HullShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetHullShaderConstantBuffers(int startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentHullShaderConstantBufferState.SetConstantBuffers(deviceContext.HullShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetHullShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentHullShaderResourceViewState.SetShaderResource(deviceContext.HullShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetHullShaderResourceViews(int startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentHullShaderResourceViewState.SetShaderResources(deviceContext.HullShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetHullShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentHullShaderSamplerState.SetSampler(deviceContext.HullShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetHullShaderSamplers(int startSlot, EngineSamplerState[] samplerStates)
        {
            currentHullShaderSamplerState.SetSamplers(deviceContext.HullShader, startSlot, samplerStates);
        }

        /// <inheritdoc/>
        public void SetDomainShader(EngineDomainShader domainShader)
        {
            if (currentDomainShader == domainShader)
            {
                return;
            }

            deviceContext.DomainShader.Set(domainShader?.GetShader());
            frameCounters.DomainShadersSets++;

            currentDomainShader = domainShader;
        }
        /// <inheritdoc/>
        public void ClearDomainShader()
        {
            SetDomainShader(null);
        }
        /// <inheritdoc/>
        public void SetDomainShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentDomainShaderConstantBufferState.SetConstantBuffer(deviceContext.DomainShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetDomainShaderConstantBuffers(int startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentDomainShaderConstantBufferState.SetConstantBuffers(deviceContext.DomainShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetDomainShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentDomainShaderResourceViewState.SetShaderResource(deviceContext.DomainShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetDomainShaderResourceViews(int startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentDomainShaderResourceViewState.SetShaderResources(deviceContext.DomainShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetDomainShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentDomainShaderSamplerState.SetSampler(deviceContext.DomainShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetDomainShaderSamplers(int startSlot, EngineSamplerState[] samplerStates)
        {
            currentDomainShaderSamplerState.SetSamplers(deviceContext.DomainShader, startSlot, samplerStates);
        }

        /// <inheritdoc/>
        public void SetGeometryShader(EngineGeometryShader geometryShader)
        {
            if (currentGeomeryShader == geometryShader)
            {
                return;
            }

            deviceContext.GeometryShader.Set(geometryShader?.GetShader());
            frameCounters.GeometryShadersSets++;

            currentGeomeryShader = geometryShader;
        }
        /// <inheritdoc/>
        public void ClearGeometryShader()
        {
            SetGeometryShader(null);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentGeometryShaderConstantBufferState.SetConstantBuffer(deviceContext.GeometryShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderConstantBuffers(int startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentGeometryShaderConstantBufferState.SetConstantBuffers(deviceContext.GeometryShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentGeometryShaderResourceViewState.SetShaderResource(deviceContext.GeometryShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderResourceViews(int startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentGeometryShaderResourceViewState.SetShaderResources(deviceContext.GeometryShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentGeometryShaderSamplerState.SetSampler(deviceContext.GeometryShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderSamplers(int startSlot, EngineSamplerState[] samplerStates)
        {
            currentGeometryShaderSamplerState.SetSamplers(deviceContext.GeometryShader, startSlot, samplerStates);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderStreamOutputTargets(EngineStreamOutputBufferBinding[] streamOutBinding)
        {
            if (Helper.CompareEnumerables(currentStreamOutputBindings, streamOutBinding))
            {
                return;
            }

            var soBindings = streamOutBinding?.Select(s => s.GetStreamOutputBufferBinding()).ToArray();

            deviceContext.StreamOutput.SetTargets(soBindings);
            frameCounters.SOTargetsSets++;

            currentStreamOutputBindings = streamOutBinding;
        }

        /// <inheritdoc/>
        public void SetPixelShader(EnginePixelShader pixelShader)
        {
            if (currentPixelShader == pixelShader)
            {
                return;
            }

            deviceContext.PixelShader.Set(pixelShader?.GetShader());
            frameCounters.PixelShadersSets++;

            currentPixelShader = pixelShader;
        }
        /// <inheritdoc/>
        public void ClearPixelShader()
        {
            SetPixelShader(null);
        }
        /// <inheritdoc/>
        public void SetPixelShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentPixelShaderConstantBufferState.SetConstantBuffer(deviceContext.PixelShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetPixelShaderConstantBuffers(int startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentPixelShaderConstantBufferState.SetConstantBuffers(deviceContext.PixelShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetPixelShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentPixelShaderResourceViewState.SetShaderResource(deviceContext.PixelShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetPixelShaderResourceViews(int startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentPixelShaderResourceViewState.SetShaderResources(deviceContext.PixelShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetPixelShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentPixelShaderSamplerState.SetSampler(deviceContext.PixelShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetPixelShaderSamplers(int startSlot, EngineSamplerState[] samplerStates)
        {
            currentPixelShaderSamplerState.SetSamplers(deviceContext.PixelShader, startSlot, samplerStates);
        }

        /// <inheritdoc/>
        public void SetComputeShader(EngineComputeShader computeShader)
        {
            if (currentComputeShader == computeShader)
            {
                return;
            }

            deviceContext.ComputeShader.Set(computeShader?.GetShader());
            frameCounters.ComputeShadersSets++;

            currentComputeShader = computeShader;
        }
        /// <inheritdoc/>
        public void ClearComputeShader()
        {
            SetComputeShader(null);
        }
        /// <inheritdoc/>
        public void SetComputeShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentComputeShaderConstantBufferState.SetConstantBuffer(deviceContext.ComputeShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetComputeShaderConstantBuffers(int startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentComputeShaderConstantBufferState.SetConstantBuffers(deviceContext.ComputeShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetComputeShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentComputeShaderResourceViewState.SetShaderResource(deviceContext.ComputeShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetComputeShaderResourceViews(int startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentComputeShaderResourceViewState.SetShaderResources(deviceContext.ComputeShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetComputeShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentComputeShaderSamplerState.SetSampler(deviceContext.ComputeShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetComputeShaderSamplers(int startSlot, EngineSamplerState[] samplerStates)
        {
            currentComputeShaderSamplerState.SetSamplers(deviceContext.ComputeShader, startSlot, samplerStates);
        }

        /// <inheritdoc/>
        public void EffectPassApply(EngineEffectTechnique technique, int index, int flags)
        {
            technique.GetPass(index).Apply(deviceContext, flags);
            frameCounters.TechniquePasses++;
        }

        /// <inheritdoc/>
        public bool UpdateConstantBuffer<T>(EngineConstantBuffer<T> constantBuffer, T data) where T : struct, IBufferData
        {
            IEngineConstantBuffer buffer = constantBuffer;

            return UpdateConstantBuffer(buffer, data);
        }
        /// <inheritdoc/>
        public bool UpdateConstantBuffer(IEngineConstantBuffer constantBuffer, IBufferData data)
        {
            if (constantBuffer == null)
            {
                return false;
            }
            var dataStream = constantBuffer.DataStream.GetDataStream();
            var buffer = constantBuffer.Buffer.GetBuffer();

            Marshal.StructureToPtr(data, dataStream.DataPointer, false);

            var dataBox = new DataBox(dataStream.DataPointer, 0, 0);
            deviceContext.UpdateSubresource(dataBox, buffer, 0);
            frameCounters.SubresourceUpdates++;

            return true;
        }

        /// <inheritdoc/>
        public void UpdateTexture1D<T>(EngineShaderResourceView texture, T[] data) where T : struct
        {
            var t = texture?.GetResource();
            if (t == null)
            {
                return;
            }

            if (data.Length <= 0)
            {
                return;
            }

            UpdateResource(t.Resource.QueryInterface<Texture1D>(), data);
        }
        /// <inheritdoc/>
        public void UpdateTexture2D<T>(EngineShaderResourceView texture, T[] data) where T : struct
        {
            var t = texture?.GetResource();
            if (t == null)
            {
                return;
            }

            if (data.Length <= 0)
            {
                return;
            }

            UpdateResource(t.Resource.QueryInterface<Texture2D1>(), data);
        }
        /// <inheritdoc/>
        public void UpdateTexture3D<T>(EngineShaderResourceView texture, T[] data) where T : struct
        {
            var t = texture?.GetResource();
            if (t == null)
            {
                return;
            }

            if (data.Length <= 0)
            {
                return;
            }

            UpdateResource(t.Resource.QueryInterface<Texture3D1>(), data);
        }
        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="resource">Resource to update</param>
        /// <param name="data">Data</param>
        private void UpdateResource<T>(Resource resource, T[] data) where T : struct
        {
            using (resource)
            {
                deviceContext.MapSubresource(resource, 0, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
                frameCounters.SubresourceMaps++;

                using (stream)
                {
                    stream.Position = 0;
                    stream.WriteRange(data.ToArray());
                }

                deviceContext.UnmapSubresource(resource, 0);
                frameCounters.SubresourceUnmaps++;
            }

            frameCounters.TextureWrites++;
        }

        /// <inheritdoc/>
        public bool WriteDiscardBuffer<T>(EngineBuffer buffer, T data)
            where T : struct
        {
            return WriteDiscardBuffer(buffer, 0, new[] { data });
        }
        /// <inheritdoc/>
        public bool WriteDiscardBuffer<T>(EngineBuffer buffer, T[] data)
            where T : struct
        {
            return WriteDiscardBuffer(buffer, 0, data);
        }
        /// <inheritdoc/>
        public bool WriteDiscardBuffer<T>(EngineBuffer buffer, long offset, T[] data)
            where T : struct
        {
            var b = buffer?.GetBuffer();
            if (b == null)
            {
                return false;
            }

            if (data.Length <= 0)
            {
                return true;
            }

            deviceContext.MapSubresource(b, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
            frameCounters.SubresourceMaps++;

            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;
                stream.WriteRange(data.ToArray());
            }

            deviceContext.UnmapSubresource(b, 0);
            frameCounters.SubresourceUnmaps++;

            frameCounters.BufferWrites++;

            return true;
        }

        /// <inheritdoc/>
        public bool WriteNoOverwriteBuffer<T>(EngineBuffer buffer, T[] data)
            where T : struct
        {
            return WriteNoOverwriteBuffer(buffer, 0, data);
        }
        /// <inheritdoc/>
        public bool WriteNoOverwriteBuffer<T>(EngineBuffer buffer, long offset, T[] data)
            where T : struct
        {
            var b = buffer?.GetBuffer();
            if (b == null)
            {
                return false;
            }

            if (data.Length <= 0)
            {
                return true;
            }

            deviceContext.MapSubresource(b, MapMode.WriteNoOverwrite, MapFlags.None, out DataStream stream);
            frameCounters.SubresourceMaps++;

            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;
                stream.WriteRange(data.ToArray());
            }

            deviceContext.UnmapSubresource(b, 0);
            frameCounters.SubresourceUnmaps++;

            frameCounters.BufferWrites++;

            return true;
        }

        /// <inheritdoc/>
        public T[] ReadBuffer<T>(EngineBuffer buffer, int length)
            where T : struct
        {
            return ReadBuffer<T>(buffer, 0, length);
        }
        /// <inheritdoc/>
        public T[] ReadBuffer<T>(EngineBuffer buffer, long offset, int length)
            where T : struct
        {
            var b = buffer?.GetBuffer();
            if (b == null)
            {
                return [];
            }

            T[] data = new T[length];

            deviceContext.MapSubresource(b, MapMode.Read, MapFlags.None, out DataStream stream);
            frameCounters.SubresourceMaps++;

            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;

                for (int i = 0; i < length; i++)
                {
                    data[i] = stream.Read<T>();
                }
            }

            deviceContext.UnmapSubresource(b, 0);
            frameCounters.SubresourceUnmaps++;

            frameCounters.BufferReads++;

            return data;
        }

        /// <inheritdoc/>
        public void Draw(int vertexCount, int startVertexLocation)
        {
            if (vertexCount <= 0)
            {
                return;
            }

            deviceContext.Draw(vertexCount, startVertexLocation);
            UpdateDrawPrimitives(vertexCount, 1);
        }
        /// <inheritdoc/>
        public void Draw(BufferDescriptor vertexBuffer)
        {
            Draw(vertexBuffer.Count, vertexBuffer.BufferOffset);
        }
        /// <inheritdoc/>
        public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            if (indexCount <= 0)
            {
                return;
            }

            deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
            UpdateDrawPrimitives(indexCount, 1);
        }
        /// <inheritdoc/>
        public void DrawIndexed(BufferDescriptor indexBuffer, BufferDescriptor vertexBuffer)
        {
            DrawIndexed(
                indexBuffer.Count, indexBuffer.BufferOffset,
                vertexBuffer.BufferOffset);
        }
        /// <inheritdoc/>
        public void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
        {
            if (vertexCountPerInstance <= 0)
            {
                return;
            }

            if (instanceCount <= 0)
            {
                return;
            }

            deviceContext.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);
            UpdateDrawPrimitives(vertexCountPerInstance, instanceCount);
        }
        /// <inheritdoc/>
        public void DrawInstanced(int instanceCount, int startInstanceLocation, BufferDescriptor vertexBuffer)
        {
            DrawInstanced(vertexBuffer.Count, instanceCount, vertexBuffer.BufferOffset, startInstanceLocation);
        }
        /// <inheritdoc/>
        public void DrawIndexedInstanced(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
            if (indexCountPerInstance <= 0)
            {
                return;
            }

            if (instanceCount <= 0)
            {
                return;
            }

            deviceContext.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);
            UpdateDrawPrimitives(indexCountPerInstance, instanceCount);
        }
        /// <inheritdoc/>
        public void DrawIndexedInstanced(int instanceCount, int startInstanceLocation, BufferDescriptor indexBuffer, BufferDescriptor vertexBuffer)
        {
            DrawIndexedInstanced(
                indexBuffer.Count, instanceCount, indexBuffer.BufferOffset,
                vertexBuffer.BufferOffset, startInstanceLocation);
        }
        /// <inheritdoc/>
        public void DrawAuto()
        {
            deviceContext.DrawAuto();
            UpdateDrawPrimitives(1, 1);
        }
        /// <summary>
        /// Updates the draw related counter variables
        /// </summary>
        /// <param name="count">Submitted vertices / indexes</param>
        /// <param name="instanceCount">Instace count</param>
        private void UpdateDrawPrimitives(int count, int instanceCount)
        {
            frameCounters.DrawCallsPerFrame++;

            //Figure number of real primitives using the primitive topology
            int div = (int)currentIAPrimitiveTopology switch
            {
                >= 2 and <= 3 or >= 10 and <= 11 => 2,
                >= 4 and <= 5 or >= 12 and <= 13 => 3,
                _ => 1,
            };

            int primitives = count / div;
            frameCounters.PrimitivesPerFrame += primitives * instanceCount;
            frameCounters.InstancesPerFrame += instanceCount;
        }

        /// <inheritdoc/>
        public IEngineCommandList FinishCommandList(string name, bool restoreState = false)
        {
            deviceContext.ClearState();
            frameCounters.ContextClears++;

            var cmdList = deviceContext.FinishCommandList(restoreState);
            frameCounters.FinishCommandLists++;

            return new EngineCommandList($"{Name} {name ?? "commands"}", cmdList);
        }
        /// <inheritdoc/>
        public void ExecuteCommandList(IEngineCommandList commandList, bool restoreState = false)
        {
            if (commandList == null)
            {
                return;
            }

            deviceContext.ExecuteCommandList(commandList.GetCommandList(), restoreState);
            frameCounters.ExecuteCommandLists++;

            commandList.Dispose();
        }
        /// <inheritdoc/>
        public void ExecuteCommandLists(IEngineCommandList[] commandLists, bool restoreState = false)
        {
            if (commandLists.Length <= 0)
            {
                return;
            }

            foreach (var commandList in commandLists)
            {
                ExecuteCommandList(commandList, restoreState);
            }
        }

        /// <summary>
        /// Implicit conversion between the Engine Device Context and the DX11 device context
        /// </summary>
        public static implicit operator DeviceContext3(EngineDeviceContext value)
        {
            return value.deviceContext;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string deviceType = IsImmediateContext ? "Immediate" : "Deferred";

            return $"{deviceType} => {Name}";
        }
    }
}
