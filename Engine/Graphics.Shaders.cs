using Engine.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine
{
    using Vortice.D3DCompiler;
    using Vortice.Direct3D;
    using Vortice.Direct3D11;

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
        public EngineInputLayout CreateInputLayout(string name, byte[] bytes, EngineInputElement[] elements)
        {
            var inputElements = elements.Select(e => new InputElementDescription(e.SemanticName, e.SemanticIndex, e.Format, e.AlignedByteOffset, e.Slot, (InputClassification)e.Classification, e.InstanceDataStepRate)).ToArray();

            return new(name, device.CreateInputLayout(inputElements, bytes));
        }
        /// <summary>
        /// Creates a new Input Layout for a vertext data type
        /// </summary>
        /// <typeparam name="T">Vertex data type</typeparam>
        /// <param name="name">Name</param>
        /// <param name="bytes">Code bytes</param>
        /// <param name="bufferSlot">Buffer slot</param>
        /// <returns>Returns a new Input Layout</returns>
        public EngineInputLayout CreateInputLayout<T>(string name, byte[] bytes, uint bufferSlot) where T : unmanaged, IVertexData
        {
            var inputElements = default(T).GetInput(bufferSlot);

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
                Game.Logger.WriteError(this, $"EngineVertexShader: {compilationErrors}");
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
            var cmpResult = Compiler.Compile(
                byteCode,
                [],
                includeManager,
                entryPoint,
                null,
                profile,
                shaderFlags,
                EffectFlags.None,
                out Blob blob, out Blob errorBlob);

            if (cmpResult.Failure)
            {
                compilationErrors = errorBlob.ToString();

                return null;
            }

            return new EngineVertexShader(name, device.CreateVertexShader(blob), blob.AsBytes());
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
                Game.Logger.WriteError(this, $"EngineHullShader: {compilationErrors}");
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
                Game.Logger.WriteError(this, $"EngineHullShader: {compilationErrors}");
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
            var cmpResult = Compiler.Compile(
                byteCode,
                [],
                includeManager,
                entryPoint,
                null,
                profile,
                shaderFlags,
                EffectFlags.None,
                out Blob blob, out Blob errorBlob);

            if (cmpResult.Failure)
            {
                compilationErrors = Encoding.UTF8.GetString(errorBlob.AsBytes());

                return null;
            }

            return new EngineHullShader(name, device.CreateHullShader(blob), blob.AsBytes());
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
                Game.Logger.WriteError(this, $"EngineDomainShader: {compilationErrors}");
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
                Game.Logger.WriteError(this, $"EngineDomainShader: {compilationErrors}");
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
            var cmpResult = Compiler.Compile(
                byteCode,
                [],
                includeManager,
                entryPoint,
                null,
                profile,
                shaderFlags,
                EffectFlags.None,
                out Blob blob, out Blob errorBlob);

            if (cmpResult.Failure)
            {
                compilationErrors = Encoding.UTF8.GetString(errorBlob.AsBytes());

                return null;
            }

            return new EngineDomainShader(name, device.CreateDomainShader(blob), blob.AsBytes());
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
                Game.Logger.WriteError(this, $"EngineGeometryShader: {compilationErrors}");
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
                Game.Logger.WriteError(this, $"EngineGeometryShader: {compilationErrors}");
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
            var cmpResult = Compiler.Compile(
                byteCode,
                [],
                includeManager,
                entryPoint,
                null,
                profile,
                shaderFlags,
                EffectFlags.None,
                out Blob blob, out Blob errorBlob);

            if (cmpResult.Failure)
            {
                compilationErrors = Encoding.UTF8.GetString(errorBlob.AsBytes());

                return null;
            }

            return new EngineGeometryShader(name, device.CreateGeometryShader(blob), blob.AsBytes());
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
                Game.Logger.WriteError(this, $"EngineGeometryShader: {compilationErrors}");
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
                Game.Logger.WriteError(this, $"EngineGeometryShader: {compilationErrors}");
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
            var cmpResult = Compiler.Compile(
                byteCode,
                [],
                includeManager,
                entryPoint,
                null,
                profile,
                shaderFlags,
                EffectFlags.None,
                out Blob blob, out Blob errorBlob);

            if (cmpResult.Failure)
            {
                compilationErrors = Encoding.UTF8.GetString(errorBlob.AsBytes());

                return null;
            }

            var so = soElements.Select(s => (SODeclarationEntry)s).ToArray();

            var s = device.CreateGeometryShaderWithStreamOutput(
                blob.BufferPointer, blob.BufferSize,
                so, (uint)so.Length,
                null, 0,
                0);

            return new EngineGeometryShader(name, s, blob.AsBytes());
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
                Game.Logger.WriteError(this, $"EnginePixelShader: {compilationErrors}");
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
                Game.Logger.WriteError(this, $"EnginePixelShader: {compilationErrors}");
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
            var cmpResult = Compiler.Compile(
                byteCode,
                [],
                includeManager,
                entryPoint,
                null,
                profile,
                shaderFlags,
                EffectFlags.None,
                out Blob blob, out Blob errorBlob);

            if (cmpResult.Failure)
            {
                compilationErrors = Encoding.UTF8.GetString(errorBlob.AsBytes());

                return null;
            }

            return new EnginePixelShader(name, device.CreatePixelShader(blob), blob.AsBytes());
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
                Game.Logger.WriteError(this, $"EngineComputeShader: {compilationErrors}");
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
                Game.Logger.WriteError(this, $"EngineComputeShader: {compilationErrors}");
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
            var cmpResult = Compiler.Compile(
                byteCode,
                [],
                includeManager,
                entryPoint,
                null,
                profile,
                shaderFlags,
                EffectFlags.None,
                out Blob blob, out Blob errorBlob);

            if (cmpResult.Failure)
            {
                compilationErrors = Encoding.UTF8.GetString(errorBlob.AsBytes());

                return null;
            }

            return new EngineComputeShader(name, device.CreateComputeShader(blob), blob.AsBytes());
        }
    }
}
