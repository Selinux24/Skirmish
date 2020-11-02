using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    using Engine.Content;

    /// <summary>
    /// Material description
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Material : IEquatable<Material>
    {
        /// <summary>
        /// Default material
        /// </summary>
        public static readonly Material Default = new Material(MaterialContent.Default);

        /// <summary>
        /// Algorithm
        /// </summary>
        public SpecularAlgorithms Algorithm;
        /// <summary>
        /// Emissive color
        /// </summary>
        public Color4 EmissiveColor;
        /// <summary>
        /// Ambient color
        /// </summary>
        public Color4 AmbientColor;
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 SpecularColor;
        /// <summary>
        /// Shininess
        /// </summary>
        public float Shininess;
        /// <summary>
        /// Transparent
        /// </summary>
        public bool IsTransparent;
        /// <summary>
        /// Cook-Torrance Roughness mode
        /// </summary>
        public SpecularCookTorranceModes RoughnessMode;
        /// <summary>
        /// Cook-Torrance Roughness value
        /// </summary>
        public float RoughnessValue;
        /// <summary>
        /// Cook-Torrance Reflection at norm incidence
        /// </summary>
        public float ReflectionAtNormIncidence;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="effect">Effect content</param>
        public Material(MaterialContent effect)
        {
            Algorithm = effect.Algorithm;
            EmissiveColor = effect.EmissionColor;
            AmbientColor = effect.AmbientColor;
            DiffuseColor = effect.DiffuseColor;
            SpecularColor = effect.SpecularColor;
            Shininess = effect.Shininess;
            IsTransparent = effect.IsTransparent;
            RoughnessMode = effect.RoughnessMode;
            RoughnessValue = effect.Reflectivity;
            ReflectionAtNormIncidence = effect.IndexOfRefraction;
        }

        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(Material other)
        {
            return
                Algorithm == other.Algorithm &&
                EmissiveColor == other.EmissiveColor &&
                AmbientColor == other.AmbientColor &&
                DiffuseColor == other.DiffuseColor &&
                SpecularColor == other.SpecularColor &&
                Shininess == other.Shininess &&
                IsTransparent == other.IsTransparent &&
                RoughnessMode == other.RoughnessMode &&
                RoughnessValue == other.RoughnessValue &&
                ReflectionAtNormIncidence == other.ReflectionAtNormIncidence;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Algorithm} EmissiveColor: {EmissiveColor}; AmbientColor: {AmbientColor}; DiffuseColor: {DiffuseColor}; SpecularColor: {SpecularColor}; Shininess: {Shininess};";
        }
    };
}
