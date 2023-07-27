using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using SharpDX.D3DCompiler;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Graphics shaders management
    /// </summary>
    public sealed partial class Graphics
    {
        /// <summary>
        /// Creates a new Input Layout for a Shader
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Code bytes</param>
        /// <param name="elements">Input elements</param>
        /// <returns>Returns a new Input Layout</returns>
        private EngineInputLayout CreateInputLayout(string name, byte[] bytes, InputElement[] elements)
        {
            return new EngineInputLayout(name, new InputLayout(device, bytes, elements));
        }
        /// <summary>
        /// Creates a new Input Layout for a vertext data type
        /// </summary>
        /// <typeparam name="T">Vertex data type</typeparam>
        /// <param name="name">Name</param>
        /// <param name="bytes">Code bytes</param>
        /// <param name="bufferSlot">Buffer slot</param>
        /// <returns>Returns a new Input Layout</returns>
        public EngineInputLayout CreateInputLayout<T>(string name, byte[] bytes, int bufferSlot) where T : struct, IVertexData
        {
            var inputElements = default(T).GetInput(bufferSlot);

            return CreateInputLayout(name, bytes, inputElements);
        }
        /// <summary>
        /// Creates a new Input Layout for a <see cref="BufferManagerVertices"/> instance
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Code bytes</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="instanced">Create a instanced layout</param>
        /// <returns>Returns a new Input Layout</returns>
        public EngineInputLayout CreateInputLayout(string name, byte[] bytes, BufferManagerVertices vertices, bool instanced)
        {
            var inputElements = instanced ?
                vertices.Input.ToArray() :
                vertices.Input.Where(i => i.Classification == InputClassification.PerVertexData).ToArray();

            return CreateInputLayout(name, bytes, inputElements);
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
            using var includeManager = new ShaderIncludeManager();
            using var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager);
            if (cmpResult.HasErrors)
            {
                compilationErrors = cmpResult.Message;
            }

            return new EngineVertexShader(name, new VertexShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
        }
        /// <summary>
        /// Loads a vertex shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineVertexShader LoadVertexShader(string name, byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            ms.Position = 0;

            using var code = ShaderBytecode.FromStream(ms);
            return new EngineVertexShader(name, new VertexShader(device, code.Data), code);
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
            using var includeManager = new ShaderIncludeManager();
            using var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager);
            if (cmpResult.HasErrors)
            {
                compilationErrors = cmpResult.Message;
            }

            return new EngineHullShader(name, new HullShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
        }
        /// <summary>
        /// Loads a hull shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineHullShader LoadHullShader(string name, byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            ms.Position = 0;

            using var code = ShaderBytecode.FromStream(ms);
            return new EngineHullShader(name, new HullShader(device, code.Data), code);
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
            using var includeManager = new ShaderIncludeManager();
            using var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager);
            if (cmpResult.HasErrors)
            {
                compilationErrors = cmpResult.Message;
            }

            return new EngineDomainShader(name, new DomainShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
        }
        /// <summary>
        /// Loads a domain shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineDomainShader LoadDomainShader(string name, byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            ms.Position = 0;

            using var code = ShaderBytecode.FromStream(ms);
            return new EngineDomainShader(name, new DomainShader(device, code.Data), code);
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
            using var includeManager = new ShaderIncludeManager();
            using var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager);
            if (cmpResult.HasErrors)
            {
                compilationErrors = cmpResult.Message;
            }

            return new EngineGeometryShader(name, new GeometryShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
        }
        /// <summary>
        /// Loads a geometry shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineGeometryShader LoadGeometryShader(string name, byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            ms.Position = 0;

            using var code = ShaderBytecode.FromStream(ms);
            return new EngineGeometryShader(name, new GeometryShader(device, code.Data), code);
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
            using var includeManager = new ShaderIncludeManager();
            using var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager);
            if (cmpResult.HasErrors)
            {
                compilationErrors = cmpResult.Message;
            }

            var so = soElements.Select(s => (StreamOutputElement)s).ToArray();

            return new EngineGeometryShader(name, new GeometryShader(device, cmpResult.Bytecode, so, Array.Empty<int>(), 0), cmpResult.Bytecode);
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
            using var ms = new MemoryStream(bytes);
            ms.Position = 0;

            using var code = ShaderBytecode.FromStream(ms);
            var so = soElements.Select(s => (StreamOutputElement)s).ToArray();

            return new EngineGeometryShader(name, new GeometryShader(device, code.Data, so, Array.Empty<int>(), 0), code);
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
            using var includeManager = new ShaderIncludeManager();
            using var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager);
            if (cmpResult.HasErrors)
            {
                compilationErrors = cmpResult.Message;
            }

            return new EnginePixelShader(name, new PixelShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
        }
        /// <summary>
        /// Loads a pixel shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EnginePixelShader LoadPixelShader(string name, byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            ms.Position = 0;

            using var code = ShaderBytecode.FromStream(ms);
            return new EnginePixelShader(name, new PixelShader(device, code.Data), code);
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
            using var includeManager = new ShaderIncludeManager();
            using var cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                shaderFlags,
                EffectFlags.None,
                null,
                includeManager);
            if (cmpResult.HasErrors)
            {
                compilationErrors = cmpResult.Message;
            }

            return new EngineComputeShader(name, new ComputeShader(device, cmpResult.Bytecode), cmpResult.Bytecode);
        }
        /// <summary>
        /// Loads a compute shader from pre-compiled file
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded shader</returns>
        public EngineComputeShader LoadComputeShader(string name, byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            ms.Position = 0;

            using var code = ShaderBytecode.FromStream(ms);
            return new EngineComputeShader(name, new ComputeShader(device, code.Data), code);
        }
    }
}
