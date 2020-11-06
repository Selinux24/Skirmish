using SharpDX;
using System;

namespace Engine
{
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Material description
    /// </summary>
    public struct MaterialCookTorrance : IEquatable<MaterialCookTorrance>, IMaterial
    {
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
                    F0 = MaterialConstants.F0,
                    Roughness = MaterialConstants.Roughness,
                    K = MaterialConstants.K,
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
        /// F0
        /// </summary>
        public float F0 { get; set; }
        /// <summary>
        /// Roughness
        /// </summary>
        public float Roughness { get; set; }
        /// <summary>
        /// K
        /// </summary>
        public float K { get; set; }
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
                Block2 = new Vector4(F0, Roughness, K, 0f),
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
                F0 == other.F0 &&
                Roughness == other.Roughness &&
                K == other.K;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Cook-Torrance. EmissiveColor: {EmissiveColor}; AmbientColor: {AmbientColor}; DiffuseColor: {DiffuseColor}; SpecularColor: {SpecularColor}; F0: {F0}; Roughness: {Roughness}; K: {K};";
        }
    };
}
