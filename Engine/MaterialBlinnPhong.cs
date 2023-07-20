using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Material description
    /// </summary>
    public struct MaterialBlinnPhong : IEquatable<MaterialBlinnPhong>, IMaterial
    {
        /// <inheritdoc/>
        public static bool operator ==(MaterialBlinnPhong left, MaterialBlinnPhong right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(MaterialBlinnPhong left, MaterialBlinnPhong right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Default material
        /// </summary>
        public static MaterialBlinnPhong Default
        {
            get
            {
                return new MaterialBlinnPhong()
                {
                    DiffuseColor = MaterialConstants.DiffuseColor,
                    EmissiveColor = MaterialConstants.EmissiveColor,
                    AmbientColor = MaterialConstants.AmbientColor,
                    SpecularColor = MaterialConstants.SpecularColor,
                    Shininess = MaterialConstants.Shininess,
                    IsTransparent = false,
                };
            }
        }
        /// <summary>
        /// Gets a material from a built-in definition
        /// </summary>
        /// <param name="builtInMaterial">Built-in material</param>
        /// <returns>Returns a material from a built-in definition</returns>
        public static MaterialBlinnPhong FromBuiltIn(BuiltInMaterial builtInMaterial)
        {
            MaterialBlinnPhongContent content = builtInMaterial;

            return new MaterialBlinnPhong()
            {
                DiffuseColor = content.DiffuseColor,
                EmissiveColor = content.EmissiveColor,
                AmbientColor = content.AmbientColor,
                SpecularColor = content.SpecularColor,
                Shininess = content.Shininess,
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
        /// Use transparency
        /// </summary>
        public bool IsTransparent { get; set; }
        /// <summary>
        /// Shininess
        /// </summary>
        public float Shininess { get; set; }

        /// <inheritdoc/>
        public readonly BufferMaterials Convert()
        {
            return new BufferMaterials
            {
                Algorithm = (uint)SpecularAlgorithms.BlinnPhong,
                Block11 = 0,
                Block12 = 0,
                Block13 = 0,
                Block2 = Vector4.Zero,
                Block3 = Vector4.Zero,
                Block4 = Vector4.Zero,
                Diffuse = DiffuseColor,
                Emissive = new Color4(EmissiveColor, 0),
                Ambient = new Color4(AmbientColor, 0),
                Specular = new Color4(SpecularColor, Shininess),
            };
        }
        /// <inheritdoc/>
        public readonly bool Equals(MaterialBlinnPhong other)
        {
            return
                EmissiveColor == other.EmissiveColor &&
                AmbientColor == other.AmbientColor &&
                DiffuseColor == other.DiffuseColor &&
                SpecularColor == other.SpecularColor &&
                IsTransparent == other.IsTransparent &&
                Shininess == other.Shininess;
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            return obj is MaterialBlinnPhong phong && Equals(phong);
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(EmissiveColor, AmbientColor, DiffuseColor, SpecularColor, IsTransparent, Shininess);
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return $"Blinn-Phong. EmissiveColor: {EmissiveColor}; AmbientColor: {AmbientColor}; DiffuseColor: {DiffuseColor}; SpecularColor: {SpecularColor}; Shininess: {Shininess};";
        }
    };
}
