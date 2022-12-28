﻿using System;

namespace Engine.Common
{
    using Engine.Content;

    /// <summary>
    /// Mesh material
    /// </summary>
    public sealed class MeshMaterial : IMeshMaterial, IEquatable<MeshMaterial>
    {
        /// <summary>
        /// Default phong
        /// </summary>
        public static MeshMaterial DefaultPhong
        {
            get
            {
                return new MeshMaterial
                {
                    Material = MaterialPhong.Default,
                };
            }
        }
        /// <summary>
        /// Default blinn-phong
        /// </summary>
        public static MeshMaterial DefaultBlinnPhong
        {
            get
            {
                return new MeshMaterial
                {
                    Material = MaterialBlinnPhong.Default,
                };
            }
        }
        /// <summary>
        /// Default cook-torrance
        /// </summary>
        public static MeshMaterial DefaultCookTorrance
        {
            get
            {
                return new MeshMaterial
                {
                    Material = MaterialCookTorrance.Default,
                };
            }
        }
        /// <summary>
        /// Gets a phong material from a built-in definition
        /// </summary>
        /// <param name="builtInMaterial">Built-in material</param>
        /// <returns>Returns a phong material from a built-in definition</returns>
        public static MeshMaterial PhongFromBuiltIn(BuiltInMaterial builtInMaterial)
        {
            return new MeshMaterial
            {
                Material = MaterialPhong.FromBuiltIn(builtInMaterial),
            };
        }
        /// <summary>
        /// Gets a blinn-phong material from a built-in definition
        /// </summary>
        /// <param name="builtInMaterial">Built-in material</param>
        /// <returns>Returns a blinn-phong material from a built-in definition</returns>
        public static MeshMaterial BlinnPhongFromBuiltIn(BuiltInMaterial builtInMaterial)
        {
            return new MeshMaterial
            {
                Material = MaterialBlinnPhong.FromBuiltIn(builtInMaterial),
            };
        }
        /// <summary>
        /// Gets a cook-torrance material from a built-in definition
        /// </summary>
        /// <param name="builtInMaterial">Built-in material</param>
        /// <returns>Returns a cook-torrance material from a built-in definition</returns>
        public static MeshMaterial CookTorranceFromBuiltIn(BuiltInMaterial builtInMaterial)
        {
            return new MeshMaterial
            {
                Material = MaterialCookTorrance.FromBuiltIn(builtInMaterial),
            };
        }

        /// <summary>
        /// Material description
        /// </summary>
        public IMaterial Material { get; set; }
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
        /// Updates the data
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        public void UpdateResource(uint index, uint offset, uint size)
        {
            ResourceIndex = index;
            ResourceOffset = offset;
            ResourceSize = size;
        }

        /// <inheritdoc/>
        public bool Equals(MeshMaterial other)
        {
            return
                Material?.Equals(other.Material) == true &&
                EmissionTexture == other.EmissionTexture &&
                AmbientTexture == other.AmbientTexture &&
                DiffuseTexture == other.DiffuseTexture &&
                NormalMap == other.NormalMap;
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as MeshMaterial);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Material, EmissionTexture, AmbientTexture, DiffuseTexture, NormalMap);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Material} EmissionTexture: {EmissionTexture != null}; AmbientTexture: {AmbientTexture != null}; DiffuseTexture: {DiffuseTexture != null}; NormalMapTexture: {NormalMap != null};";
        }
    }
}
