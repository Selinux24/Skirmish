using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace Common.Utils
{
    public static class ShaderUtils
    {
        public static string VSProfile = "vs_5_0";
        public static string PSProfile = "ps_5_0";
        public static string FXProfile = "fx_5_0";

        public static VertexShaderInfo LoadVertexShader(
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

        public static VertexShaderInfo LoadVertexShader(
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

        public static VertexShaderInfo LoadVertexShader(
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

        public static VertexShaderInfo LoadVertexShader(
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

                return new VertexShaderInfo(vertexShader, layout);
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

        public static PixelShaderInfo LoadPixelShader(
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

        public static PixelShaderInfo LoadPixelShader(
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

        public static PixelShaderInfo LoadPixelShader(
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

        public static PixelShaderInfo LoadPixelShader(
            this Device device,
            byte[] byteCode,
            string entryPoint,
            out string compilationErrors)
        {
            compilationErrors = null;

            using (CompilationResult cmpResult = ShaderBytecode.Compile(
                byteCode,
                entryPoint,
                ShaderUtils.PSProfile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                new ShaderIncludeManager()))
            {
                if (cmpResult.HasErrors)
                {
                    compilationErrors = cmpResult.Message;
                }

                return new PixelShaderInfo(new PixelShader(device, cmpResult.Bytecode));
            }
        }

        public static EffectInfo LoadEffect(
            this Device device,
            byte[] byteCode,
            string technique,
            InputElement[] input)
        {
            using (CompilationResult cmpResult = ShaderUtils.LoadVertexShader(
                device,
                byteCode,
                null,
                ShaderUtils.FXProfile))
            {
                Effect effect = new Effect(
                    device,
                    cmpResult.Bytecode.Data,
                    SharpDX.D3DCompiler.EffectFlags.None);

                EffectPassDescription desc = effect.GetTechniqueByName(technique).GetPassByIndex(0).Description;

                InputLayout layout = new InputLayout(
                    device,
                    desc.Signature,
                    input);

                return new EffectInfo(effect, layout);
            }
        }

        public static Buffer CreateConstantBuffer<T>(this Device device) where T : struct
        {
            int size = ((Marshal.SizeOf(typeof(T)) + 15) / 16) * 16;

            BufferDescription description = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = size,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            return new Buffer(device, description);
        }

        public static void WriteConstantBuffer<T>(this DeviceContext deviceContext,
            Buffer constantBuffer,
            T value,
            long offset) where T : struct
        {
            DataStream stream;
            deviceContext.MapSubresource(constantBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);
            using (stream)
            {
                stream.Position = offset;
                stream.Write(value);
            }
            deviceContext.UnmapSubresource(constantBuffer, 0);
        }
    }
}
