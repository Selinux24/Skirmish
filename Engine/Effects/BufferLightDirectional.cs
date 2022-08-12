using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    /// <summary>
    /// Directional light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferLightDirectional : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 3;

        /// <summary>
        /// Default buffer collection
        /// </summary>
        public static BufferLightDirectional[] Default
        {
            get
            {
                return new BufferLightDirectional[MAX];
            }
        }
        /// <summary>
        /// Builds a light buffer collection
        /// </summary>
        /// <param name="lights">Light list</param>
        /// <param name="lightCount">Returns the assigned light count</param>
        /// <returns>Returns a light buffer collection</returns>
        public static BufferLightDirectional[] Build(IEnumerable<ISceneLightDirectional> lights, out int lightCount)
        {
            if (!lights.Any())
            {
                lightCount = 0;

                return Default;
            }

            var bDirLights = Default;

            var dir = lights.ToArray();
            for (int i = 0; i < Math.Min(dir.Length, MAX); i++)
            {
                bDirLights[i] = new BufferLightDirectional(dir[i]);
            }

            lightCount = Math.Min(dir.Length, MAX);

            return bDirLights;
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
        /// Light direction vector
        /// </summary>
        public Vector3 DirToLight;
        /// <summary>
        /// The light casts shadow
        /// </summary>
        public float CastShadow;
        /// <summary>
        /// X cascade offsets
        /// </summary>
        public Vector4 ToCascadeOffsetX;
        /// <summary>
        /// Y cascade offsets
        /// </summary>
        public Vector4 ToCascadeOffsetY;
        /// <summary>
        /// Cascade scales
        /// </summary>
        public Vector4 ToCascadeScale;
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        public Matrix ToShadowSpace;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferLightDirectional(ISceneLightDirectional light)
        {
            DiffuseColor = new Color4(light.DiffuseColor * light.Brightness, 0f);
            SpecularColor = new Color4(light.SpecularColor * light.Brightness, 0f);
            DirToLight = -light.Direction;
            CastShadow = light.CastShadowsMarked ? 1 : 0;
            ToCascadeOffsetX = light.ToCascadeOffsetX;
            ToCascadeOffsetY = light.ToCascadeOffsetY;
            ToCascadeScale = light.ToCascadeScale;
            ToShadowSpace = Matrix.Transpose(light.ToShadowSpace);
        }

        /// <inheritdoc/>
        public int GetStride()
        {
#if DEBUG
            int size = Marshal.SizeOf(typeof(BufferLightDirectional));
            if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
            return size;
#else
            return Marshal.SizeOf(typeof(BufferLightDirectional));
#endif
        }
    }
}
