using SharpDX.Direct3D11;

namespace Common.Utils
{
    public class PixelShaderInfo : System.IDisposable
    {
        public PixelShader Shader { get; private set; }

        internal PixelShaderInfo(PixelShader shader)
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
}
