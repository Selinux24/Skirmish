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
        /// Constructor
        /// </summary>
        /// <param name="effect">Effect content</param>
        public Material(MaterialContent effect)
        {
            this.EmissiveColor = effect.EmissionColor;
            this.AmbientColor = effect.AmbientColor;
            this.DiffuseColor = effect.DiffuseColor;
            this.SpecularColor = effect.SpecularColor;
            this.Shininess = effect.Shininess;
        }

        /// <summary>
        /// Packs current instance into a Vector4 array
        /// </summary>
        /// <returns>Returns the packed material</returns>
        internal Vector4[] Pack()
        {
            Vector4[] res = new Vector4[4];

            res[0] = this.EmissiveColor;
            res[1] = this.AmbientColor;
            res[2] = this.DiffuseColor;
            res[3] = this.SpecularColor;
            res[3].W = this.Shininess;

            return res;
        }

        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(Material other)
        {
            return
                this.EmissiveColor == other.EmissiveColor &&
                this.AmbientColor == other.AmbientColor &&
                this.DiffuseColor == other.DiffuseColor &&
                this.SpecularColor == other.SpecularColor &&
                this.Shininess == other.Shininess;
        }
    };
}
