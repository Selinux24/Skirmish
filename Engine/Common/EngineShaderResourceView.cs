using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    public class EngineShaderResourceView : IDisposable
    {
        internal ShaderResourceView SRV { get; set; }

        internal EngineShaderResourceView(ShaderResourceView srv)
        {
            this.SRV = srv;
        }

        public EngineShaderResourceView(Device device, Resource texture)
        {
            this.SRV = new ShaderResourceView(device, texture);
        }

        public EngineShaderResourceView(Device device, Resource texture, ShaderResourceViewDescription description)
        {
            this.SRV = new ShaderResourceView(device, texture, description);
        }

        public void Dispose()
        {
            if (this.SRV != null)
            {
                this.SRV.Dispose();
                this.SRV = null;
            }
        }
    }
}
