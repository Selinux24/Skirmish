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
        /// Ambien color
        /// </summary>
        public Color4 Ambient;
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 Diffuse;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 Specular;
        /// <summary>
        /// Reflect color
        /// </summary>
        public Color4 Reflect;
        /// <summary>
        /// Padding
        /// </summary>
        public float Padding;
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
        /// Sets material data
        /// </summary>
        /// <param name="material">Material</param>
        public void SetMaterial(Material material)
        {
            this.Ambient = material.AmbientColor;
            this.Diffuse = material.DiffuseColor;
            this.Specular = material.SpecularColor;
            this.Specular.Alpha = material.Shininess;
            this.Reflect = material.ReflectiveColor;
            this.Padding = 1000f;
        }
    };
}
