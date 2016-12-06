using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    using Engine.Content;

    /// <summary>
    /// Material description
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Material
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
    };
}
