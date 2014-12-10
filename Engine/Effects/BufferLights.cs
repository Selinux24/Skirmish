using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    [StructLayout(LayoutKind.Sequential)]
    public struct BufferLights : IBuffer
    {
        public BufferDirectionalLight DirectionalLight1;
        public BufferDirectionalLight DirectionalLight2;
        public BufferDirectionalLight DirectionalLight3;
        public BufferPointLight PointLight;
        public BufferSpotLight SpotLight;
        public Vector3 EyePositionWorld;
        public float FogStart;
        public float FogRange;
        public Color4 FogColor;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferLights));
            }
        }

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

        public int Stride
        {
            get
            {
                return SizeInBytes;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BufferDirectionalLight : IBuffer
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Direction;
        public float Padding;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferDirectionalLight));
            }
        }

        public BufferDirectionalLight(SceneLightDirectional light)
        {
            this.Ambient = light.Ambient;
            this.Diffuse = light.Diffuse;
            this.Specular = light.Specular;
            this.Direction = light.Direction;
            this.Padding = light.Enabled ? 1f : 0f;
        }

        public int Stride
        {
            get
            {
                return SizeInBytes;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BufferPointLight : IBuffer
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Position;
        public float Range;
        public Vector3 Attributes;
        public float Padding;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferPointLight));
            }
        }

        public BufferPointLight(SceneLightPoint light)
        {
            this.Ambient = light.Ambient;
            this.Diffuse = light.Diffuse;
            this.Specular = light.Specular;
            this.Position = light.Position;
            this.Range = light.Range;
            this.Attributes = light.Attributes;
            this.Padding = light.Enabled ? 1f : 0f;
        }

        public int Stride
        {
            get
            {
                return SizeInBytes;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BufferSpotLight : IBuffer
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Position;
        public float Range;
        public Vector3 Direction;
        public float Spot;
        public Vector3 Attributes;
        public float Padding;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferSpotLight));
            }
        }

        public BufferSpotLight(SceneLightSpot light)
        {
            this.Ambient = light.Ambient;
            this.Diffuse = light.Diffuse;
            this.Specular = light.Specular;
            this.Position = light.Position;
            this.Range = light.Range;
            this.Direction = light.Direction;
            this.Spot = light.Spot;
            this.Attributes = light.Attributes;
            this.Padding = light.Enabled ? 1f : 0f;
        }

        public int Stride
        {
            get
            {
                return SizeInBytes;
            }
        }
    }
}
