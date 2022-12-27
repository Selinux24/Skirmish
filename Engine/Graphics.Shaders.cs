using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Graphics shaders management
    /// </summary>
    public sealed partial class Graphics
    {
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

        /// <summary>
        /// Current vertex shader
        /// </summary>
        private EngineVertexShader currentVertexShader;
        /// <summary>
        /// Current vertex shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentVertexShaderConstantBufferState = new ShaderConstantBufferState();
        /// <summary>
        /// Current vertex shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentVertexShaderResourceViewState = new ShaderResourceState();
        /// <summary>
        /// Current vertex shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentVertexShaderSamplerState = new ShaderSamplerState();

        /// <summary>
        /// Current hull shader
        /// </summary>
        private EngineHullShader currentHullShader;
        /// <summary>
        /// Current hull shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentHullShaderConstantBufferState = new ShaderConstantBufferState();
        /// <summary>
        /// Current hull shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentHullShaderResourceViewState = new ShaderResourceState();
        /// <summary>
        /// Current hull shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentHullShaderSamplerState = new ShaderSamplerState();

        /// <summary>
        /// Current domain shader
        /// </summary>
        private EngineDomainShader currentDomainShader;
        /// <summary>
        /// Current domain shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentDomainShaderConstantBufferState = new ShaderConstantBufferState();
        /// <summary>
        /// Current domain shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentDomainShaderResourceViewState = new ShaderResourceState();
        /// <summary>
        /// Current domain shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentDomainShaderSamplerState = new ShaderSamplerState();

        /// <summary>
        /// Current geometry shader
        /// </summary>
        private EngineGeometryShader currentGeomeryShader;
        /// <summary>
        /// Current geometry shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentGeometryShaderConstantBufferState = new ShaderConstantBufferState();
        /// <summary>
        /// Current geometry shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentGeometryShaderResourceViewState = new ShaderResourceState();
        /// <summary>
        /// Current geometry shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentGeometryShaderSamplerState = new ShaderSamplerState();

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
        private readonly ShaderConstantBufferState currentPixelShaderConstantBufferState = new ShaderConstantBufferState();
        /// <summary>
        /// Current pixel shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentPixelShaderResourceViewState = new ShaderResourceState();
        /// <summary>
        /// Current pixel shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentPixelShaderSamplerState = new ShaderSamplerState();

        /// <summary>
        /// Current compute shader
        /// </summary>
        private EngineComputeShader currentComputeShader;
        /// <summary>
        /// Current compute shader constants buffer state
        /// </summary>
        private readonly ShaderConstantBufferState currentComputeShaderConstantBufferState = new ShaderConstantBufferState();
        /// <summary>
        /// Current compute shader resource views state
        /// </summary>
        private readonly ShaderResourceState currentComputeShaderResourceViewState = new ShaderResourceState();
        /// <summary>
        /// Current compute shader sampler state
        /// </summary>
        private readonly ShaderSamplerState currentComputeShaderSamplerState = new ShaderSamplerState();

        /// <summary>
        /// Creates a new Input Layout for a Shader
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Code bytesk</param>
        /// <param name="elements">Input elements</param>
        /// <returns>Returns a new Input Layout</returns>
        internal InputLayout CreateInputLayout(string name, byte[] bytes, InputElement[] elements)
        {
            return new InputLayout(device, bytes, elements)
            {
                DebugName = name,
            };
        }

        /// <summary>
        /// Clear shader resources
        /// </summary>
        private void ClearShaderResources()
        {
            currentVertexShaderConstantBufferState.Clear(deviceContext.VertexShader);
            currentVertexShaderResourceViewState.Clear(deviceContext.VertexShader);
            currentVertexShaderSamplerState.Clear(deviceContext.VertexShader);

            currentHullShaderConstantBufferState.Clear(deviceContext.HullShader);
            currentHullShaderResourceViewState.Clear(deviceContext.HullShader);
            currentHullShaderSamplerState.Clear(deviceContext.HullShader);

            currentDomainShaderConstantBufferState.Clear(deviceContext.DomainShader);
            currentDomainShaderResourceViewState.Clear(deviceContext.DomainShader);
            currentDomainShaderSamplerState.Clear(deviceContext.DomainShader);

            currentGeometryShaderConstantBufferState.Clear(deviceContext.GeometryShader);
            currentGeometryShaderResourceViewState.Clear(deviceContext.GeometryShader);
            currentGeometryShaderSamplerState.Clear(deviceContext.GeometryShader);

            currentPixelShaderConstantBufferState.Clear(deviceContext.PixelShader);
            currentPixelShaderResourceViewState.Clear(deviceContext.PixelShader);
            currentPixelShaderSamplerState.Clear(deviceContext.PixelShader);

            currentComputeShaderConstantBufferState.Clear(deviceContext.ComputeShader);
            currentComputeShaderResourceViewState.Clear(deviceContext.ComputeShader);
            currentComputeShaderSamplerState.Clear(deviceContext.ComputeShader);
        }

        /// <summary>
        /// Gets the shader compilation flags
        /// </summary>
        private static ShaderFlags GetShaderCompilationFlags()
        {
#if DEBUG
            return ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.DebugNameForSource;
#else
            return ShaderFlags.EnableStrictness;
#endif
        }

        /// <summary>
        /// Loads vertex shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Retuns vertex shader description</returns>
        public EngineVertexShader CompileVertexShader(string name, string entryPoint, string filename, string profile)
        {
            return CompileVertexShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile);
        }
        /// <summary>
        /// Loads vertex shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Retuns vertex shader description</returns>
        public EngineVertexShader CompileVertexShader(string name, string entryPoint, string filename, string profile, out string compilationErrors)
        {
            return CompileVertexShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads vertex shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Retuns vertex shader description</returns>
        public EngineVertexShader CompileVertexShader(string name, string entryPoint, byte[] byteCode, string profile)
        {
            var res = CompileVertexShader(
                name,
                entryPoint,
                byteCode,
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineVertexShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads vertex shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Retuns vertex shader description</returns>
        public EngineVertexShader CompileVertexShader(string name, string entryPoint, byte[] byteCode, string profile, out string compilationErrors)
        {
            compilationErrors = null;

            var shaderFlags = GetShaderCompilationFlags();
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new EngineVertexShader(name, new VertexShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
            }
        }
        /// <summary>
        /// Loads a vertex shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineVertexShader LoadVertexShader(string name, byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var code = ShaderBytecode.FromStream(ms))
                {
                    return new EngineVertexShader(name, new VertexShader(device, code.Data), code);
                }
            }
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
        /// Loads a hull shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns hull shader description</returns>
        public EngineHullShader CompileHullShader(string name, string entryPoint, string filename, string profile)
        {
            var res = CompileHullShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineHullShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a hull shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns hull shader description</returns>
        public EngineHullShader CompileHullShader(string name, string entryPoint, string filename, string profile, out string compilationErrors)
        {
            return CompileHullShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a hull shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns hull shader description</returns>
        public EngineHullShader CompileHullShader(string name, string entryPoint, byte[] byteCode, string profile)
        {
            var res = CompileHullShader(
                name,
                entryPoint,
                byteCode,
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineHullShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a hull shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns hull shader description</returns>
        public EngineHullShader CompileHullShader(string name, string entryPoint, byte[] byteCode, string profile, out string compilationErrors)
        {
            compilationErrors = null;

            var shaderFlags = GetShaderCompilationFlags();
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new EngineHullShader(name, new HullShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
            }
        }
        /// <summary>
        /// Loads a hull shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineHullShader LoadHullShader(string name, byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var code = ShaderBytecode.FromStream(ms))
                {
                    return new EngineHullShader(name, new HullShader(device, code.Data), code);
                }
            }
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
        /// Loads a domain shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns domain shader description</returns>
        public EngineDomainShader CompileDomainShader(string name, string entryPoint, string filename, string profile)
        {
            var res = CompileDomainShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineDomainShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a domain shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns domain shader description</returns>
        public EngineDomainShader CompileDomainShader(string name, string entryPoint, string filename, string profile, out string compilationErrors)
        {
            return CompileDomainShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a domain shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns domain shader description</returns>
        public EngineDomainShader CompileDomainShader(string name, string entryPoint, byte[] byteCode, string profile)
        {
            var res = CompileDomainShader(
                name,
                entryPoint,
                byteCode,
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineDomainShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a domain shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns domain shader description</returns>
        public EngineDomainShader CompileDomainShader(string name, string entryPoint, byte[] byteCode, string profile, out string compilationErrors)
        {
            compilationErrors = null;

            var shaderFlags = GetShaderCompilationFlags();
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new EngineDomainShader(name, new DomainShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
            }
        }
        /// <summary>
        /// Loads a domain shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineDomainShader LoadDomainShader(string name, byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var code = ShaderBytecode.FromStream(ms))
                {
                    return new EngineDomainShader(name, new DomainShader(device, code.Data), code);
                }
            }
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
        /// Loads a geometry shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns geometry shader description</returns>
        public EngineGeometryShader CompileGeometryShader(string name, string entryPoint, string filename, string profile)
        {
            var res = CompileGeometryShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineGeometryShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a geometry shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns geometry shader description</returns>
        public EngineGeometryShader CompileGeometryShader(string name, string entryPoint, string filename, string profile, out string compilationErrors)
        {
            return CompileGeometryShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a geometry shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns geometry shader description</returns>
        public EngineGeometryShader CompileGeometryShader(string name, string entryPoint, byte[] byteCode, string profile)
        {
            var res = CompileGeometryShader(
                name,
                entryPoint,
                byteCode,
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineGeometryShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a geometry shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns geometry shader description</returns>
        public EngineGeometryShader CompileGeometryShader(string name, string entryPoint, byte[] byteCode, string profile, out string compilationErrors)
        {
            compilationErrors = null;

            var shaderFlags = GetShaderCompilationFlags();
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new EngineGeometryShader(name, new GeometryShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
            }
        }
        /// <summary>
        /// Loads a geometry shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineGeometryShader LoadGeometryShader(string name, byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var code = ShaderBytecode.FromStream(ms))
                {
                    return new EngineGeometryShader(name, new GeometryShader(device, code.Data), code);
                }
            }
        }

        /// <summary>
        /// Loads a geometry shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="soElements">Stream-out elements</param>
        /// <returns>Returns geometry shader description</returns>
        public EngineGeometryShader CompileGeometryShaderWithStreamOut(string name, string entryPoint, string filename, string profile, IEnumerable<EngineStreamOutputElement> soElements)
        {
            var res = CompileGeometryShaderWithStreamOut(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                soElements,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineGeometryShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a geometry shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <param name="soElements">Stream-out elements</param>
        /// <returns>Returns geometry shader description</returns>
        public EngineGeometryShader CompileGeometryShaderWithStreamOut(string name, string entryPoint, string filename, string profile, IEnumerable<EngineStreamOutputElement> soElements, out string compilationErrors)
        {
            return CompileGeometryShaderWithStreamOut(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                soElements,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a geometry shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="soElements">Stream-out elements</param>
        /// <returns>Returns geometry shader description</returns>
        public EngineGeometryShader CompileGeometryShaderWithStreamOut(string name, string entryPoint, byte[] byteCode, string profile, IEnumerable<EngineStreamOutputElement> soElements)
        {
            var res = CompileGeometryShaderWithStreamOut(
                name,
                entryPoint,
                byteCode,
                profile,
                soElements,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineGeometryShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a geometry shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <param name="soElements">Stream-out elements</param>
        /// <returns>Returns geometry shader description</returns>
        public EngineGeometryShader CompileGeometryShaderWithStreamOut(string name, string entryPoint, byte[] byteCode, string profile, IEnumerable<EngineStreamOutputElement> soElements, out string compilationErrors)
        {
            compilationErrors = null;

            var shaderFlags = GetShaderCompilationFlags();
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                var so = soElements.Select(s => (StreamOutputElement)s).ToArray();

                return new EngineGeometryShader(name, new GeometryShader(device, cmpResult.Bytecode, so, Array.Empty<int>(), 0), cmpResult.Bytecode);
            }
        }
        /// <summary>
        /// Loads a geometry shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <param name="soElements">Stream-out elements</param>
        /// <returns>Returns loaded shader</returns>
        public EngineGeometryShader LoadGeometryShaderWithStreamOut(string name, byte[] bytes, IEnumerable<EngineStreamOutputElement> soElements)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var code = ShaderBytecode.FromStream(ms))
                {
                    var so = soElements.Select(s => (StreamOutputElement)s).ToArray();

                    return new EngineGeometryShader(name, new GeometryShader(device, code.Data, so, Array.Empty<int>(), 0), code);
                }
            }
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
        /// Loads a pixel shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns pixel shader description</returns>
        public EnginePixelShader CompilePixelShader(string name, string entryPoint, string filename, string profile)
        {
            var res = CompilePixelShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EnginePixelShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a pixel shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns pixel shader description</returns>
        public EnginePixelShader CompilePixelShader(string name, string entryPoint, string filename, string profile, out string compilationErrors)
        {
            return CompilePixelShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a pixel shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns pixel shader description</returns>
        public EnginePixelShader CompilePixelShader(string name, string entryPoint, byte[] byteCode, string profile)
        {
            var res = CompilePixelShader(
                name,
                entryPoint,
                byteCode,
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EnginePixelShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a pixel shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns pixel shader description</returns>
        public EnginePixelShader CompilePixelShader(string name, string entryPoint, byte[] byteCode, string profile, out string compilationErrors)
        {
            compilationErrors = null;

            var shaderFlags = GetShaderCompilationFlags();
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new EnginePixelShader(name, new PixelShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
            }
        }
        /// <summary>
        /// Loads a pixel shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EnginePixelShader LoadPixelShader(string name, byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var code = ShaderBytecode.FromStream(ms))
                {
                    return new EnginePixelShader(name, new PixelShader(device, code.Data), code);
                }
            }
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
        /// Loads a compute shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns compute shader description</returns>
        public EngineComputeShader CompileComputeShader(string name, string entryPoint, string filename, string profile)
        {
            var res = CompileComputeShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineComputeShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a compute shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns compute shader description</returns>
        public EngineComputeShader CompileComputeShader(string name, string entryPoint, string filename, string profile, out string compilationErrors)
        {
            return CompileComputeShader(
                name,
                entryPoint,
                File.ReadAllBytes(filename),
                profile,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a compute shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns compute shader description</returns>
        public EngineComputeShader CompileComputeShader(string name, string entryPoint, byte[] byteCode, string profile)
        {
            var res = CompileComputeShader(
                name,
                entryPoint,
                byteCode,
                profile,
                out string compilationErrors);

            if (!string.IsNullOrEmpty(compilationErrors))
            {
                Logger.WriteError(this, $"EngineComputeShader: {compilationErrors}");
            }

            return res;
        }
        /// <summary>
        /// Loads a compute shader from byte code
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns compute shader description</returns>
        public EngineComputeShader CompileComputeShader(string name, string entryPoint, byte[] byteCode, string profile, out string compilationErrors)
        {
            compilationErrors = null;

            var shaderFlags = GetShaderCompilationFlags();
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new EngineComputeShader(name, new ComputeShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
            }
        }
        /// <summary>
        /// Loads a compute shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineComputeShader LoadComputeShader(string name, byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var code = ShaderBytecode.FromStream(ms))
                {
                    return new EngineComputeShader(name, new ComputeShader(device, code.Data), code);
                }
            }
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
    }
}
