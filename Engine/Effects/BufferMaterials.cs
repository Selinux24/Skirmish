using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    /// <summary>
    /// Material buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferMaterials : IBufferData
    {
        /// <summary>
        /// Algorithm
        /// </summary>
        public uint Algorithm;
        /// <summary>
        /// Padding
        /// </summary>
        public uint Block11;
        /// <summary>
        /// Padding
        /// </summary>
        public uint Block12;
        /// <summary>
        /// Padding
        /// </summary>
        public uint Block13;
        /// <summary>
        /// Material block 2
        /// </summary>
        public Vector4 Block2;
        /// <summary>
        /// Material block 3
        /// </summary>
        public Vector4 Block3;
        /// <summary>
        /// Material block 4
        /// </summary>
        public Vector4 Block4;
        /// <summary>
        /// Material block 5 - Diffuse color
        /// </summary>
        public Color4 Diffuse;
        /// <summary>
        /// Material block 6 - Emissive color
        /// </summary>
        public Color4 Emissive;
        /// <summary>
        /// Material block 7 - Ambient color
        /// </summary>
        public Color4 Ambient;
        /// <summary>
        /// Material block 8 - Specular color
        /// </summary>
        public Color4 Specular;

        /// <summary>
        /// Packs current instance into a Vector4 array
        /// </summary>
        /// <returns>Returns the packed material</returns>
        public Vector4[] Pack()
        {
            return new[]
            {
                new Vector4(Algorithm, Block11, Block12, Block13),
                Block2,
                Block3,
                Block4,
                Diffuse,
                Emissive,
                Ambient,
                Specular,
            };
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
