using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Directional light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferDirectionalLight : IBufferData
    {
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
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferDirectionalLight));
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
        }
    }

    /// <summary>
    /// Point light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferPointLight : IBufferData
    {
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
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferPointLight));
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
        }
    }

    /// <summary>
    /// Spot light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferSpotLight : IBufferData
    {
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
        public float Spot;
        /// <summary>
        /// Attenuation
        /// </summary>
        public Vector3 Attenuation;
        /// <summary>
        /// Cast shadow
        /// </summary>
        public float CastShadow;
        /// <summary>
        /// Is Enabled
        /// </summary>
        public float Enabled;
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferSpotLight));
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
            this.Spot = light.Spot;
            this.Attenuation = light.Attenuation;
            this.CastShadow = light.CastShadow ? 1f : 0f;
            this.Enabled = light.Enabled ? 1f : 0f;
        }
    }
}
