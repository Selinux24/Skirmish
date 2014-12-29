using System;
using System.IO;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace Engine.Helpers
{
    using Engine.Properties;

    public static class HelperShaders
    {
        #region Classes

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

        public class ShaderIncludeManager : CallbackBase, Include
        {
            public void Close(Stream stream)
            {
                stream.Close();
                stream.Dispose();
            }

            public Stream Open(IncludeType type, string fileName, Stream parentStream)
            {
                byte[] o = (byte[])Resources.ResourceManager.GetObject(Path.GetFileNameWithoutExtension(fileName));

                return new MemoryStream(o);
            }
        }

        #endregion

        public static string VSProfile = "vs_5_0";
        public static string PSProfile = "ps_5_0";
        public static string FXProfile = "fx_5_0";

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

        public static VertexShaderDescription LoadVertexShader(
            this Device device,
            byte[] byteCode,
            string entryPoint,
            InputElement[] input,
            out string compilationErrors)
        {
            compilationErrors = null;

            using (CompilationResult cmpResult = LoadVertexShader(device, byteCode, entryPoint, VSProfile))
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

        private static CompilationResult LoadVertexShader(
            this Device device,
            byte[] byteCode,
            string entryPoint,
            string profile)
        {
            return ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                profile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                new ShaderIncludeManager());
        }

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

        public static PixelShaderDescription LoadPixelShader(
            this Device device,
            byte[] byteCode,
            string entryPoint,
            out string compilationErrors)
        {
            compilationErrors = null;

            using (CompilationResult cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                HelperShaders.PSProfile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                new ShaderIncludeManager()))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new PixelShaderDescription(new PixelShader(device, cmpResult.Bytecode));
            }
        }

        public static Effect LoadEffect(
            this Device device,
            byte[] byteCode)
        {
            using (CompilationResult cmpResult = HelperShaders.LoadVertexShader(
                device,
                byteCode,
                null,
                HelperShaders.FXProfile))
            {
                return new Effect(
                    device,
                    cmpResult.Bytecode.Data,
                    SharpDX.D3DCompiler.EffectFlags.None);
            }
        }
    }
}
