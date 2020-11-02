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
        /// Constructor
        /// </summary>
        /// <param name="material">Material</param>
        public BufferMaterials(Material material)
        {
            Emissive = material.EmissiveColor;
            Ambient = material.AmbientColor;
            Diffuse = material.DiffuseColor;
            Specular = material.SpecularColor;
            Shininess = material.Shininess;

            Pad1 = 1000;
            Pad2 = 2000;
            Pad3 = 3000;
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
#if DEBUG
            int size = Marshal.SizeOf(typeof(BufferMaterials));
            if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
            return size;
#else
                return Marshal.SizeOf(typeof(BufferMaterials));
#endif
        }
    };
}
