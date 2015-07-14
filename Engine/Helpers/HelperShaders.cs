using System;
using System.IO;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace Engine.Helpers
{
    using Engine.Properties;

    /// <summary>
    /// Helper methods for shaders and effects
    /// </summary>
    public static class HelperShaders
    {
        #region Classes

        /// <summary>
        /// Vertex shader description
        /// </summary>
        public class VertexShaderDescription : IDisposable
        {
            public VertexShader Shader { get; private set; }
            public InputLayout Layout { get; private set; }

            internal VertexShaderDescription(VertexShader shader, InputLayout layout)
            {
                this.Shader = shader;
                this.Layout = layout;
            }
            public void Dispose()
            {
                if (this.Shader != null)
                {
                    this.Shader.Dispose();
                    this.Shader = null;
                }

                if (this.Layout != null)
                {
                    this.Layout.Dispose();
                    this.Layout = null;
                }
            }
        }
        /// <summary>
        /// Pixel shader description
        /// </summary>
        public class PixelShaderDescription : IDisposable
        {
            public PixelShader Shader { get; private set; }

            internal PixelShaderDescription(PixelShader shader)
            {
                this.Shader = shader;
            }
            public void Dispose()
            {
                if (this.Shader != null)
                {
                    this.Shader.Dispose();
                    this.Shader = null;
                }
            }
        }
        /// <summary>
        /// Include manager
        /// </summary>
        public class ShaderIncludeManager : CallbackBase, Include
        {
            public void Close(Stream stream)
            {
                stream.Dispose();
            }

            public Stream Open(IncludeType type, string fileName, Stream parentStream)
            {
                byte[] o = (byte[])Resources.ResourceManager.GetObject(Path.GetFileNameWithoutExtension(fileName));

                return new MemoryStream(o);
            }
        }

        #endregion

        /// <summary>
        /// Vertex shader profile
        /// </summary>
        public static string VSProfile = "vs_5_0";
        /// <summary>
        /// Pixel shader profile
        /// </summary>
        public static string PSProfile = "ps_5_0";
        /// <summary>
        /// Effect profile
        /// </summary>
        public static string FXProfile = "fx_5_0";

        /// <summary>
        /// Loads vertex shader from file
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <returns>Retuns vertex shader description</returns>
        public static VertexShaderDescription LoadVertexShader(
            this Device device,
            string filename,
            string entryPoint,
            InputElement[] input)
        {
            return LoadVertexShader(
                device,
                File.ReadAllBytes(filename),
                entryPoint,
                input);
        }
        /// <summary>
        /// Loads vertex shader from file
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Retuns vertex shader description</returns>
        public static VertexShaderDescription LoadVertexShader(
            this Device device,
            string filename,
            string entryPoint,
            InputElement[] input,
            out string compilationErrors)
        {
            return LoadVertexShader(
                device,
                File.ReadAllBytes(filename),
                entryPoint,
                input,
                out compilationErrors);
        }
        /// <summary>
        /// Loads vertex shader from byte code
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <returns>Retuns vertex shader description</returns>
        public static VertexShaderDescription LoadVertexShader(
            this Device device,
            byte[] byteCode,
            string entryPoint,
            InputElement[] input)
        {
            string compilationErrors;
            return LoadVertexShader(
                device,
                byteCode,
                entryPoint,
                input,
                out compilationErrors);
        }
        /// <summary>
        /// Loads vertex shader from byte code
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="input">Input elements</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Retuns vertex shader description</returns>
        public static VertexShaderDescription LoadVertexShader(
            this Device device,
            byte[] byteCode,
            string entryPoint,
            InputElement[] input,
            out string compilationErrors)
        {
            compilationErrors = null;
            using (ShaderIncludeManager includeManager = new ShaderIncludeManager())
            using (CompilationResult cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                VSProfile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                InputLayout layout = new InputLayout(
                    device,
                    ShaderSignature.GetInputSignature(cmpResult.Bytecode),
                    input);

                VertexShader vertexShader = new VertexShader(
                    device,
                    cmpResult.Bytecode);

                return new VertexShaderDescription(vertexShader, layout);
            }
        }
        /// <summary>
        /// Loads a pixel shader from file
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <returns>Returns pixel shader description</returns>
        public static PixelShaderDescription LoadPixelShader(
            this Device device,
            string filename,
            string entryPoint)
        {
            string compilationErrors;
            return LoadPixelShader(
                device,
                File.ReadAllBytes(filename),
                entryPoint,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a pixel shader from file
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="filename">Path to file</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns pixel shader description</returns>
        public static PixelShaderDescription LoadPixelShader(
            this Device device,
            string filename,
            string entryPoint,
            out string compilationErrors)
        {
            return LoadPixelShader(
                device,
                File.ReadAllBytes(filename),
                entryPoint,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a pixel shader from byte code
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <returns>Returns pixel shader description</returns>
        public static PixelShaderDescription LoadPixelShader(
            this Device device,
            byte[] byteCode,
            string entryPoint)
        {
            string compilationErrors;
            return LoadPixelShader(
                device,
                byteCode,
                entryPoint,
                out compilationErrors);
        }
        /// <summary>
        /// Loads a pixel shader from byte code
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="byteCode">Byte code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="compilationErrors">Gets compilation errors if any</param>
        /// <returns>Returns pixel shader description</returns>
        public static PixelShaderDescription LoadPixelShader(
            this Device device,
            byte[] byteCode,
            string entryPoint,
            out string compilationErrors)
        {
            compilationErrors = null;

            using (ShaderIncludeManager includeManager = new ShaderIncludeManager())
            using (CompilationResult cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                HelperShaders.PSProfile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new PixelShaderDescription(new PixelShader(device, cmpResult.Bytecode));
            }
        }
        /// <summary>
        /// Loads an effect from byte code
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="bytes">Byte code</param>
        /// <returns>Returns loaded effect</returns>
        public static Effect CompileEffect(
            this Device device,
            byte[] bytes)
        {
            using (ShaderIncludeManager includeManager = new ShaderIncludeManager())
            using (CompilationResult cmpResult = ShaderBytecode.Compile(
                bytes,
                null,
                FXProfile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                return new Effect(
                    device,
                    cmpResult.Bytecode.Data,
                    EffectFlags.None);
            }
        }
        /// <summary>
        /// Loads an effect from pre-compiled file
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded effect</returns>
        public static Effect LoadEffect(
            this Device device,
            byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (ShaderBytecode effectCode = ShaderBytecode.FromStream(ms))
                {
                    return new Effect(
                        device,
                        effectCode.Data,
                        EffectFlags.None);
                }
            }
        }
    }
}
