using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    using SharpDX;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.Mathematics.Interop;
    using System.Runtime.InteropServices;
    using Format = SharpDX.DXGI.Format;

    /// <summary>
    /// Engine device context
    /// </summary>
    public class EngineDeviceContext : IDisposable
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
                if (!Update(slot, buffer))
                {
                    return;
                }

                var buffers = Resources.Select(r => r?.GetBuffer()).ToArray();
                shaderStage.SetConstantBuffers(StartSlot, buffers.Length, buffers);
            }

            public void SetConstantBuffers(CommonShaderStage shaderStage, int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
            {
                if (!Update(startSlot, bufferList))
                {
                    return;
                }

                var buffers = Resources.Select(r => r?.GetBuffer()).ToArray();
                shaderStage.SetConstantBuffers(StartSlot, buffers.Length, buffers);
            }

            public void Clear(CommonShaderStage shaderStage)
            {
                if (Resources?.Any() != true)
                {
                    return;
                }

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
                if (!Update(slot, resourceView))
                {
                    return;
                }

                var resources = Resources.Select(r => r?.GetResource()).ToArray();
                shaderStage.SetShaderResources(StartSlot, resources.Length, resources);
            }

            public void SetShaderResources(CommonShaderStage shaderStage, int startSlot, IEnumerable<EngineShaderResourceView> resourceList)
            {
                if (!Update(startSlot, resourceList))
                {
                    return;
                }

                var resources = Resources.Select(r => r?.GetResource()).ToArray();
                shaderStage.SetShaderResources(StartSlot, resources.Length, resources);
            }

            public void Clear(CommonShaderStage shaderStage)
            {
                if (Resources?.Any() != true)
                {
                    return;
                }

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
                if (!Update(slot, samplerState))
                {
                    return;
                }

                var resources = Resources.Select(r => r?.GetSamplerState()).ToArray();
                shaderStage.SetSamplers(StartSlot, resources.Length, resources);
            }

            public void SetSamplers(CommonShaderStage shaderStage, int startSlot, IEnumerable<EngineSamplerState> samplerList)
            {
                if (!Update(startSlot, samplerList))
                {
                    return;
                }

                var samplers = Resources.Select(r => r?.GetSamplerState()).ToArray();
                shaderStage.SetSamplers(StartSlot, samplers.Length, samplers);
            }

            public void Clear(CommonShaderStage shaderStage)
            {
                if (Resources?.Any() != true)
                {
                    return;
                }

                shaderStage.SetSamplers(StartSlot, nullSamplers.Length - StartSlot, nullSamplers);

                Clear();
            }
        }

        #endregion

        /// <summary>
        /// Internal device context
        /// </summary>
        private DeviceContext3 deviceContext = null;
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
        private IEnumerable<StreamOutputBufferBinding> currentStreamOutputBindings;

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
        /// Name
        /// </summary>
        public string Name { get; private set; }
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
                deviceContext = null;
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

        /// <summary>
        /// Clears the device context state
        /// </summary>
        public void ClearState()
        {
            deviceContext.ClearState();
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
            SetRenderTargets(renderTargets, clearRT, clearRTColor, false);
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
        /// Sets depth stencil state
        /// </summary>
        /// <param name="state">Depth stencil state</param>
        /// <param name="stencilRef">Stencil reference</param>
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

        /// <summary>
        /// Sets blend state
        /// </summary>
        /// <param name="state">Blend state</param>
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

        /// <summary>
        /// Sets rasterizer state
        /// </summary>
        /// <param name="state">Rasterizer state</param>
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

        /// <summary>
        /// Clear shader resources
        /// </summary>
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

        /// <summary>
        /// Sets the vertex shader in the current device context
        /// </summary>
        /// <param name="vertexShader">Vertex shader</param>
        public void SetVertexShader(EngineVertexShader vertexShader)
        {
            if (currentVertexShader == vertexShader)
            {
                return;
            }

            deviceContext.VertexShader.Set(vertexShader?.GetShader());

            currentVertexShader = vertexShader;
        }
        /// <summary>
        /// Removes the vertex shader from the current device context
        /// </summary>
        public void ClearVertexShader()
        {
            if (currentVertexShader == null)
            {
                return;
            }

            deviceContext.VertexShader.Set(null);

            currentVertexShader = null;
        }
        /// <summary>
        /// Sets the constant buffer to the current vertex shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        public void SetVertexShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentVertexShaderConstantBufferState.SetConstantBuffer(deviceContext.VertexShader, slot, buffer);
        }
        /// <summary>
        /// Sets the constant buffer list to the current vertex shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        public void SetVertexShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentVertexShaderConstantBufferState.SetConstantBuffers(deviceContext.VertexShader, startSlot, bufferList);
        }
        /// <summary>
        /// Sets the specified resource in the current vertex shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetVertexShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentVertexShaderResourceViewState.SetShaderResource(deviceContext.VertexShader, slot, resourceView);
        }
        /// <summary>
        /// Sets the specified resource in the current vertex shader shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetVertexShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentVertexShaderResourceViewState.SetShaderResources(deviceContext.VertexShader, startSlot, resourceViews);
        }
        /// <summary>
        /// Sets the specified sampler state in the current vertex shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        public void SetVertexShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentVertexShaderSamplerState.SetSampler(deviceContext.VertexShader, slot, samplerState);
        }
        /// <summary>
        /// Sets the specified sampler state in the current vertex shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Samplers</param>
        public void SetVertexShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            currentVertexShaderSamplerState.SetSamplers(deviceContext.VertexShader, startSlot, samplerStates);
        }

        /// <summary>
        /// Sets the hull shader in the current device context
        /// </summary>
        /// <param name="hullShader">Hull shader</param>
        public void SetHullShader(EngineHullShader hullShader)
        {
            if (currentHullShader == hullShader)
            {
                return;
            }

            deviceContext.HullShader.Set(hullShader?.GetShader());

            currentHullShader = hullShader;
        }
        /// <summary>
        /// Removes the hull shader from the current device context
        /// </summary>
        public void ClearHullShader()
        {
            if (currentHullShader == null)
            {
                return;
            }

            deviceContext.HullShader.Set(null);

            currentHullShader = null;
        }
        /// <summary>
        /// Sets the constant buffer to the current hull shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        public void SetHullShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentHullShaderConstantBufferState.SetConstantBuffer(deviceContext.HullShader, slot, buffer);
        }
        /// <summary>
        /// Sets the constant buffer list to the current hull shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        public void SetHullShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentHullShaderConstantBufferState.SetConstantBuffers(deviceContext.HullShader, startSlot, bufferList);
        }
        /// <summary>
        /// Sets the specified resource in the current hull shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetHullShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentHullShaderResourceViewState.SetShaderResource(deviceContext.HullShader, slot, resourceView);
        }
        /// <summary>
        /// Sets the specified resource in the current hull shader shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetHullShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentHullShaderResourceViewState.SetShaderResources(deviceContext.HullShader, startSlot, resourceViews);
        }
        /// <summary>
        /// Sets the specified sampler state in the current hull shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        public void SetHullShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentHullShaderSamplerState.SetSampler(deviceContext.HullShader, slot, samplerState);
        }
        /// <summary>
        /// Sets the specified sampler state in the current hull shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Samplers</param>
        public void SetHullShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            currentHullShaderSamplerState.SetSamplers(deviceContext.HullShader, startSlot, samplerStates);
        }

        /// <summary>
        /// Sets the domain shader in the current device context
        /// </summary>
        /// <param name="domainShader">Domain shader</param>
        public void SetDomainShader(EngineDomainShader domainShader)
        {
            if (currentDomainShader == domainShader)
            {
                return;
            }

            deviceContext.DomainShader.Set(domainShader?.GetShader());

            currentDomainShader = domainShader;
        }
        /// <summary>
        /// Removes the domain shader from the current device context
        /// </summary>
        public void ClearDomainShader()
        {
            if (currentDomainShader == null)
            {
                return;
            }

            deviceContext.DomainShader.Set(null);

            currentDomainShader = null;
        }
        /// <summary>
        /// Sets the constant buffer to the current domain shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        public void SetDomainShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentDomainShaderConstantBufferState.SetConstantBuffer(deviceContext.DomainShader, slot, buffer);
        }
        /// <summary>
        /// Sets the constant buffer list to the current domain shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        public void SetDomainShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentDomainShaderConstantBufferState.SetConstantBuffers(deviceContext.DomainShader, startSlot, bufferList);
        }
        /// <summary>
        /// Sets the specified resource in the current domain shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetDomainShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentDomainShaderResourceViewState.SetShaderResource(deviceContext.DomainShader, slot, resourceView);
        }
        /// <summary>
        /// Sets the specified resource in the current domain shader shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetDomainShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentDomainShaderResourceViewState.SetShaderResources(deviceContext.DomainShader, startSlot, resourceViews);
        }
        /// <summary>
        /// Sets the specified sampler state in the current domain shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        public void SetDomainShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentDomainShaderSamplerState.SetSampler(deviceContext.DomainShader, slot, samplerState);
        }
        /// <summary>
        /// Sets the specified sampler state in the current domain shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Samplers</param>
        public void SetDomainShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            currentDomainShaderSamplerState.SetSamplers(deviceContext.DomainShader, startSlot, samplerStates);
        }

        /// <summary>
        /// Sets the geometry shader in the current device context
        /// </summary>
        /// <param name="geometryShader">Geometry shader</param>
        public void SetGeometryShader(EngineGeometryShader geometryShader)
        {
            if (currentGeomeryShader == geometryShader)
            {
                return;
            }

            deviceContext.GeometryShader.Set(geometryShader?.GetShader());

            currentGeomeryShader = geometryShader;
        }
        /// <summary>
        /// Removes the geometry shader from the current device context
        /// </summary>
        public void ClearGeometryShader()
        {
            if (currentGeomeryShader == null)
            {
                return;
            }

            deviceContext.GeometryShader.Set(null);

            currentGeomeryShader = null;
        }
        /// <summary>
        /// Sets the constant buffer to the current geometry shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        public void SetGeometryShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentGeometryShaderConstantBufferState.SetConstantBuffer(deviceContext.GeometryShader, slot, buffer);
        }
        /// <summary>
        /// Sets the constant buffer list to the current geometry shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        public void SetGeometryShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentGeometryShaderConstantBufferState.SetConstantBuffers(deviceContext.GeometryShader, startSlot, bufferList);
        }
        /// <summary>
        /// Sets the specified resource in the current geometry shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetGeometryShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentGeometryShaderResourceViewState.SetShaderResource(deviceContext.GeometryShader, slot, resourceView);
        }
        /// <summary>
        /// Sets the specified resource in the current geometry shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetGeometryShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentGeometryShaderResourceViewState.SetShaderResources(deviceContext.GeometryShader, startSlot, resourceViews);
        }
        /// <summary>
        /// Sets the specified sampler state in the current geometry shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        public void SetGeometryShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentGeometryShaderSamplerState.SetSampler(deviceContext.GeometryShader, slot, samplerState);
        }
        /// <summary>
        /// Sets the specified sampler state in the current geometry shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Samplers</param>
        public void SetGeometryShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            currentGeometryShaderSamplerState.SetSamplers(deviceContext.GeometryShader, startSlot, samplerStates);
        }

        /// <summary>
        /// Sets targets for stream output
        /// </summary>
        /// <param name="streamOutBinding">Stream output binding</param>
        public void SetGeometryShaderStreamOutputTargets(IEnumerable<StreamOutputBufferBinding> streamOutBinding)
        {
            if (Helper.CompareEnumerables(currentStreamOutputBindings, streamOutBinding))
            {
                return;
            }

            deviceContext.StreamOutput.SetTargets(streamOutBinding?.ToArray());
            Counters.SOTargetsSet++;

            currentStreamOutputBindings = streamOutBinding;
        }

        /// <summary>
        /// Sets the pixel shader in the current device context
        /// </summary>
        /// <param name="pixelShader">Pixel shader</param>
        public void SetPixelShader(EnginePixelShader pixelShader)
        {
            if (currentPixelShader == pixelShader)
            {
                return;
            }

            deviceContext.PixelShader.Set(pixelShader?.GetShader());

            currentPixelShader = pixelShader;
        }
        /// <summary>
        /// Removes the pixel shader from the current device context
        /// </summary>
        public void ClearPixelShader()
        {
            if (currentPixelShader == null)
            {
                return;
            }

            deviceContext.PixelShader.Set(null);

            currentPixelShader = null;
        }
        /// <summary>
        /// Sets the constant buffer to the current pixel shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        public void SetPixelShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentPixelShaderConstantBufferState.SetConstantBuffer(deviceContext.PixelShader, slot, buffer);
        }
        /// <summary>
        /// Sets the constant buffer list to the current pixel shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        public void SetPixelShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentPixelShaderConstantBufferState.SetConstantBuffers(deviceContext.PixelShader, startSlot, bufferList);
        }
        /// <summary>
        /// Sets the specified resource in the current pixel shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetPixelShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentPixelShaderResourceViewState.SetShaderResource(deviceContext.PixelShader, slot, resourceView);
        }
        /// <summary>
        /// Sets the specified resource in the current pixel shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetPixelShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentPixelShaderResourceViewState.SetShaderResources(deviceContext.PixelShader, startSlot, resourceViews);
        }
        /// <summary>
        /// Sets the specified sampler state in the current pixel shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        public void SetPixelShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentPixelShaderSamplerState.SetSampler(deviceContext.PixelShader, slot, samplerState);
        }
        /// <summary>
        /// Sets the specified sampler state in the current pixel shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Samplers</param>
        public void SetPixelShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            currentPixelShaderSamplerState.SetSamplers(deviceContext.PixelShader, startSlot, samplerStates);
        }

        /// <summary>
        /// Sets the compute shader in the current device context
        /// </summary>
        /// <param name="computeShader">Compute shader</param>
        public void SetComputeShader(EngineComputeShader computeShader)
        {
            if (currentComputeShader == computeShader)
            {
                return;
            }

            deviceContext.ComputeShader.Set(computeShader?.GetShader());

            currentComputeShader = computeShader;
        }
        /// <summary>
        /// Removes the compute shader from the current device context
        /// </summary>
        public void ClearComputeShader()
        {
            if (currentComputeShader == null)
            {
                return;
            }

            deviceContext.ComputeShader.Set(null);

            currentComputeShader = null;
        }
        /// <summary>
        /// Sets the constant buffer to the current compute shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        public void SetComputeShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            currentComputeShaderConstantBufferState.SetConstantBuffer(deviceContext.ComputeShader, slot, buffer);
        }
        /// <summary>
        /// Sets the constant buffer list to the current compute shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        public void SetComputeShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            currentComputeShaderConstantBufferState.SetConstantBuffers(deviceContext.ComputeShader, startSlot, bufferList);
        }
        /// <summary>
        /// Sets the specified resource in the current compute shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetComputeShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            currentComputeShaderResourceViewState.SetShaderResource(deviceContext.ComputeShader, slot, resourceView);
        }
        /// <summary>
        /// Sets the specified resource in the current compute shader shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetComputeShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            currentComputeShaderResourceViewState.SetShaderResources(deviceContext.ComputeShader, startSlot, resourceViews);
        }
        /// <summary>
        /// Sets the specified sampler state in the current compute shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        public void SetComputeShaderSampler(int slot, EngineSamplerState samplerState)
        {
            currentComputeShaderSamplerState.SetSampler(deviceContext.ComputeShader, slot, samplerState);
        }
        /// <summary>
        /// Sets the specified sampler state in the current compute shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Samplers</param>
        public void SetComputeShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            currentComputeShaderSamplerState.SetSamplers(deviceContext.ComputeShader, startSlot, samplerStates);
        }

        /// <summary>
        /// Apply effect pass
        /// </summary>
        /// <param name="technique"></param>
        /// <param name="index"></param>
        /// <param name="flags"></param>
        public void EffectPassApply(EngineEffectTechnique technique, int index, int flags)
        {
            technique.GetPass(index).Apply(deviceContext, flags);
        }

        /// <summary>
        /// Bind an array of vertex buffers to the input-assembler stage.
        /// </summary>
        /// <param name="firstSlot">The first input slot for binding</param>
        /// <param name="vertexBufferBindings">A reference to an array of VertexBufferBinding</param>
        public void IASetVertexBuffers(int firstSlot, params VertexBufferBinding[] vertexBufferBindings)
        {
            if (currentVertexBufferFirstSlot != firstSlot || !Helper.CompareEnumerables(currentVertexBufferBindings, vertexBufferBindings))
            {
                deviceContext.InputAssembler.SetVertexBuffers(firstSlot, vertexBufferBindings);
                Counters.IAVertexBuffersSets++;

                currentVertexBufferFirstSlot = firstSlot;
                currentVertexBufferBindings = vertexBufferBindings;
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
            if (currentIndexBufferRef != indexBufferRef || currentIndexFormat != format || currentIndexOffset != offset)
            {
                deviceContext.InputAssembler.SetIndexBuffer(indexBufferRef, format, offset);
                Counters.IAIndexBufferSets++;

                currentIndexBufferRef = indexBufferRef;
                currentIndexFormat = format;
                currentIndexOffset = offset;
            }
        }

        /// <summary>
        /// Updates a constant buffer value in the device
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="dataStream">Data stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="value">Value</param>
        internal bool UpdateConstantBuffer<T>(DataStream dataStream, Buffer buffer, T value) where T : struct, IBufferData
        {
            Marshal.StructureToPtr(value, dataStream.DataPointer, false);

            var dataBox = new DataBox(dataStream.DataPointer, 0, 0);
            deviceContext.UpdateSubresource(dataBox, buffer, 0);

            return true;
        }

        /// <summary>
        /// Updates a texture
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="texture">Texture to update</param>
        /// <param name="data">Data to write</param>
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
        /// <summary>
        /// Updates a texture
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="texture">Texture to update</param>
        /// <param name="data">Data to write</param>
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

        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal bool WriteDiscardBuffer<T>(Buffer buffer, T data)
            where T : struct
        {
            return WriteDiscardBuffer(buffer, 0, new[] { data });
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal bool WriteDiscardBuffer<T>(Buffer buffer, IEnumerable<T> data)
            where T : struct
        {
            return WriteDiscardBuffer(buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        internal bool WriteDiscardBuffer<T>(Buffer buffer, long offset, IEnumerable<T> data)
            where T : struct
        {
            if (buffer == null)
            {
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;
                stream.WriteRange(data.ToArray());
            }
            deviceContext.UnmapSubresource(buffer, 0);

            Counters.BufferWrites++;

            return true;
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        internal bool WriteNoOverwriteBuffer<T>(Buffer buffer, IEnumerable<T> data)
            where T : struct
        {
            return WriteNoOverwriteBuffer(buffer, 0, data);
        }
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphic context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        internal bool WriteNoOverwriteBuffer<T>(Buffer buffer, long offset, IEnumerable<T> data)
            where T : struct
        {
            if (buffer == null)
            {
                return false;
            }

            if (data?.Any() != true)
            {
                return true;
            }

            deviceContext.MapSubresource(buffer, MapMode.WriteNoOverwrite, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;
                stream.WriteRange(data.ToArray());
            }
            deviceContext.UnmapSubresource(buffer, 0);

            Counters.BufferWrites++;

            return true;
        }

        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="deviceContext">Graphics context</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        internal IEnumerable<T> ReadBuffer<T>(Buffer buffer, int length)
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
        internal IEnumerable<T> ReadBuffer<T>(Buffer buffer, long offset, int length)
            where T : struct
        {
            Counters.BufferReads++;

            T[] data = new T[length];

            deviceContext.MapSubresource(buffer, MapMode.Read, MapFlags.None, out DataStream stream);
            using (stream)
            {
                stream.Position = Marshal.SizeOf(default(T)) * offset;

                for (int i = 0; i < length; i++)
                {
                    data[i] = stream.Read<T>();
                }
            }
            deviceContext.UnmapSubresource(buffer, 0);

            return data;
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
