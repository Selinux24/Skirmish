using System;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using VertexShader = SharpDX.Direct3D11.VertexShader;

namespace Engine.Helpers
{
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
}
