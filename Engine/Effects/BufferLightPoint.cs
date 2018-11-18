using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    /// <summary>
    /// Point light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferLightPoint : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 16;

        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 SpecularColor;
        /// <summary>
        /// Light position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity;
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius;
        /// <summary>
        /// The light casts shadow
        /// </summary>
        public float CastShadow;
        /// <summary>
        /// Perspective values
        /// </summary>
        public Vector2 PerspectiveValues;
        /// <summary>
        /// Shadow map index
        /// </summary>
        public int MapIndex;
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
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferLightPoint(SceneLightPoint light)
        {
            this.Position = light.Position;
            this.DiffuseColor = light.DiffuseColor;
            this.SpecularColor = light.SpecularColor;
            this.Intensity = light.Intensity;
            this.Radius = light.Radius;
            this.CastShadow = light.CastShadow ? 1 : 0;
            this.MapIndex = light.ShadowMapIndex;

            var perspectiveMatrix = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, this.Radius + 0.1f);
            this.PerspectiveValues = new Vector2(perspectiveMatrix[2, 2], perspectiveMatrix[3, 2]);

            this.Pad1 = 1000;
            this.Pad2 = 2000;
            this.Pad3 = 3000;
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
#if DEBUG
            int size = Marshal.SizeOf(typeof(BufferLightPoint));
            if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
            return size;
#else
            return Marshal.SizeOf(typeof(BufferLightPoint));
#endif
        }
    }
}
