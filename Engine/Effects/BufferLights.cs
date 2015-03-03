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
        /// Point light
        /// </summary>
        public BufferPointLight PointLight;
        /// <summary>
        /// Spot light
        /// </summary>
        public BufferSpotLight SpotLight;
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
            this.PointLight = new BufferPointLight();
            this.SpotLight = new BufferSpotLight();
            this.FogColor = Color.White;
            this.FogStart = 0;
            this.FogRange = 0;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="lights">Lights configuration</param>
        public BufferLights(Vector3 eyePosition, SceneLight lights)
        {
            this.EyePositionWorld = eyePosition;

            this.DirectionalLight1 = new BufferDirectionalLight(lights.DirectionalLight1);
            this.DirectionalLight2 = new BufferDirectionalLight(lights.DirectionalLight2);
            this.DirectionalLight3 = new BufferDirectionalLight(lights.DirectionalLight3);
            this.PointLight = new BufferPointLight(lights.PointLight);
            this.SpotLight = new BufferSpotLight(lights.SpotLight);
            this.FogColor = lights.FogColor;
            this.FogStart = lights.FogStart;
            this.FogRange = lights.FogRange;
        }
    }

    /// <summary>
    /// Directional light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferDirectionalLight : IBufferData
    {
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
        /// Light direction vector
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// Padding
        /// </summary>
        public float Padding;
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
            this.Ambient = light.Ambient;
            this.Diffuse = light.Diffuse;
            this.Specular = light.Specular;
            this.Direction = light.Direction;
            this.Padding = light.Enabled ? 1f : 0f;
        }
    }

    /// <summary>
    /// Point light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferPointLight : IBufferData
    {
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
        /// Light position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Light range
        /// </summary>
        public float Range;
        /// <summary>
        /// Attenuation
        /// </summary>
        public Vector3 Attenuation;
        /// <summary>
        /// Padding
        /// </summary>
        public float Padding;
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
            this.Ambient = light.Ambient;
            this.Diffuse = light.Diffuse;
            this.Specular = light.Specular;
            this.Position = light.Position;
            this.Range = light.Range;
            this.Attenuation = light.Attenuation;
            this.Padding = light.Enabled ? 1f : 0f;
        }
    }

    /// <summary>
    /// Spot light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferSpotLight : IBufferData
    {
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
        /// Light position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Light range
        /// </summary>
        public float Range;
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
        public float Padding;
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
            this.Ambient = light.Ambient;
            this.Diffuse = light.Diffuse;
            this.Specular = light.Specular;
            this.Position = light.Position;
            this.Range = light.Range;
            this.Direction = light.Direction;
            this.Spot = light.Spot;
            this.Attenuation = light.Attenuation;
            this.Padding = light.Enabled ? 1f : 0f;
        }
    }
}
