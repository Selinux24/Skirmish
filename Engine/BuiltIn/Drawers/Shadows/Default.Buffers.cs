using Engine.Common;
using System.Numerics;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Drawers.Shadows
{
    /// <summary>
    /// Per-shadow casting light data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64 * MaxCount + 16)]
    struct PerCastingLight : IBufferData
    {
        /// <summary>
        /// Number of faces
        /// </summary>
        public const int MaxCount = 6;

        /// <summary>
        /// Builds the main Per-Light buffer
        /// </summary>
        /// <param name="context">Draw context</param>
        public static PerCastingLight Build(DrawContextShadows context)
        {
            var viewProjection = context.ShadowMap?.LightSource.FromLightVP ?? [];
            int length = Math.Min(MaxCount, viewProjection.Length);

            if (length > MaxCount)
            {
                throw new EngineException($"The matrix array must have a maximum length of {MaxCount}");
            }

            var m = new Matrix4x4[MaxCount];
            for (int i = 0; i < MaxCount; i++)
            {
                m[i] = Matrix4x4.Transpose(viewProjection.ElementAtOrDefault(i));
            }

            return new PerCastingLight
            {
                FromLightViewProjection = m,
                FaceCount = (uint)length,
            };
        }

        /// <summary>
        /// View projection matrix
        /// </summary>
        [FieldOffset(0), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxCount)]
        public Matrix4x4[] FromLightViewProjection;

        /// <summary>
        /// Face count
        /// </summary>
        [FieldOffset(64 * MaxCount)]
        public uint FaceCount;

        /// <inheritdoc/>
        public readonly uint GetStride()
        {
            return (uint)Marshal.SizeOf<PerCastingLight>();
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
                Local = Matrix4x4.Transpose(state.Local),
            };
        }

        /// <summary>
        /// Local transform
        /// </summary>
        [FieldOffset(0)]
        public Matrix4x4 Local;

        /// <inheritdoc/>
        public readonly uint GetStride()
        {
            return (uint)Marshal.SizeOf<PerMeshSingle>();
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
                Local = Matrix4x4.Transpose(state.Local),
                AnimationOffset = state.AnimationOffset1,
                AnimationOffset2 = state.AnimationOffset2,
                AnimationInterpolation = state.AnimationInterpolationAmount,
            };
        }

        /// <summary>
        /// Local transform
        /// </summary>
        [FieldOffset(0)]
        public Matrix4x4 Local;

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
        public readonly uint GetStride()
        {
            return (uint)Marshal.SizeOf<PerMeshSkinned>();
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
        public readonly uint GetStride()
        {
            return (uint)Marshal.SizeOf<PerMaterialTexture>();
        }
    }
}
