using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    using Engine.Common;
    using System;

    /// <summary>
    /// Directional light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferDirectionalLight : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 3;

        /// <summary>
        /// Light color
        /// </summary>
        public Color3 LightColor;
        /// <summary>
        /// Ambient intensity
        /// </summary>
        public float AmbientIntensity;
        /// <summary>
        /// Diffuse intensity
        /// </summary>
        public float DiffuseIntensity;
        /// <summary>
        /// Light direction vector
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// Cast shadow
        /// </summary>
        public float CastShadow;
        /// <summary>
        /// Is Enabled
        /// </summary>
        public float Enabled;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad1;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad2;

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
#if DEBUG
                int size = Marshal.SizeOf(typeof(BufferDirectionalLight));
                if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
                return size;
#else
                return Marshal.SizeOf(typeof(BufferDirectionalLight));
#endif
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferDirectionalLight(SceneLightDirectional light)
        {
            this.LightColor = light.LightColor.ToVector3();
            this.AmbientIntensity = light.AmbientIntensity;
            this.DiffuseIntensity = light.DiffuseIntensity;
            this.Direction = light.Direction;
            this.CastShadow = light.CastShadow ? 1f : 0f;
            this.Enabled = light.Enabled ? 1f : 0f;

            this.Pad1 = 1000;
            this.Pad2 = 2000;
        }
    }

    /// <summary>
    /// Point light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferPointLight : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 16;

        /// <summary>
        /// Light color
        /// </summary>
        public Color3 LightColor;
        /// <summary>
        /// Ambient intensity
        /// </summary>
        public float AmbientIntensity;
        /// <summary>
        /// Diffuse intensity
        /// </summary>
        public float DiffuseIntensity;
        /// <summary>
        /// Light position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius;
        /// <summary>
        /// Cast shadow
        /// </summary>
        public float CastShadow;
        /// <summary>
        /// Is Enabled
        /// </summary>
        public float Enabled;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad1;

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
#if DEBUG
                int size = Marshal.SizeOf(typeof(BufferPointLight));
                if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
                return size;
#else
                return Marshal.SizeOf(typeof(BufferPointLight));
#endif
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferPointLight(SceneLightPoint light)
        {
            this.LightColor = light.LightColor.ToVector3();
            this.AmbientIntensity = light.AmbientIntensity;
            this.DiffuseIntensity = light.DiffuseIntensity;
            this.Position = light.Position;
            this.Radius = light.Radius;
            this.CastShadow = light.CastShadow ? 1f : 0f;
            this.Enabled = light.Enabled ? 1f : 0f;

            this.Pad1 = 1000;
        }
    }

    /// <summary>
    /// Spot light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferSpotLight : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 16;

        /// <summary>
        /// Light color
        /// </summary>
        public Color3 LightColor;
        /// <summary>
        /// Ambient intensity
        /// </summary>
        public float AmbientIntensity;
        /// <summary>
        /// Diffuse intensity
        /// </summary>
        public float DiffuseIntensity;
        /// <summary>
        /// Light position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Light direction
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// Spot radius
        /// </summary>
        public float Angle;
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius;
        /// <summary>
        /// Cast shadow
        /// </summary>
        public float CastShadow;
        /// <summary>
        /// Is Enabled
        /// </summary>
        public float Enabled;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad1;

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
#if DEBUG
                int size = Marshal.SizeOf(typeof(BufferSpotLight));
                if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
                return size;
#else
                return Marshal.SizeOf(typeof(BufferSpotLight));
#endif
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferSpotLight(SceneLightSpot light)
        {
            this.LightColor = light.LightColor.ToVector3();
            this.AmbientIntensity = light.AmbientIntensity;
            this.DiffuseIntensity = light.DiffuseIntensity;
            this.Position = light.Position;
            this.Direction = light.Direction;
            this.Angle = light.Angle;
            this.Radius = light.Radius;
            this.CastShadow = light.CastShadow ? 1f : 0f;
            this.Enabled = light.Enabled ? 1f : 0f;

            this.Pad1 = 1000;
        }
    }
}
