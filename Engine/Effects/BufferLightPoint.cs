﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    /// <summary>
    /// Point light buffer
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 80)]
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
        [FieldOffset(0)]
        public Color4 DiffuseColor;

        /// <summary>
        /// Specular color
        /// </summary>
        [FieldOffset(16)]
        public Color4 SpecularColor;

        /// <summary>
        /// Light position
        /// </summary>
        [FieldOffset(32)]
        public Vector3 Position;
        /// <summary>
        /// Intensity
        /// </summary>
        [FieldOffset(44)]
        public float Intensity;

        /// <summary>
        /// Light radius
        /// </summary>
        [FieldOffset(48)]
        public float Radius;
        /// <summary>
        /// The light casts shadow
        /// </summary>
        [FieldOffset(52)]
        public float CastShadow;
        /// <summary>
        /// Perspective values
        /// </summary>
        [FieldOffset(56)]
        public Vector2 PerspectiveValues;

        /// <summary>
        /// Shadow map index
        /// </summary>
        [FieldOffset(64)]
        public int MapIndex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferLightPoint(ISceneLightPoint light)
        {
            Position = light.Position;
            DiffuseColor = new Color4(light.DiffuseColor, 0f);
            SpecularColor = new Color4(light.SpecularColor, 0f);
            Intensity = light.Intensity;
            Radius = light.Radius;
            CastShadow = light.CastShadowsMarked ? 1 : 0;
            MapIndex = light.ShadowMapIndex;

            var perspectiveMatrix = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, Radius + 0.1f);
            PerspectiveValues = new Vector2(perspectiveMatrix[2, 2], perspectiveMatrix[3, 2]);
        }

        /// <inheritdoc/>
        public int GetStride()
        {
#if DEBUG
            int size = Marshal.SizeOf(typeof(BufferLightPoint));
            if (size % 16 != 0) throw new EngineException($"Buffer {nameof(BufferLightPoint)} strides must be divisible by 16 in order to be sent to shaders and effects as arrays");
            return size;
#else
            return Marshal.SizeOf(typeof(BufferLightHemispheric));
#endif
        }
    }
}
