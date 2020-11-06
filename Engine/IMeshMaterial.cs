
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Mesh material interface
    /// </summary>
    public interface IMeshMaterial
    {
        /// <summary>
        /// Material
        /// </summary>
        IMaterial Material { get; set; }
        /// <summary>
        /// Emission texture
        /// </summary>
        EngineShaderResourceView EmissionTexture { get; set; }
        /// <summary>
        /// Ambient texture
        /// </summary>
        EngineShaderResourceView AmbientTexture { get; set; }
        /// <summary>
        /// Diffuse texture
        /// </summary>
        EngineShaderResourceView DiffuseTexture { get; set; }
        /// <summary>
        /// Specular texture
        /// </summary>
        EngineShaderResourceView SpecularTexture { get; set; }
        /// <summary>
        /// Normal map
        /// </summary>
        EngineShaderResourceView NormalMap { get; set; }

        /// <summary>
        /// Resource index
        /// </summary>
        uint ResourceIndex { get; set; }
        /// <summary>
        /// Resource offset
        /// </summary>
        uint ResourceOffset { get; set; }
        /// <summary>
        /// Resource size
        /// </summary>
        uint ResourceSize { get; set; }
    }
}
