using SharpDX.Direct3D11;

namespace Engine.Helpers
{
    public class PixelShaderDescription : System.IDisposable
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
}
