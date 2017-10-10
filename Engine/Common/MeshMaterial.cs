using SharpDX;
using System;

namespace Engine.Common
{
    /// <summary>
    /// Mesh material
    /// </summary>
    public class MeshMaterial : IDisposable, IEquatable<MeshMaterial>
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
        public EngineTexture EmissionTexture { get; set; }
        /// <summary>
        /// Ambient texture
        /// </summary>
        public EngineTexture AmbientTexture { get; set; }
        /// <summary>
        /// Diffuse texture
        /// </summary>
        public EngineTexture DiffuseTexture { get; set; }
        /// <summary>
        /// Specular texture
        /// </summary>
        public EngineTexture SpecularTexture { get; set; }
        /// <summary>
        /// Reflective texture
        /// </summary>
        public EngineTexture ReflectiveTexture { get; set; }
        /// <summary>
        /// Normal map
        /// </summary>
        public EngineTexture NormalMap { get; set; }

        /// <summary>
        /// Resource index
        /// </summary>
        public uint ResourceIndex = 0;
        /// <summary>
        /// Resource offset
        /// </summary>
        public uint ResourceOffset = 0;
        /// <summary>
        /// Resource size
        /// </summary>
        public uint ResourceSize = 0;

        /// <summary>
        /// Resource disposing
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Packs current instance into a Vector4 array
        /// </summary>
        /// <returns>Returns the packed material</returns>
        internal Vector4[] Pack()
        {
            Vector4[] res = new Vector4[4];

            res[0] = this.Material.EmissiveColor;
            res[1] = this.Material.AmbientColor;
            res[2] = this.Material.DiffuseColor;
            res[3] = this.Material.SpecularColor;
            res[3].W = this.Material.Shininess;

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
                this.Material.Equals(other.Material) &&
                this.EmissionTexture == other.EmissionTexture &&
                this.AmbientTexture == other.AmbientTexture &&
                this.DiffuseTexture == other.DiffuseTexture &&
                this.SpecularTexture == other.SpecularTexture &&
                this.ReflectiveTexture == other.ReflectiveTexture &&
                this.NormalMap == other.NormalMap;
        }
        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("{0} EmissionTexture: {1}; AmbientTexture: {2}; DiffuseTexture: {3}; SpecularTexture: {4}; ReflectiveTexture: {5}; NormalMapTexture: {6};",
                this.Material,
                this.EmissionTexture != null,
                this.AmbientTexture != null,
                this.DiffuseTexture != null,
                this.SpecularTexture != null,
                this.ReflectiveTexture != null,
                this.NormalMap != null);
        }
    }
}
