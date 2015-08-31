using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Material buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferMaterials : IBufferData
    {
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 Diffuse;
        /// <summary>
        /// Specular intensity
        /// </summary>
        public float SpecularIntensity;
        /// <summary>
        /// Specular power
        /// </summary>
        public float SpecularPower;
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferMaterials));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="material">Material</param>
        public BufferMaterials(Material material)
        {
            this.Diffuse = material.DiffuseColor;
            this.SpecularIntensity = material.SpecularIntensity;
            this.SpecularPower = material.SpecularPower;
        }

        /// <summary>
        /// Sets material data
        /// </summary>
        /// <param name="material">Material</param>
        public void SetMaterial(Material material)
        {
            this.Diffuse = material.DiffuseColor;
            this.SpecularIntensity = material.SpecularIntensity;
            this.SpecularPower = material.SpecularPower;
        }
    };
}
