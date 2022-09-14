using SharpDX;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Shadows
{
    using Engine.Common;

    /// <summary>
    /// Per-shadow casting light data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64 * MaxCount)]
    struct PerCastingLightCascade : IBufferData
    {
        /// <summary>
        /// Number of faces
        /// </summary>
        public const int MaxCount = 3;

        /// <summary>
        /// Builds the main Per-Light buffer
        /// </summary>
        /// <param name="context">Draw context</param>
        public static PerCastingLightCascade Build(DrawContextShadows context)
        {
            var viewProjection = context?.ShadowMap?.FromLightViewProjectionArray;

            if (viewProjection?.Length > MaxCount)
            {
                throw new EngineException($"The matrix array must have a maximum length of {MaxCount}");
            }

            var m = new Matrix[MaxCount];
            for (int i = 0; i < MaxCount; i++)
            {
                m[i] = Matrix.Transpose(viewProjection.ElementAtOrDefault(i));
            }

            return new PerCastingLightCascade
            {
                FromLightViewProjection = m,
            };
        }

        /// <summary>
        /// View projection matrix
        /// </summary>
        [FieldOffset(0), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCount)]
        public Matrix[] FromLightViewProjection;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerCastingLightCascade));
        }
    }

    /// <summary>
    /// Per-shadow casting light data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64 * MaxCount)]
    struct PerCastingLightSpot : IBufferData
    {
        /// <summary>
        /// Number of faces
        /// </summary>
        public const int MaxCount = 1;

        /// <summary>
        /// Builds the main Per-Light buffer
        /// </summary>
        /// <param name="context">Draw context</param>
        public static PerCastingLightSpot Build(DrawContextShadows context)
        {
            var viewProjection = context?.ShadowMap?.FromLightViewProjectionArray;

            if (viewProjection?.Length > MaxCount)
            {
                throw new EngineException($"The matrix array must have a maximum length of {MaxCount}");
            }

            var m = new Matrix[MaxCount];
            for (int i = 0; i < MaxCount; i++)
            {
                m[i] = Matrix.Transpose(viewProjection.ElementAtOrDefault(i));
            }

            return new PerCastingLightSpot
            {
                FromLightViewProjection = m,
            };
        }

        /// <summary>
        /// View projection matrix
        /// </summary>
        [FieldOffset(0), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCount)]
        public Matrix[] FromLightViewProjection;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerCastingLightSpot));
        }
    }

    /// <summary>
    /// Per-shadow casting light data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64 * MaxCount)]
    struct PerCastingLightPoint : IBufferData
    {
        /// <summary>
        /// Number of faces
        /// </summary>
        public const int MaxCount = 6;

        /// <summary>
        /// Builds the main Per-Light buffer
        /// </summary>
        /// <param name="context">Draw context</param>
        public static PerCastingLightPoint Build(DrawContextShadows context)
        {
            var viewProjection = context?.ShadowMap?.FromLightViewProjectionArray;

            if (viewProjection?.Length > MaxCount)
            {
                throw new EngineException($"The matrix array must have a maximum length of {MaxCount}");
            }

            var m = new Matrix[MaxCount];
            for (int i = 0; i < MaxCount; i++)
            {
                m[i] = Matrix.Transpose(viewProjection.ElementAtOrDefault(i));
            }

            return new PerCastingLightPoint
            {
                FromLightViewProjection = m,
            };
        }

        /// <summary>
        /// View projection matrix
        /// </summary>
        [FieldOffset(0), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCount)]
        public Matrix[] FromLightViewProjection;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerCastingLightPoint));
        }
    }

    /// <summary>
    /// Per mesh single data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    struct PerMeshSingle : IBufferData
    {
        public static PerMeshSingle Build(BuiltInDrawerMeshState state)
        {
            return new PerMeshSingle
            {
                Local = Matrix.Transpose(state.Local),
            };
        }

        /// <summary>
        /// Local transform
        /// </summary>
        [FieldOffset(0)]
        public Matrix Local;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerMeshSingle));
        }
    }

    /// <summary>
    /// Per mesh skinned data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    struct PerMeshSkinned : IBufferData
    {
        public static PerMeshSkinned Build(BuiltInDrawerMeshState state)
        {
            return new PerMeshSkinned
            {
                Local = Matrix.Transpose(state.Local),
                AnimationOffset = state.AnimationOffset1,
                AnimationOffset2 = state.AnimationOffset2,
                AnimationInterpolation = state.AnimationInterpolationAmount,
            };
        }

        /// <summary>
        /// Local transform
        /// </summary>
        [FieldOffset(0)]
        public Matrix Local;

        /// <summary>
        /// Animation offset 1
        /// </summary>
        [FieldOffset(64)]
        public uint AnimationOffset;
        /// <summary>
        /// Animation offset 2
        /// </summary>
        [FieldOffset(68)]
        public uint AnimationOffset2;
        /// <summary>
        /// Animation interpolation value
        /// </summary>
        [FieldOffset(72)]
        public float AnimationInterpolation;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerMeshSkinned));
        }
    }

    /// <summary>
    /// Per material texture data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    struct PerMaterialTexture : IBufferData
    {
        public static PerMaterialTexture Build(BuiltInDrawerMaterialState state)
        {
            return new PerMaterialTexture
            {
                TextureIndex = state.TextureIndex,
            };
        }

        /// <summary>
        /// Texture index
        /// </summary>
        [FieldOffset(0)]
        public uint TextureIndex;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerMaterialTexture));
        }
    }
}
