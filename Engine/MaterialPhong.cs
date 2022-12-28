using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Material description
    /// </summary>
    public struct MaterialPhong : IEquatable<MaterialPhong>, IMaterial
    {
        /// <inheritdoc/>
        public static bool operator ==(MaterialPhong left, MaterialPhong right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(MaterialPhong left, MaterialPhong right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Default material
        /// </summary>
        public static MaterialPhong Default
        {
            get
            {
                return new MaterialPhong()
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
        public static MaterialPhong FromBuiltIn(BuiltInMaterial builtInMaterial)
        {
            MaterialPhongContent content = builtInMaterial;

            return new MaterialPhong()
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
        public BufferMaterials Convert()
        {
            return new BufferMaterials
            {
                Algorithm = (uint)SpecularAlgorithms.Phong,
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
        public bool Equals(MaterialPhong other)
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
        public override bool Equals(object obj)
        {
            return obj is MaterialPhong phong && Equals(phong);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(EmissiveColor, AmbientColor, DiffuseColor, SpecularColor, IsTransparent, Shininess);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Phong. EmissiveColor: {EmissiveColor}; AmbientColor: {AmbientColor}; DiffuseColor: {DiffuseColor}; SpecularColor: {SpecularColor}; Shininess: {Shininess};";
        }
    };
}
