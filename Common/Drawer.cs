using System;
using SharpDX.Direct3D11;

namespace Common
{
    using Common.Utils;

    public interface Drawer : IDisposable
    {
        void UpdatePerFrame(BufferLights lBuffer);
        void UpdatePerObject(BufferMatrix mBuffer, ShaderResourceView texture);
    }
}
