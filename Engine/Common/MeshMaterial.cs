using System;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Common
{
    public class MeshMaterial : IDisposable
    {
        public Material Material { get; set; }
        public ShaderResourceView EmissionTexture { get; set; }
        public ShaderResourceView AmbientTexture { get; set; }
        public ShaderResourceView DiffuseTexture { get; set; }
        public ShaderResourceView SpecularTexture { get; set; }
        public ShaderResourceView ReflectiveTexture { get; set; }

        public void Dispose()
        {

        }
    }
}
