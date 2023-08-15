using SharpDX;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Engine device context interface
    /// </summary>
    public interface IEngineDeviceContext : IDisposable
    {
        /// <summary>
        /// Gets de device context name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets or sets the Input Assembler Primitive Topology
        /// </summary>
        Topology IAPrimitiveTopology { get; set; }
        /// <summary>
        /// Gets or sets the Inpur Assembler Input Layout
        /// </summary>
        EngineInputLayout IAInputLayout { get; set; }

        /// <summary>
        /// Clears the device context state
        /// </summary>
        void ClearState();

        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        void SetViewport(Viewport viewport);
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        void SetViewport(ViewportF viewport);
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        void SetViewports(IEnumerable<Viewport> viewports);
        /// <summary>
        /// Sets viewports
        /// </summary>
        /// <param name="viewports">Viewports</param>
        void SetViewports(IEnumerable<ViewportF> viewports);

        /// <summary>
        /// Set render target
        /// </summary>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="depthMap">Depth map</param>
        void SetRenderTargets(EngineRenderTargetView renderTargets, EngineDepthStencilView depthMap);
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clearRT">Indicates whether the target must be cleared</param>
        /// <param name="clearRTColor">Render target clear color</param>
        void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor);
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clearRT">Indicates whether the target must be cleared</param>
        /// <param name="clearRTColor">Render target clear color</param>
        /// <param name="freeOMResources">Indicates whether the Output merger Shader Resources must be cleared</param>
        void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, bool freeOMResources);
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
        void SetRenderTargets(EngineRenderTargetView renderTargets, bool clearRT, Color4 clearRTColor, EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil, bool freeOMResources);

        /// <summary>
        /// Clear depth / stencil buffer
        /// </summary>
        /// <param name="depthMap">Depth buffer</param>
        /// <param name="clearDepth">Indicates whether the depth buffer must be cleared</param>
        /// <param name="clearStencil">Indicates whether the stencil buffer must be cleared</param>
        void ClearDepthStencilBuffer(EngineDepthStencilView depthMap, bool clearDepth, bool clearStencil);

        /// <summary>
        /// Sets depth stencil state
        /// </summary>
        /// <param name="state">Depth stencil state</param>
        /// <param name="stencilRef">Stencil reference</param>
        void SetDepthStencilState(EngineDepthStencilState state, int stencilRef = 0);

        /// <summary>
        /// Sets blend state
        /// </summary>
        /// <param name="state">Blend state</param>
        void SetBlendState(EngineBlendState state);

        /// <summary>
        /// Sets rasterizer state
        /// </summary>
        /// <param name="state">Rasterizer state</param>
        void SetRasterizerState(EngineRasterizerState state);

        /// <summary>
        /// Bind an array of vertex buffers to the input-assembler stage.
        /// </summary>
        /// <param name="firstSlot">The first input slot for binding</param>
        /// <param name="vertexBufferBindings">A reference to an array of VertexBufferBinding</param>
        void IASetVertexBuffers(int firstSlot, params EngineVertexBufferBinding[] vertexBufferBindings);
        /// <summary>
        /// Bind an index buffer to the input-assembler stage.
        /// </summary>
        /// <param name="indexBufferRef">A reference to an Buffer object</param>
        /// <param name="format">A SharpDX.DXGI.Format that specifies the format of the data in the index buffer</param>
        /// <param name="offset">Offset (in bytes) from the start of the index buffer to the first index to use</param>
        void IASetIndexBuffer(EngineBuffer indexBufferRef, Format format, int offset);

        /// <summary>
        /// Clear shader resources
        /// </summary>
        void ClearShaderResources();

        /// <summary>
        /// Sets the vertex shader in the current device context
        /// </summary>
        /// <param name="vertexShader">Vertex shader</param>
        void SetVertexShader(EngineVertexShader vertexShader);
        /// <summary>
        /// Removes the vertex shader from the current device context
        /// </summary>
        void ClearVertexShader();
        /// <summary>
        /// Sets the constant buffer to the current vertex shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        void SetVertexShaderConstantBuffer(int slot, IEngineConstantBuffer buffer);
        /// <summary>
        /// Sets the constant buffer list to the current vertex shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        void SetVertexShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList);
        /// <summary>
        /// Sets the specified resource in the current vertex shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource view</param>
        void SetVertexShaderResourceView(int slot, EngineShaderResourceView resourceView);
        /// <summary>
        /// Sets the specified resource in the current vertex shader shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceViews">Resource view list</param>
        void SetVertexShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews);
        /// <summary>
        /// Sets the specified sampler state in the current vertex shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        void SetVertexShaderSampler(int slot, EngineSamplerState samplerState);
        /// <summary>
        /// Sets the specified sampler state in the current vertex shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Sampler state list</param>
        void SetVertexShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates);


        /// <summary>
        /// Sets the hull shader in the current device context
        /// </summary>
        /// <param name="hullShader">Hull shader</param>
        void SetHullShader(EngineHullShader hullShader);
        /// <summary>
        /// Removes the hull shader from the current device context
        /// </summary>
        void ClearHullShader();
        /// <summary>
        /// Sets the constant buffer to the current hull shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        void SetHullShaderConstantBuffer(int slot, IEngineConstantBuffer buffer);
        /// <summary>
        /// Sets the constant buffer list to the current hull shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        void SetHullShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList);
        /// <summary>
        /// Sets the specified resource in the current hull shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        void SetHullShaderResourceView(int slot, EngineShaderResourceView resourceView);
        /// <summary>
        /// Sets the specified resource in the current hull shader shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceViews">Resource list</param>
        void SetHullShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews);
        /// <summary>
        /// Sets the specified sampler state in the current hull shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        void SetHullShaderSampler(int slot, EngineSamplerState samplerState);
        /// <summary>
        /// Sets the specified sampler state in the current hull shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Sampler state list</param>
        void SetHullShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates);


        /// <summary>
        /// Sets the domain shader in the current device context
        /// </summary>
        /// <param name="domainShader">Domain shader</param>
        void SetDomainShader(EngineDomainShader domainShader);
        /// <summary>
        /// Removes the domain shader from the current device context
        /// </summary>
        void ClearDomainShader();
        /// <summary>
        /// Sets the constant buffer to the current domain shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        void SetDomainShaderConstantBuffer(int slot, IEngineConstantBuffer buffer);
        /// <summary>
        /// Sets the constant buffer list to the current domain shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        void SetDomainShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList);
        /// <summary>
        /// Sets the specified resource in the current domain shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        void SetDomainShaderResourceView(int slot, EngineShaderResourceView resourceView);
        /// <summary>
        /// Sets the specified resource in the current domain shader shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceViews">Resource list</param>
        void SetDomainShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews);
        /// <summary>
        /// Sets the specified sampler state in the current domain shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        void SetDomainShaderSampler(int slot, EngineSamplerState samplerState);
        /// <summary>
        /// Sets the specified sampler state in the current domain shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Sampler state list</param>
        void SetDomainShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates);

        /// <summary>
        /// Sets the geometry shader in the current device context
        /// </summary>
        /// <param name="geometryShader">Geometry shader</param>
        void SetGeometryShader(EngineGeometryShader geometryShader);
        /// <summary>
        /// Removes the geometry shader from the current device context
        /// </summary>
        void ClearGeometryShader();
        /// <summary>
        /// Sets the constant buffer to the current geometry shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        void SetGeometryShaderConstantBuffer(int slot, IEngineConstantBuffer buffer);
        /// <summary>
        /// Sets the constant buffer list to the current geometry shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        void SetGeometryShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList);
        /// <summary>
        /// Sets the specified resource in the current geometry shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        void SetGeometryShaderResourceView(int slot, EngineShaderResourceView resourceView);
        /// <summary>
        /// Sets the specified resource in the current geometry shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceViews">Resource list</param>
        void SetGeometryShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews);
        /// <summary>
        /// Sets the specified sampler state in the current geometry shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        void SetGeometryShaderSampler(int slot, EngineSamplerState samplerState);
        /// <summary>
        /// Sets the specified sampler state in the current geometry shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Sampler state list</param>
        void SetGeometryShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates);
        /// <summary>
        /// Sets targets for stream output
        /// </summary>
        /// <param name="streamOutBinding">Stream output binding</param>
        void SetGeometryShaderStreamOutputTargets(IEnumerable<EngineStreamOutputBufferBinding> streamOutBinding);

        /// <summary>
        /// Sets the pixel shader in the current device context
        /// </summary>
        /// <param name="pixelShader">Pixel shader</param>
        void SetPixelShader(EnginePixelShader pixelShader);
        /// <summary>
        /// Removes the pixel shader from the current device context
        /// </summary>
        void ClearPixelShader();
        /// <summary>
        /// Sets the constant buffer to the current pixel shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        void SetPixelShaderConstantBuffer(int slot, IEngineConstantBuffer buffer);
        /// <summary>
        /// Sets the constant buffer list to the current pixel shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        void SetPixelShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList);
        /// <summary>
        /// Sets the specified resource in the current pixel shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        void SetPixelShaderResourceView(int slot, EngineShaderResourceView resourceView);
        /// <summary>
        /// Sets the specified resource in the current pixel shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceViews">Resource list</param>
        void SetPixelShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews);
        /// <summary>
        /// Sets the specified sampler state in the current pixel shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        void SetPixelShaderSampler(int slot, EngineSamplerState samplerState);
        /// <summary>
        /// Sets the specified sampler state in the current pixel shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Sampler state list</param>
        void SetPixelShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates);

        /// <summary>
        /// Sets the compute shader in the current device context
        /// </summary>
        /// <param name="computeShader">Compute shader</param>
        void SetComputeShader(EngineComputeShader computeShader);
        /// <summary>
        /// Removes the compute shader from the current device context
        /// </summary>
        void ClearComputeShader();
        /// <summary>
        /// Sets the constant buffer to the current compute shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        void SetComputeShaderConstantBuffer(int slot, IEngineConstantBuffer buffer);
        /// <summary>
        /// Sets the constant buffer list to the current compute shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        void SetComputeShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList);
        /// <summary>
        /// Sets the specified resource in the current compute shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        void SetComputeShaderResourceView(int slot, EngineShaderResourceView resourceView);
        /// <summary>
        /// Sets the specified resource in the current compute shader shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceViews">Resource list</param>
        void SetComputeShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews);
        /// <summary>
        /// Sets the specified sampler state in the current compute shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        void SetComputeShaderSampler(int slot, EngineSamplerState samplerState);
        /// <summary>
        /// Sets the specified sampler state in the current compute shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Sampler state list</param>
        void SetComputeShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates);

        /// <summary>
        /// Apply effect pass
        /// </summary>
        /// <param name="technique"></param>
        /// <param name="index"></param>
        /// <param name="flags"></param>
        void EffectPassApply(EngineEffectTechnique technique, int index, int flags);

        /// <summary>
        /// Updates a constant buffer in the device context
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="constantBuffer">Constant buffer</param>
        /// <param name="data">Data</param>
        bool UpdateConstantBuffer<T>(EngineConstantBuffer<T> constantBuffer, T data) where T : struct, IBufferData;
        /// <summary>
        /// Updates a constant buffer in the device context
        /// </summary>
        /// <param name="constantBuffer">Constant buffer</param>
        /// <param name="data">Data</param>
        bool UpdateConstantBuffer(IEngineConstantBuffer constantBuffer, IBufferData data);

        /// <summary>
        /// Updates a texture
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="texture">Texture to update</param>
        /// <param name="data">Data to write</param>
        void UpdateTexture1D<T>(EngineShaderResourceView texture, IEnumerable<T> data) where T : struct;
        /// <summary>
        /// Updates a texture
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="texture">Texture to update</param>
        /// <param name="data">Data to write</param>
        void UpdateTexture2D<T>(EngineShaderResourceView texture, IEnumerable<T> data) where T : struct;
        /// <summary>
        /// Updates a texture
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="texture">Texture to update</param>
        /// <param name="data">Data to write</param>
        void UpdateTexture3D<T>(EngineShaderResourceView texture, IEnumerable<T> data) where T : struct;

        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        IEnumerable<T> ReadBuffer<T>(EngineBuffer buffer, int length) where T : struct;
        /// <summary>
        /// Reads an array of values from the specified buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset to read</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns readed data</returns>
        IEnumerable<T> ReadBuffer<T>(EngineBuffer buffer, long offset, int length) where T : struct;

        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        bool WriteDiscardBuffer<T>(EngineBuffer buffer, T data) where T : struct;
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        bool WriteDiscardBuffer<T>(EngineBuffer buffer, IEnumerable<T> data) where T : struct;
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        bool WriteDiscardBuffer<T>(EngineBuffer buffer, long offset, IEnumerable<T> data) where T : struct;

        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="data">Complete data</param>
        bool WriteNoOverwriteBuffer<T>(EngineBuffer buffer, IEnumerable<T> data) where T : struct;
        /// <summary>
        /// Writes data into buffer
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Buffer element offset to write</param>
        /// <param name="data">Complete data</param>
        bool WriteNoOverwriteBuffer<T>(EngineBuffer buffer, long offset, IEnumerable<T> data) where T : struct;

        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="vertexCount">Vertex count</param>
        /// <param name="startVertexLocation">Start vertex location</param>
        void Draw(int vertexCount, int startVertexLocation);
        /// <summary>
        /// Draw instanced
        /// </summary>
        /// <param name="vertexCountPerInstance">Vertex count per instance</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="startVertexLocation">Start vertex location</param>
        /// <param name="startInstanceLocation">Start instance count</param>
        void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation);
        /// <summary>
        /// Draw indexed
        /// </summary>
        /// <param name="indexCount">Index count</param>
        /// <param name="startIndexLocation">Start vertex location</param>
        /// <param name="baseVertexLocation">Base vertex location</param>
        void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation);
        /// <summary>
        /// Draw indexed instanced
        /// </summary>
        /// <param name="indexCountPerInstance">Index count per instance</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="startIndexLocation">Start index location</param>
        /// <param name="baseVertexLocation">Base vertex location</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        void DrawIndexedInstanced(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation);
        /// <summary>
        /// Draw auto
        /// </summary>
        void DrawAuto();

        /// <summary>
        /// Finish a command list
        /// </summary>
        /// <param name="name">Command list debug name</param>
        /// <param name="restoreState">Resore state</param>
        IEngineCommandList FinishCommandList(string name, bool restoreState = false);

        /// <summary>
        /// Executes a command list in the immediate context
        /// </summary>
        /// <param name="commandList">Command list</param>
        /// <param name="restoreState">Resore state</param>
        void ExecuteCommandList(IEngineCommandList commandList, bool restoreState = false);
        /// <summary>
        /// Executes a command list in the immediate context
        /// </summary>
        /// <param name="commandLists">Command list</param>
        /// <param name="restoreState">Resore state</param>
        void ExecuteCommandLists(IEnumerable<IEngineCommandList> commandLists, bool restoreState = false);
    }
}