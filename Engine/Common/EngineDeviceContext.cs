using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct3D;
using Vortice.Mathematics;

namespace Engine.Common
{
    using Vortice.Direct3D11;
    using Format = Vortice.DXGI.Format;

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
            public void SetConstantBuffer(uint slot, IEngineConstantBuffer buffer)
            {
                Update(slot, buffer);

                DeviceContext.frameCounters.ConstantBufferSets++;
            }

            public void SetConstantBuffers(uint startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
            {
                Update(startSlot, bufferList);

                DeviceContext.frameCounters.ConstantBufferSets++;
            }

            public void Clear()
            {
                DeviceContext.frameCounters.ConstantBufferClears++;

                ClearResources();
            }
        }
        /// <summary>
        /// Shader resources state
        /// </summary>
        class ShaderResourceState(EngineDeviceContext deviceContext) : EngineShaderStageState<EngineShaderResourceView>(deviceContext)
        {
            public void SetShaderResource(uint slot, EngineShaderResourceView resourceView)
            {
                Update(slot, resourceView);

                DeviceContext.frameCounters.ShaderResourceSets++;
            }

            public void SetShaderResources(uint startSlot, IEnumerable<EngineShaderResourceView> resourceList)
            {
                Update(startSlot, resourceList);

                DeviceContext.frameCounters.ShaderResourceSets++;
            }

            public void Clear()
            {
                DeviceContext.frameCounters.ShaderResourceClears++;

                ClearResources();
            }
        }
        /// <summary>
        /// Shader samplers state
        /// </summary>
        class ShaderSamplerState(EngineDeviceContext deviceContext) : EngineShaderStageState<EngineSamplerState>(deviceContext)
        {
            public void SetSampler(uint slot, EngineSamplerState samplerState)
            {
                Update(slot, samplerState);

                DeviceContext.frameCounters.SamplerSets++;
            }

            public void SetSamplers(uint startSlot, IEnumerable<EngineSamplerState> samplerList)
            {
                Update(startSlot, samplerList);

                DeviceContext.frameCounters.SamplerSets++;
            }

            public void Clear()
            {
                DeviceContext.frameCounters.SamplerClears++;

                ClearResources();
            }
        }

        #endregion

        /// <summary>
        /// Internal device context
        /// </summary>
        private readonly ID3D11DeviceContext3 deviceContext = null;
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
        private Viewport[] currentViewports;

        /// <summary>
        /// Current depth-stencil state
        /// </summary>
        private EngineDepthStencilState currentDepthStencilState = null;
        /// <summary>
        /// Current depth-stencil reference
        /// </summary>
        private uint currentDepthStencilRef = 0;
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
        private uint currentVertexBufferFirstSlot = uint.MaxValue;
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
        private uint currentIndexOffset = uint.MaxValue;

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
                    deviceContext.IASetPrimitiveTopology((PrimitiveTopology)value);
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
                    deviceContext.IASetInputLayout(value?.GetInputLayout());
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
        internal EngineDeviceContext(string name, bool immediate, int passIndex, ID3D11DeviceContext3 deviceContext)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A device context name must be specified.");
            this.deviceContext = deviceContext ?? throw new ArgumentNullException(nameof(deviceContext), "A device context must be specified.");

            deviceContext.DebugName = name;

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
        /// <inheritdoc/>
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

