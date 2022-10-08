using SharpDX.D3DCompiler;
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
        /// Null shader resources for shader clearing
        /// </summary>
        private readonly ShaderResourceView[] nullSrv = new ShaderResourceView[CommonShaderStage.InputResourceSlotCount];

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
        /// Sets the vertex shader in the current device context
        /// </summary>
        /// <param name="vertexShader">Vertex shader</param>
        public void SetVertexShader(EngineVertexShader vertexShader)
        {
            deviceContext.VertexShader.Set(vertexShader?.GetShader());
        }
        /// <summary>
        /// Removes the vertex shader from the current device context
        /// </summary>
        public void ClearVertexShader()
        {
            deviceContext.VertexShader.Set(null);
        }
        /// <summary>
        /// Sets the constant buffer to the current vertex shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        public void SetVertexShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            deviceContext.VertexShader.SetConstantBuffer(slot, buffer?.GetBuffer());
        }
        /// <summary>
        /// Sets the constant buffer list to the current vertex shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        public void SetVertexShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            if (bufferList?.Any() != true)
            {
                return;
            }

            var buffers = bufferList.Select(b => b?.GetBuffer()).ToArray();

            deviceContext.VertexShader.SetConstantBuffers(startSlot, buffers.Length, buffers);
        }
        /// <summary>
        /// Sets the specified resource in the current vertex shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetVertexShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            deviceContext.VertexShader.SetShaderResource(slot, resourceView?.GetResource());
        }
        /// <summary>
        /// Sets the specified resource in the current vertex shader shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetVertexShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            if (resourceViews?.Any() != true)
            {
                return;
            }

            var resources = resourceViews.Select(r => r?.GetResource()).ToArray();

            deviceContext.VertexShader.SetShaderResources(startSlot, resources.Length, resources);
        }
        /// <summary>
        /// Sets the specified sampler state in the current vertex shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        public void SetVertexShaderSampler(int slot, EngineSamplerState samplerState)
        {
            deviceContext.VertexShader.SetSampler(slot, samplerState?.GetSamplerState());
        }
        /// <summary>
        /// Sets the specified sampler state in the current vertex shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Samplers</param>
        public void SetVertexShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            if (samplerStates?.Any() != true)
            {
                return;
            }

            var samplers = samplerStates.Select(r => r?.GetSamplerState()).ToArray();

            deviceContext.VertexShader.SetSamplers(startSlot, samplers.Length, samplers);
        }

        /// <summary>
        /// Sets the geometry shader in the current device context
        /// </summary>
        /// <param name="geometryShader">Geometry shader</param>
        public void SetGeometryShader(EngineGeometryShader geometryShader)
        {
            deviceContext.GeometryShader.Set(geometryShader?.GetShader());
        }
        /// <summary>
        /// Removes the geometry shader from the current device context
        /// </summary>
        public void ClearGeometryShader()
        {
            deviceContext.GeometryShader.Set(null);
        }
        /// <summary>
        /// Sets the constant buffer to the current geometry shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        public void SetGeometryShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            deviceContext.GeometryShader.SetConstantBuffer(slot, buffer?.GetBuffer());
        }
        /// <summary>
        /// Sets the constant buffer list to the current geometry shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        public void SetGeometryShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            if (bufferList?.Any() != true)
            {
                return;
            }

            var buffers = bufferList.Select(b => b?.GetBuffer()).ToArray();

            deviceContext.GeometryShader.SetConstantBuffers(startSlot, buffers.Length, buffers);
        }
        /// <summary>
        /// Sets the specified resource in the current geometry shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetGeometryShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            deviceContext.GeometryShader.SetShaderResource(slot, resourceView?.GetResource());
        }
        /// <summary>
        /// Sets the specified resource in the current geometry shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetGeometryShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            if (resourceViews?.Any() != true)
            {
                return;
            }

            var resources = resourceViews.Select(r => r?.GetResource()).ToArray();

            deviceContext.GeometryShader.SetShaderResources(startSlot, resources.Length, resources);
        }
        /// <summary>
        /// Sets the specified sampler state in the current geometry shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        public void SetGeometryShaderSampler(int slot, EngineSamplerState samplerState)
        {
            deviceContext.GeometryShader.SetSampler(slot, samplerState?.GetSamplerState());
        }
        /// <summary>
        /// Sets the specified sampler state in the current geometry shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Samplers</param>
        public void SetGeometryShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            if (samplerStates?.Any() != true)
            {
                return;
            }

            var samplers = samplerStates.Select(r => r?.GetSamplerState()).ToArray();

            deviceContext.GeometryShader.SetSamplers(startSlot, samplers.Length, samplers);
        }

        /// <summary>
        /// Sets the pixel shader in the current device context
        /// </summary>
        /// <param name="pixelShader">Pixel shader</param>
        public void SetPixelShader(EnginePixelShader pixelShader)
        {
            deviceContext.PixelShader.Set(pixelShader?.GetShader());
        }
        /// <summary>
        /// Removes the pixel shader from the current device context
        /// </summary>
        public void ClearPixelShader()
        {
            deviceContext.PixelShader.Set(null);
        }
        /// <summary>
        /// Sets the constant buffer to the current pixel shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="slot">Slot</param>
        /// <param name="buffer">Buffer</param>
        public void SetPixelShaderConstantBuffer(int slot, IEngineConstantBuffer buffer)
        {
            deviceContext.PixelShader.SetConstantBuffer(slot, buffer?.GetBuffer());
        }
        /// <summary>
        /// Sets the constant buffer list to the current pixel shader
        /// </summary>
        /// <typeparam name="T">Type o buffer</typeparam>
        /// <param name="startSlot">Start slot</param>
        /// <param name="bufferList">Buffer list</param>
        public void SetPixelShaderConstantBuffers(int startSlot, IEnumerable<IEngineConstantBuffer> bufferList)
        {
            if (bufferList?.Any() != true)
            {
                return;
            }

            var buffers = bufferList.Select(b => b?.GetBuffer()).ToArray();

            deviceContext.PixelShader.SetConstantBuffers(startSlot, buffers.Length, buffers);
        }
        /// <summary>
        /// Sets the specified resource in the current pixel shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetPixelShaderResourceView(int slot, EngineShaderResourceView resourceView)
        {
            deviceContext.PixelShader.SetShaderResource(slot, resourceView?.GetResource());
        }
        /// <summary>
        /// Sets the specified resource in the current pixel shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="resourceView">Resource</param>
        public void SetPixelShaderResourceViews(int startSlot, IEnumerable<EngineShaderResourceView> resourceViews)
        {
            if (resourceViews?.Any() != true)
            {
                return;
            }

            var resources = resourceViews.Select(r => r?.GetResource()).ToArray();

            deviceContext.PixelShader.SetShaderResources(startSlot, resources.Length, resources);
        }
        /// <summary>
        /// Sets the specified sampler state in the current pixel shader
        /// </summary>
        /// <param name="slot">Slot</param>
        /// <param name="samplerState">Sampler</param>
        public void SetPixelShaderSampler(int slot, EngineSamplerState samplerState)
        {
            deviceContext.PixelShader.SetSampler(slot, samplerState?.GetSamplerState());
        }
        /// <summary>
        /// Sets the specified sampler state in the current pixel shader
        /// </summary>
        /// <param name="startSlot">Start slot</param>
        /// <param name="samplerStates">Samplers</param>
        public void SetPixelShaderSamplers(int startSlot, IEnumerable<EngineSamplerState> samplerStates)
        {
            if (samplerStates?.Any() != true)
            {
                return;
            }

            var samplers = samplerStates.Select(r => r?.GetSamplerState()).ToArray();

            deviceContext.PixelShader.SetSamplers(startSlot, samplers.Length, samplers);
        }

        /// <summary>
        /// Clear shader resources
        /// </summary>
        private void ClearShaderResources()
        {
            deviceContext.VertexShader.SetShaderResources(0, nullSrv);
            deviceContext.HullShader.SetShaderResources(0, nullSrv);
            deviceContext.DomainShader.SetShaderResources(0, nullSrv);
            deviceContext.GeometryShader.SetShaderResources(0, nullSrv);
            deviceContext.PixelShader.SetShaderResources(0, nullSrv);
        }
        /// <summary>
        /// Sets targets for stream output
        /// </summary>
        /// <param name="streamOutBinding">Stream output binding</param>
        public void SetStreamOutputTargets(StreamOutputBufferBinding[] streamOutBinding)
        {
            deviceContext.StreamOutput.SetTargets(streamOutBinding);
            Counters.SOTargetsSet++;
        }

        /// <summary>
        /// Gets the shader compilation flags
        /// </summary>
        private ShaderFlags GetShaderCompilationFlags()
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
        public EngineVertexShader CompileVertexShader(
            string name,
            string entryPoint,
            string filename,
            string profile)
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
        public EngineVertexShader CompileVertexShader(
            string name,
            string entryPoint,
            string filename,
            string profile,
            out string compilationErrors)
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
        public EngineVertexShader CompileVertexShader(
            string name,
            string entryPoint,
            byte[] byteCode,
            string profile)
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
        public EngineVertexShader CompileVertexShader(
            string name,
            string entryPoint,
            byte[] byteCode,
            string profile,
            out string compilationErrors)
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
        public EngineVertexShader LoadVertexShader(
            string name,
            byte[] bytes)
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
        /// Loads a geometry shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns geometry shader description</returns>
        public EngineGeometryShader CompileGeometryShader(
            string name,
            string entryPoint,
            string filename,
            string profile)
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
        public EngineGeometryShader CompileGeometryShader(
            string name,
            string entryPoint,
            string filename,
            string profile,
            out string compilationErrors)
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
        public EngineGeometryShader CompileGeometryShader(
            string name,
            string entryPoint,
            byte[] byteCode,
            string profile)
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
        public EngineGeometryShader CompileGeometryShader(
            string name,
            string entryPoint,
            byte[] byteCode,
            string profile,
            out string compilationErrors)
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
        public EngineGeometryShader LoadGeometryShader(
            string name,
            byte[] bytes)
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
        public EngineGeometryShader CompileGeometryShaderWithStreamOut(
            string name,
            string entryPoint,
            string filename,
            string profile,
            IEnumerable<EngineStreamOutputElement> soElements)
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
        public EngineGeometryShader CompileGeometryShaderWithStreamOut(
            string name,
            string entryPoint,
            string filename,
            string profile,
            IEnumerable<EngineStreamOutputElement> soElements,
            out string compilationErrors)
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
        public EngineGeometryShader CompileGeometryShaderWithStreamOut(
            string name,
            string entryPoint,
            byte[] byteCode,
            string profile,
            IEnumerable<EngineStreamOutputElement> soElements)
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
        public EngineGeometryShader CompileGeometryShaderWithStreamOut(
            string name,
            string entryPoint,
            byte[] byteCode,
            string profile,
            IEnumerable<EngineStreamOutputElement> soElements,
            out string compilationErrors)
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

                return new EngineGeometryShader(name, new GeometryShader(device, cmpResult.Bytecode, so, new int[] { }, 0), cmpResult.Bytecode);
            }
        }
        /// <summary>
        /// Loads a geometry shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <param name="soElements">Stream-out elements</param>
        /// <returns>Returns loaded shader</returns>
        public EngineGeometryShader LoadGeometryShaderWithStreamOut(
            string name,
            byte[] bytes,
            IEnumerable<EngineStreamOutputElement> soElements)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var code = ShaderBytecode.FromStream(ms))
                {
                    var so = soElements.Select(s => (StreamOutputElement)s).ToArray();

                    return new EngineGeometryShader(name, new GeometryShader(device, code.Data, so, new int[] { }, 0), code);
                }
            }
        }

        /// <summary>
        /// Loads a pixel shader from file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns pixel shader description</returns>
        public EnginePixelShader CompilePixelShader(
            string name,
            string entryPoint,
            string filename,
            string profile)
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
        public EnginePixelShader CompilePixelShader(
            string name,
            string entryPoint,
            string filename,
            string profile,
            out string compilationErrors)
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
        public EnginePixelShader CompilePixelShader(
            string name,
            string entryPoint,
            byte[] byteCode,
            string profile)
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
        public EnginePixelShader CompilePixelShader(
            string name,
            string entryPoint,
            byte[] byteCode,
            string profile,
            out string compilationErrors)
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
        public EnginePixelShader LoadPixelShader(
            string name,
            byte[] bytes)
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
    }
}
