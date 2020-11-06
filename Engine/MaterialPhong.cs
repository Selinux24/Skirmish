using SharpDX;
using System;

namespace Engine
{
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Material description
    /// </summary>
    public struct MaterialPhong : IEquatable<MaterialPhong>, IMaterial
    {
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
        public override string ToString()
        {
            return $"Phong. EmissiveColor: {EmissiveColor}; AmbientColor: {AmbientColor}; DiffuseColor: {DiffuseColor}; SpecularColor: {SpecularColor}; Shininess: {Shininess};";
        }
    };
}
