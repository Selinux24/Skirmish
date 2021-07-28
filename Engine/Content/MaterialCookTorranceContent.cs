using SharpDX;
using System.Collections.Generic;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Material content
    /// </summary>
    public struct MaterialCookTorranceContent : IMaterialContent
    {
        /// <summary>
        /// Default material content
        /// </summary>
        public static MaterialCookTorranceContent Default
        {
            get
            {
                return new MaterialCookTorranceContent()
                {
                    AmbientColor = MaterialConstants.AmbientColor,
                    DiffuseColor = MaterialConstants.DiffuseColor,
                    SpecularColor = MaterialConstants.SpecularColor,
                    EmissiveColor = MaterialConstants.EmissiveColor,
                    Metallic = MaterialConstants.Metallic,
                    Roughness = MaterialConstants.Roughness,
                    IsTransparent = false,
                };
            }
        }

        /// <summary>
        /// Diffuse texture name
        /// </summary>
        public string DiffuseTexture { get; set; }
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor { get; set; }
        /// <summary>
        /// Emissive texture name
        /// </summary>
        public string EmissiveTexture { get; set; }
        /// <summary>
        /// Emissive color
        /// </summary>
        public Color3 EmissiveColor { get; set; }
        /// <summary>
        /// Ambient texture name
        /// </summary>
        public string AmbientTexture { get; set; }
        /// <summary>
        /// Ambient color
        /// </summary>
        public Color3 AmbientColor { get; set; }
        /// <summary>
        /// Specular texture name
        /// </summary>
        public string SpecularTexture { get; set; }
        /// <summary>
        /// Specular color
        /// </summary>
        public Color3 SpecularColor { get; set; }
        /// <summary>
        /// Normal map texture
        /// </summary>
        public string NormalMapTexture { get; set; }
        /// <summary>
        /// Use transparency
        /// </summary>
        public bool IsTransparent { get; set; }
        /// <summary>
        /// Metallic
        /// </summary>
        public float Metallic { get; set; }
        /// <summary>
        /// Roughness
        /// </summary>
        public float Roughness { get; set; }

        /// <inheritdoc/>
        public IMeshMaterial CreateMeshMaterial(Dictionary<string, EngineShaderResourceView> textures)
        {
            return new MeshMaterial
            {
                Material = new MaterialCookTorrance
                {
                    DiffuseColor = DiffuseColor,
                    EmissiveColor = EmissiveColor,
                    AmbientColor = AmbientColor,
                    SpecularColor = SpecularColor,
                    Metallic = Metallic,
                    Roughness = Roughness,
                    IsTransparent = IsTransparent,
                },
                EmissionTexture = string.IsNullOrEmpty(EmissiveTexture) ? null : textures[EmissiveTexture],
                AmbientTexture = string.IsNullOrEmpty(AmbientTexture) ? null : textures[AmbientTexture],
                DiffuseTexture = string.IsNullOrEmpty(DiffuseTexture) ? null : textures[DiffuseTexture],
                NormalMap = string.IsNullOrEmpty(NormalMapTexture) ? null : textures[NormalMapTexture],
            };
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            var emissive = EmissiveTexture ?? $"{EmissiveColor}";
            var ambient = AmbientTexture ?? $"{AmbientColor}";
            var diffuse = DiffuseTexture ?? $"{DiffuseColor}";
            var specular = SpecularTexture ?? $"{SpecularColor}";

            return $"Cook-Torrance. Emissive: {emissive}; Ambient: {ambient}; Diffuse: {diffuse}; Specular: {specular}; Metallic: {Metallic}; Roughness: {Roughness};";
        }
    }
}
