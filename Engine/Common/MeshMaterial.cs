using System;

namespace Engine.Common
{
    /// <summary>
    /// Mesh material
    /// </summary>
    public sealed class MeshMaterial : IMeshMaterial, IEquatable<MeshMaterial>
    {
        /// <summary>
        /// Create mesh materia from material
        /// </summary>
        /// <param name="material">Material</param>
        public static MeshMaterial FromMaterial(IMaterial material)
        {
            return new MeshMaterial
            {
                Material = material,
            };
        }

        /// <inheritdoc/>
        public IMaterial Material { get; set; }
        /// <inheritdoc/>
        public EngineShaderResourceView EmissionTexture { get; set; }
        /// <inheritdoc/>
        public EngineShaderResourceView AmbientTexture { get; set; }
        /// <inheritdoc/>
        public EngineShaderResourceView DiffuseTexture { get; set; }
        /// <inheritdoc/>
        public EngineShaderResourceView NormalMap { get; set; }

        /// <inheritdoc/>
        public uint ResourceIndex { get; set; } = 0;
        /// <inheritdoc/>
        public uint ResourceOffset { get; set; } = 0;
        /// <inheritdoc/>
        public uint ResourceSize { get; set; } = 0;

        /// <inheritdoc/>
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
