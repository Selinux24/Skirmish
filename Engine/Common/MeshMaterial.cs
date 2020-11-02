using SharpDX;
using System;

namespace Engine.Common
{
    /// <summary>
    /// Mesh material
    /// </summary>
    public sealed class MeshMaterial : IEquatable<MeshMaterial>
    {
        /// <summary>
        /// Default material
        /// </summary>
        public static MeshMaterial Default
        {
            get
            {
                return new MeshMaterial()
                {
                    Material = Material.Default
                };
            }
        }

        /// <summary>
        /// Material description
        /// </summary>
        public Material Material { get; set; }
        /// <summary>
        /// Emission texture
        /// </summary>
        public EngineShaderResourceView EmissionTexture { get; set; }
        /// <summary>
        /// Ambient texture
        /// </summary>
        public EngineShaderResourceView AmbientTexture { get; set; }
        /// <summary>
        /// Diffuse texture
        /// </summary>
        public EngineShaderResourceView DiffuseTexture { get; set; }
        /// <summary>
        /// Specular texture
        /// </summary>
        public EngineShaderResourceView SpecularTexture { get; set; }
        /// <summary>
        /// Reflective texture
        /// </summary>
        public EngineShaderResourceView ReflectiveTexture { get; set; }
        /// <summary>
        /// Normal map
        /// </summary>
        public EngineShaderResourceView NormalMap { get; set; }

        /// <summary>
        /// Resource index
        /// </summary>
        public uint ResourceIndex { get; set; } = 0;
        /// <summary>
        /// Resource offset
        /// </summary>
        public uint ResourceOffset { get; set; } = 0;
        /// <summary>
        /// Resource size
        /// </summary>
        public uint ResourceSize { get; set; } = 0;

        /// <summary>
        /// Packs current instance into a Vector4 array
        /// </summary>
        /// <returns>Returns the packed material</returns>
        internal Vector4[] Pack()
        {
            Vector4[] res = new Vector4[5];

            res[0] = Material.EmissiveColor;
            res[1] = Material.AmbientColor;
            res[2] = Material.DiffuseColor;
            res[3] = Material.SpecularColor;
            res[3].W = Material.Shininess;
            res[4] = new Vector4((uint)Material.Algorithm, (uint)Material.RoughnessMode, Material.RoughnessValue, Material.ReflectionAtNormIncidence);

            return res;
        }

        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(MeshMaterial other)
        {
            return
                Material.Equals(other.Material) &&
                EmissionTexture == other.EmissionTexture &&
                AmbientTexture == other.AmbientTexture &&
                DiffuseTexture == other.DiffuseTexture &&
                SpecularTexture == other.SpecularTexture &&
                ReflectiveTexture == other.ReflectiveTexture &&
                NormalMap == other.NormalMap;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Material} EmissionTexture: {EmissionTexture != null}; AmbientTexture: {AmbientTexture != null}; DiffuseTexture: {DiffuseTexture != null}; SpecularTexture: {SpecularTexture != null}; ReflectiveTexture: {ReflectiveTexture != null}; NormalMapTexture: {NormalMap != null};";
        }
    }
}
