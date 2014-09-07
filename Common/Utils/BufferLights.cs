using System.Runtime.InteropServices;
using SharpDX;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferLights
    {
        public DirectionalLight DirectionalLight1;
        public DirectionalLight DirectionalLight2;
        public DirectionalLight DirectionalLight3;
        public PointLight PointLight;
        public SpotLight SpotLight;
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
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionalLight
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
                return Marshal.SizeOf(typeof(DirectionalLight));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PointLight
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
                return Marshal.SizeOf(typeof(PointLight));
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SpotLight
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
                return Marshal.SizeOf(typeof(SpotLight));
            }
        }
    }
}
