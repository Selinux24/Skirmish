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
        /// Shader stage state helper
        /// </summary>
        /// <typeparam name="T">Type of resource</typeparam>
        class ShaderStageState<T>
        {
            /// <summary>
            /// Start slot of the last call
            /// </summary>
            public int StartSlot { get; set; }
            /// <summary>
            /// Resource list of the last call
            /// </summary>
            public IEnumerable<T> Resources { get; set; }
            /// <summary>
            /// Number of resources of the last call
            /// </summary>
            public int Count
            {
                get
                {
                    return Resources?.Count() ?? 0;
                }
            }

            /// <summary>
            /// Finds out whether the specfied resource, was attached in the same slot in the las call
            /// </summary>
            /// <param name="slot">Slot</param>
            /// <param name="resource">Resource</param>
            /// <returns>Returns true if the resource is in the specified slot since the las call</returns>
            private bool LookupResource(int slot, T resource)
            {
                int index = Resources?.ToList()?.IndexOf(resource) ?? -1;
                if (index < 0)
                {
                    //The resource is not into the collection
                    return false;
                }

                int currentSlot = index + StartSlot;
                if (currentSlot != slot)
                {
                    //The resource is in another slot
                    return false;
                }

                //The resource is part of the current collection, and is assigned to the specified slot
                return true;
            }
            /// <summary>
            /// Finds out whether the specfied resource list, were attached in the same slot in the last call
            /// </summary>
            /// <param name="startSlot">Start slot</param>
            /// <param name="resourceList">Resource list</param>
            /// <returns>Returns true if all the elements in the resource list are in the specified slot since the last call</returns>
            private bool LookupResource(int startSlot, IEnumerable<T> resourceList)
            {
                if (resourceList?.Any() != true)
                {
                    //Nothing to compare
                    return true;
                }

                if (Resources?.Any() != true)
                {
                    //Resources is empty
                    return false;
                }

                if (StartSlot == startSlot && Helper.CompareEnumerables(Resources, resourceList))
                {
                    //Same data
                    return true;
                }

                //Look up coincidences
                int currentMaxSlot = StartSlot + Resources.Count();
                int newMaxSlot = startSlot + resourceList.Count();
                if (newMaxSlot > currentMaxSlot)
                {
                    return false;
                }

                //Get range
                var range = Resources.Skip(startSlot).Take(resourceList.Count());
                if (!Helper.CompareEnumerables(range, resourceList))
                {
                    //The specified list is not into the current resource list
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Updates the resource state
            /// </summary>
            /// <param name="slot">Slot</param>
            /// <param name="resource">Resource</param>
            /// <returns>Returns true if the update must change the current resource state in the device</returns>
            public bool Update(int slot, T resource)
            {
                if (resource == null)
                {
                    return false;
                }

                if (LookupResource(slot, resource))
                {
                    return false;
                }

                if (Resources?.Any() != true)
                {
                    //Empty resource state
                    StartSlot = slot;
                    Resources = new[] { resource };

                    return true;
                }

                int setSlot = slot + StartSlot;
                if (setSlot < Resources.Count())
                {
                    //Update the slot
                    var array = Resources.ToArray();
                    array[setSlot] = resource;
                    Resources = array;

                    return true;
                }

                //Add space to the new resource
                var list = Resources.ToList();
                list.Add(resource);
                Resources = list;

                return true;
            }
            /// <summary>
            /// Updates the resource state
            /// </summary>
            /// <param name="startSlot">Start slot</param>
            /// <param name="resourceList">Resource list</param>
            /// <returns>Returns true if the update must change the current resource state in the device</returns>
            public bool Update(int startSlot, IEnumerable<T> resourceList)
            {
                if (resourceList?.Any() != true)
                {
                    return false;
                }

                if (LookupResource(startSlot, resourceList))
                {
                    return false;
                }

                if (resourceList?.Any() != true)
                {
                    //Nothing to do
                    return false;
                }

                if (Resources?.Any() != true)
                {
                    StartSlot = startSlot;
                    Resources = resourceList;

                    return true;
                }

                //Get the range to update
                var list = Resources.ToList();
                for (int i = 0; i < resourceList.Count(); i++)
                {
                    int listSlot = i + startSlot;
                    if (listSlot < list.Count)
                    {
                        list[i + startSlot] = resourceList.ElementAt(i);
                        continue;
                    }

                    list.Add(resourceList.ElementAt(i));
                }

                Resources = list;

                return true;
            }

            /// <summary>
            /// Clears the state
            /// </summary>
            public void Clear()
            {
                StartSlot = 0;
                Resources = Enumerable.Empty<T>();
            }
        }
        /// <summary>
        /// Shader constant buffers state
        /// </summary>
        class ShaderConstantBufferState : ShaderStageState<IEngineConstantBuffer>
        {
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
        }
        /// <summary>
        /// Shader resources state
        /// </summary>
        class ShaderResourceState : ShaderStageState<EngineShaderResourceView>
        {
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
        }
        /// <summary>
        /// Shader samplers state
        /// </summary>
        class ShaderSamplerState : ShaderStageState<EngineSamplerState>
        {
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
        }

        /// <summary>
        /// Null shader resources for shader clearing
        /// </summary>
        private readonly ShaderResourceView[] nullSrv = new ShaderResourceView[CommonShaderStage.InputResourceSlotCount];

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
            deviceContext.VertexShader.SetShaderResources(0, nullSrv);
            deviceContext.HullShader.SetShaderResources(0, nullSrv);
            deviceContext.DomainShader.SetShaderResources(0, nullSrv);
            deviceContext.GeometryShader.SetShaderResources(0, nullSrv);
            deviceContext.PixelShader.SetShaderResources(0, nullSrv);

            currentVertexShaderResourceViewState.Clear();
            currentGeometryShaderResourceViewState.Clear();
            currentPixelShaderResourceViewState.Clear();
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
        public EngineGeometryShader LoadGeometryShaderWithStreamOut(string name, byte[] bytes, IEnumerable<EngineStreamOutputElement> soElements)
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
    }
}
