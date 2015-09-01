using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Scene lights buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferLights : IBufferData
    {
        /// <summary>
        /// Directional light 1
        /// </summary>
        public BufferDirectionalLight DirectionalLight1;
        /// <summary>
        /// Directional light 2
        /// </summary>
        public BufferDirectionalLight DirectionalLight2;
        /// <summary>
        /// Directional light 3
        /// </summary>
        public BufferDirectionalLight DirectionalLight3;
        /// <summary>
        /// Point light 1
        /// </summary>
        public BufferPointLight PointLight1;
        /// <summary>
        /// Point light 2
        /// </summary>
        public BufferPointLight PointLight2;
        /// <summary>
        /// Point light 3
        /// </summary>
        public BufferPointLight PointLight3;
        /// <summary>
        /// Point light 4
        /// </summary>
        public BufferPointLight PointLight4;
        /// <summary>
        /// Spot light 1
        /// </summary>
        public BufferSpotLight SpotLight1;
        /// <summary>
        /// Spot light 2
        /// </summary>
        public BufferSpotLight SpotLight2;
        /// <summary>
        /// Spot light 3
        /// </summary>
        public BufferSpotLight SpotLight3;
        /// <summary>
        /// Spot light 4
        /// </summary>
        public BufferSpotLight SpotLight4;
        /// <summary>
        /// Eye position world
        /// </summary>
        public Vector3 EyePositionWorld;
        /// <summary>
        /// Fog start
        /// </summary>
        public float FogStart;
        /// <summary>
        /// Fog range
        /// </summary>
        public float FogRange;
        /// <summary>
        /// Fog color
        /// </summary>
        public Color4 FogColor;
        /// <summary>
        /// Enable shadows
        /// </summary>
        public float EnableShadows;
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferLights));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        public BufferLights(Vector3 eyePosition)
        {
            this.EyePositionWorld = eyePosition;

            this.DirectionalLight1 = new BufferDirectionalLight();
            this.DirectionalLight2 = new BufferDirectionalLight();
            this.DirectionalLight3 = new BufferDirectionalLight();
            this.PointLight1 = new BufferPointLight();
            this.PointLight2 = new BufferPointLight();
            this.PointLight3 = new BufferPointLight();
            this.PointLight4 = new BufferPointLight();
            this.SpotLight1 = new BufferSpotLight();
            this.SpotLight2 = new BufferSpotLight();
            this.SpotLight3 = new BufferSpotLight();
            this.SpotLight4 = new BufferSpotLight();
            this.FogColor = Color.White;
            this.FogStart = 0;
            this.FogRange = 0;
            this.EnableShadows = 0;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="lights">Lights configuration</param>
        public BufferLights(Vector3 eyePosition, SceneLights lights)
        {
            this.EyePositionWorld = eyePosition;

            SceneLights setLights = lights != null ? lights : SceneLights.Empty;

            this.DirectionalLight1 = new BufferDirectionalLight();
            this.DirectionalLight2 = new BufferDirectionalLight();
            this.DirectionalLight3 = new BufferDirectionalLight();
            this.PointLight1 = new BufferPointLight();
            this.PointLight2 = new BufferPointLight();
            this.PointLight3 = new BufferPointLight();
            this.PointLight4 = new BufferPointLight();
            this.SpotLight1 = new BufferSpotLight();
            this.SpotLight2 = new BufferSpotLight();
            this.SpotLight3 = new BufferSpotLight();
            this.SpotLight4 = new BufferSpotLight();

            if (setLights.DirectionalLights.Length > 0) this.DirectionalLight1 = new BufferDirectionalLight(setLights.DirectionalLights[0]);
            if (setLights.DirectionalLights.Length > 1) this.DirectionalLight2 = new BufferDirectionalLight(setLights.DirectionalLights[1]);
            if (setLights.DirectionalLights.Length > 2) this.DirectionalLight3 = new BufferDirectionalLight(setLights.DirectionalLights[2]);
            if (setLights.PointLights.Length > 0) this.PointLight1 = new BufferPointLight(setLights.PointLights[0]);
            if (setLights.PointLights.Length > 1) this.PointLight2 = new BufferPointLight(setLights.PointLights[1]);
            if (setLights.PointLights.Length > 2) this.PointLight3 = new BufferPointLight(setLights.PointLights[2]);
            if (setLights.PointLights.Length > 3) this.PointLight4 = new BufferPointLight(setLights.PointLights[3]);
            if (setLights.SpotLights.Length > 0) this.SpotLight1 = new BufferSpotLight(setLights.SpotLights[0]);
            if (setLights.SpotLights.Length > 1) this.SpotLight2 = new BufferSpotLight(setLights.SpotLights[1]);
            if (setLights.SpotLights.Length > 2) this.SpotLight3 = new BufferSpotLight(setLights.SpotLights[2]);
            if (setLights.SpotLights.Length > 3) this.SpotLight4 = new BufferSpotLight(setLights.SpotLights[3]);

            this.FogColor = setLights.FogColor;
            this.FogStart = setLights.FogStart;
            this.FogRange = setLights.FogRange;
            this.EnableShadows = setLights.EnableShadows ? 1 : 0;
        }
    }

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
        /// Padding
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
        /// Attenuation (constant, linear, exponential)
        /// </summary>
        public Vector3 Attenuation;
        /// <summary>
        /// Padding
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
            this.Attenuation = new Vector3(light.Constant, light.Linear, light.Exponential);
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
        /// Padding
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
            this.Enabled = light.Enabled ? 1f : 0f;
        }
    }
}
