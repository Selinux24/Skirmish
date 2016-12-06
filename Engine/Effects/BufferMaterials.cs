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
        /// Emissive color
        /// </summary>
        public Color4 Emissive;
        /// <summary>
        /// Ambient color
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
        /// Shininess
        /// </summary>
        public float Shininess;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad1;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad2;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad3;

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
#if DEBUG
                int size = Marshal.SizeOf(typeof(BufferMaterials));
                if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
                return size;
#else
                return Marshal.SizeOf(typeof(BufferMaterials));
#endif
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="material">Material</param>
        public BufferMaterials(Material material)
        {
            this.Emissive = material.EmissiveColor;
            this.Ambient = material.AmbientColor;
            this.Diffuse = material.DiffuseColor;
            this.Specular = material.SpecularColor;
            this.Shininess = material.Shininess;

            this.Pad1 = 1000;
            this.Pad2 = 2000;
            this.Pad3 = 3000;
        }
    };
}
