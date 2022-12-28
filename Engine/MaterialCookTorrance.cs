using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Material description
    /// </summary>
    public struct MaterialCookTorrance : IEquatable<MaterialCookTorrance>, IMaterial
    {
        /// <inheritdoc/>
        public static bool operator ==(MaterialCookTorrance left, MaterialCookTorrance right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(MaterialCookTorrance left, MaterialCookTorrance right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Default material
        /// </summary>
        public static MaterialCookTorrance Default
        {
            get
            {
                return new MaterialCookTorrance()
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
        /// Gets a material from a built-in definition
        /// </summary>
        /// <param name="builtInMaterial">Built-in material</param>
        /// <returns>Returns a material from a built-in definition</returns>
        public static MaterialCookTorrance FromBuiltIn(BuiltInMaterial builtInMaterial)
        {
            MaterialCookTorranceContent content = builtInMaterial;

            return new MaterialCookTorrance()
            {
                DiffuseColor = content.DiffuseColor,
                EmissiveColor = content.EmissiveColor,
                AmbientColor = content.AmbientColor,
                SpecularColor = content.SpecularColor,
                Metallic = content.Metallic,
                Roughness = content.Roughness,
                IsTransparent = false,
            };
        }

        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor { get; set; }
        /// <summary>
        /// Emissive color
        /// </summary>
        public Color3 EmissiveColor { get; set; }
        /// <summary>
        /// Ambient color
        /// </summary>
        public Color3 AmbientColor { get; set; }
        /// <summary>
        /// Specular color
        /// </summary>
        public Color3 SpecularColor { get; set; }
        /// <summary>
        /// Metallic
        /// </summary>
        public float Metallic { get; set; }
        /// <summary>
        /// Roughness
        /// </summary>
        public float Roughness { get; set; }
        /// <summary>
        /// Use transparency
        /// </summary>
        public bool IsTransparent { get; set; }

        /// <inheritdoc/>
        public BufferMaterials Convert()
        {
            return new BufferMaterials
            {
                Algorithm = (uint)SpecularAlgorithms.CookTorrance,
                Block11 = 0,
                Block12 = 0,
                Block13 = 0,
                Block2 = new Vector4(Metallic, Roughness, 0f, 0f),
                Block3 = Vector4.Zero,
                Block4 = Vector4.Zero,
                Diffuse = DiffuseColor,
                Emissive = new Color4(EmissiveColor, 0),
                Ambient = new Color4(AmbientColor, 0),
                Specular = new Color4(SpecularColor, 0),
            };
        }
        /// <inheritdoc/>
        public bool Equals(MaterialCookTorrance other)
        {
            return
                EmissiveColor == other.EmissiveColor &&
                AmbientColor == other.AmbientColor &&
                DiffuseColor == other.DiffuseColor &&
                SpecularColor == other.SpecularColor &&
                IsTransparent == other.IsTransparent &&
                Metallic == other.Metallic &&
                Roughness == other.Roughness;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is MaterialCookTorrance torrance && Equals(torrance);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(EmissiveColor, AmbientColor, DiffuseColor, SpecularColor, IsTransparent, Metallic, Roughness);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Cook-Torrance. EmissiveColor: {EmissiveColor}; AmbientColor: {AmbientColor}; DiffuseColor: {DiffuseColor}; SpecularColor: {SpecularColor}; Metallic: {Metallic}; Roughness: {Roughness};";
        }
    };
}
