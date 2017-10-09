using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    public class EngineDepthStencilView : IDisposable
    {
        internal DepthStencilView DSV { get; set; }

        public EngineDepthStencilView(Device device, Resource texture, DepthStencilViewDescription description)
        {
            this.DSV = new DepthStencilView(device, texture, description);
        }

        public void Dispose()
        {
            if (this.DSV != null)
            {
                this.DSV.Dispose();
                this.DSV = null;
            }
        }
    }
}
