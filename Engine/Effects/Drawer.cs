using System;
using SharpDX.Direct3D11;

namespace Engine.Effects
{
    using Engine.Common;

    public interface Drawer : IDisposable
    {
        EffectTechnique GetTechnique(string technique);
        string AddInputLayout(VertexTypes vertexType);
        InputLayout GetInputLayout(string technique);
    }
}
