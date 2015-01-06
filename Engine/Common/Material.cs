using System.Runtime.InteropServices;
using SharpDX;

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
        /// Ambient color
        /// </summary>
        public Color4 AmbientColor;
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor;
        /// <summary>
        /// Emission color
        /// </summary>
        public Color4 EmissionColor;
        /// <summary>
        /// Index of refraction
        /// </summary>
        public float IndexOfRefraction;
        /// <summary>
        /// Reflective color
        /// </summary>
        public Color4 ReflectiveColor;
        /// <summary>
        /// Reflectivity factor
        /// </summary>
        public float Reflectivity;
        /// <summary>
        /// Shininess factor
        /// </summary>
        public float Shininess;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 SpecularColor;
        /// <summary>
        /// Transparency factor
        /// </summary>
        public float Transparency;
        /// <summary>
        /// Transparent color
        /// </summary>
        public Color4 Transparent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="effect">Effect content</param>
        public Material(MaterialContent effect)
        {
            this.AmbientColor = effect.AmbientColor;
            this.DiffuseColor = effect.DiffuseColor;
            this.EmissionColor = effect.EmissionColor;
            this.IndexOfRefraction = effect.IndexOfRefraction;
            this.ReflectiveColor = effect.ReflectiveColor;
            this.Reflectivity = effect.Reflectivity;
            this.Shininess = effect.Shininess;
            this.SpecularColor = effect.SpecularColor;
            this.Transparency = effect.Transparency;
            this.Transparent = effect.Transparent;
        }
    };
}
