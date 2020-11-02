using SharpDX;
using System.Runtime.InteropServices;

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
        /// Algorihtm
        /// </summary>
        public uint Algorihtm;
        /// <summary>
        /// Padding
        /// </summary>
        public uint Pad1;
        /// <summary>
        /// Padding
        /// </summary>
        public uint Pad2;
        /// <summary>
        /// Padding
        /// </summary>
        public uint Pad3;
        /// <summary>
        /// Ambient color
        /// </summary>
        public Color4 Ambient;
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 Diffuse;
        /// <summary>
        /// Emissive color
        /// </summary>
        public Color4 Emissive;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 Specular;
        /// <summary>
        /// Shininess
        /// </summary>
        public float Shininess;
        /// <summary>
        /// Cook-Torrance Roughness mode
        /// </summary>
        public uint RoughnessMode;
        /// <summary>
        /// Cook-Torrance Roughness value
        /// </summary>
        public float RoughnessValue;
        /// <summary>
        /// Cook-Torrance Reflection at norm incidence
        /// </summary>
        public float ReflectionAtNormIncidence;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="material">Material</param>
        public BufferMaterials(Material material)
        {
            Algorihtm = (uint)material.Algorithm;
            Pad1 = 1000;
            Pad2 = 2000;
            Pad3 = 3000;
            Emissive = material.EmissiveColor;
            Ambient = material.AmbientColor;
            Diffuse = material.DiffuseColor;
            Specular = material.SpecularColor;
            Shininess = material.Shininess;
            RoughnessMode = (uint)material.RoughnessMode;
            RoughnessValue = material.RoughnessValue;
            ReflectionAtNormIncidence = material.ReflectionAtNormIncidence;
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
