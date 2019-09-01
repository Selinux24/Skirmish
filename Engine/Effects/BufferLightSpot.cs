using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    /// <summary>
    /// Spot light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferLightSpot : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 16;

        /// <summary>
        /// Default buffer collection
        /// </summary>
        public static BufferLightSpot[] Default
        {
            get
            {
                return new BufferLightSpot[MAX];
            }
        }
        /// <summary>
        /// Builds a light buffer collection
        /// </summary>
        /// <param name="lights">Light list</param>
        /// <param name="lightCount">Returns the assigned light count</param>
        /// <returns>Returns a light buffer collection</returns>
        public static BufferLightSpot[] Build(IEnumerable<SceneLightSpot> lights, out int lightCount)
        {
            if (!lights.Any())
            {
                lightCount = 0;

                return Default;
            }

            var bSpotLights = Default;

            var spot = lights.ToArray();
            for (int i = 0; i < Math.Min(spot.Length, MAX); i++)
            {
                bSpotLights[i] = new BufferLightSpot(spot[i]);
            }

            lightCount = Math.Min(spot.Length, MAX);

            return bSpotLights;
        }

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
        public BufferLightSpot(SceneLightSpot light)
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
            int size = Marshal.SizeOf(typeof(BufferLightSpot));
            if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
            return size;
#else
            return Marshal.SizeOf(typeof(BufferLightSpot));
#endif
        }
    }
}
