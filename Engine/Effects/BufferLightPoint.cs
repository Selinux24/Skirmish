using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Default buffer collection
        /// </summary>
        public static BufferLightPoint[] Default
        {
            get
            {
                return new BufferLightPoint[MAX];
            }
        }
        /// <summary>
        /// Builds a light buffer collection
        /// </summary>
        /// <param name="lights">Light list</param>
        /// <param name="lightCount">Returns the assigned light count</param>
        /// <returns>Returns a light buffer collection</returns>
        public static BufferLightPoint[] Build(IEnumerable<ISceneLightPoint> lights, out int lightCount)
        {
            if (!lights.Any())
            {
                lightCount = 0;

                return Default;
            }

            var bPointLights = Default;

            var point = lights.ToArray();
            for (int i = 0; i < Math.Min(point.Length, MAX); i++)
            {
                bPointLights[i] = new BufferLightPoint(point[i]);
            }

            lightCount = Math.Min(point.Length, MAX);

            return bPointLights;
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
        public BufferLightPoint(ISceneLightPoint light)
        {
            Position = light.Position;
            DiffuseColor = light.DiffuseColor;
            SpecularColor = light.SpecularColor;
            Intensity = light.Intensity;
            Radius = light.Radius;
            CastShadow = light.CastShadowsMarked ? 1 : 0;
            MapIndex = light.ShadowMapIndex;

            var perspectiveMatrix = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, Radius + 0.1f);
            PerspectiveValues = new Vector2(perspectiveMatrix[2, 2], perspectiveMatrix[3, 2]);

            Pad1 = 1000;
            Pad2 = 2000;
            Pad3 = 3000;
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
