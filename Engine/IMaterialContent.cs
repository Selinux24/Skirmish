using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Material content interface
    /// </summary>
    public interface IMaterialContent
    {
        /// <summary>
        /// Diffuse texture name
        /// </summary>
        string DiffuseTexture { get; set; }
        /// <summary>
        /// Emissive texture name
        /// </summary>
        string EmissiveTexture { get; set; }
        /// <summary>
        /// Ambient texture name
        /// </summary>
        string AmbientTexture { get; set; }
        /// <summary>
        /// Specular texture name
        /// </summary>
        string SpecularTexture { get; set; }
        /// <summary>
        /// Normal map texture
        /// </summary>
        string NormalMapTexture { get; set; }

        /// <summary>
        /// Gets whether the material is textured or not <see cref="DiffuseTexture"/>
        /// </summary>
        bool Textured { get => !string.IsNullOrWhiteSpace(DiffuseTexture); }

        /// <summary>
        /// Creates a mesh material from material content
        /// </summary>
        /// <param name="textures">Texture dictionary</param>
        /// <returns>Returns a new mesh material</returns>
        IMeshMaterial CreateMeshMaterial(IDictionary<string, EngineShaderResourceView> textures);
    }
}
