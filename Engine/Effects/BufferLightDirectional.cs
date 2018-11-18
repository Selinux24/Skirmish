using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    /// <summary>
    /// Directional light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferLightDirectional : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 3;

        /// <summary>
        /// Light direction vector
        /// </summary>
        public Vector3 DirToLight;
        /// <summary>
        /// The light casts shadow
        /// </summary>
        public float CastShadow;
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 LightColor;
        /// <summary>
        /// X cascade offsets
        /// </summary>
        public Vector4 ToCascadeOffsetX;
        /// <summary>
        /// Y cascade offsets
        /// </summary>
        public Vector4 ToCascadeOffsetY;
        /// <summary>
        /// Cascade scales
        /// </summary>
        public Vector4 ToCascadeScale;
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        public Matrix ToShadowSpace;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferLightDirectional(SceneLightDirectional light)
        {
            this.DirToLight = -light.Direction;
            this.CastShadow = light.CastShadow ? 1 : 0;
            this.LightColor = light.DiffuseColor * light.Brightness;
            this.ToCascadeOffsetX = light.ToCascadeOffsetX;
            this.ToCascadeOffsetY = light.ToCascadeOffsetY;
            this.ToCascadeScale = light.ToCascadeScale;
            this.ToShadowSpace = Matrix.Transpose(light.ToShadowSpace);
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
#if DEBUG
            int size = Marshal.SizeOf(typeof(BufferLightDirectional));
            if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
            return size;
#else
            return Marshal.SizeOf(typeof(BufferLightDirectional));
#endif
        }
    }
}
