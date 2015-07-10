using System;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Common
{
    /// <summary>
    /// Mesh material
    /// </summary>
    public class MeshMaterial : IDisposable
    {
        /// <summary>
        /// Material description
        /// </summary>
        public Material Material { get; set; }
        /// <summary>
        /// Emission texture
        /// </summary>
        public ShaderResourceView EmissionTexture { get; set; }
        /// <summary>
        /// Ambient texture
        /// </summary>
        public ShaderResourceView AmbientTexture { get; set; }
        /// <summary>
        /// Diffuse texture
        /// </summary>
        public ShaderResourceView DiffuseTexture { get; set; }
        /// <summary>
        /// Specular texture
        /// </summary>
        public ShaderResourceView SpecularTexture { get; set; }
        /// <summary>
        /// Reflective texture
        /// </summary>
        public ShaderResourceView ReflectiveTexture { get; set; }
        /// <summary>
        /// Normal map
        /// </summary>
        public ShaderResourceView NormalMap { get; set; }

        /// <summary>
        /// Resource disposing
        /// </summary>
        public void Dispose()
        {

        }
    }
}