            currentVertexBufferFirstSlot = uint.MaxValue;
            currentVertexBufferBindings = null;
            currentIndexBufferRef = null;
            currentIndexFormat = Format.Unknown;
            currentIndexOffset = uint.MaxValue;
        }

        /// <inheritdoc/>
        public void SetViewport(Viewport viewport)
        {
            SetViewPorts([viewport]);
        }
        /// <inheritdoc/>
        public void SetViewports(Viewport[] viewports)
        {
            SetViewPorts(viewports);
        }
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        private void SetViewPorts(Viewport[] viewports)
        {
            if (Helper.CompareEnumerables(currentViewports, viewports))
            {
                return;
            }

            deviceContext.RSSetViewports(viewports);
            frameCounters.ViewportsSets++;

            currentViewports = viewports;
        }

        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, EngineDepthStencilView depthMap)
        {
            deviceContext.OMSetRenderTargets(renderTargets.GetRenderTarget(), depthMap.GetDepthStencil());
            frameCounters.RenderTargetSets++;
        }
        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Vector4 clearRTColor)
        {
            SetRenderTargets(renderTargets, clearRT, clearRTColor, false);
        }
        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Vector4 clearRTColor, bool freeOMResources)
        {
            if (freeOMResources)
            {
                ClearShaderResources();
            }

            var rtv = renderTargets?.GetRenderTargets() ?? [];
            uint rtvCount = (uint)rtv.Length;

            deviceContext.OMSetRenderTargets(rtvCount, rtv, null);
            frameCounters.RenderTargetSets++;

            if (clearRT && rtvCount > 0)
            {
                for (int i = 0; i < rtvCount; i++)
                {
                    deviceContext.ClearRenderTargetView(rtv[i], (Color4)clearRTColor);
                    frameCounters.RenderTargetClears++;
                }
            }
        }
        /// <inheritdoc/>
        public void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Vector4 clearRTColor, EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil, bool freeOMResources)
        {
            if (freeOMResources)
            {
                ClearShaderResources();
            }

            var dsv = depthMap?.GetDepthStencil();
            var rtv = renderTargets?.GetRenderTargets() ?? [];
            uint rtvCount = (uint)rtv.Length;

            deviceContext.OMSetRenderTargets(0, rtv, dsv);
            frameCounters.RenderTargetSets++;

            if (clearRT && rtvCount > 0)
            {
                for (int i = 0; i < rtvCount; i++)
                {
                    deviceContext.ClearRenderTargetView(rtv[i], (Color4)clearRTColor);
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
        public void SetDepthStencilState(EngineDepthStencilState state, uint stencilRef = 0)
        {
            if (currentDepthStencilState == state && currentDepthStencilRef == stencilRef)
            {
                return;
            }

            deviceContext.OMSetDepthStencilState(state.GetDepthStencilState(), stencilRef);
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

            deviceContext.RSSetState(state.GetRasterizerState());
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

            deviceContext.OMSetBlendState(state.GetBlendState(), (Color4)state.BlendFactor, state.SampleMask);
            frameCounters.OMBlendStateChanges++;

            currentBlendState = state;
        }

        /// <inheritdoc/>
        public void IASetVertexBuffers(uint firstSlot, EngineVertexBufferBinding[] vbBindings)
        {
            if (currentVertexBufferFirstSlot != firstSlot || !Helper.CompareEnumerables(currentVertexBufferBindings, vbBindings))
            {
                ID3D11Buffer[] buffers = [.. vbBindings.Select(v => v.Buffer?.GetBuffer())];
                uint[] strides = [.. vbBindings.Select(v => v.Stride)];
                uint[] offsets = [.. vbBindings.Select(v => v.Offset)];

                deviceContext.IASetVertexBuffers(firstSlot, buffers, strides, offsets);
                frameCounters.IAVertexBuffersSets++;

                currentVertexBufferFirstSlot = firstSlot;
                currentVertexBufferBindings = vbBindings;
            }
        }
        /// <inheritdoc/>
        public void IASetIndexBuffer(EngineBuffer indexBufferRef, Format format, uint offset)
        {
            if (currentIndexBufferRef != indexBufferRef || currentIndexFormat != format || currentIndexOffset != offset)
            {
                deviceContext.IASetIndexBuffer(indexBufferRef?.GetBuffer(), format, offset);
                frameCounters.IAIndexBufferSets++;

                currentIndexBufferRef = indexBufferRef;
                currentIndexFormat = format;
                currentIndexOffset = offset;
            }
        }

        /// <inheritdoc/>
        public void ClearShaderResources()
        {
            deviceContext.VSUnsetConstantBuffers(0, ID3D11DeviceContext.CommonShaderConstantBufferSlotCount);
            deviceContext.VSUnsetShaderResources(0, ID3D11DeviceContext.CommonShaderInputResourceSlotCount);
            deviceContext.VSUnsetSamplers(0, ID3D11DeviceContext.CommonShaderSamplerSlotCount);
            currentVertexShaderConstantBufferState.Clear();
            currentVertexShaderResourceViewState.Clear();
            currentVertexShaderSamplerState.Clear();

            deviceContext.HSUnsetConstantBuffers(0, ID3D11DeviceContext.CommonShaderConstantBufferSlotCount);
            deviceContext.HSUnsetShaderResources(0, ID3D11DeviceContext.CommonShaderInputResourceSlotCount);
            deviceContext.HSUnsetSamplers(0, ID3D11DeviceContext.CommonShaderSamplerSlotCount);
            currentHullShaderConstantBufferState.Clear();
            currentHullShaderResourceViewState.Clear();
            currentHullShaderSamplerState.Clear();

            deviceContext.DSUnsetConstantBuffers(0, ID3D11DeviceContext.CommonShaderConstantBufferSlotCount);
            deviceContext.DSUnsetShaderResources(0, ID3D11DeviceContext.CommonShaderInputResourceSlotCount);
            deviceContext.DSUnsetSamplers(0, ID3D11DeviceContext.CommonShaderSamplerSlotCount);
            currentDomainShaderConstantBufferState.Clear();
            currentDomainShaderResourceViewState.Clear();
            currentDomainShaderSamplerState.Clear();

            deviceContext.GSUnsetConstantBuffers(0, ID3D11DeviceContext.CommonShaderConstantBufferSlotCount);
            deviceContext.GSUnsetShaderResources(0, ID3D11DeviceContext.CommonShaderInputResourceSlotCount);
            deviceContext.GSUnsetSamplers(0, ID3D11DeviceContext.CommonShaderSamplerSlotCount);
            currentGeometryShaderConstantBufferState.Clear();
            currentGeometryShaderResourceViewState.Clear();
            currentGeometryShaderSamplerState.Clear();

            deviceContext.PSUnsetConstantBuffers(0, ID3D11DeviceContext.CommonShaderConstantBufferSlotCount);
            deviceContext.PSUnsetShaderResources(0, ID3D11DeviceContext.CommonShaderInputResourceSlotCount);
            deviceContext.PSUnsetSamplers(0, ID3D11DeviceContext.CommonShaderSamplerSlotCount);
            currentPixelShaderConstantBufferState.Clear();
            currentPixelShaderResourceViewState.Clear();
            currentPixelShaderSamplerState.Clear();

            deviceContext.CSUnsetConstantBuffers(0, ID3D11DeviceContext.CommonShaderConstantBufferSlotCount);
            deviceContext.CSUnsetShaderResources(0, ID3D11DeviceContext.CommonShaderInputResourceSlotCount);
            deviceContext.CSUnsetSamplers(0, ID3D11DeviceContext.CommonShaderSamplerSlotCount);
            currentComputeShaderConstantBufferState.Clear();
            currentComputeShaderResourceViewState.Clear();
            currentComputeShaderSamplerState.Clear();
        }

        /// <inheritdoc/>
        public void SetVertexShader(EngineVertexShader vertexShader)
        {
            if (currentVertexShader == vertexShader)
            {
                return;
            }

            deviceContext.VSSetShader(vertexShader?.GetShader());
            frameCounters.VertexShadersSets++;

            currentVertexShader = vertexShader;
        }
        /// <inheritdoc/>
        public void ClearVertexShader()
        {
            SetVertexShader(null);
        }
        /// <inheritdoc/>
        public void SetVertexShaderConstantBuffer(uint slot, IEngineConstantBuffer buffer)
        {
            currentVertexShaderConstantBufferState.SetConstantBuffer(slot, buffer);
            var buffers = currentVertexShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.VSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetVertexShaderConstantBuffers(uint startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentVertexShaderConstantBufferState.SetConstantBuffers(startSlot, bufferList);
            var buffers = currentVertexShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.VSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetVertexShaderResourceView(uint slot, EngineShaderResourceView resourceView)
        {
            currentVertexShaderResourceViewState.SetShaderResource(slot, resourceView);
            var resources = currentVertexShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.VSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetVertexShaderResourceViews(uint startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentVertexShaderResourceViewState.SetShaderResources(startSlot, resourceViews);
            var resources = currentVertexShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.VSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetVertexShaderSampler(uint slot, EngineSamplerState samplerState)
        {
            currentVertexShaderSamplerState.SetSampler(slot, samplerState);
            var resources = currentVertexShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.VSSetSamplers(0, resources);
        }
        /// <inheritdoc/>
        public void SetVertexShaderSamplers(uint startSlot, EngineSamplerState[] samplerStates)
        {
            currentVertexShaderSamplerState.SetSamplers(startSlot, samplerStates);
            var samplers = currentVertexShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.VSSetSamplers(0, samplers);
        }

        /// <inheritdoc/>
        public void SetHullShader(EngineHullShader hullShader)
        {
            if (currentHullShader == hullShader)
            {
                return;
            }

            deviceContext.HSSetShader(hullShader?.GetShader());
            frameCounters.HullShadersSets++;

            currentHullShader = hullShader;
        }
        /// <inheritdoc/>
        public void ClearHullShader()
        {
            SetHullShader(null);
        }
        /// <inheritdoc/>
        public void SetHullShaderConstantBuffer(uint slot, IEngineConstantBuffer buffer)
        {
            currentHullShaderConstantBufferState.SetConstantBuffer(slot, buffer);
            var buffers = currentHullShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.HSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetHullShaderConstantBuffers(uint startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentHullShaderConstantBufferState.SetConstantBuffers(startSlot, bufferList);
            var buffers = currentHullShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.HSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetHullShaderResourceView(uint slot, EngineShaderResourceView resourceView)
        {
            currentHullShaderResourceViewState.SetShaderResource(slot, resourceView);
            var resources = currentHullShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.HSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetHullShaderResourceViews(uint startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentHullShaderResourceViewState.SetShaderResources(startSlot, resourceViews);
            var resources = currentHullShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.HSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetHullShaderSampler(uint slot, EngineSamplerState samplerState)
        {
            currentHullShaderSamplerState.SetSampler(slot, samplerState);
            var resources = currentHullShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.HSSetSamplers(0, resources);
        }
        /// <inheritdoc/>
        public void SetHullShaderSamplers(uint startSlot, EngineSamplerState[] samplerStates)
        {
            currentHullShaderSamplerState.SetSamplers(startSlot, samplerStates);
            var samplers = currentHullShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.HSSetSamplers(0, samplers);
        }

        /// <inheritdoc/>
        public void SetDomainShader(EngineDomainShader domainShader)
        {
            if (currentDomainShader == domainShader)
            {
                return;
            }

            deviceContext.DSSetShader(domainShader?.GetShader());
            frameCounters.DomainShadersSets++;

            currentDomainShader = domainShader;
        }
        /// <inheritdoc/>
        public void ClearDomainShader()
        {
            SetDomainShader(null);
        }
        /// <inheritdoc/>
        public void SetDomainShaderConstantBuffer(uint slot, IEngineConstantBuffer buffer)
        {
            currentDomainShaderConstantBufferState.SetConstantBuffer(slot, buffer);
            var buffers = currentDomainShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.DSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetDomainShaderConstantBuffers(uint startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentDomainShaderConstantBufferState.SetConstantBuffers(startSlot, bufferList);
            var buffers = currentDomainShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.DSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetDomainShaderResourceView(uint slot, EngineShaderResourceView resourceView)
        {
            currentDomainShaderResourceViewState.SetShaderResource(slot, resourceView);
            var resources = currentDomainShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.DSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetDomainShaderResourceViews(uint startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentDomainShaderResourceViewState.SetShaderResources(startSlot, resourceViews);
            var resources = currentDomainShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.DSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetDomainShaderSampler(uint slot, EngineSamplerState samplerState)
        {
            currentDomainShaderSamplerState.SetSampler(slot, samplerState);
            var resources = currentDomainShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.DSSetSamplers(0, resources);
        }
        /// <inheritdoc/>
        public void SetDomainShaderSamplers(uint startSlot, EngineSamplerState[] samplerStates)
        {
            currentDomainShaderSamplerState.SetSamplers(startSlot, samplerStates);
            var resources = currentDomainShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.DSSetSamplers(0, resources);
        }

        /// <inheritdoc/>
        public void SetGeometryShader(EngineGeometryShader geometryShader)
        {
            if (currentGeomeryShader == geometryShader)
            {
                return;
            }

            deviceContext.GSSetShader(geometryShader?.GetShader());
            frameCounters.GeometryShadersSets++;

            currentGeomeryShader = geometryShader;
        }
        /// <inheritdoc/>
        public void ClearGeometryShader()
        {
            SetGeometryShader(null);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderConstantBuffer(uint slot, IEngineConstantBuffer buffer)
        {
            currentGeometryShaderConstantBufferState.SetConstantBuffer(slot, buffer);
            var buffers = currentGeometryShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.GSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderConstantBuffers(uint startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentGeometryShaderConstantBufferState.SetConstantBuffers(startSlot, bufferList);
            var buffers = currentGeometryShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.GSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderResourceView(uint slot, EngineShaderResourceView resourceView)
        {
            currentGeometryShaderResourceViewState.SetShaderResource(slot, resourceView);
            var resources = currentGeometryShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.GSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderResourceViews(uint startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentGeometryShaderResourceViewState.SetShaderResources(startSlot, resourceViews);
            var resources = currentGeometryShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.GSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderSampler(uint slot, EngineSamplerState samplerState)
        {
            currentGeometryShaderSamplerState.SetSampler(slot, samplerState);
            var resources = currentGeometryShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.GSSetSamplers(0, resources);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderSamplers(uint startSlot, EngineSamplerState[] samplerStates)
        {
            currentGeometryShaderSamplerState.SetSamplers(startSlot, samplerStates);
            var resources = currentGeometryShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.GSSetSamplers(0, resources);
        }
        /// <inheritdoc/>
        public void SetGeometryShaderStreamOutputTargets(EngineStreamOutputBufferBinding[] streamOutBinding)
        {
            if (Helper.CompareEnumerables(currentStreamOutputBindings, streamOutBinding))
            {
                return;
            }

            var soBindings = streamOutBinding?.Select(s => s.GetStreamOutputBufferBinding()) ?? [];
            var buffers = soBindings.Select(s => s.Buffer).ToArray();
            var offsets = soBindings.Select(s => s.Offset).ToArray();

            deviceContext.SOSetTargets(buffers, offsets);
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

            deviceContext.PSSetShader(pixelShader?.GetShader());
            frameCounters.PixelShadersSets++;

            currentPixelShader = pixelShader;
        }
        /// <inheritdoc/>
        public void ClearPixelShader()
        {
            SetPixelShader(null);
        }
        /// <inheritdoc/>
        public void SetPixelShaderConstantBuffer(uint slot, IEngineConstantBuffer buffer)
        {
            currentPixelShaderConstantBufferState.SetConstantBuffer(slot, buffer);
            var buffers = currentPixelShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.PSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetPixelShaderConstantBuffers(uint startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentPixelShaderConstantBufferState.SetConstantBuffers(startSlot, bufferList);
            var buffers = currentPixelShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.PSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetPixelShaderResourceView(uint slot, EngineShaderResourceView resourceView)
        {
            currentPixelShaderResourceViewState.SetShaderResource(slot, resourceView);
            var resources = currentPixelShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.PSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetPixelShaderResourceViews(uint startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentPixelShaderResourceViewState.SetShaderResources(startSlot, resourceViews);
            var resources = currentPixelShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.PSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetPixelShaderSampler(uint slot, EngineSamplerState samplerState)
        {
            currentPixelShaderSamplerState.SetSampler(slot, samplerState);
            var resources = currentPixelShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.PSSetSamplers(0, resources);
        }
        /// <inheritdoc/>
        public void SetPixelShaderSamplers(uint startSlot, EngineSamplerState[] samplerStates)
        {
            currentPixelShaderSamplerState.SetSamplers(startSlot, samplerStates);
            var resources = currentPixelShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.PSSetSamplers(0, resources);
        }

        /// <inheritdoc/>
        public void SetComputeShader(EngineComputeShader computeShader)
        {
            if (currentComputeShader == computeShader)
            {
                return;
            }

            deviceContext.CSSetShader(computeShader?.GetShader());
            frameCounters.ComputeShadersSets++;

            currentComputeShader = computeShader;
        }
        /// <inheritdoc/>
        public void ClearComputeShader()
        {
            SetComputeShader(null);
        }
        /// <inheritdoc/>
        public void SetComputeShaderConstantBuffer(uint slot, IEngineConstantBuffer buffer)
        {
            currentComputeShaderConstantBufferState.SetConstantBuffer(slot, buffer);
            var buffers = currentComputeShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.CSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetComputeShaderConstantBuffers(uint startSlot, IEngineConstantBuffer[] bufferList)
        {
            currentComputeShaderConstantBufferState.SetConstantBuffers(startSlot, bufferList);
            var buffers = currentComputeShaderConstantBufferState.GetResources().Select(r => r?.Buffer.GetBuffer()).ToArray();
            deviceContext.CSSetConstantBuffers(0, buffers);
        }
        /// <inheritdoc/>
        public void SetComputeShaderResourceView(uint slot, EngineShaderResourceView resourceView)
        {
            currentComputeShaderResourceViewState.SetShaderResource(slot, resourceView);
            var resources = currentComputeShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.CSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetComputeShaderResourceViews(uint startSlot, EngineShaderResourceView[] resourceViews)
        {
            currentComputeShaderResourceViewState.SetShaderResources(startSlot, resourceViews);
            var resources = currentComputeShaderResourceViewState.GetResources().Select(r => r?.GetResource()).ToArray();
            deviceContext.CSSetShaderResources(0, resources);
        }
        /// <inheritdoc/>
        public void SetComputeShaderSampler(uint slot, EngineSamplerState samplerState)
        {
            currentComputeShaderSamplerState.SetSampler(slot, samplerState);
            var resources = currentComputeShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.CSSetSamplers(0, resources);
        }
        /// <inheritdoc/>
        public void SetComputeShaderSamplers(uint startSlot, EngineSamplerState[] samplerStates)
        {
            currentComputeShaderSamplerState.SetSamplers(startSlot, samplerStates);
            var resources = currentComputeShaderSamplerState.GetResources().Select(r => r?.GetSamplerState()).ToArray();
            deviceContext.CSSetSamplers(0, resources);
        }

        /// <inheritdoc/>
        public bool UpdateConstantBuffer<T>(EngineConstantBuffer<T> constantBuffer, T data) where T : unmanaged, IBufferData
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

            Marshal.StructureToPtr(data, dataStream.BasePointer, false);

            var dataBox = new DataBox(dataStream.BasePointer, 0, 0);
            deviceContext.UpdateSubresource(dataBox, buffer, 0);
            frameCounters.SubresourceUpdates++;

            return true;
        }

        /// <inheritdoc/>
        public void UpdateTexture1D<T>(EngineShaderResourceView texture, T[] data) where T : unmanaged
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

            UpdateResource(t.Resource.QueryInterface<ID3D11Texture1D>(), data);
        }
        /// <inheritdoc/>
        public void UpdateTexture2D<T>(EngineShaderResourceView texture, T[] data) where T : unmanaged
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

            UpdateResource(t.Resource.QueryInterface<ID3D11Texture2D1>(), data);
        }
        /// <inheritdoc/>
        public void UpdateTexture3D<T>(EngineShaderResourceView texture, T[] data) where T : unmanaged
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

            UpdateResource(t.Resource.QueryInterface<ID3D11Texture3D1>(), data);
        }
        /// <summary>
        /// Updates a resource
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="resource">Resource to update</param>
        /// <param name="data">Data</param>
        private void UpdateResource<T>(ID3D11Resource resource, T[] data) where T : unmanaged
        {
            using (resource)
            {
                var msr = deviceContext.Map(resource, 0, MapMode.WriteDiscard, MapFlags.None);
                frameCounters.SubresourceMaps++;

                UnsafeUtilities.Write(msr.DataPointer, data);

                deviceContext.Unmap(resource, 0);
                frameCounters.SubresourceUnmaps++;
            }

            frameCounters.TextureWrites++;
        }

        /// <inheritdoc/>
        public bool WriteDiscardBuffer<T>(EngineBuffer buffer, T data) where T : unmanaged
        {
            return WriteDiscardBuffer(buffer, 0, [data]);
        }
        /// <inheritdoc/>
        public bool WriteDiscardBuffer<T>(EngineBuffer buffer, T[] data) where T : unmanaged
        {
            return WriteDiscardBuffer(buffer, 0, data);
        }
        /// <inheritdoc/>
        public bool WriteDiscardBuffer<T>(EngineBuffer buffer, long offset, T[] data) where T : unmanaged
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

            var msr = deviceContext.Map(b, MapMode.WriteDiscard, MapFlags.None);
            frameCounters.SubresourceMaps++;

            nint position = msr.DataPointer + Marshal.SizeOf(default(T)) * (nint)offset;
            UnsafeUtilities.Write(position, data);

            deviceContext.Unmap(b, 0);
            frameCounters.SubresourceUnmaps++;

            frameCounters.BufferWrites++;

            return true;
        }

        /// <inheritdoc/>
        public bool WriteNoOverwriteBuffer<T>(EngineBuffer buffer, T[] data) where T : unmanaged
        {
            return WriteNoOverwriteBuffer(buffer, 0, data);
        }
        /// <inheritdoc/>
        public bool WriteNoOverwriteBuffer<T>(EngineBuffer buffer, long offset, T[] data) where T : unmanaged
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

            var msr = deviceContext.Map(b, MapMode.WriteNoOverwrite, MapFlags.None);
            frameCounters.SubresourceMaps++;

            nint position = msr.DataPointer + Marshal.SizeOf(default(T)) * (nint)offset;
            UnsafeUtilities.Write(position, data);

            deviceContext.Unmap(b, 0);
            frameCounters.SubresourceUnmaps++;

            frameCounters.BufferWrites++;

            return true;
        }

        /// <inheritdoc/>
        public T[] ReadBuffer<T>(EngineBuffer buffer, int length) where T : unmanaged
        {
            return ReadBuffer<T>(buffer, 0, length);
        }
        /// <inheritdoc/>
        public T[] ReadBuffer<T>(EngineBuffer buffer, long offset, int length) where T : unmanaged
        {
            var b = buffer?.GetBuffer();
            if (b == null)
            {
                return [];
            }

            T[] data = new T[length];

            var msr = deviceContext.Map(b, MapMode.Read, MapFlags.None);
            frameCounters.SubresourceMaps++;

            UnsafeUtilities.Read(msr.DataPointer + Marshal.SizeOf(default(T)) * (nint)offset, data);

            deviceContext.Unmap(b, 0);
            frameCounters.SubresourceUnmaps++;

            frameCounters.BufferReads++;

            return data;
        }

        /// <inheritdoc/>
        public void Draw(uint vertexCount, uint startVertexLocation)
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
        public void DrawIndexed(uint indexCount, uint startIndexLocation, uint baseVertexLocation)
        {
            if (indexCount <= 0)
            {
                return;
            }

            deviceContext.DrawIndexed(indexCount, startIndexLocation, (int)baseVertexLocation);
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
        public void DrawInstanced(uint vertexCountPerInstance, uint instanceCount, uint startVertexLocation, uint startInstanceLocation)
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
        public void DrawInstanced(uint instanceCount, uint startInstanceLocation, BufferDescriptor vertexBuffer)
        {
            DrawInstanced(vertexBuffer.Count, instanceCount, vertexBuffer.BufferOffset, startInstanceLocation);
        }
        /// <inheritdoc/>
        public void DrawIndexedInstanced(uint indexCountPerInstance, uint instanceCount, uint startIndexLocation, uint baseVertexLocation, uint startInstanceLocation)
        {
            if (indexCountPerInstance <= 0)
            {
                return;
            }

            if (instanceCount <= 0)
            {
                return;
            }

            deviceContext.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, (int)baseVertexLocation, startInstanceLocation);
            UpdateDrawPrimitives(indexCountPerInstance, instanceCount);
        }
        /// <inheritdoc/>
        public void DrawIndexedInstanced(uint instanceCount, uint startInstanceLocation, BufferDescriptor indexBuffer, BufferDescriptor vertexBuffer)
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
        private void UpdateDrawPrimitives(uint count, uint instanceCount)
        {
            frameCounters.DrawCallsPerFrame++;

            //Figure number of real primitives using the primitive topology
            uint div = (uint)currentIAPrimitiveTopology switch
            {
                >= 2 and <= 3 or >= 10 and <= 11 => 2,
                >= 4 and <= 5 or >= 12 and <= 13 => 3,
                _ => 1,
            };

            uint primitives = count / div;
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
        public static implicit operator ID3D11DeviceContext3(EngineDeviceContext value)
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
