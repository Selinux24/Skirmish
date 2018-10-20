using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    /// <summary>
    /// Hemispheric light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferHemisphericLight : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 1;

        /// <summary>
        /// Default hemispheric light
        /// </summary>
        public static BufferHemisphericLight Default
        {
            get
            {
                return new BufferHemisphericLight()
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
        public BufferHemisphericLight(SceneLightHemispheric light)
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
            int size = Marshal.SizeOf(typeof(BufferHemisphericLight));
            if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
            return size;
#else
            return Marshal.SizeOf(typeof(BufferHemisphericLight));
#endif
        }
    }

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

        public Vector4 ToCascadeOffsetX;

        public Vector4 ToCascadeOffsetY;

        public Vector4 ToCascadeScale;
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        public Matrix ToShadowSpace;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferDirectionalLight(SceneLightDirectional light)
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
            int size = Marshal.SizeOf(typeof(BufferDirectionalLight));
            if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
            return size;
#else
            return Marshal.SizeOf(typeof(BufferDirectionalLight));
#endif
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
        public BufferPointLight(SceneLightPoint light)
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
            int size = Marshal.SizeOf(typeof(BufferPointLight));
            if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
            return size;
#else
            return Marshal.SizeOf(typeof(BufferPointLight));
#endif
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
        /// Spot radius
        /// </summary>
        public float Angle;
        /// <summary>
        /// Light direction vector
        /// </summary>
        public Vector3 Direction;
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
        /// Shadow map index
        /// </summary>
        public int MapIndex;
        /// <summary>
        /// Sub-shadow map count
        /// </summary>
        public uint MapCount;
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        public Matrix FromLightVP;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferSpotLight(SceneLightSpot light)
        {
            this.Position = light.Position;
            this.Direction = light.Direction;
            this.DiffuseColor = light.DiffuseColor;
            this.SpecularColor = light.SpecularColor;
            this.Intensity = light.Intensity;
            this.Intensity = light.Intensity;
            this.Angle = light.AngleRadians;
            this.Radius = light.Radius;
            this.CastShadow = light.CastShadow ? 1 : 0;
            this.MapIndex = light.ShadowMapIndex;
            this.MapCount = light.ShadowMapCount;

            this.FromLightVP = Matrix.Identity;
            if (light.FromLightVP?.Length > 0)
            {
                this.FromLightVP = Matrix.Transpose(light.FromLightVP[0]);
            }
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
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
}
