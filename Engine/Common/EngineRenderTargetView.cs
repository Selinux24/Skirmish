using System;
using System.Collections.Generic;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    public class EngineRenderTargetView : IDisposable
    {
        internal List<RenderTargetView> RTV = new List<RenderTargetView>();

        internal int Count
        {
            get
            {
                return this.RTV.Count;
            }
        }

        internal EngineRenderTargetView()
        {

        }

        internal EngineRenderTargetView(Device device, Resource texture)
        {
            this.RTV.Add(new RenderTargetView(device, texture));
        }

        internal void Add(Device device, Resource texture)
        {
            this.RTV.Add(new RenderTargetView(device, texture));
        }

        public void Dispose()
        {
            Helper.Dispose(this.RTV);
        }
    }
}
