using SharpDX;

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
                    DiffuseColor = MaterialConstants.DiffuseColor,
                    EmissiveColor = MaterialConstants.EmissiveColor,
                    AmbientColor = MaterialConstants.AmbientColor,
                    SpecularColor = MaterialConstants.SpecularColor,
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
        public IMeshMaterial CreateMeshMaterial(TextureDictionary textures)
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
                EmissionTexture = textures[EmissiveTexture],
                AmbientTexture = textures[AmbientTexture],
                DiffuseTexture = textures[DiffuseTexture],
                NormalMap = textures[NormalMapTexture],
            };
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return "Cook-Torrance;";
        }
    }
}
