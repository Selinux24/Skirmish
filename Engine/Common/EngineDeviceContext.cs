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
        class ShaderConstantBufferState : EngineShaderStageState<IEngineConstantBuffer>
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
            }

            public void SetConstantBuffers(CommonShaderStage shaderStage, int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
            {
                Update(startSlot, bufferList);

                var buffers = Resources.Select(r => r?.Buffer.GetBuffer()).ToArray();
                shaderStage.SetConstantBuffers(StartSlot, buffers.Length, buffers);
            }

            public void Clear(CommonShaderStage shaderStage)
            {
                shaderStage.SetConstantBuffers(StartSlot, nullBuffers.Length - StartSlot, nullBuffers);

                Clear();
            }
        }
        /// <summary>
        /// Shader resources state
        /// </summary>
        class ShaderResourceState : EngineShaderStageState<EngineShaderResourceView>
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
            }

            public void SetShaderResources(CommonShaderStage shaderStage, int startSlot, IEnumerable<EngineShaderResourceView> resourceList)
            {
                Update(startSlot, resourceList);

                var resources = Resources.Select(r => r?.GetResource()).ToArray();
                shaderStage.SetShaderResources(StartSlot, resources.Length, resources);
            }

            public void Clear(CommonShaderStage shaderStage)
            {
                shaderStage.SetShaderResources(StartSlot, nullSrv.Length - StartSlot, nullSrv);

                Clear();
            }
        }
        /// <summary>
        /// Shader samplers state
        /// </summary>
        class ShaderSamplerState : EngineShaderStageState<EngineSamplerState>
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
            }

            public void SetSamplers(CommonShaderStage shaderStage, int startSlot, IEnumerable<EngineSamplerState> samplerList)
            {
                Update(startSlot, samplerList);

                var samplers = Resources.Select(r => r?.GetSamplerState()).ToArray();
                shaderStage.SetSamplers(StartSlot, samplers.Length, samplers);
            }

            public void Clear(CommonShaderStage shaderStage)
            {
                shaderStage.SetSamplers(StartSlot, nullSamplers.Length - StartSlot, nullSamplers);

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
        private IEnumerable<RawViewportF> currentViewports;

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
        private readonly ShaderConstantBufferState currentVertexShaderConstantBufferState = new();
        /// <summary>
        /// Current vertex shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentVertexShaderResourceViewState = new();
        /// <summary>
        /// Current vertex shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentVertexShaderSamplerState = new();

        /// <summary>
        /// Current hull shader
        /// </summary>
        private EngineHullShader currentHullShader;
        /// <summary>
        /// Current hull shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentHullShaderConstantBufferState = new();
        /// <summary>
        /// Current hull shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentHullShaderResourceViewState = new();
        /// <summary>
        /// Current hull shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentHullShaderSamplerState = new();

        /// <summary>
        /// Current domain shader
        /// </summary>
        private EngineDomainShader currentDomainShader;
        /// <summary>
        /// Current domain shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentDomainShaderConstantBufferState = new();
        /// <summary>
        /// Current domain shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentDomainShaderResourceViewState = new();
        /// <summary>
        /// Current domain shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentDomainShaderSamplerState = new();

        /// <summary>
        /// Current geometry shader
        /// </summary>
        private EngineGeometryShader currentGeomeryShader;
        /// <summary>
        /// Current geometry shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentGeometryShaderConstantBufferState = new();
        /// <summary>
        /// Current geometry shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentGeometryShaderResourceViewState = new();
        /// <summary>
        /// Current geometry shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentGeometryShaderSamplerState = new();

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
        private readonly ShaderConstantBufferState currentPixelShaderConstantBufferState = new();
        /// <summary>
        /// Current pixel shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentPixelShaderResourceViewState = new();
        /// <summary>
        /// Current pixel shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentPixelShaderSamplerState = new();

        /// <summary>
        /// Current compute shader
        /// </summary>
        private EngineComputeShader currentComputeShader;
        /// <summary>
        /// Current compute shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentComputeShaderConstantBufferState = new();
        /// <summary>
        /// Current compute shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentComputeShaderResourceViewState = new();
        /// <summary>
        /// Current compute shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentComputeShaderSamplerState = new();

        /// <summary>
        /// Current vertex buffer first slot
        /// </summary>
        private int currentVertexBufferFirstSlot = -1;
        /// <summary>
        /// Current vertex buffer bindings
        /// </summary>
        private EngineVertexBufferBinding[] currentVertexBufferBindings = null;
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

        /// <inheritdoc/>
        public string Name { get; private set; }
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
                    Counters.IAPrimitiveTopologySets++;

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
                    Counters.IAInputLayoutSets++;

                    currentIAInputLayout = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="deviceContext">Device context</param>
        internal EngineDeviceContext(string name, DeviceContext3 deviceContext)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A device context name must be specified.");
            this.deviceContext = deviceContext ?? throw new ArgumentNullException(nameof(deviceContext), "A device context must be specified.");

            this.deviceContext.DebugName = name;
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

        /// <summary>
        /// Gets the device context
        /// </summary>
        /// <returns></returns>
        internal DeviceContext3 GetDeviceContext()
        {
            return deviceContext;
        }

        /// <inheritdoc/>
        public void ClearState()
        {
            deviceContext.ClearState();

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
            SetViewPorts(new[] { (RawViewportF)viewport });
        }
        /// <inheritdoc/>
        public void SetViewport(ViewportF viewport)
        {
            SetViewPorts(new[] { (RawViewportF)viewport });
        }
        /// <inheritdoc/>
        public void SetViewports(IEnumerable<Viewport> viewports)
        {
            SetViewPorts(viewports.Select(v => (RawViewportF)v).ToArray());
        }
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, EngineDepthStencilView depthMap)
        {
            deviceContext.OutputMerger.SetTargets(depthMap.GetDepthStencil(), renderTargets.GetRenderTarget());
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
        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil, bool freeOMResources)
        {
            if (freeOMResources)
            {
                ClearShaderResources();
            }

            var dsv = depthMap?.GetDepthStencil();
            var rtv = renderTargets?.GetRenderTargets() ?? Enumerable.Empty<RenderTargetView1>();
            var rtvCount = rtv.Count();

            deviceContext.OutputMerger.SetTargets(dsv, 0, Array.Empty<UnorderedAccessView>(), Array.Empty<int>(), rtv.ToArray());

            if (clearRT && rtvCount > 0)
            {
                for (int i = 0; i < rtvCount; i++)
                {
                    deviceContext.ClearRenderTargetView(rtv.ElementAt(i), clearRTColor);
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

                deviceContext.ClearDepthStencilView(
                    depthMap.GetDepthStencil(),
                    clearDSFlags,
                    1.0f, 0);
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

            Counters.DepthStencilStateChanges++;

            currentDepthStencilState = state;
            currentDepthStencilRef = stencilRef;
        }

        /// <inheritdoc/>
        public void SetBlendState(EngineBlendState state)
        {
            if (currentBlendState == state)
            {
                return;
            }

            deviceContext.OutputMerger.SetBlendState(state.GetBlendState(), state.BlendFactor, state.SampleMask);

            currentBlendState = state;

            Counters.BlendStateChanges++;
        }

        /// <inheritdoc/>
        public void SetRasterizerState(EngineRasterizerState state)
        {
            if (currentRasterizerState == state)
            {
                return;
            }

            deviceContext.Rasterizer.State = state.GetRasterizerState();

            currentRasterizerState = state;

            Counters.RasterizerStateChanges++;
        }

        /// <inheritdoc/>
        public void IASetVertexBuffers(int firstSlot, params EngineVertexBufferBinding[] vertexBufferBindings)
        {
            if (currentVertexBufferFirstSlot != firstSlot || !Helper.CompareEnumerables(currentVertexBufferBindings, vertexBufferBindings))
            {
                var vbBindings = vertexBufferBindings?.Select(v => v.GetVertexBufferBinding()).ToArray() ?? Array.Empty<VertexBufferBinding>();

                deviceContext.InputAssembler.SetVertexBuffers(firstSlot, vbBindings);
                Counters.IAVertexBuffersSets++;

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
                Counters.IAIndexBufferSets++;

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

            currentVertexShader = vertexShader;
        }
        /// <inheritdoc/>
        public void ClearVertexShader()
        {
            if (currentVertexShader == null)
            {
                return;
            }

            deviceContext.VertexShader.Set(null);

            currentVertexShader = null;
        }
        /// <inheritdoc/>
        public void SetVertexShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentVertexShaderConstantBufferState.SetConstantBuffer(deviceContext.VertexShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetVertexShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentVertexShaderConstantBufferState.SetConstantBuffers(deviceContext.VertexShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetVertexShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentVertexShaderResourceViewState.SetShaderResource(deviceContext.VertexShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetVertexShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentVertexShaderResourceViewState.SetShaderResources(deviceContext.VertexShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetVertexShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentVertexShaderSamplerState.SetSampler(deviceContext.VertexShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetVertexShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
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

            currentHullShader = hullShader;
        }
        /// <inheritdoc/>
        public void ClearHullShader()
        {
            if (currentHullShader == null)
            {
                return;
            }

            deviceContext.HullShader.Set(null);

            currentHullShader = null;
        }
        /// <inheritdoc/>
        public void SetHullShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentHullShaderConstantBufferState.SetConstantBuffer(deviceContext.HullShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetHullShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentHullShaderConstantBufferState.SetConstantBuffers(deviceContext.HullShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetHullShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentHullShaderResourceViewState.SetShaderResource(deviceContext.HullShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetHullShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentHullShaderResourceViewState.SetShaderResources(deviceContext.HullShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetHullShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentHullShaderSamplerState.SetSampler(deviceContext.HullShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetHullShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
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

            currentDomainShader = domainShader;
        }
        /// <inheritdoc/>
        public void ClearDomainShader()
        {
            if (currentDomainShader == null)
            {
                return;
            }

            deviceContext.DomainShader.Set(null);

            currentDomainShader = null;
        }
        /// <inheritdoc/>
        public void SetDomainShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentDomainShaderConstantBufferState.SetConstantBuffer(deviceContext.DomainShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetDomainShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentDomainShaderConstantBufferState.SetConstantBuffers(deviceContext.DomainShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetDomainShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentDomainShaderResourceViewState.SetShaderResource(deviceContext.DomainShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetDomainShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentDomainShaderResourceViewState.SetShaderResources(deviceContext.DomainShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetDomainShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentDomainShaderSamplerState.SetSampler(deviceContext.DomainShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetDomainShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
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

            currentGeomeryShader = geometryShader;
        }
        /// <inheritdoc/>
        public void ClearGeometryShader()
        {
            if (currentGeomeryShader == null)
            {
                return;
            }

            deviceContext.GeometryShader.Set(null);

            currentGeomeryShader = null;
        }
        /// <inheritdoc/>
        public void SetGeometryShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentGeometryShaderConstantBufferState.SetConstantBuffer(deviceContext.GeometryShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentGeometryShaderConstantBufferState.SetConstantBuffers(deviceContext.GeometryShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentGeometryShaderResourceViewState.SetShaderResource(deviceContext.GeometryShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentGeometryShaderResourceViewState.SetShaderResources(deviceContext.GeometryShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentGeometryShaderSamplerState.SetSampler(deviceContext.GeometryShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            currentGeometryShaderSamplerState.SetSamplers(deviceContext.GeometryShader, startSlot, samplerStates);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderStreamOutputTargets(IEnumerable<EngineStreamOutputBufferBinding> streamOutBinding)
        {
            if (Helper.CompareEnumerables(currentStreamOutputBindings, streamOutBinding))
            {
                return;
            }

            var soBindings = streamOutBinding?.Select(s => s.GetStreamOutputBufferBinding()).ToArray();

            deviceContext.StreamOutput.SetTargets(soBindings);
            Counters.SOTargetsSet++;

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

            currentPixelShader = pixelShader;
        }
        /// <inheritdoc/>
        public void ClearPixelShader()
        {
            if (currentPixelShader == null)
            {
                return;
            }

            deviceContext.PixelShader.Set(null);

            currentPixelShader = null;
        }
        /// <inheritdoc/>
        public void SetPixelShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentPixelShaderConstantBufferState.SetConstantBuffer(deviceContext.PixelShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetPixelShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentPixelShaderConstantBufferState.SetConstantBuffers(deviceContext.PixelShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetPixelShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentPixelShaderResourceViewState.SetShaderResource(deviceContext.PixelShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetPixelShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentPixelShaderResourceViewState.SetShaderResources(deviceContext.PixelShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetPixelShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentPixelShaderSamplerState.SetSampler(deviceContext.PixelShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetPixelShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
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

            currentComputeShader = computeShader;
        }
        /// <inheritdoc/>
        public void ClearComputeShader()
        {
            if (currentComputeShader == null)
            {
                return;
            }

            deviceContext.ComputeShader.Set(null);

            currentComputeShader = null;
        }
        /// <inheritdoc/>
        public void SetComputeShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentComputeShaderConstantBufferState.SetConstantBuffer(deviceContext.ComputeShader, slot, buffer);
        }
        /// <inheritdoc/>
        public void SetComputeShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentComputeShaderConstantBufferState.SetConstantBuffers(deviceContext.ComputeShader, startSlot, bufferList);
        }
        /// <inheritdoc/>
        public void SetComputeShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentComputeShaderResourceViewState.SetShaderResource(deviceContext.ComputeShader, slot, resourceView);
        }
        /// <inheritdoc/>
        public void SetComputeShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentComputeShaderResourceViewState.SetShaderResources(deviceContext.ComputeShader, startSlot, resourceViews);
        }
        /// <inheritdoc/>
        public void SetComputeShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentComputeShaderSamplerState.SetSampler(deviceContext.ComputeShader, slot, samplerState);
        }
        /// <inheritdoc/>
        public void SetComputeShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            currentComputeShaderSamplerState.SetSamplers(deviceContext.ComputeShader, startSlot, samplerStates);
        }

        /// <inheritdoc/>
        public void EffectPassApply(EngineEffectTechnique technique, int index, int flags)
        {
            technique.GetPass(index).Apply(deviceContext, flags);
        }

        /// <inheritdoc/>
        public bool UpdateConstantBuffer(IEngineConstantBuffer constantBuffer)
        {
            if (constantBuffer == null)
            {
                return false;
            }
            var value = constantBuffer.GetData();
            var dataStream = constantBuffer.DataStream.GetDataStream();
            var buffer = constantBuffer.Buffer.GetBuffer();

            Marshal.StructureToPtr(value, dataStream.DataPointer, false);

            var dataBox = new DataBox(dataStream.DataPointer, 0, 0);
            deviceContext.UpdateSubresource(dataBox, buffer, 0);

            return true;
        }

        /// <inheritdoc/>
        public void UpdateTexture1D<T>(EngineShaderResourceView texture, IEnumerable<T> data) where T : struct
        {
            if (data?.Any() == true)
            {
                using (var resource = texture.GetResource().Resource.QueryInterface<Texture1D>())
                {
                    deviceContext.MapSubresource(resource, 0, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
                    using (stream)
                    {
                        stream.Position = 0;
                        stream.WriteRange(data.ToArray());
                    }
                    deviceContext.UnmapSubresource(resource, 0);
                }

                Counters.BufferWrites++;
            }
        }
        /// <inheritdoc/>
        public void UpdateTexture2D<T>(EngineShaderResourceView texture, IEnumerable<T> data) where T : struct
        {
            if (data?.Any() == true)
            {
                using (var resource = texture.GetResource().Resource.QueryInterface<Texture2D1>())
                {
                    deviceContext.MapSubresource(resource, 0, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
                    using (stream)
                    {
                        stream.Position = 0;
                        stream.WriteRange(data.ToArray());
                    }
                    deviceContext.UnmapSubresource(resource, 0);
                }

                Counters.BufferWrites++;
            }
        }

        /// <inheritdoc/>
        public bool WriteDiscardBuffer<T>(EngineBuffer buffer, T data)
            where T : struct
        {
            return WriteDiscardBuffer(buffer, 0, new[] { data });
        }
        /// <inheritdoc/>
        public bool WriteDiscardBuffer<T>(EngineBuffer buffer, IEnumerable<T> data)
            where T : struct
        {
            return WriteDiscardBuffer(buffer, 0, data);
        }
        /// <inheritdoc/>
        public bool WriteDiscardBuffer<T>(EngineBuffer buffer, long offset, IEnumerable<T> data)
            where T : struct
        {
            var b = buffer?.GetBuffer();

            if (b == null)
            {
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            deviceContext.MapSubresource(b, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;
                stream.WriteRange(data.ToArray());
            }
            deviceContext.UnmapSubresource(b, 0);

            Counters.BufferWrites++;

            return true;
        }

        /// <inheritdoc/>
        public bool WriteNoOverwriteBuffer<T>(EngineBuffer buffer, IEnumerable<T> data)
            where T : struct
        {
            return WriteNoOverwriteBuffer(buffer, 0, data);
        }
        /// <inheritdoc/>
        public bool WriteNoOverwriteBuffer<T>(EngineBuffer buffer, long offset, IEnumerable<T> data)
            where T : struct
        {
            var b = buffer?.GetBuffer();

            if (b == null)
            {
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            //This should be MapMode.WriteNoOverwrite
            deviceContext.MapSubresource(b, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;
                stream.WriteRange(data.ToArray());
            }
            deviceContext.UnmapSubresource(b, 0);

            Counters.BufferWrites++;

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<T> ReadBuffer<T>(EngineBuffer buffer, int length)
            where T : struct
        {
            return ReadBuffer<T>(buffer, 0, length);
        }
        /// <inheritdoc/>
        public IEnumerable<T> ReadBuffer<T>(EngineBuffer buffer, long offset, int length)
            where T : struct
        {
            var b = buffer?.GetBuffer();
            if (b == null)
            {
                return Enumerable.Empty<T>();
            }

            Counters.BufferReads++;

            T[] data = new T[length];

            deviceContext.MapSubresource(b, MapMode.Read, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;

                for (int i = 0; i < length; i++)
                {
                    data[i] = stream.Read<T>();
                }
            }
            deviceContext.UnmapSubresource(b, 0);

            return data;
        }

        /// <inheritdoc/>
        public void Draw(int vertexCount, int startVertexLocation)
        {
            deviceContext.Draw(vertexCount, startVertexLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <inheritdoc/>
        public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <inheritdoc/>
        public void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation)
        {
            deviceContext.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <inheritdoc/>
        public void DrawIndexedInstanced(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
            deviceContext.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);

            Counters.DrawCallsPerFrame++;
        }
        /// <inheritdoc/>
        public void DrawAuto()
        {
            deviceContext.DrawAuto();

            Counters.DrawCallsPerFrame++;
        }

        /// <inheritdoc/>
        public IEngineCommandList FinishCommandList(string name, bool restoreState = false)
        {
            deviceContext.ClearState();

            var cmdList = deviceContext.FinishCommandList(restoreState);

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
            commandList.Dispose();
        }
        /// <inheritdoc/>
        public void ExecuteCommandLists(IEnumerable<IEngineCommandList> commandLists, bool restoreState = false)
        {
            if (!commandLists.Any())
            {
                return;
            }

            foreach (var commandList in commandLists)
            {
                ExecuteCommandList(commandList, restoreState);
            }
        }
    }
}
