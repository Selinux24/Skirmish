using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    /// <summary>
    /// Hemispheric light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferLightHemispheric : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 1;

        /// <summary>
        /// Default hemispheric light
        /// </summary>
        public static BufferLightHemispheric Default
        {
            get
            {
                return new BufferLightHemispheric()
                {
                    AmbientDown = Color.White,
                    AmbientUp = Color.White,
                };
            }
        }

        /// <summary>
        /// Ambient Up
        /// </summary>
        public Color4 AmbientDown;
        /// <summary>
        /// Ambient Down
        /// </summary>
        public Color4 AmbientUp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferLightHemispheric(SceneLightHemispheric light)
        {
            this.AmbientDown = light.AmbientDown;
            this.AmbientUp = light.AmbientUp;
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
#if DEBUG
            int size = Marshal.SizeOf(typeof(BufferLightHemispheric));
            if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
            return size;
#else
            return Marshal.SizeOf(typeof(BufferLightHemispheric));
#endif
        }
    }
}
