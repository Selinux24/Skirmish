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
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor;
        /// <summary>
        /// Specular intensity
        /// </summary>
        public float SpecularIntensity;
        /// <summary>
        /// Specular power
        /// </summary>
        public float SpecularPower;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="effect">Effect content</param>
        public Material(MaterialContent effect)
        {
            this.DiffuseColor = effect.DiffuseColor;
            this.SpecularIntensity = 0;
            this.SpecularPower = 0;
        }
    };
}
